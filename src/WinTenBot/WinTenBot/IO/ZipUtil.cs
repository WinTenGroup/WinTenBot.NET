using Ionic.Zip;
using Ionic.Zlib;
using Serilog;

namespace WinTenBot.IO
{
    public static class ZipUtil
    {
        public static string CreateZip(this string fileName, string saveTo)
        {
            Log.Information("Creating .zip from file {0}", fileName);
            using var zip = new ZipFile
            {
                CompressionLevel = CompressionLevel.BestCompression
            };

            zip.AddFile(fileName,"");

            Log.Debug("Saving to {0}", saveTo);
            zip.Save(saveTo);

            return saveTo;
        }

        public static string CreateZip(this string filePath)
        {
            var newFilePath = filePath.ReplaceExt(".zip");
            var zipFile = filePath.CreateZip(newFilePath);
            return zipFile;
        }
    }
}