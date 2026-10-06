// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical;
using SAM.Core;

namespace SAM.Tests.Helpers
{
    /// <summary>
    /// Loads the shipped TM59 resource files once per test run, for the TM59 matcher/mapping tests
    /// that exercise the real keyword lists and library rather than a hand-duplicated copy.
    /// </summary>
    public static class TM59TestData
    {
        private static TextMap? textMap;
        private static InternalConditionLibrary? internalConditionLibrary;
        private static ProfileLibrary? profileLibrary;

        public static TextMap TextMap => textMap ??= SAM.Core.Create.IJSAMObject<TextMap>(Fixtures.ReadAllText("SAM_InternalConditionTextMap_TM59.JSON"));

        public static InternalConditionLibrary InternalConditionLibrary => internalConditionLibrary ??= SAM.Core.Create.IJSAMObject<InternalConditionLibrary>(Fixtures.ReadAllText("SAM_InternalConditionLibrary_TM59.JSON"));

        public static ProfileLibrary ProfileLibrary => profileLibrary ??= SAM.Core.Create.IJSAMObject<ProfileLibrary>(Fixtures.ReadAllText("SAM_ProfileLibrary_TM59.JSON"));

        public static TM59InternalConditionResolver NewResolver()
        {
            return new TM59InternalConditionResolver(TextMap, InternalConditionLibrary);
        }
    }
}
