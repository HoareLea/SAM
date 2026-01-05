// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical.Classes;
using SAM.Core;
using SAM.Weather;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Create
    {
        public static AnalyticalModel AnalyticalModel(this AnalyticalModel analyticalModel,
            Case @case)
        {
            if (analyticalModel == null || @case == null)
            {
                return null;
            }

            if (@case is WindowSizeCase windowSizeCase)
            {
                return AnalyticalModel_ByWindowSize(analyticalModel, windowSizeCase);
            }

            if (@case is ApertureConstructionCase apertureConstructionCase)
            {
                return AnalyticalModel_ByApertureConstruction(analyticalModel, apertureConstructionCase);
            }

            if (@case is FinShadeCase finShadeCase)
            {
                return AnalyticalModel_ByFinShade(analyticalModel, finShadeCase);
            }

            if (@case is VentilationCase ventilationCase)
            {
                return AnalyticalModel_ByVentilation(analyticalModel, ventilationCase);
            }

            if (@case is WeatherDataCase weatherDataCase)
            {
                return AnalyticalModel_ByWeatherData(analyticalModel, weatherDataCase);
            }

            if (@case is ApertureCase apertureCase)
            {
                return AnalyticalModel_ByApertureByAzimuths(analyticalModel, apertureCase);
            }

            throw new NotImplementedException();
        }

        public static AnalyticalModel AnalyticalModel_ByWindowSize(this AnalyticalModel analyticalModel,
            double apertureScaleFactor,
            IEnumerable<Aperture> apertures = null)
        {
            if (analyticalModel == null || double.IsNaN(apertureScaleFactor))
            {
                return null;
            }

            List<Aperture> apertures_Temp = apertures == null ? [] : [.. apertures];

            // Resolve aperture list from model when not provided
            if (apertures_Temp.Count == 0)
            {
                apertures_Temp = analyticalModel.GetApertures();
            }
            else
            {
                // Clean input list and re-fetch from model to ensure consistency
                for (int i = apertures_Temp.Count - 1; i >= 0; i--)
                {
                    if (apertures_Temp[i] is null)
                    {
                        apertures_Temp.RemoveAt(i);
                        continue;
                    }

                    Aperture aperture = analyticalModel.GetAperture(apertures_Temp[i].Guid, out Panel panel);
                    if (aperture is null)
                    {
                        apertures_Temp.RemoveAt(i);
                        continue;
                    }

                    apertures_Temp[i] = new Aperture(aperture);
                }
            }

            // Apply scaling when we have targets and a valid factor
            if (apertures_Temp == null || apertures_Temp.Count == 0)
            {
                return new AnalyticalModel(analyticalModel);
            }

            AdjacencyCluster adjacencyCluster = new(analyticalModel.AdjacencyCluster, true);

            foreach (Aperture aperture in apertures_Temp)
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

                panel_Temp = Panel(panel_Temp);

                panel_Temp.RemoveAperture(aperture_Temp.Guid);
                panel_Temp.AddAperture(aperture_Temp);

                adjacencyCluster.AddObject(panel_Temp);
            }

            AnalyticalModel result = new(analyticalModel, adjacencyCluster);


            //CaseDataCollection
            if (!result.TryGetValue(AnalyticalModelParameter.CaseDataCollection, out CaseDataCollection caseDataCollection))
            {
                caseDataCollection = [];
            }
            else
            {
                caseDataCollection = [.. caseDataCollection];
            }

            caseDataCollection.Add(new WindowSizeCaseData(apertureScaleFactor));

            result?.SetValue(AnalyticalModelParameter.CaseDataCollection, caseDataCollection);


            //CaseDescription
            string caseDescription = string.Empty;
            if (!Core.Query.TryGetValue(result, "CaseDescription", out caseDescription))
            {
                caseDescription = string.Empty;
            }

            if (string.IsNullOrWhiteSpace(caseDescription))
            {
                caseDescription = "Case";
            }
            else
            {
                caseDescription += "_";
            }

            string sufix = "ByWindowSize_";
            if (!double.IsNaN(apertureScaleFactor))
            {
                sufix += apertureScaleFactor.ToString();
            }

            caseDescription = caseDescription + sufix;

            Core.Modify.SetValue(result, "CaseDescription", caseDescription);

            return result;
        }

        public static AnalyticalModel AnalyticalModel_ByWindowSize(this AnalyticalModel analyticalModel,
            WindowSizeCase windowSizeCase)
        {
            if (analyticalModel == null || windowSizeCase == null)
            {
                return null;
            }

            List<Aperture> apertures = null;
            if (windowSizeCase.CaseSelection is CaseSelection caseSelection)
            {
                apertures = Query.IJSAMObjects<Aperture>(caseSelection, analyticalModel);
            }

            return AnalyticalModel_ByWindowSize(analyticalModel, windowSizeCase.ApertureScaleFactor, apertures);
        }

        public static AnalyticalModel AnalyticalModel_ByApertureByAzimuths(this AnalyticalModel analyticalModel,
            Dictionary<Range<double>, Tuple<double, ApertureConstruction>> intervalRatioMap,
            bool subdivide,
            double apertureHeight,
            double sillHeight,
            double horizontalSeparation,
            double offset,
            bool keepSeparationDistance,
            IEnumerable<Panel> panels = null)
        {
            if (analyticalModel == null)
            {
                return null;
            }

            if (intervalRatioMap == null || intervalRatioMap.Count == 0)
            {
                return new AnalyticalModel(analyticalModel);
            }

            if (analyticalModel?.AdjacencyCluster is not AdjacencyCluster adjacencyCluster)
            {
                return new AnalyticalModel(analyticalModel);
            }

            adjacencyCluster = new AdjacencyCluster(adjacencyCluster, true);

            List<Panel> panels_Temp = panels == null ? [] : [.. panels];

            if (panels_Temp == null || panels_Temp.Count == 0)
            {
                panels_Temp = analyticalModel.GetPanels();
            }

            HashSet<double> ratios = [];

            foreach (Panel panel in panels_Temp)
            {
                if (panel.PanelType != PanelType.WallExternal)
                {
                    continue;
                }

                double az = NormalizeAngleDegrees(panel.Azimuth());
                if (double.IsNaN(az))
                {
                    continue;
                }

                if (!TryGetRatio(intervalRatioMap, az, out double ratio, out ApertureConstruction apertureConstruction_Temp))
                {
                    continue;
                }

                ratios.Add(ratio);

                Panel panel_New = Panel(panel);

                if (apertureConstruction_Temp is null)
                {
                    apertureConstruction_Temp = Query.DefaultApertureConstruction(panel_New, ApertureType.Window);
                }

                List<Aperture> apertures = panel_New.AddApertures(apertureConstruction_Temp, ratio, subdivide, apertureHeight, sillHeight, horizontalSeparation, offset, keepSeparationDistance);
                if (apertures == null || apertures.Count == 0)
                {
                    continue;
                }

                adjacencyCluster.AddObject(panel_New);
            }

            AnalyticalModel result = new(analyticalModel, adjacencyCluster);


            //CaseDataCollection
            if (!result.TryGetValue(AnalyticalModelParameter.CaseDataCollection, out CaseDataCollection caseDataCollection))
            {
                caseDataCollection = [];
            }
            else
            {
                caseDataCollection = [.. caseDataCollection];
            }

            caseDataCollection.Add(new ApertureCaseData(ratios));

            result?.SetValue(AnalyticalModelParameter.CaseDataCollection, caseDataCollection);


            //CaseDescription
            string caseDescription = string.Empty;
            if (!Core.Query.TryGetValue(result, "CaseDescription", out caseDescription))
            {
                caseDescription = string.Empty;
            }

            if (string.IsNullOrWhiteSpace(caseDescription))
            {
                caseDescription = "Case";
            }
            else
            {
                caseDescription += "_";
            }

            string sufix = "ByApertureByAzimuths_";
            if (ratios != null && ratios.Count != 0)
            {
                sufix += "R_" + string.Join("_", ratios);
            }

            caseDescription = caseDescription + sufix;

            Core.Modify.SetValue(result, "CaseDescription", caseDescription);

            return result;
        }

        public static AnalyticalModel AnalyticalModel_ByApertureByAzimuths(this AnalyticalModel analyticalModel,
            ApertureCase apertureCase)
        {
            if (analyticalModel == null)
            {
                return null;
            }

            if (apertureCase == null)
            {
                return new AnalyticalModel(analyticalModel);
            }

            List<Panel> panels = null;
            if (apertureCase.CaseSelection is CaseSelection caseSelection)
            {
                panels = Query.IJSAMObjects<Panel>(caseSelection, analyticalModel);
            }


            return AnalyticalModel_ByApertureByAzimuths(analyticalModel,
                Convert.ToDictionary(apertureCase.ApertureToPanelRatios),
                apertureCase.Subdivide,
                apertureCase.ApertureHeight,
                apertureCase.SillHeight,
                apertureCase.HorizontalSeparation,
                apertureCase.Offset,
                apertureCase.KeepSeparationDistance,
                panels);
        }

        public static AnalyticalModel AnalyticalModel_ByApertureConstruction(this AnalyticalModel analyticalModel,
            ApertureConstruction apertureConstruction,
            IEnumerable<Aperture> apertures = null)
        {
            if (analyticalModel is null)
            {
                return null;
            }

            List<Aperture> apertures_Temp = apertures == null ? [] : [.. apertures];
            if (apertures_Temp is null || apertures_Temp.Count == 0)
            {
                apertures_Temp = analyticalModel.GetApertures();
            }

            if (apertures_Temp == null || apertures_Temp.Count == 0)
            {
                return new AnalyticalModel(analyticalModel);
            }

            if (analyticalModel?.AdjacencyCluster is not AdjacencyCluster adjacencyCluster)
            {
                return new AnalyticalModel(analyticalModel);
            }

            adjacencyCluster = new AdjacencyCluster(adjacencyCluster, true);

            foreach (Aperture aperture in apertures_Temp)
            {
                Aperture aperture_Temp = new(aperture, apertureConstruction);

                if (adjacencyCluster.GetAperture(aperture_Temp.Guid, out Panel panel_Temp) is null || panel_Temp is null)
                {
                    continue;
                }

                panel_Temp = Panel(panel_Temp);

                panel_Temp.RemoveAperture(aperture_Temp.Guid);
                panel_Temp.AddAperture(aperture_Temp);

                adjacencyCluster.AddObject(panel_Temp);
            }

            AnalyticalModel result = new(analyticalModel, adjacencyCluster);


            //CaseDataCollection
            if (!result.TryGetValue(AnalyticalModelParameter.CaseDataCollection, out CaseDataCollection caseDataCollection))
            {
                caseDataCollection = [];
            }
            else
            {
                caseDataCollection = [.. caseDataCollection];
            }

            caseDataCollection.Add(new ApertureConstructionCaseData(apertureConstruction));

            result?.SetValue(AnalyticalModelParameter.CaseDataCollection, caseDataCollection);


            //CaseDescription
            string caseDescription = string.Empty;
            if (!Core.Query.TryGetValue(result, "CaseDescription", out caseDescription))
            {
                caseDescription = string.Empty;
            }

            if (string.IsNullOrWhiteSpace(caseDescription))
            {
                caseDescription = "Case";
            }
            else
            {
                caseDescription += "_";
            }

            string sufix = "ByApertureConstruction_";
            if (apertureConstruction is not null)
            {
                sufix += apertureConstruction.Name ?? string.Empty;
            }

            caseDescription = caseDescription + sufix;

            Core.Modify.SetValue(result, "CaseDescription", caseDescription);

            return result;
        }

        public static AnalyticalModel AnalyticalModel_ByApertureConstruction(this AnalyticalModel analyticalModel,
            ApertureConstructionCase apertureConstructionCase)
        {
            if (analyticalModel is null || apertureConstructionCase == null)
            {
                return null;
            }

            return AnalyticalModel_ByApertureConstruction(analyticalModel, apertureConstructionCase.ApertureConstruction, apertureConstructionCase.CaseSelection?.IJSAMObjects<Aperture>(analyticalModel));
        }

        public static AnalyticalModel AnalyticalModel_ByOpening(this AnalyticalModel analyticalModel,
            IEnumerable<double> openingAngles,
            IEnumerable<string> descriptions = null,
            IEnumerable<string> functions = null,
            IEnumerable<System.Drawing.Color> colors = null,
            IEnumerable<double> factors = null,
            IEnumerable<Profile> profiles = null,
            bool paneSizeOnly = true,
            IEnumerable<Aperture> apertures = null)
        {
            if (analyticalModel is null)
            {
                return null;
            }

            List<Aperture> apertures_Temp = apertures == null ? [] : [.. apertures];
            if (apertures_Temp is null || apertures_Temp.Count == 0)
            {
                apertures_Temp = analyticalModel.GetApertures();
            }

            AnalyticalModel result = new AnalyticalModel(analyticalModel);

            if (openingAngles == null || openingAngles.Count() == 0)
            {
                return result;
            }


            if (apertures_Temp == null || apertures_Temp.Count == 0)
            {
                return result;
            }

            if (analyticalModel?.AdjacencyCluster is not AdjacencyCluster adjacencyCluster)
            {
                return result;
            }

            adjacencyCluster = new AdjacencyCluster(adjacencyCluster, true);

            //List<Aperture> apertures_Result = [];
            //List<double> dischargeCoefficients_Result = [];
            //List<IOpeningProperties> openingProperties_Result = [];

            for (int i = 0; i < apertures_Temp.Count; i++)
            {
                Aperture aperture = apertures_Temp[i];

                Panel panel = adjacencyCluster.GetPanel(aperture);
                if (panel == null)
                {
                    continue;
                }

                Aperture aperture_Temp = panel.GetAperture(aperture.Guid);
                if (aperture_Temp == null)
                {
                    continue;
                }

                panel = Panel(panel);
                aperture_Temp = new Aperture(aperture_Temp);

                double openingAngle = openingAngles.Count() > i ? openingAngles.ElementAt(i) : openingAngles.Last();
                double width = paneSizeOnly ? aperture_Temp.GetWidth(AperturePart.Pane) : aperture_Temp.GetWidth();
                double height = paneSizeOnly ? aperture_Temp.GetHeight(AperturePart.Pane) : aperture_Temp.GetHeight();

                double factor = (factors != null && factors.Count() != 0) ? (factors.Count() > i ? factors.ElementAt(i) : factors.Last()) : double.NaN;

                PartOOpeningProperties partOOpeningProperties = new(width, height, openingAngle);

                double dischargeCoefficient = partOOpeningProperties.GetDischargeCoefficient();

                ISingleOpeningProperties singleOpeningProperties = null;
                if (profiles != null && profiles.Count() != 0)
                {
                    Profile profile = profiles.Count() > i ? profiles.ElementAt(i) : profiles.Last();
                    ProfileOpeningProperties profileOpeningProperties = new(partOOpeningProperties.GetDischargeCoefficient(), profile);
                    if (!double.IsNaN(factor))
                    {
                        profileOpeningProperties.Factor = factor;
                    }

                    singleOpeningProperties = profileOpeningProperties;
                }
                else
                {
                    if (!double.IsNaN(factor))
                    {
                        partOOpeningProperties.Factor = factor;
                    }

                    singleOpeningProperties = partOOpeningProperties;
                }

                if (descriptions != null && descriptions.Count() != 0)
                {
                    string description = descriptions.Count() > i ? descriptions.ElementAt(i) : descriptions.Last();
                    singleOpeningProperties.SetValue(OpeningPropertiesParameter.Description, description);
                }

                string function_Temp = "zdwno,0,19.00,21.00,99.00";
                if (functions != null && functions.Count() != 0)
                {
                    function_Temp = functions.Count() > i ? functions.ElementAt(i) : functions.Last();
                }
                singleOpeningProperties.SetValue(OpeningPropertiesParameter.Function, function_Temp);

                if (colors != null && colors.Count() != 0)
                {
                    System.Drawing.Color color = colors.Count() > i ? colors.ElementAt(i) : colors.Last();
                    aperture_Temp.SetValue(ApertureParameter.Color, color);
                }
                else
                {
                    aperture_Temp.SetValue(ApertureParameter.Color, Query.Color(ApertureType.Window, AperturePart.Pane, true));
                }

                aperture_Temp.AddSingleOpeningProperties(singleOpeningProperties);

                panel.RemoveAperture(aperture.Guid);
                if (panel.AddAperture(aperture_Temp))
                {
                    adjacencyCluster.AddObject(panel);
                    //apertures_Result.Add(aperture_Temp);
                    //dischargeCoefficients_Result.Add(singleOpeningProperties.GetDischargeCoefficient());
                    //openingProperties_Result.Add(singleOpeningProperties);
                }
            }


            result = new(analyticalModel, adjacencyCluster);


            //CaseDataCollection
            if (!result.TryGetValue(AnalyticalModelParameter.CaseDataCollection, out CaseDataCollection caseDataCollection))
            {
                caseDataCollection = [];
            }
            else
            {
                caseDataCollection = [.. caseDataCollection];
            }

            caseDataCollection.Add(new OpeningCaseData(openingAngles?.FirstOrDefault() ?? double.NaN));


            //CaseDescription
            string caseDescription = string.Empty;
            if (!Core.Query.TryGetValue(result, "CaseDescription", out caseDescription))
            {
                caseDescription = string.Empty;
            }

            if (string.IsNullOrWhiteSpace(caseDescription))
            {
                caseDescription = "Case";
            }
            else
            {
                caseDescription += "_";
            }

            string sufix = "ByOpening_";
            if (openingAngles is not null)
            {
                sufix += string.Join("_", openingAngles.ToList().ConvertAll(x => string.Format("{0}deg", x)));
            }

            caseDescription = caseDescription + sufix;

            Core.Modify.SetValue(result, "CaseDescription", caseDescription);

            return result;
        }

        public static AnalyticalModel AnalyticalModel_ByShade(this AnalyticalModel analyticalModel,
            bool glassPartOnly,
            double overhangDepth,
            double overhangVerticalOffset,
            double overhangFrontOffset,
            double leftFinDepth,
            double leftFinOffset,
            double leftFinFrontOffset,
            double rightFinDepth,
            double rightFinOffset,
            double rightFinFrontOffset,
            IEnumerable<IAnalyticalObject> analyticalObjects = null)
        {
            if (analyticalModel is null)
            {
                return null;
            }

            List<IAnalyticalObject> analyticalObjects_Temp = analyticalObjects == null ? [] : [.. analyticalObjects];
            if (analyticalObjects_Temp is null || analyticalObjects_Temp.Count == 0)
            {
                analyticalObjects_Temp = analyticalModel.GetApertures()?.ConvertAll(x => x as IAnalyticalObject);
            }

            AnalyticalModel result = new AnalyticalModel(analyticalModel);

            if (analyticalObjects_Temp == null || analyticalObjects_Temp.Count == 0)
            {
                return result;
            }

            if (analyticalModel?.AdjacencyCluster is not AdjacencyCluster adjacencyCluster)
            {
                return result;
            }

            adjacencyCluster = new AdjacencyCluster(adjacencyCluster, true);

            foreach (IAnalyticalObject analyticalObject in analyticalObjects_Temp)
            {
                List<Panel> shades = null;
                if (analyticalObject is Panel panel)
                {
                    shades = Panels_Shade(panel, overhangDepth, overhangVerticalOffset, overhangFrontOffset,
                                                 leftFinDepth, leftFinOffset, leftFinFrontOffset,
                                                 rightFinDepth, rightFinOffset, rightFinFrontOffset);
                }
                else if (analyticalObject is Aperture aperture)
                {
                    shades = Panels_Shade(aperture, glassPartOnly, overhangDepth, overhangVerticalOffset, overhangFrontOffset,
                                                 leftFinDepth, leftFinOffset, leftFinFrontOffset,
                                                 rightFinDepth, rightFinOffset, rightFinFrontOffset);
                }

                if (shades == null) continue;

                foreach (Panel shade in shades)
                {
                    adjacencyCluster.AddObject(shade);
                }
            }

            analyticalModel = new AnalyticalModel(analyticalModel, adjacencyCluster);

            result = new(analyticalModel, adjacencyCluster);


            //CaseDataCollection
            if (!result.TryGetValue(AnalyticalModelParameter.CaseDataCollection, out CaseDataCollection caseDataCollection))
            {
                caseDataCollection = [];
            }
            else
            {
                caseDataCollection = [.. caseDataCollection];
            }

            caseDataCollection.Add(new ShadeCaseData(overhangDepth, leftFinDepth, rightFinDepth));

            result?.SetValue(AnalyticalModelParameter.CaseDataCollection, caseDataCollection);


            //CaseDescription
            string caseDescription = string.Empty;
            if (!Core.Query.TryGetValue(result, "CaseDescription", out caseDescription))
            {
                caseDescription = string.Empty;
            }

            if (string.IsNullOrWhiteSpace(caseDescription))
            {
                caseDescription = "Case";
            }
            else
            {
                caseDescription += "_";
            }

            string sufix = "ByShade_";
            if (overhangDepth != 0)
            {
                sufix += string.Format("O{0}m", overhangDepth);
            }

            if (leftFinDepth != 0)
            {
                sufix += string.Format("L{0}m", leftFinDepth);
            }

            if (rightFinDepth != 0)
            {
                sufix += string.Format("R{0}m", rightFinDepth);
            }

            caseDescription = caseDescription + sufix;

            Core.Modify.SetValue(result, "CaseDescription", caseDescription);

            return result;
        }

        public static AnalyticalModel AnalyticalModel_ByFinShade(this AnalyticalModel analyticalModel,
            FinShadeCase finShadeCase)
        {
            if (analyticalModel is null || finShadeCase is null)
            {
                return null;
            }

            return AnalyticalModel_ByShade(analyticalModel,
                finShadeCase.GlassPartOnly,
                finShadeCase.OverhangDepth,
                finShadeCase.OverhangVerticalOffset,
                finShadeCase.OverhangFrontOffset,
                finShadeCase.LeftFinDepth,
                finShadeCase.LeftFinOffset,
                finShadeCase.LeftFinFrontOffset,
                finShadeCase.RightFinDepth,
                finShadeCase.RightFinOffset,
                finShadeCase.RightFinFrontOffset,
                finShadeCase.CaseSelection?.IJSAMObjects<IAnalyticalObject>(analyticalModel));
        }

        public static AnalyticalModel AnalyticalModel_ByShade(this AnalyticalModel analyticalModel,
            IEnumerable<Panel> shades)
        {
            if (analyticalModel is null)
            {
                return null;
            }

            if (shades == null || shades.Count() == 0)
            {
                return new AnalyticalModel(analyticalModel);
            }

            if (analyticalModel?.AdjacencyCluster is not AdjacencyCluster adjacencyCluster)
            {
                return new AnalyticalModel(analyticalModel);
            }

            adjacencyCluster = new AdjacencyCluster(adjacencyCluster, true);

            foreach (Panel shade in shades)
            {
                adjacencyCluster.AddObject(shade);
            }

            AnalyticalModel result = new(analyticalModel, adjacencyCluster);


            //CaseDataCollection
            if (!result.TryGetValue(AnalyticalModelParameter.CaseDataCollection, out CaseDataCollection caseDataCollection))
            {
                caseDataCollection = [];
            }
            else
            {
                caseDataCollection = [.. caseDataCollection];
            }

            caseDataCollection.Add(new ShadeCaseData());

            result?.SetValue(AnalyticalModelParameter.CaseDataCollection, caseDataCollection);


            //CaseDescription
            string caseDescription = string.Empty;
            if (!Core.Query.TryGetValue(result, "CaseDescription", out caseDescription))
            {
                caseDescription = string.Empty;
            }

            if (string.IsNullOrWhiteSpace(caseDescription))
            {
                caseDescription = "Case";
            }
            else
            {
                caseDescription += "_";
            }

            string sufix = "ByShade1_";

            caseDescription = caseDescription + sufix;

            Core.Modify.SetValue(result, "CaseDescription", caseDescription);

            return result;
        }

        public static AnalyticalModel AnalyticalModel_ByVentilation(this AnalyticalModel analyticalModel,
            string function,
            double ach,
            double m3h,
            double factor,
            double setback,
            string description,
            IEnumerable<IAnalyticalObject> analyticalObjects)
        {
            if (analyticalModel is null)
            {
                return null;
            }

            List<IAnalyticalObject> analyticalObjects_Temp = analyticalObjects == null ? [] : [.. analyticalObjects];
            if (analyticalObjects_Temp is null || analyticalObjects_Temp.Count == 0)
            {
                analyticalObjects_Temp = analyticalModel.GetSpaces()?.ConvertAll(x => x as IAnalyticalObject);
            }

            if (analyticalObjects_Temp == null || analyticalObjects_Temp.Count == 0)
            {
                return new AnalyticalModel(analyticalModel);
            }

            if (analyticalModel?.AdjacencyCluster is not AdjacencyCluster adjacencyCluster)
            {
                return new AnalyticalModel(analyticalModel);
            }

            adjacencyCluster = new AdjacencyCluster(adjacencyCluster, true);

            List<Space> spaces = [];

            foreach (IAnalyticalObject analyticalObject_Temp in analyticalObjects)
            {
                if (analyticalObject_Temp is not SAMObject sAMObject)
                    continue;

                List<Space> spaces_Temp = [];
                if (analyticalObject_Temp is Space space)
                {
                    spaces_Temp.Add(adjacencyCluster.GetObject<Space>(sAMObject.Guid));
                }
                else if (analyticalObject_Temp is Zone zone)
                {
                    spaces_Temp = adjacencyCluster.GetSpaces(zone);
                }

                spaces.AddRange(spaces_Temp);
            }

            AnalyticalModel result = new(analyticalModel, adjacencyCluster);

            if (spaces == null || spaces.Count == 0)
            {
                return result;
            }

            //List<InternalCondition> internalConditions = [];

            foreach (Space space in spaces)
            {
                if (space?.InternalCondition is not InternalCondition internalCondition)
                    continue;

                if (function is null)
                {
                    internalCondition.RemoveValue(InternalConditionParameter.VentilationFunction);
                }
                else
                {
                    Function function_Temp = Analytical.Convert.ToSAM_Function(function);
                    if (function_Temp is not null)
                    {
                        FunctionType functionType = function_Temp.GetFunctionType();

                        if (functionType == FunctionType.tcmvc || functionType == FunctionType.tcmvn || functionType == FunctionType.tmmvn)
                        {
                            int vent_Index = 3; // ACH token index for these function types

                            if (!double.IsNaN(ach))
                            {
                                function_Temp[vent_Index] = ach;
                            }
                            else if (!double.IsNaN(m3h))
                            {
                                double volume = space.Volume(adjacencyCluster);
                                if (!double.IsNaN(volume))
                                {
                                    function_Temp[vent_Index] = Core.Query.Round(m3h / volume, Tolerance.MacroDistance);
                                }
                            }
                        }
                    }

                    internalCondition.SetValue(InternalConditionParameter.VentilationFunction, function_Temp?.ToString() ?? function);
                }

                if (description is null)
                    internalCondition.RemoveValue(InternalConditionParameter.VentilationFunctionDescription);
                else
                    internalCondition.SetValue(InternalConditionParameter.VentilationFunctionDescription, description);

                if (double.IsNaN(factor))
                    internalCondition.RemoveValue(InternalConditionParameter.VentilationFunctionFactor);
                else
                    internalCondition.SetValue(InternalConditionParameter.VentilationFunctionFactor, factor);

                if (double.IsNaN(setback))
                    internalCondition.RemoveValue(InternalConditionParameter.VentilationFunctionSetback);
                else
                    internalCondition.SetValue(InternalConditionParameter.VentilationFunctionSetback, setback);

                space.InternalCondition = internalCondition;
                //internalConditions.Add(internalCondition);
                adjacencyCluster.AddObject(space);
            }


            //CaseDataCollection
            if (!result.TryGetValue(AnalyticalModelParameter.CaseDataCollection, out CaseDataCollection caseDataCollection))
            {
                caseDataCollection = [];
            }
            else
            {
                caseDataCollection = [.. caseDataCollection];
            }

            caseDataCollection.Add(new VentilationCaseData(ach));

            result?.SetValue(AnalyticalModelParameter.CaseDataCollection, caseDataCollection);


            //CaseDescription
            string caseDescription = string.Empty;
            if (!Core.Query.TryGetValue(result, "CaseDescription", out caseDescription))
            {
                caseDescription = string.Empty;
            }

            if (string.IsNullOrWhiteSpace(caseDescription))
            {
                caseDescription = "Case";
            }
            else
            {
                caseDescription += "_";
            }

            string sufix = "ByVentilation_";
            if (!double.IsNaN(ach))
            {
                sufix += string.Format("{0}ach", ach);
            }

            if (m3h != 0)
            {
                sufix += string.Format("{0}m3h", m3h);
            }

            if (factor != 0)
            {
                sufix += string.Format("F{0}", factor);
            }

            if (setback != 0)
            {
                sufix += string.Format("sb{0}", setback);
            }

            caseDescription = caseDescription + sufix;

            Core.Modify.SetValue(result, "CaseDescription", caseDescription);

            return result;
        }


        public static AnalyticalModel AnalyticalModel_ByVentilation(this AnalyticalModel analyticalModel,
            VentilationCase ventilationCase)
        {
            if (analyticalModel is null || ventilationCase is null)
            {
                return null;
            }

            return AnalyticalModel_ByVentilation(analyticalModel,
                ventilationCase.Function,
                ventilationCase.ACH,
                ventilationCase.M3h,
                ventilationCase.Factor,
                ventilationCase.Setback,
                ventilationCase.Description,
                ventilationCase.CaseSelection?.IJSAMObjects<IAnalyticalObject>(analyticalModel));
        }

        public static AnalyticalModel AnalyticalModel_ByWeatherData(this AnalyticalModel analyticalModel,
            WeatherData weatherData)
        {
            if (analyticalModel is null)
            {
                return null;
            }

            AnalyticalModel result = new AnalyticalModel(analyticalModel);

            if (weatherData == null)
            {
                return result;
            }

            result.SetValue(AnalyticalModelParameter.WeatherData, weatherData);


            //CaseDataCollection
            if (!result.TryGetValue(AnalyticalModelParameter.CaseDataCollection, out CaseDataCollection caseDataCollection))
            {
                caseDataCollection = [];
            }
            else
            {
                caseDataCollection = [.. caseDataCollection];
            }

            caseDataCollection.Add(new WeatherCaseData(weatherData?.Name));

            result?.SetValue(AnalyticalModelParameter.CaseDataCollection, caseDataCollection);


            //CaseDescription
            string caseDescription = string.Empty;
            if (!Core.Query.TryGetValue(result, "CaseDescription", out caseDescription))
            {
                caseDescription = string.Empty;
            }

            if (string.IsNullOrWhiteSpace(caseDescription))
            {
                caseDescription = "Case";
            }
            else
            {
                caseDescription += "_";
            }

            string sufix = "ByWeather_";
            if (!string.IsNullOrWhiteSpace(weatherData?.Name))
            {
                sufix += weatherData.Name;
            }

            caseDescription = caseDescription + sufix;

            Core.Modify.SetValue(result, "CaseDescription", caseDescription);

            return result;
        }

        public static AnalyticalModel AnalyticalModel_ByWeatherData(this AnalyticalModel analyticalModel,
            WeatherDataCase weatherDataCase)
        {
            if (analyticalModel == null || weatherDataCase == null)
            {
                return null;
            }

            return AnalyticalModel_ByWeatherData(analyticalModel, weatherDataCase.WeatherData);
        }

        /// <summary>Try to find the ratio whose interval contains the given azimuth.</summary>
        private static bool TryGetRatio(Dictionary<Range<double>, Tuple<double, ApertureConstruction>> map, double azimuthDeg, out double ratio, out ApertureConstruction apertureConstruction)
        {
            double azimuthDeg_Round = System.Math.Round(azimuthDeg, MidpointRounding.ToEven);
            apertureConstruction = null;
            ratio = 0.0;

            foreach (var kvp in map)
            {
                if (kvp.Key.In(azimuthDeg_Round))
                {
                    ratio = kvp.Value.Item1;
                    apertureConstruction = kvp.Value.Item2;
                    return true;
                }
            }
            return false;
        }

        /// <summary>Normalise angle to [0, 359].</summary>
        private static double NormalizeAngleDegrees(double angleDeg)
        {
            if (double.IsNaN(angleDeg) || double.IsInfinity(angleDeg))
            {
                return double.NaN;
            }

            double a = angleDeg % 360.0;

            if (a < 0)
            {
                a += 360.0;
            }

            return (a >= 360.0) ? 359.0 : a;
        }

    }
}
