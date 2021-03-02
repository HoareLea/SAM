using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace SAM.Core.Grasshopper
{
    public static partial class Modify
    {
        public static bool RunGrasshopperScript(string path_Grasshopper, string path_Rhinoceros = null, string rhinocerosVersion = null)
        {
            if (string.IsNullOrEmpty(path_Grasshopper) || !File.Exists(path_Grasshopper))
                return false;

            string path = Query.RhinocerosExePath(rhinocerosVersion);
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                return false;

            string directory = Path.GetDirectoryName(path_Grasshopper);

            List<string> lines = new List<string>();
            lines.Add(@"@ECHO OFF");
            lines.Add(string.Format("cd {0}", directory));
            lines.Add(string.Format("\"{0}\" /nosplash /runscript=\"-grasshopper editor load document open {1} _enter -exit\"", path, path_Grasshopper));

            if (!string.IsNullOrWhiteSpace(path_Rhinoceros) && File.Exists(path_Rhinoceros))
                lines[lines.Count - 1] += string.Format(" \"{0}\"", path_Rhinoceros);

            lines.Add(@"exit");

            string command = string.Join(System.Environment.NewLine, lines);

            string path_Temp = Path.Combine(Path.GetTempPath(), Path.GetTempFileName() + ".bat");

            try
            {
                File.WriteAllText(path_Temp, command);

                ProcessStartInfo processStartInfo = new ProcessStartInfo(path_Temp);
                processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                
                Process process = Process.Start(processStartInfo);
                process.WaitForExit();
            }
            catch
            {

            }
            finally
            {
                if (File.Exists(path_Temp))
                    File.Delete(path_Temp);
            }

            return true;
        }
    }
}