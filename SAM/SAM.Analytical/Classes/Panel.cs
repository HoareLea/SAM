using System;
using System.Collections.Generic;

using Newtonsoft.Json.Linq;

using SAM.Core;
using SAM.Geometry.Spatial;

namespace SAM.Analytical
{
    public class Panel : SAMInstance
    {
        private PanelType panelType;
        private PlanarBoundary3D planarBoundary3D;
        private List<Aperture> apertures;

        public Panel(Panel panel)
            : base(panel)
        {
            planarBoundary3D = new PlanarBoundary3D(panel.planarBoundary3D);
            panelType = panel.panelType;

            if (panel.apertures != null)
                apertures = new List<Aperture>(panel.apertures.ConvertAll(x => new Aperture(x)));
        }

        public Panel(Panel panel, Construction construction)
            : base(construction == null ? null : construction.Name, panel, construction)
        {
            planarBoundary3D = new PlanarBoundary3D(panel.planarBoundary3D);
            panelType = panel.panelType;

            if (panel.apertures != null)
                apertures = new List<Aperture>(panel.apertures.ConvertAll(x => new Aperture(x)));
        }

        public Panel(Panel panel, PanelType panelType)
            : base(panel)
        {
            planarBoundary3D = new PlanarBoundary3D(panel.planarBoundary3D);
            this.panelType = panelType;

            if (panel.apertures != null)
                apertures = new List<Aperture>(panel.apertures.ConvertAll(x => new Aperture(x)));
        }

        public Panel(Construction construction, PanelType panelType, Face3D face)
            : base(construction == null ? null : construction.Name, construction)
        {
            this.panelType = panelType;
            planarBoundary3D = new PlanarBoundary3D(face);
        }

        public Panel(Construction construction, PanelType panelType, PlanarBoundary3D planarBoundary3D)
            : base(construction == null ? null : construction.Name, construction)
        {
            this.panelType = panelType;
            this.planarBoundary3D = planarBoundary3D;
        }

        public Panel(Guid guid, Panel panel, Face3D face, bool trimGeometry = true, double minArea = Tolerance.MacroDistance, double maxDistance = Tolerance.MacroDistance)
            : base(guid, panel)
        {
            panelType = panel.panelType;
            planarBoundary3D = new PlanarBoundary3D(face);

            if (panel.apertures != null)
            {
                foreach(Aperture aperture in panel.apertures)
                {
                    if (aperture == null)
                        continue;

                    //TODO: Update ExternalEdge with PlanarBoundary3D
                    AddAperture(aperture.ApertureConstruction, aperture.GetFace3D()?.GetExternalEdge(), trimGeometry, minArea, maxDistance);
                }
            }
            //apertures = panel.apertures.FindAll(x => Query.IsValid(this, x, Core.Tolerance.MacroDistance)).ConvertAll(x => new Aperture(x));
        }

        public Panel(Guid guid, string name, IEnumerable<ParameterSet> parameterSets, Construction construction, PanelType panelType, PlanarBoundary3D planarBoundary3D)
            : base(guid, name, parameterSets, construction)
        {
            this.panelType = panelType;
            this.planarBoundary3D = new PlanarBoundary3D(planarBoundary3D);
        }

        public Panel(JObject jObject)
             : base(jObject)
        {

        }

        public Face3D GetFace3D()
        {
            return planarBoundary3D.GetFace3D();
        }

        public double GetArea()
        {
            Face3D face3D = GetFace3D();
            if (face3D == null)
                return double.NaN;
            
            return face3D.GetArea();
        }

        public Vector3D Normal
        {
            get
            {
                return planarBoundary3D.Plane.Normal;
            }
        }

        public void Snap(IEnumerable<Point3D> point3Ds, double maxDistance = double.NaN)
        {
            planarBoundary3D.Snap(point3Ds, maxDistance);
        }

        public BoundingBox3D GetBoundingBox(double offset = 0)
        {
            return GetFace3D().GetBoundingBox(offset);
        }

        public Construction Construction
        {
            get
            {
                return SAMType as Construction;
            }
        }

        public PanelType PanelType
        {
            get
            {
                return panelType;
            }
        }

        public PlanarBoundary3D PlanarBoundary3D
        {
            get
            {
                return new PlanarBoundary3D(planarBoundary3D);
            }
        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
                return false;

            Enum.TryParse(jObject.Value<string>("PanelType"), out this.panelType);

            planarBoundary3D = new PlanarBoundary3D(jObject.Value<JObject>("PlanarBoundary3D"));

            if (jObject.ContainsKey("Apertures"))
                apertures = Core.Create.IJSAMObjects<Aperture>(jObject.Value<JArray>("Apertures"));

            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
                return jObject;

            jObject.Add("PanelType", panelType.ToString());
            jObject.Add("PlanarBoundary3D", planarBoundary3D.ToJObject());

            if (apertures != null)
                jObject.Add("Apertures", Core.Create.JArray(apertures));
            
            return jObject;
        }

