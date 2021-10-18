using Rhino;
using Rhino.DocObjects;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public static partial class Modify
    {
        public static void BakeGeometry_ByLevel_Rooms(this RhinoDoc rhinoDoc, global::Grasshopper.Kernel.Data.IGH_Structure gH_Structure, bool cutApertures = false, double tolerance = Core.Tolerance.Distance)
        {
            Rhino.DocObjects.Tables.LayerTable layerTable = rhinoDoc?.Layers;
            if (layerTable == null)
                return;

            List<Room> rooms = new List<Room>();
            foreach (var variable in gH_Structure.AllData(true))
            {
                if (variable is GooRoom)
                {
                    Room room = ((GooRoom)variable).Value;
                    if (room == null)
                        continue;

                    Geometry.Spatial.Point3D location = room.Location;
                    if (location == null)
                        continue;

                    rooms.Add(room);
                }
            }

            if (rooms != null && rooms.Count != 0)
                BakeGeometry_ByLevel(rhinoDoc, rooms);
        }

        public static void BakeGeometry_ByLevel(this RhinoDoc rhinoDoc, IEnumerable<Room> rooms)
        {
            Rhino.DocObjects.Tables.LayerTable layerTable = rhinoDoc?.Layers;
            if (layerTable == null)
                return;

            Layer layer_SAM = Core.Grasshopper.Modify.AddSAMLayer(layerTable);
            if (layer_SAM == null)
                return;

            int index = -1;

            index = layerTable.Add();
            Layer layer_Rooms = layerTable[index];
            layer_Rooms.Name = "Rooms";
            layer_Rooms.ParentLayerId = layer_SAM.Id;

            //int currentIndex = layerTable.CurrentLayerIndex;

            ObjectAttributes objectAttributes = rhinoDoc.CreateDefaultAttributes();

            Random random = new Random();

            List<Guid> guids = new List<Guid>();
            foreach (Room room in rooms)
            {
                Geometry.Spatial.Point3D location = room?.Location;
                if (location == null)
                    continue;

                string levelName = room.GetValue<string>(RoomParameter.LevelName);
                if (string.IsNullOrWhiteSpace(levelName))
                    levelName = "Level " + System.Math.Round(location.Z, 2).ToString();

                if (string.IsNullOrWhiteSpace(levelName))
                    levelName = "???";

                System.Drawing.Color color = System.Drawing.Color.FromArgb(random.Next(0, 254), random.Next(0, 254), random.Next(0, 254));

                Layer layer_Level = Core.Grasshopper.Modify.GetLayer(layerTable, layer_Rooms.Id, levelName, color);

                string layerName = room.Name;
                if (string.IsNullOrWhiteSpace(layerName))
                    layerName = "???";

                color = System.Drawing.Color.FromArgb(random.Next(0, 254), random.Next(0, 254), random.Next(0, 254));

                Layer layer_Room = Core.Grasshopper.Modify.GetLayer(layerTable, layer_Level.Id, layerName, color);

                //layerTable.SetCurrentLayerIndex(layer_Space.Index, true);
                objectAttributes.LayerIndex = layer_Room.Index;

                Guid guid = default;
                if (BakeGeometry(room, rhinoDoc, objectAttributes, out guid))
                    guids.Add(guid);
            }

            //layerTable.SetCurrentLayerIndex(currentIndex, true);
        }
    }
}