using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SAM.Geometry.Spatial
{
    public static partial class Modify
    {
        public static bool SplitFace3Ds(this List<Shell> shells, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance)
        {
            if (shells == null || shells.Count < 2)
                return false;

            List<Tuple<Shell, bool>> tuples = shells.ConvertAll(x => new Tuple<Shell, bool>(x, false));

            Parallel.For(0, shells.Count, (int i) => 
            //for(int i=0; i < shells.Count; i++)
            {
                Shell shell = tuples[i].Item1;
                if(shell != null)
                {
                    Shell shell_New = new Shell(tuples[i].Item1);

                    bool updated = false;
                    for (int j = 0; j < shells.Count; j++)
                    {
                        if (i == j)
                        {
                            continue;
                        }

                        if (shell_New.SplitFace3Ds(shells[j], tolerance_Angle, tolerance_Distance))
                        {
                            updated = true;
                        }
                    }

                    if(updated)
                    {
                        tuples[i] = new Tuple<Shell, bool>(shell_New, true);
                    }
                }

            });

            shells.Clear();
            shells.AddRange(tuples.ConvertAll(x =>x.Item1));

            return tuples.Find(x => x.Item2) != null;
        }

        public static bool SplitFace3Ds(this List<Shell> shells, IEnumerable<Face3D> face3Ds, double maxDistance = 0.1, double maxAngle = 0.0872664626, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance)
        {
            if (shells == null || shells.Count < 2)
                return false;

            List<Tuple<Shell, bool>> tuples = shells.ConvertAll(x => new Tuple<Shell, bool>(x, false));

            Parallel.For(0, shells.Count, (int i) =>
            //for(int i=0; i < shells.Count; i++)
            {
                Shell shell = tuples[i].Item1;
                if (shell != null)
                {
                    Shell shell_New = new Shell(tuples[i].Item1);

                    bool updated = SplitFace3Ds(shell_New, face3Ds, maxDistance, maxAngle, tolerance_Angle, tolerance_Distance);
                    if (updated)
                    {
                        tuples[i] = new Tuple<Shell, bool>(shell_New, true);
                    }
                }

            });

            shells.Clear();
            shells.AddRange(tuples.ConvertAll(x => x.Item1));

            return tuples.Find(x => x.Item2) != null;
        }

        public static bool SplitFace3Ds(this Shell shell, IEnumerable<Face3D> face3Ds, double maxDistance = 0.1, double maxAngle = 0.0872664626, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance)
        {
            if (shell == null || face3Ds == null)
            {
                return false;
            }

            BoundingBox3D boundingBox3D = shell.GetBoundingBox();
            if (boundingBox3D == null || !boundingBox3D.IsValid())
            {
                return false;
            }

            List<Tuple<BoundingBox3D, Face3D>> tuples = shell.Face3Ds?.ConvertAll(x => new Tuple<BoundingBox3D, Face3D>(x.GetBoundingBox(), x));
            tuples.RemoveAll(x => x.Item1 == null || !x.Item1.IsValid());

            List<Face3D> face3Ds_Split = new List<Face3D>();
            foreach (Face3D face3D in face3Ds)
            {
                BoundingBox3D boundingBox3D_Face3D = face3D?.GetBoundingBox();
                if (boundingBox3D_Face3D == null || !boundingBox3D_Face3D.IsValid())
                {
                    continue;
                }

                if (!boundingBox3D.InRange(boundingBox3D_Face3D, maxDistance))
                {
                    continue;
                }

                Vector3D vector3D = face3D.GetPlane()?.Normal;
                if (vector3D == null)
                {
                    continue;
                }

                foreach (Tuple<BoundingBox3D, Face3D> tuple in tuples)
                {
                    if (!boundingBox3D_Face3D.InRange(tuple.Item1, maxDistance))
                    {
                        continue;
                    }

                    Plane plane = tuple.Item2.GetPlane();
                    if (plane == null)
                    {
                        continue;
                    }

                    Vector3D vector3D_Face3D = plane.Normal;
                    if (vector3D_Face3D == null)
                    {
                        continue;
                    }

                    if (vector3D.SmallestAngle(vector3D_Face3D.GetNegated()) > maxAngle && vector3D.SmallestAngle(vector3D_Face3D) > maxAngle)
                    {
                        continue;
                    }

                    Face3D face3D_Project = plane.Project(face3D);
                    if (face3D_Project == null && !face3D_Project.IsValid())
                    {
                        continue;
                    }

                    List<Planar.Face2D> face2Ds_Intersection = Planar.Query.Intersection(plane.Convert(tuple.Item2), plane.Convert(face3D_Project), tolerance_Distance);
                    if (face2Ds_Intersection == null || face2Ds_Intersection.Count == 0)
                    {
                        continue;
                    }

                    face2Ds_Intersection.RemoveAll(x => x == null || x.GetArea() < tolerance_Distance);

                    if (face2Ds_Intersection.Count == 0)
                    {
                        continue;
                    }

                    face3Ds_Split.Add(face3D_Project);
                }
            }

            if (face3Ds_Split == null || face3Ds_Split.Count == 0)
            {
                return false;
            }

            bool split = false;
            foreach (Face3D face3D_Split in face3Ds_Split)
            {
                if (shell.SplitFace3Ds(face3D_Split, tolerance_Angle, tolerance_Distance))
                {
                    split = true;
                }
            }

            return split;
        }
    }
}