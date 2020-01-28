using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAM.Geometry.Grasshopper
{
    public static partial class Convert
    { 

        public static List<Spatial.IGeometry3D> ToSAM(this IGH_GeometricGoo geometricGoo, bool simplify = true)
        {
            if (geometricGoo is GH_Curve)
                return new List<Spatial.IGeometry3D>() { ((GH_Curve)geometricGoo).ToSAM(simplify)};

            if (geometricGoo is GH_Surface)
                return ((GH_Surface)geometricGoo).ToSAM(simplify);

            if (geometricGoo is GH_Point)
                return new List<Spatial.IGeometry3D>() { ((GH_Point)geometricGoo).ToSAM() };

            if(geometricGoo is GH_Brep)
                return ((GH_Brep)geometricGoo).ToSAM(simplify);

            return (geometricGoo as dynamic).ToSAM();
        }

        public static List<Spatial.IGeometry3D> ToSAM(this Brep brep, bool simplify = true)
        {
            List<Spatial.IGeometry3D> result = new List<Spatial.IGeometry3D>();

            foreach (BrepFace brepFace in brep.Faces)
            {
                if (brepFace.IsPlanar(Tolerance.MicroDistance))
                {
                    foreach (BrepLoop brepLoop in brepFace.Loops)
                        result.Add(brepLoop.ToSAM(simplify));
                }
                else
                {
                    var meshingParameters = new MeshingParameters
                    {
                        SimplePlanes = true,
                        MinimumEdgeLength = 0.2,
                        Tolerance = 0.1
                    };
                    var surfaceMesh = Mesh.CreateFromSurface(brepFace.UnderlyingSurface(), meshingParameters);

                    foreach (var face in surfaceMesh.Faces)
                    {
                        List<Spatial.Point3D> point3Ds = new List<Spatial.Point3D>();
                        //Split to triangle and Quad
                        point3Ds.Add(new Spatial.Point3D(Convert.ToSAM(surfaceMesh.Vertices[face.A])));
                        point3Ds.Add(new Spatial.Point3D(Convert.ToSAM(surfaceMesh.Vertices[face.B])));
                        point3Ds.Add(new Spatial.Point3D(Convert.ToSAM(surfaceMesh.Vertices[face.C])));

                        if (face.IsQuad)
                            point3Ds.Add(new Spatial.Point3D(Convert.ToSAM(surfaceMesh.Vertices[face.D])));

                        result.Add(new Spatial.Polygon3D(point3Ds));
                    }
                }
            }

            return result;
        }

        public static List<Spatial.IGeometry3D> ToSAM(this GH_Surface surface, bool simplify = true)
        {
            return ToSAM(surface.Value);
        }

        public static List<Spatial.IGeometry3D> ToSAM(this Surface surface, bool simplify = true)
        {
            List<Spatial.IGeometry3D> result = new List<Spatial.IGeometry3D>();
            foreach (BrepLoop brepLoop in surface.ToBrep().Loops)
                result.Add(brepLoop.ToSAM(simplify));

            return result;
        }

    }
}
