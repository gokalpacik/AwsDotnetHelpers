using Amazon;
using Amazon.Runtime;
using Microsoft.Extensions.DependencyInjection;
using SqsHelper.Delegates;
using SqsHelper.Models;

namespace SqsHelper.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        ///  Adds sqs helper to service provider.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="options"></param>
        public static void AddSqsHelper(this IServiceCollection services, AwsSqsOptions options)
        {
            if (options is null) throw new ArgumentException("AWS Options should not be null !");
            options.Validate();

            var serviceProvider = services.BuildServiceProvider();
            var sqsServices = serviceProvider.GetServices<ISqsHelper>();

            var hasMultipleServicesWithNullQueueId = sqsServices.Where(r => string.IsNullOrWhiteSpace(((SqsHelper)r).UniqueKey)).Any();
            var isUniqueKeyAlreadyUsed = sqsServices.Where(r => ((SqsHelper)r).UniqueKey == options.UniqueKey).Any();

            if (hasMultipleServicesWithNullQueueId) throw new Exception("UniqueKey must be specified when registering more than one ISqsHelper.");
            if (isUniqueKeyAlreadyUsed) throw new Exception("UniqueKey must be distinct.");

            var regionEndpoint = RegionEndpoint.GetBySystemName(options.Region) ?? throw new ArgumentException("Region endpoint couldn't find.");

            services.AddSingleton<ISqsHelper>(_ => new SqsHelper(options, new BasicAWSCredentials(options.AccessKeyId, options.SecretAccessKey), regionEndpoint));
            services.AddSingleton<SqsHelperResolver>(sp => k =>
            {
                var services = sp.GetServices<ISqsHelper>();
                if (services.Count() == 1) return services.First();

                var service = sp.GetServices<ISqsHelper>().Where(q => ((SqsHelper)q).UniqueKey == k).First();
                return service is null
                    ? throw new KeyNotFoundException($"SQS service that related with queue ({options.QueueUrl}) couldn't found.")
                    : service;
            });
        }
    }
}