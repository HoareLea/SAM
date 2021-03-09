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

            Dictionary<string, List<Geometry.Spatial.Point3D>> dictionary = new Dictionary<string, List<Geometry.Spatial.Point3D>>();
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

                    string internalConditionName = space.InternalCondition?.Name;
                    if (string.IsNullOrWhiteSpace(internalConditionName))
                        internalConditionName = "???";

                    if(!dictionary.TryGetValue(internalConditionName, out List<Geometry.Spatial.Point3D> point3Ds))
                    {
                        point3Ds = new List<Geometry.Spatial.Point3D>();
                        dictionary[internalConditionName] = point3Ds;
                    }

                    point3Ds.Add(location);
                }
            }

            Layer layer_SAM = Core.Grasshopper.Modify.AddSAMLayer(layerTable);
            if (layer_SAM == null)
                return;

            foreach (KeyValuePair<string, List<Geometry.Spatial.Point3D>> keyValuePair in dictionary)
                BakeGeometry_ByInternalCondition(rhinoDoc, keyValuePair.Value, keyValuePair.Key, layer_SAM.Id);
        }

        public static void BakeGeometry_ByInternalCondition(this RhinoDoc rhinoDoc, IEnumerable<Geometry.Spatial.Point3D> point3Ds, string internalConditionName, Guid parentLayerId)
        {
            Rhino.DocObjects.Tables.LayerTable layerTable = rhinoDoc?.Layers;
            if (layerTable == null)
                return;

            int index = -1;

            index = layerTable.Add();
            Layer layer_Level = layerTable[index];
            layer_Level.Name = internalConditionName;
            layer_Level.ParentLayerId = parentLayerId;

            int currentIndex = layerTable.CurrentLayerIndex;

            layerTable.SetCurrentLayerIndex(layer_Level.Index, true);

            ObjectAttributes objectAttributes = rhinoDoc.CreateDefaultAttributes();

            List<Guid> guids = new List<Guid>();
            foreach (Geometry.Spatial.Point3D point3D in point3Ds)
            {
                if (point3D == null)
                    continue;

                Guid guid = default;
                if(Geometry.Grasshopper.Modify.BakeGeometry(point3D, rhinoDoc, objectAttributes, out guid))
                    guids.Add(guid);
            }

            layerTable.SetCurrentLayerIndex(currentIndex, true);
        }
    }
}