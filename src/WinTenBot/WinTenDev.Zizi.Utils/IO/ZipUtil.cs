using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ionic.Zip;
using Ionic.Zlib;
using Serilog;

namespace WinTenDev.Zizi.Utils.IO
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

            if (fileName.IsDirectory())
            {
                var files = Directory.GetFiles(fileName, "*.*", SearchOption.AllDirectories)
                    .Where(x => !x.Contains(".stikerpacks"));
                // zip.AddFiles(files, String.Empty);
                foreach (var file in files)
                {
                    zip.AddFile(file, Path.GetDirectoryName(file));
                }

                // zip.AddDirectory(fileName);
            }
            else
            {
                zip.AddFile(fileName, "");
            }

            Log.Debug("Saving to {0}", saveTo);
            zip.Save(saveTo);

            return saveTo;
        }

        public static string CreateZip(this IEnumerable<string> listPath, string saveTo)
        {
            using var zip = new ZipFile
            {
                CompressionLevel = CompressionLevel.BestCompression
            };
            
            foreach (var file in listPath)
            {
                zip.AddFile(file, Path.GetDirectoryName(file));
            }
            
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