namespace SAM.Analytical
{
    public static partial class Query
    {
        public static string UniqueName(this Panel panel, int id = -1)
        {
            if (panel == null)
                return null;

            string name = panel.Name;
            if (string.IsNullOrEmpty(name))
            {
                name = panel.Construction?.Name;
                if (string.IsNullOrEmpty(name))
                    return null;
            }

            return UniqueName(panel.PanelType, name, id);
        }

        public static string UniqueName(this Aperture aperture, int id = -1)
        {
            if (aperture == null)
                return null;

            ApertureConstruction apertureConstruction = aperture.ApertureConstruction;

            string name = aperture.Name;
            if (string.IsNullOrEmpty(name))
            {
                name = apertureConstruction?.Name;
                if (string.IsNullOrEmpty(name))
                    return null;
            }

            ApertureType apertureType = Analytical.ApertureType.Undefined;
            if (apertureConstruction != null)
                apertureType = apertureConstruction.ApertureType;

            return UniqueName(apertureType, name, id);
        }

        public static string UniqueName(this PanelType panelType, string name, int id = -1)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            string prefix = null;
            switch (panelType)
            {
                case Analytical.PanelType.Ceiling:
                    prefix = "Compound Ceiling";
                    break;
                case Analytical.PanelType.CurtainWall:
                    prefix = "Curtain Wall";
                    break;
                case Analytical.PanelType.Floor:
                case Analytical.PanelType.FloorExposed:
                case Analytical.PanelType.FloorInternal:
                case Analytical.PanelType.FloorRaised:
                case Analytical.PanelType.SlabOnGrade:
                case Analytical.PanelType.UndergroundSlab:
                case Analytical.PanelType.UndergroundCeiling:
                    prefix = "Floor";
                    break;
                case Analytical.PanelType.Roof:
                case Analytical.PanelType.Shade:
                case Analytical.PanelType.SolarPanel:
                    prefix = "Basic Roof";
                    break;
                case Analytical.PanelType.UndergroundWall:
                case Analytical.PanelType.Wall:
                case Analytical.PanelType.WallExternal:
                case Analytical.PanelType.WallInternal:
                    prefix = "Basic Wall";
                    break;
                case Analytical.PanelType.Air:
                    prefix = "Air";
                    break;
                default:
                    prefix = "Undefined";
                    break;
            }

            return UniqueName(prefix, name, id);
        }

        public static string UniqueName(this ApertureType apertureType, string name, int id = -1)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            string prefix = null;
            switch (apertureType)
            {
                case Analytical.ApertureType.Window:
                    prefix = "Window";
                    break;
                case Analytical.ApertureType.Door:
                    prefix = "Door";
                    break;
                case Analytical.ApertureType.Undefined:
                    prefix = "Undefined";
                    break;
            }

            return UniqueName(prefix, name, id);
        }

        private static string UniqueName(string prefix, string name, int id = -1)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;
            
            string result = prefix;
            if (result == null)
                return null;

            result += ": ";

            if (!name.StartsWith(result))
                result += name;
            else
                result = name;

            result.Trim();

            if (id != -1)
                result += string.Format(" [{0}]", id);

            if (result.EndsWith(":"))
                result = result.Substring(0, result.Length - 1);

            return result;
        }
    }
}