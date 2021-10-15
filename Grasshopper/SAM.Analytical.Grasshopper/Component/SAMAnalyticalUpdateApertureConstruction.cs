using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalUpdateApertureConstruction : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("6524a7b9-2066-4d36-9c43-5bcb1e1e7428");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalUpdateApertureConstruction()
          : base("SAMAnalytical.UpdateApertureConstruction", "SAMAnalytical.UpdateApertureConstruction",
              "Update Aperture Construction for given Panel",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddParameter(new GooJSAMObjectParam<SAMObject>(), "_analyticals", "_analyticals", "SAM Analytical Objects", GH_ParamAccess.list);
            inputParamManager.AddParameter(new GooApertureParam(), "_apertures", "_apertures", "SAM Analytical Apertures", GH_ParamAccess.list);
            inputParamManager.AddParameter(new GooApertureConstructionParam(), "_apertureConstruction", "_apertureConstruction", "SAM Analytical Aperture Construction", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooJSAMObjectParam<SAMObject>(), "Analytical", "Analytical", "SAM Analytical Object", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            ApertureConstruction apertureConstruction = null;
            if(!dataAccess.GetData(2, ref apertureConstruction))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Aperture> apertures = new List<Aperture>();
            if (!dataAccess.GetDataList(1, apertures))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<SAMObject> sAMObjects = new List<SAMObject>();
            if (!dataAccess.GetDataList(0, sAMObjects))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            for(int i =0; i < sAMObjects.Count; i++)
            {
                SAMObject sAMObject = sAMObjects[i];
                if(sAMObject is Panel)
                {
                    Panel panel = (Panel)sAMObject;
                    bool updated = false;
                    foreach(Aperture aperture in apertures)
                    {
                        if(panel.HasAperture(aperture.Guid))
                        {
                            if (!updated)
                                panel = Create.Panel(panel);

                            panel.RemoveAperture(aperture.Guid);
                            panel.AddAperture(new Aperture(aperture, apertureConstruction));
                            updated = true;
                        }
                    }
                    if (updated)
                        sAMObjects[i] = panel;
                }
                else if(sAMObject is AdjacencyCluster)
                {
                    AdjacencyCluster adjacencyCluster = (AdjacencyCluster)sAMObject;

                    bool updated = false;
                    foreach(Aperture aperture in apertures)
                    {
                        Panel panel = adjacencyCluster.GetPanel(aperture);
                        if (panel == null)
                            continue;

                        if (!updated)
                            adjacencyCluster = new AdjacencyCluster(adjacencyCluster);

                        panel.RemoveAperture(aperture.Guid);
                        panel.AddAperture(new Aperture(aperture, apertureConstruction));
                        adjacencyCluster.AddObject(panel);

                        updated = true;
                    }

                    if (updated)
                        sAMObjects[i] = adjacencyCluster;
                }
                else if(sAMObject is AnalyticalModel)
                {
                    AdjacencyCluster adjacencyCluster = ((AnalyticalModel)sAMObject).AdjacencyCluster;

                    bool updated = false;
                    foreach (Aperture aperture in apertures)
                    {
                        Panel panel = adjacencyCluster.GetPanel(aperture);
                        if (panel == null)
                            continue;

                        panel.RemoveAperture(aperture.Guid);
                        panel.AddAperture(new Aperture(aperture, apertureConstruction));
                        adjacencyCluster.AddObject(panel);

                        updated = true;
                    }

                    if (updated)
                        sAMObjects[i] = new AnalyticalModel((AnalyticalModel)sAMObject, adjacencyCluster);
                }

            }

            dataAccess.SetDataList(0, sAMObjects);
        }
    }
}