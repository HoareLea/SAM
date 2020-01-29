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

            List<Brep> breps = new List<Brep>();
            List<BrepFace> brepFaces = new List<BrepFace>();
            foreach (BrepFace brepFace in brep.Faces)
            {
                if (!brepFace.IsPlanar(Tolerance.MicroDistance))
                    breps.Add(brepFace.Brep);
                else
                    brepFaces.Add(brepFace);
            }

            if(breps != null && breps.Count > 0)
            {
                Mesh mesh = new Mesh();
                foreach(Brep brep_Temp in breps)
                {
                    Mesh[] meshes = Mesh.CreateFromBrep(brep_Temp, AssemblyInfo.GetMeshingParameters());
                    mesh.Append(meshes);
                }
                Brep brep_Mesh = Brep.CreateFromMesh(mesh, true);
                brep_Mesh.MergeCoplanarFaces(Tolerance.MacroDistance);

                foreach(BrepFace brepFace in brep_Mesh.Faces)
                    brepFaces.Add(brepFace);
            }

            foreach (BrepFace brepFace in brepFaces)
                foreach (BrepLoop brepLoop in brepFace.Loops)
                {
                    Spatial.IGeometry3D geometry3D = brepLoop.To3dCurve().ToSAM(simplify);
                    if(geometry3D is Spatial.Polycurve3D)
                    {
                        Spatial.Polycurve3D polycurve3D = (Spatial.Polycurve3D)geometry3D;
                        geometry3D = new Spatial.Polygon3D(polycurve3D.Explode().ConvertAll(x=> x.GetStart()));
                    }
                    result.Add(geometry3D);
                }
            return result;


            //List<Brep> breps_Mesh = new List<Brep>();

            //foreach (Brep brep_Temp in breps)
            //{
                
            //    Mesh all = new Mesh();
            //    all.Append(meshes);
            //    Brep all_brep = Brep.CreateFromMesh(all, true);
            //    all_brep.MergeCoplanarFaces(tol);

            //    all_brep.Faces[0].IsPlanar(); // true

            //    foreach (Mesh mesh in meshes)
            //    {
            //        Brep brep_Mesh = Brep.CreateFromMesh(mesh, true);
            //        breps_Mesh.Add(brep_Mesh);

            //        foreach (MeshFace meshFace in mesh.Faces)
            //        {

            //            List<Spatial.Point3D> point3Ds = new List<Spatial.Point3D>();
            //            //Split to triangle and Quad
            //            point3Ds.Add(new Spatial.Point3D(Convert.ToSAM(mesh.Vertices[meshFace.A])));
            //            point3Ds.Add(new Spatial.Point3D(Convert.ToSAM(mesh.Vertices[meshFace.B])));
            //            point3Ds.Add(new Spatial.Point3D(Convert.ToSAM(mesh.Vertices[meshFace.C])));

            //            if (meshFace.IsQuad)
            //                point3Ds.Add(new Spatial.Point3D(Convert.ToSAM(mesh.Vertices[meshFace.D])));

            //            result.Add(new Spatial.Polygon3D(point3Ds));
            //        }
            //    }

            //}

            ////RhinoDoc.ActiveDoc.ModelAbsoluteTolerance = Tolerance.MicroDistance;
            ////breps.MergeCoplanarFaces(Tolerance.MicroDistance);



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
