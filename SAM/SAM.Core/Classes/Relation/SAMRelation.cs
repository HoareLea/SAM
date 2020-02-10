using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAM.Core
{
    public class SAMRelation<T, Y> : ISAMRelation
    {
        private T @object;
        private Y relatedObject;

        public SAMRelation(T @object, Y relatedObject)
        {
            this.@object = @object;
            this.relatedObject = relatedObject;

        }

        public SAMRelation(ISAMRelation sAMRelation)
        {
            this.@object = sAMRelation.GetObject<T>();
            this.relatedObject = sAMRelation.GetRelatedObject<Y>();

        }

        public T Object
        {
            get
            {
                return @object;
            }
        }

        public Y RelatedObject
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
            else if (@object is ISAMRelation)
            {
                ISAMRelation sAMRelation = (ISAMRelation)@object;
                return ReferenceEquals(this.@object, sAMRelation.GetObject<T>()) && ReferenceEquals(this.relatedObject, sAMRelation.GetRelatedObject<Y>());
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

        public Z GetObject<Z>()
        {
            if (typeof(T).IsAssignableFrom(typeof(Z)))
                return (Z)(object)@object;

            return default;
        }

        public Z GetRelatedObject<Z>()
        {
            if (typeof(Y).IsAssignableFrom(typeof(Z)))
                return (Z)(object)relatedObject;

            return default;
        }
    }
}
