using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalAddOpeningsByAzimuth : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("44af6249-36e0-4e1f-8e9b-ed4b8bb3c6b9");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalAddOpeningsByAzimuth()
          : base("SAMAnalytical.AddOpeningsByAzimuth", "SAMAnalytical.AddOpeningsByAzimuth",
              "Add Openings to ",
              "SAM WIP", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddParameter(new GooAnalyticalObjectParam(), "_analyticalObject", "_analyticalObject", "SAM Analytical Object", GH_ParamAccess.item);
            inputParamManager.AddNumberParameter("_ratios", "_ratios", "Ratios", GH_ParamAccess.list);
            inputParamManager.AddIntervalParameter("_azimuths", "_azimuths", "Azimuths Domains/Intervals if single number given ie. 90 it will be 0 to 90, so you need to make 90 To 90 in case just signle angle is required", GH_ParamAccess.list);

            int index = inputParamManager.AddParameter(new GooOpeningTypeParam(), "_openingType_", "_openingType_", "SAM Analytical OpeningType", GH_ParamAccess.item);
            inputParamManager[index].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooAnalyticalObjectParam(), "analyticalObject", "analyticalObject", "SAM Analytical Object", GH_ParamAccess.item);
            outputParamManager.AddParameter(new GooOpeningParam(), "openings", "openings", "SAM Analytical Openings", GH_ParamAccess.list);
            outputParamManager.AddBooleanParameter("Successful", "Successful", "Successful", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        ///// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            dataAccess.SetData(2, false);

            OpeningType openingType = null;
            dataAccess.GetData(3, ref openingType);

            OpeningTypeLibrary openingTypeLibrary = Analytical.Query.DefaultOpeningTypeLibrary();

            List<double> ratios = new List<double>();
            if (!dataAccess.GetDataList(1, ratios) || ratios == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<global::Rhino.Geometry.Interval> azimuths = new List<global::Rhino.Geometry.Interval>();
            if (!dataAccess.GetDataList(2, azimuths) || azimuths == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            if(azimuths.Count != ratios.Count)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            Dictionary<global::Rhino.Geometry.Interval, double> dictionary = new Dictionary<global::Rhino.Geometry.Interval, double>();
            for (int i = 0; i < ratios.Count; i++)
                if (azimuths[i] != null)
                    dictionary[azimuths[i]] = ratios[i];

            SAMObject sAMObject = null;
            if (!dataAccess.GetData(0, ref sAMObject))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            if (sAMObject is IHostPartition)
            {
                IHostPartition hostPartition = (IHostPartition)sAMObject.Clone();

                double azimuth = hostPartition.Azimuth();
                if (double.IsNaN(azimuth))
                    return;

                double ratio;
                if (!Core.Grasshopper.Query.TryGetValue(dictionary, azimuth, out ratio))
                    return;

                OpeningType openingType_Temp = openingType;
                if(openingType_Temp == null)
                {
                    openingType_Temp = openingTypeLibrary.GetOpeningTypes(OpeningAnalyticalType.Window, hostPartition.HostPartitionCategory())?.FirstOrDefault();
                    if (openingType_Temp == null)
                    {
                        openingType_Temp = openingTypeLibrary.GetOpeningTypes(OpeningAnalyticalType.Window)?.FirstOrDefault();
                    }
                }

                if (openingType_Temp == null)
                {
                    return;
                }

                IOpening opening = hostPartition.AddOpening(openingType_Temp, ratio);

                List<IOpening> openings = opening == null ? null : new List<IOpening>() { opening };


                dataAccess.SetData(0, hostPartition);
                dataAccess.SetDataList(1, openings?.ConvertAll(x => new GooOpening(x)));
                dataAccess.SetData(2, true);
            }
            else if(sAMObject is BuildingModel)
            {
                BuildingModel buildingModel = new BuildingModel((BuildingModel)sAMObject);
                List<Wall> walls = buildingModel.GetObjects(new Func<Wall, bool>((Wall wall) => buildingModel.External(wall)));
                if(walls != null)
                {
                    List<IOpening> openings = new List<IOpening>();
                    foreach(Wall wall in walls)
                    {
                        double azimuth = wall.Azimuth();
                        if (double.IsNaN(azimuth))
                            continue;

                        double ratio;
                        if (!Core.Grasshopper.Query.TryGetValue(dictionary, azimuth, out ratio))
                            continue;

                        OpeningType openingType_Temp = openingType;
                        if (openingType_Temp == null)
                        {
                            openingType_Temp = openingTypeLibrary.GetOpeningTypes(OpeningAnalyticalType.Window, buildingModel.PartitionAnalyticalType(wall))?.FirstOrDefault();
                            if (openingType_Temp == null)
                            {
                                openingType_Temp = openingTypeLibrary.GetOpeningTypes(OpeningAnalyticalType.Window)?.FirstOrDefault();
                            }
                        }

                        IOpening opening = wall.AddOpening(openingType_Temp, ratio);
                        if(opening != null)
                        {
                            openings.Add(opening);
                        }
                    }

                    dataAccess.SetData(0, new GooBuildingModel(buildingModel));
                    dataAccess.SetDataList(1, openings.ConvertAll(x => new GooOpening(x)));
                    dataAccess.SetData(2, true);
                }
            }
        }
    }
}