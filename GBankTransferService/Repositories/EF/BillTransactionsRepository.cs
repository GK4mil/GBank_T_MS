using GBankTransferService.Interfaces;
using GBankTransferService.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GBankTransferService.Model;

namespace GBankTransferService.Repositories.EF
{
    public class BillTransactionsRepository : BaseRepository<BillTransactions>, IBillTransactionsRepository
    {
        public BillTransactionsRepository(GBankDbContext dbContext) : base(dbContext)
        {
        }

    }
}
