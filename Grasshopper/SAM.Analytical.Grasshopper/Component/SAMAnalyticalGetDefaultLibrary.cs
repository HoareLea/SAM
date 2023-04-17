using GH_IO.Serialization;
using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Attributes;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalGetDefaultLibrary : GH_SAMVariableOutputParameterComponent
    {
        private static Enum[] enums = new Enum[] {
            AnalyticalSettingParameter.DefaultApertureConstructionLibrary,
            AnalyticalSettingParameter.DefaultConstructionLibrary,
            AnalyticalSettingParameter.DefaultDegreeOfActivityLibrary,
            AnalyticalSettingParameter.DefaultGasMaterialLibrary,
            AnalyticalSettingParameter.DefaultInternalConditionLibrary,
            AnalyticalSettingParameter.DefaultInternalConditionLibrary_TM59,
            AnalyticalSettingParameter.DefaultMaterialLibrary,
            AnalyticalSettingParameter.DefaultProfileLibrary,
            AnalyticalSettingParameter.DefaultProfileLibrary_TM59,
            AnalyticalSettingParameter.DefaultSystemTypeLibrary,
        };

        private static Enum defaulEnum = enums[5];

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("684d0e03-79ce-4427-b1dc-bde4105f03d4");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        private string value = defaulEnum.ToString();

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public override GH_Exposure Exposure => GH_Exposure.primary;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalGetDefaultLibrary()
          : base("SAMAnalytical.GetDefaultLibrary", "SAMAnalytical.GetDefaultLibrary",
              "Get Default SAM Library",
              "SAM", "Analytical")
        {
        }

        public override bool Write(GH_IWriter writer)
        {
            writer.SetString("Value", value);
            return base.Write(writer);
        }

        public override bool Read(GH_IReader reader)
        {
            if (!reader.TryGetString("Value", ref value))
                value = defaulEnum.ToString();

            return base.Read(reader);
        }

        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            foreach (Enum @enum in enums)
            {
                ParameterProperties parameterProperties = ParameterProperties.Get(@enum);
                if (parameterProperties == null)
                    continue;

                string name = parameterProperties.Name;
                if (string.IsNullOrWhiteSpace(name))
                    continue;

                Menu_AppendItem(menu, name, Menu_Changed, true, @enum.ToString() == value).Tag = @enum;
            }
                
        }

        private void Menu_Changed(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem item && item.Tag is Enum)
            {
                //Do something with panelType
                value = ((Enum)item.Tag).ToString();
                ExpireSolution(true);
            }
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override GH_SAMParam[] Inputs
        {
            get
            {
                return new GH_SAMParam[0];
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
            int index;

            object @object = null;
            foreach(Enum @enum in enums)
            {
                if(@enum.ToString().Equals(value))
                {
                    @object = @enum;
                    break;
                }
            }

            if(@object == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }
            
            ISAMLibrary sAMLibrary = ActiveSetting.Setting.GetValue<ISAMLibrary>((Enum)@object);

            index = Params.IndexOfOutputParam("Library");
            if (index != -1)
                dataAccess.SetData(index, sAMLibrary);

            index = Params.IndexOfOutputParam("Objects");
            if (index != -1)
            {
                if(sAMLibrary.TryGetObjects(out List<IJSAMObject> jSAMObjects))
                {
                    dataAccess.SetDataList(index, jSAMObjects);
                }
            }
                
        }
    }
}