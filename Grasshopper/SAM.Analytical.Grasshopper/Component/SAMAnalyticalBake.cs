using GH_IO.Serialization;
using Grasshopper.Kernel;
using Rhino;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalBake : GH_SAMVariableOutputParameterComponent
    {
        private bool cutApertures = true;
        private PanelBakeMethod panelBakeMethod = PanelBakeMethod.PanelType;

        public override bool Write(GH_IWriter writer)
        {
            writer.SetInt32(typeof(PanelBakeMethod).Name, panelBakeMethod.GetHashCode());
            writer.SetBoolean("CutApertures", cutApertures);
            return base.Write(writer);
        }

        public override bool Read(GH_IReader reader)
        {
            int @int = -1;
            if (reader.TryGetInt32(typeof(PanelBakeMethod).Name, ref @int) && @int != -1)
            {
                try
                {
                    panelBakeMethod = (PanelBakeMethod)Enum.ToObject(typeof(PanelBakeMethod), @int);
                }
                catch
                {

                }
            }

            reader.TryGetBoolean("CutApertures", ref cutApertures);

            return base.Read(reader);
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("f35b06c1-14f5-4014-a19d-7722d653b428");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public override GH_Exposure Exposure => GH_Exposure.primary;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalBake()
          : base("SAMAnalytical.Bake", "SAMAnalytical.Bake",
              "Bake Analytical Object",
              "SAM WIP", "Analytical")
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
                result.Add(new GH_SAMParam(new GooAnalyticalObjectParam() { Name = "_analytical", NickName = "_analytical", Description = "SAM Analytical Object such as AdjacencyCluster or AnalyticalModel", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

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
                return result.ToArray();
            }
        }

        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            foreach (PanelBakeMethod panelBakeMethod_Temp in Enum.GetValues(typeof(PanelBakeMethod)))
            {
                if(panelBakeMethod_Temp == PanelBakeMethod.Undefined)
                {
                    continue;
                }

                Menu_AppendItem(menu, panelBakeMethod_Temp.ToString(), Menu_Changed, true, panelBakeMethod_Temp.Equals(panelBakeMethod)).Tag = panelBakeMethod_Temp;
            }

            base.AppendAdditionalComponentMenuItems(menu);
        }

        private void Menu_Changed(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem item && item.Tag is PanelBakeMethod)
            {
                panelBakeMethod = (PanelBakeMethod)item.Tag;
                ExpireSolution(true);
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

            index = Params.IndexOfInputParam("_analytical");
            IAnalyticalObject analyticalObject = null;
            if (index == -1 || !dataAccess.GetData(index, ref analyticalObject) || analyticalObject == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            RhinoDoc rhinoDoc = RhinoDoc.ActiveDoc;
            if(rhinoDoc == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Active Rhino Document is missing");
                return;
            }

            List<Panel> panels = null;
            if(analyticalObject is Panel)
            {
                panels = new List<Panel>() { (Panel)analyticalObject };
            }
            else if(analyticalObject is AdjacencyCluster)
            {
                panels = ((AdjacencyCluster)analyticalObject).GetPanels();
            }
            else if(analyticalObject is AnalyticalModel)
            {
                panels = ((AnalyticalModel)analyticalObject).GetPanels();
            }

            if(panels == null || panels.Count == 0)
            {
                return;
            }

            switch(panelBakeMethod)
            {
                case PanelBakeMethod.Construction:
                    Rhino.Modify.BakeGeometry_ByConstruction(rhinoDoc, panels, cutApertures);
                    break;

                case PanelBakeMethod.PanelType:
                    Rhino.Modify.BakeGeometry_ByPanelType(rhinoDoc, panels, cutApertures);
                    break;
            }
        }
    }
}