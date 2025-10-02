using Grasshopper.Kernel;
using Rhino.Geometry;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalAddAperturesByAzimuth : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("84d34834-8ce0-42cb-a3de-7366337bac4a");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.3";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        /// <summary>
        /// Initializes a new instance of the component.
        /// </summary>
        public SAMAnalyticalAddAperturesByAzimuth()
          : base("SAMAnalytical.AddAperturesByAzimuth",
                 "SAMAnalytical.AddAperturesByAzimuth",
                 "Add Apertures to a SAM Analytical Object (Panel, AdjacencyCluster, AnalyticalModel) based on azimuth sectors.\n" +
                 "Uses in defaults 4 directions (N, E, S, W). North wraps across 360° and is handled internally by splitting 316→44 into: 316→359 and 0→44.\n" +
                 "Ratios map to sectors in this order: [North, East, South, West].\n" +
                 "Defaults:\n" +
                 "  Ratios: [0.8, 0.7, 0.5, 0.6]\n" +
                 "  Sectors: North 316→44 (wrap), East 45→134, South 135→225, West 226→315.\n" +
                 "Notes:\n" +
                 "• If a sector interval has T0 > T1, it is treated as wrap-around and split internally.\n" +
                 "• For a fixed direction, use e.g. 90→90.\n" +
                 "• Ratios typically 0.0–1.0; they scale aperture addition per sector.",
                 "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            int idx;


            // Analytical object
            idx = inputParamManager.AddParameter(
                new GooAnalyticalObjectParam(),
                "_analyticalObject",
                "_analyticalObject",
                "SAM Analytical Object (AdjacencyCluster, Panel, or AnalyticalModel).",
                GH_ParamAccess.item);

            // Ratios (N, E, S, W)
            var ratiosParam = new global::Grasshopper.Kernel.Parameters.Param_Number()
            {
                Name = "_ratios",
                NickName = "_ratios",
                Description =
                    "Directional ratios applied to four azimuth sectors in order: [North, East, South, West].\n" +
                    "Typical range: 0.0–1.0.\n" +
                    "Defaults:\n" +
                    "  North = 0.8, East = 0.7, South = 0.5, West = 0.6\n" +
                    "These scale the aperture addition per sector.",
                Access = GH_ParamAccess.list,
            };
            ratiosParam.SetPersistentData(0.8, 0.7, 0.5, 0.6);
            idx = inputParamManager.AddParameter(ratiosParam);
            inputParamManager[idx].Optional = true;

            // Azimuth intervals (4 sectors; North wraps 316→44)
            var azimuthsParam = new global::Grasshopper.Kernel.Parameters.Param_Interval()
            {
                Name = "_azimuths",
                NickName = "_azimuths",
                Description =
                    "Azimuth sectors [°] for 8 or 4 directions (N, E, S, W) in the same order as _ratios.\n" +
                    "Defaults:\n" +
                    "  North : 316 → 44   (wrap; internally split to 316→359 and 0→44)\n" +
                    "  East  : 45  → 134\n" +
                    "  South : 135 → 225\n" +
                    "  West  : 226 → 315\n" +
                    "Notes:\n" +
                    "• If T0 > T1, the interval is treated as wrap-around and split internally.\n" +
                    "• For a fixed angle, use e.g. 90 → 90.",
                Access = GH_ParamAccess.list
            };
            azimuthsParam.SetPersistentData(
                new Interval(316,  44),  // North (wrap)
                new Interval(45,  134),  // East
                new Interval(135, 225),  // South
                new Interval(226, 315)); //West
            idx = inputParamManager.AddParameter(azimuthsParam);
            inputParamManager[idx].Optional = true;

            // Aperture construction (optional)
            idx = inputParamManager.AddParameter(
                new GooApertureConstructionParam(),
                "_apertureConstruction_",
                "_apertureConstruction_",
                "SAM Analytical Aperture Construction (optional).",
                GH_ParamAccess.item);
            inputParamManager[idx].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooAnalyticalObjectParam(), "AnalyticalObject", "AnalyticalObject", "SAM Analytical Object", GH_ParamAccess.list);
            outputParamManager.AddParameter(new GooApertureParam(), "Apertures", "Apertures", "SAM Analytical Apertures", GH_ParamAccess.list);
            outputParamManager.AddBooleanParameter("Successful", "Successful", "Successful", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            dataAccess.SetData(2, false);

            // Inputs
            ApertureConstruction apertureConstruction = null;
            dataAccess.GetData(3, ref apertureConstruction);

            var ratios = new List<double>();
            if (!dataAccess.GetDataList(1, ratios) || ratios == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid _ratios input.");
                return;
            }

            var azimuths = new List<Interval>();
            if (!dataAccess.GetDataList(2, azimuths) || azimuths == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid _azimuths input.");
                return;
            }

            //// Expect exactly 4 directions (N, E, S, W)
            //if (ratios.Count != 4 || azimuths.Count != 4)
            //{
            //    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Provide exactly 4 ratios and 4 azimuth intervals (N, E, S, W).");
            //    return;
            //}

            // Normalize ratios: allow percentages (e.g., 80) or fractions (0.8)
            ratios = NormalizeRatios(ratios);

            // Pairing rules:
            // 1) If ratios.Count == azimuths.Count -> 1:1 mapping.
            // 2) If ratios.Count == 1           -> broadcast that ratio to all intervals.
            // 3) Otherwise: map up to min(counts); extra intervals get the last ratio;
            //    extra ratios are ignored. Emit a remark so users know.
            if (ratios.Count == azimuths.Count)
            {
                // ok
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

            // Build interval→ratio map (wrap-aware: e.g., 338→22 is split into 338→359 & 0→22)
            var intervalRatioMap = BuildIntervalRatioMap(azimuths, ratios);

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

                if (!TryGetRatio(intervalRatioMap, azimuth, out double ratio)) return;

                var apertureConstruction_Temp = apertureConstruction ?? Analytical.Query.DefaultApertureConstruction(panel, ApertureType.Window);
                var apertures = panel.AddApertures(apertureConstruction_Temp, ratio);

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

                if (!TryGetRatio(intervalRatioMap, az, out double ratio))
                    continue;

                var panel_New = Create.Panel(panel);
                var apertureConstruction_Temp = apertureConstruction ?? Analytical.Query.DefaultApertureConstruction(panel_New, ApertureType.Window);

                var apertures = panel_New.AddApertures(apertureConstruction_Temp, ratio);
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
        /// Build a dictionary from intervals to ratios, expanding any wrap-around interval (T0 > T1)
        /// into two intervals: [T0→359] and [0→T1], both mapped to the same ratio.
        /// </summary>
        private static Dictionary<Interval, double> BuildIntervalRatioMap(IList<Interval> intervals, IList<double> ratios)
        {
            var map = new Dictionary<Interval, double>();
            for (int i = 0; i < intervals.Count; i++)
            {
                var iv = intervals[i];
                var r = ratios[System.Math.Min(i, ratios.Count - 1)];

                // Normalize inputs to [0, 359] where sensible
                var a = ClampTo360(iv.T0);
                var b = ClampTo360(iv.T1);

                // Non-wrapped: a <= b
                if (a <= b)
                {
                    var norm = new Interval(a, b);
                    map[norm] = r;
                }
                else
                {
                    // Wrapped: split into two
                    var iv1 = new Interval(a, 359.0);
                    var iv2 = new Interval(0.0, b);
                    map[iv1] = r;
                    map[iv2] = r;
                }
            }
            return map;
        }

        /// <summary>
        /// Try to find the ratio whose interval contains the given azimuth.
        /// </summary>
        private static bool TryGetRatio(Dictionary<Interval, double> map, double azimuthDeg, out double ratio)
        {
            double azimuthDeg_Round = System.Math.Round(azimuthDeg, MidpointRounding.ToEven);

            ratio = 0.0;
            foreach (var kvp in map)
            {
                // Interval.IncludesParameter uses numeric comparison on [T0, T1]
                if (kvp.Key.IncludesParameter(azimuthDeg_Round))
                {
                    ratio = kvp.Value;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Normalize angle to [0, 359].
        /// </summary>
        private static double NormalizeAngleDegrees(double angleDeg)
        {
            if (double.IsNaN(angleDeg) || double.IsInfinity(angleDeg)) return double.NaN;
            // Bring into [0, 360)
            double a = angleDeg % 360.0;
            if (a < 0) a += 360.0;
            // Cap 360 to 359 to keep a closed range convention
            return (a >= 360.0) ? 359.0 : a;
        }

        /// <summary>
        /// Clamp arbitrary double to [0, 359] while preserving given values in that range.
        /// </summary>
        private static double ClampTo360(double v)
        {
            if (double.IsNaN(v)) return 0.0;
            // Normalize then clamp endpoint 360 to 359
            double a = v % 360.0;
            if (a < 0) a += 360.0;
            return (a >= 360.0) ? 359.0 : a;
        }

        private static List<double> NormalizeRatios(List<double> ratios)
        {
            if (ratios == null || ratios.Count == 0) return ratios;
            // If ANY ratio > 1.0, we assume percentages and divide all by 100.
            bool looksLikePercent = false;
            foreach (var r in ratios) { if (r > 1.0) { looksLikePercent = true; break; } }
            if (!looksLikePercent) return ratios;

            var norm = new List<double>(ratios.Count);
            foreach (var r in ratios) norm.Add(r / 100.0);
            return norm;
        }

    }
}
