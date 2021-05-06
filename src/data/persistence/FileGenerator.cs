using System;
using System.IO;
using System.Diagnostics;
using System.Security.AccessControl;

namespace Meminisse
{
    public class FileGenerator
    {
        /// <summary>
        /// Creates a file and necessary directories with dsf permissions using the fullPath argument to determine where and what
        /// the name of the file should be.
        /// </summary>
        /// <param name="fullPath">Path and filename combined</param>
        /// <returns></returns>
        public static FileStream CreateFileWithPermissions(string fullPath)
        {
            // Directory
            string directoryPath = Path.GetDirectoryName(fullPath);
            if ( !Directory.Exists(directoryPath) )
                Directory.CreateDirectory(directoryPath);

            // Create file
            FileStream fs = File.Create(fullPath);

            // Set permissions
            SetPermissions(fullPath);

            return fs;
        }

        private static void SetPermissions(string fullPath)
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32NT:
                #pragma warning disable CA1416
                    FileInfo fInfo = new FileInfo(fullPath);
                    FileSecurity fSecurity = fInfo.GetAccessControl();
                    FileSystemAccessRule rule = new FileSystemAccessRule("dsf", FileSystemRights.FullControl, AccessControlType.Allow);
                    fSecurity.AddAccessRule(rule);
                    fInfo.SetAccessControl(fSecurity);
                #pragma warning restore CA1416
                    break;

                case PlatformID.Unix:
                    SetPermissionsUnix(fullPath);
                    break;

                default:
                    string message = "Operating System not supported!";
                    Console.WriteLine(message);
                    System.Environment.FailFast(message);
                    break;
            }
        }

        /// <summary>
        /// Setting the permissions of the whole path to dsf.
        /// </summary>
        /// <param name="fullPath">redundant atm.</param>
        private static void SetPermissionsUnix(string fullPath)
        {
            // Create process
            using Process p = new Process();

            // Setup start info
            p.StartInfo = new ProcessStartInfo("chown", string.Format("-R dsf {0}", Config.COBODPath));
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.RedirectStandardOutput = false;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.UseShellExecute = false;
            p.ErrorDataReceived += (s, a) => Console.WriteLine(a.Data);

            // Start and wait
            p.Start();
            p.WaitForExit();
        }
    }
}