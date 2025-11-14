using Grasshopper.Kernel;
using Rhino.Geometry;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalAddAperturesByAzimuth : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new ("84d34834-8ce0-42cb-a3de-7366337bac4a");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.6";

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
                 "\n" +
                 "Azimuth convention: 0° = North, 90° = East, increasing clockwise in the XY plane. 0° ≡ 360°.\n" +
                 "Sectors (azimuth) are specified as Grasshopper Domains, written as \"a to b\" (inclusive), e.g. \"0 to 90\".\n" +
                 "\n" +
                 "Defaults (4 directions):\n" +
                 "• Uses 4 directions (N, E, S, W). North wraps across 360° and is handled internally by splitting the wrap domain.\n" +
                 "• Ratios map to sectors in this order: [North, East, South, West].\n" +
                 "• Defaults:\n" +
                 "    Ratios : [0.8, 0.7, 0.5, 0.6]\n" +
                 "    Sectors: North 316 to 44 (wrap → internally 316 to 359 and 0 to 44),\n" +
                 "             East  45 to 134,\n" +
                 "             South 135 to 225,\n" +
                 "             West  226 to 315.\n" +
                 "\n" +
                 "Optional (8 directions):\n" +
                 "• To work with 8 compass directions, provide 8 sector Domains and 8 ratios in this order:\n" +
                 "  [N, NE, E, SE, S, SW, W, NW]. A typical 8-way partition using GH Domains is:\n" +
                 "    N : 338 to 22  (wrap → internally 338 to 359 and 0 to 22)\n" +
                 "    NE: 23  to 67\n" +
                 "    E : 68  to 112\n" +
                 "    SE: 113 to 157\n" +
                 "    S : 158 to 202\n" +
                 "    SW: 203 to 247\n" +
                 "    W : 248 to 292\n" +
                 "    NW: 293 to 337\n" +
                 "  Example ratios (WWR 0–1): [0.15, 0.10, 0.25, 0.20, 0.30, 0.20, 0.25, 0.10] → [N, NE, E, SE, S, SW, W, NW].\n" +
                 "\n" +
                 "Notes:\n" +
                 "• If a sector Domain has T0 > T1 (e.g. 338 to 22), it is treated as wrap-around and split internally.\n" +
                 "• For a fixed direction, use a degenerate Domain, East e.g. 90 to 90.\n" +
                 "• Ratios typically 0.0–1.0; they scale Panel to create aperture per azimuth sector.\n" +
                 "• Domains are inclusive; 0° is treated as North (and equivalent to 360°).",
                 "SAM", "Analytical")
        {
        }

        protected override GH_SAMParam[] Inputs
        {
            get
            {
                List<GH_SAMParam> result = [];

                GooAnalyticalObjectParam gooAnalyticalObjectParam = new() { 
                    Name = "_analyticalObject", 
                    NickName = "_analyticalObject", 
                    Description = "SAM Analytical Object (Panel, AdjacencyCluster, or AnalyticalModel).", 
                    Access = GH_ParamAccess.item 
                };
                result.Add(new GH_SAMParam(gooAnalyticalObjectParam, ParamVisibility.Binding));

                // Ratios (N, E, S, W)
                global::Grasshopper.Kernel.Parameters.Param_Number ratiosParam = new() { 
                    Name = "_ratios", 
                    NickName = "_ratios", 
                    Description = "Directional ratios applied to azimuth sectors in list ordern: [North, East, South, West].\n" +
                        "Typical range: 0.0–1.0.\n" +
                        "Defaults:\n" +
                        "  North = 0.15, East = 0.2, South = 0.25, West = 0.2\n" +
                        "  Ratios (0.0–1.0) are target WWRs; for each matched panel an aperture is created by scaling the panel polygon in its local plane so that the aperture area ≈ (ratio × panel area).", 
                    Access = GH_ParamAccess.list,
                    Optional = true
                };
                ratiosParam.SetPersistentData(0.15, 0.2, 0.25, 0.2);
                result.Add(new GH_SAMParam(ratiosParam, ParamVisibility.Binding));

                // Azimuth intervals (4 sectors; North wraps 316→44)
                global::Grasshopper.Kernel.Parameters.Param_Interval azimuthsParam = new global::Grasshopper.Kernel.Parameters.Param_Interval()
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
                        "• For a fixed direction, use a degenerate Domain, East e.g. 90 to 90." +
                        "• Domains are inclusive; 0° is treated as North (and equivalent to 360°).",
                    Access = GH_ParamAccess.list,
                    Optional = true
                };

                azimuthsParam.SetPersistentData(
                    new Interval(316, 44),  // North (wrap)
                    new Interval(45, 134),  // East
                    new Interval(135, 225),  // South
                    new Interval(226, 315)); //West
                result.Add(new GH_SAMParam(azimuthsParam, ParamVisibility.Binding));

                GooApertureConstructionParam gooApertureConstructionParam = new()
                {
                    Name = "_apertureConstructions_",
                    NickName = "_apertureConstructions_",
                    Description = "SAM Analytical Aperture Constructions (optional).",
                    Access = GH_ParamAccess.list,
                    Optional = true
                };
                result.Add(new GH_SAMParam(gooApertureConstructionParam, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Boolean param_Boolean = null;
                global::Grasshopper.Kernel.Parameters.Param_Number param_Number = null;

                param_Boolean = new global::Grasshopper.Kernel.Parameters.Param_Boolean() 
                { 
                    Name = "_subdivide_", 
                    NickName = "_subdivide_", 
                    Description = "Subdivide \n  If True, split the aperture band into multiple openings; \n  spacing is controlled by _horizontalSeparation", 
                    Access = GH_ParamAccess.item, 
                    Optional = true 
                };
                param_Boolean.SetPersistentData(true);
                result.Add(new GH_SAMParam(param_Boolean, ParamVisibility.Binding));

                param_Number = new global::Grasshopper.Kernel.Parameters.Param_Number() 
                { 
                    Name = "_apertureHeight_", 
                    NickName = "_apertureHeight_", 
                    Description = "Default aperture Height", 
                    Access = GH_ParamAccess.item, 
                    Optional = true 
                };
                param_Number.SetPersistentData(2.5);
                result.Add(new GH_SAMParam(param_Number, ParamVisibility.Binding));

                param_Number = new global::Grasshopper.Kernel.Parameters.Param_Number() 
                { 
                    Name = "_sillHeight_", 
                    NickName = "_sillHeight_", 
                    Description = "Default sill Height \n Keep sill ~0.85–1.0 [m]", 
                    Access = GH_ParamAccess.item, 
                    Optional = true 
                };
                param_Number.SetPersistentData(0.85);
                result.Add(new GH_SAMParam(param_Number, ParamVisibility.Binding));

                param_Number = new global::Grasshopper.Kernel.Parameters.Param_Number() 
                { 
                    Name = "_horizontalSeparation_", 
                    NickName = "_horizontalSeparation_", 
                    Description = "Horizontal Separation distance [m]", 
                    Access = GH_ParamAccess.item, 
                    Optional = true 
                };
                param_Number.SetPersistentData(3);
                result.Add(new GH_SAMParam(param_Number, ParamVisibility.Binding));

                GooApertureConstructionParam apertureConstructionParam = new GooApertureConstructionParam() 
                { 
                    Name = "_apertureConstruction_", 
                    NickName = "_apertureConstruction_", 
                    Description = "SAM Analytical Aperture Construction", 
                    Access = GH_ParamAccess.item, 
                    Optional = true 
                };
                result.Add(new GH_SAMParam(apertureConstructionParam, ParamVisibility.Binding));

                param_Boolean = new global::Grasshopper.Kernel.Parameters.Param_Boolean() 
                { 
                    Name = "_keepSeparationDistance_", 
                    NickName = "_keepSeparationDistance_", 
                    Description = "Keep horizontal separation distance between apertures", 
                    Access = GH_ParamAccess.item, 
                    Optional = true 
                };
                param_Boolean.SetPersistentData(false);
                result.Add(new GH_SAMParam(param_Boolean, ParamVisibility.Binding));

                param_Number = new global::Grasshopper.Kernel.Parameters.Param_Number() 
                { 
                    Name = "_offset_", 
                    NickName = "_offset_", 
                    Description = "Minimal Ofsset between wall and apertures", 
                    Access = GH_ParamAccess.item, 
                    Optional = true 
                };
                param_Number.SetPersistentData(0.1);
                result.Add(new GH_SAMParam(param_Number, ParamVisibility.Binding));

                // Aperture construction (optional)
                //idx = inputParamManager.AddParameter(
                //    new GooApertureConstructionParam(),
                //    "_apertureConstructions_",
                //    "_apertureConstructions_",
                //    "SAM Analytical Aperture Constructions (optional).",
                //    GH_ParamAccess.list);
                //inputParamManager[idx].Optional = true;

                //var ratiosParam = new global::Grasshopper.Kernel.Parameters.Param_Number()
                //{
                //    Name = "_ratios",
                //    NickName = "_ratios",
                //    Description =
                //        "Directional ratios applied to azimuth sectors in list ordern: [North, East, South, West].\n" +
                //        "Typical range: 0.0–1.0.\n" +
                //        "Defaults:\n" +
                //        "  North = 0.15, East = 0.2, South = 0.25, West = 0.2\n" +
                //        "  Ratios (0.0–1.0) are target WWRs; for each matched panel an aperture is created by scaling the panel polygon in its local plane so that the aperture area ≈ (ratio × panel area).",
                //    Access = GH_ParamAccess.list,
                //};

                //idx = inputParamManager.AddParameter(ratiosParam);
                //inputParamManager[idx].Optional = true;

                //azimuthsParam.SetPersistentData(
                //    new Interval(316, 44),  // North (wrap)
                //    new Interval(45, 134),  // East
                //    new Interval(135, 225),  // South
                //    new Interval(226, 315)); //West
                //idx = inputParamManager.AddParameter(azimuthsParam);
                //inputParamManager[idx].Optional = true;

                return [.. result];
            }
        }

        protected override GH_SAMParam[] Outputs
        {
            get
            {
                List<GH_SAMParam> result = [];

                GooAnalyticalObjectParam gooAnalyticalObjectParam = new () { Name = "AnalyticalObject", NickName = "AnalyticalObject", Description = "SAM Analytical Object", Access = GH_ParamAccess.list };
                result.Add(new GH_SAMParam(gooAnalyticalObjectParam, ParamVisibility.Binding));

                GooApertureParam gooApertureParam = new() { Name = "Apertures", NickName = "Apertures", Description = "SAM Analytical Apertures", Access = GH_ParamAccess.list };
                result.Add(new GH_SAMParam(gooAnalyticalObjectParam, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Boolean param_Boolean = new() { Name = "Successful", NickName = "Successful", Description = "Successful", Access = GH_ParamAccess.item };
                result.Add(new GH_SAMParam(param_Boolean, ParamVisibility.Binding));

                return [.. result];
            }
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
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
            if(index != -1)
            {
                dataAccess.GetDataList(index, apertureConstructions);
            }

            bool subdivide = true;
            index = Params.IndexOfInputParam("_subdivide_");
            if (index == -1 || !dataAccess.GetData(index, ref subdivide))
            {
                subdivide = true;
            }

            double apertureHeight = 2;
            index = Params.IndexOfInputParam("_apertureHeight_");
            if (index == -1 || !dataAccess.GetData(index, ref apertureHeight))
            {
                apertureHeight = 2.5;
            }

            double sillHeight = 0.8;
            index = Params.IndexOfInputParam("_sillHeight_");
            if (index == -1 || !dataAccess.GetData(index, ref sillHeight))
            {
                sillHeight = 0.8;
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
            if (!dataAccess.GetDataList(index, ratios) || ratios == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid _ratios input.");
                return;
            }

            List<Interval> azimuths = [];
            index = Params.IndexOfInputParam("_azimuths");
            if (!dataAccess.GetDataList(index, azimuths) || azimuths == null)
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

            // Pairing rules:
            // 1) If apertureConstructions.Count == azimuths.Count -> 1:1 mapping.
            // 2) If apertureConstructions.Count == 1           -> broadcast that ApertureConstruction to all intervals.
            // 3) Otherwise: map up to min(counts); extra intervals get the last apertureConstruction;
            //    extra apertureConstructions are ignored. Emit a remark so users know.
            if (apertureConstructions.Count == azimuths.Count)
            {
                // ok
            }
            else if (apertureConstructions.Count == 1)
            {
                ApertureConstruction apertureConstruction = apertureConstructions[0];
                
                List<ApertureConstruction> apertureConstructions_Temp = new (azimuths.Count);
                for (int i = 0; i < azimuths.Count; i++)
                {
                    apertureConstructions_Temp.Add(apertureConstruction);
                }

                apertureConstructions = apertureConstructions_Temp;
                AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, $"Broadcasted single ApertureConstruction {apertureConstruction.Name} to {azimuths.Count} intervals.");
            }
            else
            {
                int pairCount = System.Math.Min(apertureConstructions.Count, azimuths.Count);
                if (apertureConstructions.Count > pairCount)
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

            // Build interval→ratio map (wrap-aware: e.g., 338→22 is split into 338→359 & 0→22)
            Dictionary<Interval, Tuple<double, ApertureConstruction>> intervalRatioMap = BuildIntervalRatioMap(azimuths, ratios, apertureConstructions);

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
        /// Build a dictionary from intervals to ratios, expanding any wrap-around interval (T0 > T1)
        /// into two intervals: [T0→359] and [0→T1], both mapped to the same ratio.
        /// </summary>
        private static Dictionary<Interval, Tuple<double, ApertureConstruction>> BuildIntervalRatioMap(IList<Interval> intervals, IList<double> ratios, IList<ApertureConstruction> apertureConstructions)
        {
            Dictionary<Interval, Tuple<double, ApertureConstruction>> dictionary = [];
            for (int i = 0; i < intervals.Count; i++)
            {
                Interval interval = intervals[i];
                double ratio = ratios[System.Math.Min(i, ratios.Count - 1)];
                ApertureConstruction apertureConstruction = apertureConstructions == null || apertureConstructions.Count == 0 ? null : apertureConstructions[System.Math.Min(i, ratios.Count - 1)];

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

        /// <summary>
        /// Try to find the ratio whose interval contains the given azimuth.
        /// </summary>
        private static bool TryGetRatio(Dictionary<Interval, Tuple<double, ApertureConstruction>> map, double azimuthDeg, out double ratio, out ApertureConstruction apertureConstruction)
        {
            double azimuthDeg_Round = System.Math.Round(azimuthDeg, MidpointRounding.ToEven);
            apertureConstruction = null;

            ratio = 0.0;
            foreach (var kvp in map)
            {
                // Interval.IncludesParameter uses numeric comparison on [T0, T1]
                if (kvp.Key.IncludesParameter(azimuthDeg_Round))
                {
                    ratio = kvp.Value.Item1;
                    apertureConstruction = kvp.Value.Item2;
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
