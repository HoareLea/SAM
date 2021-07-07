using Rhino.DocObjects;

namespace SAM.Core.Grasshopper
{
    public static partial class Modify
    {
        public static bool SetUserStrings(this ObjectAttributes objectAttributes, SAMObject sAMObject)
        {
            if (objectAttributes == null || sAMObject == null)
                return false;

            foreach(string name in Core.Query.Names(sAMObject))
            {
                if (sAMObject.TryGetValue(name, out string value, true))
                    objectAttributes.SetUserString(name, value);
            }

            return true;
        }
    }
}