namespace SAM.Core
{
    public interface IIndexedModifier : IModifier
    {
        bool ContainsIndex(int index);

        double GetCalculatedValue(int index, double value);
    }
}