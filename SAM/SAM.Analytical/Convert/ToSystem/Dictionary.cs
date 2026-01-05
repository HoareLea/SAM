// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical.Classes;
using SAM.Core;
using System;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Convert
    {
        public static Dictionary<Range<double>, Tuple<double, ApertureConstruction>> ToDictionary(this ApertureToPanelRatios apertureToPanelRatios)
        {
            if (apertureToPanelRatios is null)
            {
                return null;
            }

            Dictionary<Range<double>, Tuple<double, ApertureConstruction>> result = [];

            int count = apertureToPanelRatios.Count;

            if (count == 0)
            {
                return result;

            }

            for (int i = 0; i < count; i++)
            {
                ApertureToPanelRatio apertureToPanelRatio = apertureToPanelRatios[i];
                if (apertureToPanelRatio?.AzimuthRange is not Range<double> azimuthRange)
                {
                    continue;
                }

                result[azimuthRange] = Tuple.Create(apertureToPanelRatio.Ratio, apertureToPanelRatio.ApertureConstruction);
            }

            return result;
        }
    }
}
