using Newtonsoft.Json.Linq;
using SAM.Geometry.Planar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAM.Geometry
{
    public abstract class Face : SAMGeometry
    {
        protected IClosed2D externalEdge;
        protected List<IClosed2D> internalEdges;

        public Face(IClosed2D closed2D)
        {
            if(closed2D is Face)
            {
                Face face = (Face)closed2D;
                externalEdge = (IClosed2D)face.externalEdge.Clone();
                if (face.internalEdges != null)
                    internalEdges = face.internalEdges.ConvertAll(x => (IClosed2D)x.Clone());
            }
            else
            {
                externalEdge = (IClosed2D)closed2D.Clone();
            }
        }

        public Face(JObject jObject)
        {
            FromJObject(jObject);
        }

        public override bool FromJObject(JObject jObject)
        {
            externalEdge = Planar.Create.IClosed2D(jObject.Value<JObject>("ExternalEdge"));

            if (jObject.ContainsKey("InternalEdges"))
                internalEdges = Core.Create.IJSAMObjects<IClosed2D>(jObject.Value<JArray>("InternalEdges"));

            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
                return null;

            jObject.Add("ExternalEdge", externalEdge.ToJObject());
            if (internalEdges != null)
                jObject.Add("InternalEdges", Core.Create.JArray(internalEdges));

            return jObject;
        }

        public IClosed2D ExternalEdge
        {
            get
            {
                return externalEdge.Clone() as IClosed2D;
            }
        }

        public List<IClosed2D> InternalEdges
        {
            get
            {
                if (internalEdges == null)
                    return null;

                return internalEdges.ConvertAll(x => x.Clone() as IClosed2D);
            }
        }

        public List<IClosed2D> Edges
        {
            get
            {
                List<IClosed2D> result = new List<IClosed2D>() { externalEdge };
                if (internalEdges != null && internalEdges.Count > 0)
                    result.AddRange(internalEdges);
                return result;
            }
        }

        public double GetArea()
        {
            double area = externalEdge.GetArea();
            if (internalEdges != null && internalEdges.Count > 0)
                foreach (IClosed2D closed2D in internalEdges)
                    area -= closed2D.GetArea();

            return area;
        }

        public Point2D GetInternalPoint2D()
        {
            if (externalEdge == null)
                return null;

            if (internalEdges == null || internalEdges.Count == 0)
                return externalEdge.GetInternalPoint2D();

            Point2D result = externalEdge.GetCentroid();
            if (Inside(result))
                return result;

            if(externalEdge is ISegmentable2D)
            {
                List<Point2D> point2Ds = ((ISegmentable2D)externalEdge).GetPoints();
                if (point2Ds == null || point2Ds.Count == 0)
                    return null;

                foreach(IClosed2D closed2D in internalEdges)
                {
                    if(closed2D is ISegmentable2D)
                    {
                        List<Point2D> point2Ds_Internal = ((ISegmentable2D)closed2D).GetPoints();
                        if (point2Ds_Internal != null && point2Ds_Internal.Count > 0)
                            point2Ds.AddRange(point2Ds_Internal);
                    }
                }

                int count = point2Ds.Count;
                for(int i=0; i < count - 2; i++)
                {
                    for (int j = 1; j < count - 1; j++)
                    {
                        Point2D point2D = Point2D.Mid(point2Ds[i], point2Ds[j]);
                        if (Inside(point2D))
                            return point2D;
                    }
                }
            }

            return null;
        }

        public bool Inside(Point2D point2D)
        {
            if (internalEdges == null || internalEdges.Count == 0)
                return externalEdge.Inside(point2D);

            return externalEdge.Inside(point2D) && internalEdges.TrueForAll(x => !x.Inside(point2D));
        }

        public bool Inside(IClosed2D closed2D)
        {
            if (internalEdges == null || internalEdges.Count == 0)
                return externalEdge.Inside(closed2D);

            return externalEdge.Inside(closed2D) && internalEdges.TrueForAll(x => !x.Inside(closed2D));
        }
    }
}
