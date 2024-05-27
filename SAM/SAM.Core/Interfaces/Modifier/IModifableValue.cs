namespace SAM.Core
{
    public interface IModifiableValue : IJSAMObject
    {
        double Value { get; }

        ISimpleModifier Modifier { get; }

        double GetCalculatedValue(int index);
    }
}