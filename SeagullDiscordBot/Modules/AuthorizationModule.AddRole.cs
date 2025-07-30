using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System.Threading.Tasks;
using SeagullDiscordBot.Services;

namespace SeagullDiscordBot.Modules
{
	public partial class AuthorizationModule : InteractionModuleBase<SocketInteractionContext>
	{
		const string RoleName = "갈매기";
		private readonly RoleService _roleService;

		// 생성자를 통해 RoleService 초기화
		public AuthorizationModule()
		{
			_roleService = new RoleService();
		}

		// 역할 추가 버튼 클릭 시 실행될 메서드
		[ComponentInteraction("add_role_button")]
		public async Task AddRoleButton()
		{
			await RespondAsync("갈매기 역할을 추가합니다...\n완료 메시지가 나타날때까지 기다려주세요.", ephemeral: true);
			// 역할 추가 기능 구현
			Logger.Print($"서버 '{Context.Guild.Name}'({Context.Guild.Id})에서 '{Context.User.Username}'님이 역할 추가 버튼을 클릭했습니다.");

			var guild = Context.Guild;

			try
			{
				// RoleService를 사용하여 역할 생성 (결과 객체 반환)
				RoleResult result = await _roleService.CreateRoleWithResultAsync(
					guild,
					RoleName,
					Context.User.Username,
					null,
					GuildPermissions.None,
					false,
					true
				);

				if (!result.Success)
				{
					await FollowupAsync($"역할 추가 중 오류가 발생했습니다: {result.ErrorMessage}", ephemeral: true);
					return;
				}

				// 성공 메시지 전송
				await FollowupAsync(result.Message, ephemeral: true);

				// 현재 서버의 설정 업데이트
				ulong roleId = result.Role.Id;
				Config.UpdateSetting(Context.Guild.Id, settings =>
				{
					settings.AutoRoleId = roleId;
				});

				await FollowupAsync("갈매기 역할을 추가 완료!", ephemeral: true);
			}
			catch (Exception ex)
			{
				Logger.Print($"서버 {Context.Guild.Id} 역할 추가 중 오류 발생: {ex.Message}", LogType.ERROR);
				await FollowupAsync("역할 추가 중 오류가 발생했습니다. 권한을 확인해주세요.", ephemeral: true);
			}
		}
	}
}
