using SAM.Geometry.Spatial;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static bool UpdateSpace(this BuildingModel buildingModel, Space space, double silverSpacing = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            if (buildingModel == null || space == null)
                return false;

            Point3D point3D = space.Location;
            List<Space> spaces = buildingModel.GetSpaces();
            if (spaces == null || spaces.Count == 0)
                return false;

            Dictionary<Space, Shell> dictionary = new Dictionary<Space, Shell>();
            foreach(Space space_Temp in spaces)
            {
                Shell shell = buildingModel.GetShell(space_Temp);
                if (shell == null)
                    continue;

                if (shell.InRange(point3D, tolerance) || shell.Inside(point3D, tolerance))
                    dictionary[space_Temp] = shell;
            }

            if (dictionary.Count == 0)
                return false;

            Space space_Result = null;

            if(dictionary.Count > 1)
            {
                point3D = point3D.GetMoved(Vector3D.WorldZ * silverSpacing) as Point3D;
                foreach (KeyValuePair<Space, Shell> keyValuePair in dictionary)
                {
                    Shell shell = keyValuePair.Value;
                    if (shell.InRange(point3D, tolerance) || shell.Inside(point3D, tolerance))
                    {
                        space_Result = keyValuePair.Key;
                        break;
                    }
                }
            }
            else
            {
                space_Result = dictionary.Keys.First();
            }


            if (space_Result == null)
                return false;

            List<IPartition> partitions = buildingModel.GetPartitions(space_Result);
            buildingModel.RemoveObject(space_Result);

            buildingModel.Add(space, partitions);
            return true;
        }
    }
}