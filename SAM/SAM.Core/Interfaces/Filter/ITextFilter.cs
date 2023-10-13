namespace SAM.Core
{
    public interface ITextFilter : IFilter
    {
        public bool CaseSensitive { get; set; }

        public TextComparisonType TextComparisonType { get; set; }

        public string Value { get; set; }

    }
}