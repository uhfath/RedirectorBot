using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace RedirectorBot
{
	internal class Startup : BackgroundService
	{
		private readonly IServiceScopeFactory _serviceScopeFactory;
		private readonly ILogger<Startup> _logger;

		public Startup(
			IServiceScopeFactory serviceScopeFactory,
			ILogger<Startup> logger)
		{
			this._serviceScopeFactory = serviceScopeFactory;
			this._logger = logger;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			var options = new ReceiverOptions
			{
				AllowedUpdates = new[] { UpdateType.Message },
			};

			while (!stoppingToken.IsCancellationRequested)
			{
				try
				{
					await using var scope = _serviceScopeFactory.CreateAsyncScope();
					var client = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();
					var handler = scope.ServiceProvider.GetRequiredService<UpdateHandler>();

					await client.ReceiveAsync(handler, options, stoppingToken);
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Polling error");
					await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
				}
			}
		}

		public static IServiceCollection AddServices(IServiceCollection services)
		{
			services
				.AddScoped<UpdateHandler>()
			;

			return services;
		}
	}
}
