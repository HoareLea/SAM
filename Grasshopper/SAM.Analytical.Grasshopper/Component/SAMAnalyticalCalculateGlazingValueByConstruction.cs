using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalCalculateGlazingValueByConstruction : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("c3a1cf98-6c02-4e44-b2fc-14172ffa9043");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM3);


        public override GH_Exposure Exposure => GH_Exposure.tertiary;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalCalculateGlazingValueByConstruction()
          : base("SAMAnalytical.CalculateGlazingValueByConstruction", "SAMAnalytical.CalculateGlazingValueByConstruction",
              "Calculate Glazing Value By Construction using EN 410",
              "SAM WIP", "Tas")
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
                result.Add(new GH_SAMParam(new GooAnalyticalModelParam() { Name = "_analyticalModel", NickName = "_analyticalModel", Description = "SAM Analytical Model", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooAnalyticalObjectParam() { Name = "_constructions_", NickName = "_constructions_", Description = "SAM Constructions", Access = GH_ParamAccess.list, Optional = true }, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Number param_Number;

                param_Number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_tMean_", NickName = "_tMean_", Description = "Mean gap temperature", Access = GH_ParamAccess.item, Optional = true };
                param_Number.SetPersistentData(20);
                result.Add(new GH_SAMParam(param_Number, ParamVisibility.Voluntary));

                param_Number = new global::Grasshopper.Kernel.Parameters.Param_Number()
                {
                    Name = "_hi_",
                    NickName = "_hi_",
                    Description = "hi [W/m²K]\nInternal heat transfer coefficient.\nCalculated as: hi = 3.6 + (4.1 * εi / 0.837)\nwhere εi is the corrected emissivity of the inside surface.\nFor uncoated soda-lime silicate or borosilicate glass: εi = 0.837 = hi = 7.7W/m²K \nDefault: Calculated for equation you can override with fix hi = 7.7 W/m²K for Rsi = 0.13.",
                    Access = GH_ParamAccess.item,
                    Optional = true
                };
                //param_Number.SetPersistentData(7.7);
                result.Add(new GH_SAMParam(param_Number, ParamVisibility.Voluntary));

                param_Number = new global::Grasshopper.Kernel.Parameters.Param_Number()
                {
                    Name = "_he_",
                    NickName = "_he_",
                    Description = "he [W/m²K]\nExternal heat transfer coefficient.\nDefault: he = 25 for Rse = 0.04.",
                    Access = GH_ParamAccess.item,
                    Optional = true
                };
                param_Number.SetPersistentData(25);
                result.Add(new GH_SAMParam(param_Number, ParamVisibility.Voluntary));

                global::Grasshopper.Kernel.Parameters.Param_Boolean @boolean = null;

                @boolean = new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "_run", NickName = "_run", Description = "Connect a boolean toggle to run.", Access = GH_ParamAccess.item };
                @boolean.SetPersistentData(false);
                result.Add(new GH_SAMParam(@boolean, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new GooAnalyticalModelParam() { Name = "AnalyticalModel", NickName = "AnalyticalModel", Description = "SAM Analytical Model", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooAnalyticalObjectParam() { Name = "Constructions", NickName = "Constructions", Description = "SAM Analytical Constructions", Access = GH_ParamAccess.list }, ParamVisibility.Binding));

                // Light Visible (outside incidence)
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "Tv", NickName = "Tv", Description = "Visible transmittance, outside incidence (EN 410).\n | TAS: Light Transmittance", Access = GH_ParamAccess.tree }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "RvExt", NickName = "RvExt", Description = "Visible reflectance (external), outside incidence (EN 410).\n | TAS: Light Reflectance (Ext.)", Access = GH_ParamAccess.tree }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "RvInt", NickName = "RvInt", Description = "Visible reflectance (internal), inside incidence (EN 410).\n | TAS: (not shown)", Access = GH_ParamAccess.tree }, ParamVisibility.Voluntary));

                // Solar Energy (outside incidence)
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "TauSolar", NickName = "TauSolar", Description = "Direct solar transmittance τe (EN 410 §5.4).\n | TAS: Direct Transmittance", Access = GH_ParamAccess.tree }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "RSolarExt", NickName = "RSolarExt", Description = "Direct solar reflectance (external) ρe,ext (EN 410).\n | TAS: Direct Reflectance", Access = GH_ParamAccess.tree }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "RSolarInt", NickName = "RSolarInt", Description = "Direct solar reflectance (internal) ρe,int (EN 410).\n | TAS: (not shown)", Access = GH_ParamAccess.tree }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "ASolarByLayer", NickName = "ASolarByLayer", Description = "Per-pane direct solar absorptance αᵢ (EN 410 §5.4.5).\n | TAS: External/Internal Solar Absorptance (per surface)", Access = GH_ParamAccess.tree }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "ASolarTotal", NickName = "ASolarTotal", Description = "Total direct solar absorptance Σα (EN 410).\n | TAS: Direct Absorptance", Access = GH_ParamAccess.tree }, ParamVisibility.Binding));

                // Thermal split + g
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "QiByLayer", NickName = "QiByLayer", Description = "Secondary heat-to-room fraction qᵢ per pane (EN 410 §5.4.6).\n | TAS: used internally for G Value (not listed)", Access = GH_ParamAccess.tree }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "GValue", NickName = "GValue", Description = "Total Solar Energy Transmittance g (EN 410).\n | TAS: Total Transmittance (G Value)", Access = GH_ParamAccess.tree }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "ShadingCoefficient", NickName = "ShadingCoefficient", Description = "Shading Coefficient SC = g/0.87 (EN 410 §5.7).\n | TAS: Pilkington Shading Coefficients (Total)", Access = GH_ParamAccess.tree }, ParamVisibility.Binding));

                // Diagnostics
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "SolarEnergyBalanceResidualExt", NickName = "SolarEnergyBalanceResidualExt", Description = "Energy balance residual (external incidence) = 1 − (τ + ρ + Σα).\n | TAS: n/a", Access = GH_ParamAccess.tree }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "SolarEnergyBalanceResidualInt", NickName = "SolarEnergyBalanceResidualInt", Description = "Energy balance residual (internal incidence) = 1 − (τ + ρ + Σα).\n | TAS: n/a", Access = GH_ParamAccess.tree }, ParamVisibility.Voluntary));

                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "successful", NickName = "successful", Description = "Correctly imported?\n | TAS: n/a", Access = GH_ParamAccess.item }, ParamVisibility.Binding));


                return [.. result];
            }
        }


        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            int index_successful = Params.IndexOfOutputParam("successful");
            if (index_successful != -1)
            {
                dataAccess.SetData(index_successful, false);
            }

            int index;

            bool run = false;
            index = Params.IndexOfInputParam("_run");
            if (index == -1 || !dataAccess.GetData(index, ref run))
            {
                run = false;
            }

            if (!run)
            {
                return;
            }

            AnalyticalModel analyticalModel = null;
            index = Params.IndexOfInputParam("_analyticalModel");
            if (index == -1 || !dataAccess.GetData(index, ref analyticalModel) || analyticalModel == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<IAnalyticalObject> analyticalObjects = [];
            index = Params.IndexOfInputParam("_constructions_");
            if (index != -1)
            {
                dataAccess.GetDataList(index, analyticalObjects);
            }

            if (analyticalObjects == null || analyticalObjects.Count == 0)
            {
                analyticalObjects = [];

                analyticalModel?.AdjacencyCluster?.GetConstructions()?.ForEach(x => analyticalObjects.Add(x));
                analyticalModel?.AdjacencyCluster?.GetApertureConstructions()?.ForEach(x => analyticalObjects.Add(x));
            }

            double tMean = 20;
            index = Params.IndexOfInputParam("_tMean_");
            if (index != -1)
            {
                dataAccess.GetData(index, ref tMean);
            }

            double hi = double.NaN;
            index = Params.IndexOfInputParam("_hi_");
            if (index != -1)
            {
                dataAccess.GetData(index, ref hi);
            }

            double he = 23;
            index = Params.IndexOfInputParam("_he_");
            if (index != -1)
            {
                dataAccess.GetData(index, ref he);
            }

            DataTree<GH_Number> tvs = new();
            DataTree<GH_Number> rvExts = new();
            DataTree<GH_Number> rvInts = new();
            DataTree<GH_Number> tauSolars = new();
            DataTree<GH_Number> rSolarExts = new();
            DataTree<GH_Number> rSolarInts = new();
            DataTree<GH_Number> aSolarByLayers = new();
            DataTree<GH_Number> aSolarTotals = new();
            DataTree<GH_Number> qiByLayers = new();
            DataTree<GH_Number> gValues = new();
            DataTree<GH_Number> shadingCoefficients = new();
            DataTree<GH_Number> solarEnergyBalanceResidualExts = new();
            DataTree<GH_Number> solarEnergyBalanceResidualInts = new();

            MaterialLibrary materialLibrary = analyticalModel.MaterialLibrary;

            List<IAnalyticalObject> analyticalObjects_Result = [];

            if (materialLibrary is not null && analyticalObjects is not null)
            {
                for (int i = 0; i < analyticalObjects.Count; i++)
                {

                    if(analyticalObjects[i] is not IAnalyticalObject analyticalObject)
                    {
                        continue;
                    }

                    List<ConstructionLayer> constructionLayers = null;

                    if(analyticalObject is Construction construction)
                    {
                        constructionLayers = construction.ConstructionLayers;
                    }
                    else if(analyticalObject is ApertureConstruction apertureConstruction)
                    {
                        constructionLayers = apertureConstruction.PaneConstructionLayers;
                    }
                    else
                    {
                        continue;
                    }

                    if(constructionLayers is null)
                    {
                        continue;
                    }

                    bool transparent = Analytical.Query.Transparent(constructionLayers, materialLibrary);
                    if (!transparent)
                    {
                        continue;
                    }

                    analyticalObjects_Result.Add(analyticalObject);

                    List<Glazing.Layer> layers = [];
                    List<Glazing.Gap> gaps = [];
                    //Order of layers an gases from outside to inside

                    double emissivity = double.NaN;

                    for (int j = constructionLayers.Count - 1; j >= 0; j--)
                    {
                        if (constructionLayers[j] is not ConstructionLayer constructionLayer)
                        {
                            continue;
                        }

                        IMaterial material = materialLibrary.GetMaterial(constructionLayer.Name);
                        if (material is TransparentMaterial transparentMaterial)
                        {
                            double tauSolar = transparentMaterial.GetValue<double>(TransparentMaterialParameter.SolarTransmittance);
                            double rfSolar = transparentMaterial.GetValue<double>(TransparentMaterialParameter.ExternalSolarReflectance);
                            double rbSolar = transparentMaterial.GetValue<double>(TransparentMaterialParameter.InternalSolarReflectance);
                            double tauVis = transparentMaterial.GetValue<double>(TransparentMaterialParameter.LightTransmittance);
                            double rfVis = transparentMaterial.GetValue<double>(TransparentMaterialParameter.ExternalLightReflectance);
                            double rbVis = transparentMaterial.GetValue<double>(TransparentMaterialParameter.InternalLightReflectance);

                            double thickness = transparentMaterial.GetValue<double>(Core.MaterialParameter.DefaultThickness);

                            Glazing.Layer layer = new Glazing.Layer(tauSolar, rfSolar, rbSolar, tauVis, rfVis, rbVis, thickness, transparentMaterial.ThermalConductivity);
                            layers.Add(layer);

                            emissivity = transparentMaterial.GetValue<double>(TransparentMaterialParameter.InternalEmissivity);
                        }
                        else if (material is GasMaterial gasMaterial)
                        {
                            if (!gasMaterial.TryGetValue(GasMaterialParameter.HeatTransferCoefficient, out double heatTransferCoefficient))
                            {
                                continue;
                            }

                            if (!(j > 0 && j < constructionLayers.Count - 1))
                            {
                                continue;
                            }

                            TransparentMaterial transparentMaterial_1 = materialLibrary.GetMaterial(constructionLayers[j + 1].Name) as TransparentMaterial;
                            if (transparentMaterial_1 is null)
                            {
                                continue;
                            }

                            double emissivity_1 = transparentMaterial_1.GetValue<double>(TransparentMaterialParameter.InternalEmissivity);

                            TransparentMaterial transparentMaterial_2 = materialLibrary.GetMaterial(constructionLayers[j - 1].Name) as TransparentMaterial;
                            if (transparentMaterial_2 is null)
                            {
                                continue;
                            }

                            double emissivity_2 = transparentMaterial_2.GetValue<double>(TransparentMaterialParameter.ExternalEmissivity);

                            Glazing.Gap gap = Glazing.MakeGapFromConvective(heatTransferCoefficient, emissivity_1, emissivity_2, TmeanC: tMean);

                            gaps.Add(gap);
                        }
                    }

                    if(double.IsNaN(hi))
                    {
                        if(double.IsNaN(emissivity))
                        {
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid emissivity for layer");
                            return;
                        }

                        hi = 3.6 + (4.1 * emissivity / 0.837);
                    }

                    Glazing.Result result = Glazing.Compute(layers, gaps, null, he, hi);
                    if(result != null)
                    {
                        GH_Path gH_Path = new (i);

                        tvs.Add(new GH_Number(Core.Query.Round(result.Tv, Tolerance.MacroDistance)), gH_Path);
                        rvExts.Add(new GH_Number(Core.Query.Round(result.RvExt, Tolerance.MacroDistance)), gH_Path);
                        rvInts.Add(new GH_Number(Core.Query.Round(result.RvInt, Tolerance.MacroDistance)), gH_Path);
                        tauSolars.Add(new GH_Number(Core.Query.Round(result.TauSolar, Tolerance.MacroDistance)), gH_Path);
                        rSolarExts.Add(new GH_Number(Core.Query.Round(result.RSolarExt, Tolerance.MacroDistance)), gH_Path);
                        rSolarInts.Add(new GH_Number(Core.Query.Round(result.RSolarInt, Tolerance.MacroDistance)), gH_Path);
                        if (result.ASolarByLayer is double[] aSolarByLayers_Temp)
                        {
                            foreach (double aSolarByLayer in aSolarByLayers_Temp)
                            {
                                aSolarByLayers.Add(new GH_Number(Core.Query.Round(aSolarByLayer, Tolerance.MacroDistance)), gH_Path);
                            }
                        }

                        aSolarTotals.Add(new GH_Number(Core.Query.Round(result.ASolarTotal, Tolerance.MacroDistance)), gH_Path);

                        if (result.QiByLayer is double[] qiByLayers_Temp)
                        {
                            foreach (double qiByLayer in qiByLayers_Temp)
                            {
                                qiByLayers.Add(new GH_Number(Core.Query.Round(qiByLayer, Tolerance.MacroDistance)), gH_Path);
                            }
                        }

                        gValues.Add(new GH_Number(Core.Query.Round(result.GValue, Tolerance.MacroDistance)), gH_Path);
                        gValues.Add(new GH_Number(Core.Query.Round(result.ShadingCoefficient, Tolerance.MacroDistance)), gH_Path);

                        solarEnergyBalanceResidualExts.Add(new GH_Number(Core.Query.Round(result.SolarBalanceResidualExt, Tolerance.MacroDistance)), gH_Path);
                        solarEnergyBalanceResidualInts.Add(new GH_Number(Core.Query.Round(result.SolarBalanceResidualInt, Tolerance.MacroDistance)), gH_Path);
                    }
                }
            }

            index = Params.IndexOfOutputParam("AnalyticalModel");
            if (index != -1)
            {
                dataAccess.SetData(index, analyticalModel);
            }

            index = Params.IndexOfOutputParam("Constructions");
            if (index != -1)
            {
                dataAccess.SetDataList(index, analyticalObjects_Result.ConvertAll(x => new GooAnalyticalObject(x)));
            }

            index = Params.IndexOfOutputParam("Tv");
            if (index != -1)
            {
                dataAccess.SetDataTree(index, tvs);
            }

            index = Params.IndexOfOutputParam("RvExt");
            if (index != -1)
            {
                dataAccess.SetDataTree(index, rvExts);
            }

            index = Params.IndexOfOutputParam("RvInt");
            if (index != -1)
            {
                dataAccess.SetDataTree(index, rvInts);
            }

            index = Params.IndexOfOutputParam("TauSolar");
            if (index != -1)
            {
                dataAccess.SetDataTree(index, tauSolars);
            }

            index = Params.IndexOfOutputParam("RSolarExt");
            if (index != -1)
            {
                dataAccess.SetDataTree(index, rSolarExts);
            }

            index = Params.IndexOfOutputParam("RSolarInt");
            if (index != -1)
            {
                dataAccess.SetDataTree(index, rSolarInts);
            }

            index = Params.IndexOfOutputParam("ASolarByLayer");
            if (index != -1)
            {
                dataAccess.SetDataTree(index, aSolarByLayers);
            }

            index = Params.IndexOfOutputParam("ASolarTotal");
            if (index != -1)
            {
                dataAccess.SetDataTree(index, aSolarTotals);
            }

            index = Params.IndexOfOutputParam("QiByLayer");
            if (index != -1)
            {
                dataAccess.SetDataTree(index, qiByLayers);
            }

            index = Params.IndexOfOutputParam("GValue");
            if (index != -1)
            {
                dataAccess.SetDataTree(index, gValues);
            }

            index = Params.IndexOfOutputParam("ShadingCoefficient");
            if (index != -1)
            {
                dataAccess.SetDataTree(index, shadingCoefficients);
            }

            index = Params.IndexOfOutputParam("SolarEnergyBalanceResidualExt");
            if (index != -1)
            {
                dataAccess.SetDataTree(index, solarEnergyBalanceResidualExts);
            }

            index = Params.IndexOfOutputParam("SolarEnergyBalanceResidualInt");
            if (index != -1)
            {
                dataAccess.SetDataTree(index, solarEnergyBalanceResidualInts);
            }

            if (index_successful != -1)
            {
                dataAccess.SetData(index_successful, true);
            }
        }
    }
}