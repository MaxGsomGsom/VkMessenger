﻿using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ru.MaxKuzmin.VkMessenger.Helpers;
using ru.MaxKuzmin.VkMessenger.Localization;
using ru.MaxKuzmin.VkMessenger.Loggers;
using ru.MaxKuzmin.VkMessenger.Managers;
using Tizen.Multimedia;
using Tizen.System;
using Tizen.Wearable.CircularUI.Forms;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration.TizenSpecific;
using Label = Xamarin.Forms.Label;
using NavigationPage = Xamarin.Forms.NavigationPage;
using Rectangle = Xamarin.Forms.Rectangle;
using TizenConfig = Xamarin.Forms.PlatformConfiguration.Tizen;

namespace ru.MaxKuzmin.VkMessenger.Pages
{
    public class RecordVoicePage : PageWithActivityIndicator, IDisposable
    {
        private readonly int dialogId;
        private readonly MessagesManager messagesManager;
        private string? voiceMessageTempPath;
        private bool isRecording;

        private readonly AudioRecorder audioRecorder = new AudioRecorder(RecorderAudioCodec.Aac, RecorderFileFormat.ThreeGp)
        {
            AudioBitRate = 16000,
            TimeLimit = 300,
            AudioDevice = RecorderAudioDevice.Mic,
            AudioSampleRate = 16000,
            AudioChannels = 1
        };

        private readonly Button recordButton = new Button
        {
            ImageSource = ImageResources.RecordSymbol
        };
        private readonly Button sendButton = new Button
        {
            Text = LocalizedStrings.Send,
            IsEnabled = false
        };

        private readonly Label hint = new Label
        {
            Text = LocalizedStrings.RecordMessageHint,
            HorizontalTextAlignment = TextAlignment.Center,
            FontSize = Device.GetNamedSize(NamedSize.Micro, typeof(Label)),
            TextColor = Color.Gray,
        };

        public RecordVoicePage(int dialogId, MessagesManager messagesManager)
        {
            this.dialogId = dialogId;
            this.messagesManager = messagesManager;

            NavigationPage.SetHasNavigationBar(this, false);
            recordButton.On<TizenConfig>().SetStyle(ButtonStyle.Circle);
            absoluteLayout.Children.Add(hint, new Rectangle(0.5, 0.2, 200, 200), AbsoluteLayoutFlags.PositionProportional);
            absoluteLayout.Children.Add(recordButton, new Rectangle(0.5, 0.5, 75, 75), AbsoluteLayoutFlags.PositionProportional);
            absoluteLayout.Children.Add(sendButton, new Rectangle(0.5, 0.9, 200, 60), AbsoluteLayoutFlags.PositionProportional);
            absoluteLayout.Children.Add(activityIndicator);
            Content = absoluteLayout;

            audioRecorder.RecordingLimitReached += OnRecordingLimitReached;
            recordButton.Clicked += OnRecordButtonPressed;
            sendButton.Clicked += OnSendButtonPressed;

            PrivilegeChecker.PrivilegeCheck("http://tizen.org/privilege/recorder");
            PrivilegeChecker.PrivilegeCheck("http://tizen.org/privilege/mediastorage");
        }

        private void OnRecordingLimitReached(object sender, RecordingLimitReachedEventArgs e)
        {
            Toast.DisplayText(LocalizedStrings.VoiceMessageLimit);
            recordButton.ImageSource = ImageResources.RecordSymbol;
            isRecording = false;
            sendButton.IsEnabled = true;
            Logger.Error("Audio message time limit reached");
        }

        private void OnRecordButtonPressed(object sender, EventArgs e)
        {
            if (isRecording)
            {
                audioRecorder.Commit();
                audioRecorder.Unprepare();
                recordButton.ImageSource = ImageResources.RecordSymbol;
                sendButton.IsEnabled = true;
                hint.Text = LocalizedStrings.RecordMessageHint;
            }
            else
            {
                DeleteTempFile();

                var internalStorage = StorageManager.Storages
                    .First(s => s.StorageType == StorageArea.Internal)
                    .GetAbsolutePath(DirectoryType.Others);
                voiceMessageTempPath = Path.Combine(internalStorage, Path.GetRandomFileName() + ".3gp");

                audioRecorder.Prepare();
                audioRecorder.Start(voiceMessageTempPath);
                recordButton.ImageSource = ImageResources.StopSymbol;
                sendButton.IsEnabled = false;
                hint.Text = LocalizedStrings.StopRecordMessageHint;
            }

            isRecording = !isRecording;
        }

        private async void OnSendButtonPressed(object sender, EventArgs e)
        {
            if (voiceMessageTempPath == null)
                return;

            activityIndicator.IsVisible = true;

            await NetExceptionCatchHelpers.CatchNetException(
                async () =>
                {
                    sendButton.IsEnabled = false;
                    await messagesManager.SendMessage(dialogId, null, voiceMessageTempPath);
                    DeleteTempFile();
                    await Navigation.PopAsync();
                },
                () =>
                {
                    OnSendButtonPressed(sender, e);
                    return Task.CompletedTask;
                },
                LocalizedStrings.SendMessageNoInternetError);

            activityIndicator.IsVisible = false;
            sendButton.IsEnabled = true;
        }

        public void Dispose()
        {
            audioRecorder.Dispose();
            DeleteTempFile();
        }

        private void DeleteTempFile()
        {
            if (voiceMessageTempPath == null)
                return;

            try
            {
                File.Delete(voiceMessageTempPath);
            }
            catch
            {
                // ignored
            }
        }
    }
}
