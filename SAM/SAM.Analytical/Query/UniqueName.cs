namespace SAM.Analytical
{
    public static partial class Query
    {
        public static string UniqueName(this AdjacencyCluster adjacencyCluster, Panel panel)
        {
            if(panel == null || adjacencyCluster == null)
            {
                return null;
            }

            int index = adjacencyCluster.GetIndex(panel);
            if(index == -1)
            {
                return null;
            }

            string name = panel.Name;
            if(string.IsNullOrEmpty(name))
            {
                return index.ToString();
            }

            return UniqueName(panel, index);
        }

        public static string UniqueName(this AdjacencyCluster adjacencyCluster, Aperture aperture)
        {
            if (aperture == null || adjacencyCluster == null)
            {
                return null;
            }

            int index = adjacencyCluster.GetIndex(aperture);
            if (index == -1)
            {
                return null;
            }

            string name = aperture.Name;
            if (string.IsNullOrEmpty(name))
            {
                return index.ToString();
            }

            return UniqueName(aperture, index);
        }

        public static string UniqueName(this IPanel panel, int id = -1)
        {
            if(panel == null)
            {
                return null;
            }

            return UniqueName((dynamic)panel, id);
        }

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

            return UniqueName(panel.PanelType, name, panel.Guid, id);
        }

        public static string UniqueName(this ExternalPanel externalPanel, int id = -1)
        {
            if (externalPanel == null)
                return null;

            string name = externalPanel.Name;
            if (string.IsNullOrEmpty(name))
            {
                name = externalPanel.Construction?.Name;
                if (string.IsNullOrEmpty(name))
                    return null;
            }

            return UniqueName(typeof(ExternalPanel).Name, name, externalPanel.Guid, id);
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

            return UniqueName(apertureType, name, aperture.Guid, id);
        }

        public static string UniqueName(this Construction construction, int id = -1)
        {
            if (construction == null)
                return null;

            string name = construction.Name;
            if (string.IsNullOrEmpty(name))
                return null;

            return UniqueName(construction.PanelType(), name, construction.Guid, id);
        }

        public static string UniqueName(this ApertureConstruction apertureCondtruction, int id = -1)
        {
            if (apertureCondtruction == null)
                return null;

            string name = apertureCondtruction.Name;
            if (string.IsNullOrEmpty(name))
                return null;

            return UniqueName(apertureCondtruction.ApertureType, name, apertureCondtruction.Guid, id);
        }

        public static string UniqueName(this PanelType panelType, string name, System.Guid? guid = null, int id = -1)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            return UniqueName(UniqueNamePrefix(panelType), name, guid, id);
        }

        public static string UniqueName(this ApertureType apertureType, string name, System.Guid? guid = null, int id = -1)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            return UniqueName(UniqueNamePrefix(apertureType), name, guid, id);
        }

        public static string UniqueName(string prefix, string name, System.Guid? guid = null, int id = -1)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            string result = null;
            if (!string.IsNullOrWhiteSpace(prefix) && !name.StartsWith(prefix))
            {
                result = string.Format("{0}: {1}", prefix, name);
            }


            if (result == null)
            {
                result = name;
            }

            result.Trim();

            if (guid != null && guid.HasValue)
            {
                result += string.Format(" [{0}]", guid);
            }

            if (id != -1)
            {
                result += string.Format(" [{0}]", id);
            }

            if (result.EndsWith(":"))
            {
                result = result.Substring(0, result.Length - 1);
            }

            return result;
        }
    }
}