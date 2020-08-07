using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static bool UpdateConstructionByName(this Panel panel, IEnumerable<Construction> constructions)
        {
            if (panel == null || constructions == null)
                return false;

            string constructionName = panel.Construction?.Name;
            if (string.IsNullOrWhiteSpace(constructionName))
                return false;

            foreach(Construction construction in constructions)
            {
                if (!constructionName.Equals(construction?.Name))
                    continue;

                throw new NotImplementedException();
            }

            return false;
        }
    }
}