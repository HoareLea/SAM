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

        public static List<Spatial.ISAMGeometry3D> ToSAM(this IGH_GeometricGoo geometricGoo, bool simplify = true)
        {
            if (geometricGoo is GH_Curve)
            {
                return new List<Spatial.ISAMGeometry3D>() { ((GH_Curve)geometricGoo).ToSAM(simplify) };
            }
                

            if (geometricGoo is GH_Surface)
                return ((GH_Surface)geometricGoo).ToSAM(simplify);

            if (geometricGoo is GH_Point)
                return new List<Spatial.ISAMGeometry3D>() { ((GH_Point)geometricGoo).ToSAM() };

            if(geometricGoo is GH_Brep)
                return ((GH_Brep)geometricGoo).ToSAM(simplify);

            return (geometricGoo as dynamic).ToSAM();
        }

        public static List<Spatial.ISAMGeometry3D> ToSAM(this Brep brep, bool simplify = true)
        {
            List<Brep> breps = new List<Brep>();
            List<BrepFace> brepFaces = new List<BrepFace>();
            foreach (BrepFace brepFace in brep.Faces)
            {
                if (!brepFace.IsPlanar(Core.Tolerance.MicroDistance))
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
                brep_Mesh.MergeCoplanarFaces(Core.Tolerance.MacroDistance);

                foreach(BrepFace brepFace in brep_Mesh.Faces)
                    brepFaces.Add(brepFace);
            }

            List<Spatial.ISAMGeometry3D> result = new List<Spatial.ISAMGeometry3D>();

            foreach (BrepFace brepFace in brepFaces)
            {
                List<Spatial.IClosedPlanar3D> closedPlanar3Ds = new List<Spatial.IClosedPlanar3D>();
                foreach (BrepLoop brepLoop in brepFace.Loops)
                {
                    Spatial.ISAMGeometry3D geometry3D = brepLoop.To3dCurve().ToSAM(simplify);
                    if (geometry3D is Spatial.Polycurve3D)
                    {
                        Spatial.Polycurve3D polycurve3D = (Spatial.Polycurve3D)geometry3D;
                        geometry3D = new Spatial.Polygon3D(polycurve3D.Explode().ConvertAll(x => x.GetStart()));
                    }
                    if (geometry3D is Spatial.IClosedPlanar3D)
                        closedPlanar3Ds.Add((Spatial.IClosedPlanar3D)geometry3D);
                }
                if (closedPlanar3Ds != null && closedPlanar3Ds.Count > 0)
                    result.Add(Spatial.Face3D.Create(closedPlanar3Ds));
            }

            return result;
        }

        public static List<Spatial.ISAMGeometry3D> ToSAM(this GH_Surface surface, bool simplify = true)
        {
            return ToSAM(surface.Value);
        }

        public static List<Spatial.ISAMGeometry3D> ToSAM(this Surface surface, bool simplify = true)
        {
            List<Spatial.ISAMGeometry3D> result = new List<Spatial.ISAMGeometry3D>();
            foreach (BrepLoop brepLoop in surface.ToBrep().Loops)
                result.Add(brepLoop.ToSAM(simplify));

            return result;
        }

    }
}
