using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace SAM.Core
{
    public abstract class ComplexReferenceFilter : Filter, IRelationClusterFilter
    {
        public RelationCluster RelationCluster { get; set; }

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
            RelationCluster = complexReferenceFilter?.RelationCluster;
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
            //if(ComplexReference == null || RelationCluster == null)
            //{
            //    return false;
            //}

            //System.Guid guid = RelationCluster.GetGuid(jSAMObject);
            //if(guid == null || guid == System.Guid.Empty)
            //{
            //    return false;
            //}

            //ObjectReference objectReference_First = null;
            //if(ComplexReference is ObjectReference)
            //{
            //    objectReference_First = (ObjectReference)ComplexReference;
            //}
            //if(ComplexReference is PathReference)
            //{
            //    PathReference pathReference = (PathReference)ComplexReference;
            //    if(pathReference.Count() != 0)
            //    {
            //        objectReference_First = pathReference.First();
            //    }
            //}

            //if(objectReference_First == null)
            //{
            //    return false;
            //}

            //ObjectReference objectReference_Temp = objectReference_First;
            //if (objectReference_Temp is PropertyReference)
            //{
            //    objectReference_Temp = new ObjectReference(objectReference_Temp);
            //}

            //List<object> objects = RelationCluster.GetValues(objectReference_Temp);
            //if(objects == null || objects.Count == 0)
            //{
            //    return false;
            //}

            //if(objects.Find(x => RelationCluster.GetGuid(x) == guid) == null)
            //{
            //    return false;
            //}

            //if(objectReference_First is PropertyReference)
            //{
            //    PropertyReference propertyReference = (PropertyReference)objectReference_First;
            //    objectReference_First = new PropertyReference(propertyReference.TypeName, new Reference(guid), propertyReference.PropertyName);

            //}
            //else if (objectReference_First is ObjectReference)
            //{
            //    objectReference_Temp = (ObjectReference)objectReference_First;
            //    objectReference_First = new ObjectReference(objectReference_Temp.TypeName, new Reference(guid));
            //}

            //IComplexReference complexReference = objectReference_First;
            //if (ComplexReference is PathReference)
            //{
            //    PathReference pathReference_Temp = (PathReference)ComplexReference;
            //    List<ObjectReference> objectReferences = new List<ObjectReference>(pathReference_Temp);
            //    objectReferences[0] = objectReference_First;

            //    complexReference = new PathReference(objectReferences);
            //}

            //List<object> values = RelationCluster.GetValues(complexReference);

            if (RelationCluster == null || !RelationCluster.TryGetValues(jSAMObject, ComplexReference, out List<object> values))
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