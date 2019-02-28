﻿using Xamarin.Forms;

namespace ru.MaxKuzmin.VkMessenger.Models
{
    public class Profile
    {
        public uint Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public UriImageSource Photo { get; set; }
        public bool IsOnline { get; set; }
    }
}
