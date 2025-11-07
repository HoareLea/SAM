using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    /// <summary>
    /// Create a case-specific AnalyticalModel by uniformly scaling selected apertures (windows).
    /// Makes a copy of the base model; the original stays unchanged.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This component prepares a case-specific <see cref="AnalyticalModel"/> for parametric studies by
    /// rescaling aperture size(s) with a single, dimensionless factor (e.g., 0.8 → 80%, 1.2 → 120%).
    /// If no apertures are supplied, all apertures in the model are considered.
    /// A copy of the base model is made so your original model is not changed.
    /// </para>
    /// </remarks>
    public class SAMAnalyticalCreateCaseByWindowSize : GH_SAMVariableOutputParameterComponent
    {
        // ────────────────────────────────────────────────────────────────────────────────
        // Metadata
        // ────────────────────────────────────────────────────────────────────────────────

        /// <summary>Gets the unique ID for this component. Do not change this ID after release.</summary>
        public override Guid ComponentGuid => new Guid("41f36339-ae22-4de5-9586-0b9519ad60a4");

        /// <summary>The latest version of this component.</summary>
        public override string LatestComponentVersion => "1.0.3";

        /// <summary>Component icon shown on the Grasshopper canvas.</summary>
        protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        /// <summary>Display priority in the Grasshopper ribbon.</summary>
        public override GH_Exposure Exposure => GH_Exposure.primary;

        // Long, multi-line description for tooltips and docs (MEP-friendly SAM style)
        private const string DescriptionLong =
@"Create a case-specific AnalyticalModel by scaling window/aperture size.

SUMMARY
  • Makes a copy of _baseAModel_; the original stays unchanged.
  • Applies a uniform scale factor to each chosen aperture (dimensionless).
  • If _apertures_ is empty, all model apertures are used.

INPUTS
  _baseAModel  (AnalyticalModel, required)
    Base SAM AnalyticalModel used as a template for this case.
    Type: SAM.Analytical.AnalyticalModel

  _apertures_  (Aperture[], optional)
    Specific apertures to rescale. Leave empty to use all apertures in the model.
    Type: SAM.Analytical.Aperture

  _apertureScaleFactor_  (Number, optional, default = 1.0)
    Uniform scale factor for aperture size (dimensionless).
    Examples: 0.8 → 80% of current size; 1.2 → 120%.

OUTPUTS
  CaseAModel  (AnalyticalModel)
    Copy of the base model with updated aperture sizes.

NOTES
  • Scaling is uniform in aperture width and height.
  • If _apertureScaleFactor_ is not provided (NaN), apertures are not changed.

EXAMPLE
  1) Read a baseline AnalyticalModel → _baseAModel
  2) (Optional) Select apertures to rescale → _apertures_
  3) Set _apertureScaleFactor_ (e.g., 0.9 or 1.1)
  4) Get the case model → CaseAModel
";

        /// <summary>Initializes a new instance of the component.</summary>
        public SAMAnalyticalCreateCaseByWindowSize()
          : base(
                "SAMAnalytical.CreateCaseByWindowSize",
                "CreateCaseByWindowSize",
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

                // Apertures (optional)
                var gooApertureParam = new GooApertureParam
                {
                    Name = "_apertures_",
                    NickName = "_apertures_",
                    Description = "Apertures to rescale. Leave empty to use all apertures in the model.\nType: SAM.Analytical.Aperture\nRequired: No",
                    Access = GH_ParamAccess.list,
                    Optional = true
                };
                result.Add(new GH_SAMParam(gooApertureParam, ParamVisibility.Binding));

                // Scale factor (optional; default 1.0)
                var param_Number = new global::Grasshopper.Kernel.Parameters.Param_Number
                {
                    Name = "_apertureScaleFactor_",
                    NickName = "_apertureScaleFactor_",
                    Description = "Uniform scale factor (dimensionless).\nExamples: 0.8 → 80% ; 1.2 → 120%.\nDefault: 1.0",
                    Access = GH_ParamAccess.item,
                    Optional = true
                };
                param_Number.SetPersistentData(1.0);
                result.Add(new GH_SAMParam(param_Number, ParamVisibility.Binding));

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
                    Description = "Copy of the base model with aperture sizes scaled by _apertureScaleFactor_.",
                    Access = GH_ParamAccess.item
                };
                result.Add(new GH_SAMParam(analyticalModelParam, ParamVisibility.Binding));

                Param_String param_String = new() { Name = "CaseDescription", NickName = "CaseDescription", Description = "Case Description", Access = GH_ParamAccess.item };
                result.Add(new GH_SAMParam(param_String, ParamVisibility.Binding));

                return [.. result];
            }
        }

        // ────────────────────────────────────────────────────────────────────────────────
        // Execution
        // ────────────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Core execution: read inputs, rescale apertures, and output the case model.
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

            // Apertures (optional)
            index = Params.IndexOfInputParam("_apertures_");
            List<Aperture> apertures = [];
            if (index != -1)
            {
                dataAccess.GetDataList(index, apertures);
            }

            // Scale factor (optional)
            index = Params.IndexOfInputParam("_apertureScaleFactor_");
            double apertureScaleFactor = double.NaN;
            if (index != -1)
            {
                dataAccess.GetData(index, ref apertureScaleFactor);
            }

            // Resolve aperture list from model when not provided
            if (apertures.Count == 0)
            {
                apertures = analyticalModel.GetApertures();
            }
            else
            {
                // Clean input list and re-fetch from model to ensure consistency
                for (int i = apertures.Count - 1; i >= 0; i--)
                {
                    if (apertures[i] is null)
                    {
                        apertures.RemoveAt(i);
                        continue;
                    }

                    Aperture aperture = analyticalModel.GetAperture(apertures[i].Guid, out Panel panel);
                    if (aperture is null)
                    {
                        apertures.RemoveAt(i);
                        continue;
                    }

                    apertures[i] = new Aperture(aperture);
                }
            }

            // Apply scaling when we have targets and a valid factor
            if (apertures != null && apertures.Count != 0 && !double.IsNaN(apertureScaleFactor))
            {
                AdjacencyCluster adjacencyCluster = new(analyticalModel.AdjacencyCluster, true);

                foreach (Aperture aperture in apertures)
                {
                    Aperture aperture_Temp = aperture.Rescale(apertureScaleFactor);
                    if (aperture_Temp is null)
                    {
                        continue;
                    }

                    if (adjacencyCluster.GetAperture(aperture_Temp.Guid, out Panel panel_Temp) is null || panel_Temp is null)
                    {
                        continue;
                    }

                    panel_Temp = Create.Panel(panel_Temp);

                    panel_Temp.RemoveAperture(aperture_Temp.Guid);
                    panel_Temp.AddAperture(aperture_Temp);

                    adjacencyCluster.AddObject(panel_Temp);
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

                string sufix = "ByWindowSize_";
                if (!double.IsNaN(apertureScaleFactor))
                {
                    sufix += apertureScaleFactor.ToString();
                }

                string value = caseDescription + sufix;

                dataAccess.SetData(index, value);
            }

            if (!analyticalModel.TryGetValue(AnalyticalModelParameter.CaseDataCollection, out CaseDataCollection caseDataCollection))
            {
                caseDataCollection = new CaseDataCollection();
            }
            else
            {
                caseDataCollection = new CaseDataCollection(caseDataCollection);
            }

            caseDataCollection.Values.Add(new WindowSizeCaseData(apertureScaleFactor));

            // Output
            index = Params.IndexOfOutputParam("CaseAModel");
            if (index != -1)
            {
                dataAccess.SetData(index, analyticalModel);
            }
        }
    }
}
