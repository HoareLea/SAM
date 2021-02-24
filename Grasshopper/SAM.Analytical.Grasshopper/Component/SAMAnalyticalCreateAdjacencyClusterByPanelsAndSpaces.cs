using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using SAM.Geometry;
using SAM.Geometry.Grasshopper;
using SAM.Geometry.Planar;
using SAM.Geometry.Spatial;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalCreateAdjacencyClusterByPanelsAndSpaces : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("1f2d4717-f2ec-463e-84bd-f8c0f7f230c7");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.1";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public override GH_Exposure Exposure => GH_Exposure.primary;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalCreateAdjacencyClusterByPanelsAndSpaces()
          : base("SAMAnalytical.CreateAdjacencyClusterByPanelsAndSpaces", "SAMAnalytical.CreateAdjacencyClusterByPanelsAndSpaces",
              "Create AdjacencyCluster from Panels And Spaces",
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

                GooPanelParam panelParam = new GooPanelParam() { Name = "_panels", NickName = "_panels", Description = "SAM Analytical Panels", Access = GH_ParamAccess.list };
                panelParam.DataMapping = GH_DataMapping.Flatten;
                result.Add(new GH_SAMParam(panelParam, ParamVisibility.Binding));

                GooSpaceParam spaceParam = new GooSpaceParam() { Name = "_spaces_", NickName = "_spaces_", Description = "SAM Analytical Spaces", Access = GH_ParamAccess.list, Optional = true };
                spaceParam.DataMapping = GH_DataMapping.Flatten;
                result.Add(new GH_SAMParam(spaceParam, ParamVisibility.Voluntary));

                global::Grasshopper.Kernel.Parameters.Param_Number paramNumber;

                paramNumber = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "offset_", NickName = "offset_", Description = "Offset from the floors for computation of Spaces", Access = GH_ParamAccess.item };
                paramNumber.SetPersistentData(0.1);
                result.Add(new GH_SAMParam(paramNumber, ParamVisibility.Voluntary));

                global::Grasshopper.Kernel.Parameters.Param_Boolean paramBoolean = new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "addMissingSpaces_", NickName = "addMissingSpaces_", Description = "Add Missing Spaces", Access = GH_ParamAccess.item };
                paramBoolean.SetPersistentData(true);
                result.Add(new GH_SAMParam(paramBoolean, ParamVisibility.Voluntary));

                paramNumber = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "snapTolerance_", NickName = "snapTolerance_", Description = "Snap Tolerance for computation of Spaces", Access = GH_ParamAccess.item };
                paramNumber.SetPersistentData(Core.Tolerance.MacroDistance);
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

            index = Params.IndexOfInputParam("offset_");
            double offset = 0.1;
            if (index != -1)
                dataAccess.GetData(index, ref offset);

            index = Params.IndexOfInputParam("snapTolerance_");
            double snapTolerance = Core.Tolerance.MacroDistance;
            if (index != -1)
                dataAccess.GetData(index, ref snapTolerance);

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

            AdjacencyCluster adjacencyCluster = new AdjacencyCluster();
            
            foreach (Panel panel in panels)
                adjacencyCluster.AddObject(panel);

            foreach (Space space in spaces)
                adjacencyCluster.AddObject(space);

            adjacencyCluster.RegenerateSpaces(offset, addMissingSpaces, snapTolerance, silverSpacing, minArea, tolerance);

            index = Params.IndexOfOutputParam("AdjacencyCluster");
            if (index != -1)
                dataAccess.SetData(index, new GooAdjacencyCluster(adjacencyCluster));
        }
    }
}