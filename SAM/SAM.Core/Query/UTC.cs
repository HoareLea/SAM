namespace SAM.Core
{
    public static partial class Query
    {
        public static UTC UTC(float value)
        {
            return UTC(System.Convert.ToDouble(value));
        }
        
        public static UTC UTC(double value)
        {
            if(double.IsNaN(value))
            {
                return Core.UTC.Undefined;
            }

            switch(value)
            {
                case -12.0:
                    return Core.UTC.Minus1200;

                case -11.0:
                    return Core.UTC.Minus1100;

                case -10.0:
                    return Core.UTC.Minus1000;

                case -9.5:
                    return Core.UTC.Minus0930;

                case -8.0:
                    return Core.UTC.Minus0800;

                case -7.0:
                    return Core.UTC.Minus0700;

                case -6.0:
                    return Core.UTC.Minus0600;

                case -5.0:
                    return Core.UTC.Minus0500;

                case -4.0:
                    return Core.UTC.Minus0400;

                case -3.5:
                    return Core.UTC.Minus0330;

                case -3.0:
                    return Core.UTC.Minus0300;

                case -2.0:
                    return Core.UTC.Minus0200;

                case -1.0:
                    return Core.UTC.Minus0100;

                case 0.0:
                    return Core.UTC.PlusMinus0000;

                case 1.0:
                    return Core.UTC.Plus0100;

                case 2.0:
                    return Core.UTC.Plus0200;

                case 3.0:
                    return Core.UTC.Plus0300;

                case 3.5:
                    return Core.UTC.Plus0330;

                case 4.0:
                    return Core.UTC.Plus0400;

                case 4.5:
                    return Core.UTC.Plus0430;

                case 5.0:
                    return Core.UTC.Plus0500;

                case 5.5:
                    return Core.UTC.Plus0530;

                case 5.75:
                    return Core.UTC.Plus0545;

                case 6.0:
                    return Core.UTC.Plus0600;

                case 6.5:
                    return Core.UTC.Plus0630;

                case 7.0:
                    return Core.UTC.Plus0700;

                case 8.0:
                    return Core.UTC.Plus0800;

                case 8.75:
                    return Core.UTC.Plus0845;

                case 9.0:
                    return Core.UTC.Plus0900;

                case 9.5:
                    return Core.UTC.Plus0930;

                case 10.0:
                    return Core.UTC.Plus1000;

                case 10.5:
                    return Core.UTC.Plus1030;

                case 11.0:
                    return Core.UTC.Plus1100;

                case 12.0:
                    return Core.UTC.Plus1200;

                case 12.75:
                    return Core.UTC.Plus1245;

                case 13.0:
                    return Core.UTC.Plus1300;

                case 14.0:
                    return Core.UTC.Plus1400;
            }

            return Core.UTC.Undefined;
        }

        public static UTC UTC(string value)
        {
            if(string.IsNullOrWhiteSpace(value))
            {
                return Core.UTC.Undefined;
            }

            if(Core.UTC.Undefined.ToString().ToUpper().Equals(value.ToUpper().Trim()))
            {
                return Core.UTC.Undefined;
            }
            
            UTC result = Enum<UTC>(value);
            if(result != Core.UTC.Undefined)
            {
                return result;
            }

            if(TryConvert(value, out double @double))
            {
                result = UTC(@double);
                if(result != Core.UTC.Undefined)
                {
                    return result;
                }
            }

            return Core.UTC.Undefined;
        }
    }
}