using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;
using System.Security;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalCreateAdjacencyClusterByBrepsAndNames : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("c51fc7c4-5552-4f87-9a2b-90b45e4e9e4c");

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
        public SAMAnalyticalCreateAdjacencyClusterByBrepsAndNames()
          : base("SAMAnalytical.CreateAdjacencyClusterByBrepsAndNames", "SAMAnalytical.CreateAdjacencyClusterByBrepsAndNames",
              "Create AdjacencyCluster from Breps or SAM Shells",
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

                global::Grasshopper.Kernel.Parameters.Param_GenericObject genericObject = new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_breps", NickName = "_breps", Description = "Rhino Breps or SAM Shells", Access = GH_ParamAccess.list };
                genericObject.DataMapping = GH_DataMapping.Flatten;
                result.Add(new GH_SAMParam(genericObject, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_String paramString = new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_names", NickName = "_names", Description = "Names", Access = GH_ParamAccess.list };
                result.Add(new GH_SAMParam(paramString, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Number paramNumber;

                paramNumber = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "elevationGround_", NickName = "elevationGround_", Description = "Ground Elevation", Access = GH_ParamAccess.item };
                paramNumber.SetPersistentData(0);
                result.Add(new GH_SAMParam(paramNumber, ParamVisibility.Voluntary));

                paramNumber = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "silverSpacing_", NickName = "silverSpacing_", Description = "Silver Spacing for computation of Spaces", Access = GH_ParamAccess.item };
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
                result.Add(new GH_SAMParam(new GooAdjacencyClusterParam() { Name = "adjacencyCluster", NickName = "adjacencyCluster", Description = "SAM Analytical AdjacencyCluster", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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
                if(@object is global::Rhino.Geometry.Extrusion)
                {
                    global::Rhino.Geometry.Extrusion extrusion = (global::Rhino.Geometry.Extrusion)@object;
                    breps.Add(extrusion.ToBrep(true));
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

            List<string> names = new List<string>();
            index = Params.IndexOfInputParam("_names");
            if(index != -1)
            {
                dataAccess.GetDataList(index, names);
            }

            int count = 1;
            for (int i = 0; i < shells.Count; i++)
            {
                if(names.Count < i)
                {
                    while (names.Find(x => x == string.Format("{0} {1}", "Space", count)) != null)
                    {
                        count++;
                    }

                    names.Add(string.Format("{0} {1}", "Space", count));
                }
            }

            index = Params.IndexOfInputParam("elevationGround_");
            double elevationGround = 0;
            if (index != -1)
            {
                dataAccess.GetData(index, ref elevationGround);
            }

            index = Params.IndexOfInputParam("silverSpacing_");
            double silverSpacing = Core.Tolerance.MacroDistance;
            if (index != -1)
            {
                dataAccess.GetData(index, ref silverSpacing);
            }

            index = Params.IndexOfInputParam("tolerance_");
            double tolerance = Core.Tolerance.Distance;
            if (index != -1)
            {
                dataAccess.GetData(index, ref tolerance);
            }

            List<Space> spaces = new List<Space>();
            for (int i = 0; i < shells.Count; i++)
            {
                spaces.Add(new Space(names[i], shells[i].InternalPoint3D(silverSpacing, tolerance)));
            }

            AdjacencyCluster adjacencyCluster = Create.AdjacencyCluster(shells, spaces, elevationGround, silverSpacing, Core.Tolerance.Angle, tolerance);

            index = Params.IndexOfOutputParam("AdjacencyCluster");
            if (index != -1)
            {
                dataAccess.SetData(index, new GooAdjacencyCluster(adjacencyCluster));
            }
        }
    }
}