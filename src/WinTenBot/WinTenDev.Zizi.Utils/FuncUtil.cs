using System.Threading.Tasks;
using Nito.AsyncEx;

namespace WinTenDev.Zizi.Utils
{
    /// <summary>
    /// Extension method about Func
    /// </summary>
    public static class FuncUtil
    {
        /// <summary>
        /// Run a function and ignore
        /// </summary>
        /// <param name="task"></param>
        public static void FireAndForget(this Task task)
        {
            task.Ignore();
        }
    }
}