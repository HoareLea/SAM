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

namespace SAM.Analytical.Grasshopper
{
    public class GooOpening : GooJSAMObject<IOpening>, IGH_PreviewData, IGH_BakeAwareData
    {
        public GooOpening()
            : base()
        {
        }

        public GooOpening(IOpening opening)
            : base(opening)
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
            return new GooOpening(Value);
        }

        public void DrawViewportWires(GH_PreviewWireArgs args)
        {
            if (Value == null)
                return;

            System.Drawing.Color color_ExternalEdge = System.Drawing.Color.Empty;
            System.Drawing.Color color_InternalEdges = System.Drawing.Color.Empty;

            OpeningType openingType = Value.Type();
            if (openingType != null)
            {
                color_ExternalEdge = Analytical.Query.Color(openingType, false);
                color_InternalEdges = Analytical.Query.Color(openingType, true);
            }

            if (color_ExternalEdge == System.Drawing.Color.Empty)
                color_ExternalEdge = System.Drawing.Color.DarkRed;

            if (color_InternalEdges == System.Drawing.Color.Empty)
                color_InternalEdges = System.Drawing.Color.BlueViolet;

            DrawViewportWires(args, color_ExternalEdge, color_InternalEdges);
        }

        public void DrawViewportWires(GH_PreviewWireArgs args, System.Drawing.Color color_ExternalEdge, System.Drawing.Color color_InternalEdges)
        {
            Face3D face3D = (Value as dynamic)?.GetFrameFace3D();
            if (face3D == null)
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

            displayMaterial_Pane = Query.DisplayMaterial(Value, OpeningPart.Pane);
            displayMaterial_Frame = Query.DisplayMaterial(Value, OpeningPart.Frame);

            if (displayMaterial_Pane == null)
                displayMaterial_Pane = args.Material;

            if (displayMaterial_Frame == null)
                displayMaterial_Frame = args.Material;

            List<Face3D> face3Ds = null;

            face3Ds = (Value as dynamic).GetFace3Ds(OpeningPart.Frame);
            if (face3Ds != null)
            {
                foreach (Face3D face3D in face3Ds)
                {
                    GooPlanarBoundary3D gooPlanarBoundary3D = new GooPlanarBoundary3D(new PlanarBoundary3D(face3D));
                    gooPlanarBoundary3D.DrawViewportMeshes(args, displayMaterial_Frame);
                }
            }

            face3Ds = (Value as dynamic).GetFace3Ds(OpeningPart.Pane);
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
            if (source is IOpening)
            {
                Value = (IOpening)source;
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

                if (source is IOpening)
                {
                    Value = (IOpening)source;
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
                target = (Y)(object)Value.Face3D?.ToGrasshopper_Brep();
                return true;
            }

            return base.CastTo(ref target);
        }
    }

    public class GooOpeningParam : GH_PersistentParam<GooOpening>, IGH_PreviewObject, IGH_BakeAwareObject
    {
        public override Guid ComponentGuid => new Guid("7c7fd553-1861-44c9-a16d-5c76da18fc1e");

        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        bool IGH_PreviewObject.Hidden { get; set; }

        bool IGH_PreviewObject.IsPreviewCapable => !VolatileData.IsEmpty;

        BoundingBox IGH_PreviewObject.ClippingBox => Preview_ComputeClippingBox();

        public bool IsBakeCapable => true;

        void IGH_PreviewObject.DrawViewportMeshes(IGH_PreviewArgs args) => Preview_DrawMeshes(args);

        void IGH_PreviewObject.DrawViewportWires(IGH_PreviewArgs args) => Preview_DrawWires(args);

        public GooOpeningParam()
            : base(typeof(IOpening).Name, typeof(IOpening).Name, typeof(IOpening).FullName.Replace(".", " "), "Params", "SAM")
        {
        }

        protected override GH_GetterResult Prompt_Plural(ref List<GooOpening> values)
        {
            global::Rhino.Input.Custom.GetObject getObject = new global::Rhino.Input.Custom.GetObject();
            getObject.SetCommandPrompt("Pick Surfaces to create Openings");
            getObject.GeometryFilter = ObjectType.Brep;
            getObject.SubObjectSelect = true;
            getObject.DeselectAllBeforePostSelect = false;
            getObject.OneByOnePostSelect = false;
            getObject.GetMultiple(1, 0);

            if (getObject.CommandResult() != Result.Success)
                return GH_GetterResult.cancel;

            if (getObject.ObjectCount == 0)
                return GH_GetterResult.cancel;

            values = new List<GooOpening>();

            for (int i = 0; i < getObject.ObjectCount; i++)
            {
                ObjRef objRef = getObject.Object(i);

                RhinoObject rhinoObject = objRef.Object();
                if (rhinoObject == null)
                    return GH_GetterResult.cancel;

                Brep brep = rhinoObject.Geometry as Brep;
                if (brep == null)
                    return GH_GetterResult.cancel;

                List<IOpening> openings = null;

                if (brep.HasUserData)
                {
                    string @string = brep.GetUserString("SAM");
                    if (!string.IsNullOrWhiteSpace(@string))
                    {
                        openings = Core.Convert.ToSAM<IOpening>(@string);
                    }
                }

                if (openings == null || openings.Count == 0)
                {

                    List<ISAMGeometry3D> sAMGeometry3Ds = Geometry.Rhino.Convert.ToSAM(brep);
                    if (sAMGeometry3Ds == null)
                        continue;

                    openings = Create.Openings(sAMGeometry3Ds, Analytical.Query.DefaultOpeningType(OpeningAnalyticalType.Window));
                }

                if (openings == null || openings.Count == 0)
                    continue;

                openings.RemoveAll(x => x == null);

                values.AddRange(openings.ConvertAll(x => new GooOpening(x)));
            }

            return GH_GetterResult.success;
        }

        protected override GH_GetterResult Prompt_Singular(ref GooOpening value)
        {
            global::Rhino.Input.Custom.GetObject getObject = new global::Rhino.Input.Custom.GetObject();
            getObject.SetCommandPrompt("Pick Surface to create Opening");
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

                List<IOpening> openings = null;

                if (brep.HasUserData)
                {
                    string @string = brep.GetUserString("SAM");
                    if (!string.IsNullOrWhiteSpace(@string))
                    {
                        openings = Core.Convert.ToSAM<IOpening>(@string);
                    }
                }

                if (openings == null || openings.Count == 0)
                {

                    List<ISAMGeometry3D> sAMGeometry3Ds = Geometry.Rhino.Convert.ToSAM(brep);
                    if (sAMGeometry3Ds == null)
                        continue;

                    openings = Create.Openings(sAMGeometry3Ds, Analytical.Query.DefaultOpeningType(OpeningAnalyticalType.Window));
                }

                if (openings == null || openings.Count == 0)
                    continue;

                openings.RemoveAll(x => x == null);
                if(openings.Count != 0)
                {
                    value = new GooOpening(openings[0]);
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
    }
}