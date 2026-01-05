// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Rhino.DocObjects;

namespace SAM.Core.Rhino
{
    public static partial class Modify
    {
        public static bool SetUserStrings(this ObjectAttributes objectAttributes, SAMObject sAMObject)
        {
            if (objectAttributes == null || sAMObject == null)
                return false;

            foreach (string name in Core.Query.Names(sAMObject))
            {
                if (sAMObject.TryGetValue(name, out string value, true))
                    objectAttributes.SetUserString(name, value);
            }

            return true;
        }
    }
}
