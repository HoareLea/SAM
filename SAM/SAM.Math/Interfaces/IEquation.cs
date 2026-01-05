// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core;
using System.Collections.Generic;

namespace SAM.Math
{
    public interface IEquation : IJSAMObject
    {
        double Evaluate(double value);

        List<double> Evaluate(IEnumerable<double> values);
    }
}
