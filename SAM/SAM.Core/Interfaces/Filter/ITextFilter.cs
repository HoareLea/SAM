// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace SAM.Core
{
    public interface ITextFilter : IFilter
    {
        public bool CaseSensitive { get; set; }

        public TextComparisonType TextComparisonType { get; set; }

        public string Value { get; set; }

    }
}
