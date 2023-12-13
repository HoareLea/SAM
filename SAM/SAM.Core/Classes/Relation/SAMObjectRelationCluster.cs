using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SAM.Core
{
    public class SAMObjectRelationCluster<T> : RelationCluster<T>, IJSAMObject, ISAMObjectRelationCluster where T: IJSAMObject
    {
        public SAMObjectRelationCluster()
            :base()
        {

        }

        public SAMObjectRelationCluster(JObject jObject)
            :base(jObject)
        {
        }

        public SAMObjectRelationCluster(SAMObjectRelationCluster<T> sAMObjectRelationCluster)
            : this(sAMObjectRelationCluster, false)
        {

        }

        public SAMObjectRelationCluster(SAMObjectRelationCluster<T> sAMObjectRelationCluster, bool deepClone)
            : base(sAMObjectRelationCluster)
        {
            if(deepClone)
            {
                List<T> objects = GetObjects();
                if (objects != null)
                {
                    foreach (object @object in objects)
                    {
                        if (@object is IJSAMObject)
                        {
                            AddObject(((T)@object).Clone());
                        }
                    }
                }
            }
        }

        public virtual bool TryGetValues(IJSAMObject @object, IComplexReference complexReference, out List<object> values)
        {
            values = null;

            if (!(@object is T))
            {
                return false;
            }

            return base.TryGetValues((T)@object, complexReference, out values);
        }
    }
}
