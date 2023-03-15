namespace SAM.Core
{
    public static partial class Query
    {
        public static IFilter BaseFilter(this IRelationFilter relationFilter)
        {
            IRelationFilter relationFilter_Temp = relationFilter.Filter as IRelationFilter;
            if (relationFilter_Temp == null)
            {
                return relationFilter.Filter;
            }

            return BaseFilter(relationFilter_Temp);
        }
    }
}