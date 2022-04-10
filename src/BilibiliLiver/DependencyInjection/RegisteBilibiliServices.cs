﻿using BilibiliLiver.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BilibiliLiver
{
    public static class RegisteBilibiliServices
    {
        public static IServiceCollection AddBilibiliServices(this IServiceCollection services)
        {
            //Cookie模块
            services.AddSingleton<IBilibiliCookieService, BilibiliCookieService>();
            services.AddTransient<IHttpClientService, HttpClientService>();
            services.AddTransient<IAccountService, AccountService>();
            services.AddTransient<IBilibiliLiveApiService, BilibiliLiveApiService>();

            //推流相关
            //services.AddTransient<PushStreamService>();

            return services;
        }
    }
}
