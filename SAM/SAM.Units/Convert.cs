using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAM.Units
{   
    public static partial class Convert
    {
        public static double ByUnitType(double Value, UnitType from, UnitType to)
        {
            switch(from)
            {
                case UnitType.Meter:
                    switch(to)
                    {
                        case UnitType.Feet:
                            return Value * 3.280839895;
                    }
                    break;
                case UnitType.Feet:
                    switch (to)
                    {
                        case UnitType.Feet:
                            return Value * 0.3048;
                    }
                    break;
            }

            return double.NaN;
        }
    }
}
