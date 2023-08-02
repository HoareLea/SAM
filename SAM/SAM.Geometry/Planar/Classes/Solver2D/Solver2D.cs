using System.Collections.Generic;

namespace SAM.Geometry.Planar
{
    public class Solver2D
    {
        private List<Solver2DData> solver2DDatas;

        public Solver2D()
        {
        }

        public bool Add(Rectangle2D rectangle2D, Polyline2D polyline2D, int priority = int.MinValue, object tag = null)
        {
            if (rectangle2D == null || polyline2D == null)
            {
                return false;
            }

            return Add((IClosed2D)rectangle2D, (ISAMGeometry2D)polyline2D, priority, tag);
        }

        public bool Add(Rectangle2D rectangle2D, Point2D point2D, int priority = int.MinValue, object tag = null)
        {
            if(rectangle2D == null || point2D == null)
            {
                return false;
            }
            
            return Add((IClosed2D)rectangle2D, (ISAMGeometry2D)point2D, priority, tag);
        }

        private bool Add(IClosed2D closed2D, ISAMGeometry2D geometry2D, int priority = int.MinValue, object tag = null)
        {
            if (closed2D == null || geometry2D == null)
            {
                return false;
            }

            if (solver2DDatas == null)
            {
                solver2DDatas = new List<Solver2DData>();
            }

            Solver2DData solver2DData = new Solver2DData(closed2D, geometry2D);
            solver2DData.Priority = priority;
            solver2DData.Tag = tag;

            solver2DDatas.Add(solver2DData);
            return true;
        }

        public List<Solver2DResult> Solve()
        {
            if(solver2DDatas == null || solver2DDatas.Count == 0)
            {
                return null;
            }

            // Apply priority order
            solver2DDatas.Sort((x, y) => x.Priority.CompareTo(y.Priority));

            List<Solver2DResult> result = new List<Solver2DResult>();

            foreach(Solver2DData solver2DData in solver2DDatas)
            {
                Rectangle2D rectangle2D = solver2DData.Closed2D<Rectangle2D>();
                if(rectangle2D == null)
                {
                    throw new System.NotImplementedException();
                }

                Rectangle2D rectangle2D_New = null;
                
                ISAMGeometry2D sAMGeometry2D = solver2DData.Geometry2D<ISAMGeometry2D>();
                if(sAMGeometry2D is Point2D)
                {
                    //TODO: [Maciek] Add code for Rectangle2D and Point

                    Point2D point2D = (Point2D)sAMGeometry2D;

                    //Iterate through results if nedded
                    foreach(Solver2DResult solver2DResult in result)
                    {
                        IClosed2D closed2D = solver2DResult.Closed2D<IClosed2D>();
                        if(closed2D != null)
                        {
                            bool intersect = closed2D.InRange(rectangle2D);
                        }
                    }

                    rectangle2D_New = null; //Assign new location of rectangle
                }
                else if(sAMGeometry2D is Polyline2D)
                {
                    //TODO: [Maciek] Add code for Rectangle2D and Polyline2D

                    Polyline2D polyline2D = (Polyline2D)sAMGeometry2D;
                    List<Segment2D> segment2Ds = polyline2D.GetSegments();

                    rectangle2D_New = null; //Assign new location of rectangle
                }
                else
                {
                    throw new System.NotImplementedException();
                }

                result.Add(new Solver2DResult(solver2DData, rectangle2D_New));
            }

            return result;
        }
    }
}
