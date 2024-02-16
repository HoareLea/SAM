using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace SAM.Core
{
    public class IndexedDoubles : IndexedObjects<double>
    {
        public IndexedDoubles()
            :base()
        {

        }

        public IndexedDoubles(JObject jObject)
            :base(jObject)
        {

        }

        public IndexedDoubles(IndexedDoubles indexedDoubles)
            :base(indexedDoubles)
        {

        }

        public IndexedDoubles(IEnumerable<double> values)
            : base(values)
        {

        }

        public IndexedDoubles(IEnumerable<double> values, int startIndex)
            : base(values, startIndex)
        {

        }

        public IndexedDoubles(int startIndex, int count, double value)
            : base(startIndex, count, value)
        {

        }

        public void Sum(IndexedDoubles indexedDoubles)
        {
            IEnumerable<int> keys = indexedDoubles?.Keys;
            if(keys == null)
            {
                return;
            }

            foreach(int index in keys)
            {
                double value = indexedDoubles[index];
                if(double.IsNaN(value) || value == 0)
                {
                    continue;
                }

                this[index] = !TryGetValue(index, out double value_Temp) || double.IsNaN(value_Temp) ? value : value + value_Temp;
            }
        }

        public int GetMaxValueIndex()
        {
            IEnumerable<int> keys = Keys;

            int result = -1;
            double max = double.MinValue;
            foreach(int key in keys)
            {
                double value = this[key];
                if(double.IsNaN(value))
                {
                    continue;
                }

                if(value > max)
                {
                    max = value;
                    result = key;
                }
            }

            return result;
        }

        public int GetMinValueIndex()
        {
            IEnumerable<int> keys = Keys;

            int result = -1;
            double min = double.MaxValue;
            foreach (int key in keys)
            {
                double value = this[key];
                if (double.IsNaN(value))
                {
                    continue;
                }

                if (value < min)
                {
                    min = value;
                    result = key;
                }
            }

            return result;
        }

        public double GetMaxValue()
        {
            int index = GetMaxValueIndex();
            if(index == -1)
            {
                return double.NaN;
            }

            return this[index];
        }

        public double GetMinValue()
        {
            int index = GetMinValueIndex();
            if (index == -1)
            {
                return double.NaN;
            }

            return this[index];
        }

        public double GetSum()
        {
            IEnumerable<int> keys = Keys;
            if(keys == null)
            {
                return double.NaN;
            }


            double result = 0;
            foreach(int key in keys)
            {
                double value = this[key];
                if(double.IsNaN(value))
                {
                    continue;
                }

                result += value;
            }

            return result;
        }

        public double GetAverage()
        {
            return GetSum() / Count;
        }
    }
}
