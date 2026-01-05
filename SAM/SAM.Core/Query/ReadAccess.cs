// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace SAM.Core
{
    public static partial class Query
    {
        public static bool ReadAccess(this AccessType accessType)
        {
            switch (accessType)
            {
                case AccessType.Read:
                case AccessType.ReadWrite:
                    return true;
                default:
                    return false;
            }
        }
    }
}
