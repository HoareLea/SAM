using Grasshopper.Kernel;
using SAM.Core.Grasshopper;
using SAM.Geometry.Grasshopper.Properties;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;
using Grasshopper.Kernel.Types;

namespace SAM.Geometry.Grasshopper
{
    public class SAMGeometryThinnessRatio : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("37139b71-48d1-4851-a4bf-8167cd424d5f");

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
        public SAMGeometryThinnessRatio()
          : base("SAMGeometry.ThinnessRatio", "SAMGeometry.ThinnessRatio",
              "Calculate Thinness Ratio",
              "SAM WIP", "Geometry")
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

                gerenricObject = new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_face3D", NickName = "_face3D", Description = "SAM Geometry Face3D Objects", Access = GH_ParamAccess.item };
                gerenricObject.DataMapping = GH_DataMapping.Flatten;
                result.Add(new GH_SAMParam(gerenricObject, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "thinnessRatio", NickName = "thinnessRatio", Description = "thinnessRatio", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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

            index = Params.IndexOfInputParam("_face3D");
            GH_ObjectWrapper objectWrapper = null;
            if (index == -1 || !dataAccess.GetData(index, ref objectWrapper) || objectWrapper == null )
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Face3D> face3Ds = new List<Face3D>();
            if (Query.TryGetSAMGeometries(objectWrapper, out List<Face3D> face3Ds_Temp) && face3Ds_Temp != null && face3Ds_Temp.Count > 0)
            {
                face3Ds.AddRange(face3Ds_Temp);
            }

            if (face3Ds.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfOutputParam("thinnessRatio");
            if (index != -1)
            {
                dataAccess.SetData(index, face3Ds[0].ThinnessRatio());
            }                
        }
    }
}