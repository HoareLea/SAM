using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public class CurveGraph2D
    {
        private ICurve2D[][] curve2Ds;
        private Point2D[] point2Ds;

        public CurveGraph2D(CurveGraph2D curveGraph2D)
        {
            if (curveGraph2D.point2Ds != null)
            {
                int count = curveGraph2D.point2Ds.Length;
                point2Ds = new Point2D[count];
                for (int i = 0; i < count; i++)
                {
                    Point2D point2D = curveGraph2D.point2Ds[i];
                    if (point2D != null)
                        point2Ds[i] = new Point2D(point2D);
                }
            }

            if(curveGraph2D.curve2Ds != null)
            {
                int count_1 = curveGraph2D.curve2Ds.Length;
                curve2Ds = new ICurve2D[count_1][];
                for(int i = 0; i < count_1; i++)
                {
                    int count_2 = curveGraph2D.curve2Ds[i].Length;
                    ICurve2D[] curve2Ds_Temp = new ICurve2D[count_2];
                    for (int j = 0; j < count_2; j++)
                    {
                        ICurve2D curve2D = curveGraph2D.curve2Ds[i][j];
                        if (curve2D != null)
                            curve2Ds_Temp[j] = (ICurve2D)curveGraph2D.curve2Ds[i][j].Clone();
                    }
                }
            }
        }

        public CurveGraph2D(IEnumerable<ICurve2D> curve2Ds)
        {
            if (curve2Ds == null)
                return;

            HashSet<Point2D> point2Ds_HashSet = new HashSet<Point2D>();
            List<Tuple<Point2D, Point2D, ICurve2D>> tuples = new List<Tuple<Point2D, Point2D, ICurve2D>>();
            foreach(ICurve2D curve2D in curve2Ds)
            {
                if (curve2D == null)
                    continue;

                Point2D point2D_Start = curve2D.GetStart();
                if (point2D_Start == null)
                    continue;

                Point2D point2D_End = curve2D.GetEnd();
                if (point2D_End == null)
                    continue;

                point2Ds_HashSet.Add(point2D_Start);
                point2Ds_HashSet.Add(point2D_End);

                tuples.Add(new Tuple<Point2D, Point2D, ICurve2D>(point2D_Start, point2D_End, curve2D));
            }

            point2Ds = point2Ds_HashSet.ToArray();

            int count = point2Ds.Length;

            this.curve2Ds = new ICurve2D[count][];
            for (int i = 0; i < count; i++)
                this.curve2Ds[i] = new ICurve2D[i];

            foreach(Tuple<Point2D, Point2D, ICurve2D> tuple in tuples)
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

                this.curve2Ds[Max][Min] = tuple.Item3;
            }
        }

        public bool Disconnect(int index_1, int index_2)
        {
            if (index_1 < 0 || index_2 < 0 || index_1 >= point2Ds.Length || index_2 >= point2Ds.Length)
                return false;

            curve2Ds[Math.Max(index_1, index_2)][Math.Min(index_1, index_2)] = null;
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

            count = point2Ds.Length;

            if (index >= count)
                return false;

            for (int i = index + 1; i < count; i++)
                curve2Ds[i][index] = null;

            ICurve2D[] curve2D_Temp = curve2Ds[index];
            count = curve2D_Temp.Length;
            for (int i = 0; i < count; i++)
                curve2D_Temp[i] = null;

            return true;
        }

        public bool DisctonnectAll(IEnumerable<int> indexes)
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
            int count = point2Ds.Length;

            bool result = false;
            for (int i = 0; i < count; i++)
                if (DisconnectAll(i))
                    result = true;

            return result;
        }

        public bool Disconnect(int connectionsCount)
        {
            if (connectionsCount < 0)
                return false;

            bool result = false;

            int count = point2Ds.Length;
            for(int i=0; i < count; i++)
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

            count = point2Ds.Length;

            if (index >= count)
                return null;

            List<int> result = new List<int>();

            for (int i = index + 1; i < count; i++)
                if (curve2Ds[i][index] != null)
                    result.Add(i);

            ICurve2D[] curve2D_Temp = curve2Ds[index];
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

        public int IndexOf(Point2D point2D)
        {
            if (point2Ds == null)
                return -1;

            int count = point2Ds.Length;
            for (int i = 0; i < count; i++)
                if (point2Ds[i].Equals(point2D))
                    return i;

            return -1;
        }

        public ICurve2D this[int i, int j]
        {
            get
            {
                return curve2Ds[Math.Max(i, j)][Math.Min(i, j)];
            }
        }

        public Point2D this[int i]
        {
            get
            {
                return point2Ds[i];
            }
        }

        public int Count
        {
            get
            {
                return point2Ds.Length;
            }
        }

        public bool IsConnected(int index)
        {
            if (index < 0)
                return false;

            int count = point2Ds.Length;
            if (index >= count)
                return false;

            for (int i = index + 1; i < count; i++)
                if (curve2Ds[i][index] != null)
                    return true;

            ICurve2D[] curve2D_Temp = curve2Ds[index];
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
    }
}
