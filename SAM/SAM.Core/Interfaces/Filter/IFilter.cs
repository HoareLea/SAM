namespace SAM.Core
{
    public interface IFilter : IJSAMObject
    {
        bool Inverted { get; set; }

        bool IsValid(IJSAMObject jSAMObject);

    }
}