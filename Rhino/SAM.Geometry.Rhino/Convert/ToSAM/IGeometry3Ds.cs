using Rhino.Geometry;
using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Geometry.Rhino
{
    public static partial class Convert
    {
        //In case of Non planar brep we mesh it and then convert to brep then merge cooplanar  and then create ISAMGeometry
        public static List<ISAMGeometry3D> ToSAM(this Brep brep, bool simplify = true, bool orinetNormals = true)
        {
            //Has to be common for whole method
            double tolerance = Core.Tolerance.Distance;

            List<BrepFace> brepFaces = new List<BrepFace>();
            List<BrepFace> brepFaces_Planar = new List<BrepFace>();
            foreach (BrepFace brepFace in brep.Faces)
            {
                if (!brepFace.IsPlanar(tolerance))
                    brepFaces.Add(brepFace);
                else
                    brepFaces_Planar.Add(brepFace);
            }

            if (brepFaces != null && brepFaces.Count != 0)
            {
                foreach(BrepFace brepFace in brepFaces)
                {
                    Mesh mesh = Mesh.CreateFromSurface(brepFace.UnderlyingSurface(), ActiveSetting.GetMeshingParameters());
                    Brep brep_Mesh = Brep.CreateFromMesh(mesh, true);
                    if(brep_Mesh != null)
                    {
                        brep_Mesh.MergeCoplanarFaces(tolerance);

                        foreach (BrepFace brepFace_Mesh in brep_Mesh.Faces)
                            brepFaces_Planar.Add(brepFace_Mesh);
                    }
                }
                
                //brepFaces = new List<BrepFace>();
                
                //Mesh mesh = new Mesh();
                //foreach (Brep brep_Temp in breps)
                //{
                //    Mesh[] meshes = Mesh.CreateFromBrep(brep_Temp, ActiveSetting.GetMeshingParameters());
                //    mesh.Append(meshes);
                //}
                //Brep brep_Mesh = Brep.CreateFromMesh(mesh, true);
                //if(brep_Mesh != null)
                //{
                //    brep_Mesh.MergeCoplanarFaces(tolerance); //Does not work for all cases

                //    foreach (BrepFace brepFace in brep_Mesh.Faces)
                //        brepFaces.Add(brepFace);
                //}

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
            foreach (BrepFace brepFace in brepFaces_Planar)
            {
                Spatial.Plane plane = null;
                if (brepFace.TryGetPlane(out global::Rhino.Geometry.Plane plane_BrepFace))
                {
                    plane = plane_BrepFace.ToSAM();
                }
                
                List<IClosedPlanar3D> closedPlanar3Ds = new List<IClosedPlanar3D>();
                foreach (BrepLoop brepLoop in brepFace.Loops)
                {
                    ISAMGeometry3D geometry3D = brepLoop.To3dCurve().ToSAM(simplify);
                    if (geometry3D is Polycurve3D)
                    {
                        List<Point3D> point3Ds = ((Polycurve3D)geometry3D).Explode().ConvertAll(x => x.GetStart());
                        if(plane == null)
                        {
                            PlaneFitResult planeFitResult = global::Rhino.Geometry.Plane.FitPlaneToPoints(point3Ds.ConvertAll(x => x.ToRhino()), out global::Rhino.Geometry.Plane plane_Rhino);
                            if (planeFitResult != PlaneFitResult.Failure && plane_Rhino != null)
                            {
                                geometry3D = new Polygon3D(plane_Rhino.ToSAM(), point3Ds.ConvertAll(x => plane.Convert(plane.Project(x))));
                            }
                        }
                        else
                        {
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
            {
                Shell shell = new Shell(face3Ds);
                if(orinetNormals)
                {
                    shell.OrientNormals();
                }

                if(!shell.IsClosed(Core.Tolerance.MacroDistance))
                {
                    Brep brep_Shell = shell.ToRhino(Core.Tolerance.MacroDistance);
                    if(brep_Shell != null &&  brep_Shell.IsSolid)
                    {
                        shell = brep_Shell.ToSAM_Shell(simplify);
                    }

                }

                result.Add(shell);
            }
            else
            {
                result.AddRange(face3Ds);
            }

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