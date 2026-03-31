// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Geometry.Rhino;

namespace SAM.Analytical.Rhino
{
    public static partial class Convert
    {
        public static global::Rhino.Geometry.Point ToRhino(this Space space)
        {
            global::Rhino.Geometry.Point result = space.Location.ToRhino_Point();

            string json = Core.Convert.ToString(space);
            if (!string.IsNullOrWhiteSpace(json))
            {
                result.SetUserString(Core.Rhino.Names.UserString, json);
            }

            return result;
        }
    }
}
