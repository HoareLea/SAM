using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalCreateApertureConstructionByAperture : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("375358ae-a574-42bb-99c7-93797da534e8");

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
        public SAMAnalyticalCreateApertureConstructionByAperture()
          : base("SAMAnalytical.CreateApertureConstructionByAperture", "SAMAnalytical.CreateApertureConstructionByAperture",
              "Creates ApertureConstruction By Apertures",
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
                result.Add(new GH_SAMParam(new GooJSAMObjectParam<SAMObject>() { Name = "_analyticalObject", NickName = "_analyticalObject", Description = "SAM Analytical Objects", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooApertureParam { Name = "apertures_", NickName = "apertures_", Description = "SAM Analytical Apertures", Access = GH_ParamAccess.list, Optional = true }, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new GooJSAMObjectParam<SAMObject> { Name = "AnalyticalObject", NickName = "AnalyticalObject", Description = "SAM Analytical Object", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooApertureParam { Name = "Apertures", NickName = "Apertures", Description = "SAM Analytical Apertures", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooApertureConstructionParam { Name = "ApertureConstructions", NickName = "ApertureConstructions", Description = "SAM Analytical ApertureConstructions", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
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

            index = Params.IndexOfInputParam("_analyticalObject");
            SAMObject sAMObject = null;
            if (index == -1 || !dataAccess.GetData(index, ref sAMObject))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("apertures_");
            List<Aperture> apertures = new List<Aperture>();
            if(index != -1)
            {
                dataAccess.GetDataList(index, apertures);
            }


            List<ApertureConstruction> apertureConstructions = new List<ApertureConstruction>();

            if (sAMObject is AdjacencyCluster || sAMObject is AnalyticalModel)
            {
                AdjacencyCluster adjacencyCluster = sAMObject is AdjacencyCluster ? (AdjacencyCluster)sAMObject : ((AnalyticalModel)sAMObject).AdjacencyCluster;
                adjacencyCluster = new AdjacencyCluster(adjacencyCluster);

                if(apertures == null || apertures.Count == 0)
                {
                    apertures = adjacencyCluster.GetApertures();
                }

                for (int j = 0; j < apertures.Count; j++)
                {
                    Aperture aperture = apertures[j];
                    Panel panel = adjacencyCluster.GetPanel(aperture);
                    if (panel == null)
                    {
                        continue;
                    }

                    ApertureConstruction apertureConstruction = aperture.ApertureConstruction;
                    if (apertureConstruction == null)
                    {
                        continue;
                    }

                    apertureConstruction = new ApertureConstruction(Guid.NewGuid(), apertureConstruction, string.IsNullOrWhiteSpace(apertureConstruction.Name) ? aperture.Guid.ToString() : string.Format("{0} {1}", apertureConstruction.Name, aperture.Guid));
                    apertureConstructions.Add(apertureConstruction);

                    panel.RemoveAperture(aperture.Guid);
                    aperture = new Aperture(aperture, apertureConstruction);
                    panel.AddAperture(aperture);
                    adjacencyCluster.AddObject(panel);
                    apertures[j] = aperture;
                }

                sAMObject = sAMObject is AdjacencyCluster ? adjacencyCluster : new AnalyticalModel((AnalyticalModel)sAMObject, adjacencyCluster);
            }

            index = Params.IndexOfOutputParam("AnalyticalObject");
            if (index != -1)
            {
                dataAccess.SetData(index, sAMObject);
            }

            index = Params.IndexOfOutputParam("Apertures");
            if (index != -1)
            {
                dataAccess.SetDataList(index, apertures);
            }

            index = Params.IndexOfOutputParam("ApertureConstructions");
            if (index != -1)
            {
                dataAccess.SetDataList(index, apertureConstructions);
            }
        }
    }
}