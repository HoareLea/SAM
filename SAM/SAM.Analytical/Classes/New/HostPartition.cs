using Newtonsoft.Json.Linq;
using SAM.Core;
using SAM.Geometry.Spatial;
using SAM.Geometry.Planar;
using System;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public abstract class HostPartition<T> : BuildingElement<T>, IHostPartition where T: HostPartitionType
    {
        private List<IOpening> openings;
        
        public HostPartition(HostPartition<T> hostPartition)
            : base(hostPartition)
        {
            openings = hostPartition?.openings?.ConvertAll(x => x.Clone());
        }

        public HostPartition(JObject jObject)
            : base(jObject)
        {

        }

        public HostPartition(T hostPartitionType, Face3D face3D)
            : base(hostPartitionType, face3D)
        {

        }

        public HostPartition(Guid guid, T hostPartitionType, Face3D face3D)
            : base(guid, hostPartitionType, face3D)
        {

        }

        public HostPartition(Guid guid, HostPartition<T> hostPartition, Face3D face3D, double tolerance = Tolerance.Distance)
            : base(guid, hostPartition, face3D)
        {
            List<IOpening> openings = hostPartition?.GetOpenings();
            if(openings != null)
            {
                Plane plane = face3D?.GetPlane();

                if (plane != null && plane.Coplanar(hostPartition.Face3D?.GetPlane(), tolerance))
                {
                    openings.ForEach(x => AddOpening(x, tolerance));
                }
            }
        }

        public List<IOpening> GetOpenings()
        {
            return openings?.ConvertAll(x => Core.Query.Clone(x));
        }

        public List<T> GetOpenings<T>() where T : IOpening
        {
            if(openings == null)
            {
                return null;
            }

            List<T> result = new List<T>();
            foreach(IOpening opening in openings)
            {
                if(opening is T)
                {
                    result.Add((T)opening.Clone());
                }
            }

            return result;
        }

        public IOpening RemoveOpening(Guid guid)
        {
            if(openings == null || openings.Count == 0)
            {
                return null;
            }

            foreach(IOpening opening in openings)
            {
                if(opening == null)
                {
                    continue;
                }

                if(opening.Guid == guid)
                {
                    openings.Remove(opening);
                    return opening;
                }
            }

            return null;
        }

        public List<IOpening> AddOpening(IOpening opening, double tolerance = Tolerance.Distance)
        {
            if (opening == null)
            {
                return null;
            }

            Face3D face3D = Face3D;
            if(face3D == null)
            {
                return null;
            }

            Plane plane = face3D.GetPlane();
            if(plane == null)
            {
                return null;
            }

            Face3D face3D_Opening = opening.Face3D;
            if (face3D == null)
            {
                return null;
            }

            Plane plane_Opening = face3D_Opening.GetPlane();
            if (plane == null)
            {
                return null;
            }

            if(!plane.Coplanar(plane_Opening, tolerance))
            {
                return null;
            }

            Geometry.Planar.Face2D face2D = plane.Convert(face3D);
            Geometry.Planar.Face2D face2D_Opening = plane.Convert(face3D_Opening);

            List<Geometry.Planar.Face2D> face2Ds_Intersection = Geometry.Planar.Query.Intersection(face2D, face2D_Opening, tolerance);
            if(face2Ds_Intersection == null || face2Ds_Intersection.Count == 0)
            {
                return null;
            }

            List<Face3D> face3Ds_Intersection = face2Ds_Intersection.ConvertAll(x => plane.Convert(x));

            if (openings == null)
            {
                openings = new List<IOpening>();
            }

            int index = openings.FindIndex(x => x.Guid == opening.Guid);
            if(index != -1)
            {
                openings.RemoveAt(index);
            }

            List<IOpening> result = new List<IOpening>();
            for(int i = 0; i < face3Ds_Intersection.Count; i++)
            {
                Guid guid = i == 0 ? opening.Guid : Guid.NewGuid();

                IOpening opening_Intersection = Create.Opening(guid, opening, face3Ds_Intersection[i]);
                if(opening_Intersection == null)
                {
                    continue;
                }

                result.Add(opening_Intersection);
            }

            return result;
        }

        public bool HasOpening(Guid guid)
        {
            if(openings == null || openings.Count == 0)
            {
                return false;
            }

            return openings.Find(x => x.Guid == guid) != null;
        }

        public IOpening GetOpening(Guid guid)
        {
            if (openings == null || openings.Count == 0)
            {
                return null;
            }

            return openings.Find(x => x.Guid == guid)?.Clone();
        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
            {
                return false;
            }


            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();

            if (jObject == null)
            {
                return jObject;
            }

            return jObject;
        }

        public override void Transform(Transform3D transform3D)
        {
            base.Transform(transform3D);

            if (openings != null)
            {
                foreach (IOpening opening in openings)
                {
                    opening.Transform(transform3D);
                }
            }
        }

        public List<Face3D> GetFace3Ds(bool cutOpenings = false, double tolerance = Tolerance.Distance)
        {
            Face3D face3D = Face3D;
            if(face3D == null)
            {
                return null;
            }

            List<Face3D> result = face3D.FixEdges(tolerance);
            if(!cutOpenings || result == null || result.Count == 0)
            {
                return result;
            }

            List<Face3D> face3Ds_Openings = openings?.ConvertAll(x => x?.Face3D);
            if(face3Ds_Openings == null || face3Ds_Openings.Count == 0)
            {
                return result;
            }


            Plane plane = face3D.GetPlane();

            List<Face2D> face2Ds = result.ConvertAll(x => plane.Convert(x));

            foreach (Face3D face3D_Opening in face3Ds_Openings)
            {
                List<Face3D> face3Ds_Opening_FixEdges = face3D_Opening?.FixEdges(tolerance);
                if (face3Ds_Opening_FixEdges == null || face3Ds_Opening_FixEdges.Count == 0)
                {
                    continue;
                }

                foreach (Face3D face3D_Opening_FixEdges in face3Ds_Opening_FixEdges)
                {
                    Face2D face2D_Opening = plane.Convert(face3D_Opening_FixEdges);

                    for (int i = face2Ds.Count; i <= 0; i--)
                    {
                        Face2D face2D_Partition = face2Ds[i];

                        List<Face2D> face2Ds_Difference = face2D_Partition.Difference(face2D_Opening, tolerance);
                        if (face2Ds_Difference == null || face2Ds_Difference.Count == 0)
                        {
                            face2Ds.RemoveAt(i);
                        }
                        else
                        {
                            face2Ds.AddRange(face2Ds_Difference);
                        }
                    }

                }

            }

            result = face2Ds.ConvertAll(x => plane.Convert(x));

            return result;
        }
    }
}
