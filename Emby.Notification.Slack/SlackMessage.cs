using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Emby.Notification.Slack
{
    /// <summary>
    /// Slack Message
    /// </summary>
    public class SlackMessage
    {
        /// <summary>
        /// This is the text that will be posted to the channel
        /// </summary>
        public string text { get; set; }
        /// <summary>
        /// Optional override of destination channel
        /// </summary>
        public string channel { get; set; }
        /// <summary>
        /// Optional override of the username that is displayed
        /// </summary>
        public string username { get; set; }
        /// <summary>
        /// Optional emoji displayed with the message
        /// </summary>
        public string icon_emoji { get; set; }
        /// <summary>
        /// Optional url for icon displayed with the message
        /// </summary>
    }

    public class SlackBlocksMessage
    {
        public string channel { get; set; }

        public SlackBlock[] blocks { get; set; } = Array.Empty<SlackBlock>();
    }

    public class SlackBlock
    {
        public string type { get; set; }

        public SlackElement[] elements { get; set; } = Array.Empty<SlackElement>();
    }

    public class SlackElement
    {
        public string type { get; set; }
        public string text { get; set; }
        public string alt_text { get; set; }
        public string image_url { get; set; }
        public bool emoji { get; set; }
    }
}