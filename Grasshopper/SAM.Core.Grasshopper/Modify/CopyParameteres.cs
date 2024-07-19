using Grasshopper.Kernel;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Core.Grasshopper
{
    public static partial class Modify
    {
        public static void CopyParameteres(GH_ParameterSide parameterSide, GH_SAMComponent gH_SAMComponent_From, GH_SAMComponent gH_SAMComponent_To)
        {
            if (gH_SAMComponent_From == null || gH_SAMComponent_To == null)
            {
                return;
            }

            List<IGH_Param> gH_Params;

            Dictionary<string, IGH_Param> dictionary_Old = new Dictionary<string, IGH_Param>();
            gH_Params = parameterSide == GH_ParameterSide.Output ? gH_SAMComponent_From.Params.Output : gH_SAMComponent_From.Params.Input;
            foreach (IGH_Param gH_Param in gH_Params)
            {
                dictionary_Old[gH_Param.Name] = gH_Param;
            }

            Dictionary<string, IGH_Param> dictionary_New = new Dictionary<string, IGH_Param>();
            gH_Params = parameterSide == GH_ParameterSide.Output ? gH_SAMComponent_To.Params.Output : gH_SAMComponent_To.Params.Input;
            if (gH_SAMComponent_To is GH_SAMVariableOutputParameterComponent)
            {
                GH_SAMVariableOutputParameterComponent gH_SAMVariableOutputParameterComponent = (GH_SAMVariableOutputParameterComponent)gH_SAMComponent_To;
                for (int i = gH_Params.Count - 1; i >= 0; i--)
                {
                    if (gH_SAMVariableOutputParameterComponent.CanInsertParameter(parameterSide, i))
                    {
                        gH_Params.Add(gH_SAMVariableOutputParameterComponent.CreateParameter(parameterSide, i));
                    }
                }
            }

            foreach (IGH_Param gH_Param in gH_Params)
            {
                dictionary_New[gH_Param.Name] = gH_Param;
            }

            foreach (KeyValuePair<string, IGH_Param> keyValuePair in dictionary_Old)
            {
                IEnumerable<IGH_Param> gH_Params_Connect = parameterSide == GH_ParameterSide.Output ? keyValuePair.Value.Recipients : keyValuePair.Value.Sources;
                if (gH_Params_Connect == null || gH_Params_Connect.Count() == 0)
                {
                    continue;
                }

                if (!dictionary_New.TryGetValue(keyValuePair.Key, out IGH_Param gH_Param_New))
                {
                    continue;
                }

                foreach (IGH_Param gH_Param in gH_Params_Connect)
                {
                    if (parameterSide == GH_ParameterSide.Output)
                    {
                        gH_Param.AddSource(gH_Param_New);
                    }
                    else
                    {
                        gH_Param_New.AddSource(gH_Param);
                    }
                }
            }


            if (gH_SAMComponent_To is GH_SAMVariableOutputParameterComponent)
            {
                GH_SAMVariableOutputParameterComponent gH_SAMVariableOutputParameterComponent = (GH_SAMVariableOutputParameterComponent)gH_SAMComponent_To;

                GH_ComponentParamServer gH_ComponentParamServer = gH_SAMComponent_To.Params;

                gH_Params = parameterSide == GH_ParameterSide.Output ? gH_ComponentParamServer.Output : gH_ComponentParamServer.Input;
                for (int i = gH_Params.Count - 1; i >= 0; i--)
                {
                    if (dictionary_Old.ContainsKey(gH_Params[i].Name))
                    {
                        continue;
                    }

                    if (gH_SAMVariableOutputParameterComponent.CanRemoveParameter(parameterSide, i))
                    {
                        if (parameterSide == GH_ParameterSide.Output)
                        {
                            gH_ComponentParamServer.UnregisterOutputParameter(gH_Params[i]);
                        }
                        else
                        {
                            gH_ComponentParamServer.UnregisterInputParameter(gH_Params[i]);
                        }
                    }

                }
            }
        }
    }
}