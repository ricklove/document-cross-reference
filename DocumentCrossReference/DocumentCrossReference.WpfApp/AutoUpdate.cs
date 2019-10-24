using System.IO;

namespace DocumentCrossReference.WpfApp
{
    public static class AutoUpdate
    {
        /// <summary>
        /// A simple Auto Update based on copy installation from a network drive
        /// </summary>
        public static bool RunAutoUpdate()
        {
            try
            {
                var configFilePath = Path.Combine(Directory.GetCurrentDirectory(), "setup.config");
                if (!File.Exists(configFilePath)) { return false; }

                var configDoc = File.ReadAllText(configFilePath);
                var sourceDir = configDoc;

                if (!Directory.Exists(sourceDir)) { return false; }

                var setupDir = Path.GetDirectoryName(sourceDir.TrimEnd('/', '\\'));
                var setupFilePath = Path.Combine(setupDir, "setup.exe");

                if (!File.Exists(setupFilePath)) { return false; }

                // Check if current exe is older
                var exeFilePath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
                var sourceExeFilePath = Path.Combine(sourceDir, Path.GetFileName(exeFilePath));

                if (!File.Exists(sourceExeFilePath)) { return false; }
                if (File.GetLastWriteTimeUtc(exeFilePath) >= File.GetLastWriteTimeUtc(sourceExeFilePath)) { return false; }

                // Run setup to Reinstall
                var processInfo = new System.Diagnostics.ProcessStartInfo(setupFilePath) { WorkingDirectory = setupDir };
                System.Diagnostics.Process.Start(processInfo);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
