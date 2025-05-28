using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Discord.Interactions;
using System.Threading.Tasks;

namespace SeagullDiscordBot
{
	public class BotClient
	{
		private readonly DiscordSocketClient _client;
		private readonly CommandService _commands;
		private readonly InteractionService _interactionService;
		public DiscordSocketClient Client => _client;
		public CommandService Commands => _commands;

		public BotClient()
		{
			var socketConfig = new DiscordSocketConfig { MessageCacheSize = 100 };
			_client = new DiscordSocketClient(socketConfig);
			_commands = new CommandService();
			_interactionService = new InteractionService(_client.Rest);

			// 로깅 이벤트 등록
			_client.Log += Logger.LogAsync;
		}

		public async Task InitializeAsync()
		{
			// 토큰 설정 및 로그인
			string token = Config.GetToken();
			if (string.IsNullOrEmpty(token))
				return;

			await _client.LoginAsync(TokenType.Bot, token);
			await _client.StartAsync();
		}

		public async Task StopAsync()
		{
			await _client.StopAsync();
			await _client.LogoutAsync();
			await _client.DisposeAsync();
		}
	}
}