using Newtonsoft.Json.Linq;
using SAM.Geometry.Planar;
using System.Collections.Generic;

namespace SAM.Geometry
{
    public abstract class Face : SAMGeometry
    {
        protected IClosed2D externalEdge2D;
        protected List<IClosed2D> internalEdge2Ds;

        public Face(IClosed2D closed2D)
        {
            if (closed2D != null)
            {
                if (closed2D is Face)
                {
                    Face face = (Face)closed2D;
                    externalEdge2D = (IClosed2D)face.externalEdge2D.Clone();
                    if (face.internalEdge2Ds != null)
                        internalEdge2Ds = face.internalEdge2Ds.ConvertAll(x => (IClosed2D)x.Clone());
                }
                else
                {
                    externalEdge2D = (IClosed2D)closed2D.Clone();
                }
            }
        }

        public Face(JObject jObject)
        {
            FromJObject(jObject);
        }

        public override bool FromJObject(JObject jObject)
        {
            externalEdge2D = Planar.Create.IClosed2D(jObject.Value<JObject>("ExternalEdge2D"));

            if (jObject.ContainsKey("InternalEdge2Ds"))
            {
                internalEdge2Ds = Core.Create.IJSAMObjects<IClosed2D>(jObject.Value<JArray>("InternalEdge2Ds"));
            }

            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
                return null;

            jObject.Add("ExternalEdge2D", externalEdge2D.ToJObject());
            if (internalEdge2Ds != null)
                jObject.Add("InternalEdge2Ds", Core.Create.JArray(internalEdge2Ds));

            return jObject;
        }

        public IClosed2D ExternalEdge2D
        {
            get
            {
                return externalEdge2D?.Clone() as IClosed2D;
            }
        }

        public List<IClosed2D> InternalEdge2Ds
        {
            get
            {
                if (internalEdge2Ds == null)
                    return null;

                return internalEdge2Ds.ConvertAll(x => x.Clone() as IClosed2D);
            }
        }

        public bool HasInternalEdge2Ds
        {
            get
            {
                return internalEdge2Ds != null && internalEdge2Ds.Count > 0;
            }
        }

        public List<IClosed2D> Edge2Ds
        {
            get
            {
                List<IClosed2D> result = null;

                if (externalEdge2D != null)
                {
                    result = new List<IClosed2D>() { (IClosed2D)externalEdge2D.Clone() };
                }

                if (internalEdge2Ds != null)
                {
                    if(result == null)
                    {
                        result = new List<IClosed2D>();
                    }

                    foreach(IClosed2D internalEdge2D in internalEdge2Ds)
                    {
                        result.Add((IClosed2D)internalEdge2D.Clone());
                    }
                }
                
                return result;
            }
        }

        public double GetArea()
        {
            if(externalEdge2D == null)
            {
                return double.NaN;
            }

            double area = externalEdge2D.GetArea();
            if (internalEdge2Ds != null && internalEdge2Ds.Count > 0)
                foreach (IClosed2D closed2D in internalEdge2Ds)
                    area -= closed2D.GetArea();

            return area;
        }

        public double GetPerimeter()
        {
            return Planar.Query.Perimeter(externalEdge2D);
        }

        public Point2D GetInternalPoint2D(double tolerance = Core.Tolerance.Distance)
        {
            if (externalEdge2D == null)
                return null;

            if (internalEdge2Ds == null || internalEdge2Ds.Count == 0)
                return externalEdge2D.GetInternalPoint2D(tolerance);

            Point2D result = externalEdge2D.GetCentroid();
            if (Inside(result))
                return result;

            if (externalEdge2D is ISegmentable2D)
            {
                List<Point2D> point2Ds = ((ISegmentable2D)externalEdge2D).GetPoints();
                if (point2Ds == null || point2Ds.Count == 0)
                    return null;

                foreach (IClosed2D closed2D in internalEdge2Ds)
                {
                    if (closed2D is ISegmentable2D)
                    {
                        List<Point2D> point2Ds_Internal = ((ISegmentable2D)closed2D).GetPoints();
                        if (point2Ds_Internal != null && point2Ds_Internal.Count > 0)
                            point2Ds.AddRange(point2Ds_Internal);
                    }
                }

                int count = point2Ds.Count;
                for (int i = 0; i < count - 2; i++)
                {
                    for (int j = 1; j < count - 1; j++)
                    {
                        Point2D point2D = Planar.Query.Mid(point2Ds[i], point2Ds[j]);
                        if (Inside(point2D))
                            return point2D;
                    }
                }
            }

            return null;
        }

