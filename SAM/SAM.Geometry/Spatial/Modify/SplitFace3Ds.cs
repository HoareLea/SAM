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

            //Parallel.For(0, shells.Count, (int i) => 
            for(int i=0; i < shells.Count; i++)
            {
                Shell shell = tuples[i].Item1;
                if(shell != null)
                {
                    Shell shell_New = new Shell(tuples[i].Item1);

                    bool updated = false;
                    for (int j = 0; j < shells.Count; j++)
                    {
                        if (i != j)
                        {
                            if (shell_New.SplitFace3Ds(shells[j], tolerance_Angle, tolerance_Distance))
                            {
                                updated = true;
                            }
                        }
                    }

                    if(updated)
                    {
                        tuples[i] = new Tuple<Shell, bool>(shell_New, true);
                    }
                }

            }//);

            shells.Clear();
            shells.AddRange(tuples.ConvertAll(x =>x.Item1));

            return tuples.Find(x => x.Item2) != null;
        }
    }
}