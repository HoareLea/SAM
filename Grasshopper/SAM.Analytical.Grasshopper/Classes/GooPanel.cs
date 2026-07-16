// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using GH_IO.Serialization;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Geometry;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using SAM.Geometry.Grasshopper;
using SAM.Geometry.Object.Spatial;
using SAM.Geometry.Spatial;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

namespace SAM.Analytical.Grasshopper
{
    public class GooPanel : GooJSAMObject<IPanel>, IGH_PreviewData, IGH_BakeAwareData
    {
        private sealed class PreviewSnapshot
        {
            public IPanel Source;
            public double UnitScale;
            public long Fingerprint;
            public BoundingBox ClippingBox;
            public Face3D Face3D;
            public Brep Brep;
            public List<WireLoop> PanelLoops;
            public List<ApertureLoop> ApertureLoops;
        }

        private struct WireLoop
        {
            public List<Point3d> Points;
            public bool Internal;
        }

        private struct ApertureLoop
        {
            public List<Point3d> Points;
            public System.Drawing.Color Color;
        }

        private PreviewSnapshot previewSnapshot;

        public BoundaryType? BoundaryType { get; } = null;

        public bool ShowAll { get; set; } = true;

        public GooPanel()
            : base()
        {
        }

        public GooPanel(IPanel panel)
            : base(panel)
        {
        }

        public GooPanel(IPanel panel, BoundaryType? boundaryType)
            : base(panel)
        {
            BoundaryType = boundaryType;
        }

        private static long Combine(long hash, long value)
        {
            unchecked
            {
                return (hash * 31) ^ value;
            }
        }

        private static long CombineDouble(long hash, double value)
        {
            return Combine(hash, BitConverter.DoubleToInt64Bits(value));
        }

        private static long CombinePoint3D(long hash, Point3D point3D)
        {
            if (point3D == null)
            {
                return Combine(hash, long.MinValue);
            }

            hash = CombineDouble(hash, point3D.X);
            hash = CombineDouble(hash, point3D.Y);
            hash = CombineDouble(hash, point3D.Z);
            return hash;
        }

        private static long CombineVector3D(long hash, Vector3D vector3D)
        {
            if (vector3D == null)
            {
                return Combine(hash, long.MaxValue);
            }

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

        private static long CombinePlane(long hash, Geometry.Spatial.Plane plane)
        {
            if (plane == null)
            {
                return Combine(hash, 0);
            }

            // Origin + normal + axisY fully capture position and orientation,
            // including flips (FlipNormal reverses normal and winding) without
            // being affected by re-derivation of the redundant axisX.
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
                {
                    return Combine(hash, -1);
                }

                hash = Combine(hash, point3Ds.Count);
                for (int i = 0; i < point3Ds.Count; i++)
                {
                    hash = CombinePoint3D(hash, point3Ds[i]);
                }

                return hash;
            }

            return Combine(hash, -2);
        }

        private static long CombineFace3D(long hash, Face3D face3D)
        {
            if (face3D == null)
            {
                return Combine(hash, 0);
            }

            hash = CombinePlane(hash, face3D.GetPlane());
            hash = CombineClosedPlanar3D(hash, face3D.GetExternalEdge3D());

            List<IClosedPlanar3D> internalEdge3Ds = face3D.GetInternalEdge3Ds();
            hash = Combine(hash, internalEdge3Ds == null ? 0 : internalEdge3Ds.Count);
            if (internalEdge3Ds != null)
            {
                for (int i = 0; i < internalEdge3Ds.Count; i++)
                {
                    hash = CombineClosedPlanar3D(hash, internalEdge3Ds[i]);
                }
            }

            return hash;
        }

