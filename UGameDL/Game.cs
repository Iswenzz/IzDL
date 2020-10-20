using ShellProgressBar;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Net;

namespace Iswenzz.UGameDL
{
    /// <summary>
    /// Utility Game Downloader
    /// </summary>
    public abstract class Game
    {
        public abstract string DL_DIRNAME { get; protected set; }
        public abstract string DL_FILENAME { get; protected set; }
        public abstract string DL_LAUNCHER { get; protected set; }
        public abstract string DL_URL { get; protected set; }

        public virtual ProgressBar ProgressBar { get; set; }
        public virtual Stopwatch Stopwatch { get; set; } = new Stopwatch();
        public virtual int Tick { get; set; }
        public virtual string InstallDir { get; set; }

        /// <summary>
        /// Run the setup.
        /// </summary>
        public virtual void Setup()
        {
            Choose();
            Download();
            Install();
        }

        /// <summary>
        /// Choose an installation directory.
        /// </summary>
        public abstract void Choose();

        /// <summary>
        /// Download the game and show real time progress.
        /// </summary>
        public abstract void Download();

        /// <summary>
        /// Install the game files & launchers.
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
        public virtual string FormatSize(double fileSize)
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
