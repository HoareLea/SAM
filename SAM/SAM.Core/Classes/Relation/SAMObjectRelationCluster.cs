using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SAM.Core
{
    public class SAMObjectRelationCluster<T> : RelationCluster_NEW<IJSAMObject>, IJSAMObject where T: IJSAMObject
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
                List<Type> types = GetTypes();
                if (types != null)
                {
                    foreach(Type type in types)
                    {
                        List<IJSAMObject> jSAMObjects = GetObjects(type);
                        if(jSAMObjects != null)
                        {
                            foreach(IJSAMObject jSAMObject in jSAMObjects)
                            {
                                AddObject(jSAMObject.Clone());
                            }
                        }
                    }
                }
            }
        }

        public SAMObjectRelationCluster<T> Clone(bool deepClone)
        {
            return new SAMObjectRelationCluster<T> (this, deepClone);
        }
    }
}
