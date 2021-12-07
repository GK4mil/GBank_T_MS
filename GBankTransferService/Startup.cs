using GBankTransferService.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Nest;
using Plain.RabbitMQ;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GBankTransferService
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddGBankPersistenceEFServices(Configuration);
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "GBankTransferService", Version = "v1" });
            });
           
            services.AddSingleton<IConnectionProvider>(new ConnectionProvider("amqp://guest:guest@192.168.5.3:5672"));
            services.AddSingleton<Plain.RabbitMQ.ISubscriber>(
                x => new Subscriber(x.GetService<IConnectionProvider>(),
                "transfer_exchange",
                "transfer_queue",
                "transfer.*",
                ExchangeType.Topic));

            services.AddHostedService<TransferDataProcess>();

            var settings = new ConnectionSettings(new Uri(@"http://192.168.5.3:9200"));
            services.AddSingleton<IElasticClient>(new ElasticClient(settings));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "GBankTransferService v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
