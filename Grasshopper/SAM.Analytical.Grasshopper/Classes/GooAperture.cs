using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino;
using Rhino.Commands;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.Geometry;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using SAM.Geometry.Grasshopper;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper
{
    public class GooAperture : GooJSAMObject<Aperture>, IGH_PreviewData, IGH_BakeAwareData
    {
        public GooAperture()
            : base()
        {
        }

        public GooAperture(Aperture aperture)
            : base(aperture)
        {
        }

        public BoundingBox ClippingBox
        {
            get
            {
                if (Value == null)
                    return BoundingBox.Empty;

                return Geometry.Rhino.Convert.ToRhino(Value.GetBoundingBox());
            }
        }

        public override IGH_Goo Duplicate()
        {
            return new GooAperture(Value);
        }

        public void DrawViewportWires(GH_PreviewWireArgs args)
        {
            if (Value == null)
                return;

            System.Drawing.Color color_ExternalEdge = System.Drawing.Color.Empty;
            System.Drawing.Color color_InternalEdges = System.Drawing.Color.Empty;

            if(Value.ApertureConstruction != null)
            {
                color_ExternalEdge = Analytical.Query.Color(Value.ApertureConstruction.ApertureType, false);
                color_InternalEdges = Analytical.Query.Color(Value.ApertureConstruction.ApertureType, true);
            }

            if (color_ExternalEdge == System.Drawing.Color.Empty)
                color_ExternalEdge = System.Drawing.Color.DarkRed;

            if (color_InternalEdges == System.Drawing.Color.Empty)
                color_InternalEdges = System.Drawing.Color.BlueViolet;

            DrawViewportWires(args, color_ExternalEdge, color_InternalEdges);
        }

        public void DrawViewportWires(GH_PreviewWireArgs args, System.Drawing.Color color_ExternalEdge, System.Drawing.Color color_InternalEdges)
        {
            Face3D face3D = Value?.GetFrameFace3D();
            if(face3D == null)
            {
                return;
            }

            GooPlanarBoundary3D gooPlanarBoundary3D = new GooPlanarBoundary3D(new PlanarBoundary3D(face3D));
            gooPlanarBoundary3D.DrawViewportWires(args, color_ExternalEdge, color_InternalEdges);
        }

        public void DrawViewportMeshes(GH_PreviewMeshArgs args)
        {
            if (Value == null)
                return;

            DisplayMaterial displayMaterial_Pane = null;
            DisplayMaterial displayMaterial_Frame = null;
            if (Value.ApertureConstruction != null)
            {
                displayMaterial_Pane = Query.DisplayMaterial(Value.ApertureConstruction.ApertureType, AperturePart.Pane);
                displayMaterial_Frame = Query.DisplayMaterial(Value.ApertureConstruction.ApertureType, AperturePart.Frame);
            }

            if (displayMaterial_Pane == null)
                displayMaterial_Pane = args.Material;

            if (displayMaterial_Frame == null)
                displayMaterial_Frame = args.Material;

            List<Face3D> face3Ds = null;

            face3Ds = Value.GetFace3Ds(AperturePart.Frame);
            if(face3Ds != null)
            {
                foreach(Face3D face3D in face3Ds)
                {
                    GooPlanarBoundary3D gooPlanarBoundary3D = new GooPlanarBoundary3D(new PlanarBoundary3D(face3D));
                    gooPlanarBoundary3D.DrawViewportMeshes(args, displayMaterial_Frame);
                }
            }

            face3Ds = Value.GetFace3Ds(AperturePart.Pane);
            if (face3Ds != null)
            {
                foreach (Face3D face3D in face3Ds)
                {
                    GooPlanarBoundary3D gooPlanarBoundary3D = new GooPlanarBoundary3D(new PlanarBoundary3D(face3D));
                    gooPlanarBoundary3D.DrawViewportMeshes(args, displayMaterial_Pane);
                }
            }
        }

        public bool BakeGeometry(RhinoDoc doc, ObjectAttributes att, out Guid obj_guid)
        {
            obj_guid = Guid.Empty;
            
            return Rhino.Modify.BakeGeometry(Value, doc, att, out obj_guid);
        }

        public override bool CastFrom(object source)
        {
            if (source is Aperture)
            {
                Value = (Aperture)source;
                return true;
            }

            if (typeof(IGH_Goo).IsAssignableFrom(source.GetType()))
            {
                try
                {
                    source = (source as dynamic).Value;
                }
                catch
                {
                }

                if (source is Aperture)
                {
                    Value = (Aperture)source;
                    return true;
                }
            }

            return base.CastFrom(source);
        }

        public override bool CastTo<Y>(ref Y target)
        {
            if (Value == null)
                return false;

            if (typeof(Y).IsAssignableFrom(typeof(GH_Mesh)))
            {
                target = (Y)(object)Value.ToGrasshopper_Mesh();
                return true;
            }
            else if (typeof(Y).IsAssignableFrom(typeof(GH_Brep)))
            {
                target = (Y)(object)Value.GetFace3D()?.ToGrasshopper_Brep();
                return true;
            }

            return base.CastTo(ref target);
        }
    }

    public class GooApertureParam : GH_PersistentParam<GooAperture>, IGH_PreviewObject, IGH_BakeAwareObject
    {
        public override Guid ComponentGuid => new Guid("d5f56261-608b-4cec-baa4-ca2fb29ab5be");

        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        bool IGH_PreviewObject.Hidden { get; set; }

        bool IGH_PreviewObject.IsPreviewCapable => !VolatileData.IsEmpty;

        BoundingBox IGH_PreviewObject.ClippingBox => Preview_ComputeClippingBox();

        public bool IsBakeCapable => true;

        void IGH_PreviewObject.DrawViewportMeshes(IGH_PreviewArgs args) => Preview_DrawMeshes(args);

        void IGH_PreviewObject.DrawViewportWires(IGH_PreviewArgs args) => Preview_DrawWires(args);

        public GooApertureParam()
            : base(typeof(Aperture).Name, typeof(Aperture).Name, typeof(Panel).FullName.Replace(".", " "), "Params", "SAM")
        {
        }

        public override void AppendAdditionalMenuItems(System.Windows.Forms.ToolStripDropDown menu)
        {
            Menu_AppendItem(menu, "Bake By Type", Menu_BakeByApertureType, VolatileData.AllData(true).Any());
            Menu_AppendItem(menu, "Bake By Construction", Menu_BakeByApertureConstruction, VolatileData.AllData(true).Any());
            Menu_AppendItem(menu, "Save As...", Menu_SaveAs, VolatileData.AllData(true).Any());

            base.AppendAdditionalMenuItems(menu);
        }

        private void Menu_BakeByApertureConstruction(object sender, EventArgs e)
        {
            BakeGeometry_ByApertureConstruction(RhinoDoc.ActiveDoc);
        }

        public void BakeGeometry_ByApertureConstruction(RhinoDoc doc)
        {
            Modify.BakeGeometry_ByApertureConstruction(doc, VolatileData);
        }

        protected override GH_GetterResult Prompt_Plural(ref List<GooAperture> values)
        {
            global::Rhino.Input.Custom.GetObject getObject = new global::Rhino.Input.Custom.GetObject();
            getObject.SetCommandPrompt("Pick Surfaces to create apertures");
            getObject.GeometryFilter = ObjectType.Brep;
            getObject.SubObjectSelect = true;
            getObject.DeselectAllBeforePostSelect = false;
            getObject.OneByOnePostSelect = false;
            getObject.GetMultiple(1, 0);

            if (getObject.CommandResult() != Result.Success)
                return GH_GetterResult.cancel;

            if (getObject.ObjectCount == 0)
                return GH_GetterResult.cancel;

            values = new List<GooAperture>();

            for (int i = 0; i < getObject.ObjectCount; i++)
            {
                ObjRef objRef = getObject.Object(i);

                RhinoObject rhinoObject = objRef.Object();
                if (rhinoObject == null)
                    return GH_GetterResult.cancel;

                Brep brep = rhinoObject.Geometry as Brep;
                if (brep == null)
                    return GH_GetterResult.cancel;

                List<Aperture> apertures = null;

                if (brep.HasUserData)
                {
                    string @string = brep.GetUserString("SAM");
                    if (!string.IsNullOrWhiteSpace(@string))
                    {
                        apertures = Core.Convert.ToSAM<Aperture>(@string);
                    }
                }

                if (apertures == null || apertures.Count == 0)
                {

                    List<ISAMGeometry3D> sAMGeometry3Ds = Geometry.Rhino.Convert.ToSAM(brep);
                    if (sAMGeometry3Ds == null)
                        continue;

                    apertures = Create.Apertures(sAMGeometry3Ds);
                }

                if (apertures == null || apertures.Count == 0)
                    continue;

                apertures.RemoveAll(x => x == null);

                values.AddRange(apertures.ConvertAll(x => new GooAperture(x)));
            }

            return GH_GetterResult.success;
        }

        protected override GH_GetterResult Prompt_Singular(ref GooAperture value)
        {
            global::Rhino.Input.Custom.GetObject getObject = new global::Rhino.Input.Custom.GetObject();
            getObject.SetCommandPrompt("Pick Surfaces to create apertures");
            getObject.GeometryFilter = ObjectType.Brep;
            getObject.SubObjectSelect = true;
            getObject.DeselectAllBeforePostSelect = false;
            getObject.OneByOnePostSelect = false;
            getObject.GetMultiple(1, 0);

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

                Brep brep = rhinoObject.Geometry as Brep;
                if (brep == null)
                    return GH_GetterResult.cancel;

                List<Aperture> apertures = null;

                if (brep.HasUserData)
                {
                    string @string = brep.GetUserString("SAM");
                    if (!string.IsNullOrWhiteSpace(@string))
                    {
                        apertures = Core.Convert.ToSAM<Aperture>(@string);
                    }
                }

                if (apertures == null || apertures.Count == 0)
                {

                    List<ISAMGeometry3D> sAMGeometry3Ds = Geometry.Rhino.Convert.ToSAM(brep);
                    if (sAMGeometry3Ds == null)
                        continue;

                    apertures = Create.Apertures(sAMGeometry3Ds);
                }

                if (apertures == null || apertures.Count == 0)
                    continue;

                apertures.RemoveAll(x => x == null);
                if(apertures.Count != 0)
                {
                    value = new GooAperture(apertures[0]);
                    return GH_GetterResult.success;
                }
            }

            return GH_GetterResult.cancel;
        }

        public void BakeGeometry(RhinoDoc doc, List<Guid> obj_ids)
        {
            BakeGeometry(doc, doc.CreateDefaultAttributes(), obj_ids);
        }

        public void BakeGeometry(RhinoDoc doc, ObjectAttributes att, List<Guid> obj_ids)
        {
            foreach (IGH_Goo goo in VolatileData.AllData(true))
            {
                Guid guid = default;
                
                IGH_BakeAwareData bakeAwareData = goo as IGH_BakeAwareData;
                if (bakeAwareData != null)
                    bakeAwareData.BakeGeometry(doc, att, out guid);
                
                obj_ids.Add(guid);
            }
        }

        private void Menu_SaveAs(object sender, EventArgs e)
        {
            Core.Grasshopper.Query.SaveAs(VolatileData);
        }

        private void Menu_BakeByApertureType(object sender, EventArgs e)
        {
            BakeGeometry_ByApertureType(RhinoDoc.ActiveDoc);
        }

        public void BakeGeometry_ByApertureType(RhinoDoc doc)
        {
            Modify.BakeGeometry_ByApertureType(doc, VolatileData);
        }
    }
}