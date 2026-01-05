// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Newtonsoft.Json.Linq;
using System.Drawing;

namespace SAM.Geometry.Object
{
    public class CurveAppearance : PointAppearance
    {
        public CurveAppearance(Color color, double thickness)
            : base(color, thickness)
        {
        }

        public CurveAppearance(JObject jObject)
            : base(jObject)
        {

        }

        public CurveAppearance(CurveAppearance curveAppearance)
            : base(curveAppearance)
        {

        }
    }
}
