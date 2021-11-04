using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Collections.Generic;

namespace SAM.Geometry.Grasshopper
{
    public static partial class Modify
    {
        public static void DrawViewportWires(this ISAMGeometry sAMGeomery, GH_PreviewWireArgs previewWireArgs, System.Drawing.Color color)
        {
            if(sAMGeomery == null || previewWireArgs == null)
            {
                return;
            }

            //TODO: Display Spatial.Surface as Rhino.Geometry.Surface

            List<Spatial.ICurve3D> curve3Ds = null;
            if (sAMGeomery is Spatial.Face3D)
            {
                curve3Ds = new List<Spatial.ICurve3D>();

                foreach (Spatial.IClosedPlanar3D closedPlanar3D in ((Spatial.Face3D)sAMGeomery).GetEdge3Ds())
                    if (closedPlanar3D is Spatial.ICurvable3D)
                        curve3Ds.AddRange(((Spatial.ICurvable3D)closedPlanar3D).GetCurves());
            }
            else if (sAMGeomery is Spatial.ICurvable3D)
            {
                curve3Ds = ((Spatial.ICurvable3D)sAMGeomery).GetCurves();
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

            if (sAMGeomery is Spatial.Point3D)
            {
                previewWireArgs.Pipeline.DrawPoint(Rhino.Convert.ToRhino((sAMGeomery as Spatial.Point3D)), color);
                return;
            }

            if (sAMGeomery is Planar.Point2D)
            {
                previewWireArgs.Pipeline.DrawPoint(Rhino.Convert.ToRhino((sAMGeomery as Planar.Point2D)), color);
                return;
            }

            if (sAMGeomery is Spatial.Face3D)
            {
                Brep brep = Rhino.Convert.ToRhino_Brep((sAMGeomery as Spatial.Face3D));
                if (brep != null)
                {
                    previewWireArgs.Pipeline.DrawBrepWires(brep, color);
                }
                return;
            }

            if (sAMGeomery is Spatial.Shell)
            {
                List<Brep> breps = ((Spatial.Shell)sAMGeomery).Face3Ds?.ConvertAll(x => Rhino.Convert.ToRhino_Brep(x));
                breps?.FindAll(x => x != null).ForEach(x => previewWireArgs.Pipeline.DrawBrepWires(x, color));
                return;
            }

            if (sAMGeomery is Spatial.Mesh3D)
            {
                Mesh mesh = Rhino.Convert.ToRhino(((Spatial.Mesh3D)sAMGeomery));
                if(mesh != null)
                {
                    previewWireArgs.Pipeline.DrawMeshWires(mesh, color);
                }

                return;
            }
        }

        public static void DrawViewportWires(this Spatial.Face3D face3D, GH_PreviewWireArgs previewWireArgs, System.Drawing.Color color_ExternalEdges, System.Drawing.Color color_InternalEdges)
        {
            if(face3D == null || previewWireArgs == null)
            {
                return;
            }

            Spatial.IClosedPlanar3D externalEdge3D = face3D.GetExternalEdge3D();
            if(externalEdge3D != null)
            {
                DrawViewportWires(externalEdge3D, previewWireArgs, color_ExternalEdges);
            }

            List<Spatial.IClosedPlanar3D> internalEdge3Ds = face3D.GetInternalEdge3Ds();
            if(internalEdge3Ds != null && internalEdge3Ds.Count != 0)
            {
                foreach(Spatial.IClosedPlanar3D internalEdge3D in internalEdge3Ds)
                {
                    DrawViewportWires(internalEdge3D, previewWireArgs, color_InternalEdges);
                }
            }

        }
    }
}