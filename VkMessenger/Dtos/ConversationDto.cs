﻿#pragma warning disable IDE1006 // Naming Styles
namespace ru.MaxKuzmin.VkMessenger.Dtos
{
    public sealed class ConversationDto
    {
        public PeerDto peer { get; set; } = default!;

        public int? unread_count { get; set; }

        public int? last_message_id { get; set; }

        public CanWriteDto? can_write { get; set; }

        public ChatSettingsDto? chat_settings { get; set; }
    }
}
