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
					.WithTitle($"🛠️ 서버별 봇 설정")
					.WithDescription($"서버: {Context.Guild.Name} (ID: {Context.Guild.Id})");

				// 현재 서버의 설정 가져오기
				var settings = Config.GetSettings(Context.Guild.Id);

				string authRoleName = settings.AutoRoleId.HasValue 
					? Context.Guild.GetRole(settings.AutoRoleId.Value)?.Name ?? "역할을 찾을 수 없음" 
					: "";

				string authChannelName = settings.AuthChannelId.HasValue 
					? Context.Guild.GetTextChannel(settings.AuthChannelId.Value)?.Name ?? "채널을 찾을 수 없음" 
					: "";

				embed.AddField("🎭 캡챠 인증 시스템", settings.AutoRoleEnabled ? "✅" : "❌");
				embed.AddField("🏷️ 캡챠 인증시 역할 ID", 
					settings.AutoRoleId.HasValue ? $"{authRoleName} ({settings.AutoRoleId})" : "❌ 설정되지 않음");
				embed.AddField("📢 인증 채널", 
					settings.AuthChannelId.HasValue ? $"{authChannelName} ({settings.AuthChannelId})" : "❌ 설정되지 않음");
				embed.AddField("⏱️ 스팸 감지 간격 (초)", settings.SpamDetectionInterval.ToString());

				await RespondAsync(embed: embed.Build(), ephemeral: true);
				Logger.Print($"서버 '{Context.Guild.Name}'({Context.Guild.Id})에서 '{Context.User.Username}'님이 봇 설정을 조회했습니다.");
			}
			catch (System.Exception ex)
			{
				Logger.Print($"서버 {Context.Guild.Id} 설정 조회 중 오류 발생: {ex.Message}", LogType.ERROR);
				await RespondAsync("설정을 조회하는 중 오류가 발생했습니다.", ephemeral: true);
			}
		}

		[SlashCommand("bot_info", "봇 정보 받기.")]
		public async Task BotInfoCommand()
		{
			string botInfo = 
				$"갈매기 봇 정보\n" +
				$"봇 버전: {System.Reflection.Assembly.GetExecutingAssembly().GetName().Version}\n" +
				$"현재 서버: {Context.Guild.Name} (ID: {Context.Guild.Id})";
				$"봇 버전: {System.Reflection.Assembly.GetExecutingAssembly().GetName().Version}" +
				$"매뉴얼 : https://reconco.github.io/SeagullDiscordBot/";

			await RespondAsync(botInfo, ephemeral: true);

			Logger.Print($"서버 '{Context.Guild.Name}'({Context.Guild.Id})에서 '{Context.User.Username}'님이 봇 정보를 요청했습니다.");
		}
	}
}
