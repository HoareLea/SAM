// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Core.Grasshopper;
using SAM.Geometry.Grasshopper.Properties;
using SAM.Geometry.Planar;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;

namespace SAM.Geometry.Grasshopper
{
    /// <summary>
    /// The SAMGeometry2DToNTS class is a component that converts SAM 2D geometries to NetTopologySuite (NTS) geometries.
    /// It inherits from the GH_SAMVariableOutputParameterComponent base class.
    /// </summary>
    public class SAMGeometry2DToNTS : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("0919dca3-ca2b-4783-acd3-847d5bff4a46");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.2";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Geometry;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMGeometry2DToNTS()
          : base("SAMGeometry2D.ToNTS", "SAMGeometry2D.ToNTS",
              "SAMGeometry To NetTopologySuite",
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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_geometry", NickName = "Geo", Description = "Geometry", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "nTSGeometries", NickName = "nTSGeometries", Description = "NTS Geometries", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "nTS Text", NickName = "nTS Text", Description = "Text", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "successful", NickName = "successful", Description = "Correctly imported?", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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
            int index;

            GH_ObjectWrapper objectWrapper = null;
            index = Params.IndexOfInputParam("_geometry");
            if (index == -1 || !dataAccess.GetData(index, ref objectWrapper) || objectWrapper == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                index = Params.IndexOfOutputParam("nTS Text");
                if (index != -1)
                {
                    dataAccess.SetData(index, false);
                }
                return;
            }

            List<ISAMGeometry2D> sAMGeometry2Ds = new List<ISAMGeometry2D>();

            List<ISAMGeometry3D> sAMGeometry3Ds = null;
            if (objectWrapper.Value is GooSAMGeometry)
            {
                sAMGeometry3Ds = new List<ISAMGeometry3D>();

                ISAMGeometry sAMGeometry = ((GooSAMGeometry)objectWrapper.Value).Value;
                if (sAMGeometry is ISAMGeometry3D)
                {
                    sAMGeometry3Ds.Add((ISAMGeometry3D)sAMGeometry);
                }
                else if (sAMGeometry is ISAMGeometry2D)
                {
                    sAMGeometry2Ds.Add((ISAMGeometry2D)sAMGeometry);
                }
            }
            else if (objectWrapper.Value is IGH_GeometricGoo)
            {
                sAMGeometry3Ds = Convert.ToSAM((IGH_GeometricGoo)objectWrapper.Value);
            }
            else if (objectWrapper.Value is ISAMGeometry3D)
            {
                sAMGeometry3Ds = new List<ISAMGeometry3D>() { (ISAMGeometry3D)objectWrapper.Value };
            }
            else if (objectWrapper.Value is ISAMGeometry2D)
            {
                sAMGeometry2Ds.Add((ISAMGeometry2D)objectWrapper.Value);
            }

            if (sAMGeometry3Ds != null && sAMGeometry3Ds.Count > 0)
            {
                foreach (ISAMGeometry3D sAMGeometry3D in sAMGeometry3Ds)
                {
                    if (sAMGeometry3D is Face3D)
                    {
                        sAMGeometry2Ds.Add(Spatial.Query.Convert(Plane.WorldXY, (Face3D)sAMGeometry3D));
                    }
                    else
                    {
                        List<ICurve3D> curve3Ds = new List<ICurve3D>();
                        if (sAMGeometry3D is ICurvable3D)
                            curve3Ds.AddRange(((ICurvable3D)sAMGeometry3D).GetCurves().ConvertAll(x => Spatial.Query.Project(Plane.WorldXY, x)));
                        else if (sAMGeometry3D is ICurve3D)
                            curve3Ds.Add(Spatial.Query.Project(Plane.WorldXY, (ICurve3D)sAMGeometry3D));

                        if (curve3Ds == null || curve3Ds.Count == 0)
                            continue;

                        foreach (ICurve3D curve3D in curve3Ds)
                        {
                            if (curve3D == null)
                                continue;

                            ICurvable2D curvable2D = Spatial.Query.Convert(Plane.WorldXY, curve3D) as ICurvable2D;
                            if (curvable2D != null)
                                sAMGeometry2Ds.Add(curvable2D);
                        }
                    }
                }
            }

            List<NetTopologySuite.Geometries.Geometry> geometries = sAMGeometry2Ds.ConvertAll(x => x.ToNTS());

            index = Params.IndexOfOutputParam("nTSGeometries");
            if (index != -1)
            {
                dataAccess.SetDataList(index, geometries);
            }

            index = Params.IndexOfOutputParam("nTS Text");
            if (index != -1)
            {
                dataAccess.SetDataList(index, geometries.ConvertAll(x => x.ToString()));
            }

            index = Params.IndexOfOutputParam("successful");
            if (index != -1)
            {
                dataAccess.SetData(index, true);
            }
        }
    }
}
