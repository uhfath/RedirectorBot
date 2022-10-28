using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace RedirectorBot
{
	internal class UpdateHandler : IUpdateHandler
	{
		private readonly IOptionsSnapshot<BotConfig> _botConfig;
		private readonly ILogger<UpdateHandler> _logger;

		public UpdateHandler(
			IOptionsSnapshot<BotConfig> botConfig,
			ILogger<UpdateHandler> logger)
		{
			this._botConfig = botConfig;
			this._logger = logger;
		}

		public Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
		{
			var source = update.Message.MessageId;
			if (_botConfig.Value.Routes.TryGetValue(source.ToString(), out var destinations))
			{
				var tasks = destinations
					.Select(d => botClient.ForwardMessageAsync(d, update.Message.Chat.Id, update.Message.MessageId, cancellationToken: cancellationToken))
					.ToArray()
				;

				return Task.WhenAll(tasks);
			}

			return Task.CompletedTask;
		}

		public Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
		{
			var message = exception switch
			{
				ApiRequestException apiRequestException => $"Telegram API Error:{Environment.NewLine}[{apiRequestException.ErrorCode}]{Environment.NewLine}{apiRequestException.Message}",
				_ => exception.ToString()
			};

			_logger.LogError("Handler error: {message}", message);

			if (exception is RequestException)
			{
				return Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
			}

			return Task.CompletedTask;
		}
	}
}
