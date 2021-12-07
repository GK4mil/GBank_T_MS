using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GBankTransferService.Model
{
    
    public class BillLog
    {
        public DateTime datetime;
        public String recieverName;
        public String recieverAddress;
        public String title;
        public String amount;
        public String senderBillNumber;
        public String recieverBillNumber;
        public String status;
        public String optionalInfo;
        public String transactionID;

    }
}
