using GBankTransferService.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GBankTransferService.Interfaces
{
    public interface IBillRepository : IAsyncRepository<Bill>
    {
        Task<List<Bill>> GetBillsByUserId(int userId);
        Task<List<Bill>> FindByBillNumber(String billnr);
        Task<List<Bill>> GetBillsOfUser(String username);

    }
}
