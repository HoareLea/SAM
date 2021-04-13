using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public static partial class Modify
    {
        public static bool SplitFace3Ds(this List<Shell> shells, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance)
        {
            if (shells == null || shells.Count < 2)
                return false;

            bool result = false;
            for(int i=0; i < shells.Count; i++)
            {
                Shell shell_1 = shells[i];

                for (int j = 0; j < shells.Count; j++)
                {
                    Shell shell_2 = shells[j];

                    if(shell_1 == shell_2)
                    {
                        continue;
                    }

                    if (shell_1.SplitFace3Ds(shell_2, tolerance_Angle, tolerance_Distance))
                        result = true;
                }
            }

            return result;
        }
    }
}