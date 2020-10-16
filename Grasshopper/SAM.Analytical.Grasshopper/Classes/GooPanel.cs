using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Geometry;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using SAM.Geometry.Grasshopper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper
{
    public class GooPanel : GooSAMObject<Panel>, IGH_PreviewData, IGH_BakeAwareData
    {
        public GooPanel()
            : base()
        {
        }

        public GooPanel(Panel panel)
            : base(panel)
        {
        }

        public BoundingBox ClippingBox
        {
            get
            {
                if (Value == null)
                    return BoundingBox.Empty;

                return Geometry.Grasshopper.Convert.ToRhino(Value.GetBoundingBox());
            }
        }

        public override IGH_Goo Duplicate()
        {
            return new GooPanel(Value);
        }

        public void DrawViewportWires(GH_PreviewWireArgs args)
        {
            if (Value == null)
                return;

            System.Drawing.Color color_ExternalEdge = Query.Color(Value.PanelType, false);
            System.Drawing.Color color_InternalEdges = Query.Color(Value.PanelType, true);

            if (color_ExternalEdge == System.Drawing.Color.Empty)
                color_ExternalEdge = args.Color;

            if (color_InternalEdges == System.Drawing.Color.Empty)
                color_InternalEdges = args.Color;

            GooPlanarBoundary3D gooPlanarBoundary3D = new GooPlanarBoundary3D(Value.PlanarBoundary3D);
            gooPlanarBoundary3D.DrawViewportWires(args, color_ExternalEdge, color_InternalEdges);

            List<Aperture> apertures = Value.Apertures;
            if (apertures != null)
            {
                foreach (Aperture aperture in apertures)
                {
                    if (aperture == null)
                        continue;

                    GooAperture gooAperture = new GooAperture(aperture);
                    gooAperture.DrawViewportWires(args);
                }
            }
        }

        public void DrawViewportMeshes(GH_PreviewMeshArgs args)
        {
            if (Value == null)
                return;

            Rhino.Display.DisplayMaterial displayMaterial = Query.DisplayMaterial(Value.PanelType);
            if (displayMaterial == null)
                displayMaterial = args.Material;

            GooSAMGeometry gooSAMGeometry = new GooSAMGeometry(Value.GetFace3D());
            gooSAMGeometry.DrawViewportMeshes(args, displayMaterial);

            ////TODO Play with  text values on model
            //TextEntity textEntity = new TextEntity
            //{
            //    Plane = Value.GetFace3D().GetPlane().ToRhino(),
            //    PlainText = Value.Name,
            //    Justification = TextJustification.MiddleCenter
            //};
            //args.Pipeline.DrawText(textEntity, System.Drawing.Color.Red);

            List<Aperture> apertures = Value.Apertures;
            if (apertures != null)
            {
                foreach (Aperture aperture in apertures)
                    foreach (Geometry.Spatial.IClosedPlanar3D closedPlanar3D in aperture.GetFace3D().GetEdge3Ds())
                    {
                        Rhino.Display.DisplayMaterial displayMaterial_Aperture = Query.DisplayMaterial(aperture.ApertureConstruction.ApertureType);
                        if (displayMaterial_Aperture == null)
                            displayMaterial_Aperture = args.Material;

                        GooSAMGeometry gooSAMGeometry_Aperture = new GooSAMGeometry(closedPlanar3D);
                        gooSAMGeometry_Aperture.DrawViewportMeshes(args, displayMaterial_Aperture);
                    }
            }
        }

        public bool BakeGeometry(RhinoDoc doc, ObjectAttributes att, out Guid obj_guid)
        {
            return Modify.BakeGeometry(Value, doc, att, out obj_guid);
        }

        public override bool CastFrom(object source)
        {
            if (source is Panel)
            {
                Value = (Panel)source;
                return true;
            }

            if (typeof(IGH_Goo).IsAssignableFrom(source.GetType()))
            {
                object object_Temp = null;

                try
                {
                    object_Temp = (source as dynamic).Value;
                }
                catch
                {
                }

                if (object_Temp is Panel)
                {
                    Value = (Panel)object_Temp;
                    return true;
                }
            }

            return base.CastFrom(source);
        }

        public override bool CastTo<Y>(ref Y target)
        {
            return base.CastTo(ref target);
        }
    }

    public class GooPanelParam : GH_PersistentParam<GooPanel>, IGH_PreviewObject, IGH_BakeAwareObject
    {
        public override Guid ComponentGuid => new Guid("278B438C-43EA-4423-999F-B6A906870939");

        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        bool IGH_PreviewObject.Hidden { get; set; }

        bool IGH_PreviewObject.IsPreviewCapable => !VolatileData.IsEmpty;

        BoundingBox IGH_PreviewObject.ClippingBox => Preview_ComputeClippingBox();

        public bool IsBakeCapable => true;

        void IGH_PreviewObject.DrawViewportMeshes(IGH_PreviewArgs args) => Preview_DrawMeshes(args);

        void IGH_PreviewObject.DrawViewportWires(IGH_PreviewArgs args) => Preview_DrawWires(args);

        public GooPanelParam()
            : base(typeof(Panel).Name, typeof(Panel).Name, typeof(Panel).FullName.Replace(".", " "), "Params", "SAM")
        {
        }

        protected override GH_GetterResult Prompt_Plural(ref List<GooPanel> values)
        {
            throw new NotImplementedException();
        }

        protected override GH_GetterResult Prompt_Singular(ref GooPanel value)
        {
            //throw new NotImplementedException();

            Rhino.Input.Custom.GetObject getObject = new Rhino.Input.Custom.GetObject();
            getObject.SetCommandPrompt("Pick Surface to create panel");
            getObject.GeometryFilter = ObjectType.Surface;
            getObject.SubObjectSelect = true;
            getObject.DeselectAllBeforePostSelect = false;
            getObject.OneByOnePostSelect = true;
            getObject.Get();

            if (getObject.CommandResult() != Result.Success)
                return GH_GetterResult.cancel;

            ObjRef objRef = getObject.Object(0);

            RhinoObject rhinoObject = objRef.Object();
            if (rhinoObject == null)
                return GH_GetterResult.cancel;

            Surface surface = objRef.Surface();
            if (surface == null)
                return GH_GetterResult.cancel;

            List<Panel> panels = Create.Panels(Geometry.Grasshopper.Convert.ToSAM(surface), PanelType.WallExternal, Analytical.Query.DefaultConstruction(PanelType.WallExternal), Core.Tolerance.MacroDistance);
            if (panels == null || panels.Count == 0)
                return GH_GetterResult.cancel;

            value = new GooPanel(panels.First());
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

        public void BakeGeometry_ByPanelType(RhinoDoc doc)
        {
            Rhino.DocObjects.Tables.LayerTable layerTable = doc.Layers;

            Layer layer_SAM = Core.Grasshopper.Modify.AddSAMLayer(layerTable);

            int index = -1;

            index = layerTable.Add();
            Layer layer_PanelType = layerTable[index];
            layer_PanelType.Name = "PanelType";
            layer_PanelType.ParentLayerId = layer_SAM.Id;

            index = layerTable.Add();
            Layer layer_ApertureType = layerTable[index];
            layer_ApertureType.Name = "ApertureType";
            layer_ApertureType.ParentLayerId = layer_SAM.Id;

            List<Guid> guids = new List<Guid>();
            foreach (var value in VolatileData.AllData(true))
            {
                Panel panel = (value as GooPanel)?.Value;
                if (panel == null)
                    continue;

                Layer layer = Core.Grasshopper.Modify.GetLayer(layerTable, layer_PanelType.Id, panel.PanelType.ToString());

                ObjectAttributes objectAttributes = new ObjectAttributes();
                objectAttributes.LayerIndex = layer.Index;

                Guid guid = default;
                if(Modify.BakeGeometry(panel, doc, objectAttributes, out guid))
                    guids.Add(guid);

                List<Aperture> apertures = panel.Apertures;
                if (apertures == null || apertures.Count == 0)
                    continue;

                foreach(Aperture aperture in apertures)
                {
                    layer = Core.Grasshopper.Modify.GetLayer(layerTable, layer_ApertureType.Id, aperture.ApertureType.ToString());

                    objectAttributes = new ObjectAttributes();
                    objectAttributes.LayerIndex = layer.Index;

                    guid = default;
                    if (Modify.BakeGeometry(panel, doc, objectAttributes, out guid))
                        guids.Add(guid);
                }
            }
        }

        public override void AppendAdditionalMenuItems(System.Windows.Forms.ToolStripDropDown menu)
        {
            Menu_AppendItem(menu, "Bake By PanelType", Menu_BakeByPanelType, VolatileData.AllData(true).Any());

            base.AppendAdditionalMenuItems(menu);
        }

        private void Menu_BakeByPanelType(object sender, EventArgs e)
        {
            BakeGeometry_ByPanelType(RhinoDoc.ActiveDoc);
        }
    }
}