using System.Collections.Generic;
using Newtonsoft.Json;

namespace Zizi.Bot.Models
{
    public class GoogleClients5Translator
    {
        [JsonProperty("sentences")]
        public List<Sentence> Sentences { get; set; }

        [JsonProperty("dict")]
        public List<Dict> Dict { get; set; }

        [JsonProperty("src")]
        public string Src { get; set; }

        [JsonProperty("alternative_translations")]
        public List<AlternativeTranslation> AlternativeTranslations { get; set; }

        [JsonProperty("confidence")]
        public double Confidence { get; set; }

        [JsonProperty("ld_result")]
        public LdResult LdResult { get; set; }

        [JsonProperty("query_inflections")]
        public List<QueryInflection> QueryInflections { get; set; }
    }

    public class AlternativeTranslation
    {
        [JsonProperty("src_phrase")]
        public string SrcPhrase { get; set; }

        [JsonProperty("alternative")]
        public List<Alternative> Alternative { get; set; }

        [JsonProperty("srcunicodeoffsets")]
        public List<Srcunicodeoffset> Srcunicodeoffsets { get; set; }

        [JsonProperty("raw_src_segment")]
        public string RawSrcSegment { get; set; }

        [JsonProperty("start_pos")]
        public long StartPos { get; set; }

        [JsonProperty("end_pos")]
        public long EndPos { get; set; }
    }

    public class Alternative
    {
        [JsonProperty("word_postproc")]
        public string WordPostproc { get; set; }

        [JsonProperty("score")]
        public long Score { get; set; }

        [JsonProperty("has_preceding_space")]
        public bool HasPrecedingSpace { get; set; }

        [JsonProperty("attach_to_next_token")]
        public bool AttachToNextToken { get; set; }
    }

    public class Srcunicodeoffset
    {
        [JsonProperty("begin")]
        public long Begin { get; set; }

        [JsonProperty("end")]
        public long End { get; set; }
    }

    public class Dict
    {
        [JsonProperty("pos")]
        public string Pos { get; set; }

        [JsonProperty("terms")]
        public List<string> Terms { get; set; }

        [JsonProperty("entry")]
        public List<Entry> Entry { get; set; }

        [JsonProperty("base_form")]
        public string BaseForm { get; set; }

        [JsonProperty("pos_enum")]
        public long PosEnum { get; set; }
    }

    public class Entry
    {
        [JsonProperty("word")]
        public string Word { get; set; }

        [JsonProperty("reverse_translation")]
        public List<string> ReverseTranslation { get; set; }

        [JsonProperty("score", NullValueHandling = NullValueHandling.Ignore)]
        public double? Score { get; set; }
    }

    public class LdResult
    {
        [JsonProperty("srclangs")]
        public List<string> Srclangs { get; set; }

        [JsonProperty("srclangs_confidences")]
        public List<double> SrclangsConfidences { get; set; }

        [JsonProperty("extended_srclangs")]
        public List<string> ExtendedSrclangs { get; set; }
    }

    public class QueryInflection
    {
        [JsonProperty("written_form")]
        public string WrittenForm { get; set; }

        [JsonProperty("features")]
        public Features Features { get; set; }
    }

    public class Features
    {
        [JsonProperty("gender", NullValueHandling = NullValueHandling.Ignore)]
        public long? Gender { get; set; }

        [JsonProperty("number")]
        public long Number { get; set; }
    }

    public class Sentence
    {
        [JsonProperty("trans")]
        public string Trans { get; set; }

        [JsonProperty("orig")]
        public string Orig { get; set; }

        [JsonProperty("backend")]
        public long Backend { get; set; }
    }
}