        /// <summary>
        /// Deterministic, ordered geometry fingerprint used to detect in-place
        /// mutation of the same <see cref="IPanel"/> reference (e.g. FlipNormal,
        /// boundary edits, aperture add/remove/replace or aperture geometry change)
        /// that a reference or bounding-box check alone would miss. Uses exact
        /// double bit patterns and ordered hash combination (no XOR-only, no
        /// process-dependent GetHashCode, no temporary arrays or strings beyond the
        /// point lists the public geometry API itself returns).
        /// </summary>
        private static long ComputeFingerprint(IPanel panel)
        {
            long hash = 17;
            if (panel == null)
            {
                return hash;
            }

            hash = CombineFace3D(hash, panel.Face3D);

            if (panel is Panel panel_Temp && panel_Temp.HasApertures)
            {
                List<Aperture> apertures = panel_Temp.Apertures;
                hash = Combine(hash, apertures == null ? 0 : apertures.Count);
                if (apertures != null)
                {
                    for (int i = 0; i < apertures.Count; i++)
                    {
                        Aperture aperture = apertures[i];
                        if (aperture == null)
                        {
                            hash = Combine(hash, 0);
                            continue;
                        }

                        hash = CombineGuid(hash, aperture.Guid);
                        hash = CombineGuid(hash, aperture.TypeGuid);
                        hash = CombineFace3D(hash, aperture.GetFace3D());

                        // Wire geometry is drawn from GetFrameFace3D/GetPaneFace3Ds,
                        // not the outer GetFace3D. Hash the drawn faces so construction
                        // edits (frame width, layers) with the same TypeGuid invalidate.
                        Face3D frame3D = aperture.GetFrameFace3D();
                        if (frame3D != null)
                        {
                            hash = CombineFace3D(hash, frame3D);
                        }
                        else
                        {
                            List<Face3D> pane3Ds = aperture.GetPaneFace3Ds();
                            hash = Combine(hash, pane3Ds == null ? 0 : pane3Ds.Count);
                            if (pane3Ds != null)
                            {
                                for (int iPane = 0; iPane < pane3Ds.Count; iPane++)
                                    hash = CombineFace3D(hash, pane3Ds[iPane]);
                            }
                        }
                    }
                }
            }
            else
            {
                hash = Combine(hash, 0);
            }

            return hash;
        }

        private static void AddWireLoops(PlanarBoundary3D planarBoundary3D, Action<List<Point3d>, bool> add)
        {
            if (planarBoundary3D == null)
                return;

            BoundaryEdge3DLoop externalLoop = planarBoundary3D.GetExternalEdge3DLoop();
            if (externalLoop != null)
            {
                List<BoundaryEdge3D> edge3Ds = externalLoop.BoundaryEdge3Ds;
                if (edge3Ds != null && edge3Ds.Count != 0)
                {
                    List<Point3d> point3ds = edge3Ds.ConvertAll(x => Geometry.Rhino.Convert.ToRhino(x.Curve3D.GetStart()));
                    if (point3ds.Count != 0)
                    {
                        point3ds.Add(point3ds[0]);
                        add(point3ds, false);
                    }
                }
            }

            List<BoundaryEdge3DLoop> internalLoops = planarBoundary3D.GetInternalEdge3DLoops();
            if (internalLoops != null)
            {
                foreach (BoundaryEdge3DLoop internalLoop in internalLoops)
                {
                    List<BoundaryEdge3D> edge3Ds = internalLoop?.BoundaryEdge3Ds;
                    if (edge3Ds == null || edge3Ds.Count == 0)
                        continue;

                    List<Point3d> point3ds = edge3Ds.ConvertAll(x => Geometry.Rhino.Convert.ToRhino(x.Curve3D.GetStart()));
                    if (point3ds.Count == 0)
                        continue;

                    point3ds.Add(point3ds[0]);
                    add(point3ds, true);
                }
            }
        }

