﻿using ru.MaxKuzmin.VkMessenger.Clients;
using ru.MaxKuzmin.VkMessenger.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace ru.MaxKuzmin.VkMessenger.Extensions
{
    public static class DialogsCollectionExtensions
    {
        /// <summary>
        /// Update dialogs from API. Can be used during setup of page or with <see cref="LongPolling"/>
        /// </summary>
        /// <param name="dialogIds">Dialog id collection or null</param>
        /// <returns>Null means update successfull</returns>
        public static async Task<Exception> Update(this ObservableCollection<Dialog> dialogs, IReadOnlyCollection<int> dialogIds)
        {
            try
            {
                var newDialogs = await DialogsClient.GetDialogs(dialogIds);

                foreach (var newDialog in newDialogs.AsEnumerable().Reverse())
                {
                    var foundDialog = dialogs.FirstOrDefault(d => d.Id == newDialog.Id);

                    if (foundDialog == null)
                    {
                        dialogs.Insert(0, newDialog);
                    }
                    else
                    {
                        UpdateDialog(newDialog, foundDialog);

                        if (dialogs.Last() != foundDialog)
                        {
                            dialogs.Remove(foundDialog);
                            dialogs.Insert(0, foundDialog);
                        }
                    }
                }

                return null;
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return e;
            }
        }

        /// <summary>
        /// Update dialog data without recreating it
        /// </summary>
        private static void UpdateDialog(Dialog newDialog, Dialog foundDialog)
        {
            foreach (var newProfile in newDialog.Profiles)
            {
                var foundProfile = foundDialog.Profiles.FirstOrDefault(p => p.Id == newProfile.Id);
                if (foundProfile != null)
                    foundProfile.IsOnline = newDialog.IsOnline;
            }
        }
    }
}
