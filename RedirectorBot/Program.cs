using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Systemd;
using Microsoft.Extensions.Hosting.WindowsServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;

namespace RedirectorBot
{
	internal class Program
	{
		private static IHostBuilder PrepareHost(string[] args)
		{
			var builder = Host.CreateDefaultBuilder(args)
				.ConfigureAppConfiguration(cfg =>
				{
					cfg
						.AddJsonFile("config.json", true, true)
						.AddEnvironmentVariables("RB_")
						.AddCommandLine(args)
					;
				})
				.ConfigureLogging(cfg =>
				{
					cfg
						.ClearProviders()
						.AddSimpleConsole()
					;
				})
				.ConfigureServices((ctx, srv) =>
				{
					srv
						.AddOptions<BotConfig>()
						.Bind(ctx.Configuration)
					;

					srv
						.AddHttpClient("telegram_bot_client")
						.AddTypedClient<ITelegramBotClient>((client, sp) =>
						{
							var options = sp.GetRequiredService<IOptions<BotConfig>>().Value;
							return new TelegramBotClient(options.BotToken, client);
						})
					;

					srv
						.AddHostedService<Startup>()
					;

					Startup.AddServices(srv);
				})
			;

			if (WindowsServiceHelpers.IsWindowsService())
			{
				return builder
					.UseWindowsService();
			}

			if (SystemdHelpers.IsSystemdService())
			{
				return builder
					.UseSystemd();
			}

			if (OperatingSystem.IsWindows() || OperatingSystem.IsMacOS() || OperatingSystem.IsLinux() || OperatingSystem.IsFreeBSD())
			{
				return builder
					.UseConsoleLifetime();
			}

			throw new PlatformNotSupportedException();
		}

		private static Task Main(string[] args) =>
			PrepareHost(args)
				.Build()
				.RunAsync();
	}
}