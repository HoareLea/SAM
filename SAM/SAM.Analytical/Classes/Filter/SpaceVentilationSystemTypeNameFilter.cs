using Newtonsoft.Json.Linq;
using SAM.Core;

namespace SAM.Analytical
{
    public class SpaceVentilationSystemTypeNameFilter : SpaceMechanicalSystemTypeNameFilter<VentilationSystem>
    {       
        public SpaceVentilationSystemTypeNameFilter(TextComparisonType textComparisonType, string value)
            : base(textComparisonType, value)
        {

        }

        public SpaceVentilationSystemTypeNameFilter(SpaceVentilationSystemTypeNameFilter spaceVentilationSystemTypeNameFilter)
            : base(spaceVentilationSystemTypeNameFilter)
        {

        }

        public SpaceVentilationSystemTypeNameFilter(JObject jObject)
            : base(jObject)
        {

        }
    }
}