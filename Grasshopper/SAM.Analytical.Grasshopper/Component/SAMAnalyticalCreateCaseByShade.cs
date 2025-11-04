using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    /// <summary>
    /// Create a case-specific AnalyticalModel by adding simple external shades to selected
    /// panels or apertures (makes a copy of the base model; the original stays unchanged).
    /// </summary>
    /// <remarks>
    /// <para>
    /// This component prepares a case-specific <see cref="AnalyticalModel"/> for parametric studies by
    /// generating overhangs and/or side fins around selected <see cref="Panel"/>s or <see cref="Aperture"/>s.
    /// A copy of the base model is made so your original model is not changed. Depths and offsets are in metres.
    /// </para>
    /// </remarks>
    public class SAMAnalyticalCreateCaseByShade : GH_SAMVariableOutputParameterComponent
    {
        // ────────────────────────────────────────────────────────────────────────────────
        // Metadata
        // ────────────────────────────────────────────────────────────────────────────────

        /// <summary>Gets the unique ID for this component. Do not change this ID after release.</summary>
        public override Guid ComponentGuid => new Guid("a8d0db57-5d59-493c-9dae-02ccdb7a169c");

        /// <summary>The latest version of this component.</summary>
        public override string LatestComponentVersion => "1.0.3";

        /// <summary>Component icon shown on the Grasshopper canvas.</summary>
        protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        /// <summary>Display priority in the Grasshopper ribbon.</summary>
        public override GH_Exposure Exposure => GH_Exposure.primary;

        // Long, multi-line description for tooltips and docs (MEP-friendly SAM style)
        private const string DescriptionLong =
@"Create a case-specific AnalyticalModel by adding overhangs and side fins.

SUMMARY
  • Makes a copy of _baseAModel_; the original stays unchanged.
  • Adds an overhang above, and/or left/right vertical fins, to the selected
    Panels or Apertures.
  • Depths and offsets are in metres. Set a depth to 0 to omit that element.

INPUTS
  _baseAModel  (AnalyticalModel, required)
    Base SAM AnalyticalModel used as a template for this case.
    Type: SAM.Analytical.AnalyticalModel

  _analyticalObjects  (Panel[] or Aperture[], required)
    Panels or Apertures to receive shades. Supply Panels to size shades on the
    whole opening; supply Apertures for aperture-specific sizing.

  _glassPartOnly  (Boolean, optional, default = false)
    Applies when input objects are Apertures. If true, size the shade using the
    glass pane only; if false, use the full aperture size.

  _overhangDepth_  (Number, optional, default = 0 m)
    Horizontal projection of the overhang above the opening [m].

  _overhangVerticalOffset_  (Number, optional, default = 0 m)
    Vertical shift of the overhang relative to the opening head [m].

  _overhangFrontOffset_  (Number, optional, default = 0 m)
    Forward/back offset of the overhang from the façade plane [m].

  _leftFinDepth_  (Number, optional, default = 0 m)
    Depth of the left vertical fin [m].

  _leftFinOffset_  (Number, optional, default = 0 m)
    Horizontal offset of the left fin from the opening jamb [m].

  _leftFinFrontOffset_  (Number, optional, default = 0 m)
    Forward/back offset of the left fin from the façade plane [m].

  _rightFinDepth_  (Number, optional, default = 0 m)
    Depth of the right vertical fin [m].

  _rightFinOffset_  (Number, optional, default = 0 m)
    Horizontal offset of the right fin from the opening jamb [m].

  _rightFinFrontOffset_  (Number, optional, default = 0 m)
    Forward/back offset of the right fin from the façade plane [m].

OUTPUTS
  CaseAModel  (AnalyticalModel)
    Copy of the base model with generated shade Panels added.

NOTES
  • Use 0 m for any depth to skip that shade element.
  • When you pass Panels, shades are created relative to panel geometry.
    When you pass Apertures, shades are created relative to the aperture; the
    _glassPartOnly toggle controls whether the glass or full aperture is used.

EXAMPLE
  1) Read a baseline AnalyticalModel → _baseAModel
  2) Select Panels or Apertures to shade → _analyticalObjects
  3) Set depths/offsets (e.g., _overhangDepth_ = 0.6 m; _leftFinDepth_ = 0.4 m)
  4) (Optional) Toggle _glassPartOnly_ for aperture-based sizing
  5) Get the case model with shades → CaseAModel
