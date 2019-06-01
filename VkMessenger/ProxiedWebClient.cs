﻿using System.Net;
using Tizen.Network.Connection;

namespace ru.MaxKuzmin.VkMessenger
{
    public class ProxiedWebClient : WebClient
    {
        public ProxiedWebClient()
        {
            string proxyAddress = ConnectionManager.GetProxy(AddressFamily.IPv4);
            if (!string.IsNullOrEmpty(proxyAddress))
            {
                Proxy = new WebProxy(proxyAddress, true);
            }
        }
    }
}