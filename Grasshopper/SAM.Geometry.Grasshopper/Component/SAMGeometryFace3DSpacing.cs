using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using SAM.Geometry.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel.Types;

namespace SAM.Analytical.Grasshopper
{
    public class SAMGeometryFace3DSpacing : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("42fb6db1-fe5b-40b6-8208-1033ca02a83b");

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
        public SAMGeometryFace3DSpacing()
          : base("SAMGeometry.Face3DSpacing", "SAMGeometry.Face3DSpacing",
              "Calculates Spacing between Face3Ds",
              "SAM", "Geometry")
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.primary;

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override GH_SAMParam[] Inputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_face3Ds", NickName = "_face3Ds", Description = "SAM Spatial Face3Ds", Access = GH_ParamAccess.list }, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Number number;

                number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_max_", NickName = "_max_", Description = "Maximal distance to be checked", Access = GH_ParamAccess.item, Optional = true };
                number.SetPersistentData(Tolerance.MacroDistance);
                result.Add(new GH_SAMParam(number, ParamVisibility.Voluntary));

                number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_min_", NickName = "_min_", Description = "Minimal distance to be checked", Access = GH_ParamAccess.item, Optional = true };
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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Point() { Name = "points", NickName = "points", Description = "Points", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "face3Ds", NickName = "face3Ds", Description = "SAM Spatial Face3Ds", Access = GH_ParamAccess.tree }, ParamVisibility.Voluntary));
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

            index = Params.IndexOfInputParam("_face3Ds");
            List<GH_ObjectWrapper> objectWrappers = new List<GH_ObjectWrapper>();
            if (index == -1 || !dataAccess.GetDataList(index, objectWrappers))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Face3D> face3Ds = new List<Face3D>();
            foreach(GH_ObjectWrapper objectWrapper in objectWrappers)
            {
                if(Geometry.Grasshopper.Query.TryGetSAMGeometries(objectWrapper, out List<Face3D> face3Ds_Temp))
                {
                    face3Ds.AddRange(face3Ds_Temp);
                }
            }

            if(face3Ds == null || face3Ds.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_max_");
            double max = Tolerance.MacroDistance;
            if (index != -1)
            {
                dataAccess.GetData(index, ref max);
            }

            index = Params.IndexOfInputParam("_min_");
            double min = Tolerance.Distance;
            if (index != -1)
            {
                dataAccess.GetData(index, ref min);
            }

            Dictionary<Point3D, List<Face3D>> dictionary = Geometry.Spatial.Query.SpacingDictionary(face3Ds, max, min);

            index = Params.IndexOfOutputParam("points");
            if(index != -1)
            {
                dataAccess.SetDataList(index, dictionary?.Keys.ToList().ConvertAll(x => Geometry.Rhino.Convert.ToRhino(x)));
            }

            index = Params.IndexOfOutputParam("face3Ds");
            if(index != -1)
            {
                DataTree<object> dataTree_Objects = new DataTree<object>();

                int count = 0;
                foreach (KeyValuePair<Point3D, List<Face3D>> keyValuePair in dictionary)
                {
                    GH_Path path = new GH_Path(count);
                    keyValuePair.Value?.ForEach(x => dataTree_Objects.Add(x, path));
                    count++;
                }

                dataAccess.SetDataTree(index, dataTree_Objects);
            }
        }
    }
}