using Discord;
using Discord.Interactions;
using System.Threading.Tasks;
using System.Reflection;

namespace SeagullDiscordBot.Modules
{
	// InteractionModuleBase를 상속받아 슬래시 명령어 모듈 생성
	public class DebugModule : InteractionModuleBase<SocketInteractionContext>
	{
		// 기본 슬래시 명령어 정의
		[SlashCommand("get_user_list", "유저 목록 받기.")]
		[RequireUserPermission(GuildPermission.Administrator)] // 관리자 권한이 있는 사용자만 사용 가능
		public async Task DebugCommand()
		{
			await RespondAsync("유저 목록 받기.", ephemeral: true);
			Logger.Print("get_user_list by " + Context.User.Username);

			var list = Context.Guild.Users.ToList();
			Logger.Print($"총 유저 수: {list.Count}");
			for (int i = 0; i < list.Count; i++)
			{
				Logger.Print(list[i].DisplayName);
			}
		}

		[SlashCommand("bot_info", "봇 정보 받기.")]
		public async Task BotInfoCommand()
		{
			await RespondAsync("갈매기 봇!", ephemeral: true);
			string botInfo = 
				$"갈매기 봇 정보\n" +
				$"봇 버전: {System.Reflection.Assembly.GetExecutingAssembly().GetName().Version}";

			Logger.Print($"'{Context.User.Username}'사용자가 봇 정보를 요청");
		}
	}
}
