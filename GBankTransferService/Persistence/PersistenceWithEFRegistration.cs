using GBankTransferService.Interfaces;
using GBankTransferService.Repositories.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GBankTransferService.Persistence
{
    public static class PersistenceWithEFRegistration
    {
        public static IServiceCollection AddGBankPersistenceEFServices(this IServiceCollection services, IConfiguration configuration)
        {
            {
                services.AddDbContext<GBankDbContext>(options => options.UseSqlServer(configuration.GetConnectionString("MSSQLConnectionString")));

                services.AddScoped(typeof(IAsyncRepository<>), typeof(BaseRepository<>));

                services.AddScoped<IUserRepository, UserRepository>();

                services.AddScoped<IBillRepository, BillRepository>();

                services.AddScoped<IBillTransactionsRepository, BillTransactionsRepository>();


                return services;
            }
        }
    }
}
