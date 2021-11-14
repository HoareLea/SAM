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

        public static List<T> FixEdges<T>(this T hostPartition, bool cutOpenings, double tolerance = Core.Tolerance.Distance) where T : IHostPartition
        {
            if (hostPartition == null)
            {
                return null;
            }

            Face3D face3D = hostPartition.Face3D;
            if (face3D == null)
            {
                return null;
            }

            if(cutOpenings)
            {
                List<IOpening> openings = hostPartition.GetOpenings();
                if(openings != null && openings.Count != 0)
                {
                    Plane plane = face3D.GetPlane();

                    Geometry.Planar.IClosed2D externalEdge = face3D.ExternalEdge2D;
                    List<Geometry.Planar.IClosed2D> internalEdges = face3D.InternalEdge2Ds;
                    if(internalEdges == null)
                    {
                        internalEdges = new List<Geometry.Planar.IClosed2D>();
                    }

                    foreach(IOpening opening in openings)
                    {
                        Geometry.Planar.IClosed2D closed2D = plane.Convert(opening?.Face3D)?.ExternalEdge2D;
                        if(closed2D == null)
                        {
                            continue;
                        }

                        internalEdges.Add(closed2D);
                    }

                    Geometry.Planar.Face2D face2D = Geometry.Planar.Create.Face2D(externalEdge, internalEdges);
                    if(face2D != null)
                    {
                        face3D = plane.Convert(face2D);
                    }
                }
            }

            List<Face3D> face3Ds = face3D.FixEdges(tolerance);
            if (face3Ds == null)
            {
                return null;
            }

            List<T> result = new List<T>();
            foreach (Face3D face3D_Temp in face3Ds)
            {
                System.Guid guid = hostPartition.Guid;
                while (result.Find(x => x.Guid == guid) != null)
                {
                    guid = System.Guid.NewGuid();
                }

                IHostPartition hostPartition_New = Create.HostPartition(guid, face3D_Temp, (IHostPartition)hostPartition, tolerance);
                if(hostPartition_New is T)
                {
                    result.Add((T)hostPartition_New);
                }
            }

            return result;
        }


    }
}