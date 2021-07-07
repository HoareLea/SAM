using GH_IO.Serialization;
using Grasshopper.Kernel;
using SAM.Core.Grasshopper.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace SAM.Core.Grasshopper
{
    public class SAMCoreSelectByName : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("459800dc-ca28-4b84-84c1-7baab93850df");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.2";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;


        private List<string> values;
        private string value;

        //private ToolStripDropDown menu;

        /// <summary>
        /// AboutInfo
        /// </summary>
        public SAMCoreSelectByName()
          : base("SelectByName", "SelectByName",
              "Select SAMObject By Name from List",
              "SAM", "Core")
        {
        }

        public override bool Write(GH_IWriter writer)
        {
            if (values == null)
                values = new List<string>();
            
            writer.SetString("Values", string.Join("\n", values));

            if (value == null)
                value = string.Empty;

            writer.SetString("Value", value);

            return base.Write(writer);
        }

        public override bool Read(GH_IReader reader)
        {
            string text = null;
            
            if (reader.TryGetString("Values", ref text) && !string.IsNullOrWhiteSpace(text))
                values = text.Split('\n').ToList();

            reader.TryGetString("Value", ref value);

            return base.Read(reader);
        }

        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            //this.menu = menu;
            
            foreach (string value in values)
                Menu_AppendItem(menu, value, Menu_Changed, true, value.Equals(this.value)).Tag = value;
        }

        private void Menu_Changed(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem item && item.Tag is string)
            {
                string value_New = (string)item.Tag;

                if (value != value_New)
                {
                    //Do something with panelType
                    value = value_New;
                    ExpireSolution(true);
                }
            }
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddParameter(new GooSAMObjectParam<SAMObject>(), "_objects", "_objects", "Objects", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooSAMObjectParam<SAMObject>(), "In", "In", "In", GH_ParamAccess.list);
            outputParamManager.AddParameter(new GooSAMObjectParam<SAMObject>(), "Out", "Out", "Out", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            List<SAMObject> sAMObjects = new List<SAMObject>();
            if(!dataAccess.GetDataList(0, sAMObjects) || sAMObjects == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            HashSet<string> values = new HashSet<string>();
            foreach(SAMObject sAMObject in sAMObjects)
            {
                string name = null;
                if (!Core.Query.TryGetValue(sAMObject, "Name", out name))
                    continue;

                if (string.IsNullOrWhiteSpace(name))
                    continue;

                values.Add(name);
            }

            this.values = new List<string>(values);

            //if(menu != null)
            //{
            //    menu.Items.Clear();

            //    foreach (string value in values)
            //        Menu_AppendItem(menu, value, Menu_Changed, true, value.Equals(this.value)).Tag = value;
            //}

            List<SAMObject> result_In = new List<SAMObject>();
            List<SAMObject> result_Out = new List<SAMObject>();
            foreach (SAMObject sAMObject in sAMObjects)
            {
                string name = null;
                if (!Core.Query.TryGetValue(sAMObject, "Name", out name))
                {
                    result_Out.Add(sAMObject);
                    continue;
                }

                if (value == null || !value.Equals(name))
                {
                    result_Out.Add(sAMObject);
                    continue;
                }
                result_In.Add(sAMObject);
            }

            dataAccess.SetDataList(0, result_In.ConvertAll(x => new GooSAMObject<SAMObject>(x)));
            dataAccess.SetDataList(1, result_Out.ConvertAll(x => new GooSAMObject<SAMObject>(x)));
        }
    }
}