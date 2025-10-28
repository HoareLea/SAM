using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalCreateCaseByWindowSize : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("41f36339-ae22-4de5-9586-0b9519ad60a4");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        public override GH_Exposure Exposure => GH_Exposure.primary;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalCreateCaseByWindowSize()
          : base("SAMAnalytical.CreateCaseByWindowSize", "SAMAnalytical.CreateCaseByWindowSize",
              "AAA",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override GH_SAMParam[] Inputs
        {
            get
            {
                List<GH_SAMParam> result = [];

                GooAnalyticalModelParam analyticalModelParam = new () { Name = "_baseAModel", NickName = "_baseAModel", Description = "Analytical Model", Access = GH_ParamAccess.item };
                result.Add(new GH_SAMParam(analyticalModelParam, ParamVisibility.Binding));

                GooApertureParam gooApertureParam = new() { Name = "_apertures_", NickName = "_apertures_", Description = "SAM Apertures", Access = GH_ParamAccess.list, Optional = true };
                result.Add(new GH_SAMParam(gooApertureParam, ParamVisibility.Binding));


                global::Grasshopper.Kernel.Parameters.Param_Number param_Number;

                param_Number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_apertureScaleFactor_", NickName = "_apertureScaleFactor_", Description = "Aperture Scale Factor", Access = GH_ParamAccess.item, Optional = true };
                result.Add(new GH_SAMParam(param_Number, ParamVisibility.Binding));

                return [.. result];
            }
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override GH_SAMParam[] Outputs
        {
            get
            {
                List<GH_SAMParam> result = [];

                GooAnalyticalModelParam analyticalModelParam = new () { Name = "CaseAModel", NickName = "CaseAModel", Description = "SAM AnalyticalModel", Access = GH_ParamAccess.item };
                result.Add(new GH_SAMParam(analyticalModelParam, ParamVisibility.Binding));

                return [.. result];
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
            index = Params.IndexOfInputParam("_baseAModel");
            AnalyticalModel analyticalModel = null;
            if (index == -1 || !dataAccess.GetData(index, ref analyticalModel) || analyticalModel == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_apertures_");
            List<Aperture> apertures = [];
            if (index != -1)
            {
                dataAccess.GetDataList(index, apertures);
            }

            index = Params.IndexOfInputParam("_apertureScaleFactor_");
            double apertureScaleFactor = double.NaN;
            if (index != -1)
            {
                dataAccess.GetData(index, ref apertureScaleFactor);
            }

            if(apertures.Count == 0)
            {
                apertures = analyticalModel.GetApertures();
            }
            else
            {
                for(int i = apertures.Count - 1; i >= 0; i--)
                {
                    if (apertures[i] is null)
                    {
                        apertures.RemoveAt(i);
                        continue;
                    }

                    Aperture aperture = analyticalModel.GetAperture(apertures[i].Guid, out Panel panel);
                    if(aperture is null)
                    {
                        apertures.RemoveAt(i);
                        continue;
                    }

                    apertures[i] = new Aperture(aperture);
                }
            }

            if (apertures != null && apertures.Count != 0 && !double.IsNaN(apertureScaleFactor))
            {
                AdjacencyCluster adjacencyCluster = new(analyticalModel.AdjacencyCluster, true);

                foreach (Aperture aperture in apertures)
                {
                    Aperture aperture_Temp = aperture.Rescale(apertureScaleFactor);
                    if (aperture_Temp is null)
                    {
                        continue;
                    }

                    if (adjacencyCluster.GetAperture(aperture_Temp.Guid, out Panel panel_Temp) is null || panel_Temp is null)
                    {
                        continue;
                    }

                    panel_Temp = Create.Panel(panel_Temp);

                    panel_Temp.RemoveAperture(aperture_Temp.Guid);
                    panel_Temp.AddAperture(aperture_Temp);

                    adjacencyCluster.AddObject(panel_Temp);
                }

                analyticalModel = new AnalyticalModel(analyticalModel, adjacencyCluster);
            }

            index = Params.IndexOfOutputParam("CaseAModel");
            if (index != -1)
            {
                dataAccess.SetData(index, analyticalModel);
            }
        }
    }
}