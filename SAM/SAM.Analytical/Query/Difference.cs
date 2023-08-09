using SAM.Core;
using SAM.Geometry.Spatial;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static List<Panel> Difference<T>(this Panel panel, IEnumerable<T> face3DObjects, double tolerance = Tolerance.Distance) where T : IFace3DObject
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

            List<Face3D> face3Ds = new List<Face3D>();
            foreach(T face3DObject in face3DObjects)
            {
                Face3D face3D_Face3DObject = face3DObject?.Face3D;
                if(face3D_Face3DObject == null)
                {
                    continue;
                }

                face3Ds.Add(face3D_Face3DObject);
            }

            if(face3Ds == null || face3Ds.Count == 0)
            {
                return new List<Panel>() { panel.Clone() };
            }

            // SAM.Geometry.Spatial.Query.Di

            throw new System.NotImplementedException();

        }
    }
}