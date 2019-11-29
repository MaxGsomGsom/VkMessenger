﻿using Xamarin.Forms;

namespace ru.MaxKuzmin.VkMessenger.Models
{
    public class Chat
    {
        public uint Id { get; set; }
        public ImageSource Photo { get; set; }
        public string Title { get; set; }
    }
}