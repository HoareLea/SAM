using SAM.Core;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Weather
{
    /// <summary>
    /// The prevailing mean outdoor air temperature (Tpma(out)) according with the ASHRAE 55 (2013) methods
    /// </summary>
    public class PrevailingMeanOutdoorAirTemperatureCalculator
    {
        IPrevailingMeanOutdoorAirTemperatureCalculationMethod PrevailingMeanOutdoorAirTemperatureCalculationMethod { get; set; }

        public PrevailingMeanOutdoorAirTemperatureCalculator(IPrevailingMeanOutdoorAirTemperatureCalculationMethod prevailingMeanOutdoorAirTemperatureCalculationMethod)
        {
            PrevailingMeanOutdoorAirTemperatureCalculationMethod = prevailingMeanOutdoorAirTemperatureCalculationMethod;
        }

        public PrevailingMeanOutdoorAirTemperatureCalculator(int sequentialDays)
        {
            PrevailingMeanOutdoorAirTemperatureCalculationMethod = new SimpleArithmeticMeanCalculationMethod(sequentialDays);
        }

        public PrevailingMeanOutdoorAirTemperatureCalculator(int sequentialDays, double alpha)
        {
            PrevailingMeanOutdoorAirTemperatureCalculationMethod = new WeightingCalculationMethod(sequentialDays, alpha);
        }

        public List<double> Calculate(WeatherData weatherData)
        {
            if(PrevailingMeanOutdoorAirTemperatureCalculationMethod == null)
            {
                return null;
            }

            List<WeatherDay> weatherDays = weatherData?.WeatherDays();
            if(weatherDays == null)
            {
                return null;
            }

            return Calculate(weatherDays);
        }

        public List<double> Calculate(WeatherYear weatherYear)
        {
            if (PrevailingMeanOutdoorAirTemperatureCalculationMethod == null)
            {
                return null;
            }

            List<WeatherDay> weatherDays = weatherYear?.WeatherDays;
            if (weatherDays == null)
            {
                return null;
            }

            return Calculate(weatherDays);
        }

        public List<double> Calculate(IEnumerable<WeatherDay> weatherDays)
        {
            if (PrevailingMeanOutdoorAirTemperatureCalculationMethod == null || weatherDays == null)
            {
                return null;
            }

            List<double> dryBulbTemperatures = new List<double>();

            for (int i = 0; i < weatherDays.Count(); i++)
            {
                double? dryBulbTemperature = weatherDays?.ElementAt(i)?.Average(WeatherDataType.DryBulbTemperature);
                if(dryBulbTemperature == null || !dryBulbTemperature.HasValue || double.IsNaN(dryBulbTemperature.Value))
                {
                    continue;
                }

                dryBulbTemperatures.Add(dryBulbTemperature.Value);
            }

            return Calculate(dryBulbTemperatures);

        }

        public List<double> Calculate(IEnumerable<double> dryBulbTemperatures)
        {
            List<double> dryBulbTemperatures_Temp = dryBulbTemperatures?.ToList();
            if(dryBulbTemperatures_Temp == null)
            {
                return null;
            }

            if (PrevailingMeanOutdoorAirTemperatureCalculationMethod is WeightingCalculationMethod)
            {
                return Calculate(dryBulbTemperatures_Temp, (WeightingCalculationMethod)PrevailingMeanOutdoorAirTemperatureCalculationMethod);
            }

            if (PrevailingMeanOutdoorAirTemperatureCalculationMethod is SimpleArithmeticMeanCalculationMethod)
            {
                return Calculate(dryBulbTemperatures_Temp, (SimpleArithmeticMeanCalculationMethod)PrevailingMeanOutdoorAirTemperatureCalculationMethod);
            }

            return null;
        }

        private List<double> Calculate(List<double> dryBulbTempartures, WeightingCalculationMethod weightingCalculationMethod)
        {
            if (dryBulbTempartures == null || weightingCalculationMethod == null || double.IsNaN(weightingCalculationMethod.Alpha) || weightingCalculationMethod.Alpha > 1 || weightingCalculationMethod.Alpha < 0)
            {
                return null;
            }

            int sequentialDays = weightingCalculationMethod.SequentialDays;
            if (sequentialDays < 1)
            {
                return null;
            }

            List<double> result = new List<double>();

            if (dryBulbTempartures.Count == 0)
            {
                return result;
            }

            double alpha = weightingCalculationMethod.Alpha;

            double[] alphas = new double[sequentialDays];
            alphas[0] = 1;
            if (sequentialDays > 1)
            {
                alphas[1] = alpha;
                for (int i = 2; i < alphas.Count(); i++)
                {
                    alphas[i] = alphas[i - 1] * alpha;
                }
            }

            List<double> dryBulbTemperatures_Temp = new List<double>(dryBulbTempartures);
            for (int i = 0; i < sequentialDays; i++)
            {
                double dryBulbTemperature = dryBulbTemperatures_Temp[dryBulbTemperatures_Temp.Count - i - 1];
                dryBulbTemperatures_Temp.Insert(0, dryBulbTemperature);
            }

            for (int i = sequentialDays; i < dryBulbTemperatures_Temp.Count; i++)
            {
                List<double> dryBulbTempartures_Range = dryBulbTemperatures_Temp.GetRange(i - sequentialDays, sequentialDays);

                double dryBulbTemperature = 0;
                for (int j = 0; j < dryBulbTempartures_Range.Count; j++)
                {
                    dryBulbTemperature += dryBulbTempartures_Range[j] * alphas[j];
                }

                dryBulbTemperature *= (1 - alpha);

                result.Add(dryBulbTemperature);
            }

            return result;
        }

        //private List<double> Calculate(List<double> dryBulbTempartures, WeightingCalculationMethod weightingCalculationMethod)
        //{
        //    if (dryBulbTempartures == null || weightingCalculationMethod == null || double.IsNaN(weightingCalculationMethod.Alpha) || weightingCalculationMethod.Alpha > 1 || weightingCalculationMethod.Alpha < 0)
        //    {
        //        return null;
        //    }

        //    int sequentialDays = weightingCalculationMethod.SequentialDays;
        //    if (sequentialDays < 1)
        //    {
        //        return null;
        //    }

        //    List<double> result = new List<double>();

        //    if (dryBulbTempartures.Count == 0)
        //    {
        //        return result;
        //    }

        //    result.Add(dryBulbTempartures[0]);

        //    double alpha = weightingCalculationMethod.Alpha;

        //    double[] alphas = new double[sequentialDays];
        //    alphas[0] = 1;
        //    if (sequentialDays > 1)
        //    {
        //        alphas[1] = alpha;
        //        for (int i = 2; i < alphas.Count(); i++)
        //        {
        //            alphas[i] = alphas[i - 1] * alpha;
        //        }
        //    }

        //    for (int i = 1; i < dryBulbTempartures.Count - 1; i++)
        //    {
        //        int count = sequentialDays;
        //        int indexStart = i - sequentialDays;
        //        if(indexStart < 0)
        //        {
        //            count = i;
        //            indexStart = 0;
        //        }

        //        List<double> dryBulbTempartures_Temp = dryBulbTempartures.GetRange(indexStart, count);

        //        double dryBulbTemperature = 0;
        //        for (int j = 0; j < dryBulbTempartures_Temp.Count; j++)
        //        {
        //            dryBulbTemperature += dryBulbTempartures_Temp[j] * alphas[j];
        //        }

        //        dryBulbTemperature *= (1 - alpha);

        //        result.Add(dryBulbTemperature);
        //    }

        //    return result;
        //}

        private List<double> Calculate(List<double> dryBulbTempartures, SimpleArithmeticMeanCalculationMethod simpleArithmeticMeanCalculationMethod)
        {
            if(dryBulbTempartures == null || simpleArithmeticMeanCalculationMethod == null || simpleArithmeticMeanCalculationMethod.SequentialDays <= 0)
            {
                return null;
            }

            List<double> result = new List<double>();

            if (dryBulbTempartures.Count == 0)
            {
                return result;
            }

            int sequentialDays = simpleArithmeticMeanCalculationMethod.SequentialDays;

            List<double> dryBulbTemperatures_Temp = new List<double>(dryBulbTempartures);
            for(int i = 0; i < sequentialDays; i++)
            {
                double dryBulbTemperature = dryBulbTemperatures_Temp[dryBulbTemperatures_Temp.Count - i - 1];
                dryBulbTemperatures_Temp.Insert(0, dryBulbTemperature);
            }

            for (int i = sequentialDays; i < dryBulbTemperatures_Temp.Count; i++)
            {
                List<double> dryBulbTempartures_Range = dryBulbTemperatures_Temp.GetRange(i - sequentialDays, sequentialDays);

                result.Add(dryBulbTempartures_Range.Average());
            }

            return result;
        }

        //private List<double> Calculate(List<double> dryBulbTempartures, SimpleArithmeticMeanCalculationMethod simpleArithmeticMeanCalculationMethod)
        //{
        //    if (dryBulbTempartures == null || simpleArithmeticMeanCalculationMethod == null || simpleArithmeticMeanCalculationMethod.SequentialDays <= 0)
        //    {
        //        return null;
        //    }

        //    List<double> result = new List<double>();

        //    if (dryBulbTempartures.Count == 0)
        //    {
        //        return result;
        //    }

        //    int sequentialDays = simpleArithmeticMeanCalculationMethod.SequentialDays;

        //    for (int i = 0; i < dryBulbTempartures.Count; i++)
        //    {
        //        int count = sequentialDays;
        //        int indexStart = i - sequentialDays + 1;
        //        if (indexStart < 0)
        //        {
        //            count = i + 1;
        //            indexStart = 0;
        //        }

        //        List<double> dryBulbTempartures_Temp = dryBulbTempartures.GetRange(indexStart, count);

        //        result.Add(dryBulbTempartures_Temp.Average());
        //    }

        //    return result;
        //}
    }
}