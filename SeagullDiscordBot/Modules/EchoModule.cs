using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace SeagullDiscordBot.Modules
{
	// 메시지 에코 기능을 담당하는 모듈
	public class EchoModule : InteractionModuleBase<SocketInteractionContext>
	{
		// 에코 기능 활성화/비활성화를 위한 정적 변수
		private static bool _isEchoEnabled = false;
		private static ulong? _echoChannelId = null;

		// 에코 기능 토글 명령어
		[SlashCommand("toggle_echo", "메시지 따라하기 기능을 켜거나 끕니다.")]
		[RequireUserPermission(GuildPermission.Administrator)] // 관리자 권한이 있는 사용자만 사용 가능
		public async Task ToggleEchoCommand(
			[Summary("enable", "에코 기능을 활성화할지 여부")] bool enable,
			[Summary("channel", "에코할 채널 (선택사항, 없으면 현재 채널)")] ITextChannel? channel = null)
		{
			_isEchoEnabled = enable;
			_echoChannelId = channel?.Id ?? Context.Channel.Id;

			var channelName = channel?.Name ?? Context.Channel.Name;
			var status = enable ? "활성화" : "비활성화";

			await RespondAsync($"메시지 따라하기 기능이 '{channelName}' 채널에서 {status}되었습니다.", ephemeral: true);

			// 로그 남기기
			Logger.Print($"'{Context.User.Username}'님이 '{channelName}' 채널에서 에코 기능을 {status}했습니다.");
		}

		// 메시지 받았을 때 처리하는 정적 메서드
		public static async Task HandleMessageAsync(SocketMessage message)
		{
			// 에코 기능이 비활성화되어 있거나 봇 메시지인 경우 무시
			if (!_isEchoEnabled || message.Author.IsBot)
				return;

			// 지정된 채널이 아닌 경우 무시
			if (_echoChannelId.HasValue && message.Channel.Id != _echoChannelId.Value)
				return;

			// 빈 메시지 무시
			if (string.IsNullOrWhiteSpace(message.Content))
				return;

			// 슬래시 명령어는 무시
			if (message.Content.StartsWith("/"))
				return;

			try
			{
				// 원본 메시지 내용을 그대로 따라하기
				await message.Channel.SendMessageAsync($"🦜 {message.Content}");

				// 로그 남기기
				Logger.Print($"에코: '{message.Author.Username}'의 메시지 '{message.Content}'를 따라했습니다.");
			}
			catch (Exception ex)
			{
				Logger.Print($"메시지 에코 중 오류 발생: {ex.Message}", LogType.ERROR);
			}
		}

		// 현재 에코 상태 확인 명령어
		[SlashCommand("echo_status", "현재 메시지 따라하기 기능의 상태를 확인합니다.")]
		public async Task EchoStatusCommand()
		{
			if (!_isEchoEnabled)
			{
				await RespondAsync("메시지 따라하기 기능이 비활성화되어 있습니다.", ephemeral: true);
				return;
			}

			var channelName = "알 수 없음";
			if (_echoChannelId.HasValue)
			{
				var channel = Context.Guild.GetTextChannel(_echoChannelId.Value);
				channelName = channel?.Name ?? "알 수 없음";
			}

			await RespondAsync($"메시지 따라하기 기능이 '{channelName}' 채널에서 활성화되어 있습니다.", ephemeral: true);
		}
	}
}