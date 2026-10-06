// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using NUnit.Framework;

namespace SAM.Core.Grasshopper.Tests
{
    /// <summary>
    /// Runs once before any test in this assembly. Hosts a real, headless Rhino 8 +
    /// Grasshopper runtime in-process (McNeel's Rhino.Testing / RhinoCore, configured by
    /// Rhino.Testing.Configs.xml), so <see cref="TestDocument.TryCreate"/> gets a genuine
    /// GH_Document instead of hanging on Rhino's native init.
    /// </summary>
    [SetUpFixture]
    public sealed class GrasshopperTestSetup : Rhino.Testing.Fixtures.RhinoSetupFixture
    {
    }
}
