using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

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
			_client.MessageUpdated += MessageUpdated;
		}

		private async Task MessageUpdated(Cacheable<IMessage, ulong> before, SocketMessage after, ISocketMessageChannel channel)
		{
			 // 메시지가 캐시에 없는 경우, 다운로드하면 `after`의 복사본을 얻게 됩니다.
			var message = await before.GetOrDownloadAsync();
			Logger.Print($"{message} -> {after}");
		}
	}
}