using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System.Threading.Tasks;
using SeagullDiscordBot.Services;

namespace SeagullDiscordBot.Modules
{
	public partial class AuthorizationModule : InteractionModuleBase<SocketInteractionContext>
	{
		//ChangeRoleMoulde.All에서 코드 가져옴
		[ComponentInteraction("change_role_users_button")]
		public async Task AddAllUserRoleButton()
		{
			await RespondAsync("모든 사용자들에게 역할을 추가합니다...\n완료 메시지가 나타날때까지 기다려주세요.(1초당 1명 처리)", ephemeral: true);
			Logger.Print($"서버 '{Context.Guild.Name}'({Context.Guild.Id})에서 '{Context.User.Username}'님이 사용자 역할 추가 버튼을 클릭했습니다.");

			var guild = Context.Guild;
			var requestedBy = Context.User.Username;

			await guild.DownloadUsersAsync();
			var allUsers = guild.Users;

			var targetUsers = allUsers.Where(user =>
				!user.IsBot &&
				!user.GuildPermissions.Administrator
			).ToList();

			int successCount = 0;
			int errorCount = 0;
			int excludedCount = allUsers.Count - targetUsers.Count;

			// 현재 서버의 설정 가져오기
			var settings = Config.GetSettings(Context.Guild.Id);
			var targetRole = guild.Roles.FirstOrDefault(r => r.Id == settings.AutoRoleId);
			
			if(targetRole == null)
			{
				await FollowupAsync("자동 역할이 설정되어 있지 않습니다. 1번 '갈매기 역할 추가'를 눌러 설정 후 다시 시도해주세요.", ephemeral: true);
				return;
			}

			try
			{
				int totalUsers = targetUsers.Count;
				int processedUsers = 0;

				await FollowupAsync($"총 {totalUsers}명의 사용자에게 역할을 추가합니다... (봇 및 관리자 {excludedCount}명 제외)", ephemeral: true);

				foreach (var user in targetUsers)
				{
					processedUsers++;

					if (user.Roles.Any(r => r.Id == targetRole.Id))
					{
						Logger.Print($"사용자 '{user.Username}'은(는) 이미 '{targetRole.Name}' 역할을 가지고 있습니다.");
						continue;
					}

					var result = await _roleService.AddRoleToUserAsync(user, targetRole, requestedBy);

					if (result.Success)
					{
						successCount++;
						if (processedUsers % 50 == 0 || processedUsers == totalUsers)
						{
							Logger.Print($"역할 추가 진행 중: {processedUsers}/{totalUsers} 완료 (관리자 및 봇 {excludedCount}명 제외)");
							await FollowupAsync($"진행 상황: {processedUsers}/{totalUsers} 사용자 처리 완료", ephemeral: true);
						}
					}
					else
					{
						errorCount++;
						Logger.Print($"사용자 '{user.Username}'에게 역할 추가 실패: {result.ErrorMessage}", LogType.ERROR);
					}

					await Task.Delay(500);
				}

				await FollowupAsync($"역할 추가 완료: 총 {totalUsers}명 중 {successCount}명 성공, {errorCount}명 실패\n(관리자 및 봇 {excludedCount}명 제외)", ephemeral: true);
			}
			catch (Exception ex)
			{
				Logger.Print($"서버 {Context.Guild.Id} 사용자 역할 추가 중 오류 발생: {ex.Message}", LogType.ERROR);
				await FollowupAsync($"역할 추가 중 오류가 발생했습니다: {ex.Message}", ephemeral: true);
			}

			await FollowupAsync("기존 사용자들의 역할 추가 완료!", ephemeral: true);
		}
	}
}
