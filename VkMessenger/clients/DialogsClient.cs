﻿using Newtonsoft.Json.Linq;
using ru.MaxKuzmin.VkMessenger.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Tizen.Applications;

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
                Id = dialog["conversation"]["peer"]["id"].Value<int>(),
                UnreadCount = dialog["conversation"]["unread_count"]?.Value<int>() ?? 0,
                LastMessage = MessagesClient.FromJson(dialog["last_message"] as JObject)
            };

            var peerType = dialog["conversation"]["peer"]["type"].Value<string>();
            if (peerType == "user")
            {
                result.Type = DialogType.User;
                result.Profiles = new List<Profile> { profiles.First(p => p.Id == result.Id) };
            }
            else if (peerType == "group")
            {
                result.Type = DialogType.Group;
                result.Group = groups.First(g => g.Id == Math.Abs(result.Id));
            }
            else if (peerType == "chat")
            {
                result.Type = DialogType.Chat;
                result.Chat = new Chat
                {
                    Name = dialog["conversation"]["chat_settings"]["title"].Value<string>()
                };

                result.Profiles = new List<Profile>();
                var ids = dialog["conversation"]["chat_settings"]["active_ids"] as JArray;
                foreach (var id in ids)
                {
                    result.Profiles.Add(profiles.First(p => p.Id == id.Value<int>()));
                }

                if (dialog["conversation"]["chat_settings"]["photo"] != null)
                {
                    result.Chat.Photo = dialog["conversation"]["chat_settings"]["photo"]["photo_50"].Value<string>();
                }
            }

            return result;
        }

        private static Profile GetFriend(JObject dialog, JArray profiles)
        {
            var dialogId = dialog["conversation"]["peer"]["id"].Value<int>();
            var profile = profiles.Where(o => o["id"].Value<int>() == dialogId).FirstOrDefault();
            if (profile != null)
            {
                return ProfilesClient.FromJson(profile as JObject);
            }
            else
            {
                return null;
            }
        }

        public static List<Dialog> GetDialogs()
        {
            var json = JObject.Parse(GetDialogsJson());
            var profiles = ProfilesClient.FromJsonArray(json["response"]["profiles"] as JArray);
            var groups = GroupsClient.FromJsonArray(json["response"]["groups"] as JArray);
            return FromJsonArray(json["response"]["items"] as JArray, profiles, groups);
        }

        private static string GetDialogsJson()
        {
            var url =
                "https://api.vk.com/method/messages.getConversations" +
                "?v=5.92" +
                "&extended=1" +
                "&access_token=" + Preference.Get<string>(Setting.TokenKey);

            using (var client = new WebClient())
            {
                return client.DownloadString(url);
            }
        }

        public static void MarkAsRead(int peerId)
        {
            var url =
                "https://api.vk.com/method/messages.markAsRead" +
                "?v=5.92" +
                "&peer_id=" + peerId +
                "&access_token=" + Preference.Get<string>(Setting.TokenKey);

            using (var client = new WebClient())
            {
                client.DownloadString(url);
            }
        }
    }
}
