﻿using System.Collections.Generic;
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

            string testtoken = "FAKE_TOKEN";

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
                string imagePath = image.ImageInfo.Path;
                FileAttributes imageAtributes = System.IO.File.GetAttributes(imagePath);
                FileInfo imageInfo = new FileInfo(imagePath);
                HttpRequestOptions fileUploadOptions = new HttpRequestOptions();
                fileUploadOptions.Url = SLACK_UPLOAD_URL;
                fileUploadOptions.SetPostData(new Dictionary<String,String>() {{"token", testtoken},{"filename", imagePath},{"length", imageInfo.Length.ToString()}});
                HttpResponseInfo response = await _httpClient.Post(fileUploadOptions);
                if (response.StatusCode.Equals(System.Net.HttpStatusCode.OK)) {
                    var uploadResult = _jsonSerializer.DeserializeFromStream<FileResponse>(response.Content);

                    if (uploadResult.ok.Equals(true)) {
                        _logger.Debug("uploading notification image to slack");

                        var filePath = imagePath;

                        HttpClient multipartHttpClient = new HttpClient();
                        using (var multipartFormContent = new MultipartFormDataContent())
                        {
                            var fileStreamContent = new StreamContent(System.IO.File.OpenRead(filePath));
                            multipartFormContent.Add(fileStreamContent, name: "filename", fileName: filePath);
                            var uploadResponse = await multipartHttpClient.PostAsync(uploadResult.upload_url, multipartFormContent);
                            uploadResponse.EnsureSuccessStatusCode();
                            await uploadResponse.Content.ReadAsStringAsync();
                        }


                        HttpRequestOptions finalizeUploadOptions = new HttpRequestOptions();
                        finalizeUploadOptions.Url = SLACK_UPLOAD_FINALIZE_URL;
                        finalizeUploadOptions.RequestHeaders.Add("token", testtoken);
                        var finalizePayload = new [] {new {id = uploadResult.file_id}};
                        finalizeUploadOptions.SetPostData(new Dictionary<String,String>() {{"token", testtoken},{"channel_id", "C08517Z3Y9Y"},{"files", System.Net.WebUtility.UrlEncode(_jsonSerializer.SerializeToString(finalizePayload))}});
                        HttpResponseInfo finalizeResponse = await _httpClient.Post(finalizeUploadOptions);
                        var finalUploadResult = _jsonSerializer.DeserializeFromStream<FinalizedFileResponse>(finalizeResponse.Content);

                        if (finalUploadResult.ok.Equals(true) && finalUploadResult.files.First().permalink_public != null) {
                            imageUrl = finalUploadResult.files.First().permalink_public;
                            _logger.Debug("slack image url: "+imageUrl);
                        }
                    }
                }
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

