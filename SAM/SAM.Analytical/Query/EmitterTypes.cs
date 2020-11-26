using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static List<EmitterType> EmitterTypes(this EmitterCategory emitterCategory)
        {
            if (emitterCategory == Analytical.EmitterCategory.Undefined)
                return null;

            List<EmitterType> result = new List<EmitterType>();
            foreach (EmitterType emitterType in System.Enum.GetValues(typeof(EmitterType)))
            {
                if (emitterType.EmitterCategory() == emitterCategory)
                    result.Add(emitterType);
            }
            return result;
        }
    }
}