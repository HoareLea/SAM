using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Markup;

namespace SAM.Core
{
    public abstract class ComplexReferenceFilter : Filter
    {
        private RelationCluster relationCluster;
        private IComplexReference complexReference;

        public ComplexReferenceFilter(JObject jObject)
            : base(jObject)
        {
        }

        public ComplexReferenceFilter()
            : base()
        {
        }

        public ComplexReferenceFilter(ComplexReferenceFilter complexReferenceFilter)
            : base(complexReferenceFilter)
        {

        }
        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
            {
                return false;
            }

            return true;
        }

        public override bool IsValid(IJSAMObject jSAMObject)
        {
            if(complexReference == null || relationCluster == null)
            {
                return false;
            }

            System.Guid guid = relationCluster.GetGuid(jSAMObject);
            if(guid == null || guid == System.Guid.Empty)
            {
                return false;
            }

            ObjectReference objectReference = null;
            if(complexReference is ObjectReference)
            {
                objectReference = (ObjectReference)complexReference;
            }
            if(complexReference is PathReference)
            {
                PathReference pathReference = (PathReference)complexReference;
                if(pathReference.Count() != 0)
                {
                    objectReference = pathReference.First();
                }
            }

            if(objectReference == null)
            {
                return false;
            }

            if(objectReference is PropertyReference)
            {
                objectReference = new ObjectReference(objectReference);
            }

            List<object> objects = Query.Values(objectReference, relationCluster);
            if(objects == null || objects.Count == 0)
            {
                return false;
            }

            if(objects.Find(x => relationCluster.GetGuid(x) == guid) == null)
            {
                return false;
            }

            throw new System.NotImplementedException();
        }

        public override JObject ToJObject()
        {
            JObject result = base.ToJObject();
            if (result == null)
            {
                return result;
            }

            return result;
        }
    }
}