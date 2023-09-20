using Newtonsoft.Json.Linq;
using SAM.Core;

namespace SAM.Analytical
{
    public class SpaceHeatingSystemFullNameFilter : SpaceMechanicalSystemFullNameFilter<HeatingSystem>
    {       
        public SpaceHeatingSystemFullNameFilter(TextComparisonType textComparisonType, string value)
            : base(textComparisonType, value)
        {

        }

        public SpaceHeatingSystemFullNameFilter(SpaceHeatingSystemFullNameFilter spaceHeatingSystemFullNameFilter)
            : base(spaceHeatingSystemFullNameFilter)
        {

        }

        public SpaceHeatingSystemFullNameFilter(JObject jObject)
            : base(jObject)
        {

        }
    }
}