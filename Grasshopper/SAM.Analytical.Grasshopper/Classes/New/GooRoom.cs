using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Geometry;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using SAM.Geometry.Grasshopper;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace SAM.Analytical.Grasshopper
{
    public class GooRoom : GooJSAMObject<Room>, IGH_PreviewData, IGH_BakeAwareData
    {
        public GooRoom()
            : base()
        {
        }

        public GooRoom(Room room)
            : base(room)
        {
        }

        public BoundingBox ClippingBox
        {
            get
            {
                if (Value == null)
                    return BoundingBox.Empty;

                Point3D location = Value.Location;
                if (location == null || !location.IsValid())
                    return BoundingBox.Empty;

                return Geometry.Grasshopper.Convert.ToRhino(Value.Location.GetBoundingBox(1));
            }
        }

        public override IGH_Goo Duplicate()
        {
            return new GooRoom(Value);
        }

        public void DrawViewportWires(GH_PreviewWireArgs args)
        {
            Point3D point3D = Value?.Location;
            if (point3D == null)
                return;

            args.Pipeline.DrawPoint(Geometry.Grasshopper.Convert.ToRhino(point3D));
        }

        public void DrawViewportMeshes(GH_PreviewMeshArgs args)
        {
            Point3D point3D = Value?.Location;
            if (point3D == null)
                return;

            args.Pipeline.DrawPoint(Geometry.Grasshopper.Convert.ToRhino(point3D));
        }

        public bool BakeGeometry(RhinoDoc doc, ObjectAttributes att, out Guid obj_guid)
        {
            return Modify.BakeGeometry(Value, doc, att, out obj_guid);
        }
    }

    public class GooRoomParam : GH_PersistentParam<GooRoom>, IGH_PreviewObject, IGH_BakeAwareObject
    {
        public override Guid ComponentGuid => new Guid("cb056fce-2b26-497f-980e-90f7a462f53d");

        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        bool IGH_PreviewObject.Hidden { get; set; }

        bool IGH_PreviewObject.IsPreviewCapable => !VolatileData.IsEmpty;

        BoundingBox IGH_PreviewObject.ClippingBox => Preview_ComputeClippingBox();

        public bool IsBakeCapable => !VolatileData.IsEmpty;

        void IGH_PreviewObject.DrawViewportMeshes(IGH_PreviewArgs args) => Preview_DrawMeshes(args);

        void IGH_PreviewObject.DrawViewportWires(IGH_PreviewArgs args) => Preview_DrawWires(args);

        public GooRoomParam()
            : base(typeof(Room).Name, typeof(Room).Name, typeof(Room).FullName.Replace(".", " "), "Params", "SAM")
        {
        }

        protected override GH_GetterResult Prompt_Plural(ref List<GooRoom> values)
        {
            Rhino.Input.Custom.GetObject getObject = new Rhino.Input.Custom.GetObject();
            getObject.SetCommandPrompt("Pick Points to create Rooms");
            getObject.GeometryFilter = ObjectType.Point;
            getObject.SubObjectSelect = true;
            getObject.DeselectAllBeforePostSelect = false;
            getObject.OneByOnePostSelect = false;
            getObject.GetMultiple(1, 0);

            if (getObject.CommandResult() != Result.Success)
                return GH_GetterResult.cancel;

            if (getObject.ObjectCount == 0)
                return GH_GetterResult.cancel;

            values = new List<GooRoom>();

            for (int i = 0; i < getObject.ObjectCount; i++)
            {
                ObjRef objRef = getObject.Object(i);

                RhinoObject rhinoObject = objRef.Object();
                if (rhinoObject == null)
                    return GH_GetterResult.cancel;

                Point point = rhinoObject.Geometry as Point;
                if (point == null)
                    return GH_GetterResult.cancel;

                List<Room> rooms = null;

                if (point.HasUserData)
                {
                    string @string = point.GetUserString("SAM");
                    if (!string.IsNullOrWhiteSpace(@string))
                    {
                        rooms = Core.Convert.ToSAM<Room>(@string);
                    }
                }

                if (rooms == null || rooms.Count == 0)
                {

                    Point3D point3D = point.ToSAM();
                    if (point3D == null)
                        continue;

                    string name = rhinoObject?.Name;
                    if (string.IsNullOrWhiteSpace(name))
                        name = "Cell";

                    rooms = new List<Room>() { new Room(name, point3D)};
                }

                if (rooms == null || rooms.Count == 0)
                    continue;

                values.AddRange(rooms.ConvertAll(x => new GooRoom(x)));
            }

            return GH_GetterResult.success;
        }

        protected override GH_GetterResult Prompt_Singular(ref GooRoom value)
        {
            Rhino.Input.Custom.GetObject getObject = new Rhino.Input.Custom.GetObject();
            getObject.SetCommandPrompt("Pick Point to create space");
            getObject.GeometryFilter = ObjectType.Point;
            getObject.SubObjectSelect = true;
            getObject.DeselectAllBeforePostSelect = false;
            getObject.OneByOnePostSelect = true;
            getObject.Get();

            if (getObject.CommandResult() != Result.Success)
                return GH_GetterResult.cancel;

            if (getObject.ObjectCount == 0)
                return GH_GetterResult.cancel;

            for (int i = 0; i < getObject.ObjectCount; i++)
            {
                ObjRef objRef = getObject.Object(i);

                RhinoObject rhinoObject = objRef.Object();
                if (rhinoObject == null)
                    return GH_GetterResult.cancel;

                Point point = rhinoObject.Geometry as Point;
                if (point == null)
                    return GH_GetterResult.cancel;

                List<Room> rooms = null;

                if (point.HasUserData)
                {
                    string @string = point.GetUserString("SAM");
                    if (!string.IsNullOrWhiteSpace(@string))
                    {
                        rooms = Core.Convert.ToSAM<Room>(@string);
                    }
                }

                if (rooms == null || rooms.Count == 0)
                {
                    Point3D point3D = point.ToSAM();
                    if (point3D == null)
                        continue;

                    string name = rhinoObject?.Name;
                    if (string.IsNullOrWhiteSpace(name))
                        name = "Cell";

                    rooms = new List<Room>() { new Room(name, point3D) };
                }

                if (rooms == null || rooms.Count == 0)
                    continue;

                value = new GooRoom(rooms[0]);
            }

            return GH_GetterResult.success;
        }

        public void BakeGeometry(RhinoDoc doc, List<Guid> obj_ids)
        {
            BakeGeometry(doc, doc.CreateDefaultAttributes(), obj_ids);
        }

        public void BakeGeometry(RhinoDoc doc, ObjectAttributes att, List<Guid> obj_ids)
        {
            foreach (var value in VolatileData.AllData(true))
            {
                Guid uuid = default;
                (value as IGH_BakeAwareData)?.BakeGeometry(doc, att, out uuid);
                obj_ids.Add(uuid);
            }
        }

        public override void AppendAdditionalMenuItems(ToolStripDropDown menu)
        {
            Menu_AppendItem(menu, "Bake By Internal Condition", Menu_BakeByInternalCondition, VolatileData.AllData(true).Any());
            Menu_AppendItem(menu, "Bake By Level", Menu_BakeByLevel, VolatileData.AllData(true).Any());

            //Menu_AppendSeparator(menu);

            base.AppendAdditionalMenuItems(menu);
        }

        private void Menu_BakeByInternalCondition(object sender, EventArgs e)
        {
            BakeGeometry_ByInternalCondition(RhinoDoc.ActiveDoc);
        }

        public void BakeGeometry_ByInternalCondition(RhinoDoc doc)
        {
            Modify.BakeGeometry_ByInternalCondition_Rooms(doc, VolatileData, false, Core.Tolerance.Distance);
        }

        private void Menu_BakeByLevel(object sender, EventArgs e)
        {
            BakeGeometry_ByLevel(RhinoDoc.ActiveDoc);
        }

        public void BakeGeometry_ByLevel(RhinoDoc doc)
        {
            Modify.BakeGeometry_ByLevel(doc, VolatileData, false, Core.Tolerance.Distance);
        }
    }
}