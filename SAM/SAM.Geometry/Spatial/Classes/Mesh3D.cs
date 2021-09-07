using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Spatial
{
    public class Mesh3D : SAMGeometry, ISAMGeometry3D
    {
        private List<Point3D> points;
        private List<Tuple<int, int, int>> indexes;
        
        public Mesh3D(JObject jObject)
        {
            FromJObject(jObject);
        }

        public Mesh3D(Mesh3D mesh3D)
        {
            points = mesh3D?.points?.ConvertAll(x => new Point3D(x));
            indexes = mesh3D?.indexes?.ConvertAll(x => new Tuple<int, int, int>(x.Item1, x.Item2, x.Item3));
        }

        public Mesh3D(IEnumerable<Point3D> points, IEnumerable<Tuple<int, int, int>> indexes)
        {
            points = points?.ToList().ConvertAll(x => new Point3D(x));
            indexes = indexes?.ToList().ConvertAll(x => new Tuple<int, int, int>(x.Item1, x.Item2, x.Item3));
        }

        public ISAMGeometry3D GetMoved(Vector3D vector3D)
        {
            throw new NotImplementedException();
        }

        public int IndexOf(Point3D point3D)
        {
            if (point3D == null || points == null)
            {
                return -1;
            }

            for (int i = 0; i < points.Count; i++)
            {
                if (point3D.Equals(points[i]))
                {
                    return i;
                }
            }

            return -1;
        }

        public int IndexOf(Point3D point3D, double tolerance)
        {
            if (point3D == null || points == null)
            {
                return -1;
            }

            for (int i = 0; i < points.Count; i++)
            {
                if (point3D.AlmostEquals(points[i], tolerance))
                {
                    return i;
                }
            }

            return -1;
        }

        public ISAMGeometry3D GetTransformed(Transform3D transform3D)
        {
            throw new NotImplementedException();
        }

        public override bool FromJObject(JObject jObject)
        {
            if (jObject.ContainsKey("Points"))
                points = Create.Point3Ds(jObject.Value<JArray>("Points"));

            if (jObject.ContainsKey("Indexes"))
            {
                indexes = new List<Tuple<int, int, int>>();

                JArray jArray = jObject.Value<JArray>("Indexes");
                if(jArray != null)
                {
                    foreach (JArray jArray_Temp in jArray)
                    {
                        if(jArray_Temp == null || jArray_Temp.Count < 3)
                        {
                            continue;
                        }

                        indexes.Add(new Tuple<int, int, int>(jArray_Temp[0].Value<int>(), jArray_Temp[1].Value<int>(), jArray_Temp[2].Value<int>()));
                    }
                }
            }

            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
                return null;

            if (points != null)
                jObject.Add("Points", Geometry.Create.JArray(points));

            if(indexes != null)
            {
                JArray jArray = new JArray();

                foreach(Tuple<int, int, int> tuple in indexes)
                {
                    JArray jArray_Temp = new JArray();
                    jArray_Temp.Add(tuple.Item1);
                    jArray_Temp.Add(tuple.Item2);
                    jArray_Temp.Add(tuple.Item3);
                    jArray.Add(jArray_Temp);
                }

                jObject.Add("Indexes", jArray);
            }

            return jObject;
        }

        public override ISAMGeometry Clone()
        {
            return new Mesh3D(points, indexes);
        }
    }
}
