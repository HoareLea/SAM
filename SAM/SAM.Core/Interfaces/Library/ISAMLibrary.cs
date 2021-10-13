namespace SAM.Core
{
    public interface ISAMLibrary : ISAMObject
    {
        System.Type GenericType { get; }
        bool Write(string path);
        bool Append(string path);
    }
}