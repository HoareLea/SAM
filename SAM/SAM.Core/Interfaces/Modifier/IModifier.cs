namespace SAM.Core
{
    public interface IModifier : IJSAMObject
    {
        bool ContainsIndex(int index);

        double GetCalculatedValue(int index, double value);
    }
}