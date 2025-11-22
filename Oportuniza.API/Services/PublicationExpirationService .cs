using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Oportuniza.Domain.Interfaces;
using Oportuniza.Domain.Enums;
using Oportuniza.Domain.Interfaces;

namespace Oportuniza.API.Services
{
    public class PublicationExpirationService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        public PublicationExpirationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var publicationRepository = scope.ServiceProvider.GetRequiredService<IPublicationRepository>();

                    var expiredPublications = await publicationRepository.GetAllAsync(
                        filter: p => p.ExpirationDate <= DateTime.UtcNow &&
                                     p.IsActive == PublicationAvailable.Enabled);

                    foreach (var pub in expiredPublications)
                    {
                        pub.IsActive = PublicationAvailable.Disabled;
                        pub.Expired = true;
                    }

                    await publicationRepository.UpdateRangeAsync(expiredPublications);
                }

                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }
}
