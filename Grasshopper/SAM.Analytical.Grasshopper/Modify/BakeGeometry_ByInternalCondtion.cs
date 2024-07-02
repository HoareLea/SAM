using Rhino;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper
{
    public static partial class Modify
    {
        public static void BakeGeometry_ByInternalCondition(this RhinoDoc rhinoDoc, global::Grasshopper.Kernel.Data.IGH_Structure gH_Structure, bool cutApertures = false, double tolerance = Core.Tolerance.Distance)
        {
            global::Rhino.DocObjects.Tables.LayerTable layerTable = rhinoDoc?.Layers;
            if (layerTable == null)
                return;

            List<ISpace> spaces = new List<ISpace>();
            foreach (var variable in gH_Structure.AllData(true))
            {
                if (variable is GooSpace)
                {
                    ISpace space = ((GooSpace)variable).Value;
                    if (space == null)
                        continue;

                    Geometry.Spatial.Point3D location = space.Location;
                    if (location == null)
                        continue;

                    spaces.Add(space);
                }
            }

            if (spaces != null && spaces.Count != 0)
            {
                Rhino.Modify.BakeGeometry_ByInternalCondition(rhinoDoc, spaces?.FindAll(x => x is Space)?.Cast<Space>());
            }
        }
    }
}