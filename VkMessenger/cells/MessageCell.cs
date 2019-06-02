﻿using FFImageLoading.Forms;
using ru.MaxKuzmin.VkMessenger.Models;
using Xamarin.Forms;

namespace ru.MaxKuzmin.VkMessenger.Cells
{
    public class MessageCell : ViewCell
    {
        private CachedImage photo = new CachedImage
        {
            Aspect = Aspect.AspectFit,
            HeightRequest = 40,
            WidthRequest = 40
        };
        private Label text = new Label
        {
            FontSize = Device.GetNamedSize(NamedSize.Micro, typeof(Label)),
            LineBreakMode = LineBreakMode.WordWrap
        };
        private StackLayout wrapperLayout = new StackLayout
        {
            Orientation = StackOrientation.Horizontal,
            Padding = new Thickness(10, 0),
            VerticalOptions = LayoutOptions.FillAndExpand
        };

        private static readonly BindableProperty SenderIdProperty =
            BindableProperty.Create(
                nameof(Message.SenderId),
                typeof(int),
                typeof(MessageCell),
                default(int),
                propertyChanged: OnSenderIdPropertyChanged);

        private static readonly BindableProperty UnreadProperty =
            BindableProperty.Create(
                nameof(Message.Unread),
                typeof(bool),
                typeof(MessageCell),
                default(bool),
                propertyChanged: OnUnreadPropertyChanged);

        public MessageCell()
        {
            photo.SetBinding(CachedImage.SourceProperty, nameof(Message.Photo));
            text.SetBinding(Label.TextProperty, nameof(Message.Text));
            this.SetBinding(SenderIdProperty, nameof(Message.SenderId));
            this.SetBinding(UnreadProperty, nameof(Message.Unread));

            wrapperLayout.Children.Add(photo);
            wrapperLayout.Children.Add(text);
            View = wrapperLayout;
        }

        private static void OnSenderIdPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is MessageCell cell && oldValue != newValue)
            {
                var dialogId = (int)newValue;
                if (dialogId != Authorization.UserId)
                {
                    cell.wrapperLayout.LowerChild(cell.photo);
                    cell.photo.HorizontalOptions = LayoutOptions.End;
                    cell.text.HorizontalOptions = LayoutOptions.StartAndExpand;
                }
                else
                {
                    cell.wrapperLayout.RaiseChild(cell.photo);
                    cell.photo.HorizontalOptions = LayoutOptions.Start;
                    cell.text.HorizontalOptions = LayoutOptions.EndAndExpand;
                }
            }
        }

        private static void OnUnreadPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is MessageCell cell && oldValue != newValue)
            {
                var unread = (bool)newValue;
                if (unread)
                {
                    cell.View.BackgroundColor = Color.FromHex("00354A");
                }
                else
                {
                    cell.View.BackgroundColor = Color.Black;
                }
            }
        }
    }
}