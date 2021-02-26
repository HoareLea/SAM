using SAM.Geometry.Spatial;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static Dictionary<Shell, List<Space>> DuplicatedSpacesDictionary(this AdjacencyCluster adjacencyCluster)
        {
            List<Space> spaces = adjacencyCluster?.GetSpaces();
            if (spaces == null)
                return null;

            Dictionary<Shell, List<Space>> result = new Dictionary<Shell, List<Space>>();
            if (spaces.Count == 0)
                return result;

            Dictionary<Shell, Space> dictionary = new Dictionary<Shell, Space>();

            foreach(Space space in spaces)
            {
                Shell shell = adjacencyCluster.Shell(space);
                if (shell == null)
                    continue;

                if (space.Location == null)
                    continue;

                dictionary[shell] = space;
            }

            List<Shell> shells = dictionary.Keys.ToList();
            while (shells.Count > 0)
            {
                Shell shell = shells[0];
                Space space = dictionary[shell];
                Point3D location = space.Location;

                List<Shell> shells_Space = SpaceShells(dictionary.Keys, location);

                shells.RemoveAt(0);

                if (shells_Space == null || shells_Space.Count < 2)
                    continue;

                shells_Space.ForEach(x => shells.Remove(x));

                List<Space> spaces_Space = shells_Space.ConvertAll(x => dictionary[x]);

                result[shell] = spaces_Space;
            }

            return result;
        }
    }
}