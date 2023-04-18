
namespace SAM.Analytical
{
    public interface IOpeningProperties : Core.IParameterizedSAMObject, IAnalyticalObject
    {
        double GetDischargeCoefficient();
    }
}
