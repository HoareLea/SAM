using Newtonsoft.Json.Linq;
using SAM.Core;

namespace SAM.Analytical
{
    public class SpaceCoolingSystemTypeNameFilter : SpaceMechanicalSystemTypeNameFilter<CoolingSystem>
    {       
        public SpaceCoolingSystemTypeNameFilter(TextComparisonType textComparisonType, string value)
            : base(textComparisonType, value)
        {

        }

        public SpaceCoolingSystemTypeNameFilter(SpaceCoolingSystemTypeNameFilter spaceCoolingSystemTypeNameFilter)
            : base(spaceCoolingSystemTypeNameFilter)
        {

        }

        public SpaceCoolingSystemTypeNameFilter(JObject jObject)
            : base(jObject)
        {

        }
    }
}