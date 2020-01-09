namespace SAMAnalyticalDynamo
{
    /// <summary>
    /// SAM Object
    /// </summary>
    public static class SAMObject
    {
        /// <summary>
        /// Convert SAM Analytical Object to Dynamo Geometry
        /// </summary>
        /// <param name="sAMObject">SAM Analytical Object</param>
        /// <returns name="geometry">Dynamo Geometry</returns>
        /// <search>
        /// SAMObject, Convert
        /// </search>
        public static Autodesk.DesignScript.Geometry.Geometry Convert(SAM.Core.SAMObject sAMObject)
        {
            if (sAMObject is SAM.Analytical.Panel)
                return SAMGeometryDynamo.Convert.ToDynamo(((SAM.Analytical.Panel)sAMObject).ToFace());

            if (sAMObject is SAM.Analytical.Space)
                return SAMGeometryDynamo.Convert.ToDynamo(((SAM.Analytical.Space)sAMObject).Location);

            return null;
        }
    }
}
