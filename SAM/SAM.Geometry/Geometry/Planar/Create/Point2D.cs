// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace SAM.Geometry.Planar
{
    public static partial class Create
    {
        public static Point2D Point2D(this Math.Matrix matrix)
        {
            if (matrix == null)
            {
                return null;
            }

            if (matrix.RowCount() < 2 || matrix.ColumnCount() < 1)
            {
                return null;
            }

            return new Point2D(matrix[0, 0], matrix[1, 0]);
        }
    }
}
