using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalAdjustPanels : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("9f4ac540-0c21-463f-b514-fd790e0611b8");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public override GH_Exposure Exposure => GH_Exposure.primary;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalAdjustPanels()
          : base("SAMAnalytical.AdjustPanels", "SAMAnalytical.AdjustPanels",
              "Adjusts Panels",
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
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                GooPanelParam gooPanelParam = new GooPanelParam() { Name = "_panels", NickName = "_panels", Description = "SAM Analytical Panels", Access = GH_ParamAccess.list };
                gooPanelParam.DataMapping = GH_DataMapping.Flatten;
                result.Add(new GH_SAMParam(gooPanelParam, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_GenericObject genericObject = new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_face3D", NickName = "_face3D", Description = "SAM Geometry Face3D", Access = GH_ParamAccess.item };
                genericObject.DataMapping = GH_DataMapping.Flatten;
                result.Add(new GH_SAMParam(genericObject, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Number number = null;

                number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "maxDistance_", NickName = "maxDistance_", Description = "max Distance", Access = GH_ParamAccess.item };
                number.SetPersistentData(0.5);
                result.Add(new GH_SAMParam(number, ParamVisibility.Voluntary));

                number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "tolerance_", NickName = "tolerance_", Description = "Tolerance", Access = GH_ParamAccess.item };
                number.SetPersistentData(Core.Tolerance.Distance);
                result.Add(new GH_SAMParam(number, ParamVisibility.Voluntary));

                return result.ToArray();
            }
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override GH_SAMParam[] Outputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new GooPanelParam() { Name = "panels", NickName = "panels", Description = "SAM Analytical Panels", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                return result.ToArray();
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
            
            List<Panel> panels = new List<Panel>();
            index = Params.IndexOfInputParam("_panels");
            if (index == -1 || !dataAccess.GetDataList(index, panels) || panels == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            GH_ObjectWrapper objectWrapper = null;
            index = Params.IndexOfInputParam("_face3D");
            if (index == -1 || !dataAccess.GetData(index, ref objectWrapper) || objectWrapper == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Geometry.Spatial.Face3D> face3Ds = new List<Geometry.Spatial.Face3D>();
            if (Geometry.Grasshopper.Query.TryGetSAMGeometries(objectWrapper, out List<Geometry.Spatial.Face3D> face3Ds_Temp) && face3Ds_Temp != null)
                face3Ds.AddRange(face3Ds_Temp);

            double tolerance = Core.Tolerance.Distance;
            double tolerance_Temp = double.NaN; ;
            index = Params.IndexOfInputParam("tolerance_");
            if (index != -1 && dataAccess.GetData(index, ref tolerance_Temp))
            {
                tolerance = tolerance_Temp;
            }

            double maxDistance = 0.5;
            double maxDistance_Temp = double.NaN; ;
            index = Params.IndexOfInputParam("maxDistance_");
            if (index != -1 && dataAccess.GetData(index, ref maxDistance_Temp))
            {
                maxDistance = maxDistance_Temp;
            }

            List<Panel> result = Analytical.Query.AdjustPanels(face3Ds?.FirstOrDefault(), panels, maxDistance, tolerance);

            index = Params.IndexOfOutputParam("panels");
            if (index != -1)
                dataAccess.SetDataList(index, result);
        }
    }
}