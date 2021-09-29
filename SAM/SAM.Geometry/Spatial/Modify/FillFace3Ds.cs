using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SAM.Geometry.Spatial
{
    public static partial class Modify
    {
        public static bool FillFace3Ds(this List<Shell> shells, IEnumerable<Face3D> face3Ds, double offset = 0.1, double maxDistance = 0.1, double maxAngle = 0.0872664626, double tolerance_Area = Core.Tolerance.MacroDistance, double tolerance_Distance = Core.Tolerance.Distance)
        {
            if (shells == null || shells.Count == 0)
                return false;

            List<Tuple<Shell, bool>> tuples = shells.ConvertAll(x => new Tuple<Shell, bool>(x, false));

            Parallel.For(0, shells.Count, (int i) =>
            //for(int i=0; i < shells.Count; i++)
            {
                Shell shell = tuples[i].Item1;
                if (shell != null)
                {
                    Shell shell_New = new Shell(tuples[i].Item1);

                    bool updated = shell_New.FillFace3Ds(face3Ds, offset, maxDistance, maxAngle, tolerance_Area, tolerance_Distance);
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
    }
}