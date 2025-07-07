using Grasshopper;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Rhino;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;

namespace SAM.Core.Grasshopper
{
    public static partial class Modify
    {
        public static List<string> ExportHydra(this GH_Document gH_Document, string githubUserName, string fileName, List<string> fileDescription, IEnumerable<string> changeLog, IEnumerable<string> fileTags, string targetFolder, bool includeRhino, bool gHForThumb, List<string> additionalFiles)
        {
            return null;


            List<string> result = new List<string>();

            if(gH_Document == null)
            {
                result.Add("Invalid document");
                return result;
            }

            if (string.IsNullOrWhiteSpace(targetFolder) || !Directory.Exists(targetFolder))
            {
                targetFolder = "C:\\Temp\\ScriptsHydra\\";
            }

            RhinoDoc rhinoDoc = gH_Document.RhinoDocument;

            string safeFileName = fileName.Replace(" ", "_");

            result.Add($"Target folder used: {targetFolder}");

            targetFolder = Path.Combine(targetFolder, "3-Examples_GH", safeFileName);

            if(!Directory.Exists(targetFolder))
            {
                Directory.CreateDirectory(targetFolder);
            }

            string ghPath = rhinoDoc.Path;
            if (string.IsNullOrEmpty(ghPath) || !File.Exists(ghPath))
            {
                result.Add($"Error: GH file not saved.");
                return result;
            }

            string ghTarget = Path.Combine(targetFolder, safeFileName + ".gh");
            File.Copy(ghPath, ghTarget, true);

            // Write full-size Grasshopper image
            string ghImgPath = WriteGHImage(safeFileName, targetFolder);
            Bitmap ghImg = File.Exists(ghImgPath) ? new Bitmap(ghImgPath) : null;

            // Copy Rhino file if needed & save Rhino image
            string rhinoImgPath = null;
            Bitmap rhinoImg = null;
            if (includeRhino)
            {
                string rhinoPath = rhinoDoc.Path;
                if (string.IsNullOrEmpty(rhinoPath) || !File.Exists(rhinoPath))
                {
                    ghImg?.Dispose();
                    result.Add("Error: Rhino file not saved.");
                    return result;
                }
                string rhinoTarget = Path.Combine(targetFolder, safeFileName + ".3dm");
                File.Copy(rhinoPath, rhinoTarget, true);

                rhinoImgPath = WriteRhinoImage(safeFileName, targetFolder, rhinoDoc);
                if (File.Exists(rhinoImgPath))
                    rhinoImg = new Bitmap(rhinoImgPath);
            }

            // Copy additional files
            if (additionalFiles != null)
            {
                foreach (string additionalFile in additionalFiles)
                {
                    if (File.Exists(additionalFile))
                        File.Copy(additionalFile, Path.Combine(targetFolder, Path.GetFileName(additionalFile)), true);
                }
            }

            // Make thumbnail (aspect ratio, width 200px, from GH or Rhino image)
            string thumbnailPath = MakeThumbnailImg(ghImg, rhinoImg, gHForThumb, targetFolder);

            // Dispose images
            ghImg?.Dispose();
            rhinoImg?.Dispose();

            // Write README.md
            result.Add("### Description");
            foreach (string line in fileDescription)
            {
                result.Add(line);
            }

            if (changeLog != null && changeLog.Count() > 0)
            {
                result.Add("\n### Changelog");
                foreach (string line in changeLog)
                {
                    result.Add(line);
                }
            }

            if (fileTags != null && fileTags.Count() > 0)
            {
                result.Add("\n### Tags");
                result.Add(string.Join(", ", fileTags));
            }

            result.Add("\n### Thumbnail");
            result.Add("![Screenshot](thumbnail.png)");
            File.WriteAllText(Path.Combine(targetFolder, "README.md"), string.Join("\n", result).ToString());

            // Write input.json (metadata)
            List<IGH_DocumentObject> ghComps = GetAllTheComponents(gH_Document);
            Dictionary<string, object> metaDataDict = GetMetaData(ghComps, fileTags?.ToList(), safeFileName, rhinoImgPath, additionalFiles, gHForThumb);
            WriteMetadataFile(safeFileName, targetFolder, metaDataDict);

            // Zip everything for upload
            string zipPath = Path.Combine(Path.GetDirectoryName(targetFolder), safeFileName + ".zip");
            if (File.Exists(zipPath)) File.Delete(zipPath);
            ZipFile.CreateFromDirectory(targetFolder, zipPath);

            // ========== GIT OPERATIONS ==========
            string repoRoot = Directory.GetParent(Directory.GetParent(targetFolder).FullName).FullName;

            string branchName = githubUserName + "_" + fileName;

            // 1. Pull latest
            ExecuteGitCommand(repoRoot, "pull");

            // 2. Create new branch
            ExecuteGitCommand(repoRoot, $"checkout -b {branchName}");

            // 3. Add all
            ExecuteGitCommand(repoRoot, "add .");

            // 4. Commit
            string commitMsg = $"Added {safeFileName}";
            ExecuteGitCommand(repoRoot, $"commit -m \"{commitMsg}\"");

            // 5. Push new branch
            ExecuteGitCommand(repoRoot, $"push -u origin {branchName}");

            // 6. Generate pull request link
            string repoUrl = $"https://github.com/HoareLea/ScriptsHydra/compare/{branchName}?expand=1";

            result.Add($"Export and Git push successful: {zipPath}\n\nTo create a pull request, open this link in your browser:\n{repoUrl}");
            return result;

        }

