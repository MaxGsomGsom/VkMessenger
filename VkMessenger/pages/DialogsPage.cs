﻿using ru.MaxKuzmin.VkMessenger.Cells;
using ru.MaxKuzmin.VkMessenger.Clients;
using ru.MaxKuzmin.VkMessenger.Events;
using ru.MaxKuzmin.VkMessenger.Models;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using Tizen.Wearable.CircularUI.Forms;
using Xamarin.Forms;

namespace ru.MaxKuzmin.VkMessenger.Pages
{
    public class DialogsPage : CirclePage
    {
        private readonly CircleListView dialogsListView = new CircleListView
        {
            ItemTemplate = new DataTemplate(typeof(DialogCell))
        };
        private readonly ObservableCollection<Dialog> dialogs = new ObservableCollection<Dialog>();

        public DialogsPage()
        {
            NavigationPage.SetHasNavigationBar(this, false);
            Setup();
            Update();
        }

        private async void Update()
        {
            try
            {
                Network.ThrowIfDisconnected();
                var newDialogs = await DialogsClient.GetDialogs();

                foreach (var newDialog in newDialogs.AsEnumerable().Reverse())
                {
                    var foundDialog = dialogs.FirstOrDefault(d => d.Id == newDialog.Id);

                    if (foundDialog == null)
                        dialogs.Insert(0, newDialog);
                    else
                    {
                        UpdateDialog(newDialog, foundDialog);

                        if (dialogs.Last() != foundDialog)
                        {
                            dialogs.Remove(foundDialog);
                            dialogs.Insert(0, foundDialog);
                        }
                        else foundDialog.InvokePropertyChanged();
                    }
                }
            }
            catch (WebException)
            {
                Network.StartWaiting();
            }
        }

        private static void UpdateDialog(Dialog newDialog, Dialog foundDialog)
        {
            foundDialog.LastMessage = newDialog.LastMessage;
            foundDialog.UnreadCount = newDialog.UnreadCount;

            if (newDialog.Profiles != null)
            {
                foreach (var newProfile in newDialog.Profiles)
                {
                    var foundProfile = foundDialog.Profiles.FirstOrDefault(p => p.Id == newProfile.Id);
                    if (foundProfile != null)
                        foundProfile.IsOnline = newDialog.IsOnline;
                }
            }
        }

        private void Setup()
        {
            SetBinding(RotaryFocusObjectProperty, new Binding() { Source = dialogsListView });
            dialogsListView.ItemSelected += OnDialogSelected;
            dialogsListView.ItemsSource = dialogs;
            dialogsListView.RefreshCommand = new Command(Update);
            Content = dialogsListView;

            //TODO: Replace with getting only one dialog
            LongPollingClient.OnMessageUpdate += (s, e) => Update();
            LongPollingClient.OnDialogUpdate += (s, e) => Update();

            LongPollingClient.OnUserStatusUpdate += OnUserStatusUpdate;
        }

        private void OnUserStatusUpdate(object sender, UserStatusEventArgs e)
        {
            foreach (var dialog in dialogs)
            {
                foreach (var profile in dialog.Profiles.Where(p => p.Id == e.UserId))
                    profile.IsOnline = e.IsOnline;
                dialog.InvokePropertyChanged();
            }
        }

        private async void OnDialogSelected(object sender, SelectedItemChangedEventArgs e)
        {
            try
            {
                Network.ThrowIfDisconnected();
                var dialog = e.SelectedItem as Dialog;
                await Navigation.PushAsync(new MessagesPage(dialog.Id));
                await DialogsClient.MarkAsRead(dialog.Id);
            }
            catch (WebException)
            {
                Network.StartWaiting();
            }
        }
    }
}
