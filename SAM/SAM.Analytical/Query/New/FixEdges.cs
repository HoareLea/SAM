using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static List<T> FixEdges<T>(this T buildingElement, double tolerance = Core.Tolerance.Distance) where T : IBuildingElement
        {
            if(buildingElement == null)
            {
                return null;
            }

            Face3D face3D = buildingElement.Face3D;
            if(face3D == null)
            {
                return null;
            }

            List<Face3D> face3Ds = face3D.FixEdges(tolerance);
            if(face3Ds == null)
            {
                return null;
            }

            List<T> result = new List<T>();
            foreach(Face3D face3D_Temp in face3Ds)
            {
                System.Guid guid = buildingElement.Guid;
                while(result.Find(x => x.Guid == guid) != null)
                {
                    guid = System.Guid.NewGuid();
                }

                T face3DObject_New = default;
                if(buildingElement is IPartition)
                {
                    IPartition partition = Create.Partition((IPartition)buildingElement, guid, face3D_Temp, tolerance);
                    if(partition != null)
                    {
                        face3DObject_New = (T)partition;
                    }
                }
                else if(buildingElement is IOpening)
                {
                    IOpening opening = Create.Opening(guid, (IOpening)buildingElement, face3D_Temp, OpeningLocation(face3D_Temp, tolerance));
                    if (opening != null)
                    {
                        face3DObject_New = (T)opening;
                    }
                }

                if (face3DObject_New != null)
                {
                    result.Add(face3DObject_New);
                }
            }

            return result;
        }
    }
}