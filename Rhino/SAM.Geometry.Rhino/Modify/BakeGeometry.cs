using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;

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

            if (sAMGeometry is Point3D)
            {
                guid = rhinoDoc.Objects.AddPoint(new Point(Convert.ToRhino((Point3D)sAMGeometry)), objectAttributes, null, false);
                return true;
            }

            if (sAMGeometry is ICurve3D)
            {
                guid = rhinoDoc.Objects.AddCurve(Convert.ToRhino((ICurve3D)sAMGeometry), objectAttributes);
                return true;
            }

            if (sAMGeometry is Face3D)
            {
                guid = rhinoDoc.Objects.AddBrep(Convert.ToRhino_Brep((Face3D)sAMGeometry), objectAttributes);
                return true;
            }

            if (sAMGeometry is Shell)
            {
                guid = rhinoDoc.Objects.AddBrep(Convert.ToRhino((Shell)sAMGeometry), objectAttributes);
                return true;
            }

            if (sAMGeometry is Polygon3D)
            {
                guid = rhinoDoc.Objects.AddCurve(Convert.ToRhino_PolylineCurve((Polygon3D)sAMGeometry), objectAttributes);
                return true;
            }

            if (sAMGeometry is Triangle3D)
            {
                guid = rhinoDoc.Objects.AddCurve(Convert.ToRhino_PolylineCurve((Triangle3D)sAMGeometry), objectAttributes);
                return true;
            }

            if (sAMGeometry is Mesh3D)
            {
                guid = rhinoDoc.Objects.AddMesh(Convert.ToRhino(((Mesh3D)sAMGeometry)), objectAttributes);
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

        public static bool BakeGeometry(this IEnumerable<Face3D> face3Ds, RhinoDoc rhinoDoc, ObjectAttributes objectAttributes, out Guid guid)
        {
            guid = Guid.Empty;

            if (face3Ds == null)
            {
                return false;
            }

            guid = rhinoDoc.Objects.AddBrep(Convert.ToRhino(face3Ds), objectAttributes);
            return true;
        }
    }
}