using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SAM.Geometry.Spatial
{
    public static partial class Modify
    {
        public static void Split(this List<Shell> shells, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance)
        {
            if (shells == null || shells.Count == 0)
                return;

            List<Shell> shells_Result = Enumerable.Repeat<Shell>(null, shells.Count).ToList();

            Parallel.For(0, shells.Count, (int i) =>
            {
                Shell shell = shells[i];
                if(shell == null)
                {
                    return;
                }

                BoundingBox3D boundingBox3D = shell.GetBoundingBox();

                List<Shell> shells_InRange = shells.FindAll(x => x != shell && boundingBox3D.InRange(x.GetBoundingBox()));
                if(shells_InRange == null || shells_InRange.Count == 0)
                {
                    shells_Result[i] = shell;
                    return;
                }

                shells_Result[i] = shell;

                shell = new Shell(shell);

                bool split = false;
                foreach (Shell shell_InRange in shells_InRange)
                {
                    if (shell.Split(shell_InRange, tolerance_Angle, tolerance_Distance))
                        split = true;
                }

                if(split)
                    shells_Result[i] = shell;

            });

            shells.Clear();
            shells.AddRange(shells_Result);
        }
    }
}