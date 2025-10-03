using GH_IO.Serialization;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Rhino.Geometry;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;
using System.IO;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalFilterByAzimuth : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new ("72b15a3c-6568-4a93-ac47-1e436c6a1535");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.1";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
                protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        public override GH_Exposure Exposure => GH_Exposure.primary;

        protected override GH_SAMParam[] Inputs
        {
            get
            {
                List<GH_SAMParam> result = [];
                result.Add(new GH_SAMParam(new GooAnalyticalObjectParam() { Name = "_analyticalObjects", NickName = "_analyticalObjects", Description = "SAM Analytical Objects such as Panels or Apertures", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                
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
                    Access = GH_ParamAccess.list,
                    Optional = true
                };
                azimuthsParam.SetPersistentData(
                    new Interval(316, 44),  // North (wrap)
                    new Interval(45, 134),  // East
                    new Interval(135, 225),  // South
                    new Interval(226, 315)); //West
                result.Add(new GH_SAMParam(azimuthsParam, ParamVisibility.Binding));
                
                global::Grasshopper.Kernel.Parameters.Param_Number number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_trueNorth_", NickName = "_trueNorth_", Description = "True north [0 - 359 deg] Clockwise", Access = GH_ParamAccess.item };
                number.SetPersistentData(0.0);
                result.Add(new GH_SAMParam(number, ParamVisibility.Binding));

                return [.. result];
            }
        }

        protected override GH_SAMParam[] Outputs
        {
            get
            {
                List<GH_SAMParam> result = [];
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Interval() { Name = "Azimuths", NickName = "Azimuths", Description = "Azimuths intervals", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                
                result.Add(new GH_SAMParam(new GooPanelParam() { Name = "Panels", NickName = "Panels", Description = "SAM Panels", Access = GH_ParamAccess.tree }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooApertureParam() { Name = "Apertures", NickName = "Apertures", Description = "SAM Apertures", Access = GH_ParamAccess.tree }, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new GooPanelParam() { Name = "Panels_Horizontal", NickName = "Panels_Horizontal", Description = "SAM Panels Horizontal", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooApertureParam() { Name = "Apertures_Horizontal", NickName = "Apertures_Horizontal", Description = "SAM Apertures Horizontal", Access = GH_ParamAccess.list }, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new GooPanelParam() { Name = "Panels_Tilted", NickName = "Panels_Tilted", Description = "SAM Panels Tilted", Access = GH_ParamAccess.tree }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooApertureParam() { Name = "Apertures_Tilted", NickName = "Apertures_Tilted", Description = "SAM Apertures Tilted", Access = GH_ParamAccess.tree }, ParamVisibility.Binding));

                return [.. result];
            }
        }

        /// <summary>
        /// Updates PanelTypes for AdjacencyCluster
        /// </summary>
        public SAMAnalyticalFilterByAzimuth()
          : base("SAMAnalytical.FilterByAzimuth", "SAMAnalytical.FilterByAzimuth",
              "Filter By Azimuth",
              "SAM", "Analytical01")
        {
        }

        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            int index;

            List<Interval> azimuths = [];
            index = Params.IndexOfInputParam("_azimuths");
            if (index == -1 || !dataAccess.GetDataList(index, azimuths) || azimuths == null || azimuths.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            double trueNorth = 0.0;
            index = Params.IndexOfInputParam("_trueNorth_");
            if (index != -1)
            {
                if(!dataAccess.GetData(index, ref trueNorth))
                {
                    trueNorth = 0.0;
                }
            }

            List<IAnalyticalObject> analyticalObjects = [];
            index = Params.IndexOfInputParam("_analyticalObjects");
            if(index == -1 || !dataAccess.GetDataList(index, analyticalObjects) || analyticalObjects == null || analyticalObjects.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Panel> panels = [];
            List<Aperture> apertures = []; 
            foreach(IAnalyticalObject analyticalObject in analyticalObjects)
            {
                if(analyticalObject is Panel panel)
                {
                    panels.Add(panel);
                }
                else if (analyticalObject is Aperture aperture)
                {
                    apertures.Add(aperture);
                }
                else if(analyticalObject is AnalyticalModel analyticalModel)
                {
                    panels = analyticalModel.GetPanels();
                }
                else if(analyticalObject is AdjacencyCluster adjacencyCluster)
                {
                    panels = adjacencyCluster.GetPanels();
                }
            }

            if(panels is not null && panels.Count != 0)
            {
                foreach(Panel panel in panels)
                {
                    if (!panel.HasApertures)
                    {
                        continue;
                    }

                    if(panel.Apertures is List<Aperture> apertures_Temp)
                    {
                        foreach(Aperture aperture in apertures_Temp)
                        {
                            apertures.Add(aperture);
                        }
                    }
                }
            }

            DataTree<Panel> dataTree_Panels = new ();
            DataTree<Aperture> dataTree_Apertures = new ();

            List<Panel> panels_Horizontal = new();
            List<Aperture> apertures_Horizontal = new();

            DataTree<Panel> dataTree_Panels_Tilted = new();
            DataTree<Aperture> dataTree_Apertures_Tilted = new();

            Vector3D referenceDirection = Vector3D.WorldY;
            if(trueNorth != 0.0)
            {
                referenceDirection = referenceDirection.Rotate(Geometry.Spatial.Plane.WorldXY, - trueNorth * System.Math.PI / 180.0);
            }

            Dictionary<Interval, int> dictionary = IntervalDictionary(azimuths);

            foreach (Panel panel in panels)
            {
                if (panel?.Face3D is not IClosedPlanar3D closedPlanar3D)
                {
                    continue;
                }

                double tilt = Geometry.Spatial.Query.Tilt(closedPlanar3D);
                if(Core.Query.AlmostEqual(tilt, 0, Core.Tolerance.MacroDistance) || Core.Query.AlmostEqual(tilt, 180, Core.Tolerance.MacroDistance))
                {
                    panels_Horizontal.Add(panel);
                    continue;
                }

                double azimuth = Geometry.Spatial.Query.Azimuth(closedPlanar3D, referenceDirection);
                if (double.IsNaN(azimuth))
                {
                    continue;
                }

                if(!TryGetRatio(dictionary, azimuth, out int index_Temp) || index_Temp == -1)
                {
                    continue;
                }

                DataTree<Panel> dataTree_Panels_Temp = Core.Query.AlmostEqual(tilt, 90, Core.Tolerance.MacroDistance) ? dataTree_Panels : dataTree_Panels_Tilted;

                dataTree_Panels_Temp.Add(panel, new GH_Path(index_Temp));
            }

            foreach (Aperture aperture in apertures)
            {
                if (aperture?.Face3D is not IClosedPlanar3D closedPlanar3D)
                {
                    continue;
                }

                double tilt = Geometry.Spatial.Query.Tilt(closedPlanar3D);
                if (Core.Query.AlmostEqual(tilt, 0, Core.Tolerance.MacroDistance) || Core.Query.AlmostEqual(tilt, 180, Core.Tolerance.MacroDistance))
                {
                    apertures_Horizontal.Add(aperture);
                    continue;
                }

                double azimuth = Geometry.Spatial.Query.Azimuth(closedPlanar3D, referenceDirection);
                if(double.IsNaN(azimuth))
                {
                    continue;
                }

                if (!TryGetRatio(dictionary, azimuth, out int index_Temp) || index_Temp == -1)
                {
                    continue;
                }

                DataTree<Aperture> dataTree_Apertures_Temp = Core.Query.AlmostEqual(tilt, 90, Core.Tolerance.MacroDistance) ? dataTree_Apertures : dataTree_Apertures_Tilted;

                dataTree_Apertures.Add(aperture, new GH_Path(index_Temp));
            }

            index = Params.IndexOfOutputParam("Azimuths");
            if(index != -1)
            {
                dataAccess.SetDataList(index, azimuths);
            }

            index = Params.IndexOfOutputParam("Panels");
            if (index != -1)
            {
                dataAccess.SetDataTree(index, dataTree_Panels);
            }

            index = Params.IndexOfOutputParam("Apertures");
            if (index != -1)
            {
                dataAccess.SetDataTree(index, dataTree_Apertures);
            }

            index = Params.IndexOfOutputParam("Panels_Horizontal");
            if (index != -1)
            {
                dataAccess.SetDataList(index, panels_Horizontal);
            }

            index = Params.IndexOfOutputParam("Apertures_Horizontal");
            if (index != -1)
            {
                dataAccess.SetDataList(index, apertures_Horizontal);
            }

        }

        private static Dictionary<Interval, int> IntervalDictionary(IList<Interval> azimuths)
        {
            Dictionary<Interval, int> dictionary = [];
            for (int i = 0; i < azimuths.Count; i++)
            {
                Interval interval = azimuths[i];

                // Normalize inputs to [0, 359] where sensible
                double a = ClampTo360(interval.T0);
                double b = ClampTo360(interval.T1);

                // Non-wrapped: a <= b
                if (a <= b)
                {
                    var norm = new Interval(a, b);
                    dictionary[norm] = i;
                }
                else
                {
                    // Wrapped: split into two
                    var iv1 = new Interval(a, 359.0);
                    var iv2 = new Interval(0.0, b);
                    dictionary[iv1] = i;
                    dictionary[iv2] = i;
                }
            }
            return dictionary;
        }

        /// <summary>
        /// Try to find the ratio whose interval contains the given azimuth.
        /// </summary>
        private static bool TryGetRatio(Dictionary<Interval, int> intervalDictionary, double azimuth, out int index)
        {
            double azimuth_Round = System.Math.Round(azimuth, MidpointRounding.ToEven);

            index = -1;
            foreach (KeyValuePair<Interval, int> keyValuePair in intervalDictionary)
            {
                // Interval.IncludesParameter uses numeric comparison on [T0, T1]
                if (keyValuePair.Key.IncludesParameter(azimuth_Round))
                {
                    index = keyValuePair.Value;
                    return true;
                }
            }

            return false;
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
    }
}