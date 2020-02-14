﻿using System;
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
        private BoundaryEdge2DLoop externalEdge2DLoop;
        private List<BoundaryEdge2DLoop> internalEdge2DLoops;

        public PlanarBoundary3D(Face3D face)
            : base()
        {
            plane = face.GetPlane();
            externalEdge2DLoop = new BoundaryEdge2DLoop(face.ExternalEdge);

            List<Geometry.Planar.IClosed2D> internalBoundaries = face.InternalEdges;
            if(internalBoundaries != null)
            {
                internalEdge2DLoops = new List<BoundaryEdge2DLoop>();
                foreach(Geometry.Planar.IClosed2D closed2D in internalBoundaries)
                    internalEdge2DLoops.Add(new BoundaryEdge2DLoop(closed2D));
            }
        }

        public PlanarBoundary3D(IClosedPlanar3D closedPlanar3D)
        {
            plane = closedPlanar3D.GetPlane();
            externalEdge2DLoop = new BoundaryEdge2DLoop(closedPlanar3D);
        }

        public PlanarBoundary3D(PlanarBoundary3D planarBoundary3D)
            : base(planarBoundary3D)
        {
            plane = (Geometry.Spatial.Plane)planarBoundary3D.plane.Clone();
            externalEdge2DLoop = new BoundaryEdge2DLoop(planarBoundary3D.externalEdge2DLoop);

            if (planarBoundary3D.internalEdge2DLoops != null)
                internalEdge2DLoops = planarBoundary3D.internalEdge2DLoops.ConvertAll(x => new BoundaryEdge2DLoop(x));
        }

        public PlanarBoundary3D(Plane plane, Boundary2D boundary2D)
        {
            this.plane = new Plane(plane);
            externalEdge2DLoop = new BoundaryEdge2DLoop(boundary2D.ExternalEdge2DLoop);

            List<BoundaryEdge2DLoop> internalEdge2DLoops = boundary2D.InternalEdge2DLoops;
            if (internalEdge2DLoops != null)
                this.internalEdge2DLoops = internalEdge2DLoops.ConvertAll(x => new BoundaryEdge2DLoop(x));
        }

        public PlanarBoundary3D(Guid guid, string name, IEnumerable<ParameterSet> parameterSets, Plane plane, BoundaryEdge2DLoop edge2DLoop, IEnumerable<BoundaryEdge2DLoop> internalEdge2DLoop)
            : base(guid, name, parameterSets)
        {
            this.plane = new Plane(plane);
            this.externalEdge2DLoop = new BoundaryEdge2DLoop(edge2DLoop);
            if (internalEdge2DLoop != null)
                this.internalEdge2DLoops = internalEdge2DLoop.ToList().ConvertAll(x => new BoundaryEdge2DLoop(x));
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

        public BoundaryEdge2DLoop Edge2DLoop
        {
            get
            {
                return new BoundaryEdge2DLoop(externalEdge2DLoop);
            }
        }

        public List<BoundaryEdge2DLoop> InternalEdge2DLoops
        {
            get
            {
                if (internalEdge2DLoops == null)
                    return null;

                return internalEdge2DLoops.ConvertAll(x => new BoundaryEdge2DLoop(x));
            }
        }

        public BoundaryEdge3DLoop GetEdge3DLoop()
        {
            return new BoundaryEdge3DLoop(plane, externalEdge2DLoop);
        }

        public List<BoundaryEdge2DLoop> Edge2DLoops
        {
            get
            {
                List<BoundaryEdge2DLoop> result = new List<BoundaryEdge2DLoop>() { new BoundaryEdge2DLoop(externalEdge2DLoop) };
                if (internalEdge2DLoops != null && internalEdge2DLoops.Count > 0)
                    result.AddRange(internalEdge2DLoops.ConvertAll(x => new BoundaryEdge2DLoop(x)));
                return result;
            }
        }

        public List<BoundaryEdge3DLoop> GetInternalEdge3DLoops()
        {
            if (internalEdge2DLoops == null)
                return null;

            return internalEdge2DLoops.ConvertAll(x => new BoundaryEdge3DLoop(plane, x));
        }

        public List<IClosedPlanar3D> GetInternalClosedPlanar3Ds()
        {
            if (internalEdge2DLoops == null)
                return null;

            List<IClosedPlanar3D> result = new List<IClosedPlanar3D>();
            foreach (BoundaryEdge2DLoop edge2DLoop in internalEdge2DLoops)
            {
                Polygon3D polygon3D = new Polygon3D(edge2DLoop.BoundaryEdge2Ds.ConvertAll(x => ((ICurve3D)plane.Convert(x.Curve2D)).GetStart()));
                result.Add(polygon3D);
            }

            return result;
        }

        public bool Coplanar(PlanarBoundary3D planarBoundary3D, double tolerance = Geometry.Tolerance.Angle)
        {
            return plane.Coplanar(planarBoundary3D.plane, tolerance);
        }

        public Face3D GetFace3D()
        {
            List<Geometry.Planar.IClosed2D> internalClosed2Ds = null;
            if(internalEdge2DLoops != null && internalEdge2DLoops.Count > 0)
                internalClosed2Ds = InternalEdge2DLoops.ConvertAll(x => x.GetClosed2D());

            return Face3D.Create(plane, externalEdge2DLoop.GetClosed2D(), internalClosed2Ds);
        }

        public void Snap(IEnumerable<Point3D> point3Ds, double maxDistance = double.NaN)
        {
            BoundaryEdge3DLoop edge3DLoop = GetEdge3DLoop();
            edge3DLoop.Snap(point3Ds, maxDistance);
        }

        public BoundingBox3D GetBoundingBox(double offset = 0)
        {
            return GetFace3D().GetBoundingBox(offset);
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
            externalEdge2DLoop = new BoundaryEdge2DLoop(jObject.Value<JObject>("Edge2DLoop"));
            if (jObject.ContainsKey("InternalEdge2DLoops"))
                internalEdge2DLoops = Core.Create.IJSAMObjects<BoundaryEdge2DLoop>(jObject.Value<JArray>("InternalEdge2DLoops"));
            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            jObject.Add("Plane", plane.ToJObject());
            jObject.Add("Edge2DLoop", externalEdge2DLoop.ToJObject());
            if (internalEdge2DLoops != null)
                jObject.Add("InternalEdge2DLoops", Core.Create.JArray(internalEdge2DLoops));
            
            return jObject;
        }
    }
}
