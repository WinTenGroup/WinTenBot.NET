using System.Collections.Generic;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;
using YouTubeSearch;

namespace Zizi.Core.Utils
{
    public class YoutubeUtil
    {
        public static async Task<List<VideoSearchComponents>> VideoSearch(string queryString, int queryPages = 1)
        {
            var videos = new VideoSearch();
            var items = await videos.GetVideos(queryString, queryPages);

            return items;
        }

        public static async Task<StreamManifest> GetVideoManifest(string videoId)
        {
            var youtube = new YoutubeClient();
            var streamManifest = await youtube.Videos.Streams.GetManifestAsync(videoId);

            return streamManifest;
        }

        public static void DownloadVideo(IStreamInfo streamInfo, string fileName)
        {
            var youtube = new YoutubeClient();
            var stream = youtube.Videos.Streams.GetAsync(streamInfo);

            youtube.Videos.Streams.DownloadAsync(streamInfo, $"video.{streamInfo.Container}");
        }
    }
}