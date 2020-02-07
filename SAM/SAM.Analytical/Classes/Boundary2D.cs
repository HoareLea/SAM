using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using SAM.Core;
using SAM.Geometry.Spatial;


namespace SAM.Analytical
{
    public class Boundary2D : SAMObject
    {
        private Edge2DLoop edge2DLoop;
        private List<Edge2DLoop> internalEdge2DLoops;

        public Boundary2D(Edge2DLoop edge2DLoop)
            : base()
        {
            this.edge2DLoop = new Edge2DLoop(edge2DLoop);
        }

        public Boundary2D(IClosedPlanar3D closedPlanar3D)
        {
            edge2DLoop = new Edge2DLoop(closedPlanar3D);
        }

        public Boundary2D(JObject jObject)
            : base(jObject)
        {

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

        public Edge3DLoop GetEdge3DLoop(Plane plane)
        {
            return new Edge3DLoop(plane, edge2DLoop);
        }

        public List<Edge3DLoop> GetInternalEdge3DLoops(Plane plane)
        {
            if (internalEdge2DLoops == null)
                return null;

            return internalEdge2DLoops.ConvertAll(x => new Edge3DLoop(plane, x));
        }

        public List<IClosedPlanar3D> GetInternalClosedPlanar3Ds(Plane plane)
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

        public Face GetFace(Plane plane)
        {
            return new Face(plane, edge2DLoop.GetClosed2D());
        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
                return false;

            edge2DLoop = new Edge2DLoop(jObject.Value<JObject>("Edge2DLoop"));
            if (jObject.ContainsKey("InternalEdge2DLoops"))
                internalEdge2DLoops = Core.Create.IJSAMObjects<Edge2DLoop>(jObject.Value<JArray>("InternalEdge2DLoops"));
            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            jObject.Add("Edge2DLoop", edge2DLoop.ToJObject());
            if (internalEdge2DLoops != null)
                jObject.Add("InternalEdge2DLoops", Core.Create.JArray(internalEdge2DLoops));
            
            return jObject;
        }


        public static Boundary2D Create(List<Edge2DLoop> edge2DLoops, out List<Edge2DLoop> edge2DLoops_Outside)
        {
            edge2DLoops_Outside = null;

            if (edge2DLoops == null || edge2DLoops.Count() == 0)
                return null;

            Edge2DLoop edge2DLoop_Max = null;
            double area_Max = double.MinValue;

            Dictionary<Edge2DLoop, Geometry.Planar.IClosed2D> dictionary = new Dictionary<Edge2DLoop, Geometry.Planar.IClosed2D>();
            foreach (Edge2DLoop edge2DLoop in edge2DLoops)
            {
                Geometry.Planar.IClosed2D closed2D = edge2DLoop.GetClosed2D();
                double area = edge2DLoop.GetArea();
                if (area > area_Max)
                {
                    area_Max = area;
                    edge2DLoop_Max = edge2DLoop;
                }
                dictionary[edge2DLoop] = closed2D;
            }

            if (edge2DLoop_Max == null)
                return null;

            Boundary2D boundary2D = new Boundary2D(edge2DLoop_Max);

            Geometry.Planar.IClosed2D closed2D_Max = dictionary[edge2DLoop_Max];
            dictionary.Remove(edge2DLoop_Max);

            foreach (KeyValuePair<Edge2DLoop, Geometry.Planar.IClosed2D> keyValuePair in dictionary)
            {
                if (!closed2D_Max.Inside(keyValuePair.Value))
                {
                    if (edge2DLoops_Outside == null)
                        edge2DLoops_Outside = new List<Edge2DLoop>();

                    edge2DLoops_Outside.Add(keyValuePair.Key);

                    continue;
                }

                if (boundary2D.internalEdge2DLoops == null)
                    boundary2D.internalEdge2DLoops = new List<Edge2DLoop>();

                boundary2D.internalEdge2DLoops.Add(new Edge2DLoop(keyValuePair.Key));
            }

            return boundary2D;
        }

        public static List<Boundary2D> Create(List<Edge2DLoop> edge2DLoops)
        {
            if (edge2DLoops == null)
                return null;

            List<Boundary2D> result = new List<Boundary2D>();
            if (edge2DLoops.Count == 0)
                return result;

            List<Edge2DLoop> edge2DLoops_All = new List<Edge2DLoop>(edge2DLoops);
            while(edge2DLoops_All.Count > 0)
            {
                List<Edge2DLoop> edge2DLoops_Outside = null;
                Boundary2D boundary2D = Create(edge2DLoops_All, out edge2DLoops_Outside);
                if (boundary2D != null)
                    result.Add(boundary2D);

                edge2DLoops_All = edge2DLoops_Outside;
            }

            return result;
        }
    }
}
