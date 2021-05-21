using SAM.Geometry.Planar;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Spatial
{
    public class PlanarIntersectionResult : IIntersectionResult3D
    {
        private List<ISAMGeometry2D> geometry2Ds;
        private Plane plane;

        internal PlanarIntersectionResult(Plane plane, IEnumerable<ISAMGeometry3D> sAMGeometry3Ds)
        {
            this.plane = plane;

            if (plane != null && sAMGeometry3Ds != null)
                geometry2Ds = sAMGeometry3Ds.ToList().ConvertAll(x => Query.Convert(plane, x as dynamic) as ISAMGeometry2D);
        }

        internal PlanarIntersectionResult(Plane plane, Point3D point3D)
        {
            this.plane = plane;

            if (point3D != null && plane != null)
            {
                geometry2Ds = new List<ISAMGeometry2D>();
                geometry2Ds.Add(plane.Convert(plane.Project(point3D)));
            }
        }

        internal PlanarIntersectionResult(Plane plane, Line3D line3D)
        {
            this.plane = plane;

            if (line3D != null && plane != null)
            {
                geometry2Ds = new List<ISAMGeometry2D>();
                geometry2Ds.Add(plane.Convert(plane.Project(line3D)));
            }
        }

        internal PlanarIntersectionResult(Plane plane, Segment3D segment3D)
        {
            this.plane = plane;

            if (segment3D != null && plane != null)
            {
                geometry2Ds = new List<ISAMGeometry2D>();
                geometry2Ds.Add(plane.Convert(plane.Project(segment3D)));
            }
        }

        internal PlanarIntersectionResult(Plane plane, Face3D face3D)
        {
            this.plane = plane;

            if (face3D != null && plane != null)
            {
                geometry2Ds = new List<ISAMGeometry2D>();
                geometry2Ds.Add(plane.Convert(plane.Project(face3D)));
            }
        }

        internal PlanarIntersectionResult(Plane plane)
        {
            this.plane = plane;
        }

        public bool Intersecting
        {
            get
            {
                return geometry2Ds != null && geometry2Ds.Count > 0;
            }
        }

        public List<ISAMGeometry2D> Geometry2Ds
        {
            get
            {
                return geometry2Ds?.ConvertAll(x => x.Clone() as ISAMGeometry2D);
            }
        }

        public ISAMGeometry3D Geometry3D
        {
            get
            {
                if (geometry2Ds == null || geometry2Ds.Count == 0)
                    return null;

                return plane.Convert(geometry2Ds[0]);
            }
        }

        public List<ISAMGeometry3D> Geometry3Ds
        {
            get
            {
                return geometry2Ds?.ConvertAll(x => plane?.Convert(x) as ISAMGeometry3D);
            }
        }

        public T GetGeometry3D<T>() where T : SAMGeometry, ISAMGeometry3D
        {
            ISAMGeometry3D result = Geometry3Ds?.Find(x => x is T);
            if (result == null)
                return null;

            return (T)result;
        }

        public List<T> GetGeometry3Ds<T>() where T : ISAMGeometry3D
        {
            return Geometry3Ds?.FindAll(x => x is T).ConvertAll(x => (T)x);
        }

        public List<T> GetGeometry2Ds<T>() where T : ISAMGeometry2D
        {
            return geometry2Ds?.FindAll(x => x is T).ConvertAll(x => (T)x);
        }
    }
}