using System;
using System.Threading.Tasks;
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