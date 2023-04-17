using GH_IO.Serialization;
using Grasshopper.Kernel;
using SAM.Core.Grasshopper.Properties;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace SAM.Core.Grasshopper
{
    public class SAMCoreGetType : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("d735ab9f-5b23-43d2-bd98-39bc540ac91e");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small3;

        private string typeFullName;

        /// <summary>
        /// Panel Type
        /// </summary>
        public SAMCoreGetType()
          : base("SAMCore.GetType", "SAMCore.GetType",
              "Get Type form objects to find out all Parameter, \n *to be used with Node: SAMCore.ParameterByType",
              "SAM", "Core")
        {
        }

        public override bool Write(GH_IWriter writer)
        {
            if (typeFullName == null)
                writer.SetString("TypeFullName", string.Empty);
            else
                writer.SetString("TypeFullName", typeFullName);

            return base.Write(writer);
        }

        public override bool Read(GH_IReader reader)
        {
            reader.TryGetString("TypeFullName", ref typeFullName);

            return base.Read(reader);
        }

        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            HashSet<Type> types = Core.Query.UniqueTypes(ActiveManager.GetAssociatedTypes());
            if (types == null)
                return;

            foreach (Type type in types)
                Menu_AppendItem(menu, type.Name, Menu_Changed, true, Core.Query.FullTypeName(type).Equals(typeFullName)).Tag = type;
        }

        private void Menu_Changed(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem item && item.Tag is Type)
            {
                typeFullName = Core.Query.FullTypeName((Type)item.Tag);
                ExpireSolution(true);
            }
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooObjectParam(), "Type", "Type", "Type", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            if (string.IsNullOrWhiteSpace(typeFullName))
            {
                dataAccess.SetData(0, null);
                return;
            }

            dataAccess.SetData(0, new GooObject(Type.GetType(typeFullName)));
        }
    }
}