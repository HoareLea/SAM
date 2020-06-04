using Newtonsoft.Json.Linq;
using SAM.Core;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public class PlanarBoundary3D : SAMObject
    {
        private Plane plane;
        private BoundaryEdge2DLoop externalEdge2DLoop;
        private List<BoundaryEdge2DLoop> internalEdge2DLoops;

        public PlanarBoundary3D(IClosedPlanar3D closedPlanar3D)
            : base()
        {
            if (closedPlanar3D is Face3D)
            {
                Face3D face3D = (Face3D)closedPlanar3D;

                plane = face3D.GetPlane();
                externalEdge2DLoop = new BoundaryEdge2DLoop(face3D.ExternalEdge);

                List<Geometry.Planar.IClosed2D> internalEdges = face3D.InternalEdges;
                if (internalEdges != null)
                {
                    internalEdge2DLoops = new List<BoundaryEdge2DLoop>();
                    foreach (Geometry.Planar.IClosed2D internalEdge in internalEdges)
                        internalEdge2DLoops.Add(new BoundaryEdge2DLoop(internalEdge));
                }
            }
            else
            {
                plane = closedPlanar3D.GetPlane();
                externalEdge2DLoop = new BoundaryEdge2DLoop(closedPlanar3D);
            }
        }

        public PlanarBoundary3D(IClosedPlanar3D closedPlanar3D, Point3D location)
            : base()
        {
            plane = closedPlanar3D.GetPlane();
            plane = new Plane(plane, plane.Project(location));

            if (closedPlanar3D is Face3D)
            {
                Face3D face3D = (Face3D)closedPlanar3D;
                externalEdge2DLoop = new BoundaryEdge2DLoop(plane.Convert(face3D.GetExternalEdge()));

                List<IClosedPlanar3D> internalEdges = face3D.GetInternalEdges();
                if (internalEdges != null)
                {
                    internalEdge2DLoops = new List<BoundaryEdge2DLoop>();
                    foreach (IClosedPlanar3D internalEdge in internalEdges)
                        internalEdge2DLoops.Add(new BoundaryEdge2DLoop(plane.Convert(internalEdge)));
                }
            }
            else
            {
                externalEdge2DLoop = new BoundaryEdge2DLoop(plane.Convert(closedPlanar3D));
            }
        }

        public PlanarBoundary3D(PlanarBoundary3D planarBoundary3D)
            : base(planarBoundary3D)
        {
            plane = new Plane(planarBoundary3D.plane);
            externalEdge2DLoop = new BoundaryEdge2DLoop(planarBoundary3D.externalEdge2DLoop);

            if (planarBoundary3D.internalEdge2DLoops != null)
                internalEdge2DLoops = planarBoundary3D.internalEdge2DLoops.ConvertAll(x => new BoundaryEdge2DLoop(x));
        }

        public PlanarBoundary3D(PlanarBoundary3D planarBoundary3D, Plane plane, BoundaryEdge2DLoop edge2DLoop, IEnumerable<BoundaryEdge2DLoop> internalEdge2DLoops)
            : base(planarBoundary3D)
        {
            this.plane = new Plane(plane);
            this.externalEdge2DLoop = new BoundaryEdge2DLoop(edge2DLoop);
            if (internalEdge2DLoops != null)
                this.internalEdge2DLoops = internalEdge2DLoops.ToList().ConvertAll(x => new BoundaryEdge2DLoop(x));
        }

        public PlanarBoundary3D(Plane plane, Boundary2D boundary2D)
        {
            this.plane = new Plane(plane);
            externalEdge2DLoop = new BoundaryEdge2DLoop(boundary2D.ExternalEdge2DLoop);

            List<BoundaryEdge2DLoop> internalEdge2DLoops = boundary2D.InternalEdge2DLoops;
            if (internalEdge2DLoops != null)
            {
                this.internalEdge2DLoops = new List<BoundaryEdge2DLoop>();
                foreach (BoundaryEdge2DLoop internalEdge2DLoop in internalEdge2DLoops)
                    this.internalEdge2DLoops.Add(new BoundaryEdge2DLoop(internalEdge2DLoop));
            }
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

        public void Move(Vector3D vector3D)
        {
            plane.Move(vector3D);
        }

        public void Transform(Transform3D transform3D)
        {
            BoundaryEdge3DLoop boundaryEdge3DLoop_External = GetEdge3DLoop();
            if (boundaryEdge3DLoop_External == null)
                return;

            boundaryEdge3DLoop_External.Transform(transform3D);

            List<BoundaryEdge3DLoop> boundaryEdge3DLoops_Internal = GetInternalEdge3DLoops();
            if (boundaryEdge3DLoops_Internal != null && boundaryEdge3DLoops_Internal.Count > 0)
            {
                foreach(BoundaryEdge3DLoop boundaryEdge3DLoop_Internal in boundaryEdge3DLoops_Internal)
                    boundaryEdge3DLoop_Internal.Transform(transform3D);
            }

            plane = plane.Transform(transform3D);
            externalEdge2DLoop = new BoundaryEdge2DLoop(plane, boundaryEdge3DLoop_External);
            if (boundaryEdge3DLoops_Internal != null && boundaryEdge3DLoops_Internal.Count > 0)
                internalEdge2DLoops = boundaryEdge3DLoops_Internal.ConvertAll(x => new BoundaryEdge2DLoop(plane, x));
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

        public bool Coplanar(PlanarBoundary3D planarBoundary3D, double tolerance = Tolerance.Angle)
        {
            return Coplanar(planarBoundary3D.plane, tolerance);
        }

        public bool Coplanar(Plane plane, double tolerance = Tolerance.Angle)
        {
            return plane.Coplanar(plane, tolerance);
        }

        public Face3D GetFace3D()
        {
            List<Geometry.Planar.IClosed2D> internalClosed2Ds = null;
            if (internalEdge2DLoops != null && internalEdge2DLoops.Count > 0)
                internalClosed2Ds = InternalEdge2DLoops.ConvertAll(x => x.GetClosed2D());

            return Face3D.Create(plane, externalEdge2DLoop.GetClosed2D(), internalClosed2Ds);
        }

        public double GetPerimeter()
        {
            if (externalEdge2DLoop == null)
                return double.NaN;

            return externalEdge2DLoop.GetPerimeter();
        }

        public void Snap(IEnumerable<Point3D> point3Ds, double maxDistance = double.NaN, bool snapInternalEdges = true)
        {
            BoundaryEdge3DLoop boundaryEdge3DLoop_External = GetEdge3DLoop();
            boundaryEdge3DLoop_External.Snap(point3Ds, maxDistance);
            externalEdge2DLoop = new BoundaryEdge2DLoop(plane, boundaryEdge3DLoop_External);

            List<BoundaryEdge2DLoop> internalEdge2DLoops_New = new List<BoundaryEdge2DLoop>();

            if (snapInternalEdges && internalEdge2DLoops != null)
            {
                foreach(BoundaryEdge3DLoop boundaryEdge3DLoop_Internal in GetInternalEdge3DLoops())
                {
                    boundaryEdge3DLoop_Internal.Snap(point3Ds, maxDistance);
                    internalEdge2DLoops_New.Add(new BoundaryEdge2DLoop(plane, boundaryEdge3DLoop_Internal));
                }

                internalEdge2DLoops = internalEdge2DLoops_New;
            }
        }

        public void Snap(IEnumerable<Plane> planes, double maxDistance)
        {
            BoundaryEdge3DLoop boundaryEdge3DLoop = GetEdge3DLoop();
            BoundingBox3D boundingBox3D = boundaryEdge3DLoop.GetBoundingBox(maxDistance);

            Plane plane_Min = null;
            double distance_Min = double.MaxValue;
            foreach(Plane plane_Temp in planes)
            {
                double distance_Temp = plane.Distance(plane_Temp);

                if (distance_Temp > maxDistance)
                    continue;
                
                if (plane_Temp.Distance(boundingBox3D) > maxDistance)
                    continue;

                if (!plane_Temp.Coplanar(plane))
                    continue;

                //This is true only when planes are coplanar
                if(distance_Temp < distance_Min)
                {
                    plane_Min = plane_Temp;
                    distance_Min = distance_Temp;
                }
            }

            if (plane_Min == null)
                return;

            List<BoundaryEdge3DLoop> boundaryEdge3DLoops_Internal = GetInternalEdge3DLoops();

            plane = plane_Min;
            externalEdge2DLoop = new BoundaryEdge2DLoop(plane, boundaryEdge3DLoop);
            internalEdge2DLoops = null;
            if (boundaryEdge3DLoops_Internal != null)
                internalEdge2DLoops = boundaryEdge3DLoops_Internal.ConvertAll(x => new BoundaryEdge2DLoop(plane, boundaryEdge3DLoop));
        }

        public BoundingBox3D GetBoundingBox(double offset = 0)
        {
            return GetFace3D().GetBoundingBox(offset);
        }

        public Vector3D Normal
        {
            get
            {
                return plane.Normal;
            }
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