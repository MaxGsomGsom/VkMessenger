﻿using System;
using System.Threading.Tasks;
using ru.MaxKuzmin.VkMessenger.Clients;
using Tizen.Applications;
using Xamarin.Forms;
using Application = Xamarin.Forms.Application;

namespace ru.MaxKuzmin.VkMessenger.Managers
{
    public static class AuthorizationManager
    {
        private const string TokenKey = "Token2";
        private const string UserIdKey = "UserId";
        private const string PhotoKey = "Photo";
        private const string TutorialShownKey = "TutorialShown";

        private static string? token;
        private static int userId;
        private static ImageSource? photoSource;
        private static bool? tutorialShown;

        public static string? Token
        {
            get
            {
                if (token == null)
                    token = Preference.Contains(TokenKey) ? Preference.Get<string>(TokenKey) : null;
                return token;
            }
            private set
            {
                if (value != null)
                {
                    Preference.Set(TokenKey, value);
                    token = value;
                }
                else
                {
                    Preference.Remove(TokenKey);
                }
            }
        }

        public static int UserId
        {
            get
            {
                if (userId == 0)
                    userId = Preference.Contains(UserIdKey) ? Preference.Get<int>(UserIdKey) : 0;
                return userId;
            }
            private set
            {
                Preference.Set(UserIdKey, value);
                userId = value;
            }
        }

        public static bool TutorialShown
        {
            get
            {
                tutorialShown ??= Preference.Contains(TutorialShownKey) && Preference.Get<bool>(TutorialShownKey);
                return tutorialShown.Value;
            }
            set
            {
                Preference.Set(TutorialShownKey, value);
                tutorialShown = value;
            }
        }

        public static ImageSource? Photo
        {
            get
            {
                if (photoSource == null && userId != 0)
                    photoSource = ImageSource.FromUri(new Uri(Preference.Get<string>(PhotoKey)));
                return photoSource;
            }
        }

        public static bool AuthorizeFromUrl(Uri url)
        {
            var result = AuthorizationClient.SetUserFromUrl(url);
            if (result.HasValue)
            {
                Token = result.Value.Token;
                UserId = result.Value.UserId;
            }

            return result.HasValue;
        }

        public static void CleanUserAndExit()
        {
            Token = null;
            Application.Current.Quit();
        }

        public static async Task SetPhoto()
        {
            if (token == null || userId == 0)
                return;
            
            var url = await AuthorizationClient.GetPhoto(token, userId);
            Preference.Set(PhotoKey, url.ToString());
            photoSource = ImageSource.FromUri(url);
        }
    }
}
