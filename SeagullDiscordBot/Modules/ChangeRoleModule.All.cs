using Discord;
using Discord.Interactions;
using System.Threading.Tasks;
using System.Linq;
using System;
using SeagullDiscordBot.Services;
using Discord.WebSocket;

namespace SeagullDiscordBot.Modules
{
	public partial class ChangeRoleModule : InteractionModuleBase<SocketInteractionContext>
	{

	[SlashCommand("change_all_user_role", "서버의 모든 사용자의 역할을 변경합니다.(관리자와 봇제외)")]
	[RequireUserPermission(GuildPermission.Administrator)] // 관리자 권한이 있는 사용자만 사용 가능
	public async Task ChangeAllUserRoleCommand(
		[Summary("role", "변경할 역할의 이름")] string role)
	{
		// 역할이 서버에 존재하는지 확인
		var targetRole = _roleService.FindExistingRole(Context.Guild, role);
		if (targetRole == null)
		{
			await RespondAsync($"서버에서 '{role}' 역할을 찾을 수 없습니다. 역할 이름을 정확히 입력해주세요.", ephemeral: true);
			return;
		}

		var builder = new ComponentBuilder()
					.WithButton("모든 사용자의 역할 변경", $"change_all_user_role_button:{role}", ButtonStyle.Danger);

		await RespondAsync($"서버의 모든 사용자의 역할을 '{role}'으로 변경하겠습니까?\n※ 봇과 관리자는 제외됩니다.", components: builder.Build(), ephemeral: true);

		// 로그 남기기
		Logger.Print($"'{Context.User.Username}'님이 모든 사용자의 역할 변경을 요청했습니다. 대상 역할: '{role}'");
		}
	}

	public partial class ChangeRoleModule : InteractionModuleBase<SocketInteractionContext>
	{
		// 사용자 역할 변경 버튼 클릭 시 실행될 메서드
		[ComponentInteraction("change_all_user_role_button:*")]
		public async Task ChangeAllUserRoleButton(string roleName)
		{
			await RespondAsync("모든 사용자들의 역할을 변경합니다...\n완료 메시지가 나타날때까지 기다려주세요.(1초당 1명 처리)", ephemeral: true);
			// 사용자 역할 변경 기능 구현
			Logger.Print($"'{Context.User.Username}'님이 사용자 역할 변경 버튼을 클릭했습니다.");

			var guild = Context.Guild;
			var requestedBy = Context.User.Username;

			// 서버의 모든 사용자 가져오기
			await guild.DownloadUsersAsync();
			var allUsers = guild.Users;

			// 봇 사용자와 관리자 제외하기
			var targetUsers = allUsers.Where(user => 
				!user.IsBot && // 봇 제외
				!user.GuildPermissions.Administrator // 관리자 제외
			).ToList();

			int successCount = 0;
			int errorCount = 0;
			int excludedCount = allUsers.Count - targetUsers.Count;

			try
			{
				// 새 역할이 존재하는지 확인하고, 없으면 생성
				var newRole = _roleService.FindExistingRole(guild, roleName);
				if (newRole == null)
				{
					await FollowupAsync($"{roleName} 역할이 없어 진행할 수 없습니다.", ephemeral: true);
					return;
				}

				// 모든 사용자에게 역할 추가 진행
				int totalUsers = targetUsers.Count;
				int processedUsers = 0;

				await FollowupAsync($"총 {totalUsers}명의 사용자에게 역할을 추가합니다... (봇 및 관리자 {excludedCount}명 제외)", ephemeral: true);

				foreach (var user in targetUsers)
				{
					processedUsers++;

					// 사용자가 이미 새 역할을 가지고 있는지 확인
					if (user.Roles.Any(r => r.Id == newRole.Id))
					{
						Logger.Print($"사용자 '{user.Username}'은(는) 이미 '{roleName}' 역할을 가지고 있습니다.");
						continue;
					}

					// 사용자에게 역할 추가
					var result = await _roleService.AddRoleToUserAsync(user, newRole, requestedBy);

					if (result.Success)
					{
						successCount++;
						// 진행 상황 로깅 (50명마다 로그 출력)
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

					await Task.Delay(1000);
				}

				// 결과 메시지 전송
				await FollowupAsync($"역할 변경 완료: 총 {totalUsers}명 중 {successCount}명 성공, {errorCount}명 실패\n(관리자 및 봇 {excludedCount}명 제외됨)", ephemeral: true);
			}
			catch (Exception ex)
			{
				Logger.Print($"사용자 역할 변경 중 오류 발생: {ex.Message}", LogType.ERROR);
				await FollowupAsync($"역할 변경 중 오류가 발생했습니다: {ex.Message}", ephemeral: true);
			}

			await FollowupAsync("기존 사용자들의 역할 변경 완료!", ephemeral: true);
		}
	}
}

