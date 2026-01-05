// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace SAM.Core
{
    public interface IIndexedModifier : IModifier
    {
        bool ContainsIndex(int index);

        double GetCalculatedValue(int index, double value);
    }
}
