using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Attributes;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalGetDefaultLibrary : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("684d0e03-79ce-4427-b1dc-bde4105f03d4");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public override GH_Exposure Exposure => GH_Exposure.tertiary | GH_Exposure.obscure;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalGetDefaultLibrary()
          : base("SAMAnalytical.GetDefaultLibrary", "SAMAnalytical.GetDefaultLibrary",
              "Get Default SAM Library",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override GH_SAMParam[] Inputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_name", NickName = "_name", Description = "Library name", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                return result.ToArray();
            }
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override GH_SAMParam[] Outputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "Library", NickName = "Library", Description = "SAM Analytical Library", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "Objects", NickName = "Objects", Description = "SAM Objects", Access = GH_ParamAccess.list }, ParamVisibility.Voluntary));
                return result.ToArray();
            }
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            int index = -1;

            string name = null;

            index = Params.IndexOfInputParam("_name");
            if(index == -1 || !dataAccess.GetData(index, ref name) || string.IsNullOrWhiteSpace(name))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            Dictionary<Type, AssociatedTypes> dictionary_AssociatedTypes = ActiveManager.GetAssociatedTypesDictionary(new Type[] { typeof(Setting) });
            if (dictionary_AssociatedTypes == null)
                return;

            Dictionary<Enum, ParameterProperties> dictionary_ParameterProperties = new Dictionary<Enum, ParameterProperties>();
            foreach (Type type in dictionary_AssociatedTypes.Keys)
            {
                foreach (Enum @enum in Enum.GetValues(type))
                {
                    ParameterProperties parameterProperties = ParameterProperties.Get(@enum);
                    if (parameterProperties != null)
                        dictionary_ParameterProperties[@enum] = parameterProperties;
                }

            }

            if(dictionary_ParameterProperties == null || dictionary_ParameterProperties.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            object @object = null;
            foreach(KeyValuePair<Enum, ParameterProperties> keyValuePair in dictionary_ParameterProperties)
            {
                if(name.Equals(keyValuePair.Key.ToString()))
                {
                    @object = keyValuePair.Key;
                    break;
                }

                if(name.Equals(keyValuePair.Value.Name))
                {
                    @object = keyValuePair.Key;
                    break;
                }
            }

            if(@object == null)
            {

                string name_Temp = name.Trim().ToLower().Replace(" ", string.Empty);
                int count = int.MaxValue;
                foreach (KeyValuePair<Enum, ParameterProperties> keyValuePair in dictionary_ParameterProperties)
                {
                    if (!keyValuePair.Key.ToString().ToLower().Contains(name_Temp))
                        continue;

                    int count_Temp = keyValuePair.Key.ToString().Length - name_Temp.Length;
                    if(count_Temp < count)
                    {
                        count = count_Temp;
                        @object = keyValuePair.Key;
                    }
                }
            }

            if(@object == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Cound not find library");
                return;
            }
            
            SAMLibrary sAMLibrary = ActiveSetting.Setting.GetValue<SAMLibrary>((Enum)@object);

            index = Params.IndexOfOutputParam("Library");
            if (index != -1)
                dataAccess.SetData(index, sAMLibrary);

            index = Params.IndexOfOutputParam("Objects");
            if (index != -1)
                dataAccess.SetDataList(index, sAMLibrary?.GetObjects());
        }
    }
}