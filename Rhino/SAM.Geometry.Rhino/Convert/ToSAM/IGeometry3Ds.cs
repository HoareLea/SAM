using Rhino.Geometry;
using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Geometry.Rhino
{
    public static partial class Convert
    {
        //In case of Non planar brep we mesh it and then convert to brep then merge cooplanar  and then create ISAMGeometry
        public static List<ISAMGeometry3D> ToSAM(this Brep brep, bool simplify = true)
        {
            //Has to be common for whole method
            double tolerance = Core.Tolerance.Distance;

            List<Brep> breps = new List<Brep>();
            List<BrepFace> brepFaces = new List<BrepFace>();
            foreach (BrepFace brepFace in brep.Faces)
            {
                if (!brepFace.IsPlanar(tolerance))
                    breps.Add(brepFace.Brep);
                else
                    brepFaces.Add(brepFace);
            }

            if (breps != null && breps.Count > 0)
            {
                brepFaces = new List<BrepFace>();
                
                Mesh mesh = new Mesh();
                foreach (Brep brep_Temp in breps)
                {
                    Mesh[] meshes = Mesh.CreateFromBrep(brep_Temp, ActiveSetting.GetMeshingParameters());
                    mesh.Append(meshes);
                }
                Brep brep_Mesh = Brep.CreateFromMesh(mesh, true);
                if(brep_Mesh != null)
                {
                    brep_Mesh.MergeCoplanarFaces(tolerance); //Does not work for all cases

                    foreach (BrepFace brepFace in brep_Mesh.Faces)
                        brepFaces.Add(brepFace);
                }

                //foreach (Brep brep_Temp in breps)
                //{
                //    Mesh[] meshes = Mesh.CreateFromBrep(brep_Temp, AssemblyInfo.GetMeshingParameters());
                //    foreach(Mesh mesh in meshes)
                //    {
                //        Brep brep_Mesh = Brep.CreateFromMesh(mesh, true);

                //        foreach (BrepFace brepFace in brep_Mesh.Faces)
                //            brepFaces.Add(brepFace);
                //    }
                //}
            }

            List<Face3D> face3Ds = new List<Face3D>();
            foreach (BrepFace brepFace in brepFaces)
            {
                List<IClosedPlanar3D> closedPlanar3Ds = new List<IClosedPlanar3D>();
                foreach (BrepLoop brepLoop in brepFace.Loops)
                {
                    ISAMGeometry3D geometry3D = brepLoop.To3dCurve().ToSAM(simplify);
                    if (geometry3D is Polycurve3D)
                    {
                        List<Point3D> point3Ds = ((Polycurve3D)geometry3D).Explode().ConvertAll(x => x.GetStart());

                        PlaneFitResult planeFitResult = global::Rhino.Geometry.Plane.FitPlaneToPoints(point3Ds.ConvertAll(x => x.ToRhino()), out global::Rhino.Geometry.Plane plane_Rhino);
                        if(planeFitResult != PlaneFitResult.Failure && plane_Rhino != null)
                        {
                            Spatial.Plane plane = plane_Rhino.ToSAM();
                            geometry3D = new Polygon3D(plane, point3Ds.ConvertAll(x => plane.Convert(plane.Project(x))));
                        }
                    }
                    if (geometry3D is IClosedPlanar3D)
                        closedPlanar3Ds.Add((IClosedPlanar3D)geometry3D);
                }

                if (closedPlanar3Ds == null || closedPlanar3Ds.Count == 0)
                    continue;

                Face3D face3D = Face3D.Create(closedPlanar3Ds);
                if (face3D == null)
                    continue;

                face3Ds.Add(face3D);
            }

            if (face3Ds == null || face3Ds.Count == 0)
                return null;

            List<ISAMGeometry3D> result = new List<ISAMGeometry3D>();

            if(brep.IsSolid)
                result.Add(new Shell(face3Ds));
            else
                result.AddRange(face3Ds);

            return result;
        }

        public static List<ISAMGeometry3D> ToSAM(this global::Rhino.Geometry.Surface surface, bool simplify = true)
        {
            List<ISAMGeometry3D> result = new List<ISAMGeometry3D>();
            foreach (BrepLoop brepLoop in surface.ToBrep().Loops)
                result.Add(brepLoop.ToSAM(simplify));

            return result;
        }
    }
}