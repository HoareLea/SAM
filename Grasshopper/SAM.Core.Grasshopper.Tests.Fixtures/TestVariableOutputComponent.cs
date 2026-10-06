// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using System;

namespace SAM.Core.Grasshopper.Tests
{
    /// <summary>
    /// A component shaped like the ones recently modernised to
    /// <see cref="GH_SAMVariableOutputParameterComponent"/> and declarative Inputs/Outputs: some parameters
    /// registered by default, some only there because a person inserted them.
    /// <para>
    /// Inputs: <b>a</b> (default), <b>b</b> (default), <b>tolerance</b> (voluntary).
    /// Outputs: <b>x</b> (default), <b>successful</b> (voluntary), <b>y</b> (default).
    /// </para>
    /// <para>
    /// The voluntary output sits in the MIDDLE of the declaration on purpose. Anything restoring parameters
    /// by position would put it back somewhere else and hand its wire to the wrong output, and that failure
    /// would be invisible on a component whose voluntary parameters all sit at the end.
    /// </para>
    /// </summary>
    public class TestVariableOutputComponent : GH_SAMVariableOutputParameterComponent
    {
        public override Guid ComponentGuid => new Guid("8b1e2c47-5a30-4f6d-9e18-3c7b52a90d64");

        public override string LatestComponentVersion => "1.0.0";

        public TestVariableOutputComponent()
            : base("TestVariableOutput", "TestVariableOutput", "Test component with variable parameters", "SAM", "Test")
        {
        }

        public void SetComponentVersion(string value)
        {
            SetValue("SAM_ComponentVersion", value);
        }

        protected override GH_SAMParam[] Inputs
        {
            get
            {
                return new GH_SAMParam[]
                {
                    new GH_SAMParam(Param("a"), ParamVisibility.Binding),
                    new GH_SAMParam(Param("b"), ParamVisibility.Default),
                    new GH_SAMParam(Param("tolerance"), ParamVisibility.Voluntary),
                };
            }
        }

        protected override GH_SAMParam[] Outputs
        {
            get
            {
                return new GH_SAMParam[]
                {
                    new GH_SAMParam(Param("x"), ParamVisibility.Binding),
                    new GH_SAMParam(Param("successful"), ParamVisibility.Voluntary),
                    new GH_SAMParam(Param("y"), ParamVisibility.Default),
                };
            }
        }

        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
        }

        private static Param_String Param(string name)
        {
            return new Param_String { Name = name, NickName = name, Description = name, Access = GH_ParamAccess.item, Optional = true };
        }
    }
}
