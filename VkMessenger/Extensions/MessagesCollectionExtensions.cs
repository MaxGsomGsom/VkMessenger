﻿using ru.MaxKuzmin.VkMessenger.Clients;
using ru.MaxKuzmin.VkMessenger.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
#if DEBUG
using ru.MaxKuzmin.VkMessenger.Loggers;
#endif

namespace ru.MaxKuzmin.VkMessenger.Extensions
{
    public static class MessagesCollectionExtensions
    {
        /// <summary>
        /// Update messages from API. Can be used during setup of page or with <see cref="LongPolling"/>
        /// </summary>
        public static async Task Update(
            this ObservableCollection<Message> collection,
            int dialogId,
            int unreadCount,
            int? offset = null)
        {
            var newMessages = await MessagesClient.GetMessages(dialogId, offset);
            if (newMessages.Any())
            {
                collection.AddUpdate(newMessages, unreadCount);
                _ = DurableCacheManager.SaveMessages(dialogId, collection);
            }
        }

        /// <summary>
        /// Update messages from API. Can be used during setup of page or with <see cref="LongPolling"/>
        /// </summary>
        public static async Task UpdateByIds(
            this ObservableCollection<Message> collection,
            IReadOnlyCollection<int> messagesIds,
            int dialogId,
            int unreadCount)
        {
            var newMessages = await MessagesClient.GetMessagesByIds(messagesIds);
            if (newMessages.Any())
            {
                collection.AddUpdate(newMessages, unreadCount);
                _ = DurableCacheManager.SaveMessages(dialogId, collection);
            }
        }

        public static void AddUpdate(
            this ObservableCollection<Message> collection,
            IReadOnlyCollection<Message> newMessages,
            int unreadCount)
        {
#if DEBUG
            Logger.Debug("Try to lock Messages " + collection.GetHashCode());
#endif
            lock (collection)
            {
#if DEBUG
                Logger.Debug("Locked Messages " + collection.GetHashCode());
#endif
                var newestExistingId = collection.First().ConversationMessageId;
                var oldestExistingId = collection.Last().ConversationMessageId;

                var oldMessagesToAppend = new List<Message>();
                var newMessagesToPrepend = new List<Message>();

                foreach (var newMessage in newMessages)
                {
                    var foundMessage = collection.FirstOrDefault(m => m.Id == newMessage.Id);
                    if (foundMessage != null)
                        UpdateMessage(newMessage, foundMessage);
                    else if (newestExistingId < newMessage.ConversationMessageId)
                        newMessagesToPrepend.Add(newMessage);
                    else if (oldestExistingId > newMessage.ConversationMessageId)
                        oldMessagesToAppend.Add(newMessage);
                    else
                        for (int i = 0; i < collection.Count; i++)
                        {
                            if (collection[i].ConversationMessageId < newMessage.ConversationMessageId)
                            {
                                collection.Insert(i, newMessage);
                                break;
                            }
                        }
                }

                collection.AddRange(oldMessagesToAppend);

                collection.PrependRange(newMessagesToPrepend);
            }
#if DEBUG
            Logger.Debug("Unlocked Messages " + collection.GetHashCode());
#endif
            collection.UpdateRead(unreadCount);
        }

        /// <summary>
        /// Update message data without recreating it
        /// </summary>
        private static void UpdateMessage(Message newMessage, Message foundMessage)
        {
            foundMessage.SetText(newMessage.Text);
        }

        public static void UpdateRead(this ObservableCollection<Message> collection, int unreadCount)
        {
#if DEBUG
            Logger.Debug("Try to lock Messages " + collection.GetHashCode());
#endif
            lock (collection) //To prevent enumeration exception
            {
#if DEBUG
                Logger.Debug("Locked Messages " + collection.GetHashCode());
#endif
                var leastUnread = unreadCount;
                foreach (var message in collection)
                {
                    // If it's current user message or there are must be more unread messages in dialog
                    if (message.SenderId == Authorization.UserId || leastUnread == 0)
                        message.SetRead(true);
                    // If message hasn't Read property set to true
                    else if (message.Read != true)
                    {
                        leastUnread--;
                        message.SetRead(false);
                    }
                }
            }
#if DEBUG
            Logger.Debug("Unlocked Messages " + collection.GetHashCode());
#endif
        }
    }
}