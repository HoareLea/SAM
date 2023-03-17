namespace SAM.Core
{
    public interface IMultiRelationFilter : IRelationFilter
    {
        FilterLogicalOperator FilterLogicalOperator { get; set; }
    }
}