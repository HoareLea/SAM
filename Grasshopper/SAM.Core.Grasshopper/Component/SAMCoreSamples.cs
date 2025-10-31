using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Core.Grasshopper;
using SAM.Core.Grasshopper.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using GH = Grasshopper;

namespace SAM.Core.Grasshopper
{
    /// <summary>
    /// List and insert Grasshopper sample files (.gh / .ghx) from the built-in "Samples" folder
    /// next to the SAM Core plugin, plus any additional folders you provide.
    /// Makes no changes to your model until you pick a sample from the menu.
    /// </summary>
    /// <remarks>
    /// <para>
    /// SUMMARY
    ///   • Searches for sample files under the default "Samples" directory located next to the plugin DLL.
    ///   • You can add extra directories via <c>_directory_</c> (one or more). All valid folders are searched.
    ///   • Outputs a flat list of discovered sample file paths. Use the component's button to insert content.
    /// </para>
    /// <para>
    /// INPUTS
    ///   _directory_  (Text[], optional)
    ///     Additional folder paths to include in the search. If omitted, only the built-in
    ///     <root>/Samples directory is used.
    ///     Type: Text (paths)
    ///     Required: No
    /// </para>
    /// <para>
    /// OUTPUTS
    ///   Samples  (Text[])
    ///     Full file paths to .gh / .ghx samples found in the searched folders.
    /// </para>
    /// <para>
    /// NOTES
    ///   • Use the component's button to open a pop-up menu of folders → pick an item to insert
    ///     that sample's content into the active Grasshopper document.
    ///   • Paths that do not exist are ignored. If no valid folders remain, the component raises
    ///     an informative Grasshopper runtime error message and stops.
    /// </para>
    /// <para>
    /// EXAMPLE
    ///   1) Drop the component on canvas.
    ///   2) (Optional) Supply extra sample directories → _directory_
    ///   3) Click the component's button and choose a sample to insert.
    /// </para>
    /// </remarks>
    public class SAMCoreSamples : GH_SAMVariableOutputParameterComponent
    {
        // Folders discovered during Solve → mirrored into a pop-up menu for insertion
        List<string> folderList = [];
        List<List<string>> filesList = [];

        // ────────────────────────────────────────────────────────────────────────────────
        // Metadata
        // ────────────────────────────────────────────────────────────────────────────────

        public SAMCoreSamples()
          : base(
                "SAMCore.Samples",
                "Core.Samples",
                DescriptionLong,
                "SAM", "Core")
        { }

        public override Guid ComponentGuid => new("aea07560-4491-4fa7-8f35-ee773775d9f3");

        protected override Bitmap Icon => Resources.SAM_Small;

        /// <summary>The latest version of this component.</summary>
        public override string LatestComponentVersion => "1.0.1";

