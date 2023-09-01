using System;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static Dictionary<string, Space> UpdateSpaceNames(this AdjacencyCluster adjacencyCluster, string format = "{0} {1}")
        {
            List<Space> spaces = adjacencyCluster?.GetSpaces();
            if(spaces == null || spaces.Count == 0)
            {
                return null;
            }

            List<Guid> guids = spaces.ConvertAll(x => x.Guid);

            Dictionary<string, Space> result = new Dictionary<string, Space>();

            foreach (Guid guid in guids)
            {
                spaces = adjacencyCluster.GetSpaces();
                
                int spaceIndex = spaces.FindIndex(x => x.Guid == guid);
                if(spaceIndex == -1)
                {
                    continue;
                }

                Space space = spaces[spaceIndex];
                spaces.RemoveAt(spaceIndex);

                string name = space.Name;
                if(name == null)
                {
                    continue;
                }

                List<Space> spaces_Name = spaces.FindAll(x => x.Name == name);
                if(spaces_Name == null || spaces_Name.Count == 0)
                {
                    continue;
                }

                int index = 1;
                foreach (Space space_Name in spaces_Name)
                {
                    spaces = adjacencyCluster.GetSpaces();

                    string name_New = string.Format(format, name, index.ToString());
                    while (spaces.Find(x => x.Name == name_New) != null)
                    {
                        index++;
                        name_New = string.Format(format, name, index.ToString());
                    }

                    Space space_New = new Space(space_Name, name_New, space_Name.Location);
                    adjacencyCluster.AddObject(space_New);
                    result[name_New] = space_New;
                }
            }
            return result;

        }
    }
}