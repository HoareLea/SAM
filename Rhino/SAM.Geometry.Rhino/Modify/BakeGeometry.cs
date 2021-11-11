using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;
using System;

namespace SAM.Geometry.Rhino
{
    public static partial class Modify
    {
        public static bool BakeGeometry(this ISAMGeometry sAMGeometry, RhinoDoc rhinoDoc, ObjectAttributes objectAttributes, out Guid guid)
        {
            guid = Guid.Empty;

            if (sAMGeometry == null || rhinoDoc == null || objectAttributes == null)
                return false;

            if (sAMGeometry is Planar.Point2D)
            {
                guid = rhinoDoc.Objects.AddPoint(new Point(Convert.ToRhino((Planar.Point2D)sAMGeometry)), objectAttributes, null, false);
                return true;
            }

            if (sAMGeometry is Planar.ISAMGeometry2D)
            {
                sAMGeometry = Spatial.Query.Convert(Spatial.Plane.WorldXY, sAMGeometry as dynamic);
            }

            if (sAMGeometry is Spatial.Point3D)
            {
                guid = rhinoDoc.Objects.AddPoint(new Point(Convert.ToRhino((Spatial.Point3D)sAMGeometry)), objectAttributes, null, false);
                return true;
            }

            if (sAMGeometry is Spatial.ICurve3D)
            {
                guid = rhinoDoc.Objects.AddCurve(Convert.ToRhino((Spatial.ICurve3D)sAMGeometry), objectAttributes);
                return true;
            }

            if (sAMGeometry is Spatial.Face3D)
            {
                guid = rhinoDoc.Objects.AddBrep(Convert.ToRhino_Brep((Spatial.Face3D)sAMGeometry), objectAttributes);
                return true;
            }

            if (sAMGeometry is Spatial.Shell)
            {
                guid = rhinoDoc.Objects.AddBrep(Convert.ToRhino((Spatial.Shell)sAMGeometry), objectAttributes);
                return true;
            }

            if (sAMGeometry is Spatial.Polygon3D)
            {
                guid = rhinoDoc.Objects.AddCurve(Convert.ToRhino_PolylineCurve((Spatial.Polygon3D)sAMGeometry), objectAttributes);
                return true;
            }

            if (sAMGeometry is Spatial.Triangle3D)
            {
                guid = rhinoDoc.Objects.AddCurve(Convert.ToRhino_PolylineCurve((Spatial.Triangle3D)sAMGeometry), objectAttributes);
                return true;
            }

            if (sAMGeometry is Spatial.Mesh3D)
            {
                guid = rhinoDoc.Objects.AddMesh(Convert.ToRhino(((Spatial.Mesh3D)sAMGeometry)), objectAttributes);
                return true;
            }

            GeometryBase geometryBase = (sAMGeometry as dynamic).ToRhino() as GeometryBase;
            if (geometryBase != null)
            {
                guid = rhinoDoc.Objects.Add(geometryBase, objectAttributes);
                return true;
            }

            return false;
        }
    }
}