        public double Distance(Point2D point2D, double tolerance = Core.Tolerance.Distance)
        {
            if (point2D == null)
                return double.NaN;

            if (Inside(point2D, tolerance))
                return 0;

            if (!(externalEdge2D is ISegmentable2D))
                throw new System.NotImplementedException();

            if (internalEdge2Ds != null && internalEdge2Ds.Count > 0 && !internalEdge2Ds.TrueForAll(x => x is ISegmentable2D))
                throw new System.NotImplementedException();

            double distance_Min = ((ISegmentable2D)externalEdge2D).Distance(point2D);
            if (internalEdge2Ds != null && internalEdge2Ds.Count > 0)
            {
                foreach (ISegmentable2D segmentable2D in internalEdge2Ds)
                {
                    double distance = segmentable2D.Distance(point2D);
                    if (distance < distance_Min)
                        distance_Min = distance;
                }
            }

            return distance_Min;
        }

        public double DistanceToEdge2Ds(Point2D point2D)
        {
            if (point2D == null)
                return double.NaN;

            double distance_Min = ((ISegmentable2D)externalEdge2D).Distance(point2D);
            if (internalEdge2Ds != null && internalEdge2Ds.Count > 0)
            {
                foreach (ISegmentable2D segmentable2D in internalEdge2Ds)
                {
                    double distance = segmentable2D.Distance(point2D);
                    if (distance < distance_Min)
                        distance_Min = distance;
                }
            }

            return distance_Min;
        }

        public bool Inside(Point2D point2D, double tolerance = Core.Tolerance.Distance)
        {
            if (point2D == null || !point2D.IsValid())
                return false;
            
            if (internalEdge2Ds == null || internalEdge2Ds.Count == 0)
                return externalEdge2D.Inside(point2D, tolerance) && !externalEdge2D.On(point2D, tolerance);

            bool result = externalEdge2D.Inside(point2D, tolerance) && internalEdge2Ds.TrueForAll(x => !x.Inside(point2D, tolerance));
            if (!result)
                return result;

            return !externalEdge2D.On(point2D, tolerance) && internalEdge2Ds.TrueForAll(x => !x.On(point2D, tolerance));
        }

        /// <summary>
        /// Returns true if closed2D is inside this Face2D
        /// </summary>
        /// <param name="closed2D">Closed2D</param>
        /// <param name="tolerance">Tolerance</param>
        /// <returns>Returns true if closed2D is inside this Face2D</returns>
        public bool Inside(IClosed2D closed2D, double tolerance = Core.Tolerance.Distance)
        {
            if (internalEdge2Ds == null || internalEdge2Ds.Count == 0)
                return externalEdge2D.Inside(closed2D, tolerance);

            return externalEdge2D.Inside(closed2D, tolerance) && internalEdge2Ds.TrueForAll(x => !x.Inside(closed2D, tolerance));
        }

        public void Reverse()
        {
            if (externalEdge2D is IReversible)
                ((IReversible)externalEdge2D).Reverse();

            if (internalEdge2Ds != null)
            {
                for (int i = 0; i < internalEdge2Ds.Count; i++)
                    if (internalEdge2Ds[i] is IReversible)
                        ((IReversible)internalEdge2Ds[i]).Reverse();
            }
        }

        public override int GetHashCode()
        {
            int hash = 13;
            
            hash = (hash * 7) + externalEdge2D.GetHashCode();
            
            if (internalEdge2Ds != null)
                foreach (IClosed2D internalEdge2D in internalEdge2Ds)
                    hash = (hash * 7) + internalEdge2D.GetHashCode();
            
            return hash;
        }
    }
}