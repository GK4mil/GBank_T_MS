using GBankTransferService.Interfaces;
using GBankTransferService.Model;
using GBankTransferService.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GBankTransferService.Repositories.EF
{
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        public UserRepository(GBankDbContext dbContext) : base(dbContext)
        {
        }

    }
}
