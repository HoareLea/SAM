using SAM.Core;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static bool InternalPanel(this RelationCluster relationCluster, Panel panel)
        {
            if (panel == null || relationCluster == null)
                return false;

            if (!relationCluster.Contains(typeof(Panel)))
                return false;

            List<Space> spaces = relationCluster.GetRelatedObjects<Space>(panel);
            return spaces != null && spaces.Count > 1;
        }
    }
}