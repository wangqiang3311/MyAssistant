using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ServiceStack;
using ServiceStack.Configuration;
using ServiceStack.Data;
using ServiceStack.OrmLite;

namespace MyAssistant
{
    public class ConfigureDb : IConfigureServices, IConfigureAppHost
    {
        IConfiguration Configuration { get; }
        public ConfigureDb(IConfiguration configuration) => Configuration = configuration;

        public static IOrmLiteDialectProvider GetConnectionProvider(string dbType)
        {
            return dbType switch
            {
                 "SqliteFileDb" => SqliteDialect.Provider,
                 "MySqlDb" => MySqlDialect.Provider,
                 _ => MySqlDialect.Provider
            };
        }

        public void Configure(IServiceCollection services)
        {
            var appSettings = new AppSettings();
            var dbTypeSys = appSettings.Get<string>("SYS.DbType");
            var dbConnectionSys = appSettings.Get<string>("SYS.DbConnection");

            var dbFactory = new OrmLiteConnectionFactory(dbConnectionSys, GetConnectionProvider(dbTypeSys));
            services.AddSingleton<IDbConnectionFactory>(dbFactory);

            var appDataDbConnectionsList = appSettings.GetList("AppDataDbConnections");
            foreach (var appDataDbConnection in appDataDbConnectionsList)
            {
                var connectionName = appDataDbConnection.Replace(" ", "");
                var dbType = appSettings.Get<string>(connectionName + ".DbType");
                var dbConnection = appSettings.Get<string>(connectionName + ".DbConnection");
                dbFactory.RegisterConnection(connectionName, dbConnection, GetConnectionProvider(dbType));
            }
        }

        public void Configure(IAppHost appHost)
        {
            appHost.Plugins.AddIfNotExists(new AutoQueryFeature
            {
                IncludeTotal = true,
                MaxLimit = 100
            });

            appHost.Plugins.AddIfNotExists(new AutoQueryDataFeature
            {
                IncludeTotal = true,
                MaxLimit = 100
            });
        }
    }    
}
