// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System;
using System.IO;

namespace SAM.Core
{
    public static partial class Query
    {
        public static bool FileExists(this string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return false;

            try
            {
                File.Exists(filePath);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
    }
}
