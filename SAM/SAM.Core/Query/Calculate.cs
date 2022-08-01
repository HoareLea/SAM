using System;

namespace SAM.Core
{
    public static partial class Query
    {
        public static double Calculate(this Func<double, double> func, double value, double start, double stop, out int count, out double calculationValue, int maxCount = 100, double tolerance = Core.Tolerance.MacroDistance)
        {
            calculationValue = double.NaN;
            count = -1;

            if (func == null || double.IsNaN(start) || double.IsNaN(stop))
            {
                return double.NaN;
            }

            double min = Math.Min(start, stop);
            double max = Math.Max(start, stop);

            double value_Min_Temp = func.Invoke(min);
            double value_Max_Temp = func.Invoke(max);

            int sign = Math.Sign(value_Max_Temp - value_Min_Temp);

            double value_Min = Math.Min(value_Min_Temp, value_Max_Temp);
            double value_Max = Math.Max(value_Min_Temp, value_Max_Temp);

            if(value < value_Min || value > value_Max)
            {
                return double.NaN;
            }

            count = 0;

            if (value == value_Min)
            {
                return value_Min;
            }

            if (value == value_Max)
            {
                return value_Max;
            }

            double difference = (max - min) / 2;
            double result = min + difference;

            int sign_Temp = 0;

            for (int i = 1; i <= maxCount; i++)
            {
                count = i;

                if (double.IsNaN(result))
                {
                    return double.NaN;
                }

                calculationValue = func.Invoke(result);
                if (double.IsNaN(calculationValue))
                {
                    return double.NaN;
                }

                if (Math.Abs(value - calculationValue) <= tolerance)
                {
                    return result;
                }

                if (sign_Temp == 0)
                {
                    difference = difference / 2;
                    sign_Temp = Math.Sign(value - calculationValue) * sign;
                    result = result + (difference * sign_Temp);
                }
                else
                {
                    int sign_Temp_New = Math.Sign(value - calculationValue) * sign;
                    if (sign_Temp == sign_Temp_New)
                    {
                        result = result + difference * sign_Temp_New;
                        continue;
                    }
                    else
                    {
                        sign_Temp = sign_Temp_New;
                        difference = difference / 2;
                        result = result + (difference * sign_Temp);
                    }

                }
            }

            return result;
        }

        public static double Calculate(this Func<double, double> func, double value, double start, double stop, int maxCount = 100, double tolerance = Core.Tolerance.MacroDistance)
        {
            return Calculate(func, value, start, stop, out int count, out double calculationValue, maxCount, tolerance);
        }

        /// <summary>
        /// Calculates value for given function. Returns double.NaN when function is null or reach maxCount and not meet given tolerance.
        /// </summary>
        /// <param name="func">Function</param>
        /// <param name="value">value</param>
        /// <param name="min">Minimal value of range</param>
        /// <param name="max">Maximal value of range</param>
        /// <param name="increasing">Monotonicity of the function, true means increasing function, false means descending function</param>
        /// <param name="maxCount">Maximal number of iterate until result doesnt reach tollerance</param>
        /// <param name="tolerance">tollerance</param>
        /// <returns></returns>
        public static double Calculate_BinarySearch(this Func<double, double> func, double value, double min, double max, bool increasing = true, int maxCount = 100, double tolerance = Core.Tolerance.MacroDistance)
        {
            if (func == null)
            {
                return double.NaN;
            }

            int count = 0;

            double value_Temp;
            double mid;
            do
            {
                mid = (min + max) / 2;
                value_Temp = func.Invoke(mid);
                if (value_Temp > value)
                {
                    if (increasing)
                    {
                        min = mid;
                    }
                    else
                    {
                        max = mid;
                    }
                }
                else
                {
                    if (increasing)
                    {
                        max = mid;
                    }
                    else
                    {
                        min = mid;
                    }
                }

                count++;
                if (count > maxCount)
                {
                    return double.NaN;
                }
            }
            while (Math.Abs(value_Temp - value) > tolerance);

            return mid;
        }

        public static double Calculate_ByMaxStep(this Func<double, double> func, double value, double start, double maxStep, out int count, out double calculationValue, int maxCount = 100, double tolerance = Core.Tolerance.MacroDistance)
        {
            calculationValue = double.NaN;
            count = -1;

            if (func == null || double.IsNaN(start) || double.IsNaN(maxStep))
            {
                return double.NaN;
            }

            double value_Start = func.Invoke(start);
            if (double.IsNaN(value_Start))
            {
                return double.NaN;
            }

            if (Math.Abs(value - value_Start) <= tolerance)
            {
                count = 0;
                return value_Start;
            }

            double step = maxStep;
            double result = start + step;

            double value_Next = func.Invoke(result);
            if (double.IsNaN(value_Next))
            {
                return double.NaN;
            }

            if (Math.Abs(value - value_Next) <= tolerance)
            {
                count = 0;
                return value_Next;
            }

            int sign = Math.Sign(value_Next - value_Start);
            int sign_Temp = 0;

            count = 0;
            for (int i = 1; i <= maxCount; i++)
            {
                count = i;
                
                calculationValue = func.Invoke(result);
                if (double.IsNaN(calculationValue))
                {
                    return double.NaN;
                }

                if (Math.Abs(value - calculationValue) <= tolerance)
                {
                    return result;
                }

                if (sign_Temp == 0)
                {
                    sign_Temp = Math.Sign(value - calculationValue) * sign;
                    result = result + (step * sign_Temp);
                }
                else
                {
                    int sign_Temp_New = Math.Sign(value - calculationValue) * sign;
                    if (sign_Temp == sign_Temp_New)
                    {
                        result = result + step * sign_Temp_New;
                        continue;
                    }
                    else
                    {
                        sign_Temp = sign_Temp_New;
                        step = step / 2;
                        result = result + (step * sign_Temp);
                    }

                }
            }

            return result;
        }

        public static double Calculate_ByMaxStep(this Func<double, double> func, double value, double start, double maxStep, int maxCount = 100, double tolerance = Core.Tolerance.MacroDistance)
        {
            return Calculate_ByMaxStep(func, value, start, maxStep, out int count, out double calculationValue, maxCount, tolerance);
        }
    }
}