using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalSetAdiabatic : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("3313fee8-e786-41f7-acd1-bb12b544279c");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
                protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalSetAdiabatic()
          : base("SAMAnalytical.SetAdiabatic", "SAMAnalytical.SetAdiabatic",
              "Set Adiabartic Panel",
              "SAM", "Analytical03")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddParameter(new GooAnalyticalObjectParam(), "_analytical", "_analytical", "SAM Analytical Object such as AdjacencyCluster or AnalyticalModel", GH_ParamAccess.item);
            inputParamManager.AddParameter(new GooPanelParam() { Optional = true}, "panels_", "panels_", "SAM Analytical Panels", GH_ParamAccess.list);
            inputParamManager.AddBooleanParameter("_adiabatic_", "_adiabatic", "Is Adiabatic", GH_ParamAccess.item, true);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooAnalyticalObjectParam(), "analytical", "analytical", "SAM Analytical Object", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            SAMObject sAMObject = null;
            if (!dataAccess.GetData(0, ref sAMObject))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            AdjacencyCluster adjacencyCluster = null;
            if(sAMObject is AdjacencyCluster)
            {
                adjacencyCluster = new AdjacencyCluster((AdjacencyCluster)sAMObject);
            }
            else if (sAMObject is AnalyticalModel)
            {
                adjacencyCluster = ((AnalyticalModel)sAMObject).AdjacencyCluster;
            }


            List<Panel> panels = new List<Panel>();
            dataAccess.GetDataList(1, panels);
            if(panels != null && panels.Count != 0)
            {
                bool adiabatic = true;
                if (!dataAccess.GetData(2, ref adiabatic))
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                    return;
                }

                if (panels != null)
                {
                    foreach (Panel panel in panels)
                    {
                        if (panel == null)
                        {
                            continue;
                        }

                        Panel panel_Temp = adjacencyCluster.GetObject<Panel>(panel.Guid);
                        panel_Temp = Create.Panel(panel);
                        if (panel_Temp != null)
                        {
                            panel_Temp.SetValue(PanelParameter.Adiabatic, adiabatic);
                            adjacencyCluster?.AddObject(panel_Temp);
                        }
                    }
                }
            }

            if(sAMObject is AnalyticalModel)
            {
                sAMObject = new AnalyticalModel((AnalyticalModel)sAMObject, adjacencyCluster);
            }
            else if(sAMObject is AdjacencyCluster)
            {
                sAMObject = adjacencyCluster;
            }

            dataAccess.SetData(0, sAMObject);
        }
    }
}