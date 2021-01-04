using Grasshopper;

namespace SAM.Core.Grasshopper
{
    public static partial class Query
    {
        public static object[,] Objects<T>(DataTree<T> dataTree) where T: global::Grasshopper.Kernel.Types.IGH_Goo
        {
            if (dataTree == null)
                return null;

            throw new System.NotImplementedException();
        }
    }
}