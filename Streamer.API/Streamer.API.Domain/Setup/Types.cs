﻿using Microsoft.Extensions.DependencyInjection;
using Streamer.API.Domain.Interfaces;

namespace Streamer.API.Domain.Setup
{
    public static class Types
    {
        public static void Register(IServiceCollection serviceCollection)
        {
            serviceCollection.AddHttpContextAccessor();
            serviceCollection.AddSingleton<IDataAccess, DataAccess>();
            serviceCollection.AddTransient<IGoogleTokenHelper, GoogleTokenHelper>();
            serviceCollection.AddTransient<ISessionService, SessionService>();
            serviceCollection.AddTransient<IAccountService, AccountService>();
            serviceCollection.AddTransient<ILibraryService, LibraryService>();
            serviceCollection.AddTransient(provider => provider.GetService<IAccountService>().GetAccountBySession());
        }
    }
}
