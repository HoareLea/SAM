using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SAM.Geometry.Planar
{
    public static partial class Create
    {
        /// <summary>
        /// Calculates unit vector for given angle [rad]
        /// </summary>
        /// <param name="angle">angle to X axis counted counterclockwise [rad]</param>
        /// <returns>Unit Vector2D</returns>
        public static Vector2D Vector2D(double angle)
        {
            if(double.IsNaN(angle) || double.IsInfinity(angle))
            {
                return null;
            }

            if(angle == 0)
            {
                return Planar.Vector2D.WorldX;
            }

            double xAngle = angle;
            double yAngle = (System.Math.PI / 2) - angle;

            return new Vector2D(System.Math.Cos(xAngle), System.Math.Cos(yAngle));
        }
    }
}