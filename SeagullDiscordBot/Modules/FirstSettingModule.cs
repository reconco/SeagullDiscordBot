using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System.Threading.Tasks;
using SeagullDiscordBot.Services;

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
				.WithButton("1. 역할 추가", "add_role_button", ButtonStyle.Primary, emote: new Emoji("🔑"))
				.WithButton("2. 사용자 역할 변경", "change_role_button", ButtonStyle.Primary, emote: new Emoji("👥"))
				.WithButton("3. 채널 권한 수정", "modify_channel_button", ButtonStyle.Primary, emote: new Emoji("🔒"))
				.WithButton("4. 규칙 채널 추가", "add_rule_channel_button", ButtonStyle.Primary, emote: new Emoji("📜"));


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


			await FollowupAsync("인증된 사용자 역할을 추가 완료!", ephemeral: true);
		}

		// 사용자 역할 변경 버튼 클릭 시 실행될 메서드
		[ComponentInteraction("change_role_button")]
		public async Task ChangeRoleButton()
		{
			await RespondAsync("기존 사용자들의 역할을 변경합니다...\n완료 메시지가 나타날때까지 기다려주세요.", ephemeral: true);
			// 사용자 역할 변경 기능 구현
			Logger.Print($"'{Context.User.Username}'님이 사용자 역할 변경 버튼을 클릭했습니다.");

			await FollowupAsync("기존 사용자들의 역할 변경 완료!", ephemeral: true);
		}

		// 채널 권한 수정 버튼 클릭 시 실행될 메서드
		[ComponentInteraction("modify_channel_button")]
		public async Task ModifyChannelButton()
		{
			await RespondAsync("기존 채널들의 권한을 수정합니다...\n완료 메시지가 나타날때까지 기다려주세요.", ephemeral: true);
			// 채널 권한 수정 기능 구현
			Logger.Print($"'{Context.User.Username}'님이 채널 권한 수정 버튼을 클릭했습니다.");

			await FollowupAsync("기존 채널들의 권환 변경 완료!", ephemeral: true);
		}

		// 규칙 채널 추가 버튼 클릭 시 실행될 메서드
		[ComponentInteraction("add_rule_channel_button")]
		public async Task AddRuleChannelButton()
		{
			await RespondAsync("규칙 안내 및 인증 채널을 추가합니다...\n완료 메시지가 나타날때까지 기다려주세요.", ephemeral: true);
			// 규칙 채널 추가 기능 구현
			Logger.Print($"'{Context.User.Username}'님이 규칙 채널 추가 버튼을 클릭했습니다.");

			// 채널 서비스를 통해 텍스트 채널 생성 (결과 객체 반환)
			ChannelService channelService = new ChannelService();
			var result = await channelService.CreateTextChannelWithResultAsync(
				Context.Guild,
				"서버 규칙 & 인증",
				Context.User.Username,
				"갈매기"
			);

			if (result.Success)
			{
				// 성공 메시지 전송
				await FollowupAsync(result.Message, ephemeral: true);

				await Task.Delay(2000); // 2초 대기

				// 생성된 채널에 규칙 메시지 전송
				var embed = new EmbedBuilder()
					.WithColor(Color.Blue)
					.WithTitle("📜 서버 규칙 안내")
					.WithDescription("이 채널은 서버 규칙을 안내하고 사용자 인증을 위한 채널입니다.\n아래 규칙을 확인하고 인증 버튼을 눌러주세요.")
					.AddField("1", "문의 채널에 잡담 금지")
					.AddField("2", "문의 답변에 말로 대답하지 말고 이모지 사용")
					.WithFooter(footer => footer.Text = "위 내용에 동의하면 밑의 빨간색 버튼을 눌러주세요. 인증 후에 다른 채널들을 이용할 수 있습니다.")
					.WithCurrentTimestamp()
					.Build();

				// 인증 버튼 추가
				var button = new ComponentBuilder()
					.WithButton("인증하기1", "non_verify_user_button0", ButtonStyle.Success, emote: new Emoji("✅"))
					.WithButton("인증하기2", "non_verify_user_button1", ButtonStyle.Primary, emote: new Emoji("✅"))
					.WithButton("인증하기3", "verify_user_button", ButtonStyle.Danger, emote: new Emoji("✅"))
					.WithButton("인증하기4", "non_verify_user_button2", ButtonStyle.Secondary, emote: new Emoji("✅"));

				// 생성된 채널에 메시지 전송

				await Task.Delay(1000); // 1초 대기
				await result.Channel.SendMessageAsync(embed: embed);
				await result.Channel.SendMessageAsync("아래 버튼을 클릭하여 인증을 완료하세요:", components: button.Build());
				//await result.Channel.SendMessageAsync("Test",embed: embed, components: button.Build());

				await FollowupAsync(result.Message, ephemeral: true);

			}
			else
			{
				// 오류 발생 시 처리
				await FollowupAsync($"채널 생성 중 오류가 발생했습니다: {result.ErrorMessage}", ephemeral: true);
			}

			await FollowupAsync("규칙 안내 및 인증 채널을 추가 완료!", ephemeral: true);
		}

		// 채널 권한 수정 버튼 클릭 시 실행될 메서드
		[ComponentInteraction("verify_user_button")]
		public async Task VerifyUserButton()
		{
			// 채널 권한 수정 기능 구현
			Logger.Print($"'{Context.User.Username}'님이 인증 버튼을 클릭했습니다.");

			await RespondAsync("인증 완료", ephemeral: true);
		}


		// 채널 권한 수정 버튼 클릭 시 실행될 메서드
		[ComponentInteraction("non_verify_user_button0")]
		public async Task NonVerifyUserButton()
		{
			// 채널 권한 수정 기능 구현
			Logger.Print($"'{Context.User.Username}'님이 비인증 버튼을 클릭했습니다.");

			await RespondAsync("인증 불가", ephemeral: true);
		}
		[ComponentInteraction("non_verify_user_button1")]
		public async Task NonVerifyUserButton1()
		{
			// 채널 권한 수정 기능 구현
			Logger.Print($"'{Context.User.Username}'님이 비인증 버튼을 클릭했습니다.");

			await RespondAsync("인증 불가", ephemeral: true);
		}
		[ComponentInteraction("non_verify_user_button2")]
		public async Task NonVerifyUserButton2()
		{
			// 채널 권한 수정 기능 구현
			Logger.Print($"'{Context.User.Username}'님이 비인증 버튼을 클릭했습니다.");

			await RespondAsync("인증 불가", ephemeral: true);
		}
	}
}
