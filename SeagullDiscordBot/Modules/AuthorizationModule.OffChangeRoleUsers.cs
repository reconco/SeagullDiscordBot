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
		// 사용자들에게서 갈매기 역할 제거 버튼 클릭 시 실행될 메서드 (authorization_off 2번)
		[ComponentInteraction("auth_off_change_role_users_button")]
		public async Task AuthOffChangeRoleUsersButton()
		{
			await RespondAsync("모든 사용자들에게서 갈매기 역할을 제거합니다...\n완료 메시지가 나타날때까지 기다려주세요.(1초당 1명 처리)", ephemeral: true);
			Logger.Print($"서버 '{Context.Guild.Name}'({Context.Guild.Id})에서 '{Context.User.Username}'님이 사용자 역할 제거 버튼을 클릭했습니다.");

			var guild = Context.Guild;
			var requestedBy = Context.User.Username;

			await guild.DownloadUsersAsync();
			var allUsers = guild.Users;

			// 현재 서버의 설정 가져오기
			var settings = Config.GetSettings(Context.Guild.Id);
			var targetRole = guild.Roles.FirstOrDefault(r => r.Id == settings.AutoRoleId);
			
			if(targetRole == null)
			{
				await FollowupAsync("갈매기 역할이 설정되어 있지 않거나 이미 삭제되었습니다.", ephemeral: true);
				return;
			}

			// 갈매기 역할을 가진 사용자들만 필터링
			var usersWithRole = allUsers.Where(user =>	
				!user.IsBot && 
				user.Roles.Any(r => r.Id == targetRole.Id)
			).ToList();

			int successCount = 0;
			int errorCount = 0;
			int totalUsers = usersWithRole.Count;

			if (totalUsers == 0)
			{
				await FollowupAsync("갈매기 역할을 가진 사용자가 없습니다.", ephemeral: true);
				return;
			}

			try
			{
				int processedUsers = 0;

				await FollowupAsync($"총 {totalUsers}명의 사용자에게서 갈매기 역할을 제거합니다...", ephemeral: true);

				foreach (var user in usersWithRole)
				{
					processedUsers++;

					var result = await _roleService.RemoveRoleFromUserAsync(user, targetRole, requestedBy);

					if (result.Success)
					{
						successCount++;
						if (processedUsers % 50 == 0 || processedUsers == totalUsers)
						{
							Logger.Print($"역할 제거 진행 중: {processedUsers}/{totalUsers} 완료");
							await FollowupAsync($"진행 상황: {processedUsers}/{totalUsers} 사용자 처리 완료", ephemeral: true);
						}
					}
					else
					{
						errorCount++;
						Logger.Print($"사용자 '{user.Username}'에게서 역할 제거 실패: {result.ErrorMessage}", LogType.ERROR);
					}

					// API 제한을 피하기 위해 0.5초 대기
					await Task.Delay(500);
				}

				await FollowupAsync($"갈매기 역할 제거 완료: 총 {totalUsers}명 중 {successCount}명 성공, {errorCount}명 실패", ephemeral: true);
			}
			catch (Exception ex)
			{
				Logger.Print($"서버 {Context.Guild.Id} 사용자 역할 제거 중 오류 발생: {ex.Message}", LogType.ERROR);
				await FollowupAsync($"역할 제거 중 오류가 발생했습니다: {ex.Message}", ephemeral: true);
			}

			await FollowupAsync("모든 사용자들의 갈매기 역할 제거 완료!", ephemeral: true);
		}
	}
}