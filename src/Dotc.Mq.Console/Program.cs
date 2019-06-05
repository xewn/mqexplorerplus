using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dotc.Mq.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var openQueueManager = new OpenQueueManager();
            openQueueManager.Initialize(true);
            openQueueManager.OnOk();
        }
    }
}
