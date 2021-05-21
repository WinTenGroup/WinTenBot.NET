using System;
using System.Collections.Generic;

namespace WinTenDev.Zizi.Models.Types
{
    public class BlockList
    {
        public int Id { get; set; }
        public string UrlSource { get; set; }
        public long FromId { get; set; }
        public long ChatId { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class BlockListData
    {
        public string Name { get; set; }
        public string Source { get; set; }
        public string LastUpdate { get; set; }
        public Dictionary<string, object> MetadataDic { get; set; }
        public BlockListMetaData MetaData { get; set; }
        public IEnumerable<string> ListDomain { get; set; }
        public long DomainCount { get; set; }
    }

    public class BlockListMetaData
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public string LastUpdate { get; set; }
    }
}