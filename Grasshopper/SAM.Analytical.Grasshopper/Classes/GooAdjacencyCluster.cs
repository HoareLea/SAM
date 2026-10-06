// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using SAM.Geometry.Spatial;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SAM.Analytical.Grasshopper
{
    public class GooAdjacencyCluster : GooJSAMObject<AdjacencyCluster>, IGH_PreviewData, IGH_BakeAwareData
    {
        /// <summary>
        /// Mesh-only preview cache. Wires and clipping bounds stay live; only the
        /// expensive SAM-to-Rhino Brep conversion is cached. The cache is validated
        /// against a deterministic logical fingerprint of the cluster on every
        /// DrawViewportMeshes call, so in-place mutation of shared (shallow-copied)
        /// objects can never replay stale geometry. Never serialized.
        /// </summary>
        private sealed class MeshPreviewSnapshot
        {
            public long Fingerprint;
            public double UnitScale;
            public List<MeshEntry> Entries;
        }

        /// <summary>
        /// One drawable panel. Keyed by stable identity (TypeName, Guid) instead of
        /// list position so that insertion, removal, reordering or same-Guid
        /// replacement can never associate a panel with another panel's geometry.
        /// </summary>
        private struct MeshEntry
        {
            public string TypeName;
            public Guid Guid;
            public Brep Brep;
            public BoundingBox3D BoundingBox;
        }

        private MeshPreviewSnapshot meshPreviewSnapshot;

        public GooAdjacencyCluster()
            : base()
        {
        }

        public GooAdjacencyCluster(AdjacencyCluster adjacencyCluster)
            : base(adjacencyCluster)
        {
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

        private static long CombineGuid(long hash, Guid guid)
        {
            Span<byte> bytes = stackalloc byte[16];
            guid.TryWriteBytes(bytes);
            hash = Combine(hash, BinaryPrimitives.ReadInt64LittleEndian(bytes));
            hash = Combine(hash, BinaryPrimitives.ReadInt64LittleEndian(bytes.Slice(8)));
            return hash;
        }

        private static long CombineString(long hash, string value)
        {
            if (value == null)
                return Combine(hash, long.MinValue);

            hash = Combine(hash, value.Length);
            for (int i = 0; i < value.Length; i++)
                hash = Combine(hash, value[i]);

            return hash;
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
        /// Deterministic, ordered logical fingerprint of everything that can change
        /// the cached mesh set or geometry: ordered panel identity (runtime type +
        /// Guid), construction identity (TypeGuid), panel type, complete panel
        /// geometry (plane orientation, external and internal edges), aperture
        /// identity, construction and geometry (frame/pane faces derive
        /// deterministically from the fingerprinted base face, ApertureType and
        /// TypeGuid), panel-space relationships (drawability driver) and space
        /// identity. Follows the GooPanel/GooAperture Combine design. Valid across
        /// different shallow-copy wrappers containing the same logical objects.
        /// </summary>
        private static long ComputeFingerprint(AdjacencyCluster adjacencyCluster)
        {
            long hash = 17;
            if (adjacencyCluster == null)
                return hash;

            List<IPanel> panels = adjacencyCluster.GetObjects<IPanel>();
            hash = Combine(hash, panels == null ? 0 : panels.Count);
            if (panels != null)
            {
                foreach (IPanel panel in panels)
                {
                    if (panel == null)
                    {
                        hash = Combine(hash, -3);
                        continue;
                    }

                    hash = CombineString(hash, panel.GetType().FullName);
                    hash = CombineGuid(hash, panel is Core.IGuidObject guidObject ? guidObject.Guid : Guid.Empty);
                    hash = CombineFace3D(hash, panel.Face3D);

                    if (panel is Panel panel_Cast)
                    {
                        hash = CombineGuid(hash, panel_Cast.TypeGuid);
                        hash = Combine(hash, (long)panel_Cast.PanelType);

                        List<Aperture> apertures = panel_Cast.Apertures;
                        hash = Combine(hash, apertures == null ? 0 : apertures.Count);
                        if (apertures != null)
                        {
                            foreach (Aperture aperture in apertures)
                            {
                                if (aperture == null)
                                {
                                    hash = Combine(hash, -4);
                                    continue;
                                }

                                hash = CombineGuid(hash, aperture.Guid);
                                hash = CombineGuid(hash, aperture.TypeGuid);
                                hash = Combine(hash, (long)aperture.ApertureType);
                                hash = CombineFace3D(hash, aperture.GetFace3D());
                            }
                        }
                    }

                    List<ISpace> spaces = adjacencyCluster.GetRelatedObjects<ISpace>(panel);
                    hash = Combine(hash, spaces == null ? 0 : spaces.Count);
                    if (spaces != null)
                    {
                        foreach (ISpace space in spaces)
                            hash = CombineGuid(hash, space is Core.IGuidObject guidObject_Space ? guidObject_Space.Guid : Guid.Empty);
                    }
                }
            }

            List<ISpace> spaces_All = adjacencyCluster.GetObjects<ISpace>();
            hash = Combine(hash, spaces_All == null ? 0 : spaces_All.Count);
            if (spaces_All != null)
            {
                foreach (ISpace space in spaces_All)
                    hash = CombineGuid(hash, space is Core.IGuidObject guidObject_Space ? guidObject_Space.Guid : Guid.Empty);
            }

            return hash;
        }

        private static MeshPreviewSnapshot BuildMeshPreviewSnapshot(AdjacencyCluster adjacencyCluster, long fingerprint, double unitScale)
        {
            MeshPreviewSnapshot result = new MeshPreviewSnapshot()
            {
                Fingerprint = fingerprint,
                UnitScale = unitScale,
                Entries = new List<MeshEntry>()
            };

            List<IPanel> panels = adjacencyCluster.GetObjects<IPanel>();
            if (panels == null)
            {
                return result;
            }

            foreach (IPanel panel in panels)
            {
                if (panel == null)
                {
                    continue;
                }

                List<ISpace> spaces = adjacencyCluster.GetRelatedObjects<ISpace>(panel);
                if (spaces != null && spaces.Count > 1)
                {
                    continue;
                }

                Face3D face3D = panel.Face3D;
                if (face3D == null)
                {
                    continue;
                }

                Brep brep = Geometry.Rhino.Convert.ToRhino_Brep(face3D);
                if (brep == null)
                {
                    continue;
                }

                result.Entries.Add(new MeshEntry()
                {
                    TypeName = panel.GetType().FullName,
                    Guid = panel is Core.IGuidObject guidObject ? guidObject.Guid : Guid.Empty,
                    Brep = brep,
                    BoundingBox = face3D.GetBoundingBox()
                });
            }

            return result;
        }

        public BoundingBox ClippingBox
        {
            get
            {
                if (Value == null)
                {
                    return BoundingBox.Empty;
                }

                List<BoundingBox3D> boundingBox3Ds = new List<BoundingBox3D>();

                IEnumerable<IPanel> panels = Value.GetObjects<IPanel>();
                if (panels != null)
                {
                    foreach (IPanel panel in panels)
                    {
                        BoundingBox3D boundingBox3D = panel?.Face3D?.GetBoundingBox();
                        if (boundingBox3D == null)
                        {
                            continue;
                        }

                        boundingBox3Ds.Add(boundingBox3D);
                    }
                }

                IEnumerable<ISpace> spaces = Value.GetObjects<ISpace>();
                if (spaces != null)
                {
                    foreach (ISpace space in spaces)
                    {
                        if (space == null)
                        {
                            continue;
                        }

                        Point3D location = space.Location;
                        if (location == null)
                        {
                            continue;
                        }

                        boundingBox3Ds.Add(location.GetBoundingBox(1));
                    }
                }

                if (boundingBox3Ds == null)
                {
                    return BoundingBox.Empty;
                }

                boundingBox3Ds.RemoveAll(x => x == null);

                if (boundingBox3Ds.Count == 0)
                {
                    return BoundingBox.Empty;
                }

                return Geometry.Rhino.Convert.ToRhino(new BoundingBox3D(boundingBox3Ds));
            }
        }

        public override IGH_Goo Duplicate()
        {
            return new GooAdjacencyCluster(Value);
        }

        public void DrawViewportWires(GH_PreviewWireArgs args)
        {
            List<ISpace> spaces = Value?.GetObjects<ISpace>();
            if (spaces != null)
            {
                foreach (ISpace space in spaces)
                {
                    Point3d? point3d = Geometry.Rhino.Convert.ToRhino(space?.Location);
                    if (point3d == null || !point3d.HasValue)
                    {
                        continue;
                    }

                    args.Pipeline.DrawPoint(point3d.Value);
                }
            }

            List<IPanel> panels = Value?.GetObjects<IPanel>();
            if (panels == null)
            {
                return;
            }

            BoundingBox3D boundingBox3D = null;
            if (args.Viewport.IsValidFrustum)
            {
                BoundingBox boundingBox = args.Viewport.GetFrustumBoundingBox();
                boundingBox3D = new BoundingBox3D(new Point3D[] { Geometry.Rhino.Convert.ToSAM(boundingBox.Min), Geometry.Rhino.Convert.ToSAM(boundingBox.Max) });
            }

            foreach (IPanel panel in panels)
            {
                List<ISpace> spaces_Panel = Value.GetRelatedObjects<ISpace>(panel);
                if (spaces_Panel != null && spaces_Panel.Count > 1)
                {
                    continue;
                }

                Face3D face3D = panel.Face3D;
                if (face3D == null)
                {
                    continue;
                }

                if (boundingBox3D != null)
                {
                    BoundingBox3D boundingBox3D_Temp = face3D.GetBoundingBox();
                    if (boundingBox3D_Temp != null)
                    {
                        if (!boundingBox3D.Inside(boundingBox3D_Temp) && !boundingBox3D.Intersect(boundingBox3D_Temp))
                        {
                            continue;
                        }
                    }
                }

                Dictionary<IClosedPlanar3D, System.Drawing.Color> dictionary = new Dictionary<IClosedPlanar3D, System.Drawing.Color>();

                //Assign Color for Edges
                dictionary[face3D.GetExternalEdge3D()] = System.Drawing.Color.DarkRed;

                IEnumerable<IClosedPlanar3D> internalEdge3Ds = face3D.GetInternalEdge3Ds();
                if (internalEdge3Ds != null)
                {
                    foreach (IClosedPlanar3D internalEdge3D in internalEdge3Ds)
                    {
                        dictionary[internalEdge3D] = System.Drawing.Color.BlueViolet;
                    }
                }

                foreach (KeyValuePair<IClosedPlanar3D, System.Drawing.Color> keyValuePair in dictionary)
                {
                    ISegmentable3D segmentable3D = keyValuePair.Key as ISegmentable3D;
                    if (segmentable3D == null)
                    {
                        continue;
                    }

                    List<Point3d> point3ds = segmentable3D.GetPoints().ConvertAll(x => Geometry.Rhino.Convert.ToRhino(x));
                    if (point3ds.Count == 0)
                    {
                        continue;
                    }

                    point3ds.Add(point3ds[0]);

                    args.Pipeline.DrawPolyline(point3ds, keyValuePair.Value);
                }
            }
        }

        private MeshPreviewSnapshot EnsureMeshPreviewSnapshot(AdjacencyCluster adjacencyCluster)
        {
            long fingerprint = ComputeFingerprint(adjacencyCluster);
            double unitScale = Geometry.Rhino.Query.UnitScale();

            MeshPreviewSnapshot snapshot = meshPreviewSnapshot;
            if (snapshot == null || snapshot.Fingerprint != fingerprint || !snapshot.UnitScale.Equals(unitScale))
            {
                snapshot = BuildMeshPreviewSnapshot(adjacencyCluster, fingerprint, unitScale);
                meshPreviewSnapshot = snapshot;
            }

            return snapshot;
        }

        public void DrawViewportMeshes(GH_PreviewMeshArgs args)
        {
            AdjacencyCluster adjacencyCluster = Value;
            if (adjacencyCluster == null)
            {
                return;
            }

            MeshPreviewSnapshot snapshot = EnsureMeshPreviewSnapshot(adjacencyCluster);

            BoundingBox3D boundingBox3D = null;
            if (args.Viewport.IsValidFrustum)
            {
                BoundingBox boundingBox = args.Viewport.GetFrustumBoundingBox();
                boundingBox3D = new BoundingBox3D(new Point3D[] { Geometry.Rhino.Convert.ToSAM(boundingBox.Min), Geometry.Rhino.Convert.ToSAM(boundingBox.Max) });
            }

            foreach (MeshEntry meshEntry in snapshot.Entries)
            {
                if (meshEntry.Brep == null)
                {
                    continue;
                }

                if (boundingBox3D != null && meshEntry.BoundingBox != null)
                {
                    if (!boundingBox3D.Inside(meshEntry.BoundingBox) && !boundingBox3D.Intersect(meshEntry.BoundingBox))
                    {
                        continue;
                    }
                }

                args.Pipeline.DrawBrepShaded(meshEntry.Brep, args.Material);
            }
        }

        public bool BakeGeometry(RhinoDoc doc, ObjectAttributes att, out Guid obj_guid)
        {
            obj_guid = Guid.Empty;

            List<IPanel> panels = Value?.GetObjects<IPanel>();
            if (panels == null || panels.Count == 0)
            {
                return false;
            }

            List<Brep> breps = new List<Brep>();
            foreach (IPanel panel in panels)
            {
                List<Brep> breps_Panel = Rhino.Convert.ToRhino_Breps(panel);
                if (breps_Panel == null)
                {
                    continue;
                }

                breps.AddRange(breps_Panel);
            }

            if (breps == null || breps.Count == 0)
            {
                return false;
            }

            Brep result = Brep.MergeBreps(breps, Core.Tolerance.MacroDistance); //Tolerance has been changed from Core.Tolerance.Distance
            if (result == null)
            {
                return false;
            }

            obj_guid = doc.Objects.AddBrep(result);
            return true;
        }

        public override bool CastFrom(object source)
        {
            if (source is AdjacencyCluster)
            {
                Value = (AdjacencyCluster)source;
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

                if (source is AdjacencyCluster)
                {
                    Value = (AdjacencyCluster)source;
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

            //if (typeof(Y).IsAssignableFrom(typeof(GH_Brep)))
            //{
            //    List<Geometry.Spatial.Shell> shells = Value.GetShells();
            //    if(shells != null)
            //    {
            //        Brep brep = Brep.MergeBreps(shells.ConvertAll(x => x.ToRhino()), Core.Tolerance.MacroDistance);
            //        if(brep != null)
            //        {
            //            target = (Y)(object)new GH_Brep(brep);
            //            return true;
            //        }
            //    }
            //}

            return base.CastTo(ref target);
        }
    }

    //Params Components -> SAM used for internalizing data
    public class GooAdjacencyClusterParam : GH_PersistentParam<GooAdjacencyCluster>, IGH_PreviewObject, IGH_BakeAwareObject
    {
        public override Guid ComponentGuid => new Guid("408ca3f4-0598-4f18-8b25-1f9646c53ef0");

        protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        //Here we control name, nickname, description, category, sub-category as deafult we use typeofclass name
        public GooAdjacencyClusterParam()
            : base(typeof(AdjacencyCluster).Name, typeof(AdjacencyCluster).Name, typeof(AdjacencyCluster).FullName.Replace(".", " "), "Params", "SAM")
        {
        }

        protected override GH_GetterResult Prompt_Plural(ref List<GooAdjacencyCluster> values)
        {
            throw new NotImplementedException();
        }

        protected override GH_GetterResult Prompt_Singular(ref GooAdjacencyCluster value)
        {
            throw new NotImplementedException();
        }

        #region IGH_PreviewObject

        bool IGH_PreviewObject.Hidden { get; set; }
        bool IGH_PreviewObject.IsPreviewCapable => !VolatileData.IsEmpty;
        BoundingBox IGH_PreviewObject.ClippingBox => Preview_ComputeClippingBox();

        public bool IsBakeCapable => true;

        void IGH_PreviewObject.DrawViewportMeshes(IGH_PreviewArgs args) => Preview_DrawMeshes(args);

        void IGH_PreviewObject.DrawViewportWires(IGH_PreviewArgs args) => Preview_DrawWires(args);

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
            Modify.BakeGeometry_ByPanelType(doc, VolatileData, true, Core.Tolerance.Distance);
        }

        public void BakeGeometry_ByDischargeCoefficient(RhinoDoc doc)
        {
            Modify.BakeGeometry_ByDischargeCoefficient(doc, VolatileData);
        }

        public void BakeGeometry_ByConstruction(RhinoDoc doc)
        {
            Modify.BakeGeometry_ByConstruction(doc, VolatileData, true, Core.Tolerance.Distance);
        }

        public void BakeGeometry_ByBoundaryType(RhinoDoc doc)
        {
            Modify.BakeGeometry_ByBoundaryType(doc, VolatileData, true, Core.Tolerance.Distance);
        }

        public override void AppendAdditionalMenuItems(System.Windows.Forms.ToolStripDropDown menu)
        {
            Menu_AppendItem(menu, "Bake By Type", Menu_BakeByPanelType, Core.Convert.ToBitmap(Resources.SAM3), VolatileData.AllData(true).Any());
            Menu_AppendItem(menu, "Bake By Construction", Menu_BakeByConstruction, Core.Convert.ToBitmap(Resources.SAM3), VolatileData.AllData(true).Any());
            Menu_AppendItem(menu, "Bake By BoundaryType", Menu_BakeByBoundaryType, Core.Convert.ToBitmap(Resources.SAM3), VolatileData.AllData(true).Any());
            Menu_AppendItem(menu, "Bake By Discharge Coefficient", Menu_BakeByDischargeCoefficient, Core.Convert.ToBitmap(Resources.SAM3), VolatileData.AllData(true).Any());
            Menu_AppendItem(menu, "Save As...", Menu_SaveAs, Core.Convert.ToBitmap(Resources.SAM3), VolatileData.AllData(true).Any());

            if (System.IO.File.Exists(Query.AnalyticalUIPath()))
            {
                Menu_AppendItem(menu, "Open in UI", Menu_OpenInUI, Core.Convert.ToBitmap(Resources.SAM3), VolatileData.AllData(true).Any());
            }

            base.AppendAdditionalMenuItems(menu);
        }

        private void Menu_BakeByPanelType(object sender, EventArgs e)
        {
            BakeGeometry_ByPanelType(RhinoDoc.ActiveDoc);
        }

        private void Menu_BakeByDischargeCoefficient(object sender, EventArgs e)
        {
            BakeGeometry_ByDischargeCoefficient(RhinoDoc.ActiveDoc);
        }

        private void Menu_BakeByConstruction(object sender, EventArgs e)
        {
            BakeGeometry_ByConstruction(RhinoDoc.ActiveDoc);
        }

        private void Menu_BakeByBoundaryType(object sender, EventArgs e)
        {
            BakeGeometry_ByBoundaryType(RhinoDoc.ActiveDoc);
        }

        private void Menu_SaveAs(object sender, EventArgs e)
        {
            Core.Grasshopper.Query.SaveAs(VolatileData);
        }

        private void Menu_OpenInUI(object sender, EventArgs e)
        {
            Process process = Convert.ToUI(VolatileData);
        }

        #endregion IGH_PreviewObject
    }
}
