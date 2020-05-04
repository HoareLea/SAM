namespace SAM.Core
{
    public static partial class Query
    {
        public static string AboutInfoTypeText(this AboutInfoType aboutInfoType)
        {
            switch (aboutInfoType)
            {
                case (AboutInfoType.HoareLea):
                    return "https://hoarelea.com/specialisms/ \n An award-winning engineering consultancy with a creative team of engineers, designers, and technical specialists. We provide innovative solutions to complex engineering and design challenges for buildings.";

                case (AboutInfoType.SAM):
                    return "Sustainable Analytical Model originated by Michal Dengusiak and Jakub Ziolkowski";

                case (AboutInfoType.Other):
                    return "Tbc";
            }

            return null;
        }
    }
}