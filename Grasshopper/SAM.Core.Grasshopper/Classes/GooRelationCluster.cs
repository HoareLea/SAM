using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using SAM.Core.Grasshopper.Properties;
using System;
using System.Collections.Generic;

namespace SAM.Core.Grasshopper
{
    public class GooRelationCluster : GooSAMObject<RelationCluster>
    {
        public GooRelationCluster()
            : base()
        {
        }

        public GooRelationCluster(RelationCluster relationCluster)
            : base(relationCluster)
        {
        }

        public BoundingBox ClippingBox
        {
            get
            {
                return BoundingBox.Empty;
            }
        }

        public override IGH_Goo Duplicate()
        {
            return new GooRelationCluster(Value);
        }
    }

    //Params Components -> SAM used for internalizing data
    public class GooRelationClusterParam : GH_PersistentParam<GooRelationCluster>
    {
        public override Guid ComponentGuid => new Guid("8cc38f9b-506d-4b2a-8f80-c1119fac105a");

        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        //Here we control name, nickname, description, category, sub-category as deafult we use typeofclass name
        public GooRelationClusterParam()
            : base(typeof(RelationCluster).Name, typeof(RelationCluster).Name, typeof(RelationCluster).FullName.Replace(".", " "), "Params", "SAM")
        {
        }

        protected override GH_GetterResult Prompt_Plural(ref List<GooRelationCluster> values)
        {
            throw new NotImplementedException();
        }

        protected override GH_GetterResult Prompt_Singular(ref GooRelationCluster value)
        {
            throw new NotImplementedException();
        }
    }
}