        private static PreviewSnapshot BuildPreviewSnapshot(IPanel panel, double unitScale)
        {
            PreviewSnapshot result = new PreviewSnapshot()
            {
                Source = panel,
                UnitScale = unitScale,
                PanelLoops = new List<WireLoop>(),
                ApertureLoops = new List<ApertureLoop>()
            };

            Face3D face3D = panel.Face3D;
            Geometry.Spatial.BoundingBox3D boundingBox3D = face3D?.GetBoundingBox();

            result.Face3D = face3D;
            result.Fingerprint = ComputeFingerprint(panel);
            result.ClippingBox = Geometry.Rhino.Convert.ToRhino(boundingBox3D);

            if (face3D != null)
            {
                result.Brep = Geometry.Rhino.Convert.ToRhino_Brep(face3D);
            }

            if (panel is ExternalPanel)
            {
                if (face3D != null)
                {
                    AddWireLoops(new PlanarBoundary3D(face3D), (points, @internal) => result.PanelLoops.Add(new WireLoop() { Points = points, Internal = @internal }));
                }

                return result;
            }

            Panel panel_Temp = panel as Panel;
            if (panel_Temp == null)
            {
                return result;
            }

            AddWireLoops(panel_Temp.PlanarBoundary3D, (points, @internal) => result.PanelLoops.Add(new WireLoop() { Points = points, Internal = @internal }));

            List<Aperture> apertures = panel_Temp.Apertures;
            if (apertures != null)
            {
                foreach (Aperture aperture in apertures)
                {
                    if (aperture == null)
                        continue;

                    System.Drawing.Color color_ExternalEdge = System.Drawing.Color.Empty;
                    System.Drawing.Color color_InternalEdges = System.Drawing.Color.Empty;

                    if (aperture.ApertureConstruction != null)
                    {
                        color_ExternalEdge = Analytical.Query.Color(aperture.ApertureConstruction.ApertureType, false);
                        color_InternalEdges = Analytical.Query.Color(aperture.ApertureConstruction.ApertureType, true);
                    }

                    if (color_ExternalEdge == System.Drawing.Color.Empty)
                        color_ExternalEdge = System.Drawing.Color.DarkRed;

                    if (color_InternalEdges == System.Drawing.Color.Empty)
                        color_InternalEdges = System.Drawing.Color.BlueViolet;

                    List<Face3D> face3Ds = new List<Face3D>();

                    Face3D face3D_Frame = aperture.GetFrameFace3D();
                    if (face3D_Frame != null)
                    {
                        face3Ds.Add(face3D_Frame);
                    }
                    else
                    {
                        List<Face3D> face3Ds_Pane = aperture.GetPaneFace3Ds();
                        if (face3Ds_Pane == null)
                            continue;

                        face3Ds.AddRange(face3Ds_Pane);
                    }

                    foreach (Face3D face3D_Temp in face3Ds)
                    {
                        AddWireLoops(new PlanarBoundary3D(face3D_Temp), (points, @internal) => result.ApertureLoops.Add(new ApertureLoop() { Points = points, Color = @internal ? color_InternalEdges : color_ExternalEdge }));
                    }
                }
            }

            return result;
        }

        private PreviewSnapshot EnsurePreviewSnapshot()
        {
            IPanel panel = Value;
            if (panel == null)
                return null;

            double unitScale = Geometry.Rhino.Query.UnitScale();

            PreviewSnapshot result = previewSnapshot;
            if (result != null && ReferenceEquals(result.Source, panel) && result.UnitScale.Equals(unitScale))
            {
                if (result.Fingerprint == ComputeFingerprint(panel))
                {
                    return result;
                }
            }

            result = BuildPreviewSnapshot(panel, unitScale);
            previewSnapshot = result;
            return result;
        }

        public BoundingBox ClippingBox
        {
            get
            {
                PreviewSnapshot snapshot = EnsurePreviewSnapshot();
                if (snapshot == null)
                    return BoundingBox.Empty;

                return snapshot.ClippingBox;
            }
        }

        public override IGH_Goo Duplicate()
        {
            GooPanel result = new GooPanel(Value, BoundaryType);
            result.ShowAll = ShowAll;

            return result;
        }

