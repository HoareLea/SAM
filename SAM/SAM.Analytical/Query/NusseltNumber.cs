using SAM.Core;

namespace SAM.Analytical
{
    public static partial class Query
    {

        /// <summary>
        /// Nusselt Number (Nu) according to EN 673:1997 [-]
        /// </summary>
        /// <param name="fluidMaterial">SAM Fluid Material</param>
        /// <param name="temperatureDifference">Temperature difference between glass surfaces bounding the fluid space</param>
        /// <param name="width">Width of the space</param>
        /// <param name="meanTemperature">Mean Temperature</param>
        /// <param name="angle">Angle in radians (measured in 2D from Upward direction (0, 1) Vector2D.SignedAngle(Vector2D)), angle less than 0 considered as downward direction</param>
        /// <returns>Nusselt Number (Nu) [-]</returns>
        public static double NusseltNumber(this FluidMaterial fluidMaterial, double temperatureDifference, double width, double meanTemperature, double angle)
        {
            if (fluidMaterial == null || double.IsNaN(temperatureDifference) || double.IsNaN(width) || double.IsNaN(meanTemperature))
                return double.NaN;

            if (angle < 0)
                return 1;

            double a = double.NaN;
            double n = double.NaN;

            if(angle == 0)
            {
                a = 0.035;
                n = 0.38;
            }
            else if(angle == System.Math.PI / 2)
            {
                a = 0.016;
                n = 0.28;
            }
            else if(angle == System.Math.PI / 4)
            {
                a = 0.01;
                n = 0.31;
            }
            else if(angle > 0 && angle < System.Math.PI / 4)
            {
                a = 0.035 + ((angle / (System.Math.PI / 4)) * (0.1 - 0.035));
                n = 0.38 - ((angle / (System.Math.PI / 4)) * (0.38 - 0.31));
            }
            else if(angle > System.Math.PI / 4 && angle < System.Math.PI / 2)
            {
                double angle_Temp = (System.Math.PI / 2) - angle;

                a = 0.1 + ((angle_Temp / (System.Math.PI / 4)) * (0.16 - 0.1));
                n = 0.31 - ((angle_Temp / (System.Math.PI / 4)) * (0.31 - 0.28));
            }

            if (double.IsNaN(a) || double.IsNaN(n))
                return double.NaN;

            return NusseltNumber(fluidMaterial, temperatureDifference, width, meanTemperature, a, n);
        }

        /// <summary>
        /// Nusselt Number (Nu) according to EN 673:1997 [-]
        /// </summary>
        /// <param name="fluidMaterial">SAM Fluid Material</param>
        /// <param name="temperatureDifference">Temperature difference between glass surfaces bounding the fluid space</param>
        /// <param name="width">Width of the space</param>
        /// <param name="meanTemperature">Mean Temperature</param>
        /// <param name="constant">Constant (A) value for equation Nu = A(Gr * Pr)^n</param>
        /// <param name="exponent">Exponent (n) value for equation Nu = A(Gr * Pr)^n</param>
        /// <returns>Nusselt Number (Nu) [-]</returns>
        public static double NusseltNumber(this FluidMaterial fluidMaterial, double temperatureDifference, double width, double meanTemperature, double constant, double exponent)
        {
            if (fluidMaterial == null || double.IsNaN(temperatureDifference) || double.IsNaN(width) || double.IsNaN(meanTemperature))
                return double.NaN;

            double grashofNumber = GrashofNumber(fluidMaterial, temperatureDifference, width, meanTemperature);
            if (double.IsNaN(grashofNumber))
                return double.NaN;

            double prandtlNumber = PrandtlNumber(fluidMaterial);
            if (double.IsNaN(prandtlNumber))
                return double.NaN;

            return constant * System.Math.Pow(grashofNumber * prandtlNumber, exponent);
        }
    }
}