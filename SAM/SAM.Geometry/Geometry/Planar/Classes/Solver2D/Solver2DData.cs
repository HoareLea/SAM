namespace SAM.Geometry.Planar
{
    public class Solver2DData
    {
        private IClosed2D closed2D;
        private ISAMGeometry2D geometry2D;
        private object tag = null;
        private int priority = int.MinValue;

        private Solver2DSettings solver2DSettings;

        public Solver2DSettings Solver2DSettings
        {
            get { return solver2DSettings; }
            set { solver2DSettings = value; }
        }

        public object Tag
        {
            get
            {
                return tag;
            }

            set
            {
                tag = value;
            }
        }

        public int Priority
        {
            get
            {
                return priority;
            }

            set
            {
                priority = value;
            }
        }

        public Solver2DData(IClosed2D closed2D, ISAMGeometry2D geometry2D)
        {
            this.closed2D = closed2D;
            this.geometry2D = geometry2D;
        }

        //public Solver2DData(Rectangle2D rectangle2D, Polyline2D polyline2D)
        //{
        //    closed2D = rectangle2D;
        //    geometry2D = polyline2D;
        //}

        //public Solver2DData(Rectangle2D rectangle2D, Point2D point2D)
        //{
        //    closed2D = rectangle2D;
        //    geometry2D = point2D;
        //}

        public T Geometry2D<T>() where T: ISAMGeometry2D
        {
            return geometry2D is T ? (T)geometry2D : default;
        }

        public T Closed2D<T>() where T : IClosed2D
        {
            return closed2D is T ? (T)closed2D : default;
        }
    }
}
