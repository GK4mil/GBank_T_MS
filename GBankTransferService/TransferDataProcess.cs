using GBankTransferService.Interfaces;
using GBankTransferService.Model;
using GBankTransferService.Persistence;
using GBankTransferService.Repositories.EF;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nest;
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
        private readonly IElasticClient _ec;
      

        private readonly IServiceScopeFactory scopeFactory;
        public TransferDataProcess(ISubscriber subscriber, IServiceScopeFactory scopeFactory, IElasticClient ec)
        {
            this.scopeFactory = scopeFactory;
            _subscriber = subscriber;
            _ec = ec;
          
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _subscriber.Subscribe(ProcessMessageFromRMQ);
            return Task.CompletedTask;
        }
        private bool ProcessMessageFromRMQ(string message, IDictionary<string, object> headers)
        {
            Bill senderBill = null;
            Bill receiverBill = null;
            var command = JsonConvert.DeserializeObject<TransferCommand>(message);

            using (var scope = scopeFactory.CreateScope())
            {
                var _ct = scope.ServiceProvider.GetRequiredService<GBankDbContext>();
                Console.WriteLine(message);
                
                senderBill = _ct.Bills.Where(x => x.billNumber == command.senderBillNumber).FirstOrDefault();
                receiverBill = _ct.Bills.Where(x => x.billNumber == command.recieiverBillNumber).FirstOrDefault();
            }
            
            AddToLog(command, "pending");
            Console.WriteLine("sender balance: ");

            if (senderBill != null)
                Console.WriteLine(senderBill.balance);
            else
            {
                AddToLog(command, "error", "sender bill does not exist");
                Console.WriteLine("null - bill does not exist ");
                return true;
            }
            Console.WriteLine("receiver balance: ");

            if (receiverBill != null)
                Console.WriteLine(receiverBill.balance);
            else
            {
                AddToLog(command, "error", "receiver bill does not exist");
                Console.WriteLine("null - bill does not exist ");
                return true;
            }

            Decimal parsedAmount;

            if (Decimal.TryParse(command.amount, out parsedAmount))
            {
                if (senderBill.balance >= parsedAmount)
                {
                    using (var scope = scopeFactory.CreateScope())
                    {
                        var _ct = scope.ServiceProvider.GetRequiredService<GBankDbContext>();


                        senderBill = _ct.Bills.Where(x => x.billNumber == command.senderBillNumber).FirstOrDefault();
                        receiverBill = _ct.Bills.Where(x => x.billNumber == command.recieiverBillNumber).FirstOrDefault();
                        if (senderBill != null)
                            senderBill.balance -= Decimal.Parse(command.amount);
                        if (receiverBill != null)
                            receiverBill.balance += Decimal.Parse(command.amount);
                        _ct.SaveChanges();
                    }
                }
                else
                {
                    AddToLog(command, "error", "no enough money");
                    Console.WriteLine("No enough money in source bill");
                    return true;
                }
            }
            else
            {
                AddToLog(command, "error", "amount wrong format");
                Console.WriteLine("Amount isn't a decimal number");
                return true;
            }

            AddToTransactionTable(command);
            AddToLog(command, "done");

            return true;
        }
        public Task StopAsync(CancellationToken cancellationToken)
        {

            return Task.CompletedTask;
        }
        
        private void AddToLog(TransferCommand command, String status, String optinfo="-")
        {
            _ec.Index<BillLog>(new BillLog()
            {
                datetime = DateTime.Now,
                recieverName = command.recieverName,
                recieverAddress = command.recieverAddress,
                title = command.title,
                amount = command.amount,
                senderBillNumber = command.senderBillNumber,
                recieverBillNumber = command.recieiverBillNumber,
                status = status,
                optionalInfo=optinfo,
                transactionID=command.transactionID
            }, x => x.Index("transactions"));

        }
 
        private async void AddToTransactionTable(TransferCommand command, String dir = "in")
        {
            Bill resultBill = null;
            using (var scope = scopeFactory.CreateScope())
            {
                var _br = scope.ServiceProvider.GetRequiredService<IBillRepository>();
                resultBill = (await _br.FindByBillNumber(command.recieiverBillNumber))[0];
            }
            using (var scope = scopeFactory.CreateScope())
            {
                var _ct = scope.ServiceProvider.GetRequiredService<GBankDbContext>();
                if (resultBill != null)
                {
                    /* var commandText = "INSERT Transaction (datetime, direction, billID, transactionid) VALUES (@datetime, @direction, @billID, @transactionid)";
                     var datetime = new SqlParameter("@datetime", DateTime.Now);
                     var direction = new SqlParameter("@direction", dir);
                     var billID = new SqlParameter("@billID", resultBill.ID);
                     var transactionid = new SqlParameter("@transactionid", command.transactionID);
                     _ct.Transaction.FromSqlRaw(commandText, datetime, direction, billID, transactionid);
                     _ct.SaveChanges();*/




                    /*using (SqlConnection cnn = 
                        new SqlConnection(@"Data Source = 192.168.5.3, 1433; Initial Catalog = gabank; User ID = sa; Password = 1qaz@WSX3edc")) 
                    {
                        try
                        {
                            SqlCommand c = new SqlCommand("INSERT Transaction (datetime, direction, billID, transactionid) " +
                                "VALUES ("+DateTime.Now.ToString()+", "+ dir+", " + resultBill.ID +", "+command.transactionID+");", cnn);
                            c.Connection.Open();
                            c.ExecuteNonQuery();
                            
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("sql connection error");
                        }

                    }*/

                    /////dzialajace
                    try
                    {
                        string connetionString;
                        SqlConnection cnn;
                        connetionString = @"Data Source=192.168.5.3;Initial Catalog=gabank;User ID=sa;Password=1qaz@WSX3edc";
                        cnn = new SqlConnection(connetionString);
                        cnn.Open();
                        
                        
                        SqlDataAdapter adapter = new SqlDataAdapter();
                        String query = "Insert into BillTransactions (datetime, direction, billID, transactionid) values (@datetime, @dir, @billid, @transactionid)";
                        SqlCommand comm= new SqlCommand(query, cnn);

                        adapter.InsertCommand = comm;

                        adapter.InsertCommand.Parameters.AddWithValue("@datetime", DateTime.Now);
                        adapter.InsertCommand.Parameters.AddWithValue("@dir", dir);
                        adapter.InsertCommand.Parameters.AddWithValue("@billid", resultBill.ID);
                        adapter.InsertCommand.Parameters.AddWithValue("@transactionid", command.transactionID);
                        int insertedRows = adapter.InsertCommand.ExecuteNonQuery();
                        if (insertedRows < 1)
                            throw new Exception("No row was inserted !");
                        Console.WriteLine("Inserted rows: " +insertedRows );

                        comm.Dispose();
                        cnn.Close();
                    }
                    catch(Exception e)
                    {
                        
                        Console.WriteLine(e.ToString());
                    }
                    //_ct.BillTransactions.Add(new BillTransactions() { datetime = DateTime.Now, direction = "in", bill = resultBill, transactionid = command.transactionID });
                    //_ct.SaveChanges();
                }
                  
                    //await _ct.AddAsync(new Transaction() { datetime = DateTime.Now, direction = "in", bill = resultBill, transactionid = command.transactionID });
            }

        }
    }

    internal class TransferCommand
    {
        public String recieverName { get; set; }
        public String recieverAddress { get; set; }
        public String title { get; set; }
        public String amount { get; set; }
        public String senderBillNumber { get; set; }
        public String recieiverBillNumber { get; set; }
        public String transactionID { get; set; }
    }
}
