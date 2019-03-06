﻿using Newtonsoft.Json.Linq;
using ru.MaxKuzmin.VkMessenger.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ru.MaxKuzmin.VkMessenger.Clients
{
    public static class DialogsClient
    {
        private static List<Dialog> FromJsonArray(JArray dialogs, List<Profile> profiles, List<Group> groups)
        {
            var result = new List<Dialog>();

            foreach (var item in dialogs)
            {
                result.Add(FromJson(item as JObject, profiles, groups));
            }

            return result;
        }

        private static Dialog FromJson(JObject dialog, List<Profile> profiles, List<Group> groups)
        {
            var result = new Dialog
            {
                UnreadCount = dialog["conversation"]["unread_count"]?.Value<uint>() ?? 0u,
                LastMessage = MessagesClient.FromJson(dialog["last_message"] as JObject, profiles, groups)
            };

            var dialogId = dialog["conversation"]["peer"]["id"].Value<int>();
            var peerType = dialog["conversation"]["peer"]["type"].Value<string>();
            if (peerType == "user")
            {
                result.Type = DialogType.User;
                result.Profiles = new List<Profile> { profiles.First(p => p.Id == dialogId) };
            }
            else if (peerType == "group")
            {
                result.Type = DialogType.Group;
                result.Group = groups.First(g => g.Id == Math.Abs(dialogId));
            }
            else if (peerType == "chat")
            {
                var chatSettings = dialog["conversation"]["chat_settings"];
                result.Type = DialogType.Chat;
                result.Chat = new Chat
                {
                    Title = chatSettings["title"].Value<string>(),
                    Id = (uint)dialogId
                };

                result.Profiles = new List<Profile>();
                var ids = chatSettings["active_ids"] as JArray;
                foreach (var id in ids)
                {
                    result.Profiles.Add(profiles.First(p => p.Id == id.Value<uint>()));
                }

                if (chatSettings["photo"] != null)
                {
                    result.Chat.Photo = new UriImageSource { Uri = new Uri(chatSettings["photo"]["photo_50"].Value<string>()) };
                }
            }

            return result;
        }

        private static Profile GetFriend(JObject dialog, JArray profiles)
        {
            var dialogId = dialog["conversation"]["peer"]["id"].Value<int>();
            var profile = profiles.Where(o => o["id"].Value<uint>() == dialogId).FirstOrDefault();
            if (profile != null)
            {
                return ProfilesClient.FromJson(profile as JObject);
            }
            else return null;
        }

        public async static Task<List<Dialog>> GetDialogs()
        {
            var json = JObject.Parse(await GetDialogsJson());
            var profiles = ProfilesClient.FromJsonArray(json["response"]["profiles"] as JArray);
            var groups = GroupsClient.FromJsonArray(json["response"]["groups"] as JArray);
            return FromJsonArray(json["response"]["items"] as JArray, profiles, groups);
        }

        private async static Task<string> GetDialogsJson()
        {
            var url =
                "https://api.vk.com/method/messages.getConversations" +
                "?v=5.92" +
                "&extended=1" +
                "&access_token=" + Models.Authorization.Token;

            using (var client = new WebClient())
            {
                return await client.DownloadStringTaskAsync(url);
            }
        }

        public async static Task MarkAsRead(int dialogId)
        {
            var url =
                "https://api.vk.com/method/messages.markAsRead" +
                "?v=5.92" +
                "&peer_id=" + dialogId +
                "&access_token=" + Models.Authorization.Token;

            using (var client = new WebClient())
            {
                await client.DownloadStringTaskAsync(url);
            }
        }
    }
}
