using System;
using WindowsShortcutFactory;

namespace Iswenzz.DL
{
    public static class Utils
    {
        /// <summary>
        /// Create a shortcut to a file.
        /// </summary>
        /// <param name="targetFile">The target file.</param>
        /// <param name="shortcutFile">Where to save the shortcut.</param>
        /// <param name="description">The shortcut description.</param>
        /// <param name="arguments">The shortcut args.</param>
        /// <param name="workingDirectory">The shortcut working directory.</param>
        /// <param name="iconLocation">The shortcut icon location.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void CreateShortcutToFile(string targetFile, string shortcutFile, string description = null,
            string arguments = null, string workingDirectory = null, string iconLocation = null)
        {
            if (string.IsNullOrEmpty(targetFile) || string.IsNullOrEmpty(shortcutFile))
                throw new ArgumentNullException();

            using WindowsShortcut shortcut = new()
            {
                Path = targetFile,
                Description = description,
                WorkingDirectory = workingDirectory,
                IconLocation = iconLocation,
                Arguments = arguments,
            };
            shortcut.Save(shortcutFile);
        }
    }
}
