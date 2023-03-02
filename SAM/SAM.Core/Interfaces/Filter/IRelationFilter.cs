namespace SAM.Core
{
    public interface IRelationFilter : IFilter
    {
        IFilter Filter { get; set; }
    }
}