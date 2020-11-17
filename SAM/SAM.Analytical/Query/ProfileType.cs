using System;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static ProfileType ProfileType(this string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return Analytical.ProfileType.Undefined;

            ProfileType result = Analytical.ProfileType.Undefined;
            if (Enum.TryParse(text, out result))
                return result;

            foreach(ProfileType profileType in Enum.GetValues(typeof(ProfileType)))
            {
                if (profileType.Text().Equals(text))
                    return profileType;
            }

            return Analytical.ProfileType.Undefined;
        }
    }
}