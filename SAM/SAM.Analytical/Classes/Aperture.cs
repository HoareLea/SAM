using Newtonsoft.Json.Linq;

using SAM.Core;
using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Analytical
{
    /// <summary>
    /// Analytical Aperture object which stores information about Winodws and Doors
    /// </summary>
    public class Aperture : SAMInstance
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
            : base(aperture, aperture?.SAMType)
        {
            planarBoundary3D = new PlanarBoundary3D(aperture.planarBoundary3D);
        }

        public Aperture(JObject jObject)
            : base(jObject)
        {
        }

        public Aperture(ApertureConstruction apertureConstruction, IClosedPlanar3D closedPlanar3D)
            : base(System.Guid.NewGuid(), apertureConstruction)
        {
            if (closedPlanar3D != null)
                planarBoundary3D = new PlanarBoundary3D(closedPlanar3D);
        }

        public Aperture(ApertureConstruction apertureConstruction, IClosedPlanar3D closedPlanar3D, Point3D location)
            : base(System.Guid.NewGuid(), apertureConstruction)
        {
            if (closedPlanar3D != null)
                planarBoundary3D = new PlanarBoundary3D(closedPlanar3D, location);
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

        public double GetArea()
        {
            Face3D face3D = GetFace3D();
            if (face3D == null)
                return double.NaN;

            return face3D.GetArea();
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

            BoundaryEdge3DLoop boundaryEdge3DLoop_External = planarBoundary3D.GetEdge3DLoop();
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
                return SAMType as ApertureConstruction;
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
                return planarBoundary3D;
            }
        }
    }
}