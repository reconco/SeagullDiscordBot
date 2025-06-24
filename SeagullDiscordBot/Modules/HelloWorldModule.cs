using Discord;
using Discord.Interactions;
using System.Threading.Tasks;

namespace SeagullDiscordBot.Modules
{
	// InteractionModuleBase를 상속받아 슬래시 명령어 모듈 생성
	public class HelloWorldModule : InteractionModuleBase<SocketInteractionContext>
	{
		// 기본 슬래시 명령어 정의
		[SlashCommand("hello", "Hello, World! 메시지를 출력합니다.")]
		[RequireUserPermission(GuildPermission.Administrator)] // 관리자 권한이 있는 사용자만 사용 가능
		public async Task HelloCommand()
		{
			// 응답으로 "Hello, World!" 메시지 전송
			await RespondAsync("Hello, World!");

			// 로그 남기기
			Logger.Print($"'{Context.User.Username}'님이 hello 명령어를 사용했습니다.");
		}

		// 추가 명령어: 이름을 지정하여 인사하는 명령어
		[SlashCommand("greet", "지정한 이름으로 인사합니다.")]
		public async Task GreetCommand(
			[Summary("name", "인사할 대상의 이름")] string name = "World")
		{
			// 입력받은 이름으로 인사 메시지 전송
			await RespondAsync($"Hello, {name}!");

			// 로그 남기기
			Logger.Print($"'{Context.User.Username}'님이 greet 명령어로 '{name}'에게 인사했습니다.");
		}

		// 추가 명령어: 이름을 지정하여 인사하는 명령어
		[SlashCommand("hello_msg", "인사 메시지.")]
		public async Task HelloMsgCommand()
		{
			await RespondAsync($"hello_msg");

			await Context.Channel.SendMessageAsync($"Hello! hello_msg");

			// 로그 남기기
			Logger.Print($"'{Context.User.Username}'님이 hello_msg 명령어로 인사했습니다.");
		}
	}
}