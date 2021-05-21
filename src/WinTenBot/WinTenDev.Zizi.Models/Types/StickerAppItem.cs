using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace WinTenDev.Zizi.Models.Types
{
    public class StickerAppItem
    {
        [JsonProperty("android_play_store_link")]
        public Uri AndroidPlayStoreLink { get; set; }

        [JsonProperty("ios_app_store_link")]
        public string IosAppStoreLink { get; set; }

        [JsonProperty("sticker_packs")]
        public List<StickerPack> StickerPacks { get; set; }
    }

    public class StickerPack
    {
        [JsonProperty("identifier")]
        public string Identifier { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("publisher")]
        public string Publisher { get; set; }

        [JsonProperty("tray_image_file")]
        public string TrayImageFile { get; set; }

        [JsonProperty("image_data_version")]
        // [JsonConverter(typeof(ParseStringConverter))]
        public long ImageDataVersion { get; set; }

        [JsonProperty("avoid_cache")]
        public bool AvoidCache { get; set; }

        [JsonProperty("publisher_email")]
        public string PublisherEmail { get; set; }

        [JsonProperty("publisher_website")]
        public Uri PublisherWebsite { get; set; }

        [JsonProperty("privacy_policy_website")]
        public string PrivacyPolicyWebsite { get; set; }

        [JsonProperty("license_agreement_website")]
        public string LicenseAgreementWebsite { get; set; }

        [JsonProperty("stickers")]
        public List<StickerItem> Stickers { get; set; }
    }

    public class StickerItem
    {
        [JsonProperty("image_file")]
        public string ImageFile { get; set; }

        [JsonProperty("emojis")]
        public List<string> Emojis { get; set; }
    }
}