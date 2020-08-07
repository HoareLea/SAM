using System;
using System.Runtime.InteropServices;

namespace SAM.Core
{
    public static partial class Query
    {
        public static string COMObjectTypeName(this object cOMObject)
        {
            if (cOMObject == null)
                return null;

            if (!Marshal.IsComObject(cOMObject))
                //The specified object is not a COM object
                return null;

            IDispatch dispatch = cOMObject as IDispatch;
            if (dispatch == null)
                //The specified COM object doesn't support getting type information
                return null;

            System.Runtime.InteropServices.ComTypes.ITypeInfo typeInfo = null;
            try
            {
                try
                {
                    // obtain the ITypeInfo interface from the object
                    dispatch.GetTypeInfo(0, 0, out typeInfo);
                }
                catch (Exception ex)
                {
                    //Cannot get the ITypeInfo interface for the specified COM object
                    return null;
                }

                string typeName = "";
                string documentation;
                string helpFile;
                int helpContext = -1;

                try
                {
                    //retrieves the documentation string for the specified type description 
                    typeInfo.GetDocumentation(-1, out typeName, out documentation, out helpContext, out helpFile);
                }
                catch (Exception ex)
                {
                    // Cannot extract ITypeInfo information
                    return null;
                }
                return typeName;
            }
            catch (Exception ex)
            {
                // Unexpected error
                return null;
            }
            finally
            {
                if (typeInfo != null) 
                    Marshal.ReleaseComObject(typeInfo);
            }
        }
    }
}