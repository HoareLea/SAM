using System.Collections.Generic;

namespace SAM.Geometry.Planar
{
    public static partial class Modify
    {
        /// <summary>
        /// Connect given Point2D to Segment2Ds. If point2D is on the end of Segment2D then nothing happen. If point2D is on the Segment2D then segment will be split otherwise point2D will be connected to closest Segment2D by adding extra segment.
        /// </summary>
        /// <param name="segment2Ds">Segment2Ds</param>
        /// <param name="point2D">Point2D to be connected</param>
        /// <param name="pointConnectMethod">Point Connect Method</param>
        /// <param name="tolerance">Tolerance</param>
        /// <returns>List of the segments connected to given Point2D</returns>
        public static List<Segment2D> Connect(this List<Segment2D> segment2Ds, Point2D point2D, PointConnectMethod pointConnectMethod = PointConnectMethod.Projection, double tolerance = Core.Tolerance.Distance)
        {
            if (segment2Ds == null || point2D == null || segment2Ds.Count == 0 || pointConnectMethod == PointConnectMethod.Undefined)
                return null;

            List<Segment2D> result = segment2Ds.FindAll(x => x != null && (x[0].AlmostEquals(point2D, tolerance) || x[1].AlmostEquals(point2D, tolerance)));
            if(result != null && result.Count > 0)
            {
                return result;
            }

            if (result == null)
                result = new List<Segment2D>();

            Segment2D segment2D = null;
            Point2D point2D_Project = null;
            
            double distance = double.MaxValue;
            List<int> indexes = new List<int>();

            for (int i=0; i < segment2Ds.Count; i++)
            {
                segment2D = segment2Ds[i];
                if(segment2D == null)
                {
                    continue;
                }

                double distance_Temp = segment2D.Distance(point2D);
                if(distance_Temp < tolerance)
                {
                    point2D_Project = segment2D.Project(point2D);

                    Segment2D segment2D_Temp = null;

                    segment2D_Temp = new Segment2D(segment2D[0], point2D_Project);
                    result.Add(segment2D_Temp);
                    segment2Ds[i] = segment2D_Temp;

                    segment2D_Temp = new Segment2D(point2D_Project, segment2D[1]);
                    result.Add(segment2D_Temp);

                    if (i == segment2Ds.Count - 1)
                    {
                        segment2Ds.Add(segment2D_Temp);
                    }
                    else
                    {
                        segment2Ds.Insert(i + 1, segment2D_Temp);
                    }

                    return result;
                }

                if(distance_Temp < distance)
                {
                    distance = distance_Temp;
                    indexes.Clear();
                }

                if (System.Math.Abs(distance_Temp - distance) < tolerance)
                    indexes.Add(i);
            }

            if (indexes == null || indexes.Count == 0)
            {
                return null;
            }

            indexes.Reverse();

            foreach(int index in indexes)
            {
                segment2D = segment2Ds[index];

                switch (pointConnectMethod)
                {
                    case PointConnectMethod.Ends:
                        result.Add(new Segment2D(point2D, segment2D[0]));
                        result.Add(new Segment2D(point2D, segment2D[1]));
                        segment2Ds.AddRange(result);
                        break;

                    case PointConnectMethod.Projection:
                        Segment2D segment2D_Temp = null;

                        point2D_Project = segment2D.Project(point2D);
                        if(!segment2D.On(point2D_Project, tolerance))
                        {
                            segment2D_Temp = segment2D[0].Distance(point2D_Project) > segment2D[1].Distance(point2D_Project) ? new Segment2D(point2D_Project, segment2D[1]) : new Segment2D(segment2D[0], point2D_Project);
                        }
                        else
                        {
                            segment2D_Temp = new Segment2D(segment2D[0], point2D_Project);
                            segment2Ds[index] = segment2D_Temp;

                            segment2D_Temp = new Segment2D(point2D_Project, segment2D[1]);
                        }

                        if (index == segment2Ds.Count - 1)
                        {
                            segment2Ds.Add(segment2D_Temp);
                        }
                        else
                        {
                            segment2Ds.Insert(index + 1, segment2D_Temp);
                        }

                        segment2D_Temp = new Segment2D(point2D, point2D_Project);
                        result.Add(segment2D_Temp);
                        segment2Ds.Add(segment2D_Temp);
                        break;
                }
            }

            return result;

        }
    }
}