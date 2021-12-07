using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace GBankTransferService.Model
{
    public class Bill
    {
        public Bill()
        {
            this.Users = new Collection<User>();
            this.Transactions = new Collection<BillTransactions>();

        }
        public int ID { get; set; }
        [Column(TypeName = "decimal(18, 2)")] public Decimal balance { get; set; }

        public String billNumber { get; set; }

        public virtual ICollection<BillTransactions> Transactions { get; set; }
        public virtual ICollection<User> Users { get; set; }

    }
}
