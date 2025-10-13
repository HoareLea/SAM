using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalReportRooms : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("cfe66bdb-2090-48f6-9352-25d3b15a8b67");

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
        public SAMAnalyticalReportRooms()
          : base("SAMAnalytical.ReportRooms", "SAMAnalyticalCreate.ReportRooms",
              "Report Rooms",
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

                result.Add(new GH_SAMParam(new GooAnalyticalModelParam() { Name = "_analyticalModel", NickName = "_analyticalModel", Description = "Source SAM AnalyticalModel", Access = GH_ParamAccess.item}, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooSpaceParam() { Name = "spaces_", NickName = "spaces_", Description = "SAM Spaces", Access = GH_ParamAccess.list, Optional = true}, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new Param_String() { Name = "Name", NickName = "Name", Description = "Space Name", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new Param_Number() { Name = "Area", NickName = "Area", Description = "Space Area [m2]", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new Param_Number() { Name = "Volume", NickName = "Volume", Description = "Space Volume [m3]", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new Param_String() { Name = "LevelName", NickName = "LevelName", Description = "Space Level Name", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new Param_Number() { Name = "ExternalPanelsArea", NickName = "ExternalPanelsArea", Description = "External Panels Area [m2]", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new Param_Number() { Name = "ExternalWallArea", NickName = "ExternalWallArea", Description = "External Wall Area [m2]", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new Param_Number() { Name = "WindowArea", NickName = "WindowArea", Description = "Window Area [m2]", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new Param_Number() { Name = "WindowToWallRatio", NickName = "WindowToWallRatio", Description = "Window To Wall Ratio [%]", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new Param_Number() { Name = "Window-gValue", NickName = "Window-gValue", Description = "Window gValue [0 - 1 max]", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new Param_Number() { Name = "FrameArea", NickName = "FrameArea", Description = "Frame Area [m2]", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new Param_Number() { Name = "FrameToWindowRatio", NickName = "FrameToWindowRatio", Description = "Frame To Window Ratio [%]", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new Param_Number() { Name = "OpeningGeometricArea", NickName = "OpeningGeometricArea", Description = "Opening Geometric Area [m2]", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new Param_Number() { Name = "OpeningEffectiveArea", NickName = "OpeningEffectiveArea", Description = "Opening Effective Area [m2]", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new Param_Number() { Name = "OpeningEffectiveEfficiency", NickName = "OpeningEffectiveEfficiency", Description = "Opening Effective Efficiency [%]", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new Param_Number() { Name = "OpeningEffectiveAreaToFloorAreaRatio", NickName = "OpeningEffectiveAreaToFloorAreaRatio", Description = "Opening Effective Area To Floor Area Ratio [%]", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new Param_String() { Name = "OpeningProfileName", NickName = "OpeningProfile", Description = "Opening Profile Name", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
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

            AnalyticalModel analyticalModel= null;
            index = Params.IndexOfInputParam("_analyticalModel");
            if (index == -1 || !dataAccess.GetData(index, ref analyticalModel))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            AdjacencyCluster adjacencyCluster = analyticalModel.AdjacencyCluster;

            MaterialLibrary materialLibrary = analyticalModel.MaterialLibrary;

            index = Params.IndexOfInputParam("spaces_");
            List<Space> spaces = [];
            if (index != -1)
            {
                dataAccess.GetDataList(index, spaces);
            }

            if(spaces is null || spaces.Count == 0)
            {
                spaces = adjacencyCluster.GetSpaces();
            }

            List<string> names = [];
            List<double> areas = [];
            List<double> volumes = [];
            List<string> levelNames = [];
            List<double> externalPanelsAreas = [];
            List<double> externalWallsAreas = [];
            List<double> windowsAreas = [];
            List<double> windowToWallRatios = [];
            List<double> windowTotalSolarEnergyTransmittances = [];

            List<double> frameAreas = [];
            List<double> frameToWindowRatios = [];
            List<double> openingsGeometricAreas = [];
            List<double> openingsEffectiveAreas = [];
            List<double> openingsEffectiveEfficiency = [];
            List<double> openingsEffectiveAreaToFloorAreaRatios = [];
            List<string> openingsProfileNames = [];
            for (int i =0; i < spaces.Count; i++)
            {
                if (spaces[i] == null)
                {
                    continue;
                }

                Space space = adjacencyCluster.GetObject<Space>(spaces[i].Guid);

                double floorArea = space?.GetValue<double>(SpaceParameter.Area) ?? double.NaN;

                if(double.IsNaN(floorArea))
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, string.Format("Area of space {0} [{1}] has not been provided.", space.Name ?? "???", space.Guid));
                }

                names.Add(space?.Name ?? null);
                areas.Add(floorArea); // the same as ReportSpaces Component
                volumes.Add(space?.GetValue<double>(SpaceParameter.Volume) ?? double.NaN); // the same as ReportSpaces Component
                levelNames.Add(space?.GetValue<string>(SpaceParameter.LevelName) ?? null);

                double externalPanelsArea = 0;
                double externalWallsArea = 0;
                double windowsArea = 0;
                double windowTotalSolarEnergyTransmittance = 0;
                double framesArea = 0;
                double openingsGeometricArea = 0;
                double openingsEffectiveArea = 0;

                List<string> openingProfileNames = [];
                List<Panel> panels = adjacencyCluster.GetPanels(space);
                if(panels != null)
                {
                    foreach (Panel panel in panels)
                    {
                        bool external = panel.IsExternal();
                        if (external)
                        {
                            double area = panel.GetArea(); 

                            if (panel.PanelType.PanelGroup() == PanelGroup.Wall)
                            {
                                externalWallsArea += area;
                            }

                            externalPanelsArea += area;

                            if(panel.Apertures is List<Aperture> apertures)
                            {
                                foreach (Aperture aperture in apertures)
                                {
                                    if(!Analytical.Query.Transparent(aperture, materialLibrary))
                                    {
                                        continue;
                                    }

                                    windowsArea += aperture.GetArea();

                                    framesArea += aperture.GetArea(AperturePart.Frame);

                                    if(aperture.TryGetValue(ApertureParameter.TotalSolarEnergyTransmittance, out double totalSolarEnergyTransmittance) && totalSolarEnergyTransmittance > windowTotalSolarEnergyTransmittance)
                                    {
                                        windowTotalSolarEnergyTransmittance = totalSolarEnergyTransmittance;
                                    }

                                    if(aperture.TryGetValue(ApertureParameter.OpeningProperties, out IOpeningProperties openingProperties) && openingProperties != null)
                                    {
                                        if(openingProperties.TryGetValue(OpeningPropertiesParameter.Function, out string function))
                                        {
                                            openingProfileNames.Add(function);
                                        }

                                        double factor = openingProperties.GetFactor();

                                        double openingGeometricArea = factor * (openingProperties is PartOOpeningProperties partOOpeningProperties ? partOOpeningProperties.Width * partOOpeningProperties.Height : aperture.GetArea());

                                        openingsGeometricArea += openingGeometricArea;

                                        openingsEffectiveArea += openingGeometricArea * openingProperties.GetDischargeCoefficient();
                                    }
                                }
                            }
                        }
                    }
                }

                externalPanelsAreas.Add(externalPanelsArea);
                externalWallsAreas.Add(externalWallsArea);
                windowsAreas.Add(windowsArea);
                windowToWallRatios.Add(externalWallsArea == 0 || windowsArea == 0 ? 0 : Core.Query.Round(windowsArea / externalWallsArea * 100, 0.01));
                windowTotalSolarEnergyTransmittances.Add(windowTotalSolarEnergyTransmittance);
                frameAreas.Add(framesArea);
                frameToWindowRatios.Add(windowsArea == 0 || framesArea == 0 ? 0 : Core.Query.Round(framesArea / windowsArea * 100, 0.01));
                openingsGeometricAreas.Add(openingsGeometricArea);
                openingsEffectiveAreas.Add(openingsEffectiveArea);
                openingsEffectiveEfficiency.Add(openingsGeometricArea == 0 || openingsEffectiveArea == 0 ? 0 : Core.Query.Round(openingsEffectiveArea / openingsGeometricArea * 100, 0.01));
                openingsEffectiveAreaToFloorAreaRatios.Add(double.IsNaN(floorArea) || floorArea == 0 || openingsEffectiveArea == 0 ? double.NaN : Core.Query.Round(openingsEffectiveArea / floorArea * 100, 0.01));
                openingsProfileNames.Add(string.Join("\n", openingProfileNames.Distinct()));
            }

            index = Params.IndexOfOutputParam("Name");
            if (index != -1)
            {
                dataAccess.SetDataList(index, names);
            }

            index = Params.IndexOfOutputParam("Area");
            if (index != -1)
            {
                dataAccess.SetDataList(index, areas);
            }

            index = Params.IndexOfOutputParam("Volume");
            if (index != -1)
            {
                dataAccess.SetDataList(index, volumes);
            }

            index = Params.IndexOfOutputParam("LevelName");
            if (index != -1)
            {
                dataAccess.SetDataList(index, levelNames);
            }

            index = Params.IndexOfOutputParam("ExternalPanelsArea");
            if (index != -1)
            {
                dataAccess.SetDataList(index, externalPanelsAreas);
            }

            index = Params.IndexOfOutputParam("ExternalWallArea");
            if (index != -1)
            {
                dataAccess.SetDataList(index, externalWallsAreas);
            }

            index = Params.IndexOfOutputParam("WindowArea");
            if (index != -1)
            {
                dataAccess.SetDataList(index, windowsAreas);
            }

            index = Params.IndexOfOutputParam("WindowToWallRatio");
            if (index != -1)
            {
                dataAccess.SetDataList(index, windowToWallRatios);
            }

            index = Params.IndexOfOutputParam("Window-gValue");
            if (index != -1)
            {
                dataAccess.SetDataList(index, windowTotalSolarEnergyTransmittances);
            }

            index = Params.IndexOfOutputParam("FrameArea");
            if (index != -1)
            {
                dataAccess.SetDataList(index, frameAreas);
            }

            index = Params.IndexOfOutputParam("FrameToWindowRatio");
            if (index != -1)
            {
                dataAccess.SetDataList(index, frameToWindowRatios);
            }

            index = Params.IndexOfOutputParam("OpeningGeometricArea");
            if (index != -1)
            {
                dataAccess.SetDataList(index, openingsGeometricAreas);
            }

            index = Params.IndexOfOutputParam("OpeningEffectiveArea");
            if (index != -1)
            {
                dataAccess.SetDataList(index, openingsEffectiveAreas);
            }

            index = Params.IndexOfOutputParam("OpeningEffectiveEfficiency");
            if (index != -1)
            {
                dataAccess.SetDataList(index, openingsEffectiveEfficiency);
            }

            index = Params.IndexOfOutputParam("OpeningEffectiveAreaToFloorAreaRatio");
            if (index != -1)
            {
                dataAccess.SetDataList(index, openingsEffectiveAreaToFloorAreaRatios);
            }

            index = Params.IndexOfOutputParam("OpeningProfileName");
            if (index != -1)
            {
                dataAccess.SetDataList(index, openingsProfileNames);
            }
        }
    }
}