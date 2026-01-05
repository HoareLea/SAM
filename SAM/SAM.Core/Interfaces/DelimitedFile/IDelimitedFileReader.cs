// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.Collections.Generic;

namespace SAM.Core
{
    public interface IDelimitedFileReader
    {
        char Separator { get; }

        bool Read(DelimitedFileRow DelimitedFileRow);

        List<DelimitedFileRow> Read();
    }
}
