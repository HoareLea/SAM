// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace SAM.Analytical
{
    public static partial class Create
    {
        public static Zone Zone(System.Guid guid, string name, ZoneType zoneType)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            return Zone(guid, name, zoneType.Text());
        }

        public static Zone Zone(System.Guid guid, string name, string zoneCategory)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            Zone result = new Zone(guid, name);
            if (zoneCategory != null)
                result.SetValue(ZoneParameter.ZoneCategory, zoneCategory);

            return result;
        }
    }
}
