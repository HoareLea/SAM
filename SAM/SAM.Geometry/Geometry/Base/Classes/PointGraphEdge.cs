using Newtonsoft.Json.Linq;
using SAM.Core;

namespace SAM.Geometry
{
    public class PointGraphEdge<X, T> : QuickGraph.IEdge<X>, IJSAMObject where T : IJSAMObject where X : IPoint
    {
        private T jSAMObject;
        private X source;
        private X target;

        public PointGraphEdge(T jSAMObject, X source, X target)
        {
            this.jSAMObject = jSAMObject;
            this.source = source;
            this.target = target;
        }

        public PointGraphEdge(JObject jObject)
        {
            FromJObject(jObject);
        }

        public PointGraphEdge(PointGraphEdge<X, T> pointGraphEdge)
        {
            if(pointGraphEdge != null)
            {
                jSAMObject = pointGraphEdge.jSAMObject;
                source = pointGraphEdge.source == null ? default : Core.Query.Clone(pointGraphEdge.source);
                target = pointGraphEdge.target == null ? default : Core.Query.Clone(pointGraphEdge.target);
            }
        }

        public X Source
        {
            get
            {
                return source == null ? default : Core.Query.Clone(source);
            }
        }

        public X Target
        {
            get
            {
                return target == null ? default : Core.Query.Clone(target);
            }
        }

        public T JSAMObject
        {
            get
            {
                return jSAMObject;
            }
        }

        public bool FromJObject(JObject jObject)
        {
            if(jObject == null)
            {
                return false;
            }

            if(jObject.ContainsKey("JSAMObject"))
            {
                jSAMObject = Core.Query.IJSAMObject<T>(jObject.Value<JObject>("JSAMObject"));
            }

            if(jObject.ContainsKey("Source"))
            {
                source = Core.Query.IJSAMObject<X>(jObject.Value<JObject>("Source"));
            }

            if (jObject.ContainsKey("Target"))
            {
                target = Core.Query.IJSAMObject<X>(jObject.Value<JObject>("Target"));
            }

            return true;
        }

        public JObject ToJObject()
        {
            JObject result = new JObject();
            result.Add("_type", Core.Query.FullTypeName(this));

            if (jSAMObject != null)
            {
                result.Add("JSAMObject", jSAMObject.ToJObject());
            }

            if (source != null)
            {
                result.Add("Source", source.ToJObject());
            }

            if (target != null)
            {
                result.Add("Target", target.ToJObject());
            }

            return result;
        }
    }
}