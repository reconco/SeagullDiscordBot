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
		public InteractionService InteractionService => _interactionService;

		public BotClient()
		{
			var socketConfig = new DiscordSocketConfig
			{
				MessageCacheSize = 100,
				GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.GuildMembers
				| GatewayIntents.GuildPresences | GatewayIntents.MessageContent | GatewayIntents.All

				//권한이 필요한 인텐트
				//GatewayIntents.GuildMembers: 서버 멤버 관련 이벤트
				//(디스코드 개발자 포럼에서 SERVER MEMBERS INTENT 권한 필요)
				//GatewayIntents.GuildPresences: 사용자 상태 변경 이벤트
				//(개발자포럼에서 PRESENCE INTENT 권한 필요)
				//GatewayIntents.MessageContent: 메시지 내용 접근
			};
			_client = new DiscordSocketClient(socketConfig);
			_commands = new CommandService();
			_interactionService = new InteractionService(_client.Rest);

			// 로깅 이벤트 등록
			_client.Log += Logger.LogAsync;
		}

		public async Task InitializeAsync()
		{
			Config.LoadSettings();

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