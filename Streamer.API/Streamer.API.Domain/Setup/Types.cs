using Microsoft.Extensions.DependencyInjection;
using Streamer.API.Domain.Interfaces;

namespace Streamer.API.Domain.Setup
{
    public static class Types
    {
        public static void Register(IServiceCollection serviceCollection)
        {
            serviceCollection.AddHttpContextAccessor();
            serviceCollection.AddSingleton<IDataAccess, DataAccess>();
            serviceCollection.AddTransient<ISessionService, SessionService>();
            serviceCollection.AddTransient<IAccountService, AccountService>();
        }
    }
}
