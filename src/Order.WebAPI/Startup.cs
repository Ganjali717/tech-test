using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Order.Data.Context;
using Order.Data.Repositories;
using Order.Model.Requests;
using Order.Service.Interfaces;
using Order.Service.Status;
using OrderService.WebAPI.Middleware;
using OrderService.WebAPI.Validation;

namespace OrderService.WebAPI
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
            services.AddDbContext<OrderContext>(options =>
            {
                var serviceOptions = Configuration["OrderConnectionString"];
                options
                .UseLazyLoadingProxies()
                .UseMySQL(serviceOptions);
            });

            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IOrderService, Order.Service.OrderService>();
            services.AddSingleton<IOrderStatusNormalizer, OrderStatusNormalizer>();

            services.AddTransient<IValidator<CreateOrderRequest>, CreateOrderRequestValidator>();
            services.AddTransient<IValidator<UpdateOrderStatusRequest>, UpdateOrderStatusRequestValidator>();

            services.AddControllers();

            services.AddSwaggerGen();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Order API V1");
                });
            }

            app.UseHttpsRedirection();

            app.UseMiddleware<ErrorHandlingMiddleware>();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
