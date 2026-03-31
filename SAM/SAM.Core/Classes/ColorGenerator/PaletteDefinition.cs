// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.Collections.Generic;

namespace SAM.Core
{
    public class PaletteDefinition
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public PaletteType PaletteType { get; set; }

        public IReadOnlyList<System.Drawing.Color> Colors { get; set; }

        /// <summary>
        /// Optional: Stable by name
        /// </summary>
        public bool StableByName { get; set; } = true;

        /// <summary>
        /// Optional: midpoint for diverging
        /// </summary>
        public double Midpoint { get; set; } = 0.5;

    }
}
