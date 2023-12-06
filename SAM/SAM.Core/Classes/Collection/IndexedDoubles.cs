using Newtonsoft.Json.Linq;
using System.Collections;
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
    }
}
