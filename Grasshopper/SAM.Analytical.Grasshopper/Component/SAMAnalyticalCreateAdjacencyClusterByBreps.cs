using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalCreateAdjacencyClusterByBreps : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("e37695b3-7438-49e6-b12e-7a6102ca2051");

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
        public SAMAnalyticalCreateAdjacencyClusterByBreps()
          : base("SAMAnalytical.CreateAdjacencyClusterByBreps", "SAMAnalytical.CreateAdjacencyClusterByBreps",
              "Create AdjacencyCluster from Breps",
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

                global::Grasshopper.Kernel.Parameters.Param_GenericObject genericObject = new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_breps", NickName = "_breps", Description = "Rhino Breps", Access = GH_ParamAccess.list };
                genericObject.DataMapping = GH_DataMapping.Flatten;
                result.Add(new GH_SAMParam(genericObject, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Number paramNumber;

                paramNumber = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "elevationGround_", NickName = "elevationGround_", Description = "Ground Elevation", Access = GH_ParamAccess.item };
                paramNumber.SetPersistentData(0);
                result.Add(new GH_SAMParam(paramNumber, ParamVisibility.Voluntary));

                paramNumber = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "maxDistance_", NickName = "maxDistance_", Description = "Max Distance", Access = GH_ParamAccess.item };
                paramNumber.SetPersistentData(0.01);
                result.Add(new GH_SAMParam(paramNumber, ParamVisibility.Voluntary));

                paramNumber = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "maxAngle_", NickName = "maxAngle_", Description = "Max Angle", Access = GH_ParamAccess.item };
                paramNumber.SetPersistentData(0.0872664626);
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
            index = Params.IndexOfInputParam("_breps");
            List<GH_ObjectWrapper> objectWrappers = new List<GH_ObjectWrapper>();
            if (index == -1 || !dataAccess.GetDataList(index, objectWrappers) || objectWrappers == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<global::Rhino.Geometry.Brep> breps = new List<global::Rhino.Geometry.Brep>();
            foreach(GH_ObjectWrapper objectWrapper in objectWrappers)
            {
                object @object = objectWrapper?.Value;
                if(@object == null)
                {
                    continue;
                }

                if(@object is IGH_Goo)
                {
                    @object = (@object as dynamic).Value;
                }

                if (@object is Mesh3D)
                {
                    @object = Geometry.Rhino.Convert.ToRhino((Mesh3D)@object);
                }

                if (@object is global::Rhino.Geometry.Brep)
                {
                    breps.Add((global::Rhino.Geometry.Brep)@object);
                    continue;
                }
               
                if(@object is global::Rhino.Geometry.Mesh)
                {
                    global::Rhino.Geometry.Mesh mesh = (global::Rhino.Geometry.Mesh)@object;
                    if(mesh.IsClosed)
                    {
                        global::Rhino.Geometry.Brep brep = global::Rhino.Geometry.Brep.CreateFromMesh(mesh, true);
                        if(brep != null)
                        {
                            breps.Add(brep);
                            continue;
                        }
                    }
                }

                if (@object is Shell)
                {
                    breps.Add(Geometry.Rhino.Convert.ToRhino((Shell)@object));
                    continue;
                }
            }


            List<Shell> shells = new List<Shell>();
            foreach (global::Rhino.Geometry.Brep brep in breps)
            {
                Shell shell = Geometry.Rhino.Convert.ToSAM_Shell(brep);
                if(shell != null)
                {
                    shells.Add(shell);
                }
            }

            index = Params.IndexOfInputParam("elevationGround_");
            double elevationGround = 0;
            if (index != -1)
                dataAccess.GetData(index, ref elevationGround);

            index = Params.IndexOfInputParam("maxDistance_");
            double maxDistance = 0.01;
            if (index != -1)
                dataAccess.GetData(index, ref maxDistance);

            index = Params.IndexOfInputParam("maxAngle_");
            double maxAngle = 0.0872664626;
            if (index != -1)
                dataAccess.GetData(index, ref maxAngle);

            index = Params.IndexOfInputParam("addMissingSpaces_");
            bool addMissingSpaces = true;
            if (index != -1)
                dataAccess.GetData(index, ref addMissingSpaces);

            index = Params.IndexOfInputParam("addMissingPanels_");
            bool addMissingPanels = true;
            if (index != -1)
                dataAccess.GetData(index, ref addMissingPanels);

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

            AdjacencyCluster adjacencyCluster = Create.AdjacencyCluster(shells, elevationGround, 0.01, minArea, maxDistance, maxAngle, silverSpacing, tolerance, Core.Tolerance.Angle);

            index = Params.IndexOfOutputParam("AdjacencyCluster");
            if (index != -1)
                dataAccess.SetData(index, new GooAdjacencyCluster(adjacencyCluster));
        }
    }
}