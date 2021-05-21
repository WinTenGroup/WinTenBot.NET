using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace WinTenDev.Zizi.Models.Types
{
  public class DeepAiResult
      {
          [JsonProperty("id")]
          public Guid Id { get; set; }

          [JsonProperty("output")]
          public Output Output { get; set; }
      }

      public class Output
      {
          [JsonProperty("detections")]
          public List<Detection> Detections { get; set; }

          [JsonProperty("nsfw_score")]
          public double NsfwScore { get; set; }
      }

      public class Detection
      {
          [JsonProperty("confidence")]
          public string Confidence { get; set; }

          [JsonProperty("bounding_box")]
          public List<long> BoundingBox { get; set; }

          [JsonProperty("name")]
          public string Name { get; set; }
      }
}