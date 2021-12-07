using GBankTransferService.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GBankTransferService.Interfaces
{
    public interface IUserRepository : IAsyncRepository<User>
    {
    }
}
