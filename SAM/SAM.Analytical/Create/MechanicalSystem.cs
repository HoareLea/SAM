// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace SAM.Analytical
{
    public static partial class Create
    {
        public static MechanicalSystem MechanicalSystem(MechanicalSystemType mechanicalSystemType, string namePrefix = null, int index = -1)
        {
            if (mechanicalSystemType == null)
                return null;

            string id = null;
            if (index != -1)
            {
                id = index.ToString();
            }

            return MechanicalSystem(mechanicalSystemType, namePrefix, id);
        }

        public static MechanicalSystem MechanicalSystem(MechanicalSystemType mechanicalSystemType, string namePrefix = null, string id = null)
        {
            if (mechanicalSystemType == null)
                return null;

            MechanicalSystem result = null;
            if (mechanicalSystemType is VentilationSystemType)
                result = new VentilationSystem(namePrefix, id, (VentilationSystemType)mechanicalSystemType);
            else if (mechanicalSystemType is HeatingSystemType)
                result = new HeatingSystem(namePrefix, id, (HeatingSystemType)mechanicalSystemType);
            else if (mechanicalSystemType is CoolingSystemType)
                result = new CoolingSystem(namePrefix, id, (CoolingSystemType)mechanicalSystemType);

            if (result == null)
                return null;

            return result;
        }
    }
}
