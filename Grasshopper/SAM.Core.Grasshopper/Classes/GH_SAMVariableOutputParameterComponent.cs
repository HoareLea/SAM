// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using GH_IO.Serialization;
using Grasshopper.Kernel;
using System.Linq;

namespace SAM.Core.Grasshopper
{
    public abstract class GH_SAMVariableOutputParameterComponent : GH_SAMComponent, IGH_VariableParameterComponent
    {
        protected abstract GH_SAMParam[] Inputs { get; }

        protected abstract GH_SAMParam[] Outputs { get; }

        public GH_SAMVariableOutputParameterComponent(string name, string nickname, string description, string category, string subCategory)
            : base(name, nickname, description, category, subCategory)
        {

        }

        public override bool Read(GH_IReader reader)
        {
            bool result = base.Read(reader);
            bool legacyParameterDataRead = this.ReadLegacyParameterData(reader, Inputs, Outputs);
            return result || legacyParameterDataRead;
        }

        /// <summary>
        /// <b>The declared parameter set for one side, in the order it is declared.</b>
        /// <para>
        /// <see cref="Inputs"/> and <see cref="Outputs"/> ARE the identity authority for this component's
        /// parameters - every one of <see cref="CanInsertParameter"/>, <see cref="CreateParameter"/> and
        /// <see cref="CanRemoveParameter"/> already answers by looking a name up in them. A fresh instance
        /// registers only the ones marked <see cref="ParamVisibility.Default"/>, so anything a person
        /// inserted afterwards exists on their component and not on a new one, and only this list can say
        /// what it was and where it belongs.
        /// </para>
        /// <para>
        /// Exposed so <c>Modify.ExpandVariableParameters</c> can bring a replacement component to the same
        /// parameter set as the component it replaces before the wires are restored to it. It is a read of
        /// the declaration and never a mutation.
        /// </para>
        /// </summary>
        internal GH_SAMParam[] TemplateParams(GH_ParameterSide side)
        {
            return side == GH_ParameterSide.Input ? Inputs : Outputs;
        }

        public bool CanInsertParameter(GH_ParameterSide side, int index)
        {
            var templateParams = side == GH_ParameterSide.Input ? Inputs : Outputs;
            var componentParams = side == GH_ParameterSide.Input ? Params.Input : Params.Output;

            if (index >= templateParams.Length)
                return false;

            if (index == 0)
            {
                if (componentParams.Count == 0) return templateParams.Length > 0;

                return componentParams[0].Name != templateParams[0].Param.Name;
            }

            if (index >= componentParams.Count)
                return componentParams[componentParams.Count - 1].Name != templateParams[templateParams.Length - 1].Param.Name;

            string previous = componentParams[index - 1].Name;

            for (int i = 0; i < templateParams.Length; ++i)
            {
                if (templateParams[i].Param.Name == previous)
                    return templateParams[i + 1].Param.Name != componentParams[index].Name;
            }

            return false;
        }

        public bool CanRemoveParameter(GH_ParameterSide side, int index)
        {
            var templateParams = side == GH_ParameterSide.Input ? Inputs : Outputs;
            var componentParams = side == GH_ParameterSide.Input ? Params.Input : Params.Output;

            string current = componentParams[index].Name;
            for (int i = 0; i < templateParams.Length; ++i)
            {
                if (templateParams[i].Param.Name == current)
                {
                    return !templateParams[i].ParamVisibility.HasFlag(ParamVisibility.Mandatory);
                }
            }

            return true;
        }

        private IGH_Param GetTemplateParam(GH_ParameterSide side, int index)
        {
            var templateParams = side == GH_ParameterSide.Input ? Inputs : Outputs;
            var componentParams = side == GH_ParameterSide.Input ? Params.Input : Params.Output;

            int offset = index == 0 ? -1 : +1;
            int reference = index == 0 ? index : index - 1;

            if (componentParams.Count == 0)
            {
                if (templateParams.Length > 0)
                    return templateParams[templateParams.Length + offset].Param;
            }
            else
            {
                var currentName = componentParams[reference].Name;
                for (int i = 0; i < templateParams.Length; ++i)
                {
                    if (templateParams[i].Param.Name == currentName)
                        return templateParams[i + offset].Param;
                }
            }

            return default;
        }

        public IGH_Param CreateParameter(GH_ParameterSide side, int index)
        {
            if (GetTemplateParam(side, index) is IGH_Param param)
            {
                return param.Clone();
            }

            return default;
        }

        public virtual bool DestroyParameter(GH_ParameterSide side, int index) => CanRemoveParameter(side, index);

        public void VariableParameterMaintenance()
        {

        }

        protected override sealed void RegisterInputParams(GH_InputParamManager manager)
        {
            foreach (var definition in Inputs.Where(x => x.ParamVisibility.HasFlag(ParamVisibility.Default)))
            {
                manager.AddParameter(definition.Param.Clone());
            }
        }

        protected override sealed void RegisterOutputParams(GH_OutputParamManager manager)
        {
            foreach (var definition in Outputs.Where(x => x.ParamVisibility.HasFlag(ParamVisibility.Default)))
            {
                manager.AddParameter(definition.Param.Clone());
            }
        }
    }
}