";

        /// <summary>Initializes a new instance of the component.</summary>
        public SAMAnalyticalCreateCaseByShade()
          : base(
                "SAMAnalytical.CreateCaseByShade",
                "CreateCaseByShade",
                DescriptionLong,
                "SAM", "Analytical")
        { }

        // ────────────────────────────────────────────────────────────────────────────────
        // Parameters
        // ────────────────────────────────────────────────────────────────────────────────

        /// <summary>Registers all the input parameters for this component.</summary>
        protected override GH_SAMParam[] Inputs
        {
            get
            {
                List<GH_SAMParam> result = [];

                // Base Analytical Model (required)
                var analyticalModelParam = new GooAnalyticalModelParam
                {
                    Name = "_baseAModel",
                    NickName = "_baseAModel",
                    Description = "Base SAM AnalyticalModel used as a template for this case.\nType: SAM.Analytical.AnalyticalModel\nRequired: Yes",
                    Access = GH_ParamAccess.item
                };
                result.Add(new GH_SAMParam(analyticalModelParam, ParamVisibility.Binding));

                // Analytical objects (required)
                var analyticalObject = new GooAnalyticalObjectParam
                {
                    Name = "_analyticalObjects",
                    NickName = "_analyticalObjects",
                    Description = "Panels or Apertures to receive shades.\nSupply Panels for whole-opening sizing; supply Apertures for aperture-specific sizing.",
                    Access = GH_ParamAccess.list
                };
                result.Add(new GH_SAMParam(analyticalObject, ParamVisibility.Binding));

                // Glass part only (optional, default false)
                var paramBoolean = new global::Grasshopper.Kernel.Parameters.Param_Boolean
                {
                    Name = "_glassPartOnly",
                    NickName = "_glassPartOnly",
                    Description = "When inputs are Apertures: if true, size using the glass pane only;\nif false, use the full aperture size.",
                    Access = GH_ParamAccess.item
                };
                paramBoolean.SetPersistentData(false);
                result.Add(new GH_SAMParam(paramBoolean, ParamVisibility.Binding));

                // Numbers (depths/offsets) — defaults 0 m
                var paramNumber = new global::Grasshopper.Kernel.Parameters.Param_Number
                {
                    Name = "_overhangDepth_",
                    NickName = "_overhangDepth_",
                    Description = "Horizontal projection of the overhang above the opening [m].\nDefault: 0",
                    Access = GH_ParamAccess.item
                };
                paramNumber.SetPersistentData(0);
                result.Add(new GH_SAMParam(paramNumber, ParamVisibility.Binding));

                paramNumber = new global::Grasshopper.Kernel.Parameters.Param_Number
                {
                    Name = "_overhangVerticalOffset_",
                    NickName = "_overhangVerticalOffset_",
                    Description = "Vertical shift of the overhang relative to the opening head [m].\nDefault: 0",
                    Access = GH_ParamAccess.item
                };
                paramNumber.SetPersistentData(0);
                result.Add(new GH_SAMParam(paramNumber, ParamVisibility.Binding));

                paramNumber = new global::Grasshopper.Kernel.Parameters.Param_Number
                {
                    Name = "_overhangFrontOffset_",
                    NickName = "_overhangFrontOffset_",
                    Description = "Forward/back offset of the overhang from the façade plane [m].\nDefault: 0",
                    Access = GH_ParamAccess.item
                };
                paramNumber.SetPersistentData(0);
                result.Add(new GH_SAMParam(paramNumber, ParamVisibility.Voluntary));

                paramNumber = new global::Grasshopper.Kernel.Parameters.Param_Number
                {
                    Name = "_leftFinDepth_",
                    NickName = "_leftFinDepth_",
                    Description = "Depth of the left vertical fin [m].\nDefault: 0",
                    Access = GH_ParamAccess.item
                };
                paramNumber.SetPersistentData(0);
                result.Add(new GH_SAMParam(paramNumber, ParamVisibility.Binding));

                paramNumber = new global::Grasshopper.Kernel.Parameters.Param_Number
                {
                    Name = "_leftFinOffset_",
                    NickName = "_leftFinOffset_",
                    Description = "Horizontal offset of the left fin from the opening jamb [m].\nDefault: 0",
                    Access = GH_ParamAccess.item
                };
                paramNumber.SetPersistentData(0);
                result.Add(new GH_SAMParam(paramNumber, ParamVisibility.Binding));

                paramNumber = new global::Grasshopper.Kernel.Parameters.Param_Number
                {
                    Name = "_leftFinFrontOffset_",
                    NickName = "_leftFinFrontOffset_",
                    Description = "Forward/back offset of the left fin from the façade plane [m].\nDefault: 0",
                    Access = GH_ParamAccess.item
                };
                paramNumber.SetPersistentData(0);
                result.Add(new GH_SAMParam(paramNumber, ParamVisibility.Voluntary));

                paramNumber = new global::Grasshopper.Kernel.Parameters.Param_Number
                {
                    Name = "_rightFinDepth_",
                    NickName = "_rightFinDepth_",
                    Description = "Depth of the right vertical fin [m].\nDefault: 0",
                    Access = GH_ParamAccess.item
                };
                paramNumber.SetPersistentData(0);
                result.Add(new GH_SAMParam(paramNumber, ParamVisibility.Binding));

                paramNumber = new global::Grasshopper.Kernel.Parameters.Param_Number
                {
                    Name = "_rightFinOffset_",
                    NickName = "_rightFinOffset_",
                    Description = "Horizontal offset of the right fin from the opening jamb [m].\nDefault: 0",
                    Access = GH_ParamAccess.item
                };
                paramNumber.SetPersistentData(0);
                result.Add(new GH_SAMParam(paramNumber, ParamVisibility.Binding));

                paramNumber = new global::Grasshopper.Kernel.Parameters.Param_Number
                {
                    Name = "_rightFinFrontOffset_",
                    NickName = "_rightFinFrontOffset_",
                    Description = "Forward/back offset of the right fin from the façade plane [m].\nDefault: 0",
                    Access = GH_ParamAccess.item
                };
                paramNumber.SetPersistentData(0);
                result.Add(new GH_SAMParam(paramNumber, ParamVisibility.Voluntary));

                Param_Boolean param_Boolean = new Param_Boolean
                {
                    Name = "_concatenate_",
                    NickName = "_concatenate_",
                    Description = "concatenate",
                    Access = GH_ParamAccess.item,
                    Optional = true
                };
                param_Boolean.SetPersistentData(true);
                result.Add(new GH_SAMParam(param_Boolean, ParamVisibility.Binding));

                return [.. result];
            }
        }

        /// <summary>Registers all the output parameters for this component.</summary>
        protected override GH_SAMParam[] Outputs
        {
            get
            {
                List<GH_SAMParam> result = [];

                var analyticalModelParam = new GooAnalyticalModelParam
                {
                    Name = "CaseAModel",
                    NickName = "CaseAModel",
                    Description = "Copy of the base model with shade Panels added based on the given depths/offsets.",
                    Access = GH_ParamAccess.item
                };
                result.Add(new GH_SAMParam(analyticalModelParam, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_String param_String = new() { Name = "CaseDescription", NickName = "CaseDescription", Description = "Case Description", Access = GH_ParamAccess.item };
                result.Add(new GH_SAMParam(param_String, ParamVisibility.Binding));

                return [.. result];
            }
        }

        // ────────────────────────────────────────────────────────────────────────────────
        // Execution
        // ────────────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Core execution: read inputs, create shades, and output the case model.
        /// </summary>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            // Base model (required)
            int index = Params.IndexOfInputParam("_baseAModel");
            AnalyticalModel analyticalModel = null;
            if (index == -1 || !dataAccess.GetData(index, ref analyticalModel) || analyticalModel == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "_baseAModel: Please supply a valid SAM AnalyticalModel.");
                return;
            }

            // Inputs
            index = Params.IndexOfInputParam("_analyticalObjects");
            List<IAnalyticalObject> analyticalObjects = [];
            if (index != -1)
            {
                dataAccess.GetDataList(index, analyticalObjects);
            }

            index = Params.IndexOfInputParam("_glassPartOnly");
            bool glassPartOnly = false;
            if (index != -1)
            {
                dataAccess.GetData(index, ref glassPartOnly);
            }

            index = Params.IndexOfInputParam("_overhangDepth_");
            double overhangDepth = 0.0; if (index != -1) dataAccess.GetData(index, ref overhangDepth);

            index = Params.IndexOfInputParam("_overhangVerticalOffset_");
            double overhangVerticalOffset = 0.0; if (index != -1) dataAccess.GetData(index, ref overhangVerticalOffset);

            index = Params.IndexOfInputParam("_overhangFrontOffset_");
            double overhangFrontOffset = 0.0; if (index != -1) dataAccess.GetData(index, ref overhangFrontOffset);

            index = Params.IndexOfInputParam("_leftFinDepth_");
            double leftFinDepth = 0.0; if (index != -1) dataAccess.GetData(index, ref leftFinDepth);

            index = Params.IndexOfInputParam("_leftFinOffset_");
            double leftFinOffset = 0.0; if (index != -1) dataAccess.GetData(index, ref leftFinOffset);

            index = Params.IndexOfInputParam("_leftFinFrontOffset_");
            double leftFinFrontOffset = 0.0; if (index != -1) dataAccess.GetData(index, ref leftFinFrontOffset);

            index = Params.IndexOfInputParam("_rightFinDepth_");
            double rightFinDepth = 0.0; if (index != -1) dataAccess.GetData(index, ref rightFinDepth);

            index = Params.IndexOfInputParam("_rightFinOffset_");
            double rightFinOffset = 0.0; if (index != -1) dataAccess.GetData(index, ref rightFinOffset);

            index = Params.IndexOfInputParam("_rightFinFrontOffset_");
            double rightFinFrontOffset = 0.0; if (index != -1) dataAccess.GetData(index, ref rightFinFrontOffset);

            if (analyticalObjects != null && analyticalObjects.Count != 0)
            {
                AdjacencyCluster adjacencyCluster = new AdjacencyCluster(analyticalModel.AdjacencyCluster, true);

                foreach (IAnalyticalObject analyticalObject in analyticalObjects)
                {
                    List<Panel> shades = null;
                    if (analyticalObject is Panel panel)
                    {
                        shades = Create.Panels_Shade(panel, overhangDepth, overhangVerticalOffset, overhangFrontOffset,
                                                     leftFinDepth, leftFinOffset, leftFinFrontOffset,
                                                     rightFinDepth, rightFinOffset, rightFinFrontOffset);
                    }
                    else if (analyticalObject is Aperture aperture)
                    {
                        shades = Create.Panels_Shade(aperture, glassPartOnly, overhangDepth, overhangVerticalOffset, overhangFrontOffset,
                                                     leftFinDepth, leftFinOffset, leftFinFrontOffset,
                                                     rightFinDepth, rightFinOffset, rightFinFrontOffset);
                    }

                    if (shades == null) continue;

                    foreach (Panel shade in shades)
                    {
                        adjacencyCluster.AddObject(shade);
                    }
                }

                analyticalModel = new AnalyticalModel(analyticalModel, adjacencyCluster);
            }

            index = Params.IndexOfOutputParam("CaseDescription");
            if (index != -1)
            {
                int index_Concatenate = Params.IndexOfInputParam("_concatenate_");
                bool concatenate = true;
                if (index_Concatenate != -1)
                {
                    dataAccess.GetData(index_Concatenate, ref concatenate);
                }

                string caseDescription = string.Empty;
                if (concatenate)
                {
                    if (!Core.Query.TryGetValue(analyticalModel, "CaseDescription", out caseDescription))
                    {
                        caseDescription = string.Empty;
                    }
                }

                if (string.IsNullOrWhiteSpace(caseDescription))
                {
                    caseDescription = "Case";
                }
                else
                {
                    caseDescription += "_";
                }

                string sufix = "ByShade_";
                if (overhangDepth != 0)
                {
                    sufix += string.Format("O{0}m", overhangDepth);
                }

                if (leftFinDepth != 0)
                {
                    sufix += string.Format("L{0}m", leftFinDepth);
                }

                if (rightFinDepth != 0)
                {
                    sufix += string.Format("R{0}m", rightFinDepth);
                }

                string value = caseDescription + sufix;

                dataAccess.SetData(index, value);
            }

            // Output
            index = Params.IndexOfOutputParam("CaseAModel");
            if (index != -1)
            {
                dataAccess.SetData(index, analyticalModel);
            }
        }
    }
}
