using Newtonsoft.Json.Linq;

using SAM.Geometry.Spatial;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public class Opening<T> : BuildingElement<T>, IOpening  where T : OpeningType
    {
        public Opening(Opening<T> opening)
            : base(opening)
        {

        }

        public Opening(JObject jObject)
            : base(jObject)
        {

        }

        public Opening(T openingType, Face3D face3D)
            : base(openingType, face3D)
        {

        }

        public Opening(System.Guid guid, T openingType, Face3D face3D)
            : base(guid, openingType, face3D)
        {

        }

        public Opening(System.Guid guid, Opening<T> opening, Face3D face3D)
            : base(guid, opening, face3D)
        {

        }

        public override double GetArea()
        {
            Geometry.Planar.IClosed2D closed2D = Face3D?.ExternalEdge2D;
            if(closed2D == null)
            {
                return double.NaN;
            }

            return closed2D.GetArea();
        }

        public List<Face3D> GetFace3Ds(OpeningPart openingPart)
        {
            Face3D face3D = Face3D;
            if (face3D == null)
            {
                return null;
            }

            if (openingPart == OpeningPart.Undefined)
            {
                return new List<Face3D>() { face3D };
            }

            List<Geometry.Planar.IClosed2D> internalEdge2Ds = face3D.InternalEdge2Ds;
            if (internalEdge2Ds == null || internalEdge2Ds.Count == 0)
            {
                double frameThickness = 0;

                T openingType = Type;
                if (openingType != null)
                {
                    frameThickness = openingType.GetFrameThickness();
                }

                if (!double.IsNaN(frameThickness) && frameThickness != 0)
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

            switch (openingPart)
            {
                case OpeningPart.Pane:
                    return internalEdge3Ds == null || internalEdge3Ds.Count == 0 ? new List<Face3D>() { face3D } : internalEdge3Ds.ConvertAll(x => new Face3D(x));

                case OpeningPart.Frame:
                    return internalEdge3Ds == null || internalEdge3Ds.Count == 0 ? new List<Face3D>() : new List<Face3D>() { face3D };

            }

            return null;
        }

        public Face3D GetFrameFace3D()
        {
            List<Face3D> face3Ds = GetFace3Ds(OpeningPart.Frame);
            if (face3Ds == null || face3Ds.Count == 0)
            {
                return null;
            }

            if (face3Ds.Count > 0)
            {
                face3Ds.Sort((x, y) => y.GetArea().CompareTo(x.GetArea()));
            }

            return face3Ds[0];
        }

        public List<Face3D> GetPaneFace3Ds()
        {
            return GetFace3Ds(OpeningPart.Pane);
        }

        public double GetFrameArea()
        {
            return GetArea(OpeningPart.Frame);
        }

        public double GetPaneArea()
        {
            return GetArea(OpeningPart.Pane);
        }

        public double GetArea(OpeningPart openingPart)
        {
            List<Face3D> face3Ds = GetFace3Ds(openingPart);
            if (face3Ds == null || face3Ds.Count == 0)
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
            double area_Frame = GetArea(OpeningPart.Frame);
            if (double.IsNaN(area_Frame) || area_Frame == 0)
            {
                return 0;
            }

            double area_Pane = GetArea(OpeningPart.Pane);
            if (double.IsNaN(area_Pane) || area_Pane == 0)
            {
                return 1;
            }

            return area_Frame / (area_Frame + area_Pane);
        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
            {
                return false;
            }

            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();

            if (jObject == null)
            {
                return jObject;
            }

            return jObject;
        }

    }
}
