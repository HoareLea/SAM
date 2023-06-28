namespace SAM.Core
{
    public static partial class Query
    {

        public static double LogarithmicMeanTemperatureDifference(double enteringTemperature_1, double leavingTemperature_1, double enteringTemperature_2, double leavingTemperature_2)
        {
            if(double.IsNaN(enteringTemperature_1) || double.IsNaN(leavingTemperature_1) || double.IsNaN(enteringTemperature_2) || double.IsNaN(leavingTemperature_2))
            {
                return double.NaN;
            }

            double temperatureDifference_1 = leavingTemperature_1 - enteringTemperature_2;
            if(temperatureDifference_1 == 0)
            {
                return double.NaN;
            }

            double temperatureDifference_2 = enteringTemperature_1 - leavingTemperature_2;

            return (temperatureDifference_2 - temperatureDifference_1) / System.Math.Log(temperatureDifference_2 / temperatureDifference_1);

        }
    }
}