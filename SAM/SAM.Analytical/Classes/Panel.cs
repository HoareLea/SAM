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

        /// <summary>
        /// Creates SAM Analytical Panel using information from existing panel and updating its geometry to given face
        /// </summary>
        /// <param name="guid">New Guid of panel</param>
        /// <param name="panel">SAM Analytical Panel</param>
        /// <param name="face">New face for Panel</param>
        /// <param name="apertures">Additional apertures will be added to panel (panel apertures will be included automatically).</param>
        /// <param name="trimGeometry">Trim geometry to make sure it fits on panel</param>
        /// <param name="minArea">Minimal area of aperture to be added to panel</param>
        /// <param name="maxDistance">Max distance between panel and aperture to be added</param>
        public Panel(Guid guid, Panel panel, Face3D face, IEnumerable<Aperture> apertures = null, bool trimGeometry = true, double minArea = Tolerance.MacroDistance, double maxDistance = Tolerance.MacroDistance)
            : base(guid, panel)
        {
            panelType = panel.panelType;
            planarBoundary3D = new PlanarBoundary3D(face);

            List<Aperture> apertures_All = new List<Aperture>();
            if (apertures != null)
                apertures_All.AddRange(apertures);

            if (panel.apertures != null)
                apertures_All.AddRange(panel.apertures);
            
            if (apertures_All.Count > 0)
            {
                foreach (Aperture aperture in apertures_All)
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

        public Point3D GetInternalPoint3D(double tolerance = Tolerance.Distance)
        {
            return SAM.Geometry.Spatial.Query.InternalPoint3D(GetFace3D(), tolerance);
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

        public Plane Plane
        {
            get
            {
                return planarBoundary3D?.Plane;
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

        public double Distance(Point3D point3D)
        {
            return GetFace3D().Distance(point3D);
        }

        public double DistanceToEdges(Point3D point3D)
        {
            return GetFace3D().DistanceToEdges(point3D);
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

        /// <summary>
        /// Transform Panel and Apertures by given Transform. Works only if Panel and Apertures plane axes are similar!
        /// </summary>
        /// <param name="transform3D">Transform3D to be applied</param>
        public void Transform(Transform3D transform3D)
        {
            Plane plane_Panel_Before = planarBoundary3D.Plane;

            if (planarBoundary3D != null)
            {
                planarBoundary3D.Transform(transform3D);
                //Transform3D transform3D_Aperture_Move = Transform3D.GetTranslation(new Vector3D(plane_Panel_Before.Origin, Point3D.Zero));
                //planarBoundary3D.Transform(transform3D_Aperture_Move);
            }

            //Plane plane_Panel_After = planarBoundary3D.Plane;

            if (apertures != null && apertures.Count > 0)
            {
                foreach (Aperture aperture in apertures)
                {
                    Plane plane_Aperture_Before = aperture.Plane;

                    //TODO: Find better way to transform Apertures. Current limitaton: Panel and Apertures plane axes are similar
                    bool flipHand = !plane_Panel_Before.AxisX.SameHalf(plane_Aperture_Before.AxisX);
                    bool flipFacing = !plane_Panel_Before.Normal.SameHalf(plane_Aperture_Before.Normal);
                    aperture.Transform(transform3D, flipHand, flipFacing);// THIS METHOD TO BE REMOVED use aperture.Transform(transform3D) in the future

                    //Option 1
                    //Transform3D transform3D_Aperture = Transform3D.GetPlaneToPlane(plane_Aperture_Before, plane_Panel_Before);
                    //transform3D_Aperture = transform3D_Aperture * transform3D;

                    //Option 2
                    //Transform3D transform3D_Aperture = Transform3D.GetPlaneToPlane(plane_Panel_Before, plane_Aperture_Before);
                    //transform3D_Aperture = transform3D_Aperture * transform3D;

                    //Option 3
                    //Transform3D transform3D_Aperture = Transform3D.GetPlaneToPlane(plane_Aperture_Before, plane_Panel_Before);
                    //transform3D_Aperture = transform3D * transform3D_Aperture;

                    //Option 4
                    //Transform3D transform3D_Aperture = Transform3D.GetPlaneToPlane(plane_Panel_Before, plane_Aperture_Before);
                    //transform3D_Aperture = transform3D * transform3D_Aperture;

                    //Option 5
                    //Transform3D transform3D_Aperture = Transform3D.GetPlaneToPlane(plane_Aperture_Before, plane_Panel_Before);
                    //transform3D_Aperture.Inverse();
                    //transform3D_Aperture = transform3D_Aperture * transform3D;

                    //Option 6
                    //Transform3D transform3D_Aperture = Transform3D.GetPlaneToPlane(plane_Aperture_Before, plane_Panel_Before);
                    //transform3D_Aperture.Inverse();
                    //transform3D_Aperture = transform3D * transform3D_Aperture;

                    //Transform3D transform3D_Aperture = Transform3D.GetPlaneToPlane(aperture.Plane, plane);
                    //aperture.Transform(transform3D_Aperture);
                    //aperture.Transform(transform3D_Aperture);

                    //Option 7
                    //Transform3D transform3D_Aperture = Transform3D.GetPlaneToPlane(plane_Aperture_Before, plane_Panel_Before);
                    //aperture.Transform(transform3D_Aperture);

                    //aperture.Transform(transform3D);

                    //transform3D_Aperture.Inverse();
                    //aperture.Transform(transform3D_Aperture);

                    //Transform3D transform3D_Aperture_Move = Transform3D.GetTranslation(new Vector3D(aperture.Origin, plane_Panel_Before.Origin));
                    //aperture.Transform(transform3D_Aperture_Move);

                    //double angle_X = plane_Aperture_Before.AxisX.Angle(plane_Panel_Before.AxisX);
                    //double angle_Y = plane_Aperture_Before.AxisY.Angle(plane_Panel_Before.AxisY);
                    //double angle_Z = plane_Aperture_Before.AxisZ.Angle(plane_Panel_Before.AxisZ);

                    //Transform3D transform3D_X = Transform3D.GetRotationX(angle_X);
                    //Transform3D transform3D_Y = Transform3D.GetRotationY(angle_Y);
                    //Transform3D transform3D_Z = Transform3D.GetRotationZ(angle_Z);

                    //Transform3D transform3D_Aperture_Rotation = transform3D_X * transform3D_Y * transform3D_Z;
                    //transform3D_Aperture_Rotation.Inverse();

                    //Transform3D transform3D_Aperture_Move = Transform3D.GetTranslation(new Vector3D(plane_Aperture_Before.Origin, Point3D.Zero));
                    //aperture.Transform(transform3D_Aperture_Move);
                    //transform3D_Aperture_Rotation.Inverse();
                    //aperture.Transform(transform3D_Aperture_Rotation);
                    //transform3D_Aperture_Move.Inverse();
                    //aperture.Transform(transform3D_Aperture_Move);

                    //transform3D_Aperture_Move = Transform3D.GetTranslation(new Vector3D(plane_Panel_Before.Origin, plane_Aperture_Before.Origin));
                    //aperture.Transform(transform3D_Aperture_Move);

                    //Transform3D transform3D_Aperture_PlaneToPlane = Transform3D.GetPlaneToPlane(plane_Aperture_Before, planarBoundary3D.Plane);
                    //aperture.Transform(transform3D_Aperture_PlaneToPlane);

                    //Transform3D transform3D_Aperture_PlaneToPlane = Transform3D.GetPlaneToOrigin(plane_Aperture_Before);
                    //transform3D_Aperture_PlaneToPlane.Inverse();

                    //Transform3D transform3D_Rotation = Transform3D.GetRotation(new Point3D(0, 0, 0), new Vector3D(0, 0, 1), System.Math.PI / 2);

                    //transform3D_Aperture_Rotation.Inverse();
                    //aperture.Transform(transform3D_Aperture_Rotation);

                    //transform3D_Aperture_PlaneToPlane = Transform3D.GetPlaneToOrigin(plane_Panel_Before);
                    //planarBoundary3D.Transform(transform3D_Aperture_PlaneToPlane);

                    //transform3D_Aperture_PlaneToPlane = Transform3D.GetOriginToPlane(planarBoundary3D.Plane);
                    //aperture.Transform(transform3D_Aperture_PlaneToPlane);

                    //transform3D_Aperture_Move = Transform3D.GetTranslation(new Vector3D(planarBoundary3D.Plane.Origin, aperture.Plane.Origin));
                    //transform3D_Aperture_Move.Inverse();
                    //aperture.Transform(transform3D_Aperture_Move);

                    //double angle_X = plane_Aperture_Before.AxisX.Angle(plane_Panel_Before.AxisX);
                    //double angle_Y = plane_Aperture_Before.AxisY.Angle(plane_Panel_Before.AxisY);
                    //double angle_Z = plane_Aperture_Before.AxisZ.Angle(plane_Panel_Before.AxisZ);

                    //Transform3D transform3D_X = Transform3D.GetRotationX(angle_X);
                    //Transform3D transform3D_Y = Transform3D.GetRotationY(angle_Y);
                    //Transform3D transform3D_Z = Transform3D.GetRotationZ(angle_Z);

                    //Transform3D transform3D_Aperture_Rotation = transform3D_X * transform3D_Y * transform3D_Z;
                    //transform3D_Aperture_Rotation.Inverse();
                    //aperture.Transform(transform3D_Aperture_Rotation);

                    //aperture.Transform(transform3D);

                    //double angle_X_After = aperture.Plane.AxisX.Angle(plane_Panel_Before.AxisX);
                    //double angle_Y_After = aperture.Plane.AxisY.Angle(plane_Panel_Before.AxisY);
                    //double angle_Z_After = aperture.Plane.AxisZ.Angle(plane_Panel_Before.AxisZ);

                    //transform3D_Aperture.Inverse();
                    //transform3D_Aperture_Move.Inverse();
                    //transform3D_Aperture.Inverse();

                    //aperture.Transform(transform3D_Aperture);
                    //aperture.Transform(transform3D_Aperture_Move);
                    //aperture.Transform(transform3D_Aperture);
                }
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

            //Flipping if not match with Aperture plane 
            Vector3D normal = plane.Normal;
            Vector3D normal_closedPlanar3D = plane_closedPlanar3D.Normal;
            if (!normal.SameHalf(normal_closedPlanar3D))
                plane.FlipZ(false);

            if (!plane.AxisX.SameHalf(plane_closedPlanar3D.AxisX))
                plane.FlipX(true);

            if (!normal.Collinear(normal_closedPlanar3D, Tolerance.MacroDistance))
                return null;

            IClosedPlanar3D closedPlanar3D_Projected = plane.Project(closedPlanar3D);
            if (closedPlanar3D_Projected == null)
                return null;

            Point3D point3D_ClosedPlanar3D = plane_closedPlanar3D.Origin;
            Point3D point3D_ClosedPlanar3D_Projected = plane.Project(plane_closedPlanar3D.Origin);

            if (point3D_ClosedPlanar3D.Distance(point3D_ClosedPlanar3D_Projected) >= maxDistance + tolerance)
                return null;

            IClosed2D closed2D_Aperture = plane.Convert(closedPlanar3D_Projected);

            if (minArea != 0 && closed2D_Aperture.GetArea() <= minArea)
                return null;

            Point3D point3D_Location;
            Point2D point2D_Centroid = closed2D_Aperture.GetCentroid();
            point3D_Location = plane.Convert(point2D_Centroid);
            if (Geometry.Spatial.Query.Vertical(plane, tolerance))
                point3D_Location = new Point3D(point3D_Location.X, point3D_Location.Y, closedPlanar3D_Projected.GetBoundingBox().Min.Z);

            if (trimGeometry)
            {
                if (closed2D_Aperture is ISegmentable2D)
                {
                    IClosed2D closed2D = plane.Convert(planarBoundary3D.GetFace3D().GetExternalEdge());
                    if (closed2D is ISegmentable2D)
                    {
                        if (!closed2D.Inside(closed2D_Aperture))
                        {
                            double area_Aperture = closed2D_Aperture.GetArea();

                            //List<Geometry.Planar.Segment2D> segment2Ds = Geometry.Planar.Modify.Split(new List<Geometry.Planar.ISegmentable2D>() { (Geometry.Planar.ISegmentable2D)closed2D, (Geometry.Planar.ISegmentable2D)closed2D_Aperture });

                            List<Polygon2D> polygon2Ds = Geometry.Planar.Create.Polygon2Ds(new List<ISegmentable2D>() { (ISegmentable2D)closed2D, (ISegmentable2D)closed2D_Aperture }); //new Geometry.Planar.PointGraph2D(segment2Ds).GetPolygon2Ds();

                            if (polygon2Ds != null && polygon2Ds.Count > 0)
                            {
                                double area_Difference_Min = double.MaxValue;
                                Polygon2D polygon2D_Min = null;
                                foreach (Polygon2D polygon2D_Temp in polygon2Ds)
                                {
                                    double area_Temp = polygon2D_Temp.GetArea();
                                    if (minArea != 0 && area_Temp <= minArea)
                                        continue;

                                    double area_Difference = System.Math.Abs(area_Aperture - area_Temp);

                                    if (area_Difference > area_Difference_Min)
                                        continue;

                                    Point2D point2D = polygon2D_Temp.GetInternalPoint2D();
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

            Polygon2D externalEdge = face3D.ExternalEdge as Polygon2D;
            if (externalEdge == null)
                throw new NotImplementedException();

            List<Polygon2D> polygon2Ds = Geometry.Planar.Query.Offset(externalEdge, -distance, tolerance);
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

                Polygon2D externalEdge_Aperture = plane.Convert(closedPlanar3D) as Polygon2D;
                if (externalEdge_Aperture == null)
                    continue;

                List<Point2D> point2Ds_Intersections = Geometry.Planar.Query.Intersections(externalEdge, externalEdge_Aperture);
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

        public double GetThinnessRatio()
        {
            return Geometry.Planar.Query.ThinnessRatio(planarBoundary3D.Edge2DLoop.GetClosed2D());
        }
    }
}