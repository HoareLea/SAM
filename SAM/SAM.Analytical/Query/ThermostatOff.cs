// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace SAM.Analytical
{
    public static partial class Query
    {
        /// <summary>
        /// The heating thermostat value [°C] that means "no heating": the "No Heating" profile of the SAM profile
        /// libraries (SAM_ProfileLibrary.JSON, SAM_ProfileLibrary_TM59.JSON) holds it all year, and SAM_Tas sizing
        /// treats a lower limit at or below it as no heating. It is an off switch, not a set point.
        /// </summary>
        public const double NoHeatingSetPoint = -50;

        /// <summary>
        /// The cooling thermostat value [°C] that means "no cooling": the "No Cooling" profile of the SAM profile
        /// libraries holds it all year. It is an off switch, not a set point.
        /// </summary>
        public const double NoCoolingSetPoint = 150;

        /// <summary>
        /// True when a heating set point [°C] is the "no heating" value (<see cref="NoHeatingSetPoint"/> or below).
        /// </summary>
        public static bool IsHeatingOff(double heatingSetPoint)
        {
            return heatingSetPoint <= NoHeatingSetPoint;
        }

        /// <summary>
        /// True when a cooling set point [°C] is the "no cooling" value (<see cref="NoCoolingSetPoint"/> or above).
        /// </summary>
        public static bool IsCoolingOff(double coolingSetPoint)
        {
            return coolingSetPoint >= NoCoolingSetPoint;
        }
    }
}
