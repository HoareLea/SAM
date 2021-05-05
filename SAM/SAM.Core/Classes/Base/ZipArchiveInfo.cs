using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System;
using System.IO.Compression;

namespace SAM.Core
{
    public class ZipArchiveInfo : IJSAMObject
    {
        public static string EntryName { get; } = string.Format("_{0}", typeof(ZipArchiveInfo).Name);

        HashSet<Guid> guids;
        
        internal ZipArchiveInfo()
        {

        }

        internal ZipArchiveInfo(JObject jObject)
        {
            FromJObject(jObject);
        }

        internal ZipArchiveInfo(ZipArchiveInfo zipArchiveInfo)
        {
            guids = new HashSet<Guid>();
            foreach (Guid guid in zipArchiveInfo.guids)
                guids.Add(guid);
        }

        public Guid NewGuid()
        {
            Guid guid = Guid.NewGuid();

            if (guids == null)
                guids = new HashSet<Guid>();

            while (guids.Contains(guid))
                guid = Guid.NewGuid();

            guids.Add(guid);
            return guid;
        }

        public List<IJSAMObject> OrderedIJSAMObjects(ZipArchive zipArchive)
        {
            if (zipArchive == null || guids == null)
                return null;

            List<IJSAMObject> result = new List<IJSAMObject>();
            foreach (Guid guid in guids)
                result.Add(Create.IJSAMObject(zipArchive.GetEntry(guid.ToString())));

            return result;
        }

        public bool FromJObject(JObject jObject)
        {
            if (jObject == null)
                return false;

            if(jObject.ContainsKey("Guids"))
            {
                JArray jArray = jObject.Value<JArray>("Guids");
                if(jArray != null)
                {
                    guids = new HashSet<Guid>();
                    foreach (JToken jToken in jArray)
                    {
                        Guid guid;
                        if (!Guid.TryParse(jToken.Value<string>(), out guid))
                            continue;

                        guids.Add(guid);
                    }
                }
            }

            return true;
        }

        public JObject ToJObject()
        {
            JObject jObject = new JObject();
            jObject.Add("_type", Query.FullTypeName(this));

            if(guids != null)
            {
                JArray jArray = new JArray(guids);
                jObject.Add("Guids", jArray);
            }

            return jObject;
        }
    }
}