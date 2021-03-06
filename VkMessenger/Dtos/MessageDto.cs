﻿#pragma warning disable IDE1006 // Naming Styles
namespace ru.MaxKuzmin.VkMessenger.Dtos
{
    public sealed class MessageDto
    {
        public int date { get; set; }

        public int? update_time { get; set; }
        
        /// <summary>
        /// Can be negative number
        /// </summary>
        public int from_id { get; set; }

        public int id { get; set; }

        public int conversation_message_id { get; set; }

        public string text { get; set; } = default!;

        public MessageDto[]? fwd_messages { get; set; }

        public AttachmentDto[]? attachments { get; set; }

        public int deleted { get; set; }

        public MessageActionDto? action { get; set; }
    }
}
