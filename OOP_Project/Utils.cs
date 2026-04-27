using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ErrorOr;

namespace OOP_Project
{
    internal static class Utils
    {
        public static DateTime AddDuration(DateTime start, int duration)
        {
            return start.AddMinutes(duration * 60);
        }

        public static ErrorOr<string[]> GetFilesInDir(string dir, string extension)
        {
            if (string.IsNullOrWhiteSpace(dir) || !Directory.Exists(dir))
            {
                return Error.Validation(description: $"Directory '{dir}' does not exist or is invalid.");
            }

            if (string.IsNullOrWhiteSpace(extension))
            {
                return Error.Validation(description: "Extension must be provided (e.g. \".txt\" or \"txt\").");
            }

            // Normalize extension to include leading dot and be lower-case
            var ext = extension.Trim();
            if (!ext.StartsWith('.')) ext = "." + ext;
            ext = ext.ToLowerInvariant();

            try
            {
                var files = Directory.EnumerateFiles(dir, "*", SearchOption.AllDirectories)
                                     .Where(f => string.Equals(Path.GetExtension(f).ToLowerInvariant(), ext, StringComparison.Ordinal))
                                     .ToArray();

                Debug.Log(files.Length.ToString());

                if (files.Length == 0)
                {
                    Debug.Log("called");
                    return Error.NotFound(description: $"No files with extension '{ext}' found in directory '{dir}'.");
                }
                Debug.Log("2");
                return files;
            }
            catch (UnauthorizedAccessException)
            {
                return Error.Forbidden("Access to the directory is denied.");
            }
            catch (System.Security.SecurityException)
            {
                return Error.Unexpected("Cannot access the directory given");
            }
        }
    }
}
