// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using GH_IO.Serialization;
using Grasshopper.Kernel;
using System;
using NUnit.Framework;

namespace SAM.Core.Grasshopper.Tests
{
    [TestFixture]
    public class SAMCoreUpdateContractTests
    {
        [Test]
        public void SAMCoreUpdate_ComponentGuid_Unchanged()
        {
            SAMCoreUpdate component = new SAMCoreUpdate();

            Assert.Equal(new Guid("a89bfee3-3a3c-4d29-9c7a-64073724eddc"), component.ComponentGuid);
        }

        [Test]
        public void SAMCoreUpdate_InputParams_OrderPreserved()
        {
            SAMCoreUpdate component = new SAMCoreUpdate();

            Assert.Equal(3, component.Params.Input.Count);
            Assert.Equal("_components", component.Params.Input[0].Name);
            Assert.Equal("_dryRun", component.Params.Input[1].Name);
            Assert.Equal("_run", component.Params.Input[2].Name);
        }

        [Test]
        public void SAMCoreUpdate_OutputParams_NewOutputAppendedLast()
        {
            SAMCoreUpdate component = new SAMCoreUpdate();

            Assert.Equal(3, component.Params.Output.Count);
            Assert.Equal("log", component.Params.Output[0].Name);
            Assert.Equal("succeeded", component.Params.Output[1].Name);
            Assert.Equal("manualReconnection", component.Params.Output[2].Name);
            Assert.Equal(GH_ParamAccess.list, component.Params.Output[2].Access);
        }

        [Test]
        public void SAMCoreUpdate_SerializationRoundTrip_ParamIndicesPreserved()
        {
            SAMCoreUpdate component = new SAMCoreUpdate();
            component.CreateAttributes();

            GH_LooseChunk chunk = new GH_LooseChunk("component");
            bool written = component.Write(chunk);
            Assert.True(written);

            SAMCoreUpdate component_Read = new SAMCoreUpdate();
            component_Read.CreateAttributes();
            bool read = component_Read.Read(chunk);
            Assert.True(read);

            Assert.Equal(3, component_Read.Params.Input.Count);
            Assert.Equal("_components", component_Read.Params.Input[0].Name);
            Assert.Equal("_dryRun", component_Read.Params.Input[1].Name);
            Assert.Equal("_run", component_Read.Params.Input[2].Name);
            Assert.Equal(3, component_Read.Params.Output.Count);
            Assert.Equal("log", component_Read.Params.Output[0].Name);
            Assert.Equal("succeeded", component_Read.Params.Output[1].Name);
            Assert.Equal("manualReconnection", component_Read.Params.Output[2].Name);
        }

        [Test]
        public void SAMCoreUpdate_LatestComponentVersion_Bumped()
        {
            SAMCoreUpdate component = new SAMCoreUpdate();

            Assert.Equal("1.1.0", component.LatestComponentVersion);
            Assert.False(component.Obsolete);
        }
    }
}
