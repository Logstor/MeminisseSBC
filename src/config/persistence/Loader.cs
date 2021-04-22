using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Security.AccessControl;
using Newtonsoft.Json;

namespace Meminisse.Configuration
{
    internal static class Loader
    {
        internal static Config Load(string fullPath)
        {
            try 
            {
                // Check if path and file exists
                if ( File.Exists(fullPath) )
                {
                    string json = File.ReadAllText(fullPath);
                    return JsonConvert.DeserializeObject<Config>(json);
                }
                else
                {
                   return CreateEverything(fullPath);
                }
            }
            catch (Exception e)    
            {
                Console.WriteLine("ERROR Loading or Creating configuration file!");
                Console.WriteLine(e.ToString());
                Console.WriteLine("Continuous with default configuration");
                return new Config();
            }
        }

        private static Config CreateEverything(string fullPath)
        {
            Config config = new Config();
            string json = JsonConvert.SerializeObject(config, Formatting.Indented);

            // Make sure to create Directories
            Directory.CreateDirectory(Config.ConfigurationPath);

            // Write to config file
            File.WriteAllTextAsync(fullPath, json, Encoding.UTF8);

            // Set permissions
            SetPermissions(fullPath);

            return config;
        }

        private static void SetPermissions(string fullPath)
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32NT:
                    FileInfo fInfo = new FileInfo(fullPath);
                    FileSecurity fSecurity = fInfo.GetAccessControl();
                    FileSystemAccessRule rule = new FileSystemAccessRule("dsf", FileSystemRights.FullControl, AccessControlType.Allow);
                    fSecurity.AddAccessRule(rule);
                    fInfo.SetAccessControl(fSecurity);
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

        private static void SetPermissionsUnix(string fullPath)
        {
            // Create process
            using Process p = new Process();

            // Setup start info
            p.StartInfo = new ProcessStartInfo("chown", string.Format("dsf {0}", fullPath));
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