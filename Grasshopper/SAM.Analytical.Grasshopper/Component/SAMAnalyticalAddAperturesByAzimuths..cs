using Grasshopper.Kernel;
using Rhino.Geometry;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    /// <summary>
    /// Add apertures to panels based on azimuth sectors (WWR per direction).
    /// </summary>
    /// <remarks>
    /// <para>
    /// SUMMARY
    ///   • Adds window apertures by azimuth (compass direction), using target WWR per sector.
    ///   • Works on a single Panel, an AdjacencyCluster, or an AnalyticalModel.
    ///   • Azimuth convention: 0° = North, 90° = East, clockwise in the XY plane. 0° ≡ 360°.
    /// </para>
    /// <para>
    /// INPUTS
    ///   _analyticalObject  (Panel | AdjacencyCluster | AnalyticalModel, required)
    ///     The SAM object to receive apertures.
    ///     • Panel → adds apertures to this panel only.
    ///     • AdjacencyCluster / AnalyticalModel → scans external walls and adds apertures by sector.
    ///
    ///   _ratios  (Number[], optional)
    ///     Target WWRs (0–1) per azimuth sector, in the same order as _azimuths.
    ///     You may pass one value (broadcast to all) or a list; if the list is shorter
    ///     than _azimuths, the last value is reused. Percent inputs (>1) are treated as %/100.
    ///     Default (4-way N/E/S/W): 0.15, 0.20, 0.25, 0.20.
    ///
    ///   _azimuths  (Domain[], optional)
    ///     Sector domains in degrees (inclusive). Defaults (4-way):
    ///       North 316→44  (wraps internally 316→359 & 0→44)
    ///       East   45→134
    ///       South 135→225
    ///       West  226→315
    ///     Use degenerate domains for fixed bearings (e.g., 90→90).
    ///     Wrapped domains (T0 > T1) are handled automatically.
    ///
    ///   _apertureConstructions_  (ApertureConstruction[], optional)
    ///     Optional construction(s) used per sector (1:1 with _azimuths; one value is broadcast).
    ///
    ///   _subdivide_  (Boolean, optional, default = true)
    ///     If true, splits the band into multiple openings; spacing set by _horizontalSeparation_.
    ///
    ///   _apertureHeight_  (Number, optional, default = 2.5 m)
    ///     Aperture height.
    ///
    ///   _sillHeight_  (Number, optional, default = 0.85 m)
    ///     Sill height. Keep sill ~0.85–1.0 m for typical classrooms/offices.
    ///
    ///   _horizontalSeparation_  (Number, optional, default = 3 m)
    ///     Horizontal gap between subdivided openings.
    ///
    ///   _apertureConstruction_  (ApertureConstruction, optional)
    ///     Single construction for all sectors. If both list and single are provided,
    ///     per-sector list takes precedence.
    ///
    ///   _keepSeparationDistance_  (Boolean, optional, default = false)
    ///     If true, keeps the separation distance even when geometry is tight.
    ///
    ///   _offset_  (Number, optional, default = 0.1 m)
    ///     Minimum offset from the panel boundary to apertures.
    /// </para>
    /// <para>
    /// OUTPUTS
    ///   AnalyticalObject  (Panel | AdjacencyCluster | AnalyticalModel)
    ///     The updated object (Panel if a single panel was input; otherwise the updated cluster / model).
    ///
    ///   Apertures  (Aperture[])
    ///     The apertures created.
    ///
    ///   Successful  (Boolean)
    ///     True if at least one aperture was created.
    /// </para>
    /// <para>
    /// NOTES
    ///   • Ratios and sectors pair by index. If counts differ: one ratio is broadcast; missing ratios reuse the last; extras are ignored.
    ///   • 8-way sets are supported (N, NE, E, SE, S, SW, W, NW) by providing 8 domains and 8 ratios.
    ///   • Domains are inclusive; 0° is North and equivalent to 360°.
    /// </para>
    /// <para>
    /// EXAMPLE
    ///   1) Provide an AnalyticalModel → _analyticalObject
    ///   2) Use defaults (4-way) or supply 8 sectors and 8 WWRs
    ///   3) Set _apertureHeight_ = 2.5, _sillHeight_ = 0.9, _subdivide_ = true, _horizontalSeparation_ = 3
    ///   4) (Optional) Provide constructions per sector
    ///   5) Get updated model and created apertures
    /// </para>
    /// </remarks>
    public class SAMAnalyticalAddAperturesByAzimuths : GH_SAMVariableOutputParameterComponent
    {
        public override Guid ComponentGuid => new("84d34834-8ce0-42cb-a3de-7366337bac4a");
        public override string LatestComponentVersion => "1.0.8";
        protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);
        public override GH_Exposure Exposure => GH_Exposure.primary;

        public SAMAnalyticalAddAperturesByAzimuths()
          : base("SAMAnalytical.AddAperturesByAzimuths",
                 "AddAperturesByAzimuths",
                 DescriptionLong,
                 "SAM", "Analytical")
        { }

        private const string DescriptionLong =
