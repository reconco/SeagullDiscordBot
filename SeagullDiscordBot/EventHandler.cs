using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using SeagullDiscordBot.Modules;

namespace SeagullDiscordBot
{
	public class EventHandler
	{
		private readonly DiscordSocketClient _client;

		public EventHandler(DiscordSocketClient client)
		{
			_client = client;
		}

		public void Initialize()
		{
			//_client.MessageUpdated += MessageUpdated; //기존메시지가 수정되었을 때 호출되는 이벤트 등록
			_client.MessageReceived += MessageReceived; // 메시지 받기 이벤트 등록
			// _client.UserJoined += UserJoined; // 새 사용자 입장 이벤트 등록
		}

		//private async Task MessageUpdated(Cacheable<IMessage, ulong> before, SocketMessage after, ISocketMessageChannel channel)
		//{
		//	 // 메시지가 캐시에 없는 경우, 다운로드하면 `after`의 복사본을 얻게 됩니다.
		//	var message = await before.GetOrDownloadAsync();
		//	Logger.Print($"{message} -> {after}");
		//}

		// 새 메시지가 수신되었을 때 호출되는 이벤트 핸들러
		private async Task MessageReceived(SocketMessage message)
		{
			try
			{   
				// DM 메시지는 처리하지 않음
				if (message.Channel is IDMChannel)
				{
					Logger.Print($"DM 메시지 무시: {message.Author.Username}");
					return;
				}

				// SpamModule의 메시지 처리 메서드 호출
				await SpamModule.HandleMessageAsync(message);
			}
			catch (Exception ex)
			{
				// 오류 발생 시 로그 기록
				Logger.Print($"메시지 처리 중 오류 발생: {ex.Message}", LogType.ERROR);
			}
		}

		//// 새 사용자가 서버에 입장했을 때 호출되는 이벤트 핸들러
		//private async Task UserJoined(SocketGuildUser user)
		//{	
		//	Console.WriteLine($"'{user.Username}'님이 '{user.Guild.Name}' 서버에 입장했습니다.");

		//	try
		//	{
		//		// WelcomeModule의 정적 메서드를 호출하여 환영 메시지 전송
		//		await WelcomeModule.SendWelcomeMessageAsync(user);
		//	}
		//	catch (Exception ex)
		//	{
		//		// 오류 발생 시 로그 기록
		//		Logger.Print($"사용자 환영 처리 중 오류 발생: {ex.Message}", LogType.ERROR);
		//	}
		//}
	}
}