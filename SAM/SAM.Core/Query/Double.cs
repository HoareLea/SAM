namespace SAM.Core
{
    public static partial class Query
    {
        public static double Double(this UTC uTC)
        {
            if(uTC == Core.UTC.Undefined)
            {
                return double.NaN;
            }

            switch(uTC)
            {
                case Core.UTC.Minus1200:
                    return - 12.0;

                case Core.UTC.Minus1100:
                    return -11.0;

                case Core.UTC.Minus1000:
                    return -10.0;

                case Core.UTC.Minus0930:
                    return -9.5;

                case Core.UTC.Minus0800:
                    return -8.0;

                case Core.UTC.Minus0700:
                    return -7.0;

                case Core.UTC.Minus0600:
                    return -6.0;

                case Core.UTC.Minus0500:
                    return -5.0;

                case Core.UTC.Minus0400:
                    return -4.0;

                case Core.UTC.Minus0330:
                    return -3.5;

                case Core.UTC.Minus0300:
                    return -3.0;

                case Core.UTC.Minus0200:
                    return -2.0;

                case Core.UTC.Minus0100:
                    return -1.0;

                case Core.UTC.PlusMinus0000:
                    return 0.0;

                case Core.UTC.Plus0100:
                    return 1.0;

                case Core.UTC.Plus0200:
                    return 2.0;

                case Core.UTC.Plus0300:
                    return 3.0;

                case Core.UTC.Plus0330:
                    return 3.5;

                case Core.UTC.Plus0400:
                    return 4.0;

                case Core.UTC.Plus0430:
                    return 4.5;

                case Core.UTC.Plus0500:
                    return 5.0;

                case Core.UTC.Plus0530:
                    return 5.5;

                case Core.UTC.Plus0545:
                    return 5.75;

                case Core.UTC.Plus0600:
                    return 6.0;

                case Core.UTC.Plus0630:
                    return 6.5;

                case Core.UTC.Plus0700:
                    return 7.0;

                case Core.UTC.Plus0800:
                    return 8.0;

                case Core.UTC.Plus0845:
                    return 8.75;

                case Core.UTC.Plus0900:
                    return 9.0;

                case Core.UTC.Plus0930:
                    return 9.5;

                case Core.UTC.Plus1000:
                    return 10.0;

                case Core.UTC.Plus1030:
                    return 10.5;

                case Core.UTC.Plus1100:
                    return 11.0;

                case Core.UTC.Plus1200:
                    return 12.0;

                case Core.UTC.Plus1245:
                    return 12.75;

                case Core.UTC.Plus1300:
                    return 13.0;

                case Core.UTC.Plus1400:
                    return 14.0;
            }

            return double.NaN;
        }
    }
}