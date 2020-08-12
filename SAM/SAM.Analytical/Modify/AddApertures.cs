using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static List<Aperture> AddApertures(this AdjacencyCluster adjacencyCluster, ApertureConstruction apertureConstruction, IEnumerable<IClosedPlanar3D> closedPlanar3Ds, bool trimGeometry = true, double minArea = Core.Tolerance.MacroDistance, double maxDistance = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            if (adjacencyCluster == null || closedPlanar3Ds == null)
                return null;

            List<Panel> panels = adjacencyCluster.GetPanels();
            if (panels == null || panels.Count == 0)
                return null;

            List<Tuple<BoundingBox3D, IClosedPlanar3D>> tuples_ClosedPlanar3D = new List<Tuple<BoundingBox3D, IClosedPlanar3D>>();
            foreach (IClosedPlanar3D closedPlanar3D in closedPlanar3Ds)
            {
                BoundingBox3D boundingBox3D = closedPlanar3D?.GetBoundingBox(tolerance);
                if (boundingBox3D != null)
                    tuples_ClosedPlanar3D.Add(new Tuple<BoundingBox3D, IClosedPlanar3D>(boundingBox3D, closedPlanar3D));
            }

            List<Tuple<BoundingBox3D, Panel>> tuples_Panels = new List<Tuple<BoundingBox3D, Panel>>();
            foreach (Panel panel in panels)
            {
                BoundingBox3D boundingBox3D = panel?.GetBoundingBox(tolerance);
                if (boundingBox3D != null)
                    tuples_Panels.Add(new Tuple<BoundingBox3D, Panel>(boundingBox3D, panel));
            }

            List<Tuple<Panel, Aperture>> tuples_Result = new List<Tuple<Panel, Aperture>>();
            foreach (Tuple<BoundingBox3D, Panel> tuple_Panel in tuples_Panels)
            {
                BoundingBox3D boundingBox3D_Panel = tuple_Panel.Item1;

                Panel panel = tuple_Panel.Item2;

                foreach (Tuple<BoundingBox3D, IClosedPlanar3D> tuple_ClosedPlanar3D in tuples_ClosedPlanar3D)
                {
                    BoundingBox3D boundingBox3D_Aperture = tuple_ClosedPlanar3D.Item1;

                    if (!boundingBox3D_Aperture.InRange(boundingBox3D_Panel, tolerance))
                        continue;

                    Aperture aperture = panel.AddAperture(apertureConstruction, tuple_ClosedPlanar3D.Item2, trimGeometry, minArea, maxDistance, tolerance);
                    if (aperture == null)
                        continue;

                    tuples_Result.Add(new Tuple<Panel, Aperture>(panel, aperture));
                }
            }

            return tuples_Result.ConvertAll(x => x.Item2);
        }

        public static List<Aperture> AddApertures(this AdjacencyCluster adjacencyCluster, IEnumerable<Aperture> apertures, bool trimGeometry = true, double minArea = Core.Tolerance.MacroDistance, double maxDistance = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            if (adjacencyCluster == null || apertures == null)
                return null;

            List<Tuple<ApertureConstruction, List<IClosedPlanar3D>>> tuples = new List<Tuple<ApertureConstruction, List<IClosedPlanar3D>>>();
            foreach (Aperture aperture in apertures)
            {
                ApertureConstruction apertureConstruction = aperture?.ApertureConstruction;
                if (apertureConstruction == null)
                    continue;

                Tuple<ApertureConstruction, List<IClosedPlanar3D>> tuple = tuples.Find(x => x.Item1.Guid == apertureConstruction.Guid);
                if (tuple == null)
                {
                    tuples.Add(new Tuple<ApertureConstruction, List<IClosedPlanar3D>>(apertureConstruction, new List<IClosedPlanar3D>() { aperture.GetFace3D() }));
                    continue;
                }

                tuple.Item2.Add(aperture.GetFace3D());
            }

            List<Aperture> result = new List<Aperture>();
            foreach(Tuple<ApertureConstruction, List<IClosedPlanar3D>> tuple in tuples)
            {
                List<Aperture> apertures_ApetureConstruction = AddApertures(adjacencyCluster, tuple.Item1, tuple.Item2, trimGeometry, minArea, maxDistance, tolerance);
                if (apertures_ApetureConstruction != null && apertures_ApetureConstruction.Count != 0)
                    result.AddRange(apertures_ApetureConstruction);
            }

            return result;
        }
    
        public static List<Aperture> AddApertures(this IEnumerable<Panel> panels, ApertureConstruction apertureConstruction, double ratio, double azimuth_Start, double azimuth_End, double tolerance_Area = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            if (panels == null || apertureConstruction == null)
                return null;

            List<Aperture> result = new List<Aperture>();

            foreach (Panel panel in panels)
            {
                Aperture aperture = panel.AddAperture(apertureConstruction, ratio, azimuth_Start, azimuth_End, tolerance_Area, tolerance);
                if (aperture != null)
                    result.Add(aperture);
            }

            return result;
        }
    }
}