using Newtonsoft.Json.Linq;
using SAM.Core;

namespace SAM.Analytical
{
    public class SpaceCoolingSystemFullNameFilter : SpaceMechanicalSystemFullNameFilter<CoolingSystem>
    {       
        public SpaceCoolingSystemFullNameFilter(TextComparisonType textComparisonType, string value)
            : base(textComparisonType, value)
        {

        }

        public SpaceCoolingSystemFullNameFilter(SpaceCoolingSystemFullNameFilter spaceCoolingSystemFullNameFilter)
            : base(spaceCoolingSystemFullNameFilter)
        {

        }

        public SpaceCoolingSystemFullNameFilter(JObject jObject)
            : base(jObject)
        {

        }
    }
}