        // Save hi-res image of Grasshopper canvas
        private static string WriteGHImage(string fileName, string repoTargetFolder)
        {
            GH_Canvas canvas = Instances.ActiveCanvas;
            RectangleF rectF = canvas.Document.BoundingBox(false);
            Rectangle rect = Rectangle.Truncate(rectF);
            Size size;
            string tmpPath = canvas.GenerateHiResImage(rect, new GH_Canvas.GH_ImageSettings(), out size)[0];
            string ghImgPath = Path.Combine(repoTargetFolder, fileName + "_GH.png");
            File.Copy(tmpPath, ghImgPath, true);
            File.Delete(tmpPath);
            return ghImgPath;
        }

        // Save screenshot of active Rhino view
        private static string WriteRhinoImage(string fileName, string repoTargetFolder, RhinoDoc rhinoDoc)
        {
            var view = rhinoDoc.Views.ActiveView;
            int image_w = view.ActiveViewport.Size.Width;
            int image_h = view.ActiveViewport.Size.Height;
            Size viewSize = new Size(image_w, image_h);
            Bitmap pic = view.CaptureToBitmap(viewSize);

            string fullPath = Path.Combine(repoTargetFolder, fileName + "_Rhino.png");
            pic.Save(fullPath);
            pic.Dispose();
            return fullPath;
        }

        // Make a 200px wide thumbnail from GH or Rhino image (aspect ratio)
        private static string MakeThumbnailImg(Bitmap ghImg, Bitmap rhinoImg, bool GHForThumb_, string repoTargetFolder)
        {
            int thumbnailW = 200;
            int thumbnailH = 200;

            Bitmap sourceImg = rhinoImg;
            if (GHForThumb_)
            {
                sourceImg = ghImg;
            }

            if (sourceImg == null)
            {
                return null;
            }

            double imgRatio = (double)sourceImg.Height / (double)sourceImg.Width;
            thumbnailH = (int)(thumbnailW * imgRatio);
            Size imgSize = new Size(thumbnailW, thumbnailH);

            Bitmap thumbnail = new Bitmap(sourceImg, imgSize);
            string thumbnailPath = Path.Combine(repoTargetFolder, "thumbnail.png");
            thumbnail.Save(thumbnailPath);
            thumbnail.Dispose();
            return thumbnailPath;
        }

