using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Discord.Commands;
using Bibs_Discord_dotNET.Commons.Structs;

namespace Bibs_Discord_dotNET.Preconditions
{
	public class CooldownAttribute : PreconditionAttribute
	{
		private readonly ConcurrentDictionary<CooldownInfo, DateTime> cooldowns =
			new ConcurrentDictionary<CooldownInfo, DateTime>();

		public CooldownAttribute(int seconds)
		{
			CooldownLength = TimeSpan.FromSeconds(seconds);
		}

		private TimeSpan CooldownLength { get; }

		/// Checks if a user is on cooldown
		public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command,
			IServiceProvider services)
		{
			CooldownInfo key = new CooldownInfo(context.User.Id, command.GetHashCode());

			if (cooldowns.TryGetValue(key, out DateTime endsAt))
			{
				TimeSpan difference = endsAt.Subtract(DateTime.Now);
				if (difference.Ticks > 0)
					return Task.FromResult(
						PreconditionResult.FromError(
							$"Please wait {difference:ss} seconds before trying again!"));

				DateTime time = DateTime.Now.Add(CooldownLength);
				cooldowns.TryUpdate(key, time, endsAt);
			}
			else
			{
				cooldowns.TryAdd(key, DateTime.Now.Add(CooldownLength));
			}

			return Task.FromResult(PreconditionResult.FromSuccess());
		}
	}
}
