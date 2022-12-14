using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Notifications;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Serialization;
using Emby.Notifications;
using MediaBrowser.Controller;

namespace Emby.Notification.Slack
{
    public class Notifier : IUserNotifier
    {
        private ILogger _logger;
        private IServerApplicationHost _appHost;
        private IHttpClient _httpClient;
        private IJsonSerializer _jsonSerializer;

        public Notifier(ILogger logger, IServerApplicationHost applicationHost, IHttpClient httpClient, IJsonSerializer jsonSerializer)
        {
            _logger = logger;
            _appHost = applicationHost;
            _httpClient = httpClient;
            _jsonSerializer = jsonSerializer;
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


            var parameters = new Dictionary<string, string> { };
            parameters.Add("payload", System.Net.WebUtility.UrlEncode(_jsonSerializer.SerializeToString(slackMessage)));

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

