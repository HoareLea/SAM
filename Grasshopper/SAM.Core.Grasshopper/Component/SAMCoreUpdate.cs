using Grasshopper.Kernel;
using Rhino;
using SAM.Core.Grasshopper.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace SAM.Core.Grasshopper
{
    public class SAMCoreUpdate : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("a89bfee3-3a3c-4d29-9c7a-64073724eddc");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public override GH_Exposure Exposure => GH_Exposure.primary;

        protected override GH_SAMParam[] Inputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                return result.ToArray();
            }
        }

        protected override GH_SAMParam[] Outputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                return result.ToArray();
            }
        }

        public override void CreateAttributes()
        {
            base.CreateAttributes();
            //m_attributes = new CustomAttributes(this);
        }

        /// <summary>
        /// Updates PanelTypes for AdjacencyCluster
        /// </summary>
        public SAMCoreUpdate()
          : base("SAMCore.Update", "SAMCore.Update",
              "Updates Grasshopper components to the latest version",
              "SAM", "SAM")
        {
        }

        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown toolStripDropDown)
        {
            Menu_AppendItem(toolStripDropDown, "Update", Menu_Update);
        }

        private void Menu_Update(object sender, EventArgs e)
        {
            GH_Document gH_Document = OnPingDocument();
            if(gH_Document == null)
            {
                return;
            }

            IList<IGH_DocumentObject> gH_DocumentObjects = gH_Document.Objects;
            if(gH_DocumentObjects == null || gH_DocumentObjects.Count == 0)
            {
                return;
            }

            List<GH_SAMComponent> gH_SAMComponents = new List<GH_SAMComponent>();
            foreach(IGH_DocumentObject gH_DocumentObject in gH_DocumentObjects)
            {
                GH_SAMComponent gH_SAMComponent = gH_DocumentObject as GH_SAMComponent;

                if (!(gH_DocumentObject is GH_SAMComponent))
                {
                    continue;
                }

                if(!gH_SAMComponent.Obsolete)
                {
                    gH_SAMComponents.Add(gH_SAMComponent);
                }
            }

            if (gH_SAMComponents == null || gH_SAMComponents.Count == 0)
            {
                return;
            }

            Update(gH_SAMComponents);
        }

        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {

        }

        private void Update(IEnumerable<GH_SAMComponent> gH_SAMComponents)
        {
            if(gH_SAMComponents == null || gH_SAMComponents.Count() == 0)
            {
                return;
            }

            foreach(GH_SAMComponent gH_SAMComponent in gH_SAMComponents)
            {
                Update(gH_SAMComponent);
            }
        }

        private void Update(GH_SAMComponent gH_SAMComponent)
        {
            if(gH_SAMComponent == null)
            {
                return;
            }

            GH_Document gH_Document = OnPingDocument();
            if (gH_Document == null)
            {
                return;
            }

            GH_SAMComponent gH_SAMComponent_New = Activator.CreateInstance(gH_SAMComponent.GetType()) as GH_SAMComponent;
            if (gH_SAMComponent_New == null)
            {
                RhinoApp.WriteLine("Failed to create new component.");
                return;
            }

            gH_Document.AddObject(gH_SAMComponent_New, false);

            gH_SAMComponent_New.Attributes.Pivot = gH_SAMComponent.Attributes.Pivot;

            bool updateFailed = false;


            if (gH_SAMComponent_New is IGH_VariableParameterComponent && gH_SAMComponent is IGH_VariableParameterComponent)
            {
                for (int i = 0; i < gH_SAMComponent.Params.Input.Count; i++)
                {
                    IGH_Param gh_Param_Old = gH_SAMComponent.Params.Input[i];
                    IGH_Param gh_Param_New = gH_SAMComponent_New.Params.Input.FirstOrDefault(p => p.NickName == gh_Param_Old.NickName && p.Access == gh_Param_Old.Access);

                    if (gh_Param_New == null)
                    {
                        updateFailed = true;
                        break;
                    }

                    foreach (IGH_Param gh_Param_Source in gh_Param_Old.Sources)
                    {
                        gh_Param_New.AddSource(gh_Param_Source);
                        //if (gh_Param_New.Optional && gh_Param_New.IsHidden)
                        //{
                        //    gh_Param_New.IsHidden = false;
                        //    gh_Param_New.ExpireSolution(true);
                        //}
                    }
                }
            }
            else
            {
                for (int i = 0; i < gH_SAMComponent.Params.Input.Count; i++)
                {
                    var oldParam = gH_SAMComponent.Params.Input[i];
                    var newParam = gH_SAMComponent_New.Params.Input[i];

                    if (oldParam.Sources.Any())
                    {
                        foreach (var source in oldParam.Sources)
                        {
                            newParam.AddSource(source);
                        }
                    }
                }
            }

            if (updateFailed)
            {
                gH_Document.RemoveObject(gH_SAMComponent_New, false);
                MarkComponentInRed(gH_SAMComponent, string.Format("Update of component {0} failed: Parameter mismatch", gH_SAMComponent.Name));
                return;
            }

            // Transfer outputs
            for (int i = 0; i < gH_SAMComponent.Params.Output.Count; i++)
            {
                var oldParam = gH_SAMComponent.Params.Output[i];
                var newParam = gH_SAMComponent_New.Params.Output[i];

                foreach (var recipient in oldParam.Recipients)
                {
                    recipient.AddSource(newParam);
                }
            }

            gH_Document.RemoveObject(gH_SAMComponent, true);
            gH_SAMComponent_New.ExpireSolution(true);
        }

        private void MarkComponentInRed(IGH_Component gH_Component, string message)
        {
            if(gH_Component == null)
            {
                return;
            }

            //gH_Component.Attributes.Selected = true;
            //gH_Component.Attributes.Ma  = Color.Red;
            //gH_Component.Attributes.PenWidth = 2;
            
            if(!string.IsNullOrWhiteSpace(message))
            {
                RhinoApp.WriteLine(message);
            }
            
            gH_Component.ExpireSolution(true);
        }
    }

    //public class CustomAttributes : GH_ComponentAttributes
    //{
    //    private bool _markForReview;

    //    public CustomAttributes(IGH_Component component) : base(component) { }

    //    public void MarkForReview()
    //    {
    //        _markForReview = true;
    //        Owner.OnDisplayExpired(true);
    //    }

    //    protected override void Layout()
    //    {
    //        base.Layout();
    //        if (_markForReview)
    //        {
    //            Bounds = new RectangleF(Bounds.X - 5, Bounds.Y - 5, Bounds.Width + 10, Bounds.Height + 10);
    //        }
    //    }

    //    protected override void Render(GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel)
    //    {
    //        base.Render(canvas, graphics, channel);

    //        if (channel == GH_CanvasChannel.Objects && _markForReview)
    //        {
    //            var pen = new Pen(Color.Red, 2);
    //            graphics.DrawRectangle(pen, Bounds.X, Bounds.Y, Bounds.Width, Bounds.Height);
    //        }
    //    }
    //}
}