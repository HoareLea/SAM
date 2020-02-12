﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAM.Geometry.Spatial
{
    public class Face3D : Face, IClosedPlanar3D, ISAMGeometry3D
    {
        private Plane plane;

        public Face3D(Planar.IClosed2D externalEdge)
            : base(externalEdge)
        {
            plane = new Plane(Point3D.Zero, Vector3D.BaseZ);
        }

        public Face3D(Plane plane, Planar.IClosed2D externalEdge)
            : base(externalEdge)
        {
            this.plane = new Plane(plane);
        }

        public Face3D(Plane plane, Planar.Face2D face2D)
            : base(face2D)
        {
            this.plane = new Plane(plane);
        }

        public Face3D(IClosedPlanar3D closedPlanar3D)
            : base(closedPlanar3D.GetPlane().Convert(closedPlanar3D))
        {
            plane = closedPlanar3D.GetPlane();
        }

        public Face3D(Face3D face3D)
            : base(face3D)
        {
            this.plane = new Plane(face3D.plane);
        }

        public Face3D(JObject jObject)
            : base(jObject)
        {

        }

        public Plane GetPlane()
        {
            return new Plane(plane);
        }

        public override ISAMGeometry Clone()
        {
            return new Face3D(this);
        }

        public BoundingBox3D GetBoundingBox(double offset = 0)
        {
            return plane.Convert(externalEdge).GetBoundingBox(offset);
        }

        IClosed3D IClosed3D.GetExternalEdge()
        {
            return this.GetExternalEdge();
        }

        public IClosedPlanar3D GetExternalEdge()
        {
            return plane.Convert(externalEdge);//.GetExternalEdge();
        }

        public List<IClosedPlanar3D> GetInternalEdges()
        {
            if (internalEdges == null)
                return null;

            List<IClosedPlanar3D> result = new List<IClosedPlanar3D>();
            foreach (Planar.IClosed2D closed2D in internalEdges)
                result.Add(plane.Convert(closed2D));//.GetExternalEdge());

            return result;
        }

        public List<IClosedPlanar3D> GetEdges()
        {
            List<IClosedPlanar3D> result = new List<IClosedPlanar3D>() { GetExternalEdge() };
            List<IClosedPlanar3D> closedPlanar3Ds = GetInternalEdges();
            if (closedPlanar3Ds != null && closedPlanar3Ds.Count > 0)
                result.AddRange(closedPlanar3Ds);
            return result;
        }

        public ISAMGeometry3D GetMoved(Vector3D vector3D)
        {
            return new Face3D((Plane)plane.GetMoved(vector3D), (Planar.IClosed2D)externalEdge.Clone());
        }

        public bool Inside(Face3D face3D, double tolerance = Tolerance.MicroDistance)
        {
            if (!plane.Coplanar(face3D.plane, tolerance))
                return false;

            IClosed3D closed3D = face3D.plane.Convert(face3D.externalEdge);

            return externalEdge.Inside(plane.Convert(closed3D));
        }

        public bool Inside(Point3D point3D, double tolerance = Tolerance.MicroDistance)
        {
            if (!plane.On(point3D, tolerance))
                return false;

            Planar.Point2D point2D = plane.Convert(point3D);
            return externalEdge.Inside(point2D);
        }

        public IClosedPlanar3D Project(IClosed3D closed3D)
        {
            if(closed3D is ISegmentable3D)
            {
                List<Point3D> point3Ds = ((ISegmentable3D)closed3D).GetPoints().ConvertAll(x => plane.Project(x));
                return new Polygon3D(point3Ds);
            }

            return null;
        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
                return false;

            plane = new Plane(jObject.Value<JObject>("Plane"));
            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
                return null;

            jObject.Add("Plane", plane.ToJObject());

            return jObject;
        }


        public static Face3D GetFace(Plane plane, Planar.IClosed2D externalEdge, IEnumerable<Planar.IClosed2D> internalEdges)
        {
            if (plane == null || externalEdge == null)
                return null;

            Planar.Face2D face2D = Planar.Face2D.GetFace(externalEdge, internalEdges);
            if (face2D == null)
                return null;

            return new Face3D(plane, face2D);
        }

        public static Face3D GetFace(Plane plane, IEnumerable<Planar.IClosed2D> edges, out List<Planar.IClosed2D> edges_Excluded)
        {
            edges_Excluded = null;

            if (plane == null || edges == null || edges.Count() == 0)
                return null;

            Planar.Face2D face2D = Planar.Face2D.GetFace(edges, out edges_Excluded);
            return new Face3D(plane, face2D);
        }
        
        public static Face3D GetFace(Plane plane, IEnumerable<Planar.IClosed2D> edges)
        {
            if (plane == null || edges == null || edges.Count() == 0)
                return null;
            
            return new Face3D(plane, Planar.Face2D.GetFace(edges));
        }

        public static List<Face3D> GetFaces(Plane plane, IEnumerable<Planar.IClosed2D> edges)
        {
            if (plane == null || edges == null || edges.Count() == 0)
                return null;

            List<Planar.Face2D> face2Ds = Planar.Face2D.GetFaces(edges);
            if (face2Ds == null)
                return null;

            List<Face3D> result = new List<Face3D>();
            if (face2Ds.Count == 0)
                return result;


            return face2Ds.ConvertAll(x => new Face3D(plane, x));
        }
    }
}
