using System;
using System.Collections.Generic;
using System.Drawing;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static Dictionary<Guid, Color> AssignSpaceColors(this AdjacencyCluster adjacencyCluster)
        {
            List<Space> spaces = adjacencyCluster?.GetSpaces();
            if(spaces == null || spaces.Count == 0)
            {
                return null;
            }
            Dictionary<string, Tuple<Color, List<Space>>> dictionary = new Dictionary<string, Tuple<Color, List<Space>>>();
            foreach(Space space in spaces)
            {
                InternalCondition internalCondition = space?.InternalCondition;
                if(internalCondition == null)
                {
                    continue;
                }

                if (!internalCondition.TryGetValue(InternalConditionParameter.Color, out Core.SAMColor sAMColor) || sAMColor == null)
                {
                    continue;
                }

                string name = internalCondition.Name;
                if(string.IsNullOrWhiteSpace(name))
                {
                    continue;
                }

                if(!dictionary.TryGetValue(name, out Tuple<Color, List<Space>> tuple) || tuple == null)
                {
                    tuple = new Tuple<Color, List<Space>>(sAMColor.ToColor(), new List<Space>());
                    dictionary[name] = tuple;
                }

                tuple.Item2.Add(space);
            }

            Dictionary<Guid, Color> result = new Dictionary<Guid, Color>();
            foreach (KeyValuePair<string, Tuple<Color, List<Space>>> keyValuePair in dictionary)
            {
                Color color = keyValuePair.Value.Item1;
                List<Space> spaces_Color = keyValuePair.Value.Item2;

                List<Color> colors = Core.Create.Colors(color, spaces_Color.Count, 0, 0.8);
                if(colors == null || colors.Count != spaces_Color.Count)
                {
                    continue;
                }

                for(int i = 0; i < colors.Count; i++)
                {
                    spaces_Color[i].SetValue(SpaceParameter.Color, colors[i]);
                    result[spaces_Color[i].Guid] = colors[i];
                }
            }

            return result;
        }
    }
}