using Discord;
using Discord.Interactions;
using System.Threading.Tasks;
using System.Reflection;

namespace SeagullDiscordBot.Modules
{
	// InteractionModuleBase를 상속받아 슬래시 명령어 모듈 생성
	public class DebugModule : InteractionModuleBase<SocketInteractionContext>
	{
		[SlashCommand("show_config", "현재 봇 설정을 표시합니다.")]
		[RequireUserPermission(GuildPermission.Administrator)]
		public async Task ShowConfigCommand()
		{
			try
			{
				var embed = new EmbedBuilder()
					.WithTitle("🛠️ 봇 현재 설정 (텍스트 파일 기반)");

				var settings = Config.Settings;

				string authRoleName = settings.AutoRoleId.HasValue 
					? Context.Guild.GetRole(settings.AutoRoleId.Value)?.Name ?? "" 
					: "";

				string authChannelName = settings.AuthChannelId.HasValue 
					? Context.Guild.GetTextChannel(settings.AuthChannelId.Value)?.Name ?? "" 
					: "";

				embed.AddField("🎭 자동 역할 부여", settings.AutoRoleEnabled ? "✅" : "❌");
				embed.AddField("🏷️ 자동 역할 ID", $"{authRoleName}({settings.AutoRoleId?.ToString()})" ?? "❌");
				embed.AddField("🏷️ 자동 역할 부여 채널", $"{authChannelName}({settings.AuthChannelId?.ToString()})" ?? "❌");
				embed.AddField("⏱️ 도배 감지 간격 (초)", settings.SpamDetectionInterval.ToString());

				await RespondAsync(embed: embed.Build(), ephemeral: true);
				Logger.Print($"'{Context.User.Username}'님이 봇 설정을 조회했습니다.");
			}
			catch (System.Exception ex)
			{
				Logger.Print($"설정 조회 중 오류 발생: {ex.Message}", LogType.ERROR);
				await RespondAsync("설정을 조회하는 중 오류가 발생했습니다.", ephemeral: true);
			}
		}

		[SlashCommand("bot_info", "봇 정보 받기.")]
		public async Task BotInfoCommand()
		{
			string botInfo = 
				$"갈매기 봇 정보\n" +
				$"봇 버전: {System.Reflection.Assembly.GetExecutingAssembly().GetName().Version}";

			await RespondAsync(botInfo, ephemeral: true);

			Logger.Print($"'{Context.User.Username}'사용자가 봇 정보를 요청");
		}
	}
}
