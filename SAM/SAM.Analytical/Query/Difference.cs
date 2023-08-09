using SAM.Core;
using SAM.Geometry.Spatial;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static List<Panel> Difference<T>(this Panel panel, IEnumerable<T> face3DObjects, double tolerance_Angle = Tolerance.Angle, double tolerance_Distance = Tolerance.Distance) where T : IFace3DObject
        {
            if(panel == null || face3DObjects == null)
            {
                return null;
            }

            if(face3DObjects.Count() == 0)
            {
                return new List<Panel>() { panel.Clone() };
            }

            Face3D face3D = panel.GetFace3D(false);
            if(face3D == null)
            {
                return new List<Panel>() { panel.Clone() };
            }

            BoundingBox3D boundingBox3D = face3D.GetBoundingBox();

            List<Face3D> face3Ds = new List<Face3D>();
            foreach(T face3DObject in face3DObjects)
            {
                Face3D face3D_Face3DObject = face3DObject?.Face3D;
                if(face3D_Face3DObject == null)
                {
                    continue;
                }

                BoundingBox3D boundingBox3D_Face3DObject = face3D_Face3DObject.GetBoundingBox();

                if(!boundingBox3D.InRange(boundingBox3D_Face3DObject, tolerance_Distance))
                {
                    continue;
                }

                face3Ds.Add(face3D_Face3DObject);
            }

            if(face3Ds == null || face3Ds.Count == 0)
            {
                return new List<Panel>() { panel.Clone() };
            }

            List<Panel> result = new List<Panel>();

            List<Face3D> face3Ds_Difference = Geometry.Spatial.Query.Difference(face3D, face3Ds, tolerance_Angle, tolerance_Distance);
            if(face3Ds_Difference == null || face3Ds_Difference.Count == 0)
            {
                return result;
            }

            foreach(Face3D face3D_Difference in face3Ds_Difference)
            {
                if(face3D_Difference == null)
                {
                    continue;
                }

                Panel panel_New = Create.Panel(System.Guid.NewGuid(), panel, face3D_Difference, panel.Apertures);
                if(panel_New != null)
                {
                    result.Add(panel_New);
                }
            }

            return result;

        }
    }
}