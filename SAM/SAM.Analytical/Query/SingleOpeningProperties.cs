using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static ISingleOpeningProperties SingleOpeningProperties(this MultipleOpeningProperties multipleOpeningProperties)
        {
            List<ISingleOpeningProperties> singleOpeningProperties = multipleOpeningProperties?.SingleOpeningProperties;
            if (singleOpeningProperties == null || singleOpeningProperties.Count == 0)
            {
                return null;
            }

            List<Tuple<double, ISingleOpeningProperties>> tuples = new List<Tuple<double, ISingleOpeningProperties>>();
            foreach (ISingleOpeningProperties singleOpeningProperties_Temp in singleOpeningProperties)
            {
                if (singleOpeningProperties_Temp == null)
                {
                    continue;
                }

                double dischargeCoefficient = singleOpeningProperties_Temp.GetDischargeCoefficient();
                if (double.IsNaN(dischargeCoefficient))
                {
                    continue;
                }

                double factor = singleOpeningProperties_Temp.GetFactor();
                if (double.IsNaN(factor))
                {
                    continue;
                }

                if (singleOpeningProperties_Temp is ProfileOpeningProperties)
                {
                    ProfileOpeningProperties profileOpeningProperties = singleOpeningProperties_Temp as ProfileOpeningProperties;
                    double[] values = profileOpeningProperties.Profile?.GetValues();
                    if (values != null && values.Length > 0)
                    {
                        IEnumerable<double> uniqueValues = values.ToList().Distinct();
                        if (uniqueValues.Count() == 1 && uniqueValues.ElementAt(0) == 0)
                        {
                            continue;
                        }
                    }
                }

                tuples.Add(new Tuple<double, ISingleOpeningProperties>(factor * dischargeCoefficient, singleOpeningProperties_Temp));
            }

            if (tuples == null || tuples.Count == 0)
            {
                return null;
            }

            tuples.Sort((x, y) => y.Item1.CompareTo(x.Item1));

            return tuples[0].Item2;
        }
    }
}