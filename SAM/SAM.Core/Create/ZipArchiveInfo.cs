// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.IO.Compression;

namespace SAM.Core
{
    public static partial class Create
    {
        public static ZipArchiveInfo ZipArchiveInfo(this ZipArchive zipArchive)
        {
            if (zipArchive == null)
                return null;

            ZipArchiveEntry zipArchiveEntry = zipArchive.GetEntry(Core.ZipArchiveInfo.EntryName);
            if (zipArchiveEntry == null)
                return null;

            return IJSAMObject<ZipArchiveInfo>(zipArchiveEntry);
        }
    }
}
