using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System.Linq;
using System.Threading.Tasks;
using SeagullDiscordBot.Services;
using System;

//AI코드 확인 완료(테스트 필요)
namespace SeagullDiscordBot.Modules	
{
	public partial class AuthorizationModule : InteractionModuleBase<SocketInteractionContext>
	{
		// 갈매기 역할 삭제 버튼 클릭 시 실행될 메서드 (authorization_off 3번)
		[ComponentInteraction("auth_off_remove_role_button")]
		public async Task AuthOffRemoveRoleButton()
		{
			await RespondAsync("갈매기 역할을 삭제합니다...\n완료 메시지가 나타날때까지 기다려주세요.", ephemeral: true);
			Logger.Print($"서버 '{Context.Guild.Name}'({Context.Guild.Id})에서 '{Context.User.Username}'님이 갈매기 역할 삭제 버튼을 클릭했습니다.");

			var guild = Context.Guild;

			try
			{
				// 현재 서버의 설정 가져오기
				var settings = Config.GetSettings(Context.Guild.Id);
				
				// 갈매기 역할 찾기
				var targetRole = guild.Roles.FirstOrDefault(r => r.Id == settings.AutoRoleId);
				if (targetRole == null)
				{
					await FollowupAsync("갈매기 역할이 설정되어 있지 않거나 이미 삭제되었습니다.", ephemeral: true);
					Logger.Print("갈매기 역할을 찾을 수 없습니다. 이미 삭제되었거나 설정되지 않았을 수 있습니다.", LogType.WARNING);
					return;
				}

				// 역할을 가진 사용자가 있는지 확인
				await guild.DownloadUsersAsync();
				var usersWithRole = guild.Users.Where(user => user.Roles.Any(r => r.Id == targetRole.Id)).ToList();
				
				if (usersWithRole.Count > 0)
				{
					await FollowupAsync($"⚠️ 경고: 아직 {usersWithRole.Count}명의 사용자가 갈매기 역할을 가지고 있습니다.\n먼저 2번 '사용자들 모두 갈매기 역할 제거'를 실행해주세요.", ephemeral: true);
					Logger.Print($"갈매기 역할 삭제 실패: {usersWithRole.Count}명의 사용자가 아직 이 역할을 가지고 있습니다.", LogType.WARNING);
					return;
				}

				string roleName = targetRole.Name;

				// 역할 삭제
				await targetRole.DeleteAsync();

				// 현재 서버의 Config에서 AutoRoleId 초기화
				Config.UpdateSetting(Context.Guild.Id, settings =>
				{
					settings.AutoRoleId = null;
				});

				await FollowupAsync($"'{roleName}' 역할이 성공적으로 삭제되었습니다.", ephemeral: true);
				Logger.Print($"서버 '{Context.Guild.Name}'({Context.Guild.Id})에서 '{Context.User.Username}'님이 '{roleName}' 역할을 삭제했습니다.");
			}
			catch (Exception ex)
			{
				Logger.Print($"서버 {Context.Guild.Id} 갈매기 역할 삭제 중 오류 발생: {ex.Message}", LogType.ERROR);
				await FollowupAsync($"역할 삭제 중 오류가 발생했습니다: {ex.Message}", ephemeral: true);
			}

			await FollowupAsync("갈매기 역할 삭제 완료!", ephemeral: true);
		}
	}
}