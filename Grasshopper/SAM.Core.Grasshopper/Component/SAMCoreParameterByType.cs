using GH_IO.Serialization;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Core.Attributes;
using SAM.Core.Grasshopper.Properties;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace SAM.Core.Grasshopper
{
    public class SAMCoreParameterByType : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("41116570-d334-4c9a-8924-84e48cdc465e");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.1";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small3;

        private string typeFullName_Input;

        private string typeFullName_Selected;
        private string name_Selected;

        /// <summary>
        /// Panel Type
        /// </summary>
        public SAMCoreParameterByType()
          : base("SAMCore.ParameterByType", "SAMCore.ParameterByType",
              "Get one Parameter from all possible Type parameters",
              "SAM", "Core")
        {
        }

        public override bool Write(GH_IWriter writer)
        {
            if (typeFullName_Input == null)
                writer.SetString("SAMCoreParameter_TypeFullName", string.Empty);
            else
                writer.SetString("SAMCoreParameter_TypeFullName", typeFullName_Selected);

            if (name_Selected == null)
                writer.SetString("SAMCoreParameter_Name", string.Empty);
            else
                writer.SetString("SAMCoreParameter_Name", name_Selected);

            return base.Write(writer);
        }

        public override bool Read(GH_IReader reader)
        {
            reader.TryGetString("SAMCoreParameter_TypeFullName", ref typeFullName_Selected);
            reader.TryGetString("SAMCoreParameter_Name", ref name_Selected);

            return base.Read(reader);
        }

        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            base.AppendAdditionalComponentMenuItems(menu);

            List<Type> types = null;
            if (!string.IsNullOrEmpty(typeFullName_Input))
            {
                Type type = Type.GetType(typeFullName_Input);
                if (type != null)
                    types = new List<Type>() { type };
            }

            if (types == null || types.Count == 0)
                return;


            Dictionary<Type, AssociatedTypes> dictionary = ActiveManager.GetAssociatedTypesDictionary(types);
            if (dictionary == null)
                return;

            foreach (Type type in dictionary.Keys)
            {
                foreach (Enum @enum in Enum.GetValues(type))
                {
                    ParameterProperties parameterProperties = ParameterProperties.Get(@enum);
                    if (parameterProperties != null)
                    {
                        bool enabled = Core.Query.FullTypeName(type).Equals(typeFullName_Selected) && @enum.ToString().Equals(name_Selected);
                        Menu_AppendItem(menu, parameterProperties.Name, Menu_Changed, true, enabled).Tag = @enum;
                    }
                }

            }

        }

        private void Menu_Changed(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem item && item.Tag is Enum)
            {
                Enum @enum = (Enum)item.Tag;

                typeFullName_Selected = Core.Query.FullTypeName(@enum.GetType());
                name_Selected = @enum.ToString();
                ExpireSolution(true);
            }
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddGenericParameter("Type", "Type", "Type", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooObjectParam(), "Parameter", "Parameter", "Parameter", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            Type type = null;
            if (!dataAccess.GetData(0, ref type))
            {
                GH_ObjectWrapper objectWrapper = null;
                if (!dataAccess.GetData(0, ref objectWrapper))
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                    return;
                }

                if (objectWrapper.Value is Type)
                {
                    typeFullName_Input = Core.Query.FullTypeName((Type)objectWrapper.Value);
                }
                else
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                    return;
                }
            }

            if (type != null)
                typeFullName_Input = Core.Query.FullTypeName(type);

            object result = null;
            if (!string.IsNullOrEmpty(typeFullName_Selected))
            {
                Type type_Selected = Type.GetType(typeFullName_Selected);
                if (type_Selected != null && type_Selected.IsEnum)
                {
                    foreach (Enum @enum in Enum.GetValues(type_Selected))
                    {
                        if (@enum.ToString().Equals(name_Selected))
                        {
                            result = @enum;
                            break;
                        }
                    }

                }
            }

            dataAccess.SetData(0, new GooObject(result));
        }
    }
}