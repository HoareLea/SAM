namespace SAM.Geometry.Planar
{
    public class Solver2DResult
    {
        private Solver2DData solver2DData;
        private IClosed2D closed2D;

        public Solver2DResult(Solver2DData solver2DData, IClosed2D closed2D)
        {
            this.solver2DData = solver2DData;
            this.closed2D = closed2D;
        }

        public Solver2DData Solver2DData
        {
            get 
            { 
                return solver2DData; 
            }
        }

        public T Closed2D<T>() where T : IClosed2D
        {
            return closed2D is T ? (T)closed2D : default;
        }

        public object Tag
        {
            get
            {
                return solver2DData?.Tag;
            }
        }

    }
}
