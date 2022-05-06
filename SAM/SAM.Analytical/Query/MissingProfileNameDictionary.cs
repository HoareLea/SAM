using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static Dictionary<ProfileType, List<string>> MissingProfileNameDictionary(this AnalyticalModel analyticalModel)
        {
            if(analyticalModel == null)
            {
                return null;
            }

            return MissingProfileNameDictionary(analyticalModel.ProfileLibrary, analyticalModel.AdjacencyCluster?.GetInternalConditions(true, true));
        }

        public static Dictionary<ProfileType, List<string>> MissingProfileNameDictionary(this ProfileLibrary profileLibrary, IEnumerable<InternalCondition> internalConditions)
        {
            if(profileLibrary == null || internalConditions == null)
            {
                return null;
            }

            Dictionary<ProfileType, List<string>> result = new Dictionary<ProfileType, List<string>>();
            foreach(InternalCondition internalCondition in internalConditions)
            {
                Dictionary<ProfileType, string> dictionary_Temp = MissingProfileNameDictionary(profileLibrary, internalCondition);
                if(dictionary_Temp == null || dictionary_Temp.Count == 0)
                {
                    continue;
                }

                foreach(KeyValuePair<ProfileType, string> keyValuePair in dictionary_Temp)
                {
                    if (!result.TryGetValue(keyValuePair.Key, out List<string> names))
                    {
                        names = new List<string>();
                        result[keyValuePair.Key] = names;
                    }

                    if (!names.Contains(keyValuePair.Value))
                    {
                        names.Add(keyValuePair.Value);
                    }
                }
            }

            return result;
        }

        public static Dictionary<ProfileType, string> MissingProfileNameDictionary(this ProfileLibrary profileLibrary, InternalCondition internalCondition)
        {
            if(profileLibrary == null || internalCondition == null)
            {
                return null;
            }

            Dictionary<ProfileType, string> dictionary = internalCondition.GetProfileTypeDictionary();
            if(dictionary == null)
            {
                return null;
            }

            Dictionary<ProfileType, string> result = new Dictionary<ProfileType, string>();
            foreach (KeyValuePair<ProfileType, string> keyValuePair in dictionary)
            {
                if(string.IsNullOrEmpty(keyValuePair.Value))
                {
                    continue;
                }

                Profile profile = internalCondition.GetProfile(keyValuePair.Key, profileLibrary);
                if(profile != null)
                {
                    continue;
                }

                result[keyValuePair.Key] = keyValuePair.Value;
            }

            return result;
        }
    }
}