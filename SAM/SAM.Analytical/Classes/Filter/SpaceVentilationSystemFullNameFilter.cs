using Newtonsoft.Json.Linq;
using SAM.Core;

namespace SAM.Analytical
{
    public class SpaceVentilationSystemFullNameFilter : SpaceMechanicalSystemFullNameFilter<VentilationSystem>
    {       
        public SpaceVentilationSystemFullNameFilter(TextComparisonType textComparisonType, string value)
            : base(textComparisonType, value)
        {

        }

        public SpaceVentilationSystemFullNameFilter(SpaceVentilationSystemFullNameFilter spaceVentilationSystemFullNameFilter)
            : base(spaceVentilationSystemFullNameFilter)
        {

        }

        public SpaceVentilationSystemFullNameFilter(JObject jObject)
            : base(jObject)
        {

        }
    }
}