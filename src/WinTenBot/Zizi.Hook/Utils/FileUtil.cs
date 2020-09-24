using System.IO;

namespace Zizi.Hook.Utils
{
    public static class FileUtil
    {
        public static void WriteText(this string content, string fileName)
        {
            File.WriteAllText(fileName, content);
        }
    }
}