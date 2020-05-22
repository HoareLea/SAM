using Newtonsoft.Json.Linq;
using SAM.Core;
using SAM.Geometry.Planar;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    /// <summary>
    /// SAM Analytical Panel stores information about shape and properties of building boundary such as Wall, Floor, Roof, Slab etc.
    /// </summary>
    public class Panel : SAMInstance
    {
        /// <summary>
        /// Type of the Panel such as Wall, Ceiling etc.
        /// </summary>
        private PanelType panelType;
        
        /// <summary>
        /// Planar Boundary 3D of Panel
        /// </summary>
        private PlanarBoundary3D planarBoundary3D;

        /// <summary>
        /// Apertures being hosted on Panel (Doors, Winodows, Skylight etc.)
        /// </summary>
        private List<Aperture> apertures;

        /// <summary>
        /// Creates new instance of panel based on another panel
        /// </summary>
        /// <param name="panel">SAM Analytical Panel</param>
        public Panel(Panel panel)
            : base(panel)
        {
            planarBoundary3D = new PlanarBoundary3D(panel.planarBoundary3D);
            panelType = panel.panelType;

            if (panel.apertures != null)
                apertures = new List<Aperture>(panel.apertures.ConvertAll(x => new Aperture(x)));
        }

        /// <summary>
        /// Creates new Panles by given Panel and New Construction
        /// </summary>
        /// <param name="panel">SAM Analytical Panel</param>
        /// <param name="construction">SAM Analytical Construction</param>
        public Panel(Panel panel, Construction construction)
            : base(construction == null ? null : construction.Name, panel, construction)
        {
            planarBoundary3D = new PlanarBoundary3D(panel.planarBoundary3D);
            panelType = panel.panelType;

            if (panel.apertures != null)
                apertures = new List<Aperture>(panel.apertures.ConvertAll(x => new Aperture(x)));
        }

        /// <summary>
        /// Creates new Panles by given Panel and new PanelType 
        /// </summary>
        /// <param name="panel">SAM Analytical Panel</param>
        /// <param name="panelType">SAM Analytical PanelType</param>
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
                foreach (Aperture aperture in panel.apertures)
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

        /// <summary>
        /// Gets Geometrical Representation of Panel (None Analytical Data)
        /// </summary>
        /// <returns name="face3D">SAM Geometry Face3D</returns>
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

        public double GetPerimeter()
        {
            if (planarBoundary3D == null)
                return double.NaN;
            
            return planarBoundary3D.GetPerimeter();
        }

        public Vector3D Normal
        {
            get
            {
                return planarBoundary3D?.Plane?.Normal;
            }
        }

        public Point3D Origin
        {
            get
            {
                return planarBoundary3D?.Plane?.Origin;
            }
        }

        public void Snap(IEnumerable<Point3D> point3Ds, double maxDistance = double.NaN)
        {
            planarBoundary3D.Snap(point3Ds, maxDistance);
        }

        public void Snap(IEnumerable<Plane> planes, double maxDistance)
        {
            planarBoundary3D.Snap(planes, maxDistance);
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

        public IEnumerable<ICurve2D> GetEdge2Ds(double elevation)
        {
            BoundingBox3D boundingBox3D = PlanarBoundary3D.GetBoundingBox();

            if (elevation < boundingBox3D.Min.Z || elevation > boundingBox3D.Max.Z)
                return null;

            return GetEdge2Ds(new Plane(new Point3D(0, 0, elevation), Vector3D.WorldZ));
        }

        public IEnumerable<ICurve2D> GetEdge2Ds(Plane plane)
        {
            if (plane == null)
                return null;

            Face3D face3D = GetFace3D();
            if (face3D == null)
                return null;

            PlanarIntersectionResult planarIntersectionResult = plane.Intersection(face3D);
            if (planarIntersectionResult == null || !planarIntersectionResult.Intersecting)
                return null;

            return planarIntersectionResult.GetGeometry2Ds<Segment2D>()?.Cast<ICurve2D>();
        }

        public IEnumerable<ICurve3D> GetEdge3Ds(double elevation)
        {
            BoundingBox3D boundingBox3D = PlanarBoundary3D.GetBoundingBox();

            if (elevation < boundingBox3D.Min.Z || elevation > boundingBox3D.Max.Z)
                return null;

            return GetEdge3Ds(new Plane(new Point3D(0, 0, elevation), Vector3D.WorldZ));
        }

        public IEnumerable<ICurve3D> GetEdge3Ds(Plane plane)
        {
            if (plane == null)
                return null;

            Face3D face3D = GetFace3D();
            if (face3D == null)
                return null;

            PlanarIntersectionResult planarIntersectionResult = plane.Intersection(face3D);
            if (planarIntersectionResult == null || !planarIntersectionResult.Intersecting)
                return null;

            return planarIntersectionResult.GetGeometry3Ds<Segment3D>()?.Cast<ICurve3D>();
        }

        public void Move(Vector3D vector3D)
        {
            if (vector3D == null)
                return;

            planarBoundary3D.Move(vector3D);

            if (apertures != null)
                apertures.ForEach(x => x.Move(vector3D));
        }

        public void Transform(Transform3D transform3D)
        {
            if (planarBoundary3D != null)
                planarBoundary3D.Transform(transform3D);

            if (apertures != null && apertures.Count > 0)
                apertures.ForEach(x => x.Transform(transform3D));
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

            //TODO changed tolerance fix value to 0.01, should be changed to angle
            if (!normal.AlmostSimilar(normal_closedPlanar3D, 0.01))
                return null;

            IClosedPlanar3D closedPlanar3D_Projected = plane.Project(closedPlanar3D);
            if (closedPlanar3D_Projected == null)
                return null;

            Point3D point3D_ClosedPlanar3D = plane_closedPlanar3D.Origin;
            Point3D point3D_ClosedPlanar3D_Projected = plane.Project(plane_closedPlanar3D.Origin);

            if (point3D_ClosedPlanar3D.Distance(point3D_ClosedPlanar3D_Projected) >= maxDistance + tolerance)
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

                            //List<Geometry.Planar.Segment2D> segment2Ds = Geometry.Planar.Modify.Split(new List<Geometry.Planar.ISegmentable2D>() { (Geometry.Planar.ISegmentable2D)closed2D, (Geometry.Planar.ISegmentable2D)closed2D_Aperture });

                            List<Geometry.Planar.Polygon2D> polygon2Ds = Geometry.Planar.Create.Polygon2Ds(new List<Geometry.Planar.ISegmentable2D>() { (Geometry.Planar.ISegmentable2D)closed2D, (Geometry.Planar.ISegmentable2D)closed2D_Aperture }); //new Geometry.Planar.PointGraph2D(segment2Ds).GetPolygon2Ds();

                            if (polygon2Ds != null && polygon2Ds.Count > 0)
                            {
                                double area_Difference_Min = double.MaxValue;
                                Geometry.Planar.Polygon2D polygon2D_Min = null;
                                foreach (Geometry.Planar.Polygon2D polygon2D_Temp in polygon2Ds)
                                {
                                    double area_Temp = polygon2D_Temp.GetArea();
                                    if (minArea != 0 && area_Temp <= minArea)
                                        continue;

                                    double area_Difference = System.Math.Abs(area_Aperture - area_Temp);

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

        public bool OffsetAperturesOnEdge(double distance, double tolerance = Tolerance.Distance)
        {
            if (apertures == null || apertures.Count == 0)
                return false;

            Face3D face3D = GetFace3D();
            if (face3D == null)
                return false;

            Plane plane = face3D.GetPlane();

            Geometry.Planar.Polygon2D externalEdge = face3D.ExternalEdge as Geometry.Planar.Polygon2D;
            if (externalEdge == null)
                throw new NotImplementedException();

            List<Geometry.Planar.Polygon2D> polygon2Ds = Geometry.Planar.Query.Offset(externalEdge, -distance, tolerance);
            polygon2Ds.Sort((x, y) => x.GetArea().CompareTo(y.GetArea()));

            externalEdge = polygon2Ds.Last();

            bool result = false;
            for (int i = 0; i < apertures.Count; i++)
            {
                Aperture aperture = apertures[i];

                IClosedPlanar3D closedPlanar3D = aperture?.GetFace3D()?.GetExternalEdge();
                if (closedPlanar3D == null)
                    continue;

                closedPlanar3D = plane.Project(closedPlanar3D);

                Geometry.Planar.Polygon2D externalEdge_Aperture = plane.Convert(closedPlanar3D) as Geometry.Planar.Polygon2D;
                if (externalEdge_Aperture == null)
                    continue;

                List<Geometry.Planar.Point2D> point2Ds_Intersections = Geometry.Planar.Query.Intersections(externalEdge, externalEdge_Aperture);
                if (point2Ds_Intersections == null || point2Ds_Intersections.Count == 0)
                    continue;

                polygon2Ds = Geometry.Planar.Query.Intersection(externalEdge_Aperture, externalEdge, tolerance);
                if (polygon2Ds == null || polygon2Ds.Count == 0)
                    continue;

                polygon2Ds.Sort((x, y) => x.GetArea().CompareTo(y.GetArea()));

                closedPlanar3D = plane.Convert(polygon2Ds.Last());

                apertures[i] = new Aperture(aperture.ApertureConstruction, closedPlanar3D, aperture.PlanarBoundary3D.GetFace3D().GetPlane().Origin);
                result = true;
            }

            return result;
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