        // Long, multi-line description for tooltips and docs (MEP-friendly SAM style)
        private const string DescriptionLong =
@"List and insert Grasshopper sample files from the plugin's Samples folder and optional extra folders.

SUMMARY
  • Searches the built-in <plugin>/Samples directory for .gh / .ghx files.
  • You can add more folders via _directory_.
  • Outputs a list of found sample file paths; use the button to insert a chosen sample.

INPUTS
  _directory_  (Text[], optional)
    Additional folder paths to include in the search.
    If omitted, only the built-in Samples folder is searched.
    Type: Text (paths)
    Required: No

OUTPUTS
  Samples  (Text[])
    Full file paths to .gh / .ghx samples discovered in the searched folders.

NOTES
  • Click the component's button to open the menu and pick a sample to insert.
  • Non-existent paths are ignored. If no samples folder is found, a Grasshopper error message is
    shown and the component stops.

EXAMPLE
  1) Add the component.
  2) (Optional) Provide extra folders → _directory_
  3) Click the button → choose a sample → it appears near this component.";

        // ────────────────────────────────────────────────────────────────────────────────
        // Parameters
        // ────────────────────────────────────────────────────────────────────────────────

        protected override GH_SAMParam[] Inputs
        {
            get
            {
                List<GH_SAMParam> result = [];

                var param_String = new global::Grasshopper.Kernel.Parameters.Param_String
                {
                    Name = "_directory_",
                    NickName = "_directory_",
                    Description = @"Additional folder paths to search for .gh / .ghx samples.
If empty, only the built-in <plugin>/Samples directory is used.
Type: Text (paths)
Required: No",
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

                var param_String = new global::Grasshopper.Kernel.Parameters.Param_String
                {
                    Name = "Samples",
                    NickName = "Samples",
                    Description = @"Full file paths to samples discovered in the searched folders.
Each item is a .gh or .ghx file path.",
                    Optional = true,
                    Access = GH_ParamAccess.list
                };

                result.Add(new GH_SAMParam(param_String, ParamVisibility.Binding));

                return [.. result];
            }
        }

        // ────────────────────────────────────────────────────────────────────────────────
        // Execution
        // ────────────────────────────────────────────────────────────────────────────────

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            folderList = [];
            filesList = [];

            var templateDir = GetDefaultDirectory();

            var dirs = new List<string> { templateDir };
            DA.GetDataList(0, dirs);

            dirs = [.. dirs.Where(Directory.Exists)];
            if (dirs.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "No samples folder was found. Provide a valid path in _directory_.");
                DA.SetDataList(0, Array.Empty<string>());
                return;
            }

            foreach (var dir in dirs)
            {
                List<string> fs = Directory.GetFiles(dir, "*.gh*", SearchOption.AllDirectories).ToList();
                if (fs.Any())
                {
                    folderList.Add(Path.GetDirectoryName(Path.Combine(dir, "placeholder.txt"))!);
                    filesList.Add(fs);
                }
            }

            DA.SetDataList(0, filesList.SelectMany(_ => _));

            templateMenu = GetMenu();
        }

        // ────────────────────────────────────────────────────────────────────────────────
        // UI Helpers
        // ────────────────────────────────────────────────────────────────────────────────

        private Size GetMoveVector(PointF FromLocation)
        {
            var moveX = Attributes.Bounds.Left - 80 - FromLocation.X;
            var moveY = Attributes.Bounds.Y + 180 - FromLocation.Y;
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

                // Move to where this component is
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
            // Intentionally left in place for future surface into GH menu bar if needed.
            // The component uses its own button to show the same menu.


            Menu_AppendSeparator(menu);
            Menu_AppendItem(menu, "Open Directory", Menu_OpenDirectory, Resources.SAM_Small, true, false);

        }

        //public new void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        //{
        //    base.AppendAdditionalMenuItems(menu);

        //    Menu_AppendSeparator(menu);
        //    Menu_AppendItem(menu, "Open Directory", Menu_OpenDirectory, Resources.SAM_Small, true, false);
        //}

        private void Menu_OpenDirectory(object sender, EventArgs e)
        {
            int index_Path = Params.IndexOfInputParam("_directory_");
            if (index_Path == -1)
            {
                return;
            }

            string directory = null;

            if (Params.Input[index_Path].VolatileData.AllData(true)?.OfType<object>()?.FirstOrDefault() is IGH_Goo @object)
            {
                directory = (@object as dynamic).Value?.ToString();
            }

            if (string.IsNullOrWhiteSpace(directory) || !Directory.Exists(directory))
            {
                directory = GetDefaultDirectory();
            }

            if (string.IsNullOrWhiteSpace(directory) || !Directory.Exists(directory))
            {
                return;
            }

            Process.Start(new ProcessStartInfo()
            {
                FileName = directory,
                UseShellExecute = true, // Important to open in Explorer
                Verb = "open"           // Optional, default is "open"
            });
        }

        private string GetDefaultDirectory()
        {
            var root = Path.GetDirectoryName(GetType().Assembly.Location);
            var templateDir = Path.Combine(root ?? string.Empty, "Samples");

            return templateDir;
        }

        private ToolStripDropDownMenu GetMenu()
        {
            var menu = new ToolStripDropDownMenu();

            var rootFolder = folderList.FirstOrDefault();
            if (string.IsNullOrEmpty(rootFolder) || !Directory.Exists(rootFolder))
                return menu;

            var topDirs = Directory.GetDirectories(rootFolder);

            foreach (var item in topDirs)
            {
                var menuItem = addFromFolder(item);
                if (menuItem != null) menu.Items.Add(menuItem);
            }

            return menu;
        }

        ToolStripDropDownMenu templateMenu = new();

        private ToolStripMenuItem addFromFolder(string rootFolder)
        {
            var allFiles = Directory.GetFiles(rootFolder, "*.gh*", SearchOption.AllDirectories);
            if (!allFiles.Any()) return null;

            var topDirs = Directory.GetDirectories(rootFolder);
            var topFiles = Directory.GetFiles(rootFolder, "*.gh*", SearchOption.TopDirectoryOnly);

            // Create menu item for this folder
            var folderName = new DirectoryInfo(rootFolder).Name;
            var t = new ToolStripMenuItem(folderName);

            foreach (var item in topDirs)
            {
                var menuItem = addFromFolder(item);
                if (menuItem != null) t.DropDownItems.Add(menuItem);
            }

            foreach (var item in topFiles)
            {
                var name = Path.GetFileNameWithoutExtension(item);

                EventHandler ev = (object sender, EventArgs e) =>
                {
                    var a = sender as ToolStripDropDownItem;
                    var r = true;
                    CreateTemplateFromFilePath(a!.Tag!.ToString()!, ref r);
                    ExpireSolution(true);
                };

                Menu_AppendItem(t.DropDown, name, ev, null, item);
            }

            return t;
        }

        public override void CreateAttributes()
        {
            var att = new GH_SAMComponentButtonAttributes(this)
            {
                ButtonText = "Pick a sample folder"
            };

            att.MouseDownEvent += (object loc) => templateMenu.Show((GH.GUI.Canvas.GH_Canvas)loc, ((GH.GUI.Canvas.GH_Canvas)loc).CursorControlPosition);
            Attributes = att;
        }
    }
}
