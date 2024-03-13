using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalCheck : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("8d19abcc-8cf8-4987-905d-2cbb24124527");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.2";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalCheck()
          : base("SAMAnalytical.Check", "SAMAnalytical.Check",
              "Check Document against analytical object ie. AdjacencyCluster or Analytical Model",
              "SAM", "Analytical")
        {
        }

        protected override GH_SAMParam[] Inputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new GooAnalyticalObjectParam() { Name = "_analytical", NickName = "_analytical", Description = "SAM Analytical Object such as AdjacencyCluster or AnalyticalModel", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Boolean @boolean = null;
                @boolean = new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "_run", NickName = "_run", Description = "Connect a boolean toggle to run.", Access = GH_ParamAccess.item };
                @boolean.SetPersistentData(false);
                result.Add(new GH_SAMParam(@boolean, ParamVisibility.Binding));
                
                return result.ToArray();
            }
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override GH_SAMParam[] Outputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new GooLogParam() { Name = "errors", NickName = "errors", Description = "SAM Log Errors and Warnings", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooLogParam() { Name = "messages", NickName = "messages", Description = "SAM Log Messages", Access = GH_ParamAccess.item }, ParamVisibility.Voluntary));
                return result.ToArray();
            }
        }

        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            bool run = false;
            if (!dataAccess.GetData(1, ref run) || !run)
                return;

            IAnalyticalObject analyticalObject = null;
            if (!dataAccess.GetData(0, ref analyticalObject))
                return;

            int index_Errors = Params.IndexOfOutputParam("errors");
            int index_Messages = Params.IndexOfOutputParam("messages");

            Log log = null;
            try
            {
                log = Create.Log(analyticalObject as dynamic);
            }
            catch 
            {
                if(index_Errors != -1)
                {
                    dataAccess.SetData(index_Errors, null);
                }

                if (index_Messages != -1)
                {
                    dataAccess.SetData(index_Messages, null);
                }

                return;
            }



            if (log == null)
            {
                log = new Log();
            }

            if (log.Count() == 0)
            {
                log.Add("All good! You can switch off your computer and go home now.");
            }

            if (index_Errors != -1)
            {
                dataAccess.SetData(index_Errors, log.Filter(new LogRecordType[] { LogRecordType.Error, LogRecordType.Warning, LogRecordType.Undefined }));
            }

            if (index_Messages != -1)
            {
                dataAccess.SetData(index_Messages, log.Filter(new LogRecordType[] { LogRecordType.Message }));
            }
        }
    }
}