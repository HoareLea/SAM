using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalCreateAdjacencyClusterByShells : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("ed472726-efc1-4b95-8049-db76e37d42c5");

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
        public SAMAnalyticalCreateAdjacencyClusterByShells()
          : base("SAMAnalytical.CreateAdjacencyClusterByShells", "SAMAnalytical.CreateAdjacencyClusterByShells",
              "Create AdjacencyCluster from Shells, Panels And Spaces",
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

                global::Grasshopper.Kernel.Parameters.Param_GenericObject genericObject = new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_shells", NickName = "_shells", Description = "SAM Analytical Shells", Access = GH_ParamAccess.list };
                genericObject.DataMapping = GH_DataMapping.Flatten;
                result.Add(new GH_SAMParam(genericObject, ParamVisibility.Binding));

                GooPanelParam panelParam = new GooPanelParam() { Name = "_panels", NickName = "_panels", Description = "SAM Analytical Panels", Access = GH_ParamAccess.list };
                panelParam.DataMapping = GH_DataMapping.Flatten;
                result.Add(new GH_SAMParam(panelParam, ParamVisibility.Binding));

                GooSpaceParam spaceParam = new GooSpaceParam() { Name = "_spaces_", NickName = "_spaces_", Description = "SAM Analytical Spaces", Access = GH_ParamAccess.list, Optional = true };
                spaceParam.DataMapping = GH_DataMapping.Flatten;
                result.Add(new GH_SAMParam(spaceParam, ParamVisibility.Voluntary));

                global::Grasshopper.Kernel.Parameters.Param_Number paramNumber;

                global::Grasshopper.Kernel.Parameters.Param_Boolean paramBoolean = new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "addMissingSpaces_", NickName = "addMissingSpaces_", Description = "Add Missing Spaces", Access = GH_ParamAccess.item };
                paramBoolean.SetPersistentData(true);
                result.Add(new GH_SAMParam(paramBoolean, ParamVisibility.Voluntary));

                paramNumber = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "maxDistance_", NickName = "maxDistance_", Description = "Max Distance", Access = GH_ParamAccess.item };
                paramNumber.SetPersistentData(0.01);
                result.Add(new GH_SAMParam(paramNumber, ParamVisibility.Voluntary));

                paramNumber = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "silverSpacing_", NickName = "silverSpacing_", Description = "Silver Spacing for computation of Spaces", Access = GH_ParamAccess.item };
                paramNumber.SetPersistentData(Core.Tolerance.MacroDistance);
                result.Add(new GH_SAMParam(paramNumber, ParamVisibility.Voluntary));

                paramNumber = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "minArea_", NickName = "minArea_", Description = "Minimal Area", Access = GH_ParamAccess.item };
                paramNumber.SetPersistentData(Core.Tolerance.MacroDistance);
                result.Add(new GH_SAMParam(paramNumber, ParamVisibility.Voluntary));

                paramNumber = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "tolerance_", NickName = "tolerance_", Description = "Tolerance", Access = GH_ParamAccess.item };
                paramNumber.SetPersistentData(Core.Tolerance.Distance);
                result.Add(new GH_SAMParam(paramNumber, ParamVisibility.Binding));


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
                result.Add(new GH_SAMParam(new GooAdjacencyClusterParam() { Name = "AdjacencyCluster", NickName = "AdjacencyCluster", Description = "SAM Analytical AdjacencyCluster", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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

            index = Params.IndexOfInputParam("_shells");
            List<GH_ObjectWrapper> objectWrappers = new List<GH_ObjectWrapper>();
            if (index == -1 || !dataAccess.GetDataList(index, objectWrappers) || objectWrappers == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Shell> shells = new List<Shell>();
            foreach (GH_ObjectWrapper objectWrapper in objectWrappers)
                if (Geometry.Grasshopper.Query.TryGetSAMGeometries(objectWrapper, out List<Shell> shells_Temp) && shells_Temp != null)
                    shells.AddRange(shells_Temp);

            index = Params.IndexOfInputParam("_panels");
            List<Panel> panels = new List<Panel>();
            if (index == -1 || !dataAccess.GetDataList(index, panels) || panels == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_spaces_");
            List<Space> spaces = new List<Space>();
            if (index != -1)
                dataAccess.GetDataList(index, spaces);

            index = Params.IndexOfInputParam("maxDistance_");
            double maxDistance = 0.01;
            if (index != -1)
                dataAccess.GetData(index, ref maxDistance);

            index = Params.IndexOfInputParam("addMissingSpaces_");
            bool addMissingSpaces = true;
            if (index != -1)
                dataAccess.GetData(index, ref addMissingSpaces);

            index = Params.IndexOfInputParam("silverSpacing_");
            double silverSpacing = Core.Tolerance.MacroDistance;
            if (index != -1)
                dataAccess.GetData(index, ref silverSpacing);

            index = Params.IndexOfInputParam("minArea_");
            double minArea = Core.Tolerance.MacroDistance;
            if (index != -1)
                dataAccess.GetData(index, ref minArea);

            index = Params.IndexOfInputParam("tolerance_");
            double tolerance = Core.Tolerance.Distance;
            if (index != -1)
                dataAccess.GetData(index, ref tolerance);

            AdjacencyCluster adjacencyCluster = Create.AdjacencyCluster(shells, spaces, panels, addMissingSpaces, minArea, maxDistance, silverSpacing, tolerance);

            index = Params.IndexOfOutputParam("AdjacencyCluster");
            if (index != -1)
                dataAccess.SetData(index, new GooAdjacencyCluster(adjacencyCluster));
        }
    }
}