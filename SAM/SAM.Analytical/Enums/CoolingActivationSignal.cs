// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.ComponentModel;

namespace SAM.Analytical.Enums
{
    /// <summary>
    /// Which temperature a ventilation unit's cooling is switched on - the signal its manufacturer's control
    /// compares with the cooling activation temperature.
    /// <para>
    /// <b>Two physical sensors can share one symbol.</b> A manufacturer's simplified guidance may write one
    /// "internal temperature" for both the unit's integral extract-inlet sensor (which decides bypass or
    /// recovery) and a wall-mounted room cooling-stat (which decides cooling). In a single well-mixed dwelling
    /// zone they coincide; in a dwelling where a wet room's extract is much warmer than the habitable rooms they
    /// do not, and the choice decides when the unit cools. Bypass and recovery always stay on the extract.
    /// </para>
    /// </summary>
    [Description("Cooling Activation Signal")]
    public enum CoolingActivationSignal
    {
        /// <summary>Nothing recognisable was stated. A strategy in this state refuses.</summary>
        [Description("Undefined")] Undefined,

        /// <summary>The unit's extract / return air temperature - the same signal bypass and recovery use.</summary>
        [Description("Extract temperature")] ExtractTemperature,

        /// <summary>A habitable room's air temperature - a wall-mounted room cooling-stat.</summary>
        [Description("Room temperature")] RoomTemperature,
    }
}
