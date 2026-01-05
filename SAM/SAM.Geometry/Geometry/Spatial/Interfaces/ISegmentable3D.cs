// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public interface ISegmentable3D : ICurvable3D
    {
        List<Segment3D> GetSegments();

        List<Point3D> GetPoints();

        bool On(Point3D point3D, double tolerance = Core.Tolerance.Distance);

        double GetLength();
    }
}
