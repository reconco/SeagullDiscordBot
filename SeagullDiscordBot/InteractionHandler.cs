using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace SeagullDiscordBot
{
	public class InteractionHandler
	{
		private readonly DiscordSocketClient _client;
		private readonly InteractionService _interactionService;

		public InteractionHandler(DiscordSocketClient client, InteractionService interactionService)
		{
			_client = client;
			_interactionService = interactionService;
		}

		public async Task InitializeAsync()
		{
			// 이벤트 핸들러 등록
			_client.InteractionCreated += HandleInteraction;
			//_client.Ready += ReadyAsync;

			// 모듈 등록
			await _interactionService.AddModulesAsync(Assembly.GetEntryAssembly(), null);
		}

		private async Task ReadyAsync()
		{
			// 테스트 서버에만 명령어 등록 (글로벌 등록은 시간이 오래 걸림)
			// 글로벌 등록을 원하면 두 번째 매개변수를 null로 설정
			// await _interactionService.RegisterCommandsGloballyAsync();

			// 테스트 서버 ID로 변경 필요
			// ulong guildId = 123456789012345678; 
			// await _interactionService.RegisterCommandsToGuildAsync(guildId);

			Logger.Print("Successfully registered interaction commands");
		}

		public async Task ClearAllGlobalCommands()
		{
			try
			{
				// Clear global commands
				await _client.Rest.DeleteAllGlobalCommandsAsync();
				Logger.Print("All global interaction commands have been removed", LogType.NORMAL);

			}
			catch (Exception ex)
			{
				Logger.Print($"Error occurred while removing interaction commands: {ex.Message}", LogType.ERROR);
			}
		}

		public async Task RegisterCommandsToGuildAsync(ulong guildId = 0)
		{
			try
			{
				if (guildId == 0)
				{
					// Register commands globally
					await _interactionService.RegisterCommandsGloballyAsync();
					Logger.Print("Interaction commands registered globally", LogType.NORMAL);
				}
				else
				{
					// Register commands to specific guild
					await _interactionService.RegisterCommandsToGuildAsync(guildId);
					Logger.Print($"Interaction commands registered to server {guildId}", LogType.NORMAL);
				}
			}
			catch (Exception ex)
			{
				Logger.Print($"Error occurred while registering interaction commands: {ex.Message}", LogType.ERROR);
			}
		}

		private async Task HandleInteraction(SocketInteraction interaction)
		{
			try
			{
				var context = new SocketInteractionContext(_client, interaction);
				await _interactionService.ExecuteCommandAsync(context, null);
			}
			catch (Exception ex)
			{
				Logger.Print($"Interaction Error: {ex.Message}", LogType.ERROR);

				// 이미 응답된 인터랙션이 아니라면 오류 응답
				if (interaction.Type == InteractionType.ApplicationCommand)
				{
					await interaction.GetOriginalResponseAsync().ContinueWith(async (msg) =>
					{
						if (msg.IsFaulted)
							await interaction.RespondAsync("An error occurred while executing the command.", ephemeral: true);
					});
				}
			}
		}
	}
}