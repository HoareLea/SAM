using Grasshopper.Kernel;
using SAM.Architectural.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Architectural.Grasshopper
{
    public class SAMArchitecturalAddOpeningsByAzimuth : GH_SAMComponent
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
        public SAMArchitecturalAddOpeningsByAzimuth()
          : base("SAMArchitectural.AddOpeningsByAzimuth", "SAMArchitectural.AddOpeningsByAzimuth",
              "Add Openings to ",
              "SAM", "Architectural")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddParameter(new GooArchitecturalObjectParam(), "_architecturalObject", "_architecturalObject", "SAM Architectural Object", GH_ParamAccess.item);
            inputParamManager.AddNumberParameter("_ratios", "_ratios", "Ratios", GH_ParamAccess.list);
            inputParamManager.AddIntervalParameter("_azimuths", "_azimuths", "Azimuths Domains/Intervals if single number given ie. 90 it will be 0 to 90, so you need to make 90 To 90 in case just signle angle is required", GH_ParamAccess.list);

            int index = inputParamManager.AddParameter(new GooOpeningTypeParam(), "_openingType_", "_openingType_", "SAM Architectural OpeningType", GH_ParamAccess.item);
            inputParamManager[index].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooArchitecturalObjectParam(), "architecturalObject", "architecturalObject", "SAM Architectural ArchitecturalObject", GH_ParamAccess.list);
            outputParamManager.AddParameter(new GooOpeningParam(), "openings", "openings", "SAM Architectural Openings", GH_ParamAccess.list);
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

            if (openingType == null)
            {
                openingType = Architectural.Query.DefaultOpeningType(OpeningAnalyticalType.Window);
            }
                

            List<double> ratios = new List<double>();
            if (!dataAccess.GetDataList(1, ratios) || ratios == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Rhino.Geometry.Interval> azimuths = new List<Rhino.Geometry.Interval>();
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

            Dictionary<Rhino.Geometry.Interval, double> dictionary = new Dictionary<Rhino.Geometry.Interval, double>();
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

                IOpening opening = hostPartition.AddOpening(openingType, ratio);

                List<IOpening> openings = opening == null ? null : new List<IOpening>() { opening };


                dataAccess.SetData(0, hostPartition);
                dataAccess.SetDataList(1, openings?.ConvertAll(x => new GooOpening(x)));
                dataAccess.SetData(2, true);
            }
            else if(sAMObject is ArchitecturalModel)
            {
                ArchitecturalModel architecturalModel = new ArchitecturalModel((ArchitecturalModel)sAMObject);
                List<Wall> walls = architecturalModel.GetObjects(new Func<Wall, bool>((Wall wall) => architecturalModel.External(wall)));
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

                        IOpening opening = wall.AddOpening(openingType, ratio);

                        if(wall.AddOpening(opening))
                        {
                            openings.Add(opening);
                        }
                    }

                    dataAccess.SetData(0, new GooArchitecturalModel(architecturalModel));
                    dataAccess.SetDataList(1, openings.ConvertAll(x => new GooOpening(x)));
                    dataAccess.SetData(2, true);
                }
            }
        }
    }
}