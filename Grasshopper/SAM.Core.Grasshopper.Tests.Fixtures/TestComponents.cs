// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using System;

namespace SAM.Core.Grasshopper.Tests
{
    public abstract class TestComponentBase : GH_SAMComponent
    {
        protected TestComponentBase(string name, string nickname, string description, string category, string subCategory)
            : base(name, nickname, description, category, subCategory)
        {
        }

        public void SetComponentVersion(string value)
        {
            SetValue("SAM_ComponentVersion", value);
        }
    }

    public class TestUpdatableComponent : TestComponentBase
    {
        public override Guid ComponentGuid => new Guid("6f8a3f5e-9c1d-4b7a-8a2e-1d5c9b0e4a21");

        public override string LatestComponentVersion => "1.0.0";

        public TestUpdatableComponent()
            : base("TestUpdatable", "TestUpdatable", "Test component for update tests", "SAM", "Test")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager manager)
        {
            manager.AddParameter(new Param_String { Name = "a", NickName = "a", Description = "a", Access = GH_ParamAccess.item, Optional = true });
            manager.AddParameter(new Param_String { Name = "b", NickName = "b", Description = "b", Access = GH_ParamAccess.item, Optional = true });
        }

        protected override void RegisterOutputParams(GH_OutputParamManager manager)
        {
            manager.AddParameter(new Param_String { Name = "x", NickName = "x", Description = "x", Access = GH_ParamAccess.item });
            manager.AddParameter(new Param_String { Name = "y", NickName = "y", Description = "y", Access = GH_ParamAccess.item });
        }

        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
        }
    }

    /// <summary>A fixed-output component whose default input bakes in its own persistent data at registration,
    /// the way <c>SAMAnalyticalCreateCaseByApertureByAzimuths</c>'s "_ratios" does.</summary>
    public class TestDefaultPersistentDataComponent : TestComponentBase
    {
        public override Guid ComponentGuid => new Guid("d4e5f6a7-8b9c-4d3e-9f1a-2b3c4d5e6f70");

        public override string LatestComponentVersion => "1.0.0";

        public TestDefaultPersistentDataComponent()
            : base("TestDefaultPersistentData", "TestDefaultPersistentData", "Test component whose default input ships with its own baked-in persistent data", "SAM", "Test")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager manager)
        {
            Param_Number ratios = new Param_Number { Name = "ratios", NickName = "ratios", Description = "ratios", Access = GH_ParamAccess.list, Optional = true };
            ratios.SetPersistentData(0.15, 0.2, 0.25, 0.2);
            manager.AddParameter(ratios);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager manager)
        {
            manager.AddParameter(new Param_String { Name = "x", NickName = "x", Description = "x", Access = GH_ParamAccess.item });
        }

        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
        }
    }

    public class TestFailingComponent : TestComponentBase
    {
        public static bool ThrowOnConstruct;

        public override Guid ComponentGuid => new Guid("2c4d9b7a-0e5f-4c3a-9d8b-7a6e5f4c3b12");

        public override string LatestComponentVersion => "1.0.0";

        public TestFailingComponent()
            : base("TestFailing", "TestFailing", "Test component that fails to construct", "SAM", "Test")
        {
            if (ThrowOnConstruct)
            {
                throw new InvalidOperationException("Construction failed");
            }
        }

        protected override void RegisterInputParams(GH_InputParamManager manager)
        {
            manager.AddParameter(new Param_String { Name = "a", NickName = "a", Description = "a", Access = GH_ParamAccess.item, Optional = true });
        }

        protected override void RegisterOutputParams(GH_OutputParamManager manager)
        {
            manager.AddParameter(new Param_String { Name = "x", NickName = "x", Description = "x", Access = GH_ParamAccess.item });
        }

        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
        }
    }
}
