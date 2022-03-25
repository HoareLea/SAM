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
    public class Panel : SAMInstance<Construction>, IFace3DObject, IAnalyticalObject
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
        internal Panel(Panel panel)
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
        internal Panel(Panel panel, Construction construction)
            : base(panel, construction)
        {
            planarBoundary3D = new PlanarBoundary3D(panel.planarBoundary3D);
            panelType = panel.panelType;

            if (panel.apertures != null)
                apertures = new List<Aperture>(panel.apertures.ConvertAll(x => new Aperture(x)));
        }

        internal Panel(string name, Panel panel, Construction construction)
            : base(panel, construction)
        {
            planarBoundary3D = panel == null ? null : new PlanarBoundary3D(panel.planarBoundary3D);
            panelType = panel == null ? PanelType.Undefined : panel.panelType;

            if (panel?.apertures != null)
                apertures = new List<Aperture>(panel.apertures.ConvertAll(x => new Aperture(x)));
        }

        /// <summary>
        /// Creates new Panles by given Panel and new PanelType
        /// </summary>
        /// <param name="panel">SAM Analytical Panel</param>
        /// <param name="panelType">SAM Analytical PanelType</param>
        internal Panel(Panel panel, PanelType panelType)
            : base(panel)
        {
            planarBoundary3D = new PlanarBoundary3D(panel.planarBoundary3D);
            this.panelType = panelType;

            if (panel.apertures != null)
                apertures = new List<Aperture>(panel.apertures.ConvertAll(x => new Aperture(x)));
        }

        internal Panel(Construction construction, PanelType panelType, Face3D face)
            : base(construction)
        {
            this.panelType = panelType;
            planarBoundary3D = new PlanarBoundary3D(face);
        }

        internal Panel(Construction construction, PanelType panelType, PlanarBoundary3D planarBoundary3D)
            : base(construction)
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
        /// <param name="trimGeometry">Trim apertures geometry to make sure it fits on panel</param>
        /// <param name="minArea">Minimal area of aperture to be added to panel</param>
        /// <param name="maxDistance">Max distance between panel and aperture to be added</param>
        internal Panel(Guid guid, Panel panel, Face3D face, IEnumerable<Aperture> apertures = null, bool trimGeometry = true, double minArea = Tolerance.MacroDistance, double maxDistance = Tolerance.MacroDistance)
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
                    Modify.AddApertures(this, aperture.ApertureConstruction, aperture.GetFace3D(), trimGeometry, minArea, maxDistance);
                }
            }
        }

        /// <summary>
        /// This constructor does not copy apertures acorss
        /// </summary>
        /// <param name="guid">New Guid for Panel</param>
        /// <param name="panel">Old Panel used as base</param>
        /// <param name="planarBoundary3D">New PlanarBoundary</param>
        internal Panel(Guid guid, Panel panel, PlanarBoundary3D planarBoundary3D)
            : base(guid, panel)
        {
            panelType = panel.panelType;
            this.planarBoundary3D = planarBoundary3D;
        }

        internal Panel(Guid guid, string name, IEnumerable<ParameterSet> parameterSets, Construction construction, PanelType panelType, PlanarBoundary3D planarBoundary3D)
            : base(guid, parameterSets, construction)
        {
            this.panelType = panelType;
            this.planarBoundary3D = new PlanarBoundary3D(planarBoundary3D);
        }

        internal Panel(JObject jObject)
             : base(jObject)
        {
        }

        /// <summary>
        /// Gets Geometrical Representation of Panel (None Analytical Data)
        /// </summary>
        /// <returns name="face3D">SAM Geometry Face3D</returns>
        public Face3D GetFace3D(bool cutOpenings = false)
        {
            Face2D face2D = planarBoundary3D?.GetFace2D();
            if (face2D == null)
                return null;

            Plane plane = planarBoundary3D.Plane;

            if(cutOpenings && apertures != null && apertures.Count != 0)
            {
                List<IClosed2D> internalEdges = face2D.InternalEdge2Ds;
                if(internalEdges == null)
                {
                    internalEdges = new List<IClosed2D>();
                }

                foreach(Aperture aperture in apertures)
                {
                    Face3D face3D_Aperture = aperture?.GetFace3D();
                    if (face3D_Aperture == null)
                    {
                        continue;
                    }

                    IClosed2D internalEdge = plane.Convert(face3D_Aperture)?.ExternalEdge2D;
                    if(internalEdge == null)
                    {
                        continue;
                    }

                    internalEdges.Add(internalEdge);
                }

                if(internalEdges != null && internalEdges.Count > 0)
                {
                    face2D = Geometry.Planar.Create.Face2D(face2D.ExternalEdge2D, internalEdges);
                }
            }

            return plane.Convert(face2D);
        }

        public Point3D GetInternalPoint3D(double tolerance = Tolerance.Distance)
        {
            return Geometry.Spatial.Query.InternalPoint3D(GetFace3D(), tolerance);
        }

        public double GetArea()
        {
            Face3D face3D = GetFace3D();
            if (face3D == null)
                return double.NaN;

            return face3D.GetArea();
        }

        /// <summary>
        /// Area of Panel without Apertures
        /// </summary>
        /// <returns>Net Area</returns>
        public double GetAreaNet()
        {
            if(apertures == null || apertures.Count == 0)
            {
                return GetArea();
            }

            Face3D face3D = GetFace3D(true);
            if(face3D == null)
            {
                return double.NaN;
            }

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

        public double GetTilt()
        {
            return Query.Tilt(this);
        }

        public double GetAzimuth()
        {
            return Query.Azimuth(this);
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
            return GetFace3D()?.GetBoundingBox(offset);
        }

        public Construction Construction
        {
            get
            {
                return Type;
            }
        }

        public PanelType PanelType
        {
            get
            {
                return panelType;
            }
        }

        public PanelGroup PanelGroup
        {
            get
            {
                return panelType.PanelGroup();
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

            PlanarIntersectionResult planarIntersectionResult = plane.PlanarIntersectionResult(face3D);
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

            PlanarIntersectionResult planarIntersectionResult = plane.PlanarIntersectionResult(face3D);
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

            if (jObject.ContainsKey("PlanarBoundary3D"))
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

            if (planarBoundary3D != null)
                jObject.Add("PlanarBoundary3D", planarBoundary3D.ToJObject());

            if (apertures != null)
                jObject.Add("Apertures", Core.Create.JArray(apertures));

            return jObject;
        }

        public bool AddAperture(Aperture aperture)
        {
            if (aperture == null)
                return false;

            if (!Query.IsValid(this, aperture))
                return false;

            if (apertures == null)
                apertures = new List<Aperture>();

            apertures.Add(aperture);
            return true;
        }

        public bool RemoveAperture(Guid guid)
        {
            if (apertures == null || apertures.Count == 0)
                return false;

            for(int i=0; i < apertures.Count; i++)
            {
                if(apertures[i].Guid.Equals(guid))
                {
                    apertures.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        public void RemoveApertures()
        {
            apertures = null;
        }

        public Aperture GetAperture(Guid guid)
        {
            if (apertures == null || apertures.Count == 0)
                return null;

            for (int i = 0; i < apertures.Count; i++)
            {
                if (apertures[i].Guid.Equals(guid))
                    return new Aperture(apertures[i]);
            }

            return null;
        }

        public bool HasAperture(Guid guid)
        {
            if (apertures == null || apertures.Count == 0)
                return false;

            for (int i = 0; i < apertures.Count; i++)
                if (apertures[i].Guid.Equals(guid))
                    return true;

            return false;
        }

        public void OffsetAperturesOnEdge(double distance, double tolerance = Tolerance.Distance)
        {
            apertures = Query.OffsetAperturesOnEdge(this, distance, tolerance);
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

        public List<Aperture> GetApertures(ApertureConstruction apertureConstruction)
        {
            if (apertures == null || apertureConstruction == null)
                return null;

            List<Aperture> result = new List<Aperture>();
            foreach (Aperture aperture in apertures)
                if (aperture != null && aperture.TypeGuid == apertureConstruction.Guid)
                    result.Add(aperture.Clone());

            return result;
        }

        public double GetThinnessRatio()
        {
            return Geometry.Planar.Query.ThinnessRatio(planarBoundary3D.ExternalEdge2DLoop.GetClosed2D());
        }

        public void FlipNormal(bool includeApertures, bool flipX = true)
        {
            Face3D face3D = planarBoundary3D?.GetFace3D();
            if (face3D == null)
                return;

            face3D.FlipNormal(flipX);

            planarBoundary3D = new PlanarBoundary3D(face3D);

            if (apertures != null && includeApertures)
            {
                foreach (Aperture aperture in apertures)
                    aperture.FlipNormal(flipX);
            }
        }

        public void Normalize(bool includeApertures = true, double tolerance = Tolerance.Distance)
        {
            planarBoundary3D?.Normalize(tolerance);

            if (apertures != null && includeApertures)
            {
                foreach (Aperture aperture in apertures)
                    aperture.Normalize(tolerance);
            }
        }

        public bool IsExternal()
        {
            return Query.External(panelType);
        }

        public bool IsExposedToSun()
        {
            return Query.ExposedToSun(panelType);
        }

        public bool IsInternal()
        {
            return Query.Internal(panelType);
        }

        public bool IsGround()
        {
            return Query.Ground(panelType);
        }

        public bool HasApertures
        {
            get
            {
                return apertures != null && apertures.Count != 0;
            }
        }

        public Face3D Face3D
        {
            get
            {
                Face2D face2D = planarBoundary3D?.GetFace2D();
                if (face2D == null)
                {
                    return null;
                }

                Plane plane = planarBoundary3D.Plane;
                if(plane == null)
                {
                    return null;
                }

                return plane.Convert(face2D);
            }
        }
    }
}