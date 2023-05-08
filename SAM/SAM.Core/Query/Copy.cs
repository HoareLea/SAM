using System.IO;
using System.Diagnostics;
using System;

namespace SAM.Core
{
    public static partial class Query
    {
        /// <summary>
        /// Copy file even it is used by another process
        /// </summary>
        /// <param name="pathSource">Source Path</param>
        /// <param name="pathDestination">Destination Path</param>
        /// <param name="overwrite">Overwrite if file exists</param>
        /// <returns>true if succeeded</returns>
        public static bool Copy(string pathSource, string pathDestination, bool overwrite = true)
        {
            if(string.IsNullOrWhiteSpace(pathSource) || !File.Exists(pathSource) || string.IsNullOrWhiteSpace(pathDestination))
                return false;

            if (!overwrite && File.Exists(pathDestination))
                return false;

            if (!Directory.Exists(Path.GetDirectoryName(pathDestination)))
                return false;

            try
            {
                using (FileStream fileStream_Source = new FileStream(pathSource, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (FileStream fileStream_Destination = new FileStream(pathDestination, FileMode.Create))
                    {
                        var buffer = new byte[0x10000];
                        int bytes;

                        while ((bytes = fileStream_Source.Read(buffer, 0, buffer.Length)) > 0)
                            fileStream_Destination.Write(buffer, 0, bytes);
                    }
                }

                return true;
            }
            catch(Exception )
            {
                try
                {
                    using (Process process = new Process())
                    {
                        process.StartInfo.UseShellExecute = false;
                        process.StartInfo.RedirectStandardOutput = true;
                        process.StartInfo.FileName = "cmd.exe";
                        process.StartInfo.Arguments = string.Format("/C copy \"{0}\" \"{1}\"", pathSource, pathDestination);
                        process.Start();
                        process.WaitForExit();
                        process.Close();
                    }
                    return true;
                }
                catch
                {
                    return false;
                }

            }

        }
    }
}