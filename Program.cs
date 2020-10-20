using Iswenzz.UGameDL.Games;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Iswenzz.UGameDL
{
    /// <summary>
    /// Utility Game Downloader
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// List of games
        /// </summary>
        public static List<(int index, string name, Game game)> Games = new List<(int index, string name, Game game)>
        {
            (1, "Q3", new Q3())
        };

        /// <summary>
        /// The entry point of the program.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            Console.Title = "UGameDL (c) Iswenzz";

            // Print all games in the console
            Games.ForEach(i => Console.WriteLine($"{i.index}. {i.name}"));

            // Get the selected game
            Console.WriteLine("\nChoose the game to download: ");
            string input = Console.ReadLine();
            (int index, string name, Game game) = Games.FirstOrDefault(i => i.index.ToString() == input);
            if (game == null)
                Environment.Exit(-1);

            // Run the setup
            Console.Clear();
            game.Setup();

            Console.WriteLine("\nDone!");
            Console.ReadLine();
        }
    }
}
