using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading;
using CSharpLib;
using Iswenzz.UGameDL.UGameDL;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using ShellProgressBar;

namespace Iswenzz.UGameDL.Games
{
    /// <summary>
    /// Quake 3 Defrag + Reshade + HD Pack
    /// </summary>
    public class Q3 : Game
    {
        public override string DL_DIRNAME { get; protected set; }   = "q3";
        public override string DL_FILENAME { get; protected set; }  = "q3.zip";
        public override string DL_LAUNCHER { get; protected set; }  = "defrag_launcher_installer.exe";
        public override string DL_URL { get; protected set; } = "https://iswenzz.com:1337/games/q3/q3.zip";

        /// <summary>
        /// Choose an installation directory.
        /// </summary>
        public override void Choose()
        {
            Console.WriteLine("Install Q3 to this directory (it will create a q3 directory)");
            CommonOpenFileDialog dialog = new CommonOpenFileDialog
            {
                Title = "Install Q3 to this directory (it will create a q3 directory)",
                InitialDirectory = "C:\\Program Files",
                IsFolderPicker = true
            };

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                InstallDir = dialog.FileName;
                if (string.IsNullOrEmpty(InstallDir))
                    Environment.Exit(-1);
                Console.WriteLine($"\nDownloading to: {Path.Combine(InstallDir, DL_DIRNAME, DL_FILENAME)}\n");
            }
            else
                Environment.Exit(-1);
            Thread.Sleep(3000);
        }

        /// <summary>
        /// Download the game and show real time progress.
        /// </summary>
        public override void Download()
        {
            using (WebClient wc = new WebClient())
            {
                ProgressBarOptions options = new ProgressBarOptions
                {
                    ForegroundColor = ConsoleColor.Cyan,
                    ForegroundColorDone = ConsoleColor.DarkCyan,
                    BackgroundColor = ConsoleColor.DarkGray,
                    BackgroundCharacter = '\u2593'
                };

                // Get the game size
                wc.OpenRead(DL_URL);
                int maxBytes = Convert.ToInt32(wc.ResponseHeaders["Content-Length"]);

                // Create a progress bar & start downloading
                ProgressBar = new ProgressBar(maxBytes, $"Downloading ", options);
                wc.DownloadProgressChanged += OnDownloadProgressChange;
                wc.DownloadFileCompleted += OnDownloadFinish;
                Directory.CreateDirectory(Path.Combine(InstallDir, DL_DIRNAME));
                wc.DownloadFileTaskAsync(DL_URL, Path.Combine(InstallDir, DL_DIRNAME, DL_FILENAME)).Wait();
                ProgressBar.Dispose();
            }
            Thread.Sleep(2000);
        }

        /// <summary>
        /// Install the game files & launchers.
        /// </summary>
        public override void Install()
        {
            Console.WriteLine("\nInstalling . . .\n");

            // Unzip the game
            Console.WriteLine("Extract");
            string zipPath = Path.Combine(InstallDir, DL_DIRNAME, DL_FILENAME);
            string outPath = Path.Combine(InstallDir, DL_DIRNAME);
            using (ZipArchive archive = ZipFile.OpenRead(zipPath))
                archive.ExtractToDirectory(outPath, true);

            // Start the launcher installer
            Console.WriteLine("Launcher Install");
            Process proc = new Process();
            proc.StartInfo.FileName = Path.Combine(InstallDir, DL_DIRNAME, DL_LAUNCHER);
            proc.Start();
            proc.WaitForExit();

            // REGISTRY for the launcher
            Console.WriteLine("Launcher Settings");
            RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\mDd\defrag launcher");
            key.SetValue("enginepath", Path.Combine(outPath, "iDFe.x64.exe"));
            key.SetValue("autostart", 1);

            // Create Shortcut
            Console.WriteLine("Game Shortcut");
            Shortcut shortcut = new Shortcut();
            shortcut.CreateShortcutToFile(Path.Combine(outPath, "iDFe.x64.exe"), 
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Q3.lnk"));
        }
    }
}