        // Gather all components (for metadata)
        private static List<IGH_DocumentObject> GetAllTheComponents(GH_Document document)
        {
            return document.Objects.ToList();
        }

        // Compose metadata as dictionary (JSON serializable)
        private static Dictionary<string, object> GetMetaData(
            List<IGH_DocumentObject> ghComps,
            List<string> fileTags,
            string fileName,
            string rhinoDocPath,
            List<string> additionalFiles,
            bool GHForThumb_
            )
        {
            Dictionary<string, object> metaDataDict = new Dictionary<string, object>();
            Dictionary<string, int> components = new Dictionary<string, int>();
            List<string> dependencies = new List<string>();
            List<Dictionary<string, string>> images = new List<Dictionary<string, string>>();
            List<Dictionary<string, string>> addFiles = new List<Dictionary<string, string>>();

            // These lists can be adapted to match what you want to filter
            string[] notImportantComps = new string[] {
            "Hydra", "Scribble", "SAMHydra_ExportFile", "SAMHydra_ImportFile", "Group", "Panel", "Slider", "Boolean Toggle", "Custom Preview", "Colour Swatch",
            "Button", "Control Knob", "Digit Scroller", "MD Slider", "Value List", "Point", "Vector", "Circle", "Circular Arc", "Curve", "Line", "Plane", "Rectangle", "Box", "Mesh", "Mesh Face", "Surface", "Twisted Box", "Field", "Geometry", "Geometry Cache", "Geometry Pipeline", "Transform"
        };

            foreach (IGH_DocumentObject comp in ghComps)
            {
                string componentName = comp.Name;
                if (notImportantComps.Contains(componentName)) continue;
                if (components.ContainsKey(componentName))
                    components[componentName]++;
                else
                    components[componentName] = 1;

                Type objType = comp.GetType();
                string category = (objType.GetProperty("Category")?.GetValue(comp, null) ?? "User").ToString();
                string subcategory = (objType.GetProperty("SubCategory")?.GetValue(comp, null) ?? "").ToString();
                if (!dependencies.Contains(category) && !string.IsNullOrEmpty(category))
                    dependencies.Add(category);
            }

            metaDataDict["components"] = components;
            metaDataDict["dependencies"] = dependencies;
            metaDataDict["tags"] = fileTags ?? new List<string>();

            // Images
            images.Add(new Dictionary<string, string> { { fileName + "_GH.png", "Grasshopper Definition" } });
            if (!GHForThumb_)
                images.Add(new Dictionary<string, string> { { fileName + "_Rhino.png", "Rhino Viewport Screenshot" } });
            metaDataDict["images"] = images;

            // Addfiles
            if (additionalFiles != null)
            {
                foreach (string file in additionalFiles)
                {
                    if (File.Exists(file))
                        addFiles.Add(new Dictionary<string, string> { { Path.GetFileName(file), "Additional file" } });
                }
            }
            metaDataDict["Addfiles"] = addFiles;

            metaDataDict["thumbnail"] = "thumbnail.png";
            metaDataDict["file"] = fileName + ".zip";
            return metaDataDict;
        }

        // Write metadata as JSON
        private static void WriteMetadataFile(string fileName, string repoTargetFolder, Dictionary<string, object> metaDataDict)
        {
            string jsonFilePath = Path.Combine(repoTargetFolder, "input.json");
            JsonSerializerOptions options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(metaDataDict, options);
            File.WriteAllText(jsonFilePath, json);
        }

        // Run git commands (helper)
        private static void ExecuteGitCommand(string workingDir, string command)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = "git",
                Arguments = command,
                WorkingDirectory = workingDir,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                UseShellExecute = false
            };
            using (Process process = Process.Start(startInfo))
            {
                process.WaitForExit();
            }
        }
    }
}