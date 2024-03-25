using Grasshopper.Kernel;
using Rhino.Geometry;
using SAM.Core.Grasshopper;
using SAM.Geometry.Object;
using SAM.Geometry.Object.Planar;
using SAM.Geometry.Planar;
using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Geometry.Grasshopper
{
    /// <summary>
    /// Provides methods to modify geometric objects.
    /// </summary>
    public static partial class Modify
    {
        /// <summary>
        /// Draws viewport wires for the specified SAM geometry object.
        /// </summary>
        /// <param name="sAMGeomery">The SAM geometry to draw.</param>
        /// <param name="previewWireArgs">The arguments for the preview wire.</param>
        /// <param name="color">The color of the wire.</param>
        public static void DrawViewportWires(this ISAMGeometry sAMGeomery, GH_PreviewWireArgs previewWireArgs, System.Drawing.Color color)
        {
            if (sAMGeomery == null || previewWireArgs == null)
            {
                return;
            }

            //TODO: Display Spatial.Surface as Rhino.Geometry.Surface

            List<ICurve3D> curve3Ds = null;
            if (sAMGeomery is Face3D)
            {
                curve3Ds = new List<ICurve3D>();

                foreach (IClosedPlanar3D closedPlanar3D in ((Face3D)sAMGeomery).GetEdge3Ds())
                    if (closedPlanar3D is ICurvable3D)
                        curve3Ds.AddRange(((ICurvable3D)closedPlanar3D).GetCurves());
            }
            else if (sAMGeomery is Planar.Face2D)
            {
                Face3D face3D = Spatial.Query.Convert(Spatial.Plane.WorldXY, (Planar.Face2D)sAMGeomery);
                if(face3D != null)
                {
                    curve3Ds = new List<ICurve3D>();

                    foreach (IClosedPlanar3D closedPlanar3D in face3D.GetEdge3Ds())
                        if (closedPlanar3D is ICurvable3D)
                            curve3Ds.AddRange(((ICurvable3D)closedPlanar3D).GetCurves());
                }
            }
            else if (sAMGeomery is ICurvable3D)
            {
                curve3Ds = ((ICurvable3D)sAMGeomery).GetCurves();
            }
            else if (sAMGeomery is Planar.ICurvable2D)
            {
                curve3Ds = ((Planar.ICurvable2D)sAMGeomery).GetCurves().ConvertAll(x => Spatial.Query.Convert(Spatial.Plane.WorldXY, x));
            }

            if (curve3Ds != null && curve3Ds.Count > 0)
            {
                curve3Ds.ForEach(x => previewWireArgs.Pipeline.DrawCurve(Rhino.Convert.ToRhino(x), color));
                //return;
            }

            if (sAMGeomery is Point3D)
            {
                previewWireArgs.Pipeline.DrawPoint(Rhino.Convert.ToRhino((sAMGeomery as Point3D)), color);
                return;
            }

            if (sAMGeomery is Point2D)
            {
                previewWireArgs.Pipeline.DrawPoint(Rhino.Convert.ToRhino((sAMGeomery as Point2D)), color);
                return;
            }

            if (sAMGeomery is Face3D)
            {
                Brep brep = Rhino.Convert.ToRhino_Brep((sAMGeomery as Face3D));
                if (brep != null)
                {
                    previewWireArgs.Pipeline.DrawBrepWires(brep, color);
                }
                return;
            }

            if (sAMGeomery is Shell)
            {
                List<Brep> breps = ((Shell)sAMGeomery).Face3Ds?.ConvertAll(x => Rhino.Convert.ToRhino_Brep(x));
                breps?.FindAll(x => x != null).ForEach(x => previewWireArgs.Pipeline.DrawBrepWires(x, color));
                return;
            }

            if (sAMGeomery is Mesh3D)
            {
                Mesh mesh = Rhino.Convert.ToRhino(((Mesh3D)sAMGeomery));
                if(mesh != null)
                {
                    previewWireArgs.Pipeline.DrawMeshWires(mesh, color);
                }

                return;
            }

            if(sAMGeomery is SAMGeometry2DGroup)
            {
                foreach(ISAMGeometry sAMGeometry in (SAMGeometry2DGroup)sAMGeomery)
                {
                    DrawViewportWires(sAMGeometry, previewWireArgs, color);
                }

                return;
            }

            if (sAMGeomery is SAMGeometry3DGroup)
            {
                foreach (ISAMGeometry sAMGeometry in (SAMGeometry3DGroup)sAMGeomery)
                {
                    DrawViewportWires(sAMGeometry, previewWireArgs, color);
                }

                return;
            }
        }

        public static void DrawViewportWires(this ISAMGeometryObject sAMGeometryObject, GH_PreviewWireArgs previewWireArgs, System.Drawing.Color color)
        {
            if (sAMGeometryObject == null || previewWireArgs == null)
            {
                return;
            }

            List<ISAMGeometry> sAMGeometries = Object.Convert.ToSAM_ISAMGeometry(sAMGeometryObject);
            if(sAMGeometries  == null)
            {
                return;
            }

            foreach(ISAMGeometry sAMGeometry in sAMGeometries)
            {
                DrawViewportWires(sAMGeometry, previewWireArgs, color);
            }
        }

        /// <summary>
        /// Draws viewport wires for the specified Face3D object.
        /// </summary>
        /// <param name="face3D">The Face3D object to draw.</param>
        /// <param name="previewWireArgs">The arguments for the preview wire.</param>
        /// <param name="color_ExternalEdges">The color of the external edges.</param>
        /// <param name="color_InternalEdges">The color of the internal edges.</param>
        public static void DrawViewportWires(this Face3D face3D, GH_PreviewWireArgs previewWireArgs, System.Drawing.Color color_ExternalEdges, System.Drawing.Color color_InternalEdges)
        {
            if (face3D == null || previewWireArgs == null)
            {
                return;
            }

            IClosedPlanar3D externalEdge3D = face3D.GetExternalEdge3D();
            if(externalEdge3D != null)
            {
                DrawViewportWires(externalEdge3D, previewWireArgs, color_ExternalEdges);
            }

            List<IClosedPlanar3D> internalEdge3Ds = face3D.GetInternalEdge3Ds();
            if(internalEdge3Ds != null && internalEdge3Ds.Count != 0)
            {
                foreach(IClosedPlanar3D internalEdge3D in internalEdge3Ds)
                {
                    DrawViewportWires(internalEdge3D, previewWireArgs, color_InternalEdges);
                }
            }

        }
    }
}