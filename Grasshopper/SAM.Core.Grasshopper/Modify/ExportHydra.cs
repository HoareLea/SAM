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
            List<string> result = new List<string>();

            if(gH_Document == null)
            {
                result.Add("Invalid document");
                return result;
            }

            if (string.IsNullOrWhiteSpace(targetFolder) || !Directory.Exists(targetFolder))
            {
                targetFolder = "C:\\Temp\\ScriptsHydra";
            }

            if(string.IsNullOrWhiteSpace(Path.GetFileName(targetFolder)))
            {
                targetFolder = Directory.GetParent(targetFolder)?.FullName;
            }

            string baseDirectory = Directory.GetParent(targetFolder)?.FullName;

            if (Directory.Exists(targetFolder))
            {
                Directory.Delete(targetFolder, true);
            }

            if (!Directory.Exists(baseDirectory))
            {
                Directory.CreateDirectory(baseDirectory);
            }

            //0. Clone
            result.AddRange(ExecuteGitCommand_NEW(baseDirectory, "clone https://github.com/HoareLea/ScriptsHydra")); //$"clone https://github.com/HoareLea/ScriptsHydra \"{targetFolder}\""));

            RhinoDoc rhinoDoc = gH_Document.RhinoDocument;

            string safeFileName = fileName.Replace(" ", "_");

            result.Add($"Target folder used: {targetFolder}");

            targetFolder = Path.Combine(targetFolder, "3-Examples_GH", safeFileName);

            if(!Directory.Exists(targetFolder))
            {
                Directory.CreateDirectory(targetFolder);
            }

            string path_Rhino = rhinoDoc.Path;
            if (string.IsNullOrEmpty(path_Rhino) || !File.Exists(path_Rhino))
            {
                result.Add($"Error: GH file not saved.");
                return result;
            }

            string path_Grasshoper = Path.Combine(targetFolder, safeFileName + ".gh");
            File.Copy(path_Rhino, path_Grasshoper, true);

            // Write full-size Grasshopper image
            string path_GrasshopperImage = Write_GrasshopperImage(safeFileName, targetFolder);
            Bitmap bitmap_Grasshopper = File.Exists(path_GrasshopperImage) ? new Bitmap(path_GrasshopperImage) : null;

            // Copy Rhino file if needed & save Rhino image
            string path_RhinoImage = null;
            Bitmap bitmap_Rhino = null;
            if (includeRhino)
            {
                string rhinoPath = rhinoDoc.Path;
                if (string.IsNullOrEmpty(rhinoPath) || !File.Exists(rhinoPath))
                {
                    bitmap_Grasshopper?.Dispose();
                    result.Add("Error: Rhino file not saved.");
                    return result;
                }

                string rhinoTarget = Path.Combine(targetFolder, safeFileName + ".3dm");
                File.Copy(rhinoPath, rhinoTarget, true);

                path_RhinoImage = Write_RhinoImage(safeFileName, targetFolder, rhinoDoc);
                if (File.Exists(path_RhinoImage))
                {
                    bitmap_Rhino = new Bitmap(path_RhinoImage);
                }
            }

            // Copy additional files
            if (additionalFiles != null)
            {
                foreach (string additionalFile in additionalFiles)
                {
                    if (File.Exists(additionalFile))
                    {
                        File.Copy(additionalFile, Path.Combine(targetFolder, Path.GetFileName(additionalFile)), true);
                    }
                }
            }

            // Make thumbnail (aspect ratio, width 200px, from GH or Rhino image)
            string path_ThumbnailImage = Write_ThumbnailImage(bitmap_Grasshopper, bitmap_Rhino, gHForThumb, targetFolder);

            // Dispose images
            bitmap_Grasshopper?.Dispose();
            bitmap_Rhino?.Dispose();

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
            List<IGH_DocumentObject> documentObjects = gH_Document.Objects.ToList();
            Dictionary<string, object> metaDataDictionary = GetMetaData(documentObjects, fileTags?.ToList(), safeFileName, path_RhinoImage, additionalFiles, gHForThumb);
            WriteMetadataFile(safeFileName, targetFolder, metaDataDictionary);

            // Zip everything for upload
            string zipPath = Path.Combine(Path.GetDirectoryName(targetFolder), safeFileName + ".zip");
            if (File.Exists(zipPath))
            {
                File.Delete(zipPath);
            }

            ZipFile.CreateFromDirectory(targetFolder, zipPath);

            // ========== GIT OPERATIONS ==========
            string directory_Root = Directory.GetParent(Directory.GetParent(targetFolder).FullName).FullName;

            string branchName = githubUserName + "_" + fileName;

            // 1. Pull latest
            result.AddRange(ExecuteGitCommand_NEW(directory_Root, "pull"));

            // 2. Create new branch
            result.AddRange(ExecuteGitCommand_NEW(directory_Root, $"checkout -b {branchName}"));

            // 3. Add all
            result.AddRange(ExecuteGitCommand_NEW(directory_Root, "add ."));

            // 4. Commit
            string commitMsg = $"Added {safeFileName}";
            result.AddRange(ExecuteGitCommand_NEW(directory_Root, $"commit -m \"{commitMsg}\""));

            // 5. Push new branch
            result.AddRange(ExecuteGitCommand_NEW(directory_Root, $"push -u origin {branchName}"));

            if (Directory.Exists(baseDirectory))
            {
                Directory.Delete(baseDirectory, true);
            }

            result.Add("Cleaning done");

            // 6. Generate pull request link
            string repoUrl = $"https://github.com/HoareLea/ScriptsHydra/compare/{branchName}?expand=1";

            result.Add($"Export and Git push successful: {zipPath}\n\nTo create a pull request, open this link in your browser:\n{repoUrl}");

            Core.Query.StartProcess(repoUrl);
            
            return result;
        }

        // Save hi-res image of Grasshopper canvas
        private static string Write_GrasshopperImage(string fileName, string targetFolder)
        {
            GH_Canvas canvas = Instances.ActiveCanvas;
            RectangleF rectF = canvas.Document.BoundingBox(false);
            Rectangle rect = Rectangle.Truncate(rectF);
            Size size;
            string tmpPath = canvas.GenerateHiResImage(rect, new GH_Canvas.GH_ImageSettings(), out size)[0];
            string ghImgPath = Path.Combine(targetFolder, fileName + "_GH.png");
            File.Copy(tmpPath, ghImgPath, true);
            File.Delete(tmpPath);
            return ghImgPath;
        }

        // Save screenshot of active Rhino view
        private static string Write_RhinoImage(string fileName, string repoTargetFolder, RhinoDoc rhinoDoc)
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
        private static string Write_ThumbnailImage(Bitmap bitmap_Grasshopper, Bitmap bitmap_Rhino, bool gHForThumb, string targetFolder)
        {
            int width = 200;
            int height = 200;

            Bitmap bitmap_Source = bitmap_Rhino;
            if (gHForThumb)
            {
                bitmap_Source = bitmap_Grasshopper;
            }

            if (bitmap_Source == null)
            {
                return null;
            }

            double imageRatio = bitmap_Source.Height / bitmap_Source.Width;
            height = (int)(width * imageRatio);
            Size imgSize = new Size(width, height);

            Bitmap thumbnail = new Bitmap(bitmap_Source, imgSize);
            string thumbnailPath = Path.Combine(targetFolder, "thumbnail.png");
            thumbnail.Save(thumbnailPath);
            thumbnail.Dispose();
            return thumbnailPath;
        }

        // Compose metadata as dictionary (JSON serializable)
        private static Dictionary<string, object> GetMetaData(IEnumerable<IGH_DocumentObject> documentObjects, IEnumerable<string> fileTags, string fileName, string path_Rhino, List<string> additionalFiles, bool gHForThumb)
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

            foreach (IGH_DocumentObject documentObject in documentObjects)
            {
                string componentName = documentObject.Name;
                if (notImportantComps.Contains(componentName)) continue;
                if (components.ContainsKey(componentName))
                {
                    components[componentName]++;
                }
                else
                {
                    components[componentName] = 1;
                }

                Type type = documentObject.GetType();
                string category = (type.GetProperty("Category")?.GetValue(documentObject, null) ?? "User").ToString();
                string subcategory = (type.GetProperty("SubCategory")?.GetValue(documentObject, null) ?? "").ToString();
                if (!dependencies.Contains(category) && !string.IsNullOrEmpty(category))
                {
                    dependencies.Add(category);
                }
            }

            metaDataDict["components"] = components;
            metaDataDict["dependencies"] = dependencies;
            metaDataDict["tags"] = fileTags == null ? new List<string>() : fileTags.ToList();

            // Images
            images.Add(new Dictionary<string, string> { { fileName + "_GH.png", "Grasshopper Definition" } });
            if (!gHForThumb)
            {
                images.Add(new Dictionary<string, string> { { fileName + "_Rhino.png", "Rhino Viewport Screenshot" } });
            }
            
            metaDataDict["images"] = images;

            // Addfiles
            if (additionalFiles != null)
            {
                foreach (string file in additionalFiles)
                {
                    if (File.Exists(file))
                    {
                        addFiles.Add(new Dictionary<string, string> { { Path.GetFileName(file), "Additional file" } });
                    }
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
        private static void ExecuteGitCommand(string workingDirectory, string command)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = "git",
                Arguments = command,
                WorkingDirectory = workingDirectory,
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

        private static List<string> ExecuteGitCommand_NEW(string workingDirectory, string command)
        {
            List<string> result = new List<string>();

           using(Process process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = command,
                    WorkingDirectory = workingDirectory,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                }
            })
            {
                try
                {
                    process.OutputDataReceived += (sender, e) =>
                    {
                        if (!string.IsNullOrWhiteSpace(e.Data))
                        {
                            result.Add(e.Data.Trim());
                        }
                    };

                    process.ErrorDataReceived += (sender, e) =>
                    {
                        if (!string.IsNullOrWhiteSpace(e.Data))
                        {
                            result.Add("[GIT ERROR]\n" + e.Data.Trim());
                        }
                    };

                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                    process.WaitForExit(1000);
                }
                catch (Exception ex)
                {
                    result.Add("Failed to start git process: " + ex.Message);
                }
            }

            return result;
        }

    }
}