using System;

namespace Emby.Notification.Slack
{
    public class FileResponse
    {
        public Boolean ok { get; set; }
        public string upload_url { get; set; }
        public string file_id { get; set; }
        public string error { get; set; }
    }
}

