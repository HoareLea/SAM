using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static double Area(this IEnumerable<Panel> panels, params PanelGroup[] panelGroups)
        {
            if (panels == null)
                return double.NaN;

            bool filter = panelGroups != null && panelGroups.Length > 0;

            double result = 0;
            foreach(Panel panel in panels)
            {
                if (filter && !panelGroups.Contains(panel.PanelType.PanelGroup()))
                    continue;

                result += panel.GetArea();
            }

            return result;
        }

        public static double Area(this Aperture aperture, out double paneArea, out double frameArea, double tolerance = Core.Tolerance.Distance)
        {
            paneArea = double.NaN;
            frameArea = double.NaN;

            if(aperture == null)
            {
                return double.NaN;
            }

            Geometry.Spatial.Face3D face3D = aperture.Face3D;
            if(face3D == null)
            {
                return double.NaN;
            }

            Geometry.Planar.IClosed2D externalEdge2D = face3D.ExternalEdge2D;
            if(externalEdge2D == null)
            {
                return double.NaN;
            }

            double result = externalEdge2D.GetArea();
            if(double.IsNaN(result))
            {
                return double.NaN;
            }

            List<Geometry.Planar.IClosed2D> internalEdge2Ds = face3D.InternalEdge2Ds;
            if(internalEdge2Ds == null || internalEdge2Ds.Count == 0)
            {
                ApertureConstruction apertureConstruction = aperture.ApertureConstruction;
                if (apertureConstruction != null)
                {
                    if (apertureConstruction.TryGetValue(ApertureConstructionParameter.DefaultFrameWidth, out double defaultFrameWidth) && !double.IsNaN(defaultFrameWidth))
                    {
                        List<Geometry.Planar.Polygon2D> polygon2Ds = Geometry.Planar.Query.Offset(Geometry.Planar.Create.Polygon2D(externalEdge2D), -defaultFrameWidth, tolerance);
                        if (polygon2Ds != null)
                        {
                            internalEdge2Ds = new List<Geometry.Planar.IClosed2D>();
                            polygon2Ds.ForEach(x => internalEdge2Ds.Add(x));
                        }
                    }
                }
            }

            if(internalEdge2Ds == null || internalEdge2Ds.Count == 0)
            {
                frameArea = 0;
                paneArea = result;

                return result;
            }

            paneArea = internalEdge2Ds.ConvertAll(x => x.GetArea()).FindAll(x => !double.IsNaN(x)).Sum();
            frameArea = result - paneArea;

            return result;
        }
    }
}