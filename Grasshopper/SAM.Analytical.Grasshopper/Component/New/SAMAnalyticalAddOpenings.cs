using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalAddOpenings : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("56ebc811-a6f3-409c-810b-289d4543fdba");

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
        public SAMAnalyticalAddOpenings()
          : base("SAMAnalytical.AddOpenings", "SAMAnalytical.AddOpenings",
              "Add Openings to ",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddParameter(new GooAnalyticalObjectParam(), "_analyticalObject", "_analyticalObject", "SAM Analytical Object", GH_ParamAccess.item);
            inputParamManager.AddParameter(new GooOpeningParam(), "_openings", "_openings", "SAM Analytical Opening", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooAnalyticalObjectParam(), "analyticalObject", "analyticalObject", "SAM Analytical Object", GH_ParamAccess.item);
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

            List<IOpening> openings = new List<IOpening>();
            if(!dataAccess.GetDataList(1, openings) || openings == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            SAMObject sAMObject = null;
            if (!dataAccess.GetData(0, ref sAMObject))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<IOpening> openings_Added = new List<IOpening>();
            if (sAMObject is IHostPartition)
            {
                IHostPartition hostPartition = (IHostPartition)sAMObject.Clone();
                
                foreach(IOpening opening in openings)
                {
                    if(hostPartition.TryAddOpening(opening))
                    {
                        openings_Added.Add(opening);
                    }
                }

                dataAccess.SetData(0, hostPartition);
                dataAccess.SetDataList(1, openings_Added?.ConvertAll(x => new GooOpening(x)));
                dataAccess.SetData(2, openings_Added != null && openings_Added.Count != 0);
            }
            else if(sAMObject is ArchitecturalModel)
            {
                ArchitecturalModel architecturalModel = new ArchitecturalModel((ArchitecturalModel)sAMObject);

                openings_Added = architecturalModel.AddOpenings(openings);

                dataAccess.SetData(0, new GooArchitecturalModel(architecturalModel));
                dataAccess.SetDataList(1, openings_Added.ConvertAll(x => new GooOpening(x)));
                dataAccess.SetData(2, openings_Added != null && openings_Added.Count != 0);
            }
        }
    }
}