        public Aperture AddAperture(ApertureConstruction apertureConstruction, IClosedPlanar3D closedPlanar3D, bool trimGeometry = true, double minArea = Tolerance.MacroDistance, double maxDistance = Tolerance.MacroDistance, double tolerance = Tolerance.Distance)
        {
            if (apertureConstruction == null || closedPlanar3D == null)
                return null;

            Plane plane = planarBoundary3D?.Plane;
            if (plane == null)
                return null;

            Plane plane_closedPlanar3D = closedPlanar3D.GetPlane();
            if (plane_closedPlanar3D == null)
                return null;

            Vector3D normal = plane.Normal;
            Vector3D normal_closedPlanar3D = plane_closedPlanar3D.Normal;

            if (!normal.AlmostSimilar(normal_closedPlanar3D, tolerance))
                return null;

            IClosedPlanar3D closedPlanar3D_Projected = plane.Project(closedPlanar3D);
            if (closedPlanar3D_Projected == null)
                return null;

            Point3D point3D_ClosedPlanar3D = plane_closedPlanar3D.Origin;
            Point3D point3D_ClosedPlanar3D_Projected = plane.Project(plane_closedPlanar3D.Origin);

            if (point3D_ClosedPlanar3D.Distance(point3D_ClosedPlanar3D_Projected) >= maxDistance)
                return null;

            Geometry.Planar.IClosed2D closed2D_Aperture = plane.Convert(closedPlanar3D_Projected);

            if (minArea != 0 && closed2D_Aperture.GetArea() <= minArea)
                return null;
            
            Point3D point3D_Location;
            Geometry.Planar.Point2D point2D_Centroid = closed2D_Aperture.GetCentroid();
            point3D_Location = plane.Convert(point2D_Centroid);
            if (Geometry.Spatial.Query.Vertical(plane, Tolerance.Distance))
                point3D_Location = new Point3D(point3D_Location.X, point3D_Location.Y, closedPlanar3D_Projected.GetBoundingBox().Min.Z);
                
            if (trimGeometry)
            {
                if (closed2D_Aperture is Geometry.Planar.ISegmentable2D)
                {
                    Geometry.Planar.IClosed2D closed2D = plane.Convert(planarBoundary3D.GetFace3D().GetExternalEdge());
                    if (closed2D is Geometry.Planar.ISegmentable2D)
                    {
                        if (!closed2D.Inside(closed2D_Aperture))
                        {
                            double area_Aperture = closed2D_Aperture.GetArea();

                            List<Geometry.Planar.Segment2D> segment2Ds = Geometry.Planar.Modify.Split(new List<Geometry.Planar.ISegmentable2D>() { (Geometry.Planar.ISegmentable2D)closed2D, (Geometry.Planar.ISegmentable2D)closed2D_Aperture });

                            List<Geometry.Planar.Polygon2D> polygon2Ds = new Geometry.Planar.PointGraph2D(segment2Ds).GetPolygon2Ds();
                            
                            if (polygon2Ds != null && polygon2Ds.Count > 0)
                            {
                                double area_Difference_Min = double.MaxValue;
                                Geometry.Planar.Polygon2D polygon2D_Min = null;
                                foreach (Geometry.Planar.Polygon2D polygon2D_Temp in polygon2Ds)
                                {
                                    double area_Temp = polygon2D_Temp.GetArea();
                                    if (minArea != 0 && area_Temp <= minArea)
                                        continue;

                                    double area_Difference = Math.Abs(area_Aperture - area_Temp);

                                    if (area_Difference > area_Difference_Min)
                                        continue;

                                    Geometry.Planar.Point2D point2D = polygon2D_Temp.GetInternalPoint2D();
                                    if (closed2D_Aperture.Inside(point2D) && closed2D.Inside(point2D))
                                    {
                                        point2D_Centroid = Geometry.Planar.Query.Centroid(polygon2D_Temp.Points);
                                        polygon2D_Min = polygon2D_Temp;
                                    }
                                }

                                if (polygon2D_Min != null && point2D_Centroid != null)
                                {
                                    //point2D_Centroid = new Geometry.Planar.Point2D(point2D_Centroid.X, polygon2D_Min.GetBoundingBox().Min.Y);

                                    closedPlanar3D_Projected = plane.Convert(polygon2D_Min);
                                    point3D_Location = plane.Convert(point2D_Centroid);
                                    if (Geometry.Spatial.Query.Vertical(plane, Tolerance.Distance))
                                        point3D_Location = new Point3D(point3D_Location.X, point3D_Location.Y, closedPlanar3D_Projected.GetBoundingBox().Min.Z);
                                }
                            }
                        }
                    }
                }
            }

            Aperture aperture = new Aperture(apertureConstruction, closedPlanar3D_Projected, point3D_Location);
            if (!Query.IsValid(this, aperture))
                return null;

            if (apertures == null)
                apertures = new List<Aperture>();

            apertures.Add(aperture);
            return aperture;
        }

        public bool AddAperture(Aperture aperture)
        {
            if (!Query.IsValid(this, aperture))
                return false;

            if (apertures == null)
                apertures = new List<Aperture>();

            apertures.Add(aperture);
            return true;
        }

        public List<Aperture> Apertures
        {
            get
            {
                if (apertures == null)
                    return null;
                return apertures.ConvertAll(x => x.Clone());
            }
        }
    }
}
