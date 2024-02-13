using Grasshopper.Kernel;
using SAM.Core.Grasshopper;
using SAM.Geometry.Grasshopper.Properties;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;
using Grasshopper.Kernel.Types;

namespace SAM.Geometry.Grasshopper
{
    public class SAMGeometrySectionByShell : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("c8d8304b-042d-4517-a3d4-ce92143b0068");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Geometry;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMGeometrySectionByShell()
          : base("SAMGeometry.SectionByShell", "SAMGeometry.SectionByShell",
              "Create Section ",
              "SAM", "Geometry")
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

                global::Grasshopper.Kernel.Parameters.Param_GenericObject gerenricObject;

                gerenricObject = new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_shell", NickName = "_shell", Description = "SAM Geometry Shell Objects", Access = GH_ParamAccess.item };
                gerenricObject.DataMapping = GH_DataMapping.Flatten;
                result.Add(new GH_SAMParam(gerenricObject, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Plane() { Name = "plane_", NickName = "plane_", Description = "Plane", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Voluntary));

                global::Grasshopper.Kernel.Parameters.Param_Number number = null;

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
                result.Add(new GH_SAMParam(new GooSAMGeometryParam() { Name = "geometries", NickName = "geometries", Description = "SAM Geometries", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
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

            index = Params.IndexOfInputParam("_shell");
            GH_ObjectWrapper objectWrapper = null;
            if (index == -1 || !dataAccess.GetData(index, ref objectWrapper) || objectWrapper == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            if (!Query.TryGetSAMGeometries(objectWrapper, out List<Shell> shells) || shells == null || shells.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            Plane plane = null;
            index = Params.IndexOfInputParam("plane_");
            if (index != -1 && dataAccess.GetData(index, ref objectWrapper) && objectWrapper != null)
            {
                if (Query.TryGetSAMGeometries(objectWrapper, out List<Plane> planes) && planes != null & planes.Count != 0)
                {
                    plane = planes[0];
                }
            }

            if(plane == null)
            {

            }

            List<Face3D> face3Ds = new List<Face3D>();
            foreach (Shell shell in shells)
            {
                if(shell == null)
                {
                    continue;
                }

                Plane plane_Temp = plane;
                if(plane_Temp == null)
                {
                    plane_Temp = new Plane(shell.GetBoundingBox().GetCentroid(), Vector3D.WorldZ);
                }

                if(plane_Temp == null)
                {
                    continue;
                }

                List<Face3D> face3Ds_Section = shell.Section(plane_Temp);
                if(face3Ds_Section == null || face3Ds_Section.Count == 0)
                {
                    continue;
                }
                face3Ds.AddRange(face3Ds_Section);
            }

            index = Params.IndexOfOutputParam("geometries");
            if (index != -1)
            {
                dataAccess.SetDataList(index, face3Ds?.ConvertAll(x => new GooSAMGeometry(x)));
            }                
        }
    }
}