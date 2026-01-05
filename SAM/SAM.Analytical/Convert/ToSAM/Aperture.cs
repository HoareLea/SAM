// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace SAM.Analytical
{
    public static partial class Convert
    {
        public static Aperture ToSAM(this IOpening opening)
        {
            if (opening == null)
            {
                return null;
            }

            ApertureConstruction apertureConstruction = opening.Type()?.ToSAM();

            Aperture result = new Aperture(apertureConstruction, opening.Face3D);
            Core.Modify.CopyParameterSets(opening as Core.SAMObject, result);
            return result;
        }
    }
}
