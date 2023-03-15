namespace SAM.Core
{
    public static partial class Modify
    {
        public static bool UpdateByType(this IRelationFilter relationFilter, System.Type type, IFilter filter)
        {
            if(relationFilter == null || type == null)
            {
                return false;
            }

            if(type.IsAssignableFrom(relationFilter.GetType()))
            {
                relationFilter.Filter = filter;
                return true;
            }

            IFilter result = relationFilter.Filter;
            if(result == null)
            {
                return false;
            }

            return UpdateByType(result as IRelationFilter, type, filter);
        }
    }
}