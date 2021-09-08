using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Spatial
{
    public class Mesh3D : SAMGeometry, ISAMGeometry3D, ISegmentable3D
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
            this.points = points?.ToList().ConvertAll(x => new Point3D(x));
            this.indexes = indexes?.ToList().ConvertAll(x => new Tuple<int, int, int>(x.Item1, x.Item2, x.Item3));
        }

        public ISAMGeometry3D GetMoved(Vector3D vector3D)
        {
            return new Mesh3D(points?.ConvertAll(x => x.GetMoved(vector3D) as Point3D), indexes);
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
            return new Mesh3D(points?.ConvertAll(x => x.GetTransformed(transform3D) as Point3D), indexes);
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

        public int TrianglesCount
        {
            get
            {
                if (indexes == null)
                {
                    return -1;
                }

                return indexes.Count;

            }
        }

        public int PointsCount
        {
            get
            {
                if (points == null)
                {
                    return -1;
                }

                return points.Count;
            }
        }

        public Triangle3D GetTriangle(int index)
        {
            if(points == null || indexes == null)
            {
                return null;
            }

            if(index < 0 || index >= indexes.Count)
            {
                return null;
            }

            int index_1 = indexes[index].Item1;
            if(index_1 < 0 || index_1 >= points.Count)
            {
                return null;
            }

            int index_2 = indexes[index].Item2;
            if (index_2 < 0 || index_2 >= points.Count)
            {
                return null;
            }

            int index_3 = indexes[index].Item3;
            if (index_3 < 0 || index_3 >= points.Count)
            {
                return null;
            }

            return new Triangle3D(points[index_1], points[index_2], points[index_3]);
        }

        public List<Triangle3D> GetTriangles()
        {
            if(points == null || indexes == null)
            {
                return null;
            }

            int count = TrianglesCount;
            if (count == -1)
            {
                return null;
            }

            List<Triangle3D> result = new List<Triangle3D>();
            if(count == 0)
            {
                return result;
            }
            
            for (int i=0; i < TrianglesCount; i++)
            {
                result.Add(GetTriangle(i));
            }

            return result;
        }

        public List<Segment3D> GetSegments()
        {
            return GetSegments(false);
        }

        public List<Segment3D> GetSegments(bool includeSimiliar)
        {
            if(points == null || indexes == null)
            {
                return null;
            }

            List<Segment3D> result = new List<Segment3D>();

            if (includeSimiliar)
            {
                List<Triangle3D> triangle3Ds = GetTriangles();
                if(triangle3Ds == null)
                {
                    return null;
                }

                foreach(Triangle3D triangle3D in triangle3Ds)
                {
                    List<Segment3D> segment3Ds_Triangle3D = triangle3D?.GetSegments();
                    if(segment3Ds_Triangle3D != null && segment3Ds_Triangle3D.Count != 0)
                    {
                        result.AddRange(segment3Ds_Triangle3D);
                    }
                }

                return result;
            }
            else
            {
                List<Tuple<int, int>> tuples = new List<Tuple<int, int>>();

                foreach (Tuple<int, int, int> tuple in indexes)
                {
                    List<int> indexes_Triangle = new List<int>() { tuple.Item1, tuple.Item2, tuple.Item3 };
                    indexes_Triangle.Sort();

                    int index = -1;

                    index = tuples.FindIndex(x => x.Item1 == indexes_Triangle[0] && x.Item2 == indexes_Triangle[1]);
                    if (index == -1)
                    {
                        tuples.Add(new Tuple<int, int>(indexes_Triangle[0], indexes_Triangle[1]));
                    }

                    index = tuples.FindIndex(x => x.Item1 == indexes_Triangle[0] && x.Item2 == indexes_Triangle[2]);
                    if (index == -1)
                    {
                        tuples.Add(new Tuple<int, int>(indexes_Triangle[0], indexes_Triangle[2]));
                    }

                    index = tuples.FindIndex(x => x.Item1 == indexes_Triangle[1] && x.Item2 == indexes_Triangle[2]);
                    if (index == -1)
                    {
                        tuples.Add(new Tuple<int, int>(indexes_Triangle[1], indexes_Triangle[2]));
                    }
                }

                foreach (Tuple<int, int> tuple in tuples)
                {
                    if (tuple.Item1 < 0 || tuple.Item1 >= points.Count)
                    {
                        continue;
                    }

                    if (tuple.Item2 < 0 || tuple.Item2 >= points.Count)
                    {
                        continue;
                    }

                    result.Add(new Segment3D(points[tuple.Item1], points[tuple.Item2]));
                }
            }

            return result;
        }

        public List<Point3D> GetPoints()
        {
            return points?.ConvertAll(x => new Point3D(x));
        }

        public bool On(Point3D point3D, double tolerance = Core.Tolerance.Distance)
        {
            List<Segment3D> segment3Ds = GetSegments(false);
            if(segment3Ds == null || segment3Ds.Count == 0)
            {
                return false;
            }

            return segment3Ds.Find(x => x.On(point3D, tolerance)) != null;
        }

        public List<ICurve3D> GetCurves()
        {
            return GetSegments()?.ConvertAll(x => x as ICurve3D);
        }

        public BoundingBox3D GetBoundingBox(double offset = 0)
        {
            if (points == null)
            {
                return null;
            }

            return new BoundingBox3D(points, offset);
        }
    }
}
