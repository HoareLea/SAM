using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Core.Grasshopper;
using SAM.Geometry.Grasshopper.Properties;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;

namespace SAM.Geometry.Grasshopper
{
    public class SAMGeometrySplitCoplanarFace3Ds : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("a483b16a-6d9f-4ae1-920f-2aaa60114d32");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.1";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Geometry;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMGeometrySplitCoplanarFace3Ds()
          : base("SAMGeometry.SplitCoplanarFace3Ds", "SAMGeometry.SplitCoplanarFace3Ds",
              "Split Shells Coplanar Face3Ds",
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

                global::Grasshopper.Kernel.Parameters.Param_GenericObject gerenricObject = new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_shells", NickName = "_shells", Description = "SAM Geometry Shells", Access = GH_ParamAccess.list };
                gerenricObject.DataMapping = GH_DataMapping.Flatten;
                result.Add(new GH_SAMParam(gerenricObject, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Number number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "angleTolerance_", NickName = "angleTolerance_", Description = "Angle Tolerance", Access = GH_ParamAccess.item };
                number.SetPersistentData(Core.Tolerance.Angle);
                result.Add(new GH_SAMParam(number, ParamVisibility.Voluntary));

                number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "distanceTolerance_", NickName = "distanceTolerance_", Description = "Distance Tolerance", Access = GH_ParamAccess.item };
                number.SetPersistentData(Core.Tolerance.Distance);
                result.Add(new GH_SAMParam(number, ParamVisibility.Voluntary));

                global::Grasshopper.Kernel.Parameters.Param_Boolean boolean = new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "_run", NickName = "_run", Description = "Run", Access = GH_ParamAccess.item };
                result.Add(new GH_SAMParam(boolean, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new GooSAMGeometryParam() { Name = "shells", NickName = "shells", Description = "SAM Geometry Shells", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "updated", NickName = "updated", Description = "Updated", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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

            int index_Updated = Params.IndexOfOutputParam("updated");
            if (index_Updated != -1)
                dataAccess.SetData(index_Updated, false);

            bool run = false;
            index = Params.IndexOfInputParam("_run");
            if (!dataAccess.GetData(index, ref run))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            if (!run)
                return;

            index = Params.IndexOfInputParam("_shells");
            List<GH_ObjectWrapper> objectWrappers = new List<GH_ObjectWrapper>();
            if (index == -1 || !dataAccess.GetDataList(index, objectWrappers) || objectWrappers == null || objectWrappers.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Shell> shells = new List<Shell>();
            foreach(GH_ObjectWrapper objectWrapper in objectWrappers)
            {
                if(Query.TryGetSAMGeometries(objectWrapper, out List<Shell> shells_Temp) && shells_Temp != null)
                {
                    shells.AddRange(shells_Temp);
                }
            }

            double tolerance_Angle = Core.Tolerance.Angle;
            index = Params.IndexOfInputParam("angleTolerance_");
            if(index != -1)
            {
                dataAccess.GetData(index, ref tolerance_Angle);
            }

            double tolerance_Distance = Core.Tolerance.Distance;
            index = Params.IndexOfInputParam("distanceTolerance_");
            if (index != -1)
            {
                dataAccess.GetData(index, ref tolerance_Distance);
            }

            shells = shells.ConvertAll(x => new Shell(x));

            bool result = shells.SplitCoplanarFace3Ds(tolerance_Angle, tolerance_Distance);

            index = Params.IndexOfOutputParam("shells");
            if (index != -1)
                dataAccess.SetDataList(index, shells?.ConvertAll(x => new GooSAMGeometry(x)));

            if(index_Updated != -1)
                dataAccess.SetData(index_Updated, result);
        }
    }
}