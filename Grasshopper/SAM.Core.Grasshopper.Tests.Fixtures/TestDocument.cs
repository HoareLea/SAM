// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using System;
using System.Drawing;

namespace SAM.Core.Grasshopper.Tests
{
    public static class TestDocument
    {
        public static GH_Document TryCreate()
        {
            try
            {
                return new GH_Document();
            }
            catch (TypeInitializationException)
            {
                return null;
            }
            catch (DllNotFoundException)
            {
                return null;
            }
        }

        public static T AddComponent<T>(GH_Document gH_Document, float x, float y) where T : GH_SAMComponent, new()
        {
            T result = new T();
            result.CreateAttributes();
            result.Attributes.Pivot = new PointF(x, y);
            gH_Document.AddObject(result, false);
            return result;
        }

        public static Param_String CreateFloatingParam(string name, float x, float y)
        {
            Param_String result = new Param_String { Name = name, NickName = name, Description = name, Access = GH_ParamAccess.item, Optional = true };
            result.CreateAttributes();
            result.Attributes.Pivot = new PointF(x, y);
            return result;
        }

        public static Param_String AddFloatingParam(GH_Document gH_Document, string name, float x, float y)
        {
            Param_String result = CreateFloatingParam(name, x, y);
            gH_Document.AddObject(result, false);
            return result;
        }

        public static Param_String RegisterLegacyInput(GH_SAMComponent gH_SAMComponent, string name)
        {
            Param_String result = new Param_String { Name = name, NickName = name, Description = name, Access = GH_ParamAccess.item, Optional = true };
            gH_SAMComponent.Params.RegisterInputParam(result);
            gH_SAMComponent.Params.OnParametersChanged();
            return result;
        }

        public static Param_String RegisterLegacyOutput(GH_SAMComponent gH_SAMComponent, string name)
        {
            Param_String result = new Param_String { Name = name, NickName = name, Description = name, Access = GH_ParamAccess.item };
            gH_SAMComponent.Params.RegisterOutputParam(result);
            gH_SAMComponent.Params.OnParametersChanged();
            return result;
        }

        public static void MakeObsolete(TestComponentBase testComponentBase)
        {
            testComponentBase.SetComponentVersion("0.9.0");
        }
    }
}
