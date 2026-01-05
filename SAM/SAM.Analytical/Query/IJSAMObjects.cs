// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static List<TJSAMObject> IJSAMObjects<TJSAMObject>(this CaseSelection caseSelection, AnalyticalModel analyticalModel) where TJSAMObject : IJSAMObject
        {
            if (caseSelection is null || analyticalModel is null)
            {
                return null;
            }


            if (caseSelection is ObjectReferenceCaseSelection objectReferenceCaseSelection)
            {
                return IJSAMObjects<TJSAMObject>(objectReferenceCaseSelection, analyticalModel);
            }
            else if (caseSelection is SAMObjectCaseSelection sAMObjectCaseSelection)
            {

            }
            else if (caseSelection is FilterSelection filterSelection)
            {
                return IJSAMObjects<TJSAMObject>(filterSelection, analyticalModel);
            }

            throw new System.NotImplementedException();

        }

        public static List<TJSAMObject> IJSAMObjects<TJSAMObject>(this ObjectReferenceCaseSelection objectReferenceCaseSelection, AnalyticalModel analyticalModel) where TJSAMObject : IJSAMObject
        {
            if (objectReferenceCaseSelection is null || analyticalModel is null)
            {
                return null;
            }

            List<TJSAMObject> result = [];

            if (objectReferenceCaseSelection.ObjectReferences is not IEnumerable<ObjectReference> objectReferences)
            {
                return result;
            }

            foreach (ObjectReference objectReference in objectReferences)
            {
                TJSAMObject jSAMObject = analyticalModel.GetObject<TJSAMObject>(objectReference);
                if (jSAMObject is null)
                {
                    continue;
                }

                result.Add(jSAMObject);
            }

            return result;
        }

        public static List<TJSAMObject> IJSAMObjects<TJSAMObject>(this SAMObjectCaseSelection sAMObjectCaseSelection, AnalyticalModel analyticalModel) where TJSAMObject : IJSAMObject
        {
            if (sAMObjectCaseSelection is null || analyticalModel is null)
            {
                return null;
            }

            List<TJSAMObject> result = [];

            if (sAMObjectCaseSelection.Objects is not IEnumerable<IJSAMObject> sAMObjects)
            {
                return result;
            }

            foreach (IJSAMObject jSAMObject in sAMObjects)
            {
                if (jSAMObject is TJSAMObject jSAMObject_Temp)
                {
                    result.Add(jSAMObject_Temp);
                }
            }

            return result;
        }

        public static List<TJSAMObject> IJSAMObjects<TJSAMObject>(this ApertureCaseSelection apertureCaseSelection, AnalyticalModel analyticalModel) where TJSAMObject : IJSAMObject
        {
            if (apertureCaseSelection is null || analyticalModel is null)
            {
                return null;
            }

            List<TJSAMObject> result = [];

            if (apertureCaseSelection.Objects is not IEnumerable<Aperture> apertures)
            {
                return result;
            }

            foreach (Aperture aperture in apertures)
            {
                if (aperture is TJSAMObject jSAMObject_Temp)
                {
                    result.Add(jSAMObject_Temp);
                }
            }

            return result;
        }
    }
}
