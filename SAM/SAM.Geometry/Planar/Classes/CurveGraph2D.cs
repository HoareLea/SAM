using System;
using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json.Linq;

using SAM.Core;


namespace SAM.Geometry.Planar
{
    public class CurveGraph2D : IJSAMObject
    {
        private ICurve2D[][] curves;
        private Point2D[] points;

        public CurveGraph2D(CurveGraph2D curveGraph2D)
        {
            if (curveGraph2D.points != null)
            {
                int count = curveGraph2D.points.Length;
                points = new Point2D[count];
                for (int i = 0; i < count; i++)
                {
                    Point2D point2D = curveGraph2D.points[i];
                    if (point2D != null)
                        points[i] = new Point2D(point2D);
                }
            }

            if(curveGraph2D.curves != null)
            {
                int count_1 = curveGraph2D.curves.Length;
                curves = new ICurve2D[count_1][];
                for(int i = 0; i < count_1; i++)
                {
                    int count_2 = curveGraph2D.curves[i].Length;
                    ICurve2D[] curve2Ds_Temp = new ICurve2D[count_2];
                    for (int j = 0; j < count_2; j++)
                    {
                        ICurve2D curve2D = curveGraph2D.curves[i][j];
                        if (curve2D != null)
                            curve2Ds_Temp[j] = (ICurve2D)curve2D.Clone();
                    }
                    curves[i] = curve2Ds_Temp;
                }
            }
        }

        public CurveGraph2D(CurveGraph2D curveGraph2D, IEnumerable<int> indexes)
        {
            if (curveGraph2D.points != null)
            {
                int count = curveGraph2D.points.Length;

                List<Point2D> point2Ds_Temp = new List<Point2D>();
                for (int i = 0; i < count; i++)
                    if (indexes == null || indexes.Contains(i))
                    {
                        Point2D point2D = curveGraph2D.points[i];
                        if (point2D != null)
                            point2D = new Point2D(point2D);

                        point2Ds_Temp.Add(point2D);
                    }

                points = point2Ds_Temp.ToArray();
            }

            if (curveGraph2D.curves != null)
            {
                int count_1 = points.Length;
                curves = new ICurve2D[count_1][];
                for (int i = 0; i < count_1; i++)
                {
                    ICurve2D[] curve2Ds_Temp = new ICurve2D[i];
                    for (int j = 0; j < i; j++)
                    {
                        ICurve2D curve2D = curveGraph2D[points[i], points[j]];
                        if (curve2D != null)
                            curve2Ds_Temp[j] = (ICurve2D)curve2D.Clone();
                    }
                    curves[i] = curve2Ds_Temp;
                }
            }
        }        

        public CurveGraph2D(IEnumerable<ICurve2D> curve2Ds, int decimals = 9)
        {
            Load(curve2Ds, decimals);
        }

        public CurveGraph2D(Polygon2D polygon2D, int decimals = 9)
        {
            Load(polygon2D?.GetSegments(), decimals);
        }

        public CurveGraph2D(IEnumerable<Polygon2D> polygon2Ds, int decimals = 9)
        {
            if(polygon2Ds != null)
            {
                List<Segment2D> segment2Ds = new List<Segment2D>();
                foreach(Polygon2D polygon2D in polygon2Ds)
                {
                    if (polygon2D == null)
                        continue;

                    List<Segment2D> segment2Ds_Temp = polygon2D.GetSegments();
                    if (segment2Ds_Temp == null)
                        continue;

                    segment2Ds.AddRange(segment2Ds_Temp);
                }

                Load(segment2Ds, decimals);
            }
        }


        public bool Load(IEnumerable<ICurve2D> curve2Ds, int decimals = 9)
        {
            if (curve2Ds == null)
                return false;

            curves = null;
            points = null;

            HashSet<Point2D> point2Ds_HashSet = new HashSet<Point2D>();
            List<Tuple<Point2D, Point2D, ICurve2D>> tuples = new List<Tuple<Point2D, Point2D, ICurve2D>>();
            foreach (ICurve2D curve2D in curve2Ds)
            {
                if (curve2D == null)
                    continue;

                Point2D point2D_Start = curve2D.GetStart();
                if (point2D_Start == null)
                    continue;

                Point2D point2D_End = curve2D.GetEnd();
                if (point2D_End == null)
                    continue;

                point2D_Start.Round(decimals);
                point2D_End.Round(decimals);

                point2Ds_HashSet.Add(point2D_Start);
                point2Ds_HashSet.Add(point2D_End);

                tuples.Add(new Tuple<Point2D, Point2D, ICurve2D>(point2D_Start, point2D_End, curve2D));
            }

            points = point2Ds_HashSet.ToArray();

            int count = points.Length;

            this.curves = new ICurve2D[count][];
            for (int i = 0; i < count; i++)
                this.curves[i] = new ICurve2D[i];

            foreach (Tuple<Point2D, Point2D, ICurve2D> tuple in tuples)
            {
                int index_Start = IndexOf(tuple.Item1);
                if (index_Start < 0)
                    continue;

                int index_End = IndexOf(tuple.Item2);
                if (index_End < 0)
                    continue;

                if (index_Start == index_End)
                    continue;

                int Max = Math.Max(index_Start, index_End);
                int Min = Math.Min(index_Start, index_End);

                this.curves[Max][Min] = tuple.Item3;
            }

            return true;
        }


