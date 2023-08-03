namespace SAM.Core
{
    public static partial class Query
    {
        public static Direction Opposite(this Direction direction)
        {
            if(direction == Direction.Undefined)
            {
                return Direction.Undefined;
            }

            switch (direction)
            {
                case Direction.In:
                    return Direction.Out;

                case Direction.Out:
                    return Direction.In;
            }

            return Direction.Undefined;
        }

        public static YesNo Opposite(this YesNo yesNo)
        {
            if (yesNo == YesNo.Undefined)
            {
                return YesNo.Undefined;
            }

            switch (yesNo)
            {
                case YesNo.Yes:
                    return YesNo.No;

                case YesNo.No:
                    return YesNo.Yes;
            }

            return YesNo.Undefined;
        }
    }
}