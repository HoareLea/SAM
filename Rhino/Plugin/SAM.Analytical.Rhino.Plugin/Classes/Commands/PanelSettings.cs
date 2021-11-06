using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Rhino.Plugin
{
    public class PanelSettings : Command
    {
        public PanelSettings()
        {
            // Rhino only creates one instance of each command class defined in a
            // plug-in, so it is safe to store a refence in a static property.
            Instance = this;
        }

        ///<summary>The only instance of this command.</summary>
        public static PanelSettings Instance { get; private set; }

        ///<returns>The command name as it appears on the Rhino command line.</returns>
        public override string EnglishName => "SAM_PanelSettings";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            Result result = RhinoGet.GetMultipleObjects("Select Panels", false, ObjectType.Brep, out ObjRef[] obj_refs);
            if (result != Result.Success || obj_refs == null)
                return result;

            List<Tuple<Panel, Brep>> tuples = new List<Tuple<Panel, Brep>>();
            foreach (var obj_ref in obj_refs)
            {
                Brep brep = obj_ref.Brep();
                if (brep != null && brep.HasUserData)
                {
                    string @string = brep.GetUserString("SAM");
                    if (string.IsNullOrWhiteSpace(@string))
                    {
                        continue;
                    }
                        
                    List<Panel> panels_Temp = Core.Convert.ToSAM<Panel>(@string);
                    if(panels_Temp == null)
                    {
                        continue;
                    }

                    foreach(Panel panel in panels_Temp)
                    {
                        if(panel == null)
                        {
                            continue;
                        }
                        
                        tuples.Add(new Tuple<Panel, Brep>(panel, brep));
                    }
                }
            }

            if(tuples == null || tuples.Count == 0)
            {
                return Result.Nothing;
            }

            List<Panel> panels = new List<Panel>();
            using (PanelForm panelForm = new PanelForm(tuples.ConvertAll(x => x.Item1)))
            {
                if(panelForm.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                {
                    return Result.Cancel;
                }

                panels = panelForm.Panels;
            }

            foreach(Panel panel in panels)
            {
                if(panel == null)
                {
                    continue;
                }

                Tuple<Panel, Brep> tuple = tuples.Find(x => x.Item1.Guid == panel.Guid);
                if(tuple == null)
                {
                    continue;
                }

                string @string = panel.ToJObject()?.ToString();
                if(string.IsNullOrWhiteSpace(@string))
                {
                    continue;
                }

                tuple.Item2.SetUserString("SAM", @string);
            }

            return Result.Success;
            
            //// TODO: start here modifying the behaviour of your command.
            //// ---
            //RhinoApp.WriteLine("The {0} command will add a line right now.", EnglishName);

            //Point3d pt0;
            //using (GetPoint getPointAction = new GetPoint())
            //{
            //    getPointAction.SetCommandPrompt("Please select the start point");
            //    if (getPointAction.Get() != GetResult.Point)
            //    {
            //        RhinoApp.WriteLine("No start point was selected.");
            //        return getPointAction.CommandResult();
            //    }
            //    pt0 = getPointAction.Point();
            //}

            //Point3d pt1;
            //using (GetPoint getPointAction = new GetPoint())
            //{
            //    getPointAction.SetCommandPrompt("Please select the end point");
            //    getPointAction.SetBasePoint(pt0, true);
            //    getPointAction.DynamicDraw +=
            //      (sender, e) => e.Display.DrawLine(pt0, e.CurrentPoint, System.Drawing.Color.DarkRed);
            //    if (getPointAction.Get() != GetResult.Point)
            //    {
            //        RhinoApp.WriteLine("No end point was selected.");
            //        return getPointAction.CommandResult();
            //    }
            //    pt1 = getPointAction.Point();
            //}

            //doc.Objects.AddLine(pt0, pt1);
            //doc.Views.Redraw();
            //RhinoApp.WriteLine("The {0} command added one line to the document.", EnglishName);

            //// ---
            //return Result.Success;
        }
    }
}
