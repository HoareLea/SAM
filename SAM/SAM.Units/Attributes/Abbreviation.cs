// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System;

namespace SAM.Units
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class Abbreviation : Attribute
    {
        private string value;

        public Abbreviation(string value)
        {
            this.value = value;
        }

        public string Value
        {
            get
            {
                return value;
            }
        }
    }
}
