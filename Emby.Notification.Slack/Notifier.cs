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
using System;
using System.IO;
using System.Net.Http;
using Emby.Media.Common.Extensions;
using System.Net.Http.Headers;
using System.Data.Common;

namespace Emby.Notification.Slack
{
    public class Notifier : IUserNotifier
    {
        private const string SLACK_UPLOAD_URL = "https://slack.com/api/files.getUploadURLExternal";
        private const string SLACK_UPLOAD_FINALIZE_URL = "https://slack.com/api/files.completeUploadExternal";
        private const string SLACK_CHANNELS_URL = "https://slack.com/api/conversations.list";
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
            options.TryGetValue("EnableImages", out string EnableImages);

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

            string imageUrl = null;

            if (string.Equals(EnableImages, "true", StringComparison.OrdinalIgnoreCase))
            {
                // get a series or album image if available, otherwise, the image from the media
                var image = request.GetSeriesImageInfo(MediaBrowser.Model.Entities.ImageType.Primary)
                    ?? request.GetSeriesImageInfo(MediaBrowser.Model.Entities.ImageType.Thumb)
                    ?? request.GetImageInfo(MediaBrowser.Model.Entities.ImageType.Primary);

                if (image != null)
                {
                    imageUrl = await image.GetRemoteApiImageUrl(new ApiImageOptions
                    {
                        Format = "jpg",
                        MaxWidth = 1280

                    }, cancellationToken).ConfigureAwait(false);
                }
            }

            var finalMessage = new object { };
            //  It doesn't seem to be possible to embed non-public images via webhook. The following commented out code block should otherwise work to
            //     combine the item image and the message
            //    https://forums.slackcommunity.com/s/question/0D5Hq00009pHglcKAC/how-to-include-private-slack-file-in-incoming-webhook

            if (!string.IsNullOrEmpty(imageUrl))
            {
                _logger.Debug("sending notification with embedded image to slack");
                finalMessage = new
                {
                    channel = slackMessage.channel,
                    blocks = new[] {
                                        new {
                                            type = "context",
                                            elements = new object[]
                                            {
                                                new
                                                {
                                                    type = "image",
                                                    image_url = imageUrl,
                                                    alt_text = request.Title.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;")
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
            }
            else
            {
                _logger.Debug("sending notification with no embedded image to slack");
                finalMessage = new
                {
                    channel = slackMessage.channel,
                    blocks = new[] {
                            new {
                                type = "context",
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

