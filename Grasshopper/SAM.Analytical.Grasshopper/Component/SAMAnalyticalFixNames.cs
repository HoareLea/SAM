using GH_IO.Serialization;
using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalFixNames : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("118794ac-53f5-47c4-902f-cf17deb97573");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public override GH_Exposure Exposure => GH_Exposure.primary;

        private string name = null;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalFixNames()
          : base("SAMAnalytical.FixNames", "SAMAnalytical.FixNames",  //SAMAnalytical.ModifyToISOBasicLatinAlphabet
              "Fix/Modidy SAM Analytical Object Names (Spaces, Panel, Aperture, Construction) by removing special characters. \nreplace special character with ISO basic Latin alphabet https://en.wikipedia.org/wiki/ISO_basic_Latin_alphabet \n *Right click and select language.",
              "SAM", "Analytical")
        {
        }

        public override bool Write(GH_IWriter writer)
        {
            if (name != null)
                writer.SetString("Language_", name);

            return base.Write(writer);
        }

        public override bool Read(GH_IReader reader)
        {
            if (!reader.TryGetString("Language_", ref name))
                name = null;

            return base.Read(reader);
        }

        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            List<string> names = ActiveManager.GetSpecialCharacterMapNames();
            if (names == null || names.Count == 0)
                return;

            foreach (string name in names)
                Menu_AppendItem(menu, name, Menu_Changed, true, name == this.name).Tag = name;
        }

        private void Menu_Changed(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem item && item.Tag is string)
            {
                //Do something with panelType
                this.name = (string)item.Tag;
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
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_analytical", NickName = "_analytical", Description = "SAM Analytical Object such as AdjacencyCluster or AnalyticalModel", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "Analytical", NickName = "Analytical", Description = "SAM Analytical AnalyticalModel", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                
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

            index = Params.IndexOfInputParam("_analytical");
            SAMObject sAMObject = null;
            if(index == -1 || !dataAccess.GetData(index, ref sAMObject) || sAMObject == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            if(string.IsNullOrEmpty(name))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Please select language to fix");
            }
            else
            {
                if(sAMObject is AnalyticalModel || sAMObject is AdjacencyCluster)
                {
                    AdjacencyCluster adjacencyCluster = null;
                    if (sAMObject is AnalyticalModel)
                        adjacencyCluster = ((AnalyticalModel)sAMObject).AdjacencyCluster;
                    else if(sAMObject is AdjacencyCluster)
                        adjacencyCluster = (AdjacencyCluster)sAMObject;

                    if (adjacencyCluster != null)
                    {
                        adjacencyCluster = new AdjacencyCluster(adjacencyCluster);
                        List<Panel> panels = adjacencyCluster.GetPanels();
                        if(panels != null)
                        {
                            foreach (Panel panel in panels)
                            {
                                Panel panel_New = Analytical.Query.ReplaceNameSpecialCharacters(panel, name);

                                if (panel_New != null && panel_New.HasApertures)
                                {
                                    foreach (Aperture aperture in panel_New.Apertures)
                                    {
                                        Aperture aperture_New = Analytical.Query.ReplaceNameSpecialCharacters(aperture, name);
                                        if (aperture_New == aperture)
                                            continue;

                                        panel_New = Create.Panel(panel_New);

                                        panel_New.RemoveAperture(aperture.Guid);
                                        panel.AddAperture(aperture_New);
                                    }
                                }

                                if (panel_New != panel)
                                    adjacencyCluster.AddObject(panel_New);
                            }
                        }

                        List<Space> spaces = adjacencyCluster.GetSpaces();
                        if (spaces != null)
                        {
                            foreach (Space space in spaces)
                            {
                                InternalCondition internalCondition = Analytical.Query.ReplaceNameSpecialCharacters(space.InternalCondition, name);
                                Space space_New = null;
                                if (internalCondition.Name != space.InternalCondition.Name)
                                    space_New = new Space(space) { InternalCondition = internalCondition };
                                else
                                    space_New = space;

                                space_New = Analytical.Query.ReplaceNameSpecialCharacters(space_New, name);
                                if (space_New != space)
                                    adjacencyCluster.AddObject(space_New);
                            }
                        }
                    }

                    if (sAMObject is AdjacencyCluster)
                        sAMObject = adjacencyCluster;
                    else if (sAMObject is AnalyticalModel)
                        sAMObject = new AnalyticalModel((AnalyticalModel)sAMObject, adjacencyCluster);
                        
                }
            }

            index = Params.IndexOfOutputParam("Analytical");
            if (index != -1)
                dataAccess.SetData(index, sAMObject);
        }
    }
}