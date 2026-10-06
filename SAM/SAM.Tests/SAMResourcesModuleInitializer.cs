// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical;
using SAM.Core;
using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace SAM.Tests
{
    /// <summary>
    /// Seeds <see cref="SAM.Analytical.ActiveSetting"/> from <b>this repository's own</b> resource files before
    /// any test runs, so the suite does not depend on a SAM install existing on the machine.
    /// <para>
    /// <b>Why this exists.</b> Several libraries reach the tests through <c>ActiveSetting</c>, which loads a
    /// persisted <c>settings</c> file if one exists and otherwise calls <c>GetDefault()</c> - and
    /// <c>GetDefault()</c> reads its resource files through <c>Core.Query.ResourcesDirectory</c>, which resolves
    /// a directory from <c>Assembly.CodeBase</c>: obsolete since .NET 5 and not usable under the
    /// <c>dotnet test</c> host. So on a machine with a real SAM install every library is present, and on a clean
    /// CI runner every one of them is <b>null</b>.
    /// </para>
    /// <para>
    /// That difference hid two separate defects until PR #73's CI ran the suite on a clean runner: five TM59 test
    /// classes silently produced no assessment (a null <c>TextMap</c> makes
    /// <c>TMOverheatingCalculator.Calculate_TM59</c> refuse), and
    /// <c>TMOverheatingCalculator.SystemTypeName</c> threw <c>NullReferenceException</c> on a null
    /// <c>SystemTypeLibrary</c>. The second was a genuine production bug and is fixed in production code; this
    /// type exists so the <b>tests</b> stop depending on ambient machine state, which is what let both hide.
    /// </para>
    /// <para>
    /// <b>Seeded unconditionally, from the repo rather than from the install.</b> Overwriting whatever a
    /// developer happens to have installed is deliberate: it is the only way the same test run means the same
    /// thing on a laptop and on CI. <c>AppContext.BaseDirectory</c> is used rather than
    /// <c>Assembly.CodeBase</c> - it is the one path .NET Core guarantees is populated and correct under every
    /// hosting scenario, which is precisely the property the code being routed around here lacks.
    /// </para>
    /// <para>
    /// <b>If a new test needs another <c>ActiveSetting</c> default, add it here</b> and copy the matching
    /// resource file in <c>SAM.Tests.csproj</c>. Do not assume a clean runner behaves like a developer machine.
    /// </para>
    /// </summary>
    internal static class SAMResourcesModuleInitializer
    {
        [ModuleInitializer]
        internal static void Initialize()
        {
            Setting setting = Analytical.ActiveSetting.Setting;
            if (setting == null)
            {
                return;
            }

            //The text maps and the system type library first. PartFData is loaded after the SpaceUse text map
            //because it resolves its space-use vocabulary against it - the same dependency order
            //ActiveSetting.GetDefault() documents and observes.
            Seed<TextMap>(setting, AnalyticalSettingParameter.InternalConditionTextMap_TM59, "SAM_InternalConditionTextMap_TM59.JSON");
            Seed<TextMap>(setting, AnalyticalSettingParameter.InternalConditionTextMap, "SAM_InternalConditionTextMap.JSON");
            Seed<TextMap>(setting, AnalyticalSettingParameter.SpaceUseTextMap, "SAM_SpaceUseTextMap.JSON");
            Seed<SystemTypeLibrary>(setting, AnalyticalSettingParameter.DefaultSystemTypeLibrary, "SAM_SystemTypeLibrary.JSON");
            Seed<InternalConditionLibrary>(setting, AnalyticalSettingParameter.DefaultInternalConditionLibrary_TM59, "SAM_InternalConditionLibrary_TM59.JSON");
            Seed<ProfileLibrary>(setting, AnalyticalSettingParameter.DefaultProfileLibrary_TM59, "SAM_ProfileLibrary_TM59.JSON");
            Seed<ApertureConstructionLibrary>(setting, AnalyticalSettingParameter.DefaultApertureConstructionLibrary, "SAM_ApertureConstructionLibrary.JSON");

            //Not a plain IJSAMObject deserialisation: PartFData is built from its path by a factory, so it gets
            //its own step rather than going through Seed.
            string path = Path("SAM_PartFSpaceRulesUKDwellingsMVHR.json");
            if (path != null)
            {
                PartFData partFData = Analytical.Create.PartFData(path);
                if (partFData != null)
                {
                    setting.SetValue(AnalyticalSettingParameter.PartFData, partFData);
                }
            }
        }

        /// <summary>
        /// Reads one resource into the setting, leaving whatever was there if the file is missing or unreadable.
        /// <para>
        /// Deliberately silent on a missing file: a test that does not need the value must not fail here, and a
        /// test that does will fail on its own clear assertion rather than on an exception from an initializer.
        /// </para>
        /// </summary>
        private static void Seed<T>(Setting setting, AnalyticalSettingParameter analyticalSettingParameter, string fileName) where T : IJSAMObject
        {
            string path = Path(fileName);
            if (path == null)
            {
                return;
            }

            T result = Core.Create.IJSAMObject<T>(File.ReadAllText(path));
            if (result != null)
            {
                setting.SetValue(analyticalSettingParameter, result);
            }
        }

        /// <summary>The copied resource's path in the test output, or null where it was not copied.</summary>
        private static string Path(string fileName)
        {
            string result = System.IO.Path.Combine(AppContext.BaseDirectory, "Resources", fileName);

            return File.Exists(result) ? result : null;
        }
    }
}
