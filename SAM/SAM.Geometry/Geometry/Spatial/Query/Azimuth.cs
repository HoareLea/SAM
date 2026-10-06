// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        /// <summary>
        /// Azimuth of the given geometry to reference direction expressed in degrees
        /// </summary>
        /// <param name="closedPlanar3D">Closed Planar 3D Geometry</param>
        /// <param name="referenceDirection">Reference Direction</param>
        /// <returns>Azmiuth in degrees</returns>
        public static double Azimuth(this IClosedPlanar3D closedPlanar3D, Vector3D referenceDirection)
        {
            if (closedPlanar3D == null || referenceDirection == null)
                return double.NaN;

            return Azimuth(closedPlanar3D.GetPlane(), referenceDirection);
        }

        public static double Azimuth(this Plane plane, Vector3D referenceDirection)
        {
            if (plane == null || referenceDirection == null)
                return double.NaN;

            Vector3D normal = plane.InternalNormal;
            if (normal == null)
                return double.NaN;

            if (normal.Z == 1)
                return 0;

            if (normal.Z == -1)
                return 180;

            Vector3D vector3D_Project_Normal = new Vector3D(normal.X, normal.Y, 0);
            Vector3D vector3D_Project_ReferenceDirection = new Vector3D(referenceDirection.X, referenceDirection.Y, 0);

            double azimuth = SignedAngle(vector3D_Project_Normal, vector3D_Project_ReferenceDirection, Vector3D.WorldZ) * (180 / System.Math.PI);
            if (azimuth < 0)
                azimuth = 360 + azimuth;

            return azimuth;
        }
    }
}
