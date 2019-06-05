using System;
using System.ComponentModel;
using System.Security;
using Dotc.MQ;
using IBM.WMQ;
using IBM.WMQ.PCF;
using Dotc.MQExplorerPlus.Core.Controllers;
using Dotc.MQExplorerPlus.Core.Models;
using Dotc.MQExplorerPlus.Core.Views;
using Dotc.Mvvm;
using System.Collections.Generic;
using System.Linq;
using Dotc.MQExplorerPlus.Core.ViewModels;
using Dotc.MQ.Websphere;

namespace Dotc.Mq.Console
{
    public sealed class OpenQueueManager
    {
        private string _queueManagerName = "QMESB";
        private string _queueNamePrefixFilter;
        private bool _isRemote = true;
        private bool _showObjectFilter = true;

        public OpenQueueManager()
        {
            StaticQueueNames = new QueueNames();
        }

        public void Initialize(bool remote = true)
        {
            IsRemote = remote;
            if (remote)
            {
                RemoteConfig = new RemoteConfiguration();
            }
        }


        public bool IsRemote
        {
            get { return _isRemote; }
            set { _isRemote = value; }
        }

        public bool ShowObjectFilter
        {
            get { return _showObjectFilter; }
            set { _showObjectFilter = value; }
        }

        public bool ShowObjectPrefixFilter
        {
            get { return ShowObjectFilter && FilterByPrefix; }
        }

        public RemoteConfiguration RemoteConfig { get; private set; }


        public string QueueManagerName
        {
            get { return _queueManagerName; }
            set { _queueManagerName = value; }
        }

        public string QueueNamePrefixFilter
        {
            get { return _queueNamePrefixFilter; }
            set { _queueNamePrefixFilter = value; }
        }


        public IQueueManager QueueManager { get; private set; }

        private bool _filterByPrefix;
        public bool FilterByPrefix
        {
            get
            {
                return _filterByPrefix;
            }
            set
            {
                _filterByPrefix = value;
            }
        }

        private bool _filterByQueueNames;
        public bool FilterByQueueNames
        {
            get
            {
                return _filterByQueueNames;
            }
            set
            {
                _filterByQueueNames = value;
            }
        }

        public IObjectNameFilter GetObjectNameFilter()
        {

            if (FilterByPrefix)
            {
                return QueueManager?.NewObjectNameFilter(this.QueueNamePrefixFilter);
            }
            if (FilterByQueueNames)
            {
                return new StaticQueueList((IEnumerable<string>)StaticQueueNames.List);
            }
            return QueueManager?.NewObjectNameFilter(this.QueueNamePrefixFilter); ;
        }

        public void OnOk()
        {
            try
            {
                IQueueManagerFactory _mqFactory = new WsQueueManagerFactory();
                var cp = _mqFactory.NewConnectionProperties();
                cp.Set(RemoteConfig.Host, RemoteConfig.Port, RemoteConfig.Channel, RemoteConfig.UserId, RemoteConfig.Password);
                QueueManager = _mqFactory.Connect(_queueManagerName, cp);

                var q = new WsQueue(QueueManager as WsQueueManager, "FDDS_TCDM");
                q.DeleteMessages(5);
            }
            catch (MqException ex)
            {
                System.Console.WriteLine("Connection failed", ex);
            }
        }

        public QueueNames StaticQueueNames { get; private set; }
    }
}
