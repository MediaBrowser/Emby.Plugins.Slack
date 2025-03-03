using System.Collections.Generic;

namespace Emby.Notification.Slack {

    public class Canvas
        {
            public string file_id { get; set; }
            public string is_empty { get; set; }
            public string quip_thread_id { get; set; }
        }

        public class Channel
        {
            public string id { get; set; }
            public string name { get; set; }
            public string is_channel { get; set; }
            public string is_group { get; set; }
            public string is_im { get; set; }
            public string is_mpim { get; set; }
            public string is_private { get; set; }
            public int created { get; set; }
            public string is_archived { get; set; }
            public string is_general { get; set; }
            public int unlinked { get; set; }
            public string name_normalized { get; set; }
            public string is_shared { get; set; }
            public string is_org_shared { get; set; }
            public string is_pending_ext_shared { get; set; }
            public List<object> pending_shared { get; set; }
            public string context_team_id { get; set; }
            public object updated { get; set; }
            public object parent_conversation { get; set; }
            public string creator { get; set; }
            public string is_ext_shared { get; set; }
            public List<string> shared_team_ids { get; set; }
            public List<object> pending_connected_team_ids { get; set; }
            public string is_member { get; set; }
            public Topic topic { get; set; }
            public Purpose purpose { get; set; }
            public Properties properties { get; set; }
            public List<string> previous_names { get; set; }
            public int num_members { get; set; }
        }

        public class Properties
        {
            public Canvas canvas { get; set; }
            public string use_case { get; set; }
            public List<Tab> tabs { get; set; }
            public List<Tabz> tabz { get; set; }
        }

        public class Purpose
        {
            public string value { get; set; }
            public string creator { get; set; }
            public int last_set { get; set; }
        }

        public class ResponseMetadata
        {
            public string next_cursor { get; set; }
        }

        public class ChannelsListResponse
        {
            public string ok { get; set; }
            public List<Channel> channels { get; set; }
            public ResponseMetadata response_metadata { get; set; }
            public string error { get; set; }
        }

        public class Tab
        {
            public string type { get; set; }
            public string label { get; set; }
            public string id { get; set; }
        }

        public class Tabz
        {
            public string type { get; set; }
        }

        public class Topic
        {
            public string value { get; set; }
            public string creator { get; set; }
            public int last_set { get; set; }
        }
}
