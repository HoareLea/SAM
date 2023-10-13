namespace SAM.Core
{
    public interface INumberFilter : IFilter
    {
        public NumberComparisonType NumberComparisonType { get; set; }

        public double Value { get; set; }
    }
}