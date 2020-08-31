using System;
using System.Linq;
using Microsoft.Extensions.Hosting;

namespace BotRunner
{
    public static class EnvironmentHelper
    {
        public static readonly string[] ValidEnvironments = new string[] { Environments.Development, Environments.Staging, Environments.Production };

        private const string EnvironmentNameVariable = "ASPNETCORE_ENVIRONMENT";

        public static void SetEnvironmentName(string environment)
        {
            if (ValidEnvironments.Contains(environment))
                Environment.SetEnvironmentVariable(EnvironmentNameVariable, environment);
        }

        public static string GetEnvironmentName() => Environment.GetEnvironmentVariable(EnvironmentNameVariable);
    }
}
