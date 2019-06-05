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
using System.Threading;

namespace Dotc.Mq.Console
{
    public sealed class OpenQueueManager
    {
        private string _queueManagerName = "QMESB";
        private string _queueNamePrefixFilter;
        private bool _isRemote = true;
        private bool _showObjectFilter = true;
        private int messageCount = 5;
        private List<string> queueNameList;
        public OpenQueueManager()
        {
            StaticQueueNames = new QueueNames();
            queueNameList = new List<string>() { "FDDS_TCDM", "ACDMDS_TCDM", "CDMDS_TCDM", "PIDS_PSTA_TCDM" };
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

                //var q = new WsQueue(QueueManager as WsQueueManager, "PIDS_PSTA_TCDM");
                while(true)
                { 
                    foreach (var item in queueNameList)
                    {
                        System.Console.WriteLine($"读取队列{item}");
                        var messages = ReadMessage(item);
                        System.Console.WriteLine($"{item} 一共读取到{messages.Count}条数据");
                        DeleteMessages(item, messages);
                    }
                    Thread.Sleep(500);
                }
            }
            catch (MqException ex)
            {
                System.Console.WriteLine("Connection failed", ex);
            }
        }
        /// <summary>
        /// 读取消息
        /// </summary>
        /// <param name="queueName"></param>
        /// <returns></returns>
        private IList<IMessage> ReadMessage(string queueName)
        {
            IList<IMessage> messages = new List<IMessage>();
            var ibmQueue = (QueueManager as WsQueueManager).OpenQueueCore(queueName, OpenQueueMode.ForBrowseAndRead);
            try
            {
                var browseOption = MQC.MQGMO_BROWSE_FIRST;

                var getMsgOpts = new MQGetMessageOptions()
                {
                    Options = MQC.MQGMO_FAIL_IF_QUIESCING | browseOption
                };
                int count = messageCount;
                while (count > 0)
                {
                    var msg = new MQMessage();
                    System.Console.WriteLine($"{queueName} 循环第{count}次");
                    ibmQueue.Get(msg, getMsgOpts);
                    var localMsg = new WsMessage(msg);
                    messages.Add(localMsg);
                    getMsgOpts.Options = MQC.MQGMO_FAIL_IF_QUIESCING | MQC.MQGMO_BROWSE_NEXT;
                    count--;
                }
            }
            catch (MQException ex)
            {
                if (ex.ReasonCode == 2033 /* MQRC_NO_MSG_AVAILABLE */)
                {
                }
                else throw;
            }
            finally
            {
                ibmQueue.Close();
            }
            return messages;
        }

        /// <summary>
        /// 批量删除消息
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="messages"></param>
        public void DeleteMessages(string queueName, IList<IMessage> messages)
        {

            if (messages == null) throw new ArgumentNullException(nameof(messages));

            try
            {
                var ibmQueue = (QueueManager as WsQueueManager).OpenQueueCore(queueName, OpenQueueMode.ForRead);
                try
                {
                    var mqGetMsgOpts = new MQGetMessageOptions
                    {
                        Options = MQC.MQGMO_FAIL_IF_QUIESCING,
                        MatchOptions = MQC.MQMO_MATCH_MSG_ID
                    };

                    int count = 0;
                    foreach (var m in messages)
                    {
                        try
                        {
                            var msg = ((WsMessage)m).IbmMessage;
                            ibmQueue.Get(msg, mqGetMsgOpts);

                            count++;
                        }
                        catch (MQException ex)
                        {
                            if (ex.ReasonCode == 2033 /* MQRC_NO_MSG_AVAILABLE */)
                            {
                            }
                            else throw;
                        }
                    }
                }
                finally
                {
                    ibmQueue.Close();
                }


            }

            catch (MQException ibmEx)
            {
            }
        }
        public QueueNames StaticQueueNames { get; private set; }
    }
}
