using System.Collections.Generic;

namespace Emby.Notification.Slack {
    public class File
    {
        public string id { get; set; }
        public int created { get; set; }
        public int timestamp { get; set; }
        public string name { get; set; }
        public string title { get; set; }
        public string mimetype { get; set; }
        public string filetype { get; set; }
        public string pretty_type { get; set; }
        public string user { get; set; }
        public string user_team { get; set; }
        public bool editable { get; set; }
        public int size { get; set; }
        public string mode { get; set; }
        public bool is_external { get; set; }
        public string external_type { get; set; }
        public bool is_public { get; set; }
        public bool public_url_shared { get; set; }
        public bool display_as_bot { get; set; }
        public string username { get; set; }
        public string url_private { get; set; }
        public string url_private_download { get; set; }
        public string media_display_type { get; set; }
        public string permalink { get; set; }
        public string permalink_public { get; set; }
        public int comments_count { get; set; }
        public bool is_starred { get; set; }
        public Shares shares { get; set; }
        public List<object> channels { get; set; }
        public List<object> groups { get; set; }
        public List<object> ims { get; set; }
        public bool has_more_shares { get; set; }
        public bool has_rich_preview { get; set; }
        public string file_access { get; set; }
    }

    public class FinalizedFileResponse
    {
        public bool ok { get; set; }
        public List<File> files { get; set; }
    }

    public class Shares
    {
    }

}