using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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
    }
}
