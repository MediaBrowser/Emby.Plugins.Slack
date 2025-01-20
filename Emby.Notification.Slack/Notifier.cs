using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Serialization;
using Emby.Notifications;
using MediaBrowser.Controller;
using MediaBrowser.Model.IO;

namespace Emby.Notification.Slack
{
    public class Notifier : IUserNotifier
    {
        private ILogger _logger;
        private IServerApplicationHost _appHost;
        private IHttpClient _httpClient;
        private IJsonSerializer _jsonSerializer;
        private IFileSystem _fileSystem;

        public Notifier(ILogger logger, IServerApplicationHost applicationHost, IHttpClient httpClient, IJsonSerializer jsonSerializer, IFileSystem fileSystem)
        {
            _logger = logger;
            _appHost = applicationHost;
            _httpClient = httpClient;
            _jsonSerializer = jsonSerializer;
            _fileSystem = fileSystem;
        }

        private Plugin Plugin => _appHost.Plugins.OfType<Plugin>().First();

        public string Name => Plugin.StaticName;

        public string Key => "slacknotifications";

        public string SetupModuleUrl => Plugin.NotificationSetupModuleUrl;

        public async Task SendNotification(InternalNotificationRequest request, CancellationToken cancellationToken)
        {
            var options = request.Configuration.Options;

            options.TryGetValue("Channel", out string channel);
            options.TryGetValue("Emoji", out string emoji);
            options.TryGetValue("UserName", out string userName);
            options.TryGetValue("SlackWebHookURI", out string slackWebHookURI);

            var slackMessage = new SlackMessage { channel = channel, icon_emoji = emoji, username = userName };

            if (string.IsNullOrEmpty(request.Description))
            {
                slackMessage.text = request.Title;
            }
            else
            {
                slackMessage.text = request.Title + "\r\n" + request.Description;
            }

            slackMessage.text = slackMessage.text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");

            // get a series or album image if available, otherwise, the image from the media
            var image = request.GetSeriesImageInfo(MediaBrowser.Model.Entities.ImageType.Primary)
                ?? request.GetSeriesImageInfo(MediaBrowser.Model.Entities.ImageType.Thumb)
                ?? request.GetImageInfo(MediaBrowser.Model.Entities.ImageType.Primary);

            string imageUrl = null;

            if (image != null)
            {
                imageUrl = image.GetRemoteApiImageUrl(new ApiImageOptions
                {
                    Format = "jpg"

                });
            }

            var finalMessage = new object {};

            if (!string.IsNullOrEmpty(imageUrl)) {
                finalMessage = new {
                        channel = slackMessage.channel,
                        blocks = new [] {
                            new {
                                type = "context",
                                elements = new object[]
                                {
                                    new
                                    {
                                        type = "image",
                                        image_url = imageUrl,
                                        alt_text = "test"
                                    },
                                    new
                                    {
                                        type = "plain_text",
                                        text = $"{slackMessage.icon_emoji} {slackMessage.text}",
                                        emoji = true
                                    }
                                }
                            }
                        }
                    };
            } else {
                finalMessage = new {
                        channel = slackMessage.channel,
                        blocks = new [] {
                            new {
                                type = "context",
                                emoji = slackMessage.icon_emoji,
                                elements = new object[]
                                {
                                    new
                                    {
                                        type = "plain_text",
                                        text = $"{slackMessage.icon_emoji} {slackMessage.text}",
                                        emoji = true
                                    }
                                }
                            }
                        }
                    };
            }


            _logger.Debug(_jsonSerializer.SerializeToString(finalMessage));

            var parameters = new Dictionary<string, string> { };
            parameters.Add("payload", System.Net.WebUtility.UrlEncode(_jsonSerializer.SerializeToString(finalMessage)));

            var _httpRequest = new HttpRequestOptions
            {
                Url = slackWebHookURI,
                CancellationToken = cancellationToken
            };

            _httpRequest.SetPostData(parameters);
            using (await _httpClient.Post(_httpRequest).ConfigureAwait(false))
            {

            }
        }
    }
}

