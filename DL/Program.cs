using System.Collections.Generic;
using System.Linq;
using System;

using Iswenzz.DL.Games;

namespace Iswenzz.DL
{
    /// <summary>
    /// Downloader Program.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// List of downloads.
        /// </summary>
        public static Dictionary<string, AbstractDownload> Downloads { get; set; } = new() 
        { 
            { "Q3", new Q3() } 
        };

        /// <summary>
        /// The entry point of the program.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            PickDownload();

            Console.WriteLine("\nDone!");
            Console.ReadLine();
        }

        /// <summary>
        /// Print all download entries.
        /// </summary>
        private static void PrintEntries()
        {
            Console.Title = "IzDL (c) Iswenzz";
            for (int i = 0; i < Downloads.Count; i++)
                Console.WriteLine($"{i + 1}. {Downloads.ElementAt(i).Key}");
        }

        /// <summary>
        /// Pick a download.
        /// </summary>
        /// <exception cref="Exception"></exception>
        private static void PickDownload()
        {
            int input = -1;
            while (input <= 0 || input > Downloads.Count)
            {
                Console.Clear();
                PrintEntries();

                Console.WriteLine("\nChoose the download: ");
                input = int.Parse(Console.ReadLine());
            }

            // Get the selected download
            AbstractDownload download = Downloads.ElementAt(input - 1).Value;
            if (download == null)
                throw new Exception("Could not find the download.");
            Console.Clear();

            // Run the setup
            download.Setup();
        }
    }
}
