using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace SeagullDiscordBot.Modules
{
	// 새로운 사용자 환영 기능을 담당하는 모듈
	public class FirstSettingModule : InteractionModuleBase<SocketInteractionContext>
	{
		const string _description = "봇이 처음 서버에 입장하고 나서 서버 초기설정을 하는 모듈입니다.\n" +
			"1번부터 순서대로 눌러주세요." +
			"버튼을 누르고 완료 메시지가 나올때까지 기다린 다음 다음 버튼을 눌러주세요.\n" +
			"1. 인증된 사용자 역할 추가\n" +
			"2. 기존 사용자들 역할 변경\n" +
			"3. 기존 채널들 권한 수정(인증된 사용자만 사용 가능하도록)\n" +
			"4. 규칙 안내 및 인증 채널 추가";

		//봇이 처음 서버에 입장하고 나서 서버 초기설정을 하는 모듈
		//1. 인증된 사용자 역할 추가
		//2. 기존 사용자들 역할 변경
		//3. 기존 채널들 권한 수정(인증된 사용자만 사용 가능하도록)
		//4. 규칙 안내 및 인증 채널 추가

		// 기본 슬래시 명령어 정의
		[SlashCommand("first_setting", "봇이 처음 서버에 입장하고 나서 서버 초기설정을 하는 명령입니다.")]
		[RequireUserPermission(GuildPermission.Administrator)] // 관리자 권한이 있는 사용자만 사용 가능
		public async Task FirstSettingCommand()
		{    // 버튼 컴포넌트 생성
			var builder = new ComponentBuilder()
				.WithButton("역할 추가", "add_role_button", ButtonStyle.Primary, emote: new Emoji("🔑"))
				.WithButton("사용자 역할 변경", "change_role_button", ButtonStyle.Success, emote: new Emoji("👥"))
				.WithButton("채널 권한 수정", "modify_channel_button", ButtonStyle.Secondary, emote: new Emoji("🔒"))
				.WithButton("규칙 채널 추가", "add_rule_channel_button", ButtonStyle.Danger, emote: new Emoji("📜"));


			await RespondAsync(_description, components: builder.Build(), ephemeral: true);

			// 로그 남기기
			Logger.Print($"'{Context.User.Username}'님이 first_setting 명령어를 사용했습니다.");
		}

		// 역할 추가 버튼 클릭 시 실행될 메서드
		[ComponentInteraction("add_role_button")]
		public async Task AddRoleButton()
		{
			await RespondAsync("인증된 사용자 역할을 추가합니다...\n완료 메시지가 나타날때까지 기다려주세요.", ephemeral: true);
			// 역할 추가 기능 구현
			Logger.Print($"'{Context.User.Username}'님이 역할 추가 버튼을 클릭했습니다.");


			await RespondAsync("인증된 사용자 역할을 추가 완료!", ephemeral: true);
		}

		// 사용자 역할 변경 버튼 클릭 시 실행될 메서드
		[ComponentInteraction("change_role_button")]
		public async Task ChangeRoleButton()
		{
			await RespondAsync("기존 사용자들의 역할을 변경합니다...\n완료 메시지가 나타날때까지 기다려주세요.", ephemeral: true);
			// 사용자 역할 변경 기능 구현
			Logger.Print($"'{Context.User.Username}'님이 사용자 역할 변경 버튼을 클릭했습니다.");

			await RespondAsync("기존 사용자들의 역할 변경 완료!", ephemeral: true);
		}

		// 채널 권한 수정 버튼 클릭 시 실행될 메서드
		[ComponentInteraction("modify_channel_button")]
		public async Task ModifyChannelButton()
		{
			await RespondAsync("기존 채널들의 권한을 수정합니다...\n완료 메시지가 나타날때까지 기다려주세요.", ephemeral: true);
			// 채널 권한 수정 기능 구현
			Logger.Print($"'{Context.User.Username}'님이 채널 권한 수정 버튼을 클릭했습니다.");

			await RespondAsync("기존 채널들의 권환 변경 완료!", ephemeral: true);
		}

		// 규칙 채널 추가 버튼 클릭 시 실행될 메서드
		[ComponentInteraction("add_rule_channel_button")]
		public async Task AddRuleChannelButton()
		{
			await RespondAsync("규칙 안내 및 인증 채널을 추가합니다...\n완료 메시지가 나타날때까지 기다려주세요.", ephemeral: true);
			// 규칙 채널 추가 기능 구현
			Logger.Print($"'{Context.User.Username}'님이 규칙 채널 추가 버튼을 클릭했습니다.");

			await RespondAsync("규칙 안내 및 인증 채널을 추가 완료!", ephemeral: true);
		}
	}
}
