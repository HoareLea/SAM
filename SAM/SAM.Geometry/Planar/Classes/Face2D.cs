using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace SAM.Geometry.Planar
{
    public class Face2D : Face, ISAMGeometry2D
    {
        public Face2D(Face2D face2D)
            : base(face2D)
        {

        }

        public Face2D(IClosed2D externalEdge)
            : base(externalEdge)
        {

        }

        public Face2D(JObject jObject)
            : base(jObject)
        {

        }
        
        public override ISAMGeometry Clone()
        {
            return new Face2D(this);
        }


        public static Face2D GetFace(IClosed2D externalEdge, IEnumerable<IClosed2D> internalEdges)
        {
            Face2D result = new Face2D(externalEdge);
            if(internalEdges != null && internalEdges.Count() > 0)
            {
                result.internalEdges = new List<IClosed2D>();
                foreach (IClosed2D closed2D in internalEdges)
                {
                    if (externalEdge.Inside(closed2D))
                        result.internalEdges.Add(closed2D);

                }
            }

            return result;
        }
        
        public static Face2D GetFace(IEnumerable<IClosed2D> edges, out List<IClosed2D> edges_Excluded)
        {
            edges_Excluded = null;

            if (edges == null || edges.Count() == 0)
                return null;

            IClosed2D closed2D_Max = null;
            double area_Max = double.MinValue;
            foreach (IClosed2D closed2D in edges)
            {
                double area = closed2D.GetArea();
                if (area > area_Max)
                {
                    area_Max = area;
                    closed2D_Max = closed2D;
                }

            }

            if (closed2D_Max == null)
                return null;

            Face2D result = new Face2D(closed2D_Max);
            edges_Excluded = new List<IClosed2D>();
            foreach (IClosed2D closed2D in edges)
            {
                if (result == closed2D_Max)
                    continue;

                if (!closed2D_Max.Inside(closed2D))
                {
                    edges_Excluded.Add(closed2D);
                    continue;
                }

                if (result.internalEdges == null)
                    result.internalEdges = new List<IClosed2D>();

                result.internalEdges.Add((IClosed2D)closed2D.Clone());
            }

            return result;
        }

        public static Face2D GetFace(IEnumerable<IClosed2D> edges)
        {
            List<Planar.IClosed2D> edges_Excluded = null;
            return GetFace(edges, out edges_Excluded);
        }

        public static List<Face2D> GetFaces(IEnumerable<IClosed2D> edges)
        {
            if (edges == null)
                return null;

            List<Face2D> result = new List<Face2D>();
            if (edges.Count() == 0)
                return result;

            List<Planar.IClosed2D> edges_Current = new List<IClosed2D>(edges);
            while (edges_Current.Count > 0)
            {
                List<IClosed2D> edges_Excluded = null;
                Face2D face = GetFace(edges_Current, out edges_Excluded);
                if (face == null)
                    break;

                result.Add(face);

                edges_Current = edges_Excluded;
            }

            return result;
        }
    }
}
