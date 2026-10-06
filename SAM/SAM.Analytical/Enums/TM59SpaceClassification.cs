// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.ComponentModel;

namespace SAM.Analytical
{
    /// <summary>
    /// TM59 Space Classification used by TM59InternalConditionResolver
    /// </summary>
    public enum TM59SpaceClassification
    {
        [Description("Undefined")] Undefined,
        [Description("NonHabitable")] NonHabitable,
        [Description("Bedroom")] Bedroom,
        [Description("LivingRoom")] LivingRoom,
        [Description("Kitchen")] Kitchen,
        [Description("LivingRoomKitchen")] LivingRoomKitchen,
        [Description("Studio")] Studio
    }
}