        public void DrawViewportWires(GH_PreviewWireArgs args)
        {
            if (Value == null)
            {
                return;
            }

            PreviewSnapshot snapshot = EnsurePreviewSnapshot();
            if (snapshot == null)
            {
                return;
            }

            System.Drawing.Color color_ExternalEdge;
            System.Drawing.Color color_InternalEdges;

            if (Value is ExternalPanel)
            {
                System.Drawing.Color color = Analytical.Query.Color((ExternalPanel)Value);
                color_ExternalEdge = color;
                color_InternalEdges = color;
            }
            else if (Value is Panel panel)
            {
                color_ExternalEdge = Analytical.Query.Color(panel.PanelType, false);
                color_InternalEdges = Analytical.Query.Color(panel.PanelType, true);

                if (color_ExternalEdge == System.Drawing.Color.Empty)
                    color_ExternalEdge = args.Color;

                if (color_InternalEdges == System.Drawing.Color.Empty)
                    color_InternalEdges = args.Color;
            }
            else
            {
                return;
            }

            foreach (WireLoop wireLoop in snapshot.PanelLoops)
            {
                args.Pipeline.DrawPolyline(wireLoop.Points, wireLoop.Internal ? color_InternalEdges : color_ExternalEdge);
            }

            foreach (ApertureLoop apertureLoop in snapshot.ApertureLoops)
            {
                args.Pipeline.DrawPolyline(apertureLoop.Points, apertureLoop.Color);
            }
        }

        public void DrawViewportMeshes(GH_PreviewMeshArgs args)
        {
            if (Value == null)
            {
                return;
            }

            PreviewSnapshot snapshot = EnsurePreviewSnapshot();
            if (snapshot == null || snapshot.Face3D == null)
            {
                return;
            }

            if (!ShowAll)
            {
                Point3D point3D_CameraLocation = Geometry.Rhino.Convert.ToSAM(RhinoDoc.ActiveDoc.Views.ActiveView.ActiveViewport.CameraLocation);
                if (point3D_CameraLocation == null)
                {
                    return;
                }

                double distance = snapshot.Face3D.Distance(point3D_CameraLocation);
                if (distance < 8 || distance > 15)
                {
                    return;
                }
            }

            global::Rhino.Display.DisplayMaterial displayMaterial = BoundaryType != null && BoundaryType.HasValue ? Query.DisplayMaterial(BoundaryType.Value) : Value is Panel ? Query.DisplayMaterial(((Panel)Value).PanelType) : Query.DisplayMaterial(Value as ExternalPanel);
            if (displayMaterial == null)
            {
                displayMaterial = args.Material;
            }

            if (snapshot.Brep == null)
            {
                return;
            }

            args.Pipeline.DrawBrepShaded(snapshot.Brep, displayMaterial);

            // Note: the pre-snapshot implementation iterated aperture edge loops and
            // passed them to Geometry.Grasshopper.Modify.DrawViewportMeshes via
            // GooSAMGeometry. IClosedPlanar3D matches no branch in that method, so no
            // geometry was ever drawn for apertures in the mesh pass. The traversal is
            // intentionally not replicated here; visual output is unchanged.
        }

