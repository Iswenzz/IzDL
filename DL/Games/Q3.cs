using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;

using Microsoft.Win32;

namespace Iswenzz.DL.Games
{
    /// <summary>
    /// Quake 3 Defrag & Reshade & HD Pack
    /// </summary>
    public class Q3 : AbstractDownload
    {
        /// <summary>
        /// Setup <see cref="Q3"/> infos.
        /// </summary>
        public Q3()
        {
            IFNO_NAME       = "Q3";
            INFO_DIRNAME    = "q3";
            INFO_FILENAME   = "q3.zip";
            INFO_URL        = "https://iswenzz.com:1337/games/q3/q3.zip";
            NAME_LAUNCHER   = "defrag_launcher_installer.exe";
            NAME_EXE        = "iDFe.x64.exe";
        }

        /// <summary>
        /// Install the archive files.
        /// </summary>
        public override void Install()
        {
            Console.WriteLine("\nInstalling . . .\n");
            PATH_EXE = Path.Combine(PATH_DIR, NAME_EXE);
            PATH_LAUNCHER = Path.Combine(PATH_DIR, NAME_LAUNCHER);
            PATH_SHORTCUT = Path.Combine(Environment.GetFolderPath(
                Environment.SpecialFolder.Desktop), $"{IFNO_NAME}.lnk");

            // Unzip the archive
            Console.WriteLine("Extract");
            using ZipArchive archive = ZipFile.OpenRead(PATH_FILE);
            archive.ExtractToDirectory(PATH_DIR, true);

            // Start the launcher installer
            Console.WriteLine("Launcher Install");
            Process proc = new();
            proc.StartInfo.FileName = PATH_LAUNCHER;
            proc.Start();
            proc.WaitForExit();

            // REGISTRY for the launcher
            if (OperatingSystem.IsWindows())
            {
                Console.WriteLine("Launcher Settings");
                RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\mDd\defrag launcher");
                key.SetValue("enginepath", PATH_EXE);
                key.SetValue("autostart", 1);
            }

            // Create Shortcut
            Console.WriteLine("Shortcut");
            Utils.CreateShortcutToFile(PATH_EXE, PATH_SHORTCUT);
        }
    }
}
