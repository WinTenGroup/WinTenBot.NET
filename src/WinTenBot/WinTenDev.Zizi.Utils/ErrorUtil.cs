using System;
using System.Linq;
using System.Threading.Tasks;
using WinTenDev.Zizi.Models.Types;
using WinTenDev.Zizi.Utils.IO;

namespace WinTenDev.Zizi.Utils
{
    /// <summary>
    /// Error writer and reader util
    /// </summary>
    public static class ErrorUtil
    {
        private const string ErrorFile = "last_ftl_error.txt";

        /// <summary>
        /// Write/save error exception into .txt file
        /// </summary>
        /// <param name="ex"></param>
        public static async Task SaveErrorToText(this Exception ex)
        {
            var content = $"{ex.Message}" +
                          $"\n\n{ex}";

            await content.Trim().WriteTextAsync(ErrorFile);
        }

        /// <summary>
        /// Write/save error string into .txt file
        /// </summary>
        /// <param name="ex"></param>
        public static async Task SaveErrorToText(this string ex)
        {
            var content = ex;

            await content.Trim().WriteTextAsync(ErrorFile);
        }

        /// <summary>
        /// Read last error from .txt async
        /// </summary>
        /// <returns></returns>
        public static async Task<string> ReadErrorTextAsync()
        {
            return await ErrorFile.ReadTextAsync();
        }

        public static async Task<LastError> ParseErrorTextAsync()
        {
            var errorText = await ReadErrorTextAsync();
            var errorSplit = errorText.Split("\n\n");
            var errorLines = errorText.Split(new[] {"\n", "\n\n", "\r", "\r\n", Environment.NewLine},
                    StringSplitOptions.RemoveEmptyEntries)
                .Select(line => line.Trim())
                .ToArray();

            return new LastError()
            {
                FullText = errorText,
                ErrorLines = errorLines,
                ErrorSplit = errorSplit
            };
        }

        /// <summary>
        /// Read last error from .txt
        /// </summary>
        /// <returns></returns>
        public static string ReadErrorText()
        {
            return ErrorFile.ReadText();
        }
    }
}