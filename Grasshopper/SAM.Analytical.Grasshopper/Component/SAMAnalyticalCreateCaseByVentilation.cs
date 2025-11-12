using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using SAM.Weather;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    /// <summary>
    /// Create a case-specific AnalyticalModel by setting ventilation behaviour on selected Spaces/Zones
    /// (makes a copy of the base model; the original stays unchanged).
    /// </summary>
    /// <remarks>
    /// <para>
    /// This component prepares a case-specific <see cref="AnalyticalModel"/> by applying a ventilation
    /// function to spaces (optionally filtered by the Spaces/Zones list). You can override air change rate
    /// (ACH) or provide a fixed volumetric flow [m³/h]; the latter is converted to ACH per space by its volume.
    /// A copy of the base model is made so your original model is not changed.
    /// </para>
    /// </remarks>
    public class SAMAnalyticalCreateCaseByVentilation : GH_SAMVariableOutputParameterComponent
    {
        // ────────────────────────────────────────────────────────────────────────────────
        // Metadata
        // ────────────────────────────────────────────────────────────────────────────────

        /// <summary>Gets the unique ID for this component. Do not change this ID after release.</summary>
        public override Guid ComponentGuid => new Guid("fdc0efd6-3ce7-4fa7-b6f6-1fe4235c0639");

        /// <summary>The latest version of this component.</summary>
        public override string LatestComponentVersion => "1.0.3";

        /// <summary>Component icon shown on the Grasshopper canvas.</summary>
        protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        /// <summary>Display priority in the Grasshopper ribbon.</summary>
        public override GH_Exposure Exposure => GH_Exposure.primary;

        // Long, multi-line description for tooltips and docs (MEP-friendly SAM style)
        private const string DescriptionLong =
@"Create a case-specific AnalyticalModel by setting ventilation behaviour on Spaces/Zones.

SUMMARY
  • Makes a copy of _baseAModel_; the original stays unchanged.
  • Applies a ventilation function to target Spaces (optionally scoped by the _spaces/zones list).
  • You may override ACH directly, or give a volumetric flow [m³/h] that is converted per space by volume.

INPUTS
  _baseAModel  (AnalyticalModel, required)
    Base SAM AnalyticalModel used as a template for this case.
    Type: SAM.Analytical.AnalyticalModel

  _spaces/zones  (Space[] or Zone[], optional)
    Target scope. If omitted or empty, all spaces in the model are affected.
    Type: SAM.Analytical.Space or SAM.Analytical.Zone

  _function  (Text, required)
    Ventilation function string controlling temperature-/pollutant-/hybrid-driven ventilation.
    For TAS 9.5.7+, the last value may define a fixed supply air temperature (°C). If disabled,
    keep -50.00 (meaning off). Examples:\n
      tcmvc,0,19.00,20.00,0.000,-50.00\n
      tcmvn,0,19.00,20.00,0.000,-50.00\n
      tmmvc,0,19.00,20.00,0.200,0.000,-50.00\n
      tmmvn,0,19.00,20.00,0.200,0.000,-50.00\n
      tcbvc,0,19.00,20.00,0.200,19.00,20.00,30.000,0,0,50.000,\n
             604.799,755.999,16.000,12.000,755.999,1209.598

  ac/h_  (Number, optional)
    Override air changes per hour. If provided, replaces any 0.000 ACH token in the function.

  m3/h_  (Number, optional)
    Override airflow [m³/h]. Converted per space as:  ACH = (m³/h) / Volume[m³].
    If both ac/h_ and m3/h_ are provided, ac/h_ takes precedence.

  _factor_  (Number, optional, default = 1.0)
    Multiplier applied to the function result.

  _setback_  (Number, optional, default = 0.0)
    Setback applied to the function (e.g., night/weekend reduction).

  description_  (Text, optional)
    Free text description stored with the ventilation function.

OUTPUTS
  CaseAModel  (AnalyticalModel)
    Copy of the base model with updated ventilation settings on the target spaces.

NOTES
  • To affect the whole building, leave _spaces/zones empty.
  • ACH precedence: ac/h_ > m3/h_ > value in _function_.

EXAMPLE
  1) Read a baseline AnalyticalModel → _baseAModel
  2) (Optional) Pick target Spaces/Zones → _spaces/zones
  3) Provide a ventilation function string → _function
  4) (Optional) Override with ac/h_ or m3/h_; set _factor_/_setback_ → CaseAModel
