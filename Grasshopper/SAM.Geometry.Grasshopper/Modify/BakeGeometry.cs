using System;

using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;

namespace SAM.Geometry.Grasshopper
{
    public static partial class Modify
    {
        public static bool BakeGeometry(this ISAMGeometry sAMGeometry, RhinoDoc rhinoDoc, ObjectAttributes objectAttributes, out Guid guid)
        {
            guid = Guid.Empty;

            if (sAMGeometry == null || rhinoDoc == null || objectAttributes == null)
                return false;
            
            if(sAMGeometry is Spatial.Point3D)
            {
                guid = rhinoDoc.Objects.AddPoint(((Spatial.Point3D)sAMGeometry).ToRhino());
                return true;
            }

            if(sAMGeometry is Spatial.ICurve3D)
            {
                guid = rhinoDoc.Objects.AddCurve(((Spatial.ICurve3D)sAMGeometry).ToRhino());
                return true;
            }

            if(sAMGeometry is Spatial.Face3D)
            {
                guid = rhinoDoc.Objects.AddBrep(((Spatial.Face3D)sAMGeometry).ToRhino_Brep());
                return true;
            }

            GeometryBase geometryBase = (sAMGeometry as dynamic).ToRhino() as GeometryBase;
            if(geometryBase != null)
            {
                guid = rhinoDoc.Objects.Add(geometryBase);
                return true;
            }

            return false;

        }

    }
}