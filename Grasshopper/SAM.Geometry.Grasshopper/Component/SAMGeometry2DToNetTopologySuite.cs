using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

using SAM.Geometry.Grasshopper.Properties;
using SAM.Geometry.Planar;

namespace SAM.Geometry.Grasshopper
{
    public class SAMGeometry2DToNetTopologySuite : GH_Component
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("0919dca3-ca2b-4783-acd3-847d5bff4a46");

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Geometry;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMGeometry2DToNetTopologySuite()
          : base("SAMGeometry.Snap", "SAMGeometry.Snap",
              "SAMGeometry To NetTopologySuite",
              "SAM", "Geometry")
        {

        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddGenericParameter("_geometry", "Geo", "Geometry", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddGenericParameter("NTSGeometries", "Geo", "modified SAM Geometry", GH_ParamAccess.list);
            outputParamManager.AddGenericParameter("Text", "Text", "Text", GH_ParamAccess.list);
            outputParamManager.AddBooleanParameter("Successful", "Successful", "Correctly imported?", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            GH_ObjectWrapper objectWrapper = null;
            if (!dataAccess.GetData(0, ref objectWrapper) || objectWrapper == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                dataAccess.SetData(1, false);
                return;
            }

            List<ISAMGeometry2D> sAMGeometry2Ds = new List<ISAMGeometry2D>();

            List<Spatial.ISAMGeometry3D> sAMGeometry3Ds = null;
            if (objectWrapper.Value is GooSAMGeometry)
            {
                sAMGeometry3Ds = new List<Spatial.ISAMGeometry3D>();

                ISAMGeometry sAMGeometry = ((GooSAMGeometry)objectWrapper.Value).Value;
                if (sAMGeometry is Spatial.ISAMGeometry3D)
                    sAMGeometry3Ds.Add((Spatial.ISAMGeometry3D)sAMGeometry);
                else if (sAMGeometry is Planar.ISAMGeometry2D)
                    sAMGeometry3Ds.Add(Spatial.Plane.Base.Convert((Planar.ISAMGeometry2D)sAMGeometry));
            }
            else if (objectWrapper.Value is IGH_GeometricGoo)
            {
                sAMGeometry3Ds = Convert.ToSAM((IGH_GeometricGoo)objectWrapper.Value);
            }

            if (sAMGeometry3Ds != null && sAMGeometry3Ds.Count > 0)
            {
                foreach (Spatial.ISAMGeometry3D sAMGeometry3D in sAMGeometry3Ds)
                {
                    List<Spatial.ICurve3D> curve3Ds = new List<Spatial.ICurve3D>();
                    if (sAMGeometry3D is Spatial.ICurvable3D)
                        curve3Ds.AddRange(((Spatial.ICurvable3D)sAMGeometry3D).GetCurves().ConvertAll(x => Spatial.Plane.Base.Project(x)));
                    else if (sAMGeometry3D is Spatial.ICurve3D)
                        curve3Ds.Add(Spatial.Plane.Base.Project((Spatial.ICurve3D)sAMGeometry3D));

                    if (curve3Ds == null || curve3Ds.Count == 0)
                        continue;

                    foreach (Spatial.ICurve3D curve3D in curve3Ds)
                    {
                        if (curve3D == null)
                            continue;

                        ICurvable2D curvable2D = Spatial.Plane.Base.Convert(curve3D) as ICurvable2D;
                        if (curvable2D != null)
                            sAMGeometry2Ds.Add(curvable2D);
                    }
                }
            }

            List<NetTopologySuite.Geometries.Geometry> geometries = sAMGeometry2Ds.ConvertAll(x => x.ToNetTopologySuite());

            dataAccess.SetData(0, geometries);
            dataAccess.SetData(1, geometries.ConvertAll(x => x.ToString()));
            dataAccess.SetData(2, true);

            //AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Cannot split segments");
        }
    }
}