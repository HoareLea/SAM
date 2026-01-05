// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace SAM.Analytical
{
    public static partial class Query
    {
        /// <summary>
        /// The Radiative Coefficient for a black-body surface according to BS EN ISO 6946:2017 [W/m2K]
        /// </summary>
        /// <param name="meanTemperature">The mean thermodynamic temperature of the surface and of its surroundings [K]</param>
        /// <returns>Black-Body Surface Radiative Coefficient [W/m2K]</returns>
        public static double BlackBodySurfaceRadiativeCoefficient(double meanTemperature = 283.15)
        {
            return 4 * 5.67e-8 * System.Math.Pow(meanTemperature, 3);
        }
    }
}
