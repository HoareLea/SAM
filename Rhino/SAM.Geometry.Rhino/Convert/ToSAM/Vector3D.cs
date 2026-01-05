// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace SAM.Geometry.Rhino
{
    public static partial class Convert
    {
        public static Spatial.Vector3D ToSAM(this global::Rhino.Geometry.Vector3d vector3d)
        {
            return new Spatial.Vector3D(vector3d.X, vector3d.Y, vector3d.Z);
        }
    }
}
