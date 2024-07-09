using Rhino;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper
{
    public static partial class Modify
    {
        public static void BakeGeometry_ByLevel(this RhinoDoc rhinoDoc, global::Grasshopper.Kernel.Data.IGH_Structure gH_Structure, bool cutApertures = false, double tolerance = Core.Tolerance.Distance)
        {
            global::Rhino.DocObjects.Tables.LayerTable layerTable = rhinoDoc?.Layers;
            if (layerTable == null)
                return;

            List<Space> spaces = new List<Space>();
            foreach (var variable in gH_Structure.AllData(true))
            {
                if (variable is GooSpace)
                {
                    Space space = ((GooSpace)variable).Value as Space;
                    if (space == null)
                        continue;

                    Geometry.Spatial.Point3D location = space.Location;
                    if (location == null)
                        continue;

                    spaces.Add(space);
                }
            }

            if (spaces != null && spaces.Count != 0)
                Rhino.Modify.BakeGeometry_ByLevel(rhinoDoc, spaces);
        }
    }
}