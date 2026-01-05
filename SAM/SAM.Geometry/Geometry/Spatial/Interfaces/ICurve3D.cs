// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace SAM.Geometry.Spatial
{
    public interface ICurve3D : IBoundable3D
    {
        Point3D GetStart();

        Point3D GetEnd();

        double GetLength();

        void Reverse();
    }
}
