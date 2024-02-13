using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalUpdatePollutantGains : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("44835842-bd3c-498c-b2aa-cd483420d8a9");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.1";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public override GH_Exposure Exposure => GH_Exposure.primary;

        protected override GH_SAMParam[] Inputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new GooAnalyticalModelParam() { Name = "_analyticalModel", NickName = "_analyticalModel", Description = "SAM Analytical AnalyticalModel", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooSpaceParam() { Name = "_spaces_", NickName = "_spaces_", Description = "SAM Analytical Spaces, if nothing connected all spaces from AnalyticalModel will be used", Access = GH_ParamAccess.list, Optional = true}, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooProfileParam() { Name = "profile_", NickName = "profile_", Description = "SAM Analytical Profile", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Number number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_pollutantGPerPerson_", NickName = "_pollutantGPerPerson_", Description = "Pollutant Per Person [g/h/p]", Access = GH_ParamAccess.item };
                number.SetPersistentData(28.7);
                result.Add( new GH_SAMParam(number, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "pollutantGPerHrPerPerson_", NickName = "pollutantGPerHrPerPerson_", Description = "Pollutant Per Hour Per Person [g/h/m2]", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Voluntary));
                return result.ToArray();
            }
        }

        protected override GH_SAMParam[] Outputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new GooAnalyticalModelParam() {Name = "AnalyticalModel", NickName = "AnalyticalModel", Description = "SAM Analytical AnalyticalModel", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooInternalConditionParam() { Name = "InternalConditions", NickName = "InternalConditions", Description = "SAM Analytical InternalConditions", Access = GH_ParamAccess.list }, ParamVisibility.Voluntary));
                return result.ToArray();
            }
        }

        /// <summary>
        /// Updates PanelTypes for AdjacencyCluster
        /// </summary>
        public SAMAnalyticalUpdatePollutantGains()
          : base("SAMAnalytical.UpdatePollutantGains", "SAMAnalytical.UpdatePollutantGains",
              "Updates Pollutant Gains Properties for Spaces"+
              "\n*As the whole-body emission rates above were derived from Caucasian volunteers in Denmark, "+
              "\nwe estimate the total metabolic CO2 and CH4 emissions for the Danish Caucasian population using a simple calculation based on the obtained emission rates." +
              "\nWe assumed a population of 5.1 million (Caucasians in Denmark; The World Factbook at cia.gov) " +
              "\nemitting CO2 at a whole-body CO2 emission rate of 28.7 g/h/person (range 25.16–32.11)." +
              "\nThe whole-body CH4 emission rate for non-CH4 producers was assumed to be 1.31 mg/h/person (range 0.87–1.75) " +
              "\nbased on groups A1 and A3 in the ICHEAR study. A ten times higher rate was used for CH4 producers. " +
              "\nMengze Li, Gabriel Bekö, Nora Zannoni, Giovanni Pugliese, Mariana Carrito, Nicoletta Cera, Catarina Moura, Pawel Wargocki, Priscila Vasconcelos, Pedro Nobre, Nijing Wang, Lisa Ernle, Jonathan Williams," +
              "\nHuman metabolic emissions of carbon dioxide and methane and their implications for carbon emissions," +
              "\nScience of The Total Environment," +
              "\nVolume 833," +
              "\n2022," +
              "\n155241," +
              "\nISSN 0048 - 9697," +
              "\nhttps://doi.org/10.1016/j.scitotenv.2022.155241." +
              "\n(https://www.sciencedirect.com/science/article/pii/S0048969722023348)" +
              "\nAbstract: Carbon dioxide(CO2) and methane(CH4) are important greenhouse gases in the atmosphere and have large impacts on Earth's radiative forcing and climate. Their natural and anthropogenic emissions have often been in focus, while the role of human metabolic emissions has received less attention. In this study, exhaled, dermal and whole-body CO2 and CH4 emission rates from a total of 20 volunteers were quantified under various controlled environmental conditions in a climate chamber. The whole-body CO2 emissions increased with temperature. Individual differences were the most important factor for the whole-body CH4 emissions. Dermal emissions of CO2 and CH4 only contributed ~3.5% and ~5.5% to the whole-body emissions, respectively. Breath measurements conducted on 24 volunteers in a companion study identified one third of the volunteers as CH4 producers (exhaled CH4 exceeded 1 ppm above ambient level). The exhaled CH4 emission rate of these CH4 producers (4.03 ± 0.71 mg/h/person, mean ± one standard deviation) was ten times higher than that of the rest of the volunteers (non-CH4 producers; 0.41 ± 0.45 mg/h/person). With increasing global population and the expected large reduction in global anthropogenic carbon emissions in the next decades, metabolic emissions of CH4 (although not CO2) from humans may play an increasing role in regional and global carbon budgets.",

              "SAM", "Analytical")
        {
        }
    protected override void SolveInstance(IGH_DataAccess dataAccess)
    {
        int index;
        index = Params.IndexOfInputParam("_analyticalModel");
        if(index == -1)
        {
            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
            return;
        }
        AnalyticalModel analyticalModel = null;
        if (!dataAccess.GetData(index, ref analyticalModel) || analyticalModel == null)
        {
            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
            return;
        }
        AdjacencyCluster adjacencyCluster = analyticalModel.AdjacencyCluster;
        List<Space> spaces = null;
        index = Params.IndexOfInputParam("_spaces_");
        if(index != -1)
        {
            spaces = new List<Space>();
            dataAccess.GetDataList(index, spaces);
            if (spaces != null && spaces.Count == 0)
                spaces = null;
        }
        if (spaces == null)
            spaces = analyticalModel.GetSpaces();
        Profile profile = null;
        index = Params.IndexOfInputParam("profile_");
        if (index != -1)
            dataAccess.GetData(index, ref profile);
        double pollutantGenerationPerPerson = double.NaN;
        index = Params.IndexOfInputParam("_pollutantGPerPerson_");
        if (index != -1)
            dataAccess.GetData(index, ref pollutantGenerationPerPerson);
        double pollutantGenerationPerArea = double.NaN;
        index = Params.IndexOfInputParam("pollutantGPerHrPerPerson_");
        if (index != -1)
            dataAccess.GetData(index, ref pollutantGenerationPerArea);
        ProfileLibrary profileLibrary = analyticalModel.ProfileLibrary;
        if (profile != null)
            profileLibrary.Add(profile);
        List<InternalCondition> internalConditions = new List<InternalCondition>();
        foreach(Space space in spaces)
        {
            if (space == null)
                continue;
            Space space_Temp = adjacencyCluster.SAMObject<Space>(space.Guid);
            if (space_Temp == null)
                continue;

                space_Temp = new Space(space_Temp);

                InternalCondition internalCondition = space_Temp.InternalCondition;
                if(internalCondition == null)
                    internalCondition = new InternalCondition(space_Temp.Name);

                if (profile != null)
                    internalCondition.SetValue(InternalConditionParameter.PollutantProfileName, profile.Name);

                if(!double.IsNaN(pollutantGenerationPerPerson))
                    internalCondition.SetValue(InternalConditionParameter.PollutantGenerationPerPerson, pollutantGenerationPerPerson);

                if (!double.IsNaN(pollutantGenerationPerArea))
                    internalCondition.SetValue(InternalConditionParameter.PollutantGenerationPerArea, pollutantGenerationPerArea);

                space_Temp.InternalCondition = internalCondition;
                internalConditions.Add(internalCondition);
                adjacencyCluster.AddObject(space_Temp);
            }

            index = Params.IndexOfOutputParam("AnalyticalModel");
            if(index != -1)
            {
                analyticalModel = new AnalyticalModel(analyticalModel, adjacencyCluster, analyticalModel.MaterialLibrary, profileLibrary);
                dataAccess.SetData(index, new GooAnalyticalModel(analyticalModel));
            }

            index = Params.IndexOfOutputParam("InternalConditions");
            if (index != -1)
                dataAccess.SetDataList(index, internalConditions?.ConvertAll(x => new GooInternalCondition(x)));
        }
    }
}