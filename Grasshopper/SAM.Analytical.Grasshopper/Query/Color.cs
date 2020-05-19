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
                        return System.Drawing.Color.Red;
                    case PanelType.Floor:
                        return System.Drawing.Color.Blue;
                    default:
                        return System.Drawing.Color.Empty;
                }
            }
            else
            {
                switch (panelType)
                {
                    case PanelType.Wall:
                        return System.Drawing.Color.Red;
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
                        return System.Drawing.Color.Red;
                    case ApertureType.Window:
                        return System.Drawing.Color.Blue;
                    default:
                        return System.Drawing.Color.Empty;
                }
            }
            else
            {
                switch (apertureType)
                {
                    case ApertureType.Door:
                        return System.Drawing.Color.Red;
                    case ApertureType.Window:
                        return System.Drawing.Color.Blue;
                    default:
                        return System.Drawing.Color.Empty;
                }
            }
        }
    }
}