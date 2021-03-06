﻿using FFImageLoading.Forms;
using ru.MaxKuzmin.VkMessenger.Localization;
using ru.MaxKuzmin.VkMessenger.Models;
using Tizen.Applications;
using Tizen.Wearable.CircularUI.Forms;
using Xamarin.Forms;

namespace ru.MaxKuzmin.VkMessenger.Pages
{
    public class MessagePage : BezelInteractionPage
    {
        private readonly CircleScrollView scrollView = new CircleScrollView();
        private readonly StackLayout wrapperLayout = new StackLayout
        {
            Orientation = StackOrientation.Vertical,
            VerticalOptions = LayoutOptions.FillAndExpand
        };

        private static Label CreateLabel(string text, bool marginTop = false) => new Label
        {
            FontSize = Device.GetNamedSize(NamedSize.Micro, typeof(Label)),
            LineBreakMode = LineBreakMode.WordWrap,
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalTextAlignment = TextAlignment.Center,
            Margin = new Thickness(30, marginTop ? 70 : 10, 30, 0),
            Text = text
        };

        private static Label CreateUri(string text) => new Label
        {
            FontSize = Device.GetNamedSize(NamedSize.Micro, typeof(Label)),
            LineBreakMode = LineBreakMode.WordWrap,
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalTextAlignment = TextAlignment.Center,
            Margin = new Thickness(30, 10, 30, 0),
            TextColor = Consts.BrightBlue,
            TextDecorations = TextDecorations.Underline,
            Text = text
        };

        private static CachedImage CreateImage(ImageSource source) => new CachedImage
        {
            Margin = new Thickness(0, 10, 0, 0),
            LoadingPlaceholder = ImageResources.Placeholder,
            Source = source
        };

        public MessagePage(Message message)
        {
            NavigationPage.SetHasNavigationBar(this, false);

            wrapperLayout.Children.Add(CreateLabel(message.FullText, true));

            foreach (var item in message.AttachmentMessages)
            {
                var text = LocalizedStrings.ForwardedMessage
                           + (item.Profile?.Name != null 
                               ? $" {LocalizedStrings.From} " + item.Profile.Name
                               : string.Empty)
                           + $":\n\"{item.Text}\"";
                wrapperLayout.Children.Add(CreateLabel(text));
            }

            foreach (var item in message.AttachmentUris)
            {
                var uri = CreateUri(item.ToString());
                wrapperLayout.Children.Add(uri);

                var tapGestureRecognizer = new TapGestureRecognizer();
                tapGestureRecognizer.Tapped += (s, e) =>
                {
                    AppControl.SendLaunchRequest(new AppControl
                    {
                        Operation = AppControlOperations.View,
                        Uri = item.ToString()
                    });
                    Toast.DisplayText(LocalizedStrings.OpeningBrowser);
                };
                uri.GestureRecognizers.Add(tapGestureRecognizer);
            }

            foreach (var item in message.AttachmentImages)
            {
                var image = CreateImage(item.Url);
                wrapperLayout.Children.Add(image);
            }

            var emptyLabel = new Label { Margin = new Thickness(0, 0, 0, 70) };
            wrapperLayout.Children.Add(emptyLabel);

            scrollView.Content = wrapperLayout;
            Content = scrollView;
            SetBinding(RotaryFocusObjectProperty, new Binding { Source = scrollView });
        }
    }
}
