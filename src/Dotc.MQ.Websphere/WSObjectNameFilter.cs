﻿#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System;

namespace Dotc.MQ.Websphere
{

    public sealed class WsObjectNameFilter : IObjectNameFilter
    {
        public WsObjectNameFilter(string namePrefix)
        {
            NamePrefix = new [] { namePrefix };
        }

        public string[] NamePrefix { get; }

        public string UniqueId
        {
            get
            {
                return NamePrefix[0] ?? "*";
            }
        }

        public bool IsMatch(string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (name.StartsWith("AMQ.",StringComparison.Ordinal)) // Exclude temporary queues
                return false;

            if (!string.IsNullOrEmpty(NamePrefix[0])
                && !name.StartsWith(NamePrefix[0], StringComparison.Ordinal))
                return  false;

            return true;
        }
    }

    public class WsSystemObjectNameFilter : IObjectNameFilter
    {

        public WsSystemObjectNameFilter()
        {
            NamePrefix = new[] {"SYSTEM."};
        }

        public string[] NamePrefix { get; }

        public bool IsMatch(string name)
        {
            return IsSystemQueue(name);
        }

        public static bool IsSystemQueue(string name)
        {
            return (name.StartsWith("SYSTEM.", StringComparison.Ordinal));
        }

        public static bool IsSystemChannel(string name)
        {
            return (name.StartsWith("SYSTEM.", StringComparison.Ordinal));
        }

        public static bool IsSystemListener(string name)
        {
            return (name.StartsWith("SYSTEM.", StringComparison.Ordinal));
        }

        public string UniqueId
        {
            get
            {
                return NamePrefix[0];
            }
        }
    }
}
