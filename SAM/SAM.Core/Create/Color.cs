// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace SAM.Core
{
    public static partial class Create
    {
        public static Color Color()
        {
            Random random = new Random();

            return System.Drawing.Color.FromArgb(random.Next(0, 256), random.Next(0, 256), random.Next(0, 256));
        }

        /// <summary>
        /// Generate random color
        /// </summary>
        /// <param name="excludedColors">Colors to be excluded</param>
        /// <param name="count">number of attempts to generate new color</param>
        /// <returns>Random Color</returns>
        public static Color Color(IEnumerable<Color> excludedColors, int count = 10000)
        {
            Color result = Color();

            if (excludedColors == null || excludedColors.Count() == 0)
            {
                return result;
            }

            List<Color> excludedColors_Temp = new List<Color>(excludedColors);

            for (int i = 0; i < count; i++)
            {
                if (excludedColors_Temp.Find(x => Query.Equals(x, result)) == null)
                {
                    return result;
                }

                result = Color();
            }

            return result;
        }

        public static Color Color(string text)
        {
            int hash = text.GetHashCode();

            byte r = (byte)((hash & 0xFF0000) >> 16);
            byte g = (byte)((hash & 0x00FF00) >> 8);
            byte b = (byte)(hash & 0x0000FF);

            return System.Drawing.Color.FromArgb(255, r, g, b);
        }
    }
}
