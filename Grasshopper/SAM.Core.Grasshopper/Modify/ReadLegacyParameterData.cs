// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using GH_IO.Serialization;
using Grasshopper.Kernel;

namespace SAM.Core.Grasshopper
{
    public static partial class Modify
    {
        /// <summary>
        /// Restores parameters from a legacy fixed-parameter archive after Grasshopper clears them while
        /// deserializing a component that now implements <see cref="IGH_VariableParameterComponent"/>.
        /// </summary>
        public static bool ReadLegacyParameterData(this GH_Component gH_Component, GH_IReader gH_IReader, GH_SAMParam[] inputs, GH_SAMParam[] outputs)
        {
            if (gH_Component == null || gH_IReader == null)
            {
                return false;
            }

            if (gH_IReader.FindChunk("ParameterData") != null || gH_IReader.FindChunk("ParamTypeData") != null)
            {
                return false;
            }

            if (gH_IReader.FindChunk("VariableInput") != null || gH_IReader.FindChunk("VariableOutput") != null)
            {
                return false;
            }

            if (gH_Component.Params.Input.Count != 0 || gH_Component.Params.Output.Count != 0)
            {
                return false;
            }

            if (gH_IReader.FindChunk("param_input", 0) == null && gH_IReader.FindChunk("param_output", 0) == null)
            {
                return false;
            }

            if (inputs != null)
            {
                foreach (GH_SAMParam gH_SAMParam in inputs)
                {
                    if (gH_SAMParam.Param != null && gH_SAMParam.ParamVisibility.HasFlag(ParamVisibility.Default))
                    {
                        gH_Component.Params.RegisterInputParam(gH_SAMParam.Param.Clone());
                    }
                }
            }

            if (outputs != null)
            {
                foreach (GH_SAMParam gH_SAMParam in outputs)
                {
                    if (gH_SAMParam.Param != null && gH_SAMParam.ParamVisibility.HasFlag(ParamVisibility.Default))
                    {
                        gH_Component.Params.RegisterOutputParam(gH_SAMParam.Param.Clone());
                    }
                }
            }

            for (int i = 0; i < gH_Component.Params.Input.Count; i++)
            {
                GH_IReader gH_IReader_Param = gH_IReader.FindChunk("param_input", i);
                if (gH_IReader_Param == null)
                {
                    continue;
                }

                GH_ParamAccess gH_ParamAccess = gH_Component.Params.Input[i].Access;
                gH_Component.Params.Input[i].Read(gH_IReader_Param);
                gH_Component.Params.Input[i].Access = gH_ParamAccess;
            }

            for (int i = 0; i < gH_Component.Params.Output.Count; i++)
            {
                GH_IReader gH_IReader_Param = gH_IReader.FindChunk("param_output", i);
                if (gH_IReader_Param == null)
                {
                    continue;
                }

                GH_ParamAccess gH_ParamAccess = gH_Component.Params.Output[i].Access;
                gH_Component.Params.Output[i].Read(gH_IReader_Param);
                gH_Component.Params.Output[i].Access = gH_ParamAccess;
            }

            if (gH_Component is IGH_VariableParameterComponent variableParameterComponent)
            {
                variableParameterComponent.VariableParameterMaintenance();
            }

            gH_Component.Params.OnParametersChanged();

            return true;
        }
    }
}
