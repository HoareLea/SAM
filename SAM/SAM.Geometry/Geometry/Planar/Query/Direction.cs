// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static Vector2D Direction(this Alignment alignment)
        {
            if (alignment == Alignment.Undefined)
                return null;

            switch (alignment)
            {
                case Alignment.Vertical:
                    return new Vector2D(1, 0);

                case Alignment.Horizontal:
                    return new Vector2D(0, 1);
            }

            return null;
        }
    }
}
