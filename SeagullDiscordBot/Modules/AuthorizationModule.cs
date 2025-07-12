using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System.Threading.Tasks;
using SeagullDiscordBot.Services;

namespace SeagullDiscordBot.Modules
{ 
	public partial class AuthorizationModule : InteractionModuleBase<SocketInteractionContext>
	{
		const string _description = "인증된 사용자만 메세지를 보낼 수 있는 인증 시스템을 구축합니다.\n" +
			"1번부터 순서대로 눌러주세요." +
			"버튼을 누르고 완료 메시지가 나올때까지 기다린 다음 다음 버튼을 눌러주세요.\n" +
			"1. 갈매기 역할 추가\n" +
			"2. 기존 사용자들 모두 갈매기 역할로 변경\n" +
			"3. 기존 채널들 권한 수정(인증된 사용자만 메시지 전송 가능하도록)\n" +
			"4. 규칙 안내 및 인증 채널 추가";

		//봇이 처음 서버에 입장하고 나서 서버 초기설정을 하는 모듈
		//1. 인증된 사용자 역할 추가
		//2. 기존 사용자들 역할 변경
		//3. 기존 채널들 권한 수정(인증된 사용자만 메시지 전송 가능하도록)
		//4. 규칙 안내 및 인증 채널 추가

		// 기본 슬래시 명령어 정의
		[SlashCommand("authorization_on", "인증된 사용자만 메세지를 보낼 수 있는 인증 시스템을 구축합니다.")]
		[RequireUserPermission(GuildPermission.Administrator)] // 관리자 권한이 있는 사용자만 사용 가능
		public async Task FirstSettingCommand()
		{    // 버튼 컴포넌트 생성
			var builder = new ComponentBuilder()
				.WithButton("1. 갈매기 역할 추가", "add_role_button", ButtonStyle.Primary, emote: new Emoji("🔑"))
				.WithButton("2. 기존 사용자들 모두 갈매기 역할 추가", "change_role_users_button", ButtonStyle.Primary, emote: new Emoji("👥"))
				.WithButton("3. 채널 권한 수정", "modify_channel_button", ButtonStyle.Primary, emote: new Emoji("🔒"))
				.WithButton("4. 규칙 채널 추가", "add_rule_channel_button", ButtonStyle.Primary, emote: new Emoji("📜"));


			await RespondAsync(_description, components: builder.Build(), ephemeral: true);

			// 로그 남기기
			Logger.Print($"'{Context.User.Username}'님이 authorization_on 명령어를 사용했습니다.");
		}
	}
}
