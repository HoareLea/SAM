using SAM.Core;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static List<Panel> ExternalPanels(this RelationCluster relationCluster)
        {
            if (!relationCluster.Contains(typeof(Panel)))
                return null;

            List<Panel> result = new List<Panel>();
            foreach(Panel panel in relationCluster.GetObjects<Panel>())
            {
                if (relationCluster.ExternalPanel(panel))
                    result.Add(panel);
            }

            return result;
        }
    }
}