﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Core
{
    public static partial class Query
    {
        public static double Calculate(this ArithmeticOperator arithmeticOperator, double value_1, double value_2)
        {
            if (double.IsNaN(value_1) || double.IsNaN(value_2))
            {
                return double.NaN;
            }

            switch (arithmeticOperator)
            {
                case ArithmeticOperator.Addition:
                    return value_1 + value_2;

                case ArithmeticOperator.Subtraction:
                    return value_1 - value_2;

                case ArithmeticOperator.Division:
                    return value_1 / value_2;
                
                case ArithmeticOperator.Multiplication:
                    return value_1 * value_2;

                case ArithmeticOperator.Modulus:
                    return value_1 % value_2;
            }

            return double.NaN;
        }

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

        public static double Calculate_ByDivision(this Func<double, double> func, double value, double min, double max, int maxDivisions = 100, int maxCount = 100, double tolerance = Core.Tolerance.MacroDistance)
        {
            if(func == null || double.IsNaN(min) || double.IsNaN(max))
            {
                return double.NaN;
            }

            if (AlmostEqual(min, max, tolerance))
            {
                double value_Temp = func.Invoke((min + max) / 2);
                if(AlmostEqual(value_Temp, value, tolerance))
                {
                    return (min + max) / 2;
                }
            }

            Func<double, double, int, List<double>> divide = new((double min_Temp, double max_Temp, int count) =>
            {
                if (double.IsNaN(min_Temp) || double.IsNaN(max_Temp) || count <= -1)
                {
                    return null;
                }

                if(count == 0)
                {
                    return new List<double> {min_Temp, max_Temp };
                }

                double value = (max_Temp - min_Temp) / count;

                List<double> result_Temp = [];
                for (int i = 0; i <= count; i++)
                {
                    result_Temp.Add(min_Temp + (i * value));
                }

                return result_Temp;
            });


            Func<SortedSet<double>, double, Tuple<double, double>> getMinAndMax = new((values, value) => 
            { 
                if(values == null || values.Count == 0)
                {
                    return null;
                }

                if(values.Count == 1)
                {
                    if(!AlmostEqual(values.First(), value, tolerance))
                    {
                        return null;
                    }

                    return new Tuple<double, double>(values.First(), values.First());
                }

                if(values.First() > value)
                {
                    return new Tuple<double, double> (double.NaN, values.First());
                }

                if (values.Last() < value)
                {
                    return new Tuple<double, double>(values.Last(), double.NaN);
                }

                for (int i =0; i < values.Count; i++)
                {
                    if(values.ElementAt(i) > value)
                    {
                        return new Tuple<double, double>(values.ElementAt(i - 1), values.ElementAt(i));
                    }
                }

                return null; 
            });

            Dictionary<double, double> dictionary = new Dictionary<double, double>();

            Tuple<double, double> tuple = null;
            for (int i = 2; i <= maxDivisions; i++)
            {
                List<double> values = divide.Invoke(min, max, i);
                if(values == null || values.Count == 0)
                {
                    continue;
                }

                List<Tuple<double, double>> tuples = new List<Tuple<double, double>>();
                foreach(double value_Temp in values)
                {
                    if(!dictionary.TryGetValue(value_Temp, out double calculatedValue))
                    {
                        calculatedValue = func.Invoke(value_Temp);
                        dictionary[value_Temp] = calculatedValue;
                    }

                    if(double.IsNaN(calculatedValue))
                    {
                        continue;
                    }

                    tuples.Add(new Tuple<double, double>(value_Temp, calculatedValue));
                }

                if(tuples == null || tuples.Count == 0)
                {
                    continue;
                }

                SortedSet<double> sortedSet = new SortedSet<double>(tuples.ConvertAll(x => x.Item2));
                Tuple<double, double> tuple_Temp = getMinAndMax(sortedSet, value);
                if(tuple_Temp == null)
                {
                    continue;
                }

                if (AlmostEqual(tuple_Temp.Item1, value, tolerance))
                {
                    return tuples.Find(x => x.Item2 == tuple_Temp.Item1).Item1;
                }

                if (AlmostEqual(tuple_Temp.Item2, value, tolerance))
                {
                    return tuples.Find(x => x.Item2 == tuple_Temp.Item2).Item1;
                }

                if (!double.IsNaN(tuple_Temp.Item1) && !double.IsNaN(tuple_Temp.Item2))
                {
                    List<Tuple<double, double>> tuples_Temp_1 = tuples.FindAll(x => x.Item2 == tuple_Temp.Item1);
                    List<Tuple<double, double>> tuples_Temp_2 = tuples.FindAll(x => x.Item2 == tuple_Temp.Item2);

                    double value_1 = tuples_Temp_1[0].Item1;
                    double value_2 = tuples_Temp_2[0].Item1;

                    if (tuples_Temp_1.Count != 1 || tuples_Temp_2.Count != 1)
                    {
                        List<double> values_1 = tuples_Temp_1.ConvertAll(x => x.Item1);
                        List<double> values_2 = tuples_Temp_2.ConvertAll(x => x.Item1);

                        double difference = Math.Abs(value_1 - value_2);
                        foreach (double value_Temp_1 in values_1)
                        {
                            foreach (double value_Temp_2 in values_2)
                            {
                                double difference_Temp = Math.Abs(value_Temp_1 - value_Temp_2);
                                if (difference > difference_Temp)
                                {
                                    difference = difference_Temp;
                                    value_1 = value_Temp_1;
                                    value_2 = value_Temp_2;
                                }
                            }
                        }
                    }

                    tuple = new Tuple<double, double>(value_1, value_2);
                    break;
                }
            }

            if(tuple == null || maxCount == 0)
            {
                return double.NaN;
            }

            return Calculate_ByDivision(func, value, tuple.Item1, tuple.Item2, maxDivisions, maxCount - 1, tolerance);
        }
    }
}