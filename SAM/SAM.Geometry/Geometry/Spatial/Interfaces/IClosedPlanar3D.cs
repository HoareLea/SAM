// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace SAM.Geometry.Spatial
{
    public interface IClosedPlanar3D : IClosed3D, IPlanar3D, IBoundable3D, IReversible
    {
        double GetArea();

        Point3D GetCentroid();
    }
}
