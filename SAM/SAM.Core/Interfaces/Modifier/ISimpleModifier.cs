namespace SAM.Core
{
    public interface ISimpleModifier : IModifier
    {
        ArithmeticOperator ArithmeticOperator { get; }
    }
}