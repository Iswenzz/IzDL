using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading;

using ShellProgressBar;
using Ookii.Dialogs.WinForms;

namespace Iswenzz.DL
{
    /// <summary>
    /// Represent a download entry.
    /// </summary>
    public abstract class AbstractDownload
    {
        public virtual string INFO_DIRNAME { get; set; }
        public virtual string INFO_FILENAME { get; set; }
        public virtual string IFNO_NAME { get; set; }
        public virtual string INFO_URL { get; set; }

        public virtual string NAME_LAUNCHER { get; set; }
        public virtual string NAME_EXE { get; set; }

        public virtual string PATH_INSTALLDIR { get; set; }
        public virtual string PATH_FILE { get; set; }
        public virtual string PATH_DIR { get; set; }
        public virtual string PATH_EXE { get; set; }
        public virtual string PATH_SHORTCUT { get; set; }
        public virtual string PATH_LAUNCHER { get; set; }

        public string TAB { get; } = "    ";

        public virtual Stopwatch Stopwatch { get; set; }
        public virtual ProgressBar ProgressBar { get; set; }
        public virtual int Tick { get; set; }

        /// <summary>
        /// <see cref="AbstractDownload"/> object.
        /// </summary>
        protected AbstractDownload()
        {
            Stopwatch = new Stopwatch();
        }

        /// <summary>
        /// Run the setup.
        /// </summary>
        public virtual void Setup()
        {
            Location();
            Download();
            Install();
        }

        /// <summary>
        /// Pick an installation directory.
        /// </summary>
        public virtual void Location()
        {
            string title = $"Install {GetType().Name} to this directory:\n";
            Console.WriteLine(title);

            if (OperatingSystem.IsWindowsVersionAtLeast(7))
            {
                VistaFolderBrowserDialog dialog = new()
                {
                    Description = title,
                    RootFolder = Environment.SpecialFolder.ProgramFiles,
                    UseDescriptionForTitle = true
                };

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    PATH_INSTALLDIR = dialog.SelectedPath;
                    if (string.IsNullOrEmpty(PATH_INSTALLDIR))
                        throw new Exception("Installation directory is empty.");
                }
            }
            else
                PATH_INSTALLDIR = Console.ReadLine();
            if (string.IsNullOrEmpty(PATH_INSTALLDIR))
                Environment.Exit(-1);
        }

        /// <summary>
        /// Download the archive and show real time progress.
        /// </summary>
        public virtual void Download()
        {
            using WebClient wc = new();
            PATH_DIR = Path.Combine(PATH_INSTALLDIR, INFO_DIRNAME);
            PATH_FILE = Path.Combine(PATH_DIR, INFO_FILENAME);
            Console.Clear();
            Console.WriteLine($"\nDownloading to: {PATH_FILE}\n");

            ProgressBarOptions options = new()
            {
                ForegroundColor = ConsoleColor.Cyan,
                ForegroundColorDone = ConsoleColor.DarkCyan,
                BackgroundColor = ConsoleColor.DarkGray,
                BackgroundCharacter = '\u2593'
            };

            // Get the archive size
            wc.OpenRead(INFO_URL);
            int maxBytes = Convert.ToInt32(wc.ResponseHeaders["Content-Length"]);

            // Create a progress bar & start downloading
            ProgressBar = new ProgressBar(maxBytes, $"Downloading ", options);
            wc.DownloadProgressChanged += OnDownloadProgressChange;
            wc.DownloadFileCompleted += OnDownloadFinish;
            Directory.CreateDirectory(PATH_DIR);

            // Wait for download to finish
            wc.DownloadFileTaskAsync(INFO_URL, PATH_FILE).Wait();
            ProgressBar.Dispose();
            Thread.Sleep(2000);
        }

        /// <summary>
        /// Install the software files.
        /// </summary>
        public abstract void Install();

        /// <summary>
        /// Get the download speed & progress ticks.
        /// </summary>
        /// <param name="currentTick">The current tick.</param>
        public virtual void ProgressTick(int currentTick)
        {
            if (Tick == 0)
            {
                Stopwatch.Restart();
                Tick = currentTick;
            }

            // Get the download speed/progress.
            Tick = currentTick;
            TimeSpan estimatedTime = TimeSpan.FromSeconds(
                (ProgressBar.MaxTicks - Tick) / 
                (Tick / Stopwatch.Elapsed.TotalSeconds));
            string ptick_formated = FormatSize(Tick);
            string maxtick_formated = FormatSize(ProgressBar.MaxTicks);
            string bytesPerSeconds_formated = FormatSize(Tick / Stopwatch.Elapsed.TotalSeconds) + "/s";

            ProgressBar.Tick(Tick, estimatedTime, 
                $"{INFO_FILENAME}{TAB}{ptick_formated}/{maxtick_formated}{TAB}{bytesPerSeconds_formated}");
        }

        /// <summary>
        /// Format file size to string.
        /// </summary>
        /// <param name="fileSize">The file size in bytes.</param>
        /// <returns></returns>
        public virtual string FormatSize(double fileSize)
        {
            string[] sizes = new[] { "B", "KB", "MB", "GB", "TB" };
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
        protected virtual void OnDownloadFinish(object sender, AsyncCompletedEventArgs e) =>
          Stopwatch.Stop();

        /// <summary>
        /// Update the progresss bar on download tick.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnDownloadProgressChange(object sender, DownloadProgressChangedEventArgs e) =>
            ProgressTick((int)e.BytesReceived);
    }
}
