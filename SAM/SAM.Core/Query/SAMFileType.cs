// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace SAM.Core
{
    public static partial class Query
    {
        public static SAMFileType SAMFileType(this string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return Core.SAMFileType.Undefined;

            string extension = System.IO.Path.GetExtension(path);
            if (extension == null)
                return Core.SAMFileType.Undefined;

            if (extension.StartsWith("."))
                extension = extension.Substring(1);

            extension = extension.ToLower();

            switch (extension)
            {
                case "sam":
                    return Core.SAMFileType.SAM;
                case "json":
                    return Core.SAMFileType.Json;
            }

            return Core.SAMFileType.Undefined;
        }
    }
}