        public bool Disconnect(int connectionsCount)
        {
            if (connectionsCount < 0)
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
                result = true;
            }

            return result;
        }

        public bool Disconnect(int index_1, int index_2)
        {
            if (index_1 < 0 || index_2 < 0 || index_1 >= points.Length || index_2 >= points.Length)
                return false;

            curves[Math.Max(index_1, index_2)][Math.Min(index_1, index_2)] = null;
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
                curves[i][index] = null;

            ICurve2D[] curve2D_Temp = curves[index];
            count = curve2D_Temp.Length;
            for (int i = 0; i < count; i++)
                curve2D_Temp[i] = null;

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
                if (curves[i][index] != null)
                    result.Add(i);

            ICurve2D[] curve2D_Temp = curves[index];
            count = curve2D_Temp.Length;
            for (int i = 0; i < count; i++)
                if (curve2D_Temp[i] != null)
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

        
        public ICurve2D this[int i, int j]
        {
            get
            {
                if (curves == null)
                    return null;
                
                if (i < 0 || j < 0)
                    return null;

                int row = Math.Max(i, j);

                if (curves.Length <= row)
                    return null;

                int column = Math.Min(i, j);

                if (curves[row].Length <= column)
                    return null;

                ICurve2D curve2D = curves[row][column];
                if (curve2D == null)
                    return null;

                return (ICurve2D)curve2D.Clone();
            }
        }

        public ICurve2D this [Point2D point2D_1, Point2D point2D_2]
        {
            get
            {
                int index_1 = IndexOf(point2D_1);
                if (index_1 == -1)
                    return null;

                int index_2 = IndexOf(point2D_2);
                if (index_2 == -1)
                    return null;

                return curves[Math.Max(index_1, index_2)][Math.Min(index_1, index_2)];
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
                if (curves[i][index] != null)
                    return true;

            ICurve2D[] curve2D_Temp = curves[index];
            count = curve2D_Temp.Length;
            for (int i = 0; i < count; i++)
                if (curve2D_Temp[i] != null)
                    return true;

            return false;
        }

        
        public bool IsDiconnected(int index)
        {
            return !IsConnected(index);
        }

        
        public CurveGraph2D Clone()
        {
            return new CurveGraph2D(this);
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
            int count = points.Length;

            for (int i = 0; i < count; i++)
                if (ConnectionsCount(i) > 0)
                    return i;

            return -1;
        }


        public bool HasConnections()
        {
            int count_1 = curves.Length;

            for (int i = 1; i < count_1; i++)
            {
                int count_2 = curves[i].Length;
                for (int j = 0; j < count_2; j++)
                    if (curves[i][j] != null)
                        return true;
            }

            return false;
        }


        public bool HasLoops()
        {
            CurveGraph2D curveGraph2D = new CurveGraph2D(this);
            curveGraph2D.Disconnect(1);

            return curveGraph2D.HasConnections();
        }


        public List<ICurve2D> GetCurves(IEnumerable<int> indexes, bool close = false)
        {
            if (indexes == null)
                return null;

            List<int> indexes_Temp = new List<int>(indexes);
            if (close)
                indexes_Temp.Add(indexes_Temp.First());

            int count = indexes_Temp.Count();

            List<ICurve2D> result = new List<ICurve2D>();
            for (int i = 0; i < count - 1; i++)
            {
                ICurve2D curve2D = this[indexes_Temp[i], indexes_Temp[i + 1]];
                if (curve2D == null)
                    return null;
                //throw new NotImplementedException();

                int count_Result = result.Count;

                if (count_Result > 0)
                    Modify.OrientByEnds(curve2D, result[count_Result - 1], count_Result == 1, true);

                result.Add(curve2D);
            }
            return result;
        }


        public List<CurveGraph2D> Split()
        {
            List<CurveGraph2D> result = new List<CurveGraph2D>();
            CurveGraph2D curveGraph2D = Clone();

            int index = curveGraph2D.GetFirstConnected();
            while (index >= 0)
            {
                HashSet<int> indexes = GetAllConnections(index);
                if (indexes == null || indexes.Count == 0)
                    break;
                    

                result.Add(new CurveGraph2D(curveGraph2D, indexes));
                curveGraph2D.DisconnectAll(indexes);

                index = curveGraph2D.GetFirstConnected();             
            }
            return result;
        }

        public List<PolycurveLoop2D> GetPolycurveLoop2Ds()
        {
            CurveGraph2D curveGraph2D = new CurveGraph2D(this);
            curveGraph2D.Disconnect(1);

            List<CurveGraph2D> curveGraph2Ds = curveGraph2D.Split();

            List<PolycurveLoop2D> result = new List<PolycurveLoop2D>();
            foreach (CurveGraph2D curveGraph2D_Temp in curveGraph2Ds)
            {
                List<List<int>> loops = curveGraph2D_Temp.GetLoops();
                if (loops == null || loops.Count == 0)
                    continue;

                List<Tuple<PolycurveLoop2D, BoundingBox2D, Point2D>> tuples = new List<Tuple<PolycurveLoop2D, BoundingBox2D, Point2D>>();
                foreach (List<int> loop in loops)
                {
                    List<ICurve2D> curve2Ds = curveGraph2D_Temp.GetCurves(loop, true);
                    if (curve2Ds == null)
                        continue;

                    PolycurveLoop2D polycurveLoop2D = new PolycurveLoop2D(curve2Ds);
                    if (polycurveLoop2D == null)
                        continue;

                    Point2D point2D = polycurveLoop2D.GetInternalPoint2D();
                    if (point2D == null)
                        continue;

                    BoundingBox2D boundingBox2D = polycurveLoop2D.GetBoundingBox();
                    if (boundingBox2D == null)
                        continue;

                    tuples.Add(new Tuple<PolycurveLoop2D, BoundingBox2D, Point2D>(polycurveLoop2D, boundingBox2D, point2D));
                }

                int count = tuples.Count;

                HashSet<int> redundantLoopIndexes = new HashSet<int>();
                for (int i = 0; i < count - 1; i++)
                {
                    if (redundantLoopIndexes.Contains(i))
                        continue;

                    Tuple<PolycurveLoop2D, BoundingBox2D, Point2D> tuple_1 = tuples[i];

                    for (int j = i + 1; j < count; j++)
                    {
                        if (redundantLoopIndexes.Contains(j))
                            continue;

                        Tuple<PolycurveLoop2D, BoundingBox2D, Point2D> tuple_2 = tuples[j];

                        if (tuple_1.Item2.Inside(tuple_2.Item3))
                        {
                            if(tuple_1.Item1.Inside(tuple_2.Item3))
                            {
                                if(tuple_1.Item2.GetArea() > tuple_2.Item2.GetArea())
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

                for(int i=0; i < tuples.Count(); i++)
                {
                    if (redundantLoopIndexes != null && redundantLoopIndexes.Contains(i))
                        continue;

                    result.Add(tuples[i].Item1);
                }
            }

            return result;
        }

        public List<PolycurveLoop2D> GetPolycurveLoop2Ds_External()
        {
            CurveGraph2D curveGraph2D = new CurveGraph2D(this);
            curveGraph2D.Disconnect(1);

            List<CurveGraph2D> curveGraph2Ds = curveGraph2D.Split();

            List<PolycurveLoop2D> result = new List<PolycurveLoop2D>();
            foreach (CurveGraph2D curveGraph2D_Temp in curveGraph2Ds)
            {
                List<List<int>> loops = curveGraph2D_Temp.GetLoops();
                if (loops == null || loops.Count == 0)
                    continue;

                PolycurveLoop2D polycurveLoop2D = null;
                double area_Max = 0;
                foreach (List<int> loop in loops)
                {
                    List<ICurve2D> curve2Ds = curveGraph2D_Temp.GetCurves(loop, true);
                    if (curve2Ds == null)
                        continue;

                    PolycurveLoop2D polycurveLoop2D_Temp = new PolycurveLoop2D(curve2Ds);
                    if (polycurveLoop2D_Temp == null)
                        continue;

                    double area = polycurveLoop2D_Temp.GetBoundingBox().GetArea();
                    if(area > area_Max)
                    {
                        polycurveLoop2D = polycurveLoop2D_Temp;
                        area_Max = area;
                    }
                }

                if (polycurveLoop2D != null)
                    result.Add(polycurveLoop2D);
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

                    curves = new ICurve2D[count][];
                    for (int i = 0; i < count; i++)
                        curves[i] = new ICurve2D[i];

                    foreach(JObject jObject_Curve in jArray)
                    {
                        int index_1 = jObject_Curve.Value<int>("Index_1");
                        int index_2 = jObject_Curve.Value<int>("Index_2");

                        ICurve2D curve2D = Geometry.Create.ISAMGeometry<ICurve2D>(jObject_Curve.Value<JObject>("Curve"));

                        int min = Math.Min(index_1, index_2);
                        int max = Math.Max(index_1, index_2);
                        curves[max][min] = curve2D;
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

            if(curves != null)
            {
                JArray jArray = new JArray();
                int count_1 = curves.Length;
                for(int i =0; i < count_1; i++)
                {
                    int count_2 = curves[i].Length;
                    for(int j =0; j < count_2; j++)
                    {
                        if(curves[i][j] != null)
                        {
                            jObject = new JObject();
                            jObject.Add("Index_1", i);
                            jObject.Add("Index_2", j);
                            jObject.Add("Curve", curves[i][j].ToJObject());
                            jArray.Add(jObject);
                        }
                    }
                }

                jObject.Add("Curves", jArray);
            }

            return jObject;
        }


        private Dictionary<int, Tuple<double, Orientation>> GetSortedConnectionDataDictionary(int index_1, int index_2, Orientation orientation = Orientation.Undefined)
        {
            List<int> connections_2 = Connections(index_2);
            if (connections_2 == null || connections_2.Count() == 0)
                return null;

            Point2D point2D_1 = this[index_1];
            Point2D point2D_2 = this[index_2];

            connections_2.Remove(index_1);

            Vector2D vector2D_1 = new Vector2D(point2D_1, point2D_2);

            List<Tuple<int, Orientation, double>> tuples = new List<Tuple<int, Orientation, double>>();
            foreach (int index_3 in connections_2)
            {
                Point2D point2D_3 = this[index_3];
                Vector2D vector2D_2 = new Vector2D(point2D_2, point2D_3);

                tuples.Add(new Tuple<int, Orientation, double>(index_3, Point2D.Orientation(point2D_1, point2D_2, point2D_3), vector2D_1.Angle(vector2D_2)));
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

            bool aContinue = true;

            while (aContinue)
            {
                indexes = GetSortedConnections(index_Temp_1, index_Temp_2, orientation);
                if (indexes == null || indexes.Count == 0)
                {
                    aContinue = false;
                    continue;
                }

                int index_Temp_3 = indexes[0];

                if (result.Contains(index_Temp_3))
                {
                    result.Add(index_Temp_3);
                    aContinue = false;
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

        private List<List<int>> GetLoops()
        {
            CurveGraph2D curveGraph2D = new CurveGraph2D(this);
            curveGraph2D.Disconnect(1);

            List<CurveGraph2D> curveGraph2Ds = curveGraph2D.Split();

            List<List<int>> result = new List<List<int>>();
            foreach (CurveGraph2D curveGraph2D_Temp in curveGraph2Ds)
            {
                int index_1 = curveGraph2D_Temp.GetFirstConnected(); //index_1 = curveGraph2D_Temp.GetFirstConnected(2);
                while (index_1 >= 0)
                {
                    List<int> connections = curveGraph2D_Temp.Connections(index_1);
                    if (connections == null || connections.Count == 0)
                        break;

                    foreach (int index_2 in connections)
                    {
                        List<int> aLoop = curveGraph2D_Temp.GetLoop(index_1, index_2);
                        if (aLoop == null)
                            continue;

                        aLoop = Trim(aLoop);
                        if (aLoop == null || aLoop.Count < 2)
                            continue;

                        aLoop = aLoop.ConvertAll(x => points[x]).ConvertAll(x => curveGraph2D.IndexOf(x));

                        if (!HasSimilar(result, aLoop))
                            result.Add(aLoop);
                    }

                    curveGraph2D_Temp.DisconnectAll(index_1);
                    curveGraph2D_Temp.Disconnect(1);
                    index_1 = curveGraph2D_Temp.GetFirstConnected(); //index_1 = curveGraph2D_Temp.GetFirstConnected(2);
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
