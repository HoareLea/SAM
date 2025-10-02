using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalModifyVentilationProfileByFunction : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("e01a70b5-b59b-4b38-b012-b08728f76dc4");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.1";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        public override GH_Exposure Exposure => GH_Exposure.primary;

        /// <summary>
        /// Initializes a new instance of the <c>SAMAnalyticalModifyVentilationProfileByFunction</c> component.
        /// 
        /// This analytical component provides functionality to **modify a ventilation profile** in SAM 
        /// (Sustainable Analytical Model) according to functional rules and control strategies.
        /// It encapsulates different ventilation zones, each governed by temperature-based and 
        /// pollutant-based thresholds, hybrid strategies, and schedule-based overrides.
        /// 
        /// ### Purpose
        /// The component is designed to allow computational adjustment of airflow rates (ACH or L/s·m²) 
        /// based on indoor/outdoor dry-bulb temperatures, pollutant levels, and seasonal or 
        /// operational schedules. This ensures that ventilation dynamically adapts to conditions 
        /// while maintaining comfort, energy efficiency, and pollutant control.
        /// 
        /// ### Supported Ventilation Control Functions
        /// - **tcmvc (Temperature-Controlled Mechanical Ventilation Cooling):**
        ///   Operates when indoor temperature rises above a lower limit. 
        ///   Gradually increases airflow until reaching a maximum ACH at the upper limit.
        ///   Ventilation reduces when outside ≈ inside and is fully off when outside ≥ inside + 1°C.
        /// 
        /// - **tcmvn (Temperature-Controlled Mechanical Ventilation Neutral):**
        ///   Similar to tcmvc, but without applying a cooling offset. 
        ///   Used in neutral control mode where ACH increases linearly between lower and upper thresholds.
        /// 
        /// - **tmmvc (Temperature + Minimum Mechanical Ventilation Cooling):**
        ///   Provides an additional ACH above a defined baseline. 
        ///   Ensures a minimum base ventilation (L/s·m²) and ramps up when indoor dry-bulb exceeds limits.
        ///   Ventilation reduces to outside ≈ inside conditions, with minimum base maintained.
        /// 
        /// - **tmmvn (Temperature + Minimum Mechanical Ventilation Neutral):**
        ///   Neutral version of tmmvc: includes base ventilation plus additional ventilation.
        ///   Ramps up airflow between lower and upper thresholds but maintains baseline even if conditions 
        ///   fall outside range.
        /// 
        /// - **tcbvc (Temperature + Pollutant Controlled Hybrid Ventilation):**
        ///   Hybrid control combining pollutant-triggered and temperature-triggered ventilation.
        ///   Allows seasonal cut-off (switch between Summer/Winter), supports daily scheduling 
        ///   (Day = 1 / Night = 0), and defines maximum allowable flow rates.
        ///   Summer prioritises temperature control with pollutant override; Winter prioritises pollutant only.
        /// 
        /// ### Key Features
        /// - **Dynamic control:** Adjusts airflow incrementally, not binary on/off.
        /// - **Base ventilation enforcement:** Ensures minimum flow rates even in neutral conditions.
        /// - **Hybrid logic:** Switches between pollutant-based and thermal-based criteria.
        /// - **Seasonal adaptability:** Different behaviours depending on Winter vs Summer.
        /// - **Scheduling integration:** Day/night ventilation strategies with optional overrides.
        /// 
        /// This component is intended for use in **parametric workflows**, enabling users to 
        /// analytically test how ventilation strategies impact comfort, indoor air quality, 
        /// and energy use under varying scenarios
        /// https://docs.edsl.net/9.5.6/TBD/Details/VentilationFunctions.html.
        /// </summary>
        public SAMAnalyticalModifyVentilationProfileByFunction()
        : base("SAMAnalytical.ModifyVentilationProfileByFunction",
               "SAMAnalytical.ModifyVentilationProfileByFunction",
               "Modify a zone’s ventilation profile by functional rules.\n" +
               "Supports temperature-controlled, pollutant-controlled, and hybrid strategies.\n" +
               "Ventilation ramps between lower/upper limits (ACH or L/s·m²) with optional base flow.\n" +
               "Cooling modes reduce flow when outdoor ≈ indoor, shutting off at outdoor ≥ indoor +1 °C.\n" +
               "Neutral modes omit cooling offset but maintain linear ramping.\n" +
               "Hybrid control allows pollutant triggers, seasonal cut-offs (Summer/Winter), and\n" +
               "day/night schedules. Ensures minimum ventilation is preserved while enabling\n" +
               "dynamic, parametric testing of comfort, air quality, and energy use.\n\n" +
               "Examples:\n" +
               " - tcmvc (Cooling): tcmvc,0,19.00,20.00,0,0.000\n" +
               " - tcmvn (Neutral): tcmvn,0,19.00,20.00,0,0.000\n" +
               " - tmmvc (Cooling+Min): tmmvc,0,19.00,20.00,0.200,0.000\n" +
               " - tmmvn (Neutral+Min): tmmvn,0,19.00,20.00,0.200,0.000\n" +
               " - tcbvc (Hybrid): tcbvc,0,19.00,20.00,0.200,19.00,20.00,30.000,0,0,50.000,\n" +
               "                   604.799,755.999,16.000,12.000,755.999,1209.598",
               "SAM", "Analytical02")

        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override GH_SAMParam[] Inputs
        {
            get
            {
                // Inputs
                var result = new List<GH_SAMParam>();

                result.Add(new GH_SAMParam(
                    new GooAnalyticalObjectParam()
                    {
                        Name = "_analyticalObject",
                        NickName = "_analyticalObject",
                        Description = "SAM AnalyticalModel or AdjacencyCluster.",
                        Access = GH_ParamAccess.item,
                        Optional = false
                    },
                    ParamVisibility.Binding));

                result.Add(new GH_SAMParam(
                    new GooAnalyticalObjectParam()
                    {
                        Name = "_spaces/zones",
                        NickName = "_spaces/zones",
                        Description = "SAM Space or Zone objects.",
                        Access = GH_ParamAccess.list,
                        Optional = true
                    },
                    ParamVisibility.Binding));

                result.Add(new GH_SAMParam(
                    new Param_String()
                    {
                        Name = "_function",
                        NickName = "_function",
                        Description =
                            "Ventilation function string.\n" +
                            "Examples:\n" +
                            "  tcmvc,0,19.00,20.00,0,0.000\n" +
                            "  tcmvn,0,19.00,20.00,0,0.000\n" +
                            "  tmmvc,0,19.00,20.00,0.200,0.000\n" +
                            "  tmmvn,0,19.00,20.00,0.200,0.000\n" +
                            "  tcbvc,0,19.00,20.00,0.200,19.00,20.00,30.000,0,0,50.000,604.799,755.999,16.000,12.000,755.999,1209.598",
                        Access = GH_ParamAccess.item,
                        Optional = false
                    },
                    ParamVisibility.Binding));

                result.Add(new GH_SAMParam(
                    new Param_Number()
                    {
                        Name = "ac/h_",
                        NickName = "ac/h_",
                        Description =
                            "Override ACH (air changes per hour).\n" +
                            "If supplied, replaces any 0.00 ACH token in the function string.\n" +
                            "Example: If ac/h_ = 0.3, then\n" +
                            "  tcmvc,0,19.00,20.00,0,0.000\n" +
                            "becomes\n" +
                            "  tcmvc,0,19.00,20.00,0,0.300",
                        Access = GH_ParamAccess.item,
                        Optional = true
                    },
                    ParamVisibility.Binding));

                result.Add(new GH_SAMParam(
                    new Param_Number()
                    {
                        Name = "m3/h_",
                        NickName = "m3/h_",
                        Description =
                            "Override airflow [m³/h]. This will be converted to ACH per space as:\n" +
                            "  ACH = (m³/h) / Volume[m³]\n" +
                            "If both ac/h_ and m3/h_ are provided, ac/h_ takes precedence.",
                        Access = GH_ParamAccess.item,
                        Optional = true
                    },
                    ParamVisibility.Binding));

                Param_Number param_Number;

                param_Number = new Param_Number() { Name = "_factor_", NickName = "_factor_", Description = "Factor", Access = GH_ParamAccess.item, Optional = true };
                param_Number.SetPersistentData(1.0);
                result.Add(new GH_SAMParam(param_Number, ParamVisibility.Binding));

                param_Number = new Param_Number() { Name = "_setback_", NickName = "_setback_", Description = "Setback", Access = GH_ParamAccess.item, Optional = true };
                param_Number.SetPersistentData(0.0);
                result.Add(new GH_SAMParam(param_Number, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new Param_String() { Name = "description_", NickName = "description_", Description = "Description", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new GooAnalyticalObjectParam() { Name = "AnalyticalObject", NickName = "AnalyticalObject", Description = "SAM AnalyticalObject", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooInternalConditionParam() { Name = "InternalConditions", NickName = "InternalConditions", Description = "SAM InternalConditions", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
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

            IAnalyticalObject analyticalObject = null;
            index = Params.IndexOfInputParam("_analyticalObject");
            if (index == -1 || !dataAccess.GetData(index, ref analyticalObject) || analyticalObject == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            string function = null;
            index = Params.IndexOfInputParam("_function");
            if (index == -1 || !dataAccess.GetData(index, ref function) || function == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            double ach = double.NaN;
            index = Params.IndexOfInputParam("ac/h_");
            if (index != -1)
            {
                dataAccess.GetData(index, ref ach);
            }

            double m3h = double.NaN;
            index = Params.IndexOfInputParam("m3/h_");
            if (index != -1)
            {
                dataAccess.GetData(index, ref m3h);
            }

            double factor = double.NaN;
            index = Params.IndexOfInputParam("_factor_");
            if (index != -1)
            {
                if (!dataAccess.GetData(index, ref factor))
                {
                    factor = 1.0;
                }
            }

            double setback = double.NaN;
            index = Params.IndexOfInputParam("_setback_");
            if (index != -1)
            {
                if (!dataAccess.GetData(index, ref setback))
                {
                    setback = 0.0;
                }
            }

            string description = null;
            index = Params.IndexOfInputParam("description_");
            if (index != -1)
            {
                if (!dataAccess.GetData(index, ref description))
                {
                    description = null;
                }
            }

            AdjacencyCluster adjacencyCluster = null;
            if (analyticalObject is AdjacencyCluster)
            {
                adjacencyCluster = (AdjacencyCluster)analyticalObject;
            }
            else if (analyticalObject is AnalyticalModel)
            {
                adjacencyCluster = ((AnalyticalModel)analyticalObject).AdjacencyCluster;
            }

            List<Space> spaces = [];

            List<InternalCondition> internalConditions = [];

            if (adjacencyCluster is not null)
            {
                adjacencyCluster = new AdjacencyCluster(adjacencyCluster, true);

                List<IAnalyticalObject> analyticalObjects = [];
                index = Params.IndexOfInputParam("_spaces/zones");
                if (index != -1 && dataAccess.GetDataList(index, analyticalObjects) && analyticalObjects != null)
                {
                    foreach (IAnalyticalObject analyticalObject_Temp in analyticalObjects)
                    {
                        if (analyticalObject_Temp is not SAMObject sAMObject)
                        {
                            continue;
                        }

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
                    {
                        continue;
                    }

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
                                int vent_Index = 3;

                                if (!double.IsNaN(ach))
                                {
                                    function_Temp[vent_Index] = ach;
                                }
                                else if (!double.IsNaN(m3h))
                                {
                                    double volume = space.Volume(adjacencyCluster);
                                    if (!double.IsNaN(volume))
                                    {
                                        function_Temp[vent_Index] = m3h / volume;
                                    }
                                }
                            }
                        }

                        internalCondition.SetValue(InternalConditionParameter.VentilationFunction, function_Temp?.ToString() ?? function);
                    }

                    if (description is null)
                    {
                        internalCondition.RemoveValue(InternalConditionParameter.VentilationFunctionDescription);
                    }
                    else
                    {
                        internalCondition.SetValue(InternalConditionParameter.VentilationFunctionDescription, description);
                    }

                    if (double.IsNaN(factor))
                    {
                        internalCondition.RemoveValue(InternalConditionParameter.VentilationFunctionFactor);
                    }
                    else
                    {
                        internalCondition.SetValue(InternalConditionParameter.VentilationFunctionFactor, factor);
                    }

                    if (double.IsNaN(setback))
                    {
                        internalCondition.RemoveValue(InternalConditionParameter.VentilationFunctionSetback);
                    }
                    else
                    {
                        internalCondition.SetValue(InternalConditionParameter.VentilationFunctionSetback, setback);
                    }

                    space.InternalCondition = internalCondition;

                    internalConditions.Add(internalCondition);

                    adjacencyCluster.AddObject(space);
                }
            }

            if (analyticalObject is AdjacencyCluster)
            {
                analyticalObject = adjacencyCluster;
            }
            else if (analyticalObject is AnalyticalModel)
            {
                analyticalObject = new AnalyticalModel((AnalyticalModel)analyticalObject, adjacencyCluster);
            }

            index = Params.IndexOfOutputParam("AnalyticalObject");
            if (index != -1)
            {
                dataAccess.SetData(index, analyticalObject);
            }

            index = Params.IndexOfOutputParam("InternalConditions");
            if (index != -1)
            {
                dataAccess.SetDataList(index, internalConditions);
            }
        }
    }
}