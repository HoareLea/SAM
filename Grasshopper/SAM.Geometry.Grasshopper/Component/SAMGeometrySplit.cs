// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Core.Grasshopper;
using SAM.Geometry.Grasshopper.Properties;
using SAM.Geometry.Planar;
using System;
using System.Collections.Generic;

namespace SAM.Geometry.Grasshopper
{
    [Obsolete("Obsolete since 2022.08.24")]
    public class SAMGeometrySplit : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("5de5aeeb-7d6c-4598-bf45-df9d72edb8f5");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.1";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Geometry;

        public override GH_Exposure Exposure => GH_Exposure.tertiary | GH_Exposure.hidden;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMGeometrySplit()
          : base("SAMGeometry.Split", "SAMGeometry.Split",
              "Split Geometry or Segment2Ds",
              "SAM", "Geometry")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override GH_SAMParam[] Inputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();

                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_geometry", NickName = "Geo", Description = "Geometry", Access = GH_ParamAccess.list }, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Number param_Number;
                param_Number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_tolerance_", NickName = "tolrnc", Description = "Split Tolerance", Access = GH_ParamAccess.item, Optional = true };
                param_Number.SetPersistentData(Core.Tolerance.Distance);
                result.Add(new GH_SAMParam(param_Number, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Boolean param_Boolean;
                param_Boolean = new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "_run", NickName = "_run", Description = "Run", Access = GH_ParamAccess.item, Optional = true };
                param_Boolean.SetPersistentData(false);
                result.Add(new GH_SAMParam(param_Boolean, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "Geometry", NickName = "Geo", Description = "modified SAM Geometry", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "Successful", NickName = "Successful", Description = "Correctly imported?", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                return result.ToArray();
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

            int index_Successful = Params.IndexOfOutputParam("Successful");
            if (index_Successful != -1)
            {
                dataAccess.SetData(index_Successful, false);
            }

            bool run = false;
            index = Params.IndexOfInputParam("_run");
            if (index == -1 || !dataAccess.GetData(index, ref run))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }
            if (!run)
                return;

            double tolerance = Core.Tolerance.Distance;
            index = Params.IndexOfInputParam("_tolerance_");
            if (index == -1 || !dataAccess.GetData(index, ref tolerance))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<GH_ObjectWrapper> objectWrapperList = new List<GH_ObjectWrapper>();

            index = Params.IndexOfInputParam("_geometry");
            if (index == -1 || !dataAccess.GetDataList(index, objectWrapperList) || objectWrapperList == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Segment2D> segment2Ds = new List<Segment2D>();
            foreach (GH_ObjectWrapper gHObjectWrapper in objectWrapperList)
            {
                Segment2D segment2D = gHObjectWrapper.Value as Segment2D;
                if (segment2D != null)
                {
                    segment2Ds.Add(segment2D);
                    continue;
                }

                List<Spatial.ISAMGeometry3D> sAMGeometry3Ds = null;
                if (gHObjectWrapper.Value is GooSAMGeometry)
                {
                    sAMGeometry3Ds = new List<Spatial.ISAMGeometry3D>();

                    ISAMGeometry sAMGeometry = ((GooSAMGeometry)gHObjectWrapper.Value).Value;
                    if (sAMGeometry is Spatial.ISAMGeometry3D)
                        sAMGeometry3Ds.Add((Spatial.ISAMGeometry3D)sAMGeometry);
                    else if (sAMGeometry is ISAMGeometry2D)
                        sAMGeometry3Ds.Add(Spatial.Query.Convert(Spatial.Plane.WorldXY, (ISAMGeometry2D)sAMGeometry));
                }
                else if (gHObjectWrapper.Value is IGH_GeometricGoo)
                {
                    sAMGeometry3Ds = Convert.ToSAM((IGH_GeometricGoo)gHObjectWrapper.Value);
                }

                if (sAMGeometry3Ds != null && sAMGeometry3Ds.Count > 0)
                {
                    foreach (Spatial.ISAMGeometry3D sAMGeometry3D in sAMGeometry3Ds)
                    {
                        List<Spatial.ICurve3D> curve3Ds = new List<Spatial.ICurve3D>();
                        if (sAMGeometry3D is Spatial.ICurvable3D)
                            curve3Ds.AddRange(((Spatial.ICurvable3D)sAMGeometry3D).GetCurves().ConvertAll(x => Spatial.Query.Project(Spatial.Plane.WorldXY, x)));
                        else if (sAMGeometry3D is Spatial.ICurve3D)
                            curve3Ds.Add(Spatial.Query.Project(Spatial.Plane.WorldXY, (Spatial.ICurve3D)sAMGeometry3D));

                        if (curve3Ds == null || curve3Ds.Count == 0)
                            continue;

                        foreach (Spatial.ICurve3D curve3D in curve3Ds)
                        {
                            if (curve3D == null)
                                continue;

                            ICurvable2D curvable2D = Spatial.Query.Convert(Spatial.Plane.WorldXY, curve3D) as ICurvable2D;
                            if (curvable2D != null)
                                curvable2D.GetCurves().ForEach(x => segment2Ds.Add(new Segment2D(x.GetStart(), x.GetEnd())));
                        }
                    }
                }
            }

            index = Params.IndexOfOutputParam("Geometry");
            if (index != -1)
            {
                dataAccess.SetDataList(index, segment2Ds.Split(tolerance));
            }

            if (index_Successful != -1)
            {
                dataAccess.SetData(index_Successful, true);
            }
        }
    }
}
