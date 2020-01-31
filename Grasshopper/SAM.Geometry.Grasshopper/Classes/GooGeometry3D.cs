using System;
using System.Collections.Generic;
using System.Linq;

using GH_IO.Serialization;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using SAM.Geometry.Grasshopper.Properties;

namespace SAM.Geometry.Grasshopper
{
    public class GooGeometry3D : GooGeometry<Spatial.IGeometry3D>, IGH_PreviewData
    {
        public GooGeometry3D()
            : base()
        {

        }
        
        public GooGeometry3D(Spatial.IGeometry3D geometry)
        {
            Value = geometry;
        }

        public BoundingBox ClippingBox
        {
            get
            {
                if (Value is Spatial.IBoundable3D)
                    return ((Spatial.IBoundable3D)Value).GetBoundingBox().ToRhino();

                throw new NotImplementedException();
            }
        }

        public override IGH_Goo Duplicate()
        {
            return new GooGeometry3D(Value);
        }
    }

    public class GooGeometry3DParam : GH_PersistentParam<GooGeometry3D>
    {
        public override Guid ComponentGuid => new Guid("0baffeac-1d53-49e0-bb65-81d332483e42");
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Geometry;

        public GooGeometry3DParam()
            : base(typeof(Spatial.IGeometry3D).Name, typeof(Spatial.IGeometry3D).Name, typeof(Spatial.IGeometry3D).FullName.Replace(".", " "), "Params", "SAM")
        {

        }

        protected override GH_GetterResult Prompt_Plural(ref List<GooGeometry3D> values)
        {
            throw new NotImplementedException();
        }

        protected override GH_GetterResult Prompt_Singular(ref GooGeometry3D value)
        {
            throw new NotImplementedException();
        }
    }
}
