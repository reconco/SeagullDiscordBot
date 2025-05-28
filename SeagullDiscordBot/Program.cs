using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Discord.Net;
using Discord.Interactions;
using System.Reflection;
using System.Threading.Tasks;

namespace SeagullDiscordBot
{
	public class Program
	{
		private static BotClient _botClient = new BotClient();
		private static CancellationTokenSource _cts = new CancellationTokenSource();
		private static ConsoleCommandHandler _consoleCommandHandler = new ConsoleCommandHandler();
		private static InteractionHandler _interactionHandler;

		public static InteractionHandler InteractionHandler
		{
			get { return _interactionHandler; }
			set { _interactionHandler = value; }
		}

		public static async Task Main()
		{
			// 콘솔 종료 이벤트 처리
			Console.CancelKeyPress += Console_CancelKeyPress;
			AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;

			// 로그 파일 생성
			Logger.CreateLogFile();

			// 봇 클라이언트 초기화
			await _botClient.InitializeAsync();

			// 이벤트 핸들러 초기화
			var eventHandler = new EventHandler(_botClient.Client);
			eventHandler.Initialize();

			// 인터랙션 핸들러 초기화
			_interactionHandler = new InteractionHandler(_botClient.Client, _botClient.InteractionService);
			await _interactionHandler.InitializeAsync();

			// 명령어 핸들러 초기화
			//var commandHandler = new CommandHandler(botClient.Client, botClient.Commands);
			//await commandHandler.InitializeAsync();

			// 콘솔 명령어 초기화
			Thread inputThread = new Thread(_consoleCommandHandler.WatingUserCommand);
			inputThread.Start();

			// 스레드 종료 대기
			inputThread.Join();

			// 무한 대기
			//await Task.Delay(-1);
		}

		private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
		{
			e.Cancel = true; // 기본 종료 방지
			_cts.Cancel();   // 취소 토큰 발행
		}

		private static void CurrentDomain_ProcessExit(object sender, EventArgs e)
		{
			// 프로세스 종료 시 호출
			_cts.Cancel();

			// 비동기 종료 작업을 동기적으로 실행
			ShutdownAsync().GetAwaiter().GetResult();
		}

		private static async Task ShutdownAsync()
		{
			if (_botClient != null)
			{
				await _botClient.StopAsync();
				Logger.Print("Finish this application.");
			}
		}
	}
}