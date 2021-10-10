using Grasshopper.Kernel;
using SAM.Core.Grasshopper;
using SAM.Geometry.Grasshopper.Properties;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;
using Grasshopper.Kernel.Types;
using System.Linq;

namespace SAM.Geometry.Grasshopper
{
    public class SAMGeometryCreateShellsByElevations : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("6b077a21-ef00-425c-8e8c-c4755aa9d911");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMGeometryCreateShellsByElevations()
          : base("SAMGeometry.CreateShellsByElevations", "SAMGeometry.CreateShellsByElevations",
              "Create Shells ",
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

                gerenricObject = new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_face3DObjects", NickName = "_face3DObjects", Description = "SAM Geometry Face3D Objects", Access = GH_ParamAccess.list };
                gerenricObject.DataMapping = GH_DataMapping.Flatten;
                result.Add(new GH_SAMParam(gerenricObject, ParamVisibility.Binding));


                global::Grasshopper.Kernel.Parameters.Param_Number number = null;

                number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "elevations_", NickName = "elevations_", Description = "Elevations", Access = GH_ParamAccess.list, Optional = true };
                result.Add(new GH_SAMParam(number, ParamVisibility.Binding));

                number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "offset_", NickName = "offset_", Description = "Offset", Access = GH_ParamAccess.item, Optional = true };
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
                result.Add(new GH_SAMParam(new GooSAMGeometryParam() { Name = "shells", NickName = "shells", Description = "SAM Geometry Shells", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
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

            index = Params.IndexOfInputParam("_face3DObjects");
            List<GH_ObjectWrapper> objectWrappers = new List<GH_ObjectWrapper>();
            if (index == -1 || !dataAccess.GetDataList(index, objectWrappers) || objectWrappers == null || objectWrappers.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Face3D> face3Ds = new List<Face3D>();
            foreach (GH_ObjectWrapper objectWrapper_Temp in objectWrappers)
            {
                if (Query.TryGetSAMGeometries(objectWrapper_Temp, out List<Face3D> face3Ds_Temp) && face3Ds_Temp != null && face3Ds_Temp.Count > 0)
                    face3Ds.AddRange(face3Ds_Temp);
            }

            if (face3Ds.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<double> elevations = new List<double>();
            index = Params.IndexOfInputParam("elevations_");
            if (index != -1)
            {
                dataAccess.GetDataList(index, elevations);
            }

            double offset = 0.1;
            index = Params.IndexOfInputParam("offset_");
            if (index != -1)
            {
                dataAccess.GetData(index, ref offset);
            }

            index = Params.IndexOfInputParam("tolerance_");
            double tolerance = Core.Tolerance.Distance;
            if (index != -1)
            {
                double tolerance_Temp = double.NaN;
                if (!dataAccess.GetData(index, ref tolerance_Temp) && !double.IsNaN(tolerance_Temp))
                    tolerance = tolerance_Temp;
            }

            if(elevations == null || elevations.Count == 0)
            {
                List<Face3D> face3Ds_Horizontal = face3Ds.FindAll(x => Spatial.Query.Horizontal(x, tolerance));
                
                Dictionary<double, List<Face3D>> elevationDictionary = face3Ds_Horizontal.ElevationDictionary(tolerance);
                elevations = elevationDictionary.Keys.ToList();
            }
            List<Shell> shells = Spatial.Create.Shells(face3Ds, elevations, offset, 0.01, Core.Tolerance.MacroDistance, Core.Tolerance.MacroDistance, Core.Tolerance.MacroDistance, Core.Tolerance.Angle, tolerance);

            index = Params.IndexOfOutputParam("Shells");
            if (index != -1)
                dataAccess.SetDataList(index, shells?.ConvertAll(x => new GooSAMGeometry(x)));
        }
    }
}