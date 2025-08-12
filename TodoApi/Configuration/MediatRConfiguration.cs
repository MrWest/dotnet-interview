namespace TodoApi.Configuration
{
    /// <summary>
    /// Extension methods for configuring MediatR and related services.
    /// </summary>
    public static class MediatRConfiguration
    {
        /// <summary>
        /// Adds MediatR and related services to the dependency injection container.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddMediatRServices(this IServiceCollection services)
        {
            // Add MediatR
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

            // Add logging
            services.AddLogging();

            return services;
        }
    }
}