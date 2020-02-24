namespace SAM.Core
{
    public class SAMRelation : ISAMRelation
    {
        private object @object;
        private object relatedObject;

        public SAMRelation(object @object, object relatedObject)
        {
            this.@object = @object;
            this.relatedObject = relatedObject;

        }

        public SAMRelation(SAMRelation sAMRelation)
        {
            this.@object = sAMRelation.@object;
            this.relatedObject = sAMRelation.relatedObject;

        }

        public object Object
        {
            get
            {
                return @object;
            }
        }

        public object RelatedObject
        {
            get
            {
                return RelatedObject;
            }
        }

        public override bool Equals(object @object)
        {
            //Check for null and compare run-time types.
            if ((@object == null) || !this.GetType().Equals(@object.GetType()))
            {
                return false;
            }
            else if (@object is SAMRelation)
            {
                SAMRelation sAMRelation = (SAMRelation)@object;
                return ReferenceEquals(this.@object, sAMRelation.@object) && ReferenceEquals(this.relatedObject, sAMRelation.relatedObject);
            }

            return false;
        }

        public override int GetHashCode()
        {
            int hash = 13;
            hash = (hash * 7) + (!ReferenceEquals(null, @object) ? @object.GetHashCode() : 0);
            hash = (hash * 7) + (!ReferenceEquals(null, relatedObject) ? relatedObject.GetHashCode() : 0);
            return hash;
        }

        public T GetObject<T>()
        {
            if (@object == null)
                return default;
            
            if (typeof(T).IsAssignableFrom(@object.GetType()))
                return (T)(object)@object;

            return default;
        }

        public T GetRelatedObject<T>()
        {
            if (relatedObject == null)
                return default;

            if (typeof(T).IsAssignableFrom(relatedObject.GetType()))
                return (T)(object)relatedObject;

            return default;
        }
    }
}
