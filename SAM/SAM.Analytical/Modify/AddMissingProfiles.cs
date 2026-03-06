// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static List<Profile> AddMissingProfiles(this AnalyticalModel analyticalModel, ProfileLibrary profileLibrary)
        {
            return AddMissingProfiles(analyticalModel, profileLibrary, out List<string> missiingProfileNames);
        }


        public static List<Profile> AddMissingProfiles(this AnalyticalModel analyticalModel, ProfileLibrary profileLibrary, out List<string> missingProfileNames)
        {
            missingProfileNames = null;

            if (analyticalModel == null || profileLibrary == null)
            {
                return null;
            }

            List<Profile> result = new List<Profile>();

            Dictionary<ProfileType, HashSet<string>> profileDatas = analyticalModel.GetMissingProfileDatas();
            if (profileDatas == null || profileDatas.Count == 0)
            {
                return result;
            }

            missingProfileNames = new List<string>();
            foreach (KeyValuePair<ProfileType, HashSet<string>> keyValuePair in profileDatas)
            {
                foreach(string profileName in keyValuePair.Value)
                {
                    if (string.IsNullOrWhiteSpace(profileName))
                    {
                        continue;
                    }

                    Profile profile = profileLibrary.GetProfile(profileName, keyValuePair.Key);
                    if(profile is null)
                    {
                        profile = profileLibrary.GetProfile(profileName, keyValuePair.Key.ProfileGroup());
                    }

                    if(profile is null)
                    {
                        profile = profileLibrary.GetProfiles(profileName)?.FirstOrDefault();
                    }

                    if (profile is null)
                    {
                        missingProfileNames.Add(profileName);
                    }
                    else
                    {
                        result.Add(profile);
                        analyticalModel.AddProfile(profile);
                    }
                }
            }

            return result;
        }
    }
}
