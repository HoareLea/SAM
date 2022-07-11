namespace SAM.Math
{
    public static partial class Create
    {
        public static LinearEquation LinearEquation(double x_1, double y_1, double x_2, double y_2)
        {
            if(double.IsNaN(x_1) || double.IsNaN(x_2) || double.IsNaN(y_1) || double.IsNaN(y_2))
            {
                return null;
            }

            double a = double.NaN;
            double b = double.NaN;

            double x = x_2 - x_1;
            a = x != 0 ? (y_2 - y_1) / x : 0;

            if (!double.IsNaN(a))
            {
                double ax = a * x_1;
                double y = y_1;
                if(ax == 0)
                {
                    ax = a * x_2;
                    y = y_2;
                }

                if(ax != 0)
                {
                    b = y / ax;
                }
            }

            return new LinearEquation(a, b);
        }
    }
}
