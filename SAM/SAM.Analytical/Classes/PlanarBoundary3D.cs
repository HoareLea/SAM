using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using SAM.Core;
using SAM.Geometry.Spatial;


namespace SAM.Analytical
{
    public class PlanarBoundary3D : SAMObject
    {
        private Plane plane;
        private Edge2DLoop edge2DLoop;
        private List<Edge2DLoop> internalEdge2DLoops;

        public PlanarBoundary3D(Face face)
            : base()
        {
            plane = face.GetPlane();
            edge2DLoop = new Edge2DLoop(face.ExternalBoundary);

            List<Geometry.Planar.IClosed2D> internalBoundaries = face.InternalBoundaries;
            if(internalBoundaries != null)
            {
                internalEdge2DLoops = new List<Edge2DLoop>();
                foreach(Geometry.Planar.IClosed2D closed2D in internalBoundaries)
                    internalEdge2DLoops.Add(new Edge2DLoop(closed2D));
            }
        }

        public PlanarBoundary3D(IClosedPlanar3D closedPlanar3D)
        {
            plane = closedPlanar3D.GetPlane();
            edge2DLoop = new Edge2DLoop(closedPlanar3D);
        }

        public PlanarBoundary3D(PlanarBoundary3D planarBoundary3D)
            : base(planarBoundary3D)
        {
            plane = (Geometry.Spatial.Plane)planarBoundary3D.plane.Clone();
            edge2DLoop = new Edge2DLoop(planarBoundary3D.edge2DLoop);

            if (planarBoundary3D.internalEdge2DLoops != null)
                internalEdge2DLoops = planarBoundary3D.internalEdge2DLoops.ConvertAll(x => new Edge2DLoop(x));
        }

        public PlanarBoundary3D(Guid guid, string name, IEnumerable<ParameterSet> parameterSets, Plane plane, Edge2DLoop edge2DLoop, IEnumerable<Edge2DLoop> internalEdge2DLoop)
            : base(guid, name, parameterSets)
        {
            this.plane = new Plane(plane);
            this.edge2DLoop = new Edge2DLoop(edge2DLoop);
            if (internalEdge2DLoop != null)
                this.internalEdge2DLoops = internalEdge2DLoop.ToList().ConvertAll(x => new Edge2DLoop(x));
        }

        public PlanarBoundary3D(JObject jObject)
            : base(jObject)
        {

        }

        public Plane Plane
        {
            get
            {
                return new Geometry.Spatial.Plane(plane);
            }
        }

        public Edge2DLoop Edge2DLoop
        {
            get
            {
                return new Edge2DLoop(edge2DLoop);
            }
        }

        public List<Edge2DLoop> InternalEdge2DLoops
        {
            get
            {
                if (internalEdge2DLoops == null)
                    return null;

                return internalEdge2DLoops.ConvertAll(x => new Edge2DLoop(x));
            }
        }

        public Edge3DLoop GetEdge3DLoop()
        {
            return new Edge3DLoop(plane, edge2DLoop);
        }

        public List<Edge3DLoop> GetInternalEdge3DLoops()
        {
            if (internalEdge2DLoops == null)
                return null;

            return internalEdge2DLoops.ConvertAll(x => new Edge3DLoop(plane, x));
        }

        public List<IClosedPlanar3D> GetInternalClosedPlanar3Ds()
        {
            if (internalEdge2DLoops == null)
                return null;

            List<IClosedPlanar3D> result = new List<IClosedPlanar3D>();
            foreach (Edge2DLoop edge2DLoop in internalEdge2DLoops)
            {
                Polygon3D polygon3D = new Polygon3D(edge2DLoop.Edge2Ds.ConvertAll(x => ((ICurve3D)plane.Convert(x.Curve2D)).GetStart()));
                result.Add(polygon3D);
            }

            return result;
        }

        public bool Coplanar(PlanarBoundary3D planarBoundary3D, double tolerance = Geometry.Tolerance.Angle)
        {
            return plane.Coplanar(planarBoundary3D.plane, tolerance);
        }

        public Face GetFace()
        {
            return new Face(plane, edge2DLoop.GetClosed2D());
        }

        public void Snap(IEnumerable<Point3D> point3Ds, double maxDistance = double.NaN)
        {
            Edge3DLoop edge3DLoop = GetEdge3DLoop();
            edge3DLoop.Snap(point3Ds, maxDistance);
        }

        public BoundingBox3D GetBoundingBox(double offset = 0)
        {
            return GetFace().GetBoundingBox(offset);
        }

        public Vector3D GetNormal(double tolerance = Geometry.Tolerance.MicroDistance)
        {
            return GetEdge3DLoop().GetNormal(tolerance);
        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
                return false;

            plane = new Plane(jObject.Value<JObject>("Plane"));
            edge2DLoop = new Edge2DLoop(jObject.Value<JObject>("Edge2DLoop"));
            if (jObject.ContainsKey("InternalEdge2DLoops"))
                internalEdge2DLoops = Core.Create.IJSAMObjects<Edge2DLoop>(jObject.Value<JArray>("InternalEdge2DLoops"));
            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            jObject.Add("Plane", plane.ToJObject());
            jObject.Add("Edge2DLoop", edge2DLoop.ToJObject());
            if (internalEdge2DLoops != null)
                jObject.Add("InternalEdge2DLoops", Core.Create.JArray(internalEdge2DLoops));
            
            return jObject;
        }
    }
}
