using System.Collections.Generic;
using System.Threading.Tasks;
using Zizi.Bot.Model;

namespace Zizi.Bot.Interfaces
{
    public interface ITagsService
    {
        Task<List<CloudTag>> GetTagsAsync();
    }
}