@"Add apertures to panels by azimuth sector with target WWR per direction.

SUMMARY
  • Works on a Panel / AdjacencyCluster / AnalyticalModel.
  • Azimuth: 0° = North, 90° = East, clockwise; 0° ≡ 360°.
  • Pairs _azimuths and _ratios; handles broadcasting and wrap-around.

INPUTS
  _analyticalObject  (Panel | AdjacencyCluster | AnalyticalModel, required)
    Target SAM object to receive apertures.

  _ratios  (Number[], optional; default N/E/S/W = 0.15, 0.20, 0.25, 0.20)
    WWR per sector in the same order as _azimuths. One value broadcasts to all.
    Values > 1 are treated as percentages (divided by 100).

  _azimuths  (Domain[], optional; default N/E/S/W as 316→44, 45→134, 135→225, 226→315)
    Inclusive sector domains in degrees. Wrapped ranges (T0 > T1) are split internally.

  _apertureConstructions_  (ApertureConstruction[], optional)
    Per-sector constructions (1:1 with _azimuths). One value broadcasts.

  _subdivide_  (Boolean, optional, default = true)
    Split the band into multiple openings (spacing via _horizontalSeparation_).

  _apertureHeight_  (Number, optional, default = 2.5 m)
    Aperture height.

  _sillHeight_  (Number, optional, default = 0.85 m)
    Sill height.

  _horizontalSeparation_  (Number, optional, default = 3 m)
    Gap between subdivided openings.

  _apertureConstruction_  (ApertureConstruction, optional)
    Single construction used when per-sector list is not provided.

  _keepSeparationDistance_  (Boolean, optional, default = false)
    Keep the separation distance even in tight geometry.

  _offset_  (Number, optional, default = 0.1 m)
    Minimum offset from panel edge to apertures.

