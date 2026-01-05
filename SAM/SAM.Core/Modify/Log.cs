// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System;

namespace SAM.Core
{
    public static partial class Modify
    {
        public static bool Log(this string path, string format, params object[] values)
        {
            if (format == null || string.IsNullOrWhiteSpace(path))
            {
                return false;
            }

            string directory = System.IO.Path.GetDirectoryName(path);
            if (string.IsNullOrWhiteSpace(directory))
            {
                return false;
            }

            if (!Create.Directory(directory))
            {
                return false;
            }

            LogRecord logRecord = new LogRecord(format, values);

            try
            {
                logRecord.Write(path);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
    }
}