";

        /// <summary>Initializes a new instance of the component.</summary>
        public SAMAnalyticalCreateCaseByVentilation()
          : base(
                "SAMAnalytical.CreateCaseByVentilation",
                "CreateCaseByVentilation",
                DescriptionLong,
                "SAM", "Analytical")
        { }

        // ────────────────────────────────────────────────────────────────────────────────
        // Parameters
        // ────────────────────────────────────────────────────────────────────────────────

        /// <summary>Registers all the input parameters for this component.</summary>
        protected override GH_SAMParam[] Inputs
        {
            get
            {
                List<GH_SAMParam> result = [];

                // Base Analytical Model (required)
                var analyticalModelParam = new GooAnalyticalModelParam
                {
                    Name = "_baseAModel",
                    NickName = "_baseAModel",
                    Description = "Base SAM AnalyticalModel used as a template for this case.\nType: SAM.Analytical.AnalyticalModel\nRequired: Yes",
                    Access = GH_ParamAccess.item
                };
                result.Add(new GH_SAMParam(analyticalModelParam, ParamVisibility.Binding));

                // Target scope (optional)
                result.Add(new GH_SAMParam(
                    new GooAnalyticalObjectParam
                    {
                        Name = "_spaces/zones",
                        NickName = "_spaces/zones",
                        Description = "Target Spaces or Zones. Leave empty to affect all spaces.",
                        Access = GH_ParamAccess.list,
                        Optional = true
                    },
                    ParamVisibility.Binding));

                // _function (Text, required)
                result.Add(new GH_SAMParam(
                    new Param_String
                    {
                        Name = "_function",
                        NickName = "_function",
                        Description = @"Ventilation function string.
Defines the zone's ventilation behaviour (temperature-/pollutant-/hybrid-controlled).
For TAS 9.5.7+, the last value may fix supply air temperature (°C). If that option is disabled,
keep -50.00 (meaning off).

Examples:
  tcmvc,0,19.00,20.00,0.000,-50.00
  tcmvn,0,19.00,20.00,0.000,-50.00
  tmmvc,0,19.00,20.00,0.200,0.000,-50.00
  tmmvn,0,19.00,20.00,0.200,0.000,-50.00
  tcbvc,0,19.00,20.00,0.200,19.00,20.00,30.000,0,0,50.000,
         604.799,755.999,16.000,12.000,755.999,1209.598",
                        Access = GH_ParamAccess.item,
                        Optional = false
                    },
                    ParamVisibility.Binding));


                // ac/h_ (Number, optional)
                result.Add(new GH_SAMParam(
                    new Param_Number
                    {
                        Name = "ac/h_",
                        NickName = "ac/h_",
                        Description = @"Override ACH (air changes per hour).
If provided, replaces any 0.000 ACH token in the function string.",
                        Access = GH_ParamAccess.item,
                        Optional = true
                    },
                    ParamVisibility.Binding));


                // m3/h_ (Number, optional)
                result.Add(new GH_SAMParam(
                    new Param_Number
                    {
                        Name = "m3/h_",
                        NickName = "m3/h_",
                        Description = @"Override airflow [m³/h].
Converted per space: ACH = (m³/h) / Volume [m³].
If both ac/h_ and m3/h_ are provided, ac/h_ takes precedence.",
                        Access = GH_ParamAccess.item,
                        Optional = true
                    },
                    ParamVisibility.Binding));


                // Factor (optional, default 1.0)
                var param_Number = new Param_Number { Name = "_factor_", NickName = "_factor_", Description = "Multiplier applied to the function result.", Access = GH_ParamAccess.item, Optional = true };
                param_Number.SetPersistentData(1.0);
                result.Add(new GH_SAMParam(param_Number, ParamVisibility.Binding));

                // Setback (optional, default 0.0)
                param_Number = new Param_Number { Name = "_setback_", NickName = "_setback_", Description = "Setback applied to the function (e.g., night/weekend).", Access = GH_ParamAccess.item, Optional = true };
                param_Number.SetPersistentData(0.0);
                result.Add(new GH_SAMParam(param_Number, ParamVisibility.Binding));

                // Description (optional)
                result.Add(new GH_SAMParam(new Param_String { Name = "description_", NickName = "description_", Description = "Free text description stored with the ventilation function.", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));

                Param_Boolean param_Boolean = new Param_Boolean
                {
                    Name = "_concatenate_",
                    NickName = "_concatenate_",
                    Description = "concatenate",
                    Access = GH_ParamAccess.item,
                    Optional = true
                };
                param_Boolean.SetPersistentData(true);
                result.Add(new GH_SAMParam(param_Boolean, ParamVisibility.Binding));

                return [.. result];
            }
        }

        /// <summary>Registers all the output parameters for this component.</summary>
        protected override GH_SAMParam[] Outputs
        {
            get
            {
                List<GH_SAMParam> result = [];

                var analyticalModelParam = new GooAnalyticalModelParam
                {
                    Name = "CaseAModel",
                    NickName = "CaseAModel",
                    Description = "Copy of the base model with ventilation settings applied to target spaces.",
                    Access = GH_ParamAccess.item
                };
                result.Add(new GH_SAMParam(analyticalModelParam, ParamVisibility.Binding));

                Param_String param_String = new() { Name = "CaseDescription", NickName = "CaseDescription", Description = "Case Description", Access = GH_ParamAccess.item };
                result.Add(new GH_SAMParam(param_String, ParamVisibility.Binding));

                return [.. result];
            }
        }

        // ────────────────────────────────────────────────────────────────────────────────
        // Execution
        // ────────────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Core execution: read inputs, apply ventilation, and output the case model.
        /// </summary>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            // Base model (required)
            int index = Params.IndexOfInputParam("_baseAModel");
            AnalyticalModel analyticalModel = null;
            if (index == -1 || !dataAccess.GetData(index, ref analyticalModel) || analyticalModel == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "_baseAModel: Please supply a valid SAM AnalyticalModel.");
                return;
            }

            // Function (required)
            string function = null;
            index = Params.IndexOfInputParam("_function");
            if (index == -1 || !dataAccess.GetData(index, ref function) || function == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "_function: Please supply a ventilation function string.");
                return;
            }

            // Optional overrides
            double ach = double.NaN;
            index = Params.IndexOfInputParam("ac/h_");
            if (index != -1) dataAccess.GetData(index, ref ach);

            double m3h = double.NaN;
            index = Params.IndexOfInputParam("m3/h_");
            if (index != -1) dataAccess.GetData(index, ref m3h);

            double factor = double.NaN;
            index = Params.IndexOfInputParam("_factor_");
            if (index != -1 && !dataAccess.GetData(index, ref factor)) factor = 1.0;

            double setback = double.NaN;
            index = Params.IndexOfInputParam("_setback_");
            if (index != -1 && !dataAccess.GetData(index, ref setback)) setback = 0.0;

            string description = null;
            index = Params.IndexOfInputParam("description_");
            if (index != -1 && !dataAccess.GetData(index, ref description)) description = null;

            // Work on a modifiable copy of the cluster
            AdjacencyCluster adjacencyCluster = analyticalModel.AdjacencyCluster;

            List<Space> spaces = [];
            List<InternalCondition> internalConditions = [];

            if (adjacencyCluster is not null)
            {
                adjacencyCluster = new AdjacencyCluster(adjacencyCluster, true);

                // Scope selection (Spaces or Zones)
                List<IAnalyticalObject> analyticalObjects = [];
                index = Params.IndexOfInputParam("_spaces/zones");
                if (index != -1 && dataAccess.GetDataList(index, analyticalObjects) && analyticalObjects != null)
                {
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
                }

                if (spaces is null || spaces.Count == 0)
                {
                    spaces = adjacencyCluster.GetSpaces();
                }
            }

            if (adjacencyCluster != null && spaces != null && spaces.Count != 0)
            {
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
                    internalConditions.Add(internalCondition);
                    adjacencyCluster.AddObject(space);
                }
            }

            if (adjacencyCluster != null)
            {
                analyticalModel = new AnalyticalModel(analyticalModel, adjacencyCluster);
            }

            index = Params.IndexOfOutputParam("CaseDescription");
            if (index != -1)
            {
                int index_Concatenate = Params.IndexOfInputParam("_concatenate_");
                bool concatenate = true;
                if (index_Concatenate != -1)
                {
                    dataAccess.GetData(index_Concatenate, ref concatenate);
                }

                string caseDescription = string.Empty;
                if (concatenate)
                {
                    if (!Core.Query.TryGetValue(analyticalModel, "CaseDescription", out caseDescription))
                    {
                        caseDescription = string.Empty;
                    }
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

                string value = caseDescription + sufix;

                dataAccess.SetData(index, value);
            }

            if (!analyticalModel.TryGetValue(AnalyticalModelParameter.CaseDataCollection, out CaseDataCollection caseDataCollection))
            {
                caseDataCollection = new CaseDataCollection();
            }
            else
            {
                caseDataCollection = new CaseDataCollection(caseDataCollection);
            }

            caseDataCollection.Add(new VentilationCaseData(ach));

            analyticalModel?.SetValue(AnalyticalModelParameter.CaseDataCollection, caseDataCollection);

            // Output
            index = Params.IndexOfOutputParam("CaseAModel");
            if (index != -1)
            {
                dataAccess.SetData(index, analyticalModel);
            }
        }
    }
}
