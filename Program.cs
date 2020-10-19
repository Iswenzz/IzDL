using Microsoft.WindowsAPICodePack.Dialogs;
using ShellProgressBar;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading;

namespace Iswenzz.Q3Downloader
{
    /// <summary>
    /// Q3 Downloader
    /// </summary>
    public static class Program
    {
        const string DL_DIRNAME = "q3";
        const string DL_FILENAME = "q3.zip";
        const string DL_LAUNCHER = "defrag_launcher_installer.exe";
        const string DL_URL = "https://iswenzz.com:1337/fastdl/usermaps/mp_deathrun_boss/mp_deathrun_boss.iwd";
        //const string DL_URL = "https://iswenzz.com:1337/games/q3/q3.zip";

        public static ProgressBar ProgressBar { get; set; }
        public static Stopwatch Stopwatch { get; set; } = new Stopwatch();
        public static int Tick { get; set; }
        public static string InstallDir { get; set; }

        /// <summary>
        /// The entry point of the program.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            Choose();
            Download();
            Install();
            Console.ReadLine();
        }

        /// <summary>
        /// Choose an installation directory.
        /// </summary>
        public static void Choose()
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
        public static void Download()
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
        public static void Install()
        {
            Console.WriteLine("\nInstalling . . .\n");

            // Unzip the game
            string zipPath = Path.Combine(InstallDir, DL_DIRNAME, DL_FILENAME);
            string outPath = Path.Combine(InstallDir, DL_DIRNAME);
            using (ZipArchive archive = ZipFile.OpenRead(zipPath))
                archive.ExtractToDirectory(outPath, true);

            // Start the launcher installer
            Process proc = new Process();
            proc.StartInfo.FileName = Path.Combine(InstallDir, DL_DIRNAME, DL_LAUNCHER);
            proc.Start();
            proc.WaitForExit();

            // REGISTRY for the launcher
        }

        /// <summary>
        /// Get the download speed & progress ticks.
        /// </summary>
        /// <param name="currentTick">The current tick.</param>
        public static void ProgressTick(int currentTick)
        {
            if (Tick == 0)
            {
                Stopwatch.Restart();
                Tick = currentTick;
            }

            // Get the download speed/progress.
            Tick = currentTick;
            TimeSpan estimatedTime = TimeSpan.FromSeconds((ProgressBar.MaxTicks - Tick) / (Tick / Stopwatch.Elapsed.TotalSeconds));
            string ptick_formated = FormatSize(Tick);
            string maxtick_formated = FormatSize(ProgressBar.MaxTicks);
            string bytesPerSeconds_formated = FormatSize(Tick / Stopwatch.Elapsed.TotalSeconds) + "/s";

            ProgressBar.Tick(Tick, estimatedTime, $"{DL_FILENAME}       {ptick_formated}/{maxtick_formated}     {bytesPerSeconds_formated}");
        }

        /// <summary>
        /// Format file size to string.
        /// </summary>
        /// <param name="fileSize">The file size in bytes.</param>
        /// <returns></returns>
        public static string FormatSize(double fileSize)
        {
            string[] sizes = new string[] { "B", "KB", "MB", "GB", "TB" };
            int order = 0;

            while (fileSize >= 1024 && order < sizes.Length - 1)
            {
                order++;
                fileSize /= 1024;
            }
            return fileSize.ToString("0.##", CultureInfo.InvariantCulture) + " " + sizes[order];
        }

        /// <summary>
        /// On download finish callback.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnDownloadFinish(object sender, AsyncCompletedEventArgs e) =>
          Stopwatch.Stop();

        /// <summary>
        /// Update the progresss bar on download tick.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnDownloadProgressChange(object sender, DownloadProgressChangedEventArgs e) => 
            ProgressTick((int)e.BytesReceived);

        /// <summary>
        /// ZipArchive extension method to extract a zip archive to an output directory and choose
        /// to overwrite files.
        /// </summary>
        /// <param name="archive">The archive to extract.</param>
        /// <param name="destinationDirectoryName">The destination directory.</param>
        /// <param name="overwrite">Overwrite the files.</param>
        public static void ExtractToDirectory(this ZipArchive archive, string destinationDirectoryName, bool overwrite)
        {
            if (!overwrite)
            {
                archive.ExtractToDirectory(destinationDirectoryName);
                return;
            }
            DirectoryInfo di = Directory.CreateDirectory(destinationDirectoryName);
            string destinationDirectoryFullPath = di.FullName;

            foreach (ZipArchiveEntry file in archive.Entries)
            {
                string completeFileName = Path.GetFullPath(Path.Combine(destinationDirectoryFullPath, file.FullName));

                if (!completeFileName.StartsWith(destinationDirectoryFullPath, StringComparison.OrdinalIgnoreCase))
                {
                    throw new IOException("Trying to extract file outside of destination directory. See this link for more info: https://snyk.io/research/zip-slip-vulnerability");
                }

                if (file.Name == "")
                {
                    // Assuming Empty for Directory
                    Directory.CreateDirectory(Path.GetDirectoryName(completeFileName));
                    continue;
                }
                file.ExtractToFile(completeFileName, true);
            }
        }
    }
}
