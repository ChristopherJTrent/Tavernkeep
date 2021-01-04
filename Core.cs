using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Tavernkeep
{
	class Core
	{
		static void Main(string[] args)
		{
			mainAsync().GetAwaiter().GetResult();
		}
		public static async Task mainAsync()
		{
			using FileStream stream = File.OpenRead("./configuration.json");
			Dictionary<string, string> Config = await JsonSerializer.DeserializeAsync<Dictionary<String, String>>(stream);
			var discord = new DiscordClient(new DiscordConfiguration() {
				Token = Config["AccessToken"],
				TokenType = TokenType.Bot
			});
			var CommandsNextObj = discord.UseCommandsNext(new CommandsNextConfiguration() {
				StringPrefixes = new[] { Config["CommandPrefix"] }
			});

			CommandsNextObj.RegisterCommands<Commands.ReactionRoleCommandModule>();

			var InteractivityObj = discord.UseInteractivity(new InteractivityConfiguration() {
				PollBehaviour = PollBehaviour.DeleteEmojis,
				Timeout = TimeSpan.FromSeconds(60)
			});
			await discord.ConnectAsync();
			await Task.Delay(-1);
		}
	}
}
