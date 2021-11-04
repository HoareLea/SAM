using Rhino;
using Rhino.DocObjects;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public static partial class Modify
    {
        public static void BakeGeometry_ByInternalCondition(this RhinoDoc rhinoDoc, global::Grasshopper.Kernel.Data.IGH_Structure gH_Structure, bool cutApertures = false, double tolerance = Core.Tolerance.Distance)
        {
            Rhino.DocObjects.Tables.LayerTable layerTable = rhinoDoc?.Layers;
            if (layerTable == null)
                return;

            List<Space> spaces = new List<Space>();
            foreach (var variable in gH_Structure.AllData(true))
            {
                if (variable is GooSpace)
                {
                    Space space = ((GooSpace)variable).Value;
                    if (space == null)
                        continue;

                    Geometry.Spatial.Point3D location = space.Location;
                    if (location == null)
                        continue;

                    spaces.Add(space);
                }
            }

            if (spaces != null && spaces.Count != 0)
                BakeGeometry_ByInternalCondition(rhinoDoc, spaces);
        }

        public static void BakeGeometry_ByInternalCondition(this RhinoDoc rhinoDoc, IEnumerable<Space> spaces)
        {
            Rhino.DocObjects.Tables.LayerTable layerTable = rhinoDoc?.Layers;
            if (layerTable == null)
                return;

            Layer layer_SAM = Core.Grasshopper.Modify.AddSAMLayer(layerTable);
            if (layer_SAM == null)
                return;

            int index = -1;

            index = layerTable.Add();
            Layer layer_Spaces = layerTable[index];
            layer_Spaces.Name = "Spaces";
            layer_Spaces.ParentLayerId = layer_SAM.Id;

            //int currentIndex = layerTable.CurrentLayerIndex;

            ObjectAttributes objectAttributes = rhinoDoc.CreateDefaultAttributes();

            Random random = new Random();

            List<Guid> guids = new List<Guid>();
            foreach (Space space in spaces)
            {
                Geometry.Spatial.Point3D location = space?.Location;
                if (location == null)
                    continue;

                string internalConditionName = space.InternalCondition?.Name;
                if (string.IsNullOrWhiteSpace(internalConditionName))
                    internalConditionName = "???";

                System.Drawing.Color color = System.Drawing.Color.FromArgb(random.Next(0, 254), random.Next(0, 254), random.Next(0, 254));

                Layer layer_Level = Core.Grasshopper.Modify.GetLayer(layerTable, layer_Spaces.Id, internalConditionName, color);

                string layerName = space.Name;
                if (string.IsNullOrWhiteSpace(layerName))
                    layerName = "???";

                color = System.Drawing.Color.FromArgb(random.Next(0, 254), random.Next(0, 254), random.Next(0, 254));

                Layer layer_Space = Core.Grasshopper.Modify.GetLayer(layerTable, layer_Level.Id, layerName, color);

                //layerTable.SetCurrentLayerIndex(layer_Space.Index, true);
                objectAttributes.LayerIndex = layer_Space.Index;

                Guid guid = default;
                if (BakeGeometry(space, rhinoDoc, objectAttributes, out guid))
                    guids.Add(guid);
            }

            //layerTable.SetCurrentLayerIndex(currentIndex, true);
        }
    }
}