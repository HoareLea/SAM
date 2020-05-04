using Newtonsoft.Json.Linq;
using SAM.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public class PointGraph2D : IJSAMObject
    {
        private object[][] objects;
        private Point2D[] points;

        public PointGraph2D(PointGraph2D pointGraph2D)
        {
            if (pointGraph2D.points != null)
            {
                int count = pointGraph2D.points.Length;
                points = new Point2D[count];
                for (int i = 0; i < count; i++)
                {
                    Point2D point2D = pointGraph2D.points[i];
                    if (point2D != null)
                        points[i] = new Point2D(point2D);
                }
            }

            if (pointGraph2D.objects != null)
            {
                int count_1 = pointGraph2D.objects.Length;
                objects = new object[count_1][];
                for (int i = 0; i < count_1; i++)
                {
                    int count_2 = pointGraph2D.objects[i].Length;
                    object[] objects_Temp = new object[count_2];
                    for (int j = 0; j < count_2; j++)
                    {
                        object @object = pointGraph2D.objects[i][j];
                        if (@object != null)
                            objects_Temp[j] = @object;
                    }
                    objects[i] = objects_Temp;
                }
            }
        }

        public PointGraph2D(PointGraph2D pointGraph2D, IEnumerable<int> indexes)
        {
            if (pointGraph2D.points != null)
            {
                int count = pointGraph2D.points.Length;

                List<Point2D> point2Ds_Temp = new List<Point2D>();
                for (int i = 0; i < count; i++)
                    if (indexes == null || indexes.Contains(i))
                    {
                        Point2D point2D = pointGraph2D.points[i];
                        if (point2D != null)
                            point2D = new Point2D(point2D);

                        point2Ds_Temp.Add(point2D);
                    }

                points = point2Ds_Temp.ToArray();
            }

            if (pointGraph2D.objects != null)
            {
                int count_1 = points.Length;
                objects = new object[count_1][];
                for (int i = 0; i < count_1; i++)
                {
                    object[] curve2Ds_Temp = new object[i];
                    for (int j = 0; j < i; j++)
                    {
                        object @object = pointGraph2D[points[i], points[j]];
                        if (@object != null)
                            curve2Ds_Temp[j] = @object;
                    }
                    objects[i] = curve2Ds_Temp;
                }
            }
        }

        public PointGraph2D(IEnumerable<Segment2D> segment2Ds, bool split = true, double tolerance = Tolerance.Distance)
        {
            Load(segment2Ds, split, tolerance);
        }

        public PointGraph2D(Polygon2D polygon2D, bool split = true, double tolerance = Tolerance.Distance)
        {
            Load(polygon2D?.GetSegments(), split, tolerance);
        }

        public PointGraph2D(IEnumerable<Polygon2D> polygon2Ds, bool split = true, double tolerance = Tolerance.Distance)
        {
            if (polygon2Ds != null)
            {
                List<Segment2D> segment2Ds = new List<Segment2D>();
                foreach (Polygon2D polygon2D in polygon2Ds)
                {
                    if (polygon2D == null)
                        continue;

                    List<Segment2D> segment2Ds_Temp = polygon2D.GetSegments();
                    if (segment2Ds_Temp == null)
                        continue;

                    segment2Ds.AddRange(segment2Ds_Temp);
                }

                Load(segment2Ds, split, tolerance);
            }
        }

        public bool Load(IEnumerable<Segment2D> segment2Ds, bool split = false, double tolerance = Tolerance.Distance)
        {
            if (segment2Ds == null)
                return false;

            objects = null;
            points = null;

            if (segment2Ds.Count() == 0)
                return true;

            IEnumerable<Segment2D> segments_Temp = segment2Ds;
            if (split)
                segments_Temp = Query.Split(segment2Ds, tolerance);

            HashSet<Point2D> point2Ds_HashSet = new HashSet<Point2D>();
            List<Tuple<Point2D, Point2D, Segment2D>> tuples = new List<Tuple<Point2D, Point2D, Segment2D>>();
            foreach (Segment2D segment2D in segments_Temp)
            {
                if (!Query.IsValid(segment2D))
                    continue;

                Point2D point2D_Start = segment2D.Start;
                if (point2D_Start == null)
                    continue;

                Point2D point2D_End = segment2D.End;
                if (point2D_End == null)
                    continue;

                if (point2D_End.AlmostEquals(point2D_Start, tolerance))
                    continue;

                point2D_Start.Round(tolerance);
                point2D_End.Round(tolerance);

                point2Ds_HashSet.Add(point2D_Start);
                point2Ds_HashSet.Add(point2D_End);

                tuples.Add(new Tuple<Point2D, Point2D, Segment2D>(point2D_Start, point2D_End, segment2D));
            }

            points = point2Ds_HashSet.ToArray();

            int count = points.Length;

            objects = new object[count][];
            for (int i = 0; i < count; i++)
                objects[i] = new object[i];

            foreach (Tuple<Point2D, Point2D, Segment2D> tuple in tuples)
            {
                int index_Start = IndexOf(tuple.Item1);
                if (index_Start < 0)
                    continue;

                int index_End = IndexOf(tuple.Item2);
                if (index_End < 0)
                    continue;

                if (index_Start == index_End)
                    continue;

                int Max = System.Math.Max(index_Start, index_End);
                int Min = System.Math.Min(index_Start, index_End);

                this.objects[Max][Min] = tuple.Item3;
            }

            return true;
        }

        public bool Disconnect(int connectionsCount)
        {
            if (connectionsCount < 0)
                return false;

            if (points == null || points.Length == 0)
                return false;

            bool result = false;

            int count = points.Length;
            for (int i = 0; i < count; i++)
            {
                List<int> connections = Connections(i);
                if (connections == null)
                    continue;

                if (connections.Count != connectionsCount)
                    continue;

                DisconnectAll(i);
                Disconnect(connectionsCount);
                result = true;
            }

            return result;
        }

        public bool Disconnect(int index_1, int index_2)
        {
            if (index_1 < 0 || index_2 < 0 || index_1 >= points.Length || index_2 >= points.Length)
                return false;

            objects[System.Math.Max(index_1, index_2)][System.Math.Min(index_1, index_2)] = null;
            return true;
        }

        public bool Disconnect(Point2D point2D_1, Point2D point2D_2)
        {
            if (point2D_1 == null || point2D_2 == null)
                return false;

            return Disconnect(IndexOf(point2D_1), IndexOf(point2D_2));
        }

        public bool DisconnectAll(int index)
        {
            if (index < 0)
                return false;

            int count;

            count = points.Length;

            if (index >= count)
                return false;

            for (int i = index + 1; i < count; i++)
                objects[i][index] = null;

            object[] objects_Temp = objects[index];
            count = objects_Temp.Length;
            for (int i = 0; i < count; i++)
                objects_Temp[i] = null;

            return true;
        }

        public bool DisconnectAll(IEnumerable<int> indexes)
        {
            if (indexes == null)
                return false;

            bool result = false;
            foreach (int index in indexes)
                if (DisconnectAll(index))
                    result = true;

            return result;
        }

        public bool DisconnectAll(Point2D point2D)
        {
            return DisconnectAll(IndexOf(point2D));
        }

        public bool DisconnectAll()
        {
            int count = points.Length;

            bool result = false;
            for (int i = 0; i < count; i++)
                if (DisconnectAll(i))
                    result = true;

            return result;
        }

        public int ConnectionsCount(int index)
        {
            List<int> connections = Connections(index);
            if (connections == null)
                return -1;

            return connections.Count;
        }

        public int ConnectionsCount(Point2D point2D)
        {
            return ConnectionsCount(IndexOf(point2D));
        }

        public List<int> Connections(int index)
        {
            if (index < 0)
                return null;

            int count;

            count = points.Length;

            if (index >= count)
                return null;

            List<int> result = new List<int>();

            for (int i = index + 1; i < count; i++)
                if (objects[i][index] != null)
                    result.Add(i);

            object[] objects_Temp = objects[index];
            count = objects_Temp.Length;
            for (int i = 0; i < count; i++)
                if (objects_Temp[i] != null)
                    result.Add(i);

            return result;
        }

        public List<int> Connections(Point2D point2D)
        {
            return Connections(IndexOf(point2D));
        }

        public HashSet<int> GetAllConnections(int index)
        {
            HashSet<int> result = new HashSet<int>();
            AddAllConnections(index, result);
            return result;
        }

        public int IndexOf(Point2D point2D)
        {
            if (points == null)
                return -1;

            int count = points.Length;
            for (int i = 0; i < count; i++)
                if (points[i].Equals(point2D))
                    return i;

            return -1;
        }

        public object this[int i, int j]
        {
            get
            {
                if (objects == null)
                    return null;

                if (i < 0 || j < 0)
                    return null;

                int row = System.Math.Max(i, j);

                if (objects.Length <= row)
                    return null;

                int column = System.Math.Min(i, j);

                if (objects[row].Length <= column)
                    return null;

                return objects[row][column];
            }
        }

        public object this[Point2D point2D_1, Point2D point2D_2]
        {
            get
            {
                int index_1 = IndexOf(point2D_1);
                if (index_1 == -1)
                    return null;

                int index_2 = IndexOf(point2D_2);
                if (index_2 == -1)
                    return null;

                return objects[System.Math.Max(index_1, index_2)][System.Math.Min(index_1, index_2)];
            }
        }

        public Point2D this[int i]
        {
            get
            {
                return new Point2D(points[i]);
            }
        }

        public int Count
        {
            get
            {
                return points.Length;
            }
        }

        public bool IsConnected(int index)
        {
            if (index < 0)
                return false;

            int count = points.Length;
            if (index >= count)
                return false;

            for (int i = index + 1; i < count; i++)
                if (objects[i][index] != null)
                    return true;

            object[] objects_Temp = objects[index];
            count = objects_Temp.Length;
            for (int i = 0; i < count; i++)
                if (objects_Temp[i] != null)
                    return true;

            return false;
        }

        public bool IsDiconnected(int index)
        {
            return !IsConnected(index);
        }

        public PointGraph2D Clone()
        {
            return new PointGraph2D(this);
        }

        public int GetFirstConnected(int connectionsCount)
        {
            if (connectionsCount < 0 || points == null)
                return -1;

            int count = points.Length;

            for (int i = 0; i < count; i++)
                if (ConnectionsCount(i) == connectionsCount)
                    return i;

            return -1;
        }

        public int GetFirstConnected()
        {
            if (points == null)
                return -1;

            int count = points.Length;

            for (int i = 0; i < count; i++)
                if (ConnectionsCount(i) > 0)
                    return i;

            return -1;
        }

        public bool HasConnections()
        {
            int count_1 = objects.Length;

            for (int i = 1; i < count_1; i++)
            {
                int count_2 = objects[i].Length;
                for (int j = 0; j < count_2; j++)
                    if (objects[i][j] != null)
                        return true;
            }

            return false;
        }

        public bool HasLoops()
        {
            PointGraph2D pointGraph2D = new PointGraph2D(this);
            pointGraph2D.Disconnect(1);

            return pointGraph2D.HasConnections();
        }

        public List<T> GetObjects<T>(IEnumerable<int> indexes, bool close = false)
        {
            if (indexes == null)
                return null;

            List<int> indexes_Temp = new List<int>(indexes);
            if (close)
                indexes_Temp.Add(indexes_Temp.First());

            int count = indexes_Temp.Count();

            List<T> result = new List<T>();
            for (int i = 0; i < count - 1; i++)
            {
                object @object = this[indexes_Temp[i], indexes_Temp[i + 1]];
                if (@object == null)
                    result.Add(default(T));

                result.Add((T)@object);
            }
            return result;
        }

        public List<Point2D> GetPoint2Ds(IEnumerable<int> indexes)
        {
            if (indexes == null)
                return null;

            List<Point2D> result = new List<Point2D>();
            foreach (int index in indexes)
                result.Add(points[index]);

            return result;
        }

        public List<Point2D> GetConnectedPoint2Ds()
        {
            if (points == null)
                return null;

            List<Point2D> result = new List<Point2D>();
            for (int i = 0; i < points.Length; i++)
                if (IsConnected(i))
                    result.Add(this[i]);
            return result;
        }

        public List<PointGraph2D> Split()
        {
            List<PointGraph2D> result = new List<PointGraph2D>();
            PointGraph2D pointGraph2D = Clone();

            int index = pointGraph2D.GetFirstConnected();
            while (index >= 0)
            {
                HashSet<int> indexes = GetAllConnections(index);
                if (indexes == null || indexes.Count == 0)
                    break;

                result.Add(new PointGraph2D(pointGraph2D, indexes));
                pointGraph2D.DisconnectAll(indexes);

                index = pointGraph2D.GetFirstConnected();
            }
            return result;
        }

        public Polygon2D GetPolygon2D(Point2D point2D)
        {
            if (point2D == null)
                return null;

            List<int> loop = GetLoop(point2D);
            if (loop == null || loop.Count == 0)
                return null;

            List<Point2D> point2Ds = GetPoint2Ds(loop);
            if (point2Ds == null)
                return null;

            Polygon2D result = new Polygon2D(point2Ds);
            if (!result.Inside(point2D))
                return null;

            return result;
        }

        public List<Polygon2D> GetPolygon2Ds()
        {
            PointGraph2D pointGraph2D = new PointGraph2D(this);
            pointGraph2D.Disconnect(1);

            List<PointGraph2D> pointGraph2Ds = pointGraph2D.Split();

            List<Polygon2D> result = new List<Polygon2D>();
            foreach (PointGraph2D pointGraph2D_Temp in pointGraph2Ds)
            {
                List<List<int>> loops = pointGraph2D_Temp.GetLoops();
                if (loops == null || loops.Count == 0)
                    continue;

                List<Tuple<Polygon2D, BoundingBox2D, Point2D, double>> tuples = new List<Tuple<Polygon2D, BoundingBox2D, Point2D, double>>();
                foreach (List<int> loop in loops)
                {
                    List<Point2D> point2Ds = pointGraph2D_Temp.GetPoint2Ds(loop);
                    if (point2Ds == null)
                        continue;

                    Polygon2D polygon2D = new Polygon2D(point2Ds);
                    if (polygon2D == null)
                        continue;

                    double area = polygon2D.GetArea();
                    if (area == 0)
                        continue;

                    Point2D point2D = polygon2D.GetInternalPoint2D();
                    if (point2D == null)
                        continue;

                    BoundingBox2D boundingBox2D = polygon2D.GetBoundingBox();
                    if (boundingBox2D == null)
                        continue;

                    tuples.Add(new Tuple<Polygon2D, BoundingBox2D, Point2D, double>(polygon2D, boundingBox2D, point2D, area));
                }

                int count = tuples.Count;

                HashSet<int> redundantLoopIndexes = new HashSet<int>();
                for (int i = 0; i < count - 1; i++)
                {
                    if (redundantLoopIndexes.Contains(i))
                        continue;

                    Tuple<Polygon2D, BoundingBox2D, Point2D, double> tuple_1 = tuples[i];

                    for (int j = i + 1; j < count; j++)
                    {
                        if (redundantLoopIndexes.Contains(j))
                            continue;

                        Tuple<Polygon2D, BoundingBox2D, Point2D, double> tuple_2 = tuples[j];

                        if (tuple_1.Item2.Inside(tuple_2.Item3))
                        {
                            if (tuple_1.Item1.Inside(tuple_2.Item3))
                            {
                                if (tuple_1.Item4 > tuple_2.Item4)
                                {
                                    redundantLoopIndexes.Add(i);
                                    break;
                                }
                                else
                                {
                                    redundantLoopIndexes.Add(j);
                                    continue;
                                }
                            }
                        }

                        if (tuple_2.Item2.Inside(tuple_1.Item3))
                        {
                            if (tuple_2.Item1.Inside(tuple_1.Item3))
                            {
                                if (tuple_1.Item2.GetArea() > tuple_2.Item2.GetArea())
                                {
                                    redundantLoopIndexes.Add(i);
                                    break;
                                }
                                else
                                {
                                    redundantLoopIndexes.Add(j);
                                    continue;
                                }
                            }
                        }
                    }
                }

                for (int i = 0; i < tuples.Count(); i++)
                {
                    if (redundantLoopIndexes != null && redundantLoopIndexes.Contains(i))
                        continue;

                    result.Add(tuples[i].Item1);
                }
            }

            return result;
        }

        public List<Polygon2D> GetPolygon2Ds_External()
        {
            PointGraph2D pointGraph2D = new PointGraph2D(this);
            pointGraph2D.Disconnect(1);

            List<PointGraph2D> pointGraph2Ds = pointGraph2D.Split();

            List<Polygon2D> result = new List<Polygon2D>();
            foreach (PointGraph2D pointGraph2D_Temp in pointGraph2Ds)
            {
                List<List<int>> loops = pointGraph2D_Temp.GetLoops();
                if (loops == null || loops.Count == 0)
                    continue;

                Polygon2D polygon2D = null;
                double area_Max = 0;
                foreach (List<int> loop in loops)
                {
                    List<Point2D> point2Ds = pointGraph2D_Temp.GetPoint2Ds(loop);
                    if (point2Ds == null)
                        continue;

                    Polygon2D polygon2D_Temp = new Polygon2D(point2Ds);
                    if (polygon2D_Temp == null)
                        continue;

                    double area = polygon2D_Temp.GetBoundingBox().GetArea();
                    if (area >= area_Max)
                    {
                        area = polygon2D_Temp.GetArea();
                        if (area > area_Max)
                        {
                            polygon2D = polygon2D_Temp;
                            area_Max = area;
                        }
                    }
                }

                if (polygon2D != null)
                    result.Add(polygon2D);
            }

            return result;
        }

        public Polyline2D GetPolyline2D()
        {
            int index = GetFirstConnected(1);
            if (index < 0)
                return null;

            List<int> connections = Connections(index);
            if (connections == null || connections.Count == 0)
                return null;

            List<int> loop = GetLoop(index, connections[0]);
            if (loop == null || loop.Count < 2)
                loop = new List<int>() { index, connections[0] };

            return new Polyline2D(GetPoint2Ds(loop));
        }

        public Polyline2D GetPolyline2D(Point2D point2D_1, Point2D point2D_2)
        {
            throw new NotImplementedException();

            if (point2D_1 == null || point2D_2 == null)
                return null;

            List<PointGraph2D> pointGraph2Ds = Split();
            if (pointGraph2Ds == null || pointGraph2Ds.Count == 0)
                return null;

            PointGraph2D pointGraph2D = null;
            int index_1 = -1;
            int index_2 = -2;

            foreach (PointGraph2D pointGraph2D_Temp in pointGraph2Ds)
            {
                index_1 = pointGraph2D_Temp.IndexOf(point2D_1);
                if (index_1 == -1)
                    continue;

                index_2 = pointGraph2D_Temp.IndexOf(point2D_2);
                if (index_2 == -1)
                    continue;

                if (!IsConnected(index_1) || !IsConnected(index_2))
                    continue;

                pointGraph2D = pointGraph2D_Temp;
                break;
            }

            if (pointGraph2D == null)
                return null;

            bool @continue = true;
            while (@continue)
            {
                List<int> connections = pointGraph2D.Connections(index_1);
                if (connections == null || connections.Count == 0)
                    break;

                List<int> loop = pointGraph2D.GetLoop(index_1, connections[0]);
                if (loop == null || loop.Count == 0)
                    break;

                List<int> loop_Temp = Trim(loop);
                if (loop_Temp == null || loop_Temp.Count == 0)
                    break;

                List<int> indexes_ToRemove = new List<int>();
                if (!loop_Temp.Contains(index_1) && !loop_Temp.Contains(index_2))
                {
                    foreach (int index in loop_Temp)
                    {
                        //pointGraph2D
                    }

                    indexes_ToRemove.AddRange(loop_Temp);
                }
                else
                {
                }

                if (indexes_ToRemove.Count > 0)
                {
                    pointGraph2D.DisconnectAll();
                }
            }
            return null;
        }

        public List<Polyline2D> GetPolyline2Ds()
        {
            List<PointGraph2D> pointGraph2Ds = Split();
            if (pointGraph2Ds == null || pointGraph2Ds.Count == 0)
                return null;

            List<Polyline2D> result = new List<Polyline2D>();
            foreach (PointGraph2D pointGraph2D in pointGraph2Ds)
            {
                Polyline2D polyline2D = pointGraph2D.GetPolyline2D();
                if (polyline2D != null)
                    result.Add(polyline2D);
            }

            return result;
        }

        public bool FromJObject(JObject jObject)
        {
            if (jObject == null)
                return false;

            if (jObject.ContainsKey("Points"))
                points = Geometry.Create.ISAMGeometries<Point2D>(jObject.Value<JArray>("Points"))?.ToArray();

            if (points != null && jObject.ContainsKey("Curves"))
            {
                JArray jArray = jObject.Value<JArray>("Curves");
                if (jArray != null)
                {
                    int count = points.Length;

                    objects = new ICurve2D[count][];
                    for (int i = 0; i < count; i++)
                        objects[i] = new ICurve2D[i];

                    foreach (JObject jObject_Object in jArray)
                    {
                        int index_1 = jObject_Object.Value<int>("Index_1");
                        int index_2 = jObject_Object.Value<int>("Index_2");

                        object @object = null;
                        switch (jObject_Object.Type)
                        {
                            case JTokenType.Object:
                                @object = Geometry.Create.ISAMGeometry<ICurve2D>(jObject_Object.Value<JObject>("Object"));
                                break;

                            case JTokenType.String:
                                @object = jObject_Object.Value<string>("Object");
                                break;

                            case JTokenType.Float:
                                @object = jObject_Object.Value<double>("Object");
                                break;
                        }

                        if (@object == null)
                            continue;

                        int min = System.Math.Min(index_1, index_2);
                        int max = System.Math.Max(index_1, index_2);
                        objects[max][min] = @object;
                    }
                }
            }
            return true;
        }

        public JObject ToJObject()
        {
            JObject jObject = new JObject();
            jObject.Add("_type", Core.Query.FullTypeName(this));

            if (points != null)
                jObject.Add("Points", Geometry.Create.JArray(points));

            if (objects != null)
            {
                JArray jArray = new JArray();
                int count_1 = objects.Length;
                for (int i = 0; i < count_1; i++)
                {
                    int count_2 = objects[i].Length;
                    for (int j = 0; j < count_2; j++)
                    {
                        if (objects[i][j] != null)
                        {
                            jObject = new JObject();
                            jObject.Add("Index_1", i);
                            jObject.Add("Index_2", j);

                            object @object = objects[i][j];

                            if (@object is IJSAMObject)
                                jObject.Add("Object", ((IJSAMObject)@object).ToJObject());
                            else if (@object is double)
                                jObject.Add("Object", (double)@object);
                            else if (@object is string)
                                jObject.Add("Object", (string)@object);
                            jArray.Add(jObject);
                        }
                    }
                }

                jObject.Add("Object", jArray);
            }

            return jObject;
        }

        private Dictionary<int, Tuple<double, Orientation>> GetSortedConnectionDataDictionary(Point2D point2D, int index, Orientation orientation = Orientation.Undefined)
        {
            List<int> connections_2 = Connections(index);
            if (connections_2 == null || connections_2.Count() == 0)
                return null;

            Point2D point2D_2 = this[index];

            Vector2D vector2D_1 = new Vector2D(point2D, point2D_2);

            List<Tuple<int, Orientation, double>> tuples = new List<Tuple<int, Orientation, double>>();
            foreach (int index_3 in connections_2)
            {
                Point2D point2D_3 = this[index_3];
                Vector2D vector2D_2 = new Vector2D(point2D_2, point2D_3);

                tuples.Add(new Tuple<int, Orientation, double>(index_3, Query.Orientation(point2D, point2D_2, point2D_3), vector2D_1.Angle(vector2D_2)));
            }

            List<Tuple<int, Orientation, double>> tuples_Temp;

            Dictionary<int, Tuple<double, Orientation>> result = new Dictionary<int, Tuple<double, Orientation>>();
            switch (orientation)
            {
                case Orientation.Undefined:
                    tuples.ForEach(x => result[x.Item1] = new Tuple<double, Orientation>(x.Item3, x.Item2));
                    break;

                case Orientation.Clockwise:
                    //Clockwise
                    tuples_Temp = tuples.FindAll(x => x.Item2 == Orientation.Clockwise);
                    tuples_Temp.Sort((x, y) => x.Item3.CompareTo(y.Item3));
                    tuples_Temp.Reverse();
                    tuples_Temp.ForEach(x => result[x.Item1] = new Tuple<double, Orientation>(x.Item3, x.Item2));

                    //Collinear
                    tuples.FindAll(x => x.Item2 == Orientation.Collinear).ForEach((x => result[x.Item1] = new Tuple<double, Orientation>(x.Item3, x.Item2)));

                    //CounterClockwise
                    tuples_Temp = tuples.FindAll(x => x.Item2 == Orientation.CounterClockwise);
                    tuples_Temp.Sort((x, y) => x.Item3.CompareTo(y.Item3));
                    tuples_Temp.ForEach(x => result[x.Item1] = new Tuple<double, Orientation>(x.Item3, x.Item2));

                    break;

                case Orientation.CounterClockwise:

                    //CounterClockwise
                    tuples_Temp = tuples.FindAll(x => x.Item2 == Orientation.CounterClockwise);
                    tuples_Temp.Sort((x, y) => x.Item3.CompareTo(y.Item3));
                    tuples_Temp.Reverse();
                    tuples_Temp.ForEach(x => result[x.Item1] = new Tuple<double, Orientation>(x.Item3, x.Item2));

                    //Collinear
                    tuples.FindAll(x => x.Item2 == Orientation.Collinear).ForEach((x => result[x.Item1] = new Tuple<double, Orientation>(x.Item3, x.Item2)));

                    //Clockwise
                    tuples_Temp = tuples.FindAll(x => x.Item2 == Orientation.Clockwise);
                    tuples_Temp.Sort((x, y) => x.Item3.CompareTo(y.Item3));
                    tuples_Temp.ForEach(x => result[x.Item1] = new Tuple<double, Orientation>(x.Item3, x.Item2));

                    break;

                case Orientation.Collinear:

                    //Collinear
                    tuples.FindAll(x => x.Item2 == Orientation.Collinear).ForEach((x => result[x.Item1] = new Tuple<double, Orientation>(x.Item3, x.Item2)));

                    //Clockwise
                    tuples_Temp = tuples.FindAll(x => x.Item2 == Orientation.Clockwise);
                    tuples_Temp.Sort((x, y) => x.Item3.CompareTo(y.Item3));
                    tuples_Temp.ForEach(x => result[x.Item1] = new Tuple<double, Orientation>(x.Item3, x.Item2));

                    //CounterClockwise
                    tuples_Temp = tuples.FindAll(x => x.Item2 == Orientation.CounterClockwise);
                    tuples_Temp.Sort((x, y) => x.Item3.CompareTo(y.Item3));
                    tuples_Temp.Reverse();
                    tuples_Temp.ForEach(x => result[x.Item1] = new Tuple<double, Orientation>(x.Item3, x.Item2));

                    break;
            }

            return result;
        }

        private Dictionary<int, Tuple<double, Orientation>> GetSortedConnectionDataDictionary(int index_1, int index_2, Orientation orientation = Orientation.Undefined)
        {
            Dictionary<int, Tuple<double, Orientation>> dictionary = GetSortedConnectionDataDictionary(this[index_1], index_2, orientation);
            if (dictionary != null)
                dictionary.Remove(index_1);

            return dictionary;
        }

        private List<int> GetSortedConnections(int index_1, int index_2, Orientation orientation = Orientation.Undefined)
        {
            Dictionary<int, Tuple<double, Orientation>> aDictionary = GetSortedConnectionDataDictionary(index_1, index_2, orientation);

            return aDictionary?.Keys.ToList();
        }

        //First and last index shall be the same
        private List<int> GetLoop(int index_1, int index_2, Orientation orientation = Orientation.Clockwise)
        {
            if (index_1 < 0 || index_2 < 0)
                return null;

            int index_Temp_1 = index_1;
            int index_Temp_2 = index_2;

            List<int> indexes = Connections(index_Temp_1);
            if (indexes == null || indexes.Count() == 0)
                return null;

            if (!indexes.Contains(index_2))
                return null;

            List<int> result = new List<int>();
            result.Add(index_Temp_1);
            result.Add(index_Temp_2);

            bool @continue = true;

            while (@continue)
            {
                indexes = GetSortedConnections(index_Temp_1, index_Temp_2, orientation);
                if (indexes == null || indexes.Count == 0)
                {
                    @continue = false;
                    continue;
                }

                int index_Temp_3 = indexes[0];

                if (result.Contains(index_Temp_3))
                {
                    result.Add(index_Temp_3);
                    @continue = false;
                    continue;
                }

                result.Add(index_Temp_3);
                index_Temp_1 = index_Temp_2;
                index_Temp_2 = index_Temp_3;
            }

            if (result.Count < 3)
                return null;

            return result;
        }

        private void AddAllConnections(int index, HashSet<int> indexes)
        {
            List<int> connections = Connections(index);
            if (connections == null)
                return;

            foreach (int aIndex in connections)
            {
                if (indexes.Contains(aIndex))
                    continue;

                indexes.Add(aIndex);
                AddAllConnections(aIndex, indexes);
            }
        }

        public List<int> GetLoop(Point2D point2D)
        {
            Point2D point2D_Closest = Query.Closest(points, point2D);
            if (point2D_Closest == null)
                return null;

            int index = IndexOf(point2D_Closest);
            if (index == -1)
                return null;

            Dictionary<int, Tuple<double, Orientation>> dictionary = GetSortedConnectionDataDictionary(point2D, index, Orientation.Clockwise);
            if (dictionary == null || dictionary.Count == 0)
                return null;

            foreach (KeyValuePair<int, Tuple<double, Orientation>> keyValuePair in dictionary)
            {
                List<int> loop = GetLoop(index, keyValuePair.Key, keyValuePair.Value.Item2);
                if (loop == null || loop.Count < 3)
                    continue;

                loop = Trim(loop);
                if (loop == null || loop.Count < 2)
                    continue;

                return loop;
            }

            return null;
        }

        private List<List<int>> GetLoops()
        {
            PointGraph2D pointGraph2D = new PointGraph2D(this);
            pointGraph2D.Disconnect(1);

            List<PointGraph2D> pointGraph2Ds = pointGraph2D.Split();

            List<List<int>> result = new List<List<int>>();
            foreach (PointGraph2D pointGraph2D_Temp in pointGraph2Ds)
            {
                int index_1 = pointGraph2D_Temp.GetFirstConnected(); //index_1 = curveGraph2D_Temp.GetFirstConnected(2);
                while (index_1 >= 0)
                {
                    List<int> connections = pointGraph2D_Temp.Connections(index_1);
                    if (connections == null || connections.Count == 0)
                        break;

                    foreach (int index_2 in connections)
                    {
                        List<int> loop = pointGraph2D_Temp.GetLoop(index_1, index_2);
                        if (loop == null)
                            continue;

                        loop = Trim(loop);
                        if (loop == null || loop.Count < 2)
                            continue;

                        loop = loop.ConvertAll(x => points[x]).ConvertAll(x => pointGraph2D.IndexOf(x));

                        if (!HasSimilar(result, loop))
                            result.Add(loop);
                    }

                    pointGraph2D_Temp.DisconnectAll(index_1);
                    pointGraph2D_Temp.Disconnect(1);
                    index_1 = pointGraph2D_Temp.GetFirstConnected(); //index_1 = curveGraph2D_Temp.GetFirstConnected(2);
                }
            }

            return result;
        }

        private static List<int> Trim(List<int> IndexList)
        {
            if (IndexList == null)
                return null;

            List<int> aResult = new List<int>();
            if (IndexList.Count() == 1)
                return aResult;

            for (int i = 0; i < IndexList.Count - 1; i++)
            {
                int aIndex = IndexList[i];

                for (int j = i + 1; j < IndexList.Count; j++)
                {
                    if (aIndex == IndexList[j])
                        return IndexList.GetRange(i, j - i);
                }

                aResult.Add(IndexList[i]);
            }

            return aResult;
        }

        private static bool HasSimilar(IEnumerable<List<int>> LoopList, List<int> Loop)
        {
            if (LoopList == null || Loop == null)
                return false;

            foreach (List<int> aLoop_Temp in LoopList)
                if (IsSimilar(aLoop_Temp, Loop))
                    return true;

            return false;
        }

        private static bool IsSimilar(List<int> Loop_1, List<int> Loop_2)
        {
            if (Loop_1 == null && Loop_1 == null)
                return true;

            if (Loop_1 == null || Loop_2 == null)
                return false;

            if (Loop_1.Count != Loop_2.Count)
                return false;

            bool aIsSimilar = true;

            List<int> aLoop_1 = Sort(Loop_1);
            List<int> aLoop_2 = Sort(Loop_2);

            for (int i = 0; i < aLoop_1.Count; i++)
                if (aLoop_1[i] != aLoop_2[i])
                {
                    aIsSimilar = false;
                    break;
                }

            if (aIsSimilar)
                return true;

            aIsSimilar = true;

            aLoop_2.Reverse();
            aLoop_2 = Sort(aLoop_2);
            for (int i = 0; i < aLoop_1.Count; i++)
                if (aLoop_1[i] != aLoop_2[i])
                {
                    aIsSimilar = false;
                    break;
                }

            return aIsSimilar;
        }

        private static List<int> Sort(List<int> IndexList)
        {
            int aMin = IndexList.Min();
            int aIndex = IndexList.IndexOf(aMin);

            List<int> aResult = new List<int>();
            for (int i = aIndex; i < IndexList.Count; i++)
                aResult.Add(IndexList[i]);

            for (int i = 0; i < aIndex; i++)
                aResult.Add(IndexList[i]);

            return aResult;
        }
    }
}