OUTPUTS
  AnalyticalObject  (Panel | AdjacencyCluster | AnalyticalModel)
    Updated object with apertures applied.

  Apertures  (Aperture[])
    Created apertures.

  Successful  (Boolean)
    True if any apertures were created.";

        // ────────────────────────────────────────────────────────────────────────────────
        // Parameters
        // ────────────────────────────────────────────────────────────────────────────────

        protected override GH_SAMParam[] Inputs
        {
            get
            {
                List<GH_SAMParam> result = [];

                var gooAnalyticalObjectParam = new GooAnalyticalObjectParam
                {
                    Name = "_analyticalObject",
                    NickName = "_analyticalObject",
                    Description = "Panel, AdjacencyCluster, or AnalyticalModel to receive apertures.",
                    Access = GH_ParamAccess.item
                };
                result.Add(new GH_SAMParam(gooAnalyticalObjectParam, ParamVisibility.Binding));

                // Ratios (N, E, S, W)
                var ratiosParam = new global::Grasshopper.Kernel.Parameters.Param_Number
                {
                    Name = "_ratios",
                    NickName = "_ratios",
                    Description =
                        "Directional ratios applied to azimuth sectors in list order: [North, East, South, West].\n" +
                        "Typical range: 0.0–1.0.\n" +
                        "Defaults:\n" +
                        "  North = 0.15, East = 0.2, South = 0.25, West = 0.2\n" +
                        "  Ratios (0.0–1.0) are target WWRs; for each matched panel an aperture is created\n" +
                        "  by scaling the panel polygon in its local plane so that the aperture area ≈ (ratio × panel area).",
                    Access = GH_ParamAccess.list,
                    Optional = true
                };
                ratiosParam.SetPersistentData(0.15, 0.2, 0.25, 0.2);
                result.Add(new GH_SAMParam(ratiosParam, ParamVisibility.Binding));

                // Azimuths
                var azimuthsParam = new global::Grasshopper.Kernel.Parameters.Param_Interval
                {
                    Name = "_azimuths",
                    NickName = "_azimuths",
                    Description =
                        "Azimuth sectors [°] normally for 8 or 4 directions (N, E, S, W) in the same order as _ratios.\n" +
                        "Defaults:\n" +
                        "  North : 316 to 44   (wrap; internally split to 316→359 and 0→44)\n" +
                        "  East  : 45  to 134\n" +
                        "  South : 135 to 225\n" +
                        "  West  : 226 to 315\n" +
                        "Notes:\n" +
                        "• If a sector Domain has T0 > T1 (e.g. 338 to 22), it is treated as wrap-around and split internally.\n" +
                        "• For a fixed direction, use a degenerate Domain, East e.g. 90 to 90.\n" +
                        "• Domains are inclusive; 0° is treated as North (and equivalent to 360°).",
                    Access = GH_ParamAccess.list,
                    Optional = true
                };

                azimuthsParam.SetPersistentData(
                    new Interval(316, 44),   // North (wrap)
                    new Interval(45, 134),   // East
                    new Interval(135, 225),  // South
                    new Interval(226, 315)); // West
                result.Add(new GH_SAMParam(azimuthsParam, ParamVisibility.Binding));

                // Per-sector constructions (optional)
                var gooApertureConstructionParam = new GooApertureConstructionParam
                {
                    Name = "_apertureConstructions_",
                    NickName = "_apertureConstructions_",
                    Description = "SAM Analytical Aperture Constructions (optional).",
                    Access = GH_ParamAccess.list,
                    Optional = true
                };
                result.Add(new GH_SAMParam(gooApertureConstructionParam, ParamVisibility.Binding));

                // Subdivide toggle
                var param_Boolean = new global::Grasshopper.Kernel.Parameters.Param_Boolean
                {
                    Name = "_subdivide_",
                    NickName = "_subdivide_",
                    Description = "Subdivide \n  If True, split the aperture band into multiple openings; \n  (spacing is controlled by via _horizontalSeparation_).",
                    Access = GH_ParamAccess.item,
                    Optional = true
                };
                param_Boolean.SetPersistentData(true);
                result.Add(new GH_SAMParam(param_Boolean, ParamVisibility.Binding));

                // Geometry numbers
                var param_Number = new global::Grasshopper.Kernel.Parameters.Param_Number
                {
                    Name = "_apertureHeight_",
                    NickName = "_apertureHeight_",
                    Description = "Aperture height [m]. Default: 2.5",
                    Access = GH_ParamAccess.item,
                    Optional = true
                };
                param_Number.SetPersistentData(2.5);
                result.Add(new GH_SAMParam(param_Number, ParamVisibility.Binding));

                param_Number = new global::Grasshopper.Kernel.Parameters.Param_Number
                {
                    Name = "_sillHeight_",
                    NickName = "_sillHeight_",
                    Description = "Sill height [m]. Default: 0.85 \n Keep sill in range ~0.85–1.0 [m]",
                    Access = GH_ParamAccess.item,
                    Optional = true
                };
                param_Number.SetPersistentData(0.85);
                result.Add(new GH_SAMParam(param_Number, ParamVisibility.Binding));

                param_Number = new global::Grasshopper.Kernel.Parameters.Param_Number
                {
                    Name = "_horizontalSeparation_",
                    NickName = "_horizontalSeparation_",
                    Description = "Horizontal separation between subdivided apertures [m]. Default: 3",
                    Access = GH_ParamAccess.item,
                    Optional = true
                };
                param_Number.SetPersistentData(3);
                result.Add(new GH_SAMParam(param_Number, ParamVisibility.Binding));

                // Single construction (optional)
                var apertureConstructionParam = new GooApertureConstructionParam
                {
                    Name = "_apertureConstruction_",
                    NickName = "_apertureConstruction_",
                    Description = "Single aperture construction for all sectors. Per-sector list overrides this.",
                    Access = GH_ParamAccess.item,
                    Optional = true
                };
                result.Add(new GH_SAMParam(apertureConstructionParam, ParamVisibility.Binding));

                // Keep separation
                param_Boolean = new global::Grasshopper.Kernel.Parameters.Param_Boolean
                {
                    Name = "_keepSeparationDistance_",
                    NickName = "_keepSeparationDistance_",
                    Description = "Keep horizontal separation even in tight geometry.",
                    Access = GH_ParamAccess.item,
                    Optional = true
                };
                param_Boolean.SetPersistentData(false);
                result.Add(new GH_SAMParam(param_Boolean, ParamVisibility.Binding));

                // Offset
                param_Number = new global::Grasshopper.Kernel.Parameters.Param_Number
                {
                    Name = "_offset_",
                    NickName = "_offset_",
                    Description = "Minimum offset from panel boundary to apertures [m]. Default: 0.1",
                    Access = GH_ParamAccess.item,
                    Optional = true
                };
                param_Number.SetPersistentData(0.1);
                result.Add(new GH_SAMParam(param_Number, ParamVisibility.Binding));

                return [.. result];
            }
        }

        protected override GH_SAMParam[] Outputs
        {
            get
            {
                List<GH_SAMParam> result = [];

                var gooAnalyticalObjectParam = new GooAnalyticalObjectParam
                {
                    Name = "AnalyticalObject",
                    NickName = "AnalyticalObject",
                    Description = "Updated object: Panel if a single panel was input, otherwise the updated AdjacencyCluster/AnalyticalModel.",
                    Access = GH_ParamAccess.item
                };
                result.Add(new GH_SAMParam(gooAnalyticalObjectParam, ParamVisibility.Binding));

                var gooApertureParam = new GooApertureParam
                {
                    Name = "Apertures",
                    NickName = "Apertures",
                    Description = "Created apertures.",
                    Access = GH_ParamAccess.list
                };
                result.Add(new GH_SAMParam(gooApertureParam, ParamVisibility.Binding));

                var param_Boolean = new global::Grasshopper.Kernel.Parameters.Param_Boolean
                {
                    Name = "Successful",
                    NickName = "Successful",
                    Description = "True if at least one aperture was created.",
                    Access = GH_ParamAccess.item
                };
                result.Add(new GH_SAMParam(param_Boolean, ParamVisibility.Binding));

                return [.. result];
            }
        }

        // ────────────────────────────────────────────────────────────────────────────────
        // Execution
        // ────────────────────────────────────────────────────────────────────────────────

        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            int index_Successful = Params.IndexOfOutputParam("Successful");
            if (index_Successful != -1)
            {
                dataAccess.SetData(index_Successful, false);
            }

            int index = -1;

            // Inputs
            List<ApertureConstruction> apertureConstructions = [];
            index = Params.IndexOfInputParam("_apertureConstructions_");
            if (index != -1)
            {
                dataAccess.GetDataList(index, apertureConstructions);
            }

            bool subdivide = true;
            index = Params.IndexOfInputParam("_subdivide_");
            if (index == -1 || !dataAccess.GetData(index, ref subdivide))
            {
                subdivide = true;
            }

            double apertureHeight = 2.5;
            index = Params.IndexOfInputParam("_apertureHeight_");
            if (index == -1 || !dataAccess.GetData(index, ref apertureHeight))
            {
                apertureHeight = 2.5;
            }

            double sillHeight = 0.85;
            index = Params.IndexOfInputParam("_sillHeight_");
            if (index == -1 || !dataAccess.GetData(index, ref sillHeight))
            {
                sillHeight = 0.85;
            }

            double horizontalSeparation = 3;
            index = Params.IndexOfInputParam("_horizontalSeparation_");
            if (index == -1 || !dataAccess.GetData(index, ref horizontalSeparation))
            {
                horizontalSeparation = 3;
            }

            double offset = 0.1;
            index = Params.IndexOfInputParam("_offset_");
            if (index == -1 || !dataAccess.GetData(index, ref offset))
            {
                offset = 0.1;
            }

            bool keepSeparationDistance = false;
            index = Params.IndexOfInputParam("_keepSeparationDistance_");
            if (index == -1 || !dataAccess.GetData(index, ref keepSeparationDistance))
            {
                keepSeparationDistance = false;
            }

            List<double> ratios = [];
            index = Params.IndexOfInputParam("_ratios");
            if (index == -1 || !dataAccess.GetDataList(index, ratios) || ratios == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid _ratios input.");
                return;
            }

            List<Interval> azimuths = [];
            index = Params.IndexOfInputParam("_azimuths");
            if (index == -1 || !dataAccess.GetDataList(index, azimuths) || azimuths == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid _azimuths input.");
                return;
            }

            // Normalize ratios: allow percentages (e.g., 80) or fractions (0.8)
            ratios = NormalizeRatios(ratios);

            // Pairing rules for ratios vs intervals
            if (ratios.Count == azimuths.Count)
            {
                // 1:1 OK
            }
            else if (ratios.Count == 1)
            {
                var r = ratios[0];
                var filled = new List<double>(azimuths.Count);
                for (int i = 0; i < azimuths.Count; i++) filled.Add(r);
                ratios = filled;
                AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, $"Broadcasted single ratio {r:0.###} to {azimuths.Count} intervals.");
            }
            else
            {
                int pairCount = System.Math.Min(ratios.Count, azimuths.Count);
                if (azimuths.Count > pairCount)
                {
                    double last = ratios[System.Math.Max(0, pairCount - 1)];
                    for (int i = pairCount; i < azimuths.Count; i++) ratios.Add(last);
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Remark,
                        $"Ratios ({ratios.Count}) < intervals ({azimuths.Count}). Reused last ratio for remaining intervals.");
                }
                else if (ratios.Count > pairCount)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Remark,
                        $"Ratios ({ratios.Count}) > intervals ({azimuths.Count}). Extra ratios ignored.");
                    ratios.RemoveRange(pairCount, ratios.Count - pairCount);
                }
            }

            // Pairing rules for constructions vs intervals
            if (apertureConstructions.Count == azimuths.Count)
            {
                // 1:1 OK
            }
            else if (apertureConstructions.Count == 1)
            {
                ApertureConstruction apertureConstruction = apertureConstructions[0];

                List<ApertureConstruction> apertureConstructions_Temp = new(azimuths.Count);
                for (int i = 0; i < azimuths.Count; i++)
                {
                    apertureConstructions_Temp.Add(apertureConstruction);
                }

                apertureConstructions = apertureConstructions_Temp;
                AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, $"Broadcasted single ApertureConstruction {apertureConstruction.Name} to {azimuths.Count} intervals.");
            }
            else if (apertureConstructions.Count != azimuths.Count)
            {
                int pairCount = System.Math.Min(apertureConstructions.Count, azimuths.Count);
                if (apertureConstructions.Count < azimuths.Count && pairCount > 0)
                {
                    ApertureConstruction last = apertureConstructions[System.Math.Max(0, pairCount - 1)];
                    for (int i = pairCount; i < azimuths.Count; i++) apertureConstructions.Add(last);
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Remark,
                        $"ApertureConstructions ({apertureConstructions.Count}) < intervals ({azimuths.Count}). Reused last ApertureConstruction for remaining intervals.");
                }
                else if (apertureConstructions.Count > pairCount)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, $"ApertureConstructions ({apertureConstructions.Count}) > intervals ({azimuths.Count}). Extra ApertureConstructions ignored.");
                    apertureConstructions.RemoveRange(pairCount, apertureConstructions.Count - pairCount);
                }
            }

            // Map intervals to (ratio, construction) — wrap-aware
            Dictionary<Interval, Tuple<double, ApertureConstruction>> intervalRatioMap =
                BuildIntervalRatioMap(azimuths, ratios, apertureConstructions);

            // Analytical object
            SAMObject sAMObject = null;
            if (!dataAccess.GetData(0, ref sAMObject))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid _analyticalObject input.");
                return;
            }

            // Single panel path
            if (sAMObject is Panel)
            {
                Panel panel = Create.Panel((Panel)sAMObject);

                double azimuth = NormalizeAngleDegrees(panel.Azimuth());
                if (double.IsNaN(azimuth)) return;

                if (!TryGetRatio(intervalRatioMap, azimuth, out double ratio, out ApertureConstruction apertureConstruction_Temp))
                {
                    return;
                }

                if (apertureConstruction_Temp is null)
                {
                    apertureConstruction_Temp = Analytical.Query.DefaultApertureConstruction(panel, ApertureType.Window);
                }

                var apertures = panel.AddApertures(apertureConstruction_Temp, ratio, subdivide, apertureHeight, sillHeight, horizontalSeparation, offset, keepSeparationDistance);

                dataAccess.SetData(0, panel);
                dataAccess.SetDataList(1, apertures?.ConvertAll(x => new GooAperture(x)));
                dataAccess.SetData(2, apertures != null);
                return;
            }

            // Cluster / Model path
            AdjacencyCluster adjacencyCluster = null;
            AnalyticalModel analyticalModel = null;

            if (sAMObject is AdjacencyCluster ac)
            {
                adjacencyCluster = new AdjacencyCluster(ac);
            }
            else if (sAMObject is AnalyticalModel am)
            {
                analyticalModel = am;
                adjacencyCluster = analyticalModel.AdjacencyCluster;
            }

            if (adjacencyCluster == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Failed to resolve AdjacencyCluster.");
                return;
            }

            var panels = adjacencyCluster.GetPanels();
            if (panels == null) return;

            var tuples_Result = new List<Tuple<Panel, Aperture>>();

            foreach (var panel in panels)
            {
                if (panel.PanelType != PanelType.WallExternal)
                    continue;

                double az = NormalizeAngleDegrees(panel.Azimuth());
                if (double.IsNaN(az))
                    continue;

                if (!TryGetRatio(intervalRatioMap, az, out double ratio, out ApertureConstruction apertureConstruction_Temp))
                {
                    continue;
                }

                var panel_New = Create.Panel(panel);

                if (apertureConstruction_Temp is null)
                {
                    apertureConstruction_Temp = Analytical.Query.DefaultApertureConstruction(panel_New, ApertureType.Window);
                }

                var apertures = panel_New.AddApertures(apertureConstruction_Temp, ratio, subdivide, apertureHeight, sillHeight, horizontalSeparation, offset, keepSeparationDistance);
                if (apertures == null || apertures.Count == 0)
                    continue;

                apertures.ForEach(x => tuples_Result.Add(new Tuple<Panel, Aperture>(panel_New, x)));
                adjacencyCluster.AddObject(panel_New);
            }

            if (analyticalModel != null)
            {
                var analyticalModel_Result = new AnalyticalModel(analyticalModel, adjacencyCluster);
                dataAccess.SetData(0, analyticalModel_Result);
            }
            else
            {
                dataAccess.SetData(0, adjacencyCluster);
            }

            dataAccess.SetDataList(1, tuples_Result.ConvertAll(x => new GooAperture(x.Item2)));
            dataAccess.SetData(2, tuples_Result.Count > 0);
        }

        // ========================= Helpers =========================

        /// <summary>
        /// Build a dictionary from intervals to (ratio, construction), expanding wrap-around
        /// intervals (T0 > T1) into [T0→359] and [0→T1].
        /// </summary>
        private static Dictionary<Interval, Tuple<double, ApertureConstruction>> BuildIntervalRatioMap(
            IList<Interval> intervals,
            IList<double> ratios,
            IList<ApertureConstruction> apertureConstructions)
        {
            Dictionary<Interval, Tuple<double, ApertureConstruction>> dictionary = [];
            for (int i = 0; i < intervals.Count; i++)
            {
                Interval interval = intervals[i];
                double ratio = ratios[System.Math.Min(i, ratios.Count - 1)];
                ApertureConstruction apertureConstruction =
                    (apertureConstructions == null || apertureConstructions.Count == 0)
                        ? null
                        : apertureConstructions[System.Math.Min(i, ratios.Count - 1)];

                // Normalize inputs to [0, 359] where sensible
                double a = ClampTo360(interval.T0);
                double b = ClampTo360(interval.T1);

                // Non-wrapped: a <= b
                if (a <= b)
                {
                    var norm = new Interval(a, b);
                    dictionary[norm] = new Tuple<double, ApertureConstruction>(ratio, apertureConstruction);
                }
                else
                {
                    // Wrapped: split into two
                    var iv1 = new Interval(a, 359.0);
                    var iv2 = new Interval(0.0, b);
                    dictionary[iv1] = new Tuple<double, ApertureConstruction>(ratio, apertureConstruction);
                    dictionary[iv2] = new Tuple<double, ApertureConstruction>(ratio, apertureConstruction);
                }
            }
            return dictionary;
        }

        /// <summary>Try to find the ratio whose interval contains the given azimuth.</summary>
        private static bool TryGetRatio(
            Dictionary<Interval, Tuple<double, ApertureConstruction>> map,
            double azimuthDeg,
            out double ratio,
            out ApertureConstruction apertureConstruction)
        {
            double azimuthDeg_Round = System.Math.Round(azimuthDeg, MidpointRounding.ToEven);
            apertureConstruction = null;
            ratio = 0.0;

            foreach (var kvp in map)
            {
                if (kvp.Key.IncludesParameter(azimuthDeg_Round))
                {
                    ratio = kvp.Value.Item1;
                    apertureConstruction = kvp.Value.Item2;
                    return true;
                }
            }
            return false;
        }

        /// <summary>Normalise angle to [0, 359].</summary>
        private static double NormalizeAngleDegrees(double angleDeg)
        {
            if (double.IsNaN(angleDeg) || double.IsInfinity(angleDeg)) return double.NaN;
            double a = angleDeg % 360.0;
            if (a < 0) a += 360.0;
            return (a >= 360.0) ? 359.0 : a;
        }

        /// <summary>Clamp arbitrary double to [0, 359] while preserving values in that range.</summary>
        private static double ClampTo360(double v)
        {
            if (double.IsNaN(v)) return 0.0;
            double a = v % 360.0;
            if (a < 0) a += 360.0;
            return (a >= 360.0) ? 359.0 : a;
        }

        /// <summary>If any ratio &gt; 1, treat all as percentages and divide by 100.</summary>
        private static List<double> NormalizeRatios(List<double> ratios)
        {
            if (ratios == null || ratios.Count == 0) return ratios;
            bool looksLikePercent = false;
            foreach (var r in ratios) { if (r > 1.0) { looksLikePercent = true; break; } }
            if (!looksLikePercent) return ratios;

            var norm = new List<double>(ratios.Count);
            foreach (var r in ratios) norm.Add(r / 100.0);
            return norm;
        }
    }
}
