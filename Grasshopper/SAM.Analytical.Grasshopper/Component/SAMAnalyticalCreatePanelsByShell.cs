using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Linq;
using System.Collections.Generic;
using Grasshopper.Kernel.Types;
using SAM.Geometry.Spatial;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalCreatePanelsByShell : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("7030c1cb-8b48-4afe-be37-4e9952a90fbb");

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
        public SAMAnalyticalCreatePanelsByShell()
          : base("SAMAnalytical.CreatePanelsByShell", "SAMAnalytical.CreatePanelsByShell",
              "Creates Panels By Shell",
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

                global::Grasshopper.Kernel.Parameters.Param_GenericObject genericObject = new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_shell", NickName = "_shell", Description = "SAM Geometry Shell", Access = GH_ParamAccess.item };
                result.Add(new GH_SAMParam(genericObject, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Number number;

                number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "silverSpacing_", NickName = "silverSpacing_", Description = "Silver Spacing", Access = GH_ParamAccess.item };
                number.SetPersistentData(Tolerance.MacroDistance);
                result.Add(new GH_SAMParam(number, ParamVisibility.Voluntary));

                number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "tolerance_", NickName = "tolerance_", Description = "Tolerance", Access = GH_ParamAccess.item };
                number.SetPersistentData(Tolerance.Distance);
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
                result.Add(new GH_SAMParam(new GooPanelParam() { Name = "Walls", NickName = "Walls", Description = "SAM Analytical Wall Panels", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooPanelParam() { Name = "Floors", NickName = "Floors", Description = "SAM Analytical Floor Panels", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooPanelParam() { Name = "Roofs", NickName = "Roofs", Description = "SAM Analytical Roof Panels", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooPanelParam() { Name = "Others", NickName = "Others", Description = "SAM Analytical Other Panels", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
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
            int index;

            GH_ObjectWrapper objectWrapper = null;
            index = Params.IndexOfInputParam("_shell");
            if (index == -1 || !dataAccess.GetData(index, ref objectWrapper) || objectWrapper == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            Shell shell = null;
            if (Geometry.Grasshopper.Query.TryGetSAMGeometries(objectWrapper, out List<Shell> shells) && shells != null)
                shell = shells.FirstOrDefault();

            if(shell == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }


            double silverSpacing = Tolerance.MacroDistance;
            index = Params.IndexOfInputParam("silverSpacing_");
            if (index != -1)
                dataAccess.GetData(index, ref silverSpacing);

            double tolerance = Tolerance.Distance;
            index = Params.IndexOfInputParam("tolerance_");
            if (index != -1)
                dataAccess.GetData(index, ref tolerance);

            List<Panel> panels = Create.Panels(shell, silverSpacing, tolerance);


            index = Params.IndexOfOutputParam("Walls");
            if (index != -1)
                dataAccess.SetDataList(index, panels?.FindAll(x => x.PanelGroup == PanelGroup.Wall));

            index = Params.IndexOfOutputParam("Floors");
            if (index != -1)
                dataAccess.SetDataList(index, panels?.FindAll(x => x.PanelGroup == PanelGroup.Floor));

            index = Params.IndexOfOutputParam("Roofs");
            if (index != -1)
                dataAccess.SetDataList(index, panels?.FindAll(x => x.PanelGroup == PanelGroup.Roof));

            index = Params.IndexOfOutputParam("Others");
            if (index != -1)
                dataAccess.SetDataList(index, panels?.FindAll(x => x.PanelGroup == PanelGroup.Other));

        }
    }
}