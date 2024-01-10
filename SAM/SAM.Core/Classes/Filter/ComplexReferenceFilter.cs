using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace SAM.Core
{
    public abstract class ComplexReferenceFilter : Filter, ISAMObjectRelationClusterFilter
    {
        public ISAMObjectRelationCluster SAMObjectRelationCluster { get; set; }

        public IComplexReference ComplexReference { get; set; }

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
            ComplexReference = complexReferenceFilter?.ComplexReference?.Clone();
            SAMObjectRelationCluster = complexReferenceFilter?.SAMObjectRelationCluster;
        }
        
        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
            {
                return false;
            }

            if (jObject.ContainsKey("ComplexReference"))
            {
                ComplexReference = Query.IJSAMObject<IComplexReference>(jObject.Value<JObject>("ComplexReference"));
            }

            return true;
        }

        public override bool IsValid(IJSAMObject jSAMObject)
        {

            if (SAMObjectRelationCluster == null || !SAMObjectRelationCluster.TryGetValues(jSAMObject, ComplexReference, out List<object> values))
            {
                return false;
            }

            bool result = IsValid(values);
            if(Inverted)
            {
                result = !result;
            }

            return result;
        }

        protected abstract bool IsValid(IEnumerable<object> values);

        public override JObject ToJObject()
        {
            JObject result = base.ToJObject();
            if (result == null)
            {
                return result;
            }

            if(ComplexReference != null)
            {
                result.Add("ComplexReference", ComplexReference.ToJObject());
            }

            return result;
        }
    }
}