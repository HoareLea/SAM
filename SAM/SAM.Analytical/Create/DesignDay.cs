// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Weather;

namespace SAM.Analytical
{
    public static partial class Create
    {
        public static DesignDay DesignDay(string name, string description, short year, byte month, byte day, WeatherDay weatherDay)
        {
            if (year < 0)
            {
                return null;
            }

            if (month < 1 || month > 12)
            {
                return null;
            }

            DesignDay result = new DesignDay(name, description, year, month, day);

            if (weatherDay != null)
            {
                foreach (string key in weatherDay.Keys)
                {
                    result[key] = weatherDay[key];
                }
            }

            return result;
        }
    }
}
