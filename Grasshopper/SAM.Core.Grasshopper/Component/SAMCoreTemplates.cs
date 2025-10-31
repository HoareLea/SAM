using Grasshopper.Kernel;
using SAM.Core.Grasshopper;
using SAM.Core.Grasshopper.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using GH = Grasshopper;

namespace Ironbug.Grasshopper.Component
{
    public class SAMCoreTemplates : GH_SAMVariableOutputParameterComponent
    {
        List<string> folderList = [];
        List<List<string>> filesList = [];

        public SAMCoreTemplates()
          : base("SAMCore.Templates", "SAMCore.Templates",
              "AAA",
              "SAM", "Core")
        {
        }
        
        public override Guid ComponentGuid => new ("aea07560-4491-4fa7-8f35-ee773775d9f3");
        
        protected override Bitmap Icon => Resources.SAM_Small;

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        protected override GH_SAMParam[] Inputs
        {
            get
            {
                List<GH_SAMParam> result = [];

                global::Grasshopper.Kernel.Parameters.Param_String param_String = new global::Grasshopper.Kernel.Parameters.Param_String()
                {
                    Name = "_directory_",
                    NickName = "_directory",
                    Description = "Additional folder path to import the HVAC template.",
                    Optional = true,
                    Access = GH_ParamAccess.list
                };

                result.Add(new GH_SAMParam(param_String, ParamVisibility.Binding));

                return [.. result];
            }
        }

        protected override GH_SAMParam[] Outputs
        {
            get
            {
                List<GH_SAMParam> result = [];

                global::Grasshopper.Kernel.Parameters.Param_String param_String = new()
                {
                    Name = "Templates",
                    NickName = "Templates",
                    Description = "Templates found from folders",
                    Optional = true,
                    Access = GH_ParamAccess.list
                };

                result.Add(new GH_SAMParam(param_String, ParamVisibility.Binding));

                return [.. result];
            }
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            this.folderList = [];
            this.filesList = [];

            var root = Path.GetDirectoryName(this.GetType().Assembly.Location);

            var templateDir = Path.Combine(root, "Templates");
            var dirs = new List<string>() { templateDir };
            DA.GetDataList(0, dirs);

            dirs = [.. dirs.Where(_ => Directory.Exists(_))];
            if (dirs.Count == 0)
            {
                throw new ArgumentException("No template folder was found!");
            }

            foreach (var dir in dirs)
            {
                List<string> fs = Directory.GetFiles(dir, "*.gh*", SearchOption.AllDirectories).ToList();
                if (fs.Any())
                {
                    this.folderList.Add(Path.GetDirectoryName(Path.Combine(dir, "test.txt")));
                    this.filesList.Add(fs);

                }
            }
            DA.SetDataList(0, this.filesList.SelectMany(_ => _));

            this.templateMenu = GetHVACMenu();

        }

        private Size GetMoveVector(PointF FromLocation)
        {
            var moveX = this.Attributes.Bounds.Left - 80 - FromLocation.X;
            var moveY = this.Attributes.Bounds.Y + 180 - FromLocation.Y;
            var loc = new Point(System.Convert.ToInt32(moveX), System.Convert.ToInt32(moveY));

            return new Size(loc);
        }

        private void CreateTemplateFromFilePath(string FilePath, ref bool Run)
        {
            var canvasCurrent = GH.Instances.ActiveCanvas;
            var f = canvasCurrent.Focused;
            var isFileExist = File.Exists(FilePath);

            if (Run && f && isFileExist)
            {
                var io = new GH_DocumentIO();

                var success = io.Open(FilePath);

                if (!success)
                {
                    MessageBox.Show("Failed to add template.");
                    return;
                }
                var docTemp = io.Document;

                docTemp.SelectAll();
                docTemp.MutateAllIds();

                //move to where this component is...
                var box = docTemp.BoundingBox(false);
                var vec = GetMoveVector(box.Location);
                docTemp.TranslateObjects(vec, true);

                docTemp.ExpireSolution();
                var docCurrent = canvasCurrent.Document;
                docCurrent.DeselectAll();
                docCurrent.MergeDocument(docTemp);
            }
        }


        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            //var newMenu = menu;
            //newMenu.Items.Clear();

            //newMenu.Items.AddRange(GetHVACMenu().Items);
            //templateMenu = newMenu;

        }

        private ToolStripDropDownMenu GetHVACMenu()
        {
            var menu = new ToolStripDropDownMenu();

            var rootFolder = this.folderList.FirstOrDefault();
            var topDirs = Directory.GetDirectories(rootFolder);

            foreach (var item in topDirs)
            {
                var menuItem = addFromFolder(item);
                if (menuItem == null)
                    continue;
                menu.Items.Add(menuItem);
            }

            return menu;
        }

        ToolStripDropDownMenu templateMenu = new ToolStripDropDownMenu();
        private ToolStripMenuItem addFromFolder(string rootFolder)
        {
            var allFiles = Directory.GetFiles(rootFolder, "*.gh*", SearchOption.AllDirectories);
            if (!allFiles.Any()) return null;

            var topDirs = Directory.GetDirectories(rootFolder);
            var topFiles = Directory.GetFiles(rootFolder, "*.gh*", SearchOption.TopDirectoryOnly);


            // create menu item
            var folderName = new DirectoryInfo(rootFolder).Name;
            var t = new ToolStripMenuItem(folderName);

            foreach (var item in topDirs)
            {
                var menuItem = addFromFolder(item);
                if (menuItem == null)
                    continue;
                t.DropDownItems.Add(menuItem);
            }


            foreach (var item in topFiles)
            {
                //var p = Path.GetDirectoryName(item);
                var name = Path.GetFileNameWithoutExtension(item);
                //var showName = p.Length > rootFolder.Length ? p.Replace(rootFolder+"\\", "") + "\\" + name : name;

                EventHandler ev = (object sender, EventArgs e) =>
                {
                    var a = sender as ToolStripDropDownItem;
                    var r = true;
                    CreateTemplateFromFilePath(a.Tag.ToString(), ref r);
                    this.ExpireSolution(true);

                };

                Menu_AppendItem(t.DropDown, name, ev, null, item);
            }

            return t;
        }

        public override void CreateAttributes()
        {
            var att = new GH_SAMComponentButtonAttributes(this);
            att.ButtonText = "Pick a template";

            att.MouseDownEvent += (object loc) => this.templateMenu.Show((GH.GUI.Canvas.GH_Canvas)loc, (loc as GH.GUI.Canvas.GH_Canvas).CursorControlPosition);
            this.Attributes = att;

        }

    }

}
