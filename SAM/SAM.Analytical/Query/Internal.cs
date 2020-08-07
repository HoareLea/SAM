using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static bool Internal(this PanelType panelType)
        {
            switch(panelType)
            {
                case Analytical.PanelType.FloorInternal:
                case Analytical.PanelType.Air:
                case Analytical.PanelType.Ceiling:
                case Analytical.PanelType.UndergroundCeiling:
                case Analytical.PanelType.WallInternal:
                    return true;

                default:
                    return false;
            }
        }
    }
}