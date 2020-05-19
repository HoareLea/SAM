using Grasshopper;
using Grasshopper.Kernel.Data;

using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public static partial class Query
    {
        public static System.Drawing.Color Color(this PanelType panelType, bool internalEdges = false)
        {
            if(internalEdges)
            {
                switch (panelType)
                {
                    case PanelType.Wall:
                        return System.Drawing.Color.Gray;
                    default:
                        return System.Drawing.Color.Empty;
                }
            }
            else
            //geometry external edges
            {
                switch (panelType)
                {
                    case PanelType.Wall:
                        return System.Drawing.Color.Red;
                    case PanelType.WallExternal:
                        return System.Drawing.Color.Red;
                    case PanelType.WallInternal:
                        return System.Drawing.Color.Blue;
                    case PanelType.Floor:
                        return System.Drawing.Color.Blue;
                    default:
                        return System.Drawing.Color.Empty;
                }
            }
        }

        public static System.Drawing.Color Color(this ApertureType apertureType, bool internalEdges = false)
        {
            if (internalEdges)
            {
                switch (apertureType)
                {
                    case ApertureType.Door:
                        return System.Drawing.Color.Violet;
                    case ApertureType.Window:
                        return System.Drawing.Color.Violet;
                    default:
                        return System.Drawing.Color.Empty;
                }
            }
            else
            {
                switch (apertureType)
                {
                    case ApertureType.Door:
                        return System.Drawing.Color.Violet;
                    case ApertureType.Window:
                        return System.Drawing.Color.Violet;
                    default:
                        return System.Drawing.Color.Empty;
                }
            }
        }
    }
}