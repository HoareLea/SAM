using Newtonsoft.Json.Linq;

using SAM.Core;
using SAM.Geometry.Spatial;

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