using Newtonsoft.Json.Linq;

using SAM.Core;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    /// <summary>
    /// Analytical Aperture object which stores information about Winodws and Doors
    /// </summary>
    public class Aperture : SAMInstance<ApertureConstruction>, IFace3DObject, IAnalyticalObject
    {
        /// <summary>
        /// Planar Boundary 3D of Aperture
        /// </summary>
        private PlanarBoundary3D planarBoundary3D;

        /// <summary>
        ///  Constructor for Analytical Aperture
        /// </summary>
        /// <param name="aperture">Other Aperture</param>
        public Aperture(Aperture aperture)
            : base(aperture, aperture?.Type)
        {
            planarBoundary3D = new PlanarBoundary3D(aperture.planarBoundary3D);
        }

        public Aperture(Guid guid, Aperture aperture, IClosedPlanar3D closedPlanar3D)
            : base(guid, aperture)
        {
            planarBoundary3D = new PlanarBoundary3D(closedPlanar3D);
        }

        public Aperture(JObject jObject)
            : base(jObject)
        {
        }

        public Aperture(ApertureConstruction apertureConstruction, IClosedPlanar3D closedPlanar3D)
            : base(Guid.NewGuid(), apertureConstruction)
        {
            if (closedPlanar3D != null)
                planarBoundary3D = new PlanarBoundary3D(closedPlanar3D);
        }

        public Aperture(Aperture aperture, ApertureConstruction apertureConstruction)
            : base(aperture, apertureConstruction)
        {
            if (aperture.planarBoundary3D != null)
                planarBoundary3D = new PlanarBoundary3D(aperture.planarBoundary3D);
        }

        public Aperture(ApertureConstruction apertureConstruction, IClosedPlanar3D closedPlanar3D, Point3D location)
            : base(Guid.NewGuid(), null, apertureConstruction)
        {
            if (closedPlanar3D != null)
                planarBoundary3D = new PlanarBoundary3D(closedPlanar3D, location);
        }

        public Aperture(Aperture aperture, PlanarBoundary3D planarBoundary3D)
            : base(aperture, aperture.ApertureConstruction)
        {
            if (planarBoundary3D != null)
                this.planarBoundary3D = new PlanarBoundary3D(planarBoundary3D);
        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
                return false;

            planarBoundary3D = new PlanarBoundary3D(jObject.Value<JObject>("PlanarBoundary3D"));
            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
                return jObject;

            jObject.Add("PlanarBoundary3D", planarBoundary3D.ToJObject());

            return jObject;
        }

        public Face3D GetFace3D()
        {
            if (planarBoundary3D == null)
                return null;

            return planarBoundary3D.GetFace3D();
        }

        public IClosedPlanar3D GetExternalEdge3D()
        {
            return GetFace3D().GetExternalEdge3D();
        }

        public List<Face3D> GetFace3Ds(AperturePart aperturePart)
        {
            Face3D face3D = GetFace3D();
            if (face3D == null)
            {
                return null;
            }

            if (aperturePart == AperturePart.Undefined)
            {
                return new List<Face3D>() { face3D };
            }

            List<Geometry.Planar.IClosed2D> internalEdge2Ds = face3D.InternalEdge2Ds;
            if(internalEdge2Ds == null || internalEdge2Ds.Count == 0)
            {
                double frameThickness = 0;

                ApertureConstruction apertureConstruction = Type;
                if(apertureConstruction != null)
                {
                    if(apertureConstruction.TryGetValue(ApertureConstructionParameter.DefaultFrameWidth, out double frameThickness_Temp))
                    {
                        frameThickness = frameThickness_Temp;
                    }
                    
                    if(frameThickness == 0 || double.IsNaN(frameThickness))
                    {
                        frameThickness = apertureConstruction.GetFrameThickness();
                    }
                }

                if(!double.IsNaN(frameThickness) && frameThickness != 0)
                {
                    Plane plane = face3D.GetPlane();
                    Geometry.Planar.Face2D face2D = plane.Convert(face3D);

                    Geometry.Planar.IClosed2D externalEdge2D = face3D.ExternalEdge2D;

                    List<Geometry.Planar.Face2D> face2Ds = Geometry.Planar.Query.Offset(face2D, -frameThickness);
                    internalEdge2Ds = face2Ds?.ConvertAll(x => x.ExternalEdge2D);

                    face2D = Geometry.Planar.Create.Face2D(externalEdge2D, internalEdge2Ds);
                    face3D = plane.Convert(face2D);
                }
            }

            List<IClosedPlanar3D> internalEdge3Ds = face3D?.GetInternalEdge3Ds();

            switch (aperturePart)
            {
                case AperturePart.Pane:
                    return internalEdge3Ds == null || internalEdge3Ds.Count == 0 ? new List<Face3D>() { face3D } : internalEdge3Ds.ConvertAll(x => new Face3D(x));

                case AperturePart.Frame:
                    return internalEdge3Ds == null || internalEdge3Ds.Count == 0 ? new List<Face3D>() : new List<Face3D>() { face3D };

            }

            return null;
        }

        public Face3D GetFrameFace3D()
        {
            List<Face3D> face3Ds = GetFace3Ds(AperturePart.Frame);
            if(face3Ds == null || face3Ds.Count == 0)
            {
                return null;
            }

            if(face3Ds.Count > 0)
            {
                face3Ds.Sort((x, y) => y.GetArea().CompareTo(x.GetArea()));
            }

            return face3Ds[0];
        }

        public List<Face3D> GetPaneFace3Ds()
        {
            return GetFace3Ds(AperturePart.Pane);
        }

        public Face3D Face3D
        {
            get
            {
                if (planarBoundary3D == null)
                    return null;

                return planarBoundary3D.GetFace3D();
            }
        }

        public double GetArea()
        {
            Geometry.Planar.IClosed2D closed2D = GetFace3D()?.ExternalEdge2D;
            if (closed2D == null)
                return double.NaN;

            return closed2D.GetArea();
        }

        public double GetFrameArea()
        {
            return GetArea(AperturePart.Frame);
        }

        public double GetPaneArea()
        {
            return GetArea(AperturePart.Pane);
        }

        public double GetArea(AperturePart aperturePart)
        {
            List<Face3D> face3Ds = GetFace3Ds(aperturePart);
            if(face3Ds == null || face3Ds.Count ==0)
            {
                return 0;
            }

            return face3Ds.ConvertAll(x => x.GetArea()).Sum();
        }

        /// <summary>
        /// Frame Factor (0-1)
        /// </summary>
        /// <returns>Frame Factor (0-1)</returns>
        public double GetFrameFactor()
        {
            double area_Frame = GetArea(AperturePart.Frame);
            if(double.IsNaN(area_Frame) || area_Frame == 0)
            {
                return 0;
            }

            double area_Pane = GetArea(AperturePart.Pane);
            if(double.IsNaN(area_Pane) || area_Pane == 0)
            {
                return 1;
            }

            return area_Frame / (area_Frame + area_Pane);
        }

        public double GetThickness(AperturePart aperturePart)
        {
            if(aperturePart == AperturePart.Undefined)
            {
                return double.NaN;
            }

            ApertureConstruction apertureConstruction = Type;
            if(apertureConstruction == null)
            {
                return double.NaN;
            }

            return apertureConstruction.GetThickness(aperturePart);
        }

        public Aperture Clone()
        {
            return new Aperture(this);
        }

        public Plane Plane
        {
            get
            {
                if (planarBoundary3D == null)
                    return null;

                return planarBoundary3D.Plane;
            }
        }

        public double GetWidth()
        {
            return Query.Width(planarBoundary3D);
        }

        public double GetHeight()
        {
            return Query.Height(planarBoundary3D);
        }

        public void Move(Vector3D vector3D)
        {
            if (vector3D == null)
                return;

            planarBoundary3D.Move(vector3D);
        }

        public void Transform(Transform3D transform3D)
        {
            if (planarBoundary3D != null)
                planarBoundary3D.Transform(transform3D);
        }

        /// <summary>
        /// TEMPORARY METHOD to Transform Aperture with Plane. Find the way to use Transform(Transform3D transform3D) method
        /// </summary>
        /// <param name="transform3D">Transform3D</param>
        /// <param name="flipHand">Flip Hand</param>
        /// <param name="flipFacing">Flip Facing</param>
        internal void Transform(Transform3D transform3D, bool flipHand, bool flipFacing)
        {
            //TODO: Remove this method and find better way to transform Aperture
            if (transform3D == null)
                return;

            BoundaryEdge3DLoop boundaryEdge3DLoop_External = planarBoundary3D.GetExternalEdge3DLoop();
            if (boundaryEdge3DLoop_External == null)
                return;

            boundaryEdge3DLoop_External.Transform(transform3D);

            List<BoundaryEdge3DLoop> boundaryEdge3DLoops_Internal = planarBoundary3D.GetInternalEdge3DLoops();
            if (boundaryEdge3DLoops_Internal != null && boundaryEdge3DLoops_Internal.Count > 0)
            {
                foreach (BoundaryEdge3DLoop boundaryEdge3DLoop_Internal in boundaryEdge3DLoops_Internal)
                    boundaryEdge3DLoop_Internal.Transform(transform3D);
            }

            Plane plane = planarBoundary3D.Plane;

            Vector3D normal = plane.Normal;
            Vector3D axisX = plane.AxisX;
            if (flipHand)
                axisX.Negate();

            if (flipFacing)
                normal.Negate();

            Vector3D axisY = Geometry.Spatial.Query.AxisY(normal, axisX);

            plane = new Plane(plane.Origin, axisX, axisY).Transform(transform3D);

            normal = plane.Normal;
            axisX = plane.AxisX;
            if (flipHand)
                axisX.Negate();

            if (flipFacing)
                normal.Negate();

            axisY = Geometry.Spatial.Query.AxisY(normal, axisX);

            plane = new Plane(plane.Origin, axisX, axisY);

            planarBoundary3D = new PlanarBoundary3D(planarBoundary3D, plane, new BoundaryEdge2DLoop(plane, boundaryEdge3DLoop_External), boundaryEdge3DLoops_Internal?.ConvertAll(x => new BoundaryEdge2DLoop(plane, x)));
        }

        public ApertureConstruction ApertureConstruction
        {
            get
            {
                return Type;
            }
        }

        public ApertureType ApertureType
        {
            get
            {
                ApertureConstruction apertureConstruction = ApertureConstruction;
                if (apertureConstruction == null)
                    return ApertureType.Undefined;

                return apertureConstruction.ApertureType;
            }
        }

        public BoundingBox3D GetBoundingBox(double offset = 0)
        {
            return GetFace3D().GetBoundingBox(offset);
        }

        public PlanarBoundary3D PlanarBoundary3D
        {
            get
            {
                if(planarBoundary3D == null)
                {
                    return null;
                }
                
                return new PlanarBoundary3D(planarBoundary3D);
            }
        }

        public void Normalize(double tolerance = Tolerance.Distance)
        {
            planarBoundary3D?.Normalize(tolerance);
        }

        public void FlipNormal(bool flipX = true)
        {
            Face3D face3D = planarBoundary3D?.GetFace3D();
            if (face3D == null)
                return;

            face3D.FlipNormal(flipX);

            planarBoundary3D = new PlanarBoundary3D(face3D);
        }
    }
}