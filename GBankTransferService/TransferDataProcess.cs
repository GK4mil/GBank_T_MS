using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Plain.RabbitMQ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GBankTransferService
{
    public class TransferDataProcess : IHostedService
    {
        private readonly ISubscriber _subscriber;
        public TransferDataProcess(ISubscriber subscriber)
        {
            _subscriber = subscriber;
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _subscriber.Subscribe(ProcessMessageFromRMQ);
            return Task.CompletedTask;
        }
        private bool ProcessMessageFromRMQ(string message, IDictionary<string,object> headers)
        {
            Console.WriteLine(message);
            var command = JsonConvert.DeserializeObject<TransferCommand>(message);
            return true;
        }
        public Task StopAsync(CancellationToken cancellationToken)
        {

            return Task.CompletedTask;
        }
    }

    internal class TransferCommand
    {
        public String amount { get; set; }
        public String senderBillNumber { get; set; }
        public String recieiverBillNumber { get; set; }
    }
}
