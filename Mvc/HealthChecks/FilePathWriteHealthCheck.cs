using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Mvc.HealthChecks
{
    public class FilePathWriteHealthCheck : IHealthCheck
    {
        private readonly string _filePath;
        private IReadOnlyDictionary<string, object> healthCheckData;

        public FilePathWriteHealthCheck(string filePath)
        {
            _filePath = filePath;
            healthCheckData = new Dictionary<string, object> { { "filePath", _filePath } };
        }
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var testFile = $"{_filePath}\\test.txt";
                using (var fs = File.Create(testFile))
                {
                }
                File.Delete(testFile);
                return Task.FromResult(HealthCheckResult.Healthy());
            }
            catch (Exception e)
            {
                switch(context.Registration.FailureStatus)
                {
                    case HealthStatus.Degraded:
                        return Task.FromResult(HealthCheckResult.Degraded($"Issues writing to file path", e, healthCheckData));
                    case HealthStatus.Healthy:
                        return Task.FromResult(HealthCheckResult.Healthy($"Issues writing to file path", healthCheckData));
                    default:
                        return Task.FromResult(HealthCheckResult.Unhealthy($"Issues writing to file path", e, healthCheckData));
                }
            }
        }
    }
}
