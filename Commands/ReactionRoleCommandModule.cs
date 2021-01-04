using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Tavernkeep.Commands
{
	class ReactionRoleCommandModule : BaseCommandModule
	{
		private Dictionary<DiscordEmoji, DiscordRole> EmojiRolePairs = new Dictionary<DiscordEmoji, DiscordRole>();
		[Command("reactionroles")]
		public async Task SetupReactionRolesCommand(CommandContext ctx, DiscordMember invoker)
		{
			List<DiscordMessage> messagesForCleanup = new List<DiscordMessage>();
			var channelTagMessage = await ctx.RespondAsync("Please tag the channel you wish to have the reaction roles message be in.");
			messagesForCleanup.Add(channelTagMessage);

			var response = await ctx.Message.GetNextMessageAsync();
			while (response.TimedOut || response.Result.MentionedChannels.Count == 0) {
				messagesForCleanup.Add(await ctx.RespondAsync("Please make sure to tag the channel you want place the reaction roles message in."));
				response = await ctx.Message.GetNextMessageAsync();
			}

			messagesForCleanup.Add(response.Result);
			var ReactionRoleMessageChannel = response.Result.MentionedChannels.First();

			var builder = new DiscordEmbedBuilder()
					.WithTitle("Reaction Roles Setup")
					.WithColor(DiscordColor.DarkGreen)
					.WithDescription($"Please React to this message with the emoji you want to use, then follow the prompts before chosing another.\n" +
						$"React with {DiscordEmoji.FromName(ctx.Client, "stop_sign")} to finish.\n" +
						$"Chosen Reaction Roles:\n")
					.WithTimestamp(DateTime.Now);

			var ReactionRoleReactionSelectionMessage = await ctx.RespondAsync(null, false, builder.Build());
			messagesForCleanup.Add(ReactionRoleReactionSelectionMessage);
			var result = await ReactionRoleReactionSelectionMessage.WaitForReactionAsync(invoker);

			Dictionary<int, DiscordRole> OrderedRoles = new Dictionary<int, DiscordRole>();
			StringBuilder RolesStringBuilder = new StringBuilder();
			int lastRoleNumber = 1;

			foreach (var role in ctx.Guild.Roles) {
				RolesStringBuilder.Append($"`{lastRoleNumber}`: `{role.Value.Name}`\n");
				OrderedRoles.Add(lastRoleNumber++, role.Value);
			}
			var RolesString = RolesStringBuilder.ToString();
			while (!result.TimedOut) {
				if (result.Result.Emoji.Equals(DiscordEmoji.FromName(ctx.Client, "stop_sign"))){
					foreach (var m in messagesForCleanup) {
						await m.DeleteAsync("Tavernkeep Interaction Cleanup");
					}
					return;
				}
				if (!EmojiRolePairs.ContainsKey(result.Result.Emoji)) {
					var roleRequestEmbed = new DiscordEmbedBuilder()
						.WithTitle($"Role to assign for {result.Result.Emoji}")
						.WithColor(DiscordColor.DarkGreen)
						.WithDescription("Please respond with a role number from the following list:\n" + RolesString)
						.Build();
					var roleSelectionMessage = await ctx.RespondAsync(null, false, roleRequestEmbed);
					
				}
			}
		}

		private DiscordEmbed getUpdatedEmbedForReactionRoles(DiscordEmbed existing, DiscordEmoji emoji, DiscordRole role)
		{
			var builder = new DiscordEmbedBuilder(existing);
			builder.WithDescription(existing.Description + $"\n{emoji}: `{role.Name}`");
			return builder.Build();
		}
	}
}
