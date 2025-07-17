using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalUpdateApertureConstruction : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("6524a7b9-2066-4d36-9c43-5bcb1e1e7428");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.1";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
                protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalUpdateApertureConstruction()
          : base("SAMAnalytical.UpdateApertureConstruction", "SAMAnalytical.UpdateApertureConstruction",
              "Update Aperture Construction for given Panel",
              "SAM", "Analytical04")
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
                result.Add(new GH_SAMParam(new GooJSAMObjectParam<SAMObject>() { Name = "_analyticals", NickName = "_analyticals", Description = "SAM Analytical Objects", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooApertureParam { Name = "_apertures", NickName = "_apertures", Description = "SAM Analytical Apertures", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooApertureConstructionParam { Name = "_apertureConstructions", NickName = "_apertureConstructions", Description = "SAM Analytical Aperture Construction", Access = GH_ParamAccess.list, Optional = true }, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new GooJSAMObjectParam<SAMObject> { Name = "Analyticals", NickName = "Analyticas", Description = "SAM Analytical Objects", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
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

            index = Params.IndexOfInputParam("_apertureConstructions");
            List<ApertureConstruction> apertureConstructions = new List<ApertureConstruction>();
            if(index == -1 || !dataAccess.GetDataList(index, apertureConstructions))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_apertures");
            List<Aperture> apertures = new List<Aperture>();
            if (index == -1 || !dataAccess.GetDataList(index, apertures))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            apertureConstructions.Extend(apertures.Count);

            index = Params.IndexOfInputParam("_analyticals");
            List<SAMObject> sAMObjects = new List<SAMObject>();
            if (index == -1 || !dataAccess.GetDataList(index, sAMObjects))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }


            for (int i = 0; i < sAMObjects.Count; i++)
            {
                SAMObject sAMObject = sAMObjects[i];
                if (sAMObject is Panel)
                {
                    Panel panel = (Panel)sAMObject;
                    bool updated = false;
                    for(int j =0; j < apertures.Count; j++)
                    {
                        Aperture aperture = apertures[j];
                        if (panel.HasAperture(aperture.Guid))
                        {
                            if (!updated)
                            {
                                panel = Create.Panel(panel);
                            }

                            panel.RemoveAperture(aperture.Guid);
                            panel.AddAperture(new Aperture(aperture, apertureConstructions[j]));
                            updated = true;
                        }
                    }

                    if (updated)
                    {
                        sAMObjects[i] = panel;
                    }
                }
                else if(sAMObject is AdjacencyCluster || sAMObject is AnalyticalModel)
                {
                    AdjacencyCluster adjacencyCluster = sAMObject is AdjacencyCluster ? (AdjacencyCluster)sAMObject : ((AnalyticalModel)sAMObject).AdjacencyCluster;
                    adjacencyCluster = new AdjacencyCluster(adjacencyCluster);

                    bool updated = false;
                    for (int j = 0; j < apertures.Count; j++)
                    {
                        Aperture aperture = apertures[j];
                        Panel panel = adjacencyCluster.GetPanel(aperture);
                        if (panel == null)
                        {
                            continue;
                        }

                        if (!updated)
                        {
                            adjacencyCluster = new AdjacencyCluster(adjacencyCluster);
                        }

                        panel.RemoveAperture(aperture.Guid);
                        panel.AddAperture(new Aperture(aperture, apertureConstructions[j]));
                        adjacencyCluster.AddObject(panel);

                        updated = true;
                    }

                    if (updated)
                    {
                        sAMObjects[i] = sAMObject is AdjacencyCluster ? adjacencyCluster : new AnalyticalModel((AnalyticalModel)sAMObject, adjacencyCluster);
                    }
                }
            }

            index = Params.IndexOfOutputParam("Analyticals");
            if (index != -1)
            {
                dataAccess.SetDataList(index, sAMObjects);
            }
        }
    }
}