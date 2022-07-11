using SAM.Core;
using System.Collections.Generic;

namespace SAM.Math
{
    public interface IEquation : IJSAMObject
    {
        double Evaluate(double value);

        List<double> Evaluate(IEnumerable<double> values);
    }
}
