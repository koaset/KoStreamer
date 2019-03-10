using DbUp;
using System;
using System.Linq;
using System.Reflection;

namespace PostegresDbUp
{
    class Program
    {
        static int Main(string[] args)
        {
            var dbEnvVarKey = "DATABASE_CONNECTIONSTRING";
            var connectionString = Environment.GetEnvironmentVariable(dbEnvVarKey) ?? Environment.GetEnvironmentVariable(dbEnvVarKey, EnvironmentVariableTarget.Machine);

            var upgrader =
                DeployChanges.To
                    .PostgresqlDatabase(connectionString)
                    .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
                    .LogToConsole()
                    .Build();

            var scripts = upgrader.GetScriptsToExecute();
            if (scripts.Any())
            {
                Console.WriteLine("Scripts to execute:");
                foreach (var script in scripts)
                    Console.WriteLine(script.Name);
            }
            else
            {
                Console.WriteLine("No scripts to exectue.");
                return 0;
            }

            var result = upgrader.PerformUpgrade();

            if (!result.Successful)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(result.Error);
                Console.ResetColor();
                Console.ReadLine();
                return -1;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Success!");
            Console.ResetColor();
            return 0;
        }
    }
}
