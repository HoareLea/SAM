// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

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
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper
{
    public class GooAperture : GooJSAMObject<Aperture>, IGH_PreviewData, IGH_BakeAwareData
    {
        private sealed class PreviewSnapshot
        {
            public Aperture Source;
            public long Fingerprint;
            public double UnitScale;
            public List<FacePart> Faces;
        }

        private struct FacePart
        {
            public bool IsFrame;
            public Brep Brep;
        }

        private PreviewSnapshot previewSnapshot;

        public GooAperture()
            : base()
        {
        }

        public GooAperture(Aperture aperture)
            : base(aperture)
        {
        }

        private static PreviewSnapshot BuildPreviewSnapshot(Aperture aperture, double unitScale)
        {
            PreviewSnapshot result = new PreviewSnapshot()
            {
                Source = aperture,
                Fingerprint = ComputeFingerprint(aperture),
                UnitScale = unitScale,
                Faces = new List<FacePart>()
            };

            void AddFace(Face3D face3D, bool isFrame)
            {
                if (face3D == null) return;

                result.Faces.Add(new FacePart()
                {
                    IsFrame = isFrame,
                    Brep = Geometry.Rhino.Convert.ToRhino_Brep(face3D)
                });
            }

            Face3D face3D_Frame = aperture.GetFrameFace3D();
            if (face3D_Frame != null)
            {
                AddFace(face3D_Frame, true);
            }
            else
            {
                List<Face3D> face3Ds_Pane = aperture.GetPaneFace3Ds();
                if (face3Ds_Pane != null)
                {
                    foreach (Face3D f in face3Ds_Pane)
                        AddFace(f, false);
                }
            }

            return result;
        }

        private static long Combine(long hash, long value)
        {
            unchecked
            {
                return hash * 31 + value;
            }
        }

        private static long CombineDouble(long hash, double value)
        {
            return Combine(hash, BitConverter.DoubleToInt64Bits(value));
        }

        private static long CombinePoint3D(long hash, Point3D point3D)
        {
            if (point3D == null)
                return Combine(hash, long.MinValue);

            hash = CombineDouble(hash, point3D.X);
            hash = CombineDouble(hash, point3D.Y);
            hash = CombineDouble(hash, point3D.Z);
            return hash;
        }

        private static long CombineVector3D(long hash, Vector3D vector3D)
        {
            if (vector3D == null)
                return Combine(hash, long.MaxValue);

            hash = CombineDouble(hash, vector3D.X);
            hash = CombineDouble(hash, vector3D.Y);
            hash = CombineDouble(hash, vector3D.Z);
            return hash;
        }

        private static long CombineGuid(long hash, Guid guid)
        {
            Span<byte> bytes = stackalloc byte[16];
            guid.TryWriteBytes(bytes);
            hash = Combine(hash, BinaryPrimitives.ReadInt64LittleEndian(bytes));
            hash = Combine(hash, BinaryPrimitives.ReadInt64LittleEndian(bytes.Slice(8)));
            return hash;
        }

        private static long CombinePlane(long hash, SAM.Geometry.Spatial.Plane plane)
        {
            if (plane == null)
                return Combine(hash, 0);

            hash = CombinePoint3D(hash, plane.Origin);
            hash = CombineVector3D(hash, plane.Normal);
            hash = CombineVector3D(hash, plane.AxisY);
            return hash;
        }

        private static long CombineClosedPlanar3D(long hash, IClosedPlanar3D closedPlanar3D)
        {
            if (closedPlanar3D is ISegmentable3D segmentable3D)
            {
                List<Point3D> point3Ds = segmentable3D.GetPoints();
                if (point3Ds == null)
                    return Combine(hash, -1);

                hash = Combine(hash, point3Ds.Count);
                for (int i = 0; i < point3Ds.Count; i++)
                    hash = CombinePoint3D(hash, point3Ds[i]);

                return hash;
            }

            return Combine(hash, -2);
        }

        private static long CombineFace3D(long hash, Face3D face3D)
        {
            if (face3D == null)
                return Combine(hash, 0);

            hash = CombinePlane(hash, face3D.GetPlane());
            hash = CombineClosedPlanar3D(hash, face3D.GetExternalEdge3D());

            List<IClosedPlanar3D> internalEdge3Ds = face3D.GetInternalEdge3Ds();
            hash = Combine(hash, internalEdge3Ds == null ? 0 : internalEdge3Ds.Count);
            if (internalEdge3Ds != null)
            {
                for (int i = 0; i < internalEdge3Ds.Count; i++)
                    hash = CombineClosedPlanar3D(hash, internalEdge3Ds[i]);
            }

            return hash;
        }

        /// <summary>
        /// Deterministic, ordered geometry fingerprint used to detect in-place
        /// mutation of the same Aperture reference that a reference check alone
        /// would miss. Uses the same Combine design as the GooPanel fingerprint.
        /// </summary>
        private static long ComputeFingerprint(Aperture aperture)
        {
            long hash = 17;
            if (aperture == null)
                return hash;

            hash = CombineGuid(hash, aperture.Guid);
            hash = CombineGuid(hash, aperture.TypeGuid);
            hash = Combine(hash, aperture.ApertureType.GetHashCode());

            Face3D frameFace = aperture.GetFrameFace3D();
            if (frameFace != null)
                hash = CombineFace3D(hash, frameFace);

            List<Face3D> paneFaces = aperture.GetPaneFace3Ds();
            hash = Combine(hash, paneFaces == null ? 0 : paneFaces.Count);
            if (paneFaces != null)
            {
                for (int i = 0; i < paneFaces.Count; i++)
                    hash = CombineFace3D(hash, paneFaces[i]);
            }

            return hash;
        }

        private PreviewSnapshot EnsurePreviewSnapshot()
        {
            Aperture aperture = Value;
            if (aperture == null)
                return null;

            double unitScale = Geometry.Rhino.Query.UnitScale();

            PreviewSnapshot result = previewSnapshot;
            if (result != null && ReferenceEquals(result.Source, aperture) && result.UnitScale.Equals(unitScale))
            {
                if (result.Fingerprint == ComputeFingerprint(aperture))
                    return result;
            }

            result = BuildPreviewSnapshot(aperture, unitScale);
            previewSnapshot = result;
            return result;
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

            if (Value.ApertureConstruction != null)
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
            List<Face3D> face3Ds = new List<Face3D>();

            Face3D face3D = Value?.GetFrameFace3D();
            if (face3D != null)
            {
                face3Ds.Add(face3D);
            }
            else
            {
                List<Face3D> face3Ds_Pane = Value?.GetPaneFace3Ds();
                if (face3Ds_Pane == null)
                {
                    return;
                }

                face3Ds.AddRange(face3Ds_Pane);
            }

            if (face3Ds == null || face3Ds.Count == 0)
            {
                return;
            }

            foreach (Face3D face3D_Temp in face3Ds)
            {
                GooPlanarBoundary3D gooPlanarBoundary3D = new GooPlanarBoundary3D(new PlanarBoundary3D(face3D_Temp));
                gooPlanarBoundary3D.DrawViewportWires(args, color_ExternalEdge, color_InternalEdges);
            }
        }

        public void DrawViewportMeshes(GH_PreviewMeshArgs args)
        {
            if (Value == null)
                return;

            PreviewSnapshot snapshot = EnsurePreviewSnapshot();
            if (snapshot == null || snapshot.Faces == null)
                return;

            DisplayMaterial displayMaterial_Pane = null;
            DisplayMaterial displayMaterial_Frame = null;
            if (Value.ApertureConstruction != null)
            {
                AperturePart aperturePart = Value.ApertureType == ApertureType.Door ? AperturePart.Frame : AperturePart.Pane;

                displayMaterial_Pane = Query.DisplayMaterial(Value.ApertureConstruction.ApertureType, aperturePart);
                displayMaterial_Frame = Query.DisplayMaterial(Value.ApertureConstruction.ApertureType, AperturePart.Frame);
            }

            if (Value.Openable())
            {
                displayMaterial_Pane = Query.DisplayMaterial(Value.ApertureConstruction.ApertureType, AperturePart.Pane, true);
            }

            if (displayMaterial_Pane == null)
                displayMaterial_Pane = args.Material;

            if (displayMaterial_Frame == null)
                displayMaterial_Frame = args.Material;

            foreach (FacePart facePart in snapshot.Faces)
            {
                if (facePart.Brep == null) continue;

                DisplayMaterial mat = facePart.IsFrame ? displayMaterial_Frame : displayMaterial_Pane;
                args.Pipeline.DrawBrepShaded(facePart.Brep, mat);
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

        protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

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
            Menu_AppendItem(menu, "Bake By Type", Menu_BakeByApertureType, Core.Convert.ToBitmap(Resources.SAM3), VolatileData.AllData(true).Any());
            Menu_AppendItem(menu, "Bake By Construction", Menu_BakeByApertureConstruction, Core.Convert.ToBitmap(Resources.SAM3), VolatileData.AllData(true).Any());
            Menu_AppendItem(menu, "Bake By Type With Frame", Menu_BakeByApertureTypeWithFrame, Core.Convert.ToBitmap(Resources.SAM3), VolatileData.AllData(true).Any());
            Menu_AppendItem(menu, "Bake By Construction With Frame", Menu_BakeByApertureConstructionWithFrame, Core.Convert.ToBitmap(Resources.SAM3), VolatileData.AllData(true).Any());
            Menu_AppendItem(menu, "Bake By Discharge Coefficient", Menu_BakeByDischargeCoefficient, Core.Convert.ToBitmap(Resources.SAM3), VolatileData.AllData(true).Any());
            Menu_AppendItem(menu, "Save As...", Menu_SaveAs, Core.Convert.ToBitmap(Resources.SAM3), VolatileData.AllData(true).Any());

            base.AppendAdditionalMenuItems(menu);
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
                if (apertures.Count != 0)
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

        private void Menu_BakeByDischargeCoefficient(object sender, EventArgs e)
        {
            BakeGeometry_ByDischargeCoefficient(RhinoDoc.ActiveDoc);
        }

        public void BakeGeometry_ByDischargeCoefficient(RhinoDoc doc)
        {
            Modify.BakeGeometry_ByDischargeCoefficient(doc, VolatileData);
        }

        public void BakeGeometry_ByApertureType(RhinoDoc doc)
        {
            Modify.BakeGeometry_ByApertureType(doc, VolatileData);
        }

        private void Menu_BakeByApertureConstruction(object sender, EventArgs e)
        {
            BakeGeometry_ByApertureConstruction(RhinoDoc.ActiveDoc);
        }

        public void BakeGeometry_ByApertureConstruction(RhinoDoc doc)
        {
            Modify.BakeGeometry_ByApertureConstruction(doc, VolatileData);
        }

        private void Menu_BakeByApertureTypeWithFrame(object sender, EventArgs e)
        {
            BakeGeometry_ByApertureTypeWithFrame(RhinoDoc.ActiveDoc);
        }

        public void BakeGeometry_ByApertureTypeWithFrame(RhinoDoc doc)
        {
            Modify.BakeGeometry_ByApertureType(doc, VolatileData, true);
        }

        private void Menu_BakeByApertureConstructionWithFrame(object sender, EventArgs e)
        {
            BakeGeometry_ByApertureConstructionWithFrame(RhinoDoc.ActiveDoc);
        }

        public void BakeGeometry_ByApertureConstructionWithFrame(RhinoDoc doc)
        {
            Modify.BakeGeometry_ByApertureConstruction(doc, VolatileData, true);
        }
    }
}
