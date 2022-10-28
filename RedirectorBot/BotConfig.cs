using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedirectorBot
{
	internal class BotConfig
	{
		public string BotToken { get; init; }
		public IDictionary<string, string[]> Routes { get; init; }
	}
}
