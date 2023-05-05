using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;

namespace SAM.Core
{
    /// <summary>
    /// A collection of Guids that inherits from SAMObject and implements the IEnumerable interface.
    /// </summary>
    public class GuidCollection : SAMObject, IEnumerable<Guid>
    {
        private List<Guid> guids = new List<Guid>();

        /// <summary>
        /// Constructor for a GuidCollection with a Guid and a name.
        /// </summary>
        public GuidCollection(Guid guid, string name)
            : base(guid, name)
        {

        }

        /// <summary>
        /// Constructor for a GuidCollection with only a name.
        /// </summary>
        public GuidCollection(string name)
            : base(name)
        {

        }

        /// <summary>
        /// Constructor for a GuidCollection with a name and a ParameterSet.
        /// </summary>
        public GuidCollection(string name, ParameterSet parameterSet)
            : base(Guid.NewGuid(), name, new ParameterSet[] { parameterSet })
        {

        }

        /// <summary>
        /// Constructor for a GuidCollection from a JObject.
        /// </summary>
        public GuidCollection(JObject jObject)
        {
            FromJObject(jObject);
        }

        /// <summary>
        /// Constructor for a GuidCollection from another GuidCollection.
        /// </summary>
        public GuidCollection(GuidCollection guidCollection)
            : base(guidCollection)
        {

            if (guidCollection?.guids != null)
                guids = new List<Guid>(guidCollection.guids);
        }

        /// <summary>
        /// Default constructor for a GuidCollection.
        /// </summary>
        public GuidCollection()
            : base()
        {

        }

        /// <summary>
        /// Constructor for a GuidCollection from an IEnumerable of Guids.
        /// </summary>
        public GuidCollection(IEnumerable<Guid> guids)
            : base()
        {
            foreach (Guid guid in guids)
                this.guids.Add(guid);
        }

        /// <summary>
        /// Adds a Guid to the GuidCollection.
        /// </summary>
        public virtual void Add(Guid guid)
        {
            guids.Add(guid);
        }

        /// <summary>
        /// Removes a Guid from the GuidCollection.
        /// </summary>
        public bool Remove(Guid guid)
        {
            return guids.Remove(guid);
        }

        /// <summary>
        /// Removes a collection of Guids from the GuidCollection.
        /// </summary>
        public List<bool> Remove(IEnumerable<Guid> guids)
        {
            if (guids == null)
                return null;

            List<bool> result = new List<bool>();

            foreach (Guid guid in guids)
                result.Add(Remove(guid));

            return result;
        }

        /// <summary>
        /// Overrides the FromJObject method from SAMObject to populate the GuidCollection from a JObject.
        /// </summary>
        public new virtual bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
                return false;

            if (jObject.ContainsKey("Collection"))
            {
                guids = new List<Guid>();
                foreach (JToken jToken in jObject.Value<JArray>("Collection"))
                    guids.Add(Query.Guid(jToken));
            }

            return true;
        }

        /// <summary>
        /// Overrides the ToJObject method from SAMObject to create a JObject from the GuidCollection.
        /// </summary>
        public new virtual JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
                return null;

            JArray jArray = new JArray();
            foreach (Guid guid in this)
                jArray.Add(guid);

            jObject.Add("Collection", jArray);

            return jObject;
        }

        /// <summary>
        /// Implements the IEnumerable interface for the GuidCollection.
        /// </summary>
        public IEnumerator<Guid> GetEnumerator()
        {
            return guids?.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return guids?.GetEnumerator();
        }
    }
}