        public bool BakeGeometry(RhinoDoc doc, ObjectAttributes att, out Guid obj_guid)
        {
            obj_guid = Guid.Empty;
            if (Value == null)
            {
                return false;
            }

            bool result = Rhino.Modify.BakeGeometry(Value, doc, att, out List<Guid> obj_guids);
            if (!result)
            {
                return false;
            }

            if (obj_guids == null || obj_guids.Count == 0)
            {
                return false;
            }

            if (obj_guids.Count == 1)
            {
                obj_guid = obj_guids[0];
                return result;
            }

            int index = doc.Groups.Add(Value.Guid.ToString());

            Group group = doc.Groups.ElementAt(index);
            foreach (Guid guid in obj_guids)
            {
                doc.Groups.AddToGroup(index, guid);
            }

            obj_guid = group.Id;
            return result;
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

    public class GooPanelParam : GH_PersistentParam<GooPanel>, IGH_PreviewObject, IGH_BakeAwareObject
    {
        private bool showAll = true;

        public override Guid ComponentGuid => new Guid("278B438C-43EA-4423-999F-B6A906870939");

        protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        bool IGH_PreviewObject.Hidden { get; set; }

        bool IGH_PreviewObject.IsPreviewCapable => !VolatileData.IsEmpty;

        BoundingBox IGH_PreviewObject.ClippingBox => Preview_ComputeClippingBox();

        public bool IsBakeCapable => true;

        void IGH_PreviewObject.DrawViewportMeshes(IGH_PreviewArgs args)
        {
            foreach (var variable in VolatileData.AllData(true))
            {
                GooPanel gooPanel = variable as GooPanel;
                if (gooPanel == null)
                    continue;

                gooPanel.ShowAll = showAll;
            }

            Preview_DrawMeshes(args);
        }

        void IGH_PreviewObject.DrawViewportWires(IGH_PreviewArgs args)
        {
            foreach (var variable in VolatileData.AllData(true))
            {
                GooPanel gooPanel = variable as GooPanel;
                if (gooPanel == null)
                    continue;

                gooPanel.ShowAll = showAll;
            }

            Preview_DrawWires(args);
        }

        public GooPanelParam()
            : base(typeof(Panel).Name, typeof(Panel).Name, typeof(Panel).FullName.Replace(".", " "), "Params", "SAM")
        {
        }

        protected override GH_GetterResult Prompt_Plural(ref List<GooPanel> values)
        {
            global::Rhino.Input.Custom.GetObject getObject = new global::Rhino.Input.Custom.GetObject();
            getObject.SetCommandPrompt("Pick Surfaces to create panels");
            getObject.GeometryFilter = ObjectType.Brep;
            getObject.SubObjectSelect = true;
            getObject.DeselectAllBeforePostSelect = false;
            getObject.OneByOnePostSelect = false;
            getObject.GetMultiple(1, 0);

            if (getObject.CommandResult() != Result.Success)
                return GH_GetterResult.cancel;

            if (getObject.ObjectCount == 0)
                return GH_GetterResult.cancel;

            values = new List<GooPanel>();

            for (int i = 0; i < getObject.ObjectCount; i++)
            {
                ObjRef objRef = getObject.Object(i);

                RhinoObject rhinoObject = objRef.Object();
                if (rhinoObject == null)
                    return GH_GetterResult.cancel;

                Brep brep = rhinoObject.Geometry as Brep;
                if (brep == null)
                    return GH_GetterResult.cancel;

                List<ISAMGeometry3D> sAMGeometry3Ds = Geometry.Rhino.Convert.ToSAM(brep);
                if (sAMGeometry3Ds == null)
                {
                    continue;
                }

                List<Panel> panels = Create.Panels(sAMGeometry3Ds);
                if (brep.HasUserData)
                {
                    List<Panel> panels_Old = null;
                    string @string = brep.GetUserString("SAM");
                    if (!string.IsNullOrWhiteSpace(@string))
                    {
                        panels_Old = Core.Convert.ToSAM<Panel>(@string);
                        if (panels_Old != null)
                        {
                            panels_Old.RemoveAll(x => x == null);
                        }

                        if (panels_Old != null && panels_Old.Count != 0)
                        {
                            for (int j = 0; j < panels.Count; j++)
                            {
                                Panel panel_Old = panels_Old.Closest(panels[j].GetInternalPoint3D());
                                if (panel_Old != null)
                                {
                                    panels[j] = Create.Panel(panel_Old.Guid, panel_Old, panels[j].GetFace3D());
                                }

                            }
                        }
                    }
                }

                if (panels == null || panels.Count == 0)
                {
                    continue;
                }

                values.AddRange(panels.FindAll(x => x != null).ConvertAll(x => new GooPanel(x)));
            }

            return GH_GetterResult.success;
        }

        protected override GH_GetterResult Prompt_Singular(ref GooPanel value)
        {
            global::Rhino.Input.Custom.GetObject getObject = new global::Rhino.Input.Custom.GetObject();
            getObject.SetCommandPrompt("Pick Surface to create panel");
            getObject.GeometryFilter = ObjectType.Brep;
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

                Brep brep = rhinoObject.Geometry as Brep;
                if (brep == null)
                    return GH_GetterResult.cancel;

                List<ISAMGeometry3D> sAMGeometry3Ds = Geometry.Rhino.Convert.ToSAM(brep);
                if (sAMGeometry3Ds == null)
                {
                    continue;
                }

                List<Panel> panels = Create.Panels(sAMGeometry3Ds);
                if (brep.HasUserData)
                {
                    List<Panel> panels_Old = null;
                    string @string = brep.GetUserString("SAM");
                    if (!string.IsNullOrWhiteSpace(@string))
                    {
                        panels_Old = Core.Convert.ToSAM<Panel>(@string);
                        if (panels_Old != null)
                        {
                            panels_Old.RemoveAll(x => x == null);
                        }

                        if (panels_Old != null && panels_Old.Count != 0)
                        {
                            for (int j = 0; j < panels.Count; j++)
                            {
                                Panel panel_Old = panels_Old.Closest(panels[j].GetInternalPoint3D());
                                if (panel_Old != null)
                                {
                                    panels[j] = Create.Panel(panel_Old.Guid, panel_Old, panels[j].GetFace3D());
                                }

                            }
                        }
                    }
                }

                if (panels == null || panels.Count == 0)
                {
                    continue;
                }

                value = new GooPanel(panels[0]);
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

        public void BakeGeometry_ByPanelType(RhinoDoc doc)
        {
            Modify.BakeGeometry_ByPanelType(doc, VolatileData, false, Core.Tolerance.Distance);
        }

        private void Menu_BakeByDischargeCoefficient(object sender, EventArgs e)
        {
            Modify.BakeGeometry_ByDischargeCoefficient(RhinoDoc.ActiveDoc, VolatileData);
        }

        public void BakeGeometry_ByConstruction(RhinoDoc doc)
        {
            Modify.BakeGeometry_ByConstruction(doc, VolatileData, false, Core.Tolerance.Distance);
        }

        public override void AppendAdditionalMenuItems(ToolStripDropDown menu)
        {
            Menu_AppendItem(menu, "Show All", Menu_ShowAll, Core.Convert.ToBitmap(Resources.SAM3), VolatileData.AllData(true).Any(), showAll).Tag = showAll;

            Menu_AppendItem(menu, "Bake By Type", Menu_BakeByPanelType, Core.Convert.ToBitmap(Resources.SAM3), VolatileData.AllData(true).Any());
            Menu_AppendItem(menu, "Bake By Construction", Menu_BakeByConstruction, Core.Convert.ToBitmap(Resources.SAM3), VolatileData.AllData(true).Any());
            Menu_AppendItem(menu, "Bake By Discharge Coefficient", Menu_BakeByDischargeCoefficient, Core.Convert.ToBitmap(Resources.SAM3), VolatileData.AllData(true).Any());
            Menu_AppendItem(menu, "Save As...", Menu_SaveAs, Core.Convert.ToBitmap(Resources.SAM3), VolatileData.AllData(true).Any());

            if (System.IO.File.Exists(Query.AnalyticalUIPath()))
            {
                Menu_AppendItem(menu, "Open in UI", Menu_OpenInUI, Core.Convert.ToBitmap(Resources.SAM3), VolatileData.AllData(true).Any());
            }

            base.AppendAdditionalMenuItems(menu);
        }

        private void Menu_SaveAs(object sender, EventArgs e)
        {
            Core.Grasshopper.Query.SaveAs(VolatileData);
        }

        private void Menu_BakeByPanelType(object sender, EventArgs e)
        {
            BakeGeometry_ByPanelType(RhinoDoc.ActiveDoc);
        }

        private void Menu_ShowAll(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem item && item.Tag is bool)
            {
                showAll = !(bool)item.Tag;
                ExpirePreview(true);
            }
        }

        private void Menu_BakeByConstruction(object sender, EventArgs e)
        {
            BakeGeometry_ByConstruction(RhinoDoc.ActiveDoc);
        }

        public override bool Write(GH_IWriter writer)
        {
            writer.SetBoolean(GetType().FullName, showAll);

            return base.Write(writer);
        }

        public override bool Read(GH_IReader reader)
        {
            if (reader != null)
                reader.TryGetBoolean(GetType().FullName, ref showAll);

            return base.Read(reader);
        }

        private void Menu_OpenInUI(object sender, EventArgs e)
        {
            Process process = Convert.ToUI(VolatileData);
        }
    }
}
