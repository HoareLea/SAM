using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Rhino.Render.DataSources;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalCreateCaseByVentilation : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("fdc0efd6-3ce7-4fa7-b6f6-1fe4235c0639");

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
        public SAMAnalyticalCreateCaseByVentilation()
          : base("SAMAnalytical.CreateCaseByVentilation", "SAMAnalytical.CreateCaseByVentilation",
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
                            "Defines the zone’s ventilation behavior (temperature-, pollutant-, or hybrid-controlled).\n" +
                            "For TAS 9.5.7 or higher, a fixed supply air temperature can be specified as the last value\n" +
                            "in the function string. If this option is disabled, the last value should remain -50.00\n" +
                            "(indicating 'off'). For example, setting it to 12.00 will fix the supply air temperature\n" +
                            "at 12 °C.\n\n" +
                            "Examples:\n" +
                            "  tcmvc,0,19.00,20.00,0.000,-50.00\n" +
                            "  tcmvn,0,19.00,20.00,0.000,-50.00\n" +
                            "  tmmvc,0,19.00,20.00,0.200,0.000,-50.00\n" +
                            "  tmmvn,0,19.00,20.00,0.200,0.000,-50.00\n" +
                            "  tcbvc,0,19.00,20.00,0.200,19.00,20.00,30.000,0,0,50.000,\n" +
                            "         604.799,755.999,16.000,12.000,755.999,1209.598",
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
                            "Optional Override ACH (air changes per hour).\n" +
                            "If provided, his value replaces any 0.000 ACH token in the function string.\n" +
                            "Example:\n" +
                            "  Input:  tcmvc,0,19.00,20.00,0,0.000,-50.00\n" +
                            "  With ac/h_ = 0.3 → becomes:\n" +
                            "  tcmvc,0,19.00,20.00,0,0.300,-50.00",
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

            AdjacencyCluster adjacencyCluster = analyticalModel.AdjacencyCluster;

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
                                        function_Temp[vent_Index] = Core.Query.Round(m3h / volume, Tolerance.MacroDistance);
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

            if(adjacencyCluster != null)
            {
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