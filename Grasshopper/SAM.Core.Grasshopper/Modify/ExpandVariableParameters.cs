// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using System;
using System.Collections.Generic;

namespace SAM.Core.Grasshopper
{
    public static partial class Modify
    {
        /// <summary>
        /// <b>Brings a replacement component to the same parameter set as the component it replaces</b>,
        /// before anything is reconnected to it.
        ///
        /// <para><b>Why a replacement does not already have them</b></para>
        /// <para>
        /// <see cref="GH_SAMVariableOutputParameterComponent"/> declares its whole parameter set in
        /// <c>Inputs</c> and <c>Outputs</c>, and registers only the ones marked
        /// <see cref="ParamVisibility.Default"/>. Every other one exists on a person's component because
        /// they inserted it, and a brand new instance has no way to know that: it starts with the defaults
        /// and nothing else. So a wired-up voluntary output has nowhere to land on the replacement, and
        /// <see cref="RestoreConnections"/> - which quite correctly refuses to guess - reports the wire
        /// dropped and asks for it to be reconnected by hand.
        /// </para>
        ///
        /// <para><b>The declaration is the authority, not the position</b></para>
        /// <para>
        /// Which parameter is which is decided by NAME against the declared template, exactly as
        /// <see cref="GH_SAMVariableOutputParameterComponent.CanInsertParameter"/>,
        /// <see cref="GH_SAMVariableOutputParameterComponent.CreateParameter"/> and
        /// <see cref="RestoreConnections"/> already decide it. Nothing here reads an index off the old
        /// component: a parameter added to the middle of a declaration between two versions would make every
        /// index after it name a different thing, and that is precisely the silent mis-reconnection this
        /// exists to avoid.
        /// </para>
        /// <para>
        /// The template's ORDER decides where a restored parameter sits, so the replacement reads the way
        /// the component is declared to read rather than with the recovered ones appended at the end.
        /// </para>
        ///
        /// <para><b>What it will not do</b></para>
        /// <para>
        /// Only parameters the declaration knows about are added. A parameter the old component carried that
        /// the new version no longer declares is genuinely gone, and saying so - through the dropped-wire
        /// report - is the honest answer; fabricating a parameter to hang a wire on would be worse than the
        /// wire being lost. Nothing is ever removed from the replacement either: a default the person had
        /// deleted comes back empty and unwired, which costs them one click, where removing it silently
        /// could take away something the new version needs.
        /// </para>
        /// </summary>
        internal static void ExpandVariableParameters(GH_SAMComponent gH_SAMComponent_From, GH_SAMComponent gH_SAMComponent_To)
        {
            if (gH_SAMComponent_From == null)
            {
                return;
            }

            if (!(gH_SAMComponent_To is GH_SAMVariableOutputParameterComponent gH_SAMVariableOutputParameterComponent))
            {
                return;
            }

            try
            {
                bool changed = ExpandSide(GH_ParameterSide.Input, gH_SAMComponent_From, gH_SAMVariableOutputParameterComponent);

                //Both sides are asked before anything is announced, so the component is told once that its
                //parameters changed rather than once per side.
                changed |= ExpandSide(GH_ParameterSide.Output, gH_SAMComponent_From, gH_SAMVariableOutputParameterComponent);

                if (changed)
                {
                    gH_SAMComponent_To.Params.OnParametersChanged();
                    gH_SAMVariableOutputParameterComponent.VariableParameterMaintenance();
                }
            }
            catch
            {
                // Best-effort: a component that will not expand still updates, and the wires it could not
                // take are reported as dropped rather than lost silently.
            }
        }

        /// <summary>
        /// Walks the declared template for one side and registers, in declared order, every parameter the
        /// replacement is missing and the component being replaced had.
        /// </summary>
        private static bool ExpandSide(GH_ParameterSide side, GH_SAMComponent gH_SAMComponent_From, GH_SAMVariableOutputParameterComponent gH_SAMVariableOutputParameterComponent)
        {
            GH_SAMParam[] gH_SAMParams = gH_SAMVariableOutputParameterComponent.TemplateParams(side);
            if (gH_SAMParams == null || gH_SAMParams.Length == 0)
            {
                return false;
            }

            List<IGH_Param> params_From = side == GH_ParameterSide.Input ? gH_SAMComponent_From.Params?.Input : gH_SAMComponent_From.Params?.Output;
            if (params_From == null || params_From.Count == 0)
            {
                return false;
            }

            HashSet<string> names_From = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (IGH_Param param in params_From)
            {
                if (param != null && param.Name != null)
                {
                    names_From.Add(param.Name);
                }
            }

            GH_ComponentParamServer gH_ComponentParamServer = gH_SAMVariableOutputParameterComponent.Params;

            bool result = false;

            //index walks the REPLACEMENT's parameters in step with the declaration. Because a fresh instance
            //registers its defaults in declared order, what it already has is a subsequence of the template,
            //so index is where the next declared parameter belongs the moment the one before it has been
            //accounted for - which is what puts a recovered parameter back in its declared place rather than
            //at the end.
            int index = 0;

            foreach (GH_SAMParam gH_SAMParam in gH_SAMParams)
            {
                IGH_Param param_Template = gH_SAMParam.Param;
                if (param_Template == null || param_Template.Name == null)
                {
                    continue;
                }

                List<IGH_Param> params_To = side == GH_ParameterSide.Input ? gH_ComponentParamServer.Input : gH_ComponentParamServer.Output;

                if (index < params_To.Count && string.Equals(params_To[index]?.Name, param_Template.Name, StringComparison.OrdinalIgnoreCase))
                {
                    //Already there. The existing instance is kept - it is the one the replacement
                    //registered, and re-creating it would throw away a parameter the wires are about to be
                    //restored to.
                    index++;
                    continue;
                }

                if (!names_From.Contains(param_Template.Name))
                {
                    //Neither component has it, and it is not this update's business to add one.
                    continue;
                }

                if (!(param_Template.Clone() is IGH_Param param_New))
                {
                    continue;
                }

                if (side == GH_ParameterSide.Input)
                {
                    gH_ComponentParamServer.RegisterInputParam(param_New, index);
                }
                else
                {
                    gH_ComponentParamServer.RegisterOutputParam(param_New, index);
                }

                index++;
                result = true;
            }

            return result;
        }
    }
}
