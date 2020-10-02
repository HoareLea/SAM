using System;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static IEnumerable<Construction> Constructions(this IEnumerable<Panel> panels)
        {
            if (panels == null)
                return null;
            
            Dictionary<Guid, Construction> dictionary = new Dictionary<Guid, Construction>();

            foreach (Panel panel in panels)
            {
                if (dictionary.ContainsKey(panel.SAMTypeGuid))
                    continue;

                Construction construction = panel.Construction;
                if (construction == null)
                    continue;

                dictionary[construction.Guid] = construction;
            }

            return dictionary.Values;
        }
    }
}