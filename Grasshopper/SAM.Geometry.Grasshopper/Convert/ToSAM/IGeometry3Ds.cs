using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using Rhino.Geometry.Collections;
using SAM.Geometry.Spatial;
using System.Collections;
using System.Collections.Generic;

namespace SAM.Geometry.Grasshopper
{
    public static partial class Convert
    {
        public static List<ISAMGeometry3D> ToSAM(this IGH_GeometricGoo geometricGoo, bool simplify = true)
        {
            if (geometricGoo is GH_Curve)
                return new List<ISAMGeometry3D>() { ((GH_Curve)geometricGoo).ToSAM(simplify) };

            if (geometricGoo is GH_Surface)
                return ((GH_Surface)geometricGoo).ToSAM(simplify);

            //if (geometricGoo is GH_Point)
            //    return new List<Spatial.ISAMGeometry3D>() { ((GH_Point)geometricGoo).ToSAM() };

            if (geometricGoo is GH_Brep)
                return ((GH_Brep)geometricGoo).ToSAM(simplify);

            if (geometricGoo is GH_Mesh)
                return ((GH_Mesh)geometricGoo).ToSAM();

            object @object = Convert.ToSAM(geometricGoo as dynamic);
            if (@object == null)
                return null;

            if (@object is ISAMGeometry3D)
                return new List<ISAMGeometry3D>() { (ISAMGeometry3D)@object };

            if (@object is IEnumerable)
            {
                List<ISAMGeometry3D> result = new List<ISAMGeometry3D>();
                foreach (object object_Temp in (IEnumerable)@object)
                    if (object_Temp is ISAMGeometry3D)
                        result.Add((ISAMGeometry3D)object_Temp);

                return result;
            }

            return null;
        }

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

                        PlaneFitResult planeFitResult = Rhino.Geometry.Plane.FitPlaneToPoints(point3Ds.ConvertAll(x => x.ToRhino()), out Rhino.Geometry.Plane plane_Rhino);
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

        public static List<ISAMGeometry3D> ToSAM(this GH_Surface surface, bool simplify = true)
        {
            return ToSAM(surface.Value, simplify);
        }

        public static List<ISAMGeometry3D> ToSAM(this Rhino.Geometry.Surface surface, bool simplify = true)
        {
            List<ISAMGeometry3D> result = new List<ISAMGeometry3D>();
            foreach (BrepLoop brepLoop in surface.ToBrep().Loops)
                result.Add(brepLoop.ToSAM(simplify));

            return result;
        }

        public static List<ISAMGeometry3D> ToSAM(this GH_Mesh mesh)
        {
            if (mesh == null || mesh.Value == null)
                return null;

            MeshVertexList meshVertexList = mesh.Value.Vertices;
            if (meshVertexList == null)
                return null;

            IEnumerable<MeshFace> meshFaces = mesh.Value.Faces;
            if (meshFaces == null)
                return null;

            List<ISAMGeometry3D> result = new List<ISAMGeometry3D>();
            foreach(MeshFace meshFace in meshFaces)
            {
                if (meshFace.IsQuad)
                    result.Add(new Polygon3D(new Point3D[] { meshVertexList[meshFace.A].ToSAM(), meshVertexList[meshFace.B].ToSAM(), meshVertexList[meshFace.C].ToSAM(), meshVertexList[meshFace.D].ToSAM() }));
                else if (meshFace.IsTriangle)
                    result.Add(new Triangle3D(meshVertexList[meshFace.A].ToSAM(), meshVertexList[meshFace.B].ToSAM(), meshVertexList[meshFace.C].ToSAM()));
            }

            return result;
        }
    }
}