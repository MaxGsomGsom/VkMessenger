﻿#pragma warning disable IDE1006 // Naming Styles
namespace ru.MaxKuzmin.VkMessenger.Dtos
{
    public sealed class ChatSettingsDto
    {
        public string title { get; set; } = default!;

        public PhotoDto? photo { get; set; }

        /// <summary>
        /// Can be negative number
        /// </summary>
        public int[]? active_ids { get; set; }
    }
}
