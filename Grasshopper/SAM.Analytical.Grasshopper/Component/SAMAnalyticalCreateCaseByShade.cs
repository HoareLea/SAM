using Grasshopper.Kernel;
using Rhino.Render.DataSources;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalCreateCaseByShade : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("a8d0db57-5d59-493c-9dae-02ccdb7a169c");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        public override GH_Exposure Exposure => GH_Exposure.primary;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalCreateCaseByShade()
          : base("SAMAnalytical.CreateCaseByShade", "SAMAnalytical.CreateCaseByShade",
              "AAA",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override GH_SAMParam[] Inputs
        {
            get
            {
                List<GH_SAMParam> result = [];

                GooAnalyticalModelParam analyticalModelParam = new () { Name = "_baseAModel", NickName = "_baseAModel", Description = "Analytical Model", Access = GH_ParamAccess.item };
                result.Add(new GH_SAMParam(analyticalModelParam, ParamVisibility.Binding));

                GooAnalyticalObjectParam analyticalObject = new GooAnalyticalObjectParam() { Name = "_analyticalObjects", NickName = "_analyticalObjects", Description = "SAM AnalyticalObject such as Aperture or Panel", Access = GH_ParamAccess.list };
                result.Add(new GH_SAMParam(analyticalObject, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Boolean paramBoolean;

                paramBoolean = new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "_glassPartOnly", NickName = "_glassPartOnly", Description = "Glass part only", Access = GH_ParamAccess.item };
                paramBoolean.SetPersistentData(false);
                result.Add(new GH_SAMParam(paramBoolean, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Number paramNumber;

                paramNumber = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_overhangDepth_", NickName = "_overhangDepth_", Description = "Overhang depth [m]", Access = GH_ParamAccess.item };
                paramNumber.SetPersistentData(0);
                result.Add(new GH_SAMParam(paramNumber, ParamVisibility.Binding));

                paramNumber = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_overhangVerticalOffset_", NickName = "_overhangVerticalOffset_", Description = "Overhang vertical offset [m]", Access = GH_ParamAccess.item };
                paramNumber.SetPersistentData(0);
                result.Add(new GH_SAMParam(paramNumber, ParamVisibility.Binding));

                paramNumber = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_overhangFrontOffset_", NickName = "_overhangFrontOffset_", Description = "Overhang front offset [m]", Access = GH_ParamAccess.item };
                paramNumber.SetPersistentData(0);
                result.Add(new GH_SAMParam(paramNumber, ParamVisibility.Voluntary));

                paramNumber = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_leftFinDepth_", NickName = "_leftFinDepth_", Description = "Left fin depth [m]", Access = GH_ParamAccess.item };
                paramNumber.SetPersistentData(0);
                result.Add(new GH_SAMParam(paramNumber, ParamVisibility.Binding));

                paramNumber = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_leftFinOffset_", NickName = "_leftFinOffset_", Description = "Left fin offset [m]", Access = GH_ParamAccess.item };
                paramNumber.SetPersistentData(0);
                result.Add(new GH_SAMParam(paramNumber, ParamVisibility.Binding));

                paramNumber = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_leftFinFrontOffset_", NickName = "_leftFinFrontOffset_", Description = "Left fin front offset [m]", Access = GH_ParamAccess.item };
                paramNumber.SetPersistentData(0);
                result.Add(new GH_SAMParam(paramNumber, ParamVisibility.Voluntary));

                paramNumber = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_rightFinDepth_", NickName = "_rightFinDepth_", Description = "Right fin depth [m]", Access = GH_ParamAccess.item };
                paramNumber.SetPersistentData(0);
                result.Add(new GH_SAMParam(paramNumber, ParamVisibility.Binding));

                paramNumber = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_rightFinOffset_", NickName = "_rightFinOffset_", Description = "Right fin offset [m]", Access = GH_ParamAccess.item };
                paramNumber.SetPersistentData(0);
                result.Add(new GH_SAMParam(paramNumber, ParamVisibility.Binding));

                paramNumber = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_rightFinFrontOffset_", NickName = "_rightFinFrontOffset_", Description = "Right fin front offset [m]", Access = GH_ParamAccess.item };
                paramNumber.SetPersistentData(0);
                result.Add(new GH_SAMParam(paramNumber, ParamVisibility.Voluntary));

                return [.. result];
            }
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override GH_SAMParam[] Outputs
        {
            get
            {
                List<GH_SAMParam> result = [];

                GooAnalyticalModelParam analyticalModelParam = new () { Name = "CaseAModel", NickName = "CaseAModel", Description = "SAM AnalyticalModel", Access = GH_ParamAccess.item };
                result.Add(new GH_SAMParam(analyticalModelParam, ParamVisibility.Binding));

                return [.. result];
            }
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            int index = -1;
            index = Params.IndexOfInputParam("_baseAModel");
            AnalyticalModel analyticalModel = null;
            if (index == -1 || !dataAccess.GetData(index, ref analyticalModel) || analyticalModel == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

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
            double overhangDepth = 0.0;
            if (index != -1)
            {
                dataAccess.GetData(index, ref overhangDepth);
            }

            index = Params.IndexOfInputParam("_overhangVerticalOffset_");
            double overhangVerticalOffset = 0.0;
            if (index != -1)
            {
                dataAccess.GetData(index, ref overhangVerticalOffset);
            }

            index = Params.IndexOfInputParam("_overhangFrontOffset_");
            double overhangFrontOffset = 0.0;
            if (index != -1)
            {
                dataAccess.GetData(index, ref overhangFrontOffset);
            }

            index = Params.IndexOfInputParam("_leftFinDepth_");
            double leftFinDepth = 0.0;
            if (index != -1)
            {
                dataAccess.GetData(index, ref leftFinDepth);
            }

            index = Params.IndexOfInputParam("_leftFinOffset_");
            double leftFinOffset = 0.0;
            if (index != -1)
            {
                dataAccess.GetData(index, ref leftFinOffset);
            }

            index = Params.IndexOfInputParam("_leftFinFrontOffset_");
            double leftFinFrontOffset = 0.0;
            if (index != -1)
            {
                dataAccess.GetData(index, ref leftFinFrontOffset);
            }


            index = Params.IndexOfInputParam("_rightFinDepth_");
            double rightFinDepth = 0.0;
            if (index != -1)
            {
                dataAccess.GetData(index, ref rightFinDepth);
            }

            index = Params.IndexOfInputParam("_rightFinOffset_");
            double rightFinOffset = 0.0;
            if (index != -1)
            {
                dataAccess.GetData(index, ref rightFinOffset);
            }

            index = Params.IndexOfInputParam("_rightFinFrontOffset_");
            double rightFinFrontOffset = 0.0;
            if (index != -1)
            {
                dataAccess.GetData(index, ref rightFinFrontOffset);
            }

            if (analyticalObjects != null && analyticalObjects.Count != 0)
            {
                AdjacencyCluster adjacencyCluster = new (analyticalModel.AdjacencyCluster, true);;

                foreach (IAnalyticalObject analyticalObject in analyticalObjects)
                {
                    List<Panel> shades = null;
                    if (analyticalObject is Panel panel)
                    {
                        shades = Create.Panels_Shade(panel, overhangDepth, overhangVerticalOffset, overhangFrontOffset, leftFinDepth, leftFinOffset, leftFinFrontOffset, rightFinDepth, rightFinOffset, rightFinFrontOffset);
                    }
                    else if (analyticalObject is Aperture aperture)
                    {
                        shades = Create.Panels_Shade(aperture, glassPartOnly, overhangDepth, overhangVerticalOffset, overhangFrontOffset, leftFinDepth, leftFinOffset, leftFinFrontOffset, rightFinDepth, rightFinOffset, rightFinFrontOffset);
                    }

                    if(shades == null)
                    {
                        continue;
                    }

                    foreach(Panel shade in shades)
                    {
                        adjacencyCluster.AddObject(shade);
                    }
                }

                analyticalModel = new AnalyticalModel(analyticalModel, adjacencyCluster);
            }

            index = Params.IndexOfOutputParam("CaseAModel");
            if (index != -1)
            {
                dataAccess.SetData(index, analyticalModel);
            }
        }
    }
}