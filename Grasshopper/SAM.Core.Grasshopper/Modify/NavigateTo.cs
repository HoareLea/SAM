// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace SAM.Core.Grasshopper
{
    public static partial class Modify
    {
        public static bool NavigateTo(GH_Document gH_Document, Guid instanceGuid)
        {
            return NavigateTo(gH_Document, new Guid[] { instanceGuid });
        }

        public static bool NavigateTo(GH_Document gH_Document, IEnumerable<Guid> instanceGuids)
        {
            if (gH_Document == null || instanceGuids == null)
            {
                return false;
            }

            GH_Canvas gH_Canvas = Instances.ActiveCanvas;
            if (gH_Canvas == null || gH_Canvas.Document != gH_Document)
            {
                return false;
            }

            List<IGH_Attributes> attributesList = new List<IGH_Attributes>();
            foreach (Guid instanceGuid in instanceGuids)
            {
                IGH_DocumentObject gH_DocumentObject = gH_Document.FindObject(instanceGuid, true);

                IGH_Attributes attributes = gH_DocumentObject?.Attributes?.GetTopLevel;
                if (attributes == null)
                {
                    continue;
                }

                if (!attributesList.Contains(attributes))
                {
                    attributesList.Add(attributes);
                }
            }

            if (attributesList.Count == 0)
            {
                return false;
            }

            gH_Document.DeselectAll();

            foreach (IGH_Attributes attributes in attributesList)
            {
                attributes.Selected = true;
            }

            CenterViewport(gH_Canvas, attributesList);

            gH_Canvas.Refresh();

            return true;
        }

        private static void CenterViewport(GH_Canvas gH_Canvas, List<IGH_Attributes> attributesList)
        {
            RectangleF bounds = attributesList[0].Bounds;
            for (int i = 1; i < attributesList.Count; i++)
            {
                bounds = RectangleF.Union(bounds, attributesList[i].Bounds);
            }

            bounds.Inflate(50, 50);

            GH_Viewport gH_Viewport = gH_Canvas.Viewport;

            float zoom = gH_Viewport.Zoom;
            if (bounds.Width > 0 && bounds.Height > 0)
            {
                Rectangle screenPort = gH_Viewport.ScreenPort;
                if (screenPort.Width > 0 && screenPort.Height > 0)
                {
                    float fitZoom = Math.Min(screenPort.Width / bounds.Width, screenPort.Height / bounds.Height);
                    zoom = Math.Min(1.0f, fitZoom);
                }
            }

            gH_Viewport.Zoom = zoom;
            gH_Viewport.MidPoint = new PointF(bounds.X + bounds.Width / 2f, bounds.Y + bounds.Height / 2f);
            gH_Viewport.ComputeProjection();
        }
    }
}
