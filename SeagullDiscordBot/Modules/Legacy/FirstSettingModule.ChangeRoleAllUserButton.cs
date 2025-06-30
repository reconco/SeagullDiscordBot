using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System.Threading.Tasks;
using SeagullDiscordBot.Services;

namespace SeagullDiscordBot.Modules
{
	public partial class FirstSettingModule : InteractionModuleBase<SocketInteractionContext>
	{
		// 사용자 역할 변경 버튼 클릭 시 실행될 메서드
		[ComponentInteraction("change_role_button")]
		public async Task ChangeRoleButton()
		{
			await RespondAsync("기존 사용자들의 역할을 변경합니다...\n완료 메시지가 나타날때까지 기다려주세요.(1초당 1명 처리)", ephemeral: true);
			// 사용자 역할 변경 기능 구현
			Logger.Print($"'{Context.User.Username}'님이 사용자 역할 변경 버튼을 클릭했습니다.");

			var guild = Context.Guild;
			var requestedBy = Context.User.Username;

			// 서버의 모든 사용자 가져오기
			await guild.DownloadUsersAsync();
			var allUsers = guild.Users;

			// 봇 사용자 제외하기
			var humanUsers = allUsers.Where(user => !user.IsBot).ToList();


			// 변경할 역할 이름 (현재는 상수로 설정되어 있으나, 필요에 따라 변경 가능)
			const string oldRoleName = "everyone"; // 기본 역할 (모든 사용자가 가진 역할)
			const string newRoleName = "갈매기";    // 새롭게 추가할 역할

			int successCount = 0;
			int errorCount = 0;

			try
			{
				// 새 역할이 존재하는지 확인하고, 없으면 생성
				var newRole = _roleService.FindExistingRole(guild, newRoleName);
				if (newRole == null)
				{
					await FollowupAsync($"{newRoleName} 역할이 없어 진행할 수 없습니다.", ephemeral: true);
					return;
				}

				// 모든 사용자에게 역할 추가 진행
				int totalUsers = humanUsers.Count;
				int processedUsers = 0;

				await FollowupAsync($"총 {totalUsers}명의 사용자에게 역할을 추가합니다...", ephemeral: true);

				foreach (var user in humanUsers)
				{
					processedUsers++;

					// 사용자가 이미 새 역할을 가지고 있는지 확인
					if (user.Roles.Any(r => r.Id == newRole.Id))
					{
						Logger.Print($"사용자 '{user.Username}'은(는) 이미 '{newRoleName}' 역할을 가지고 있습니다.");
						continue;
					}

					// 사용자에게 역할 추가
					var result = await _roleService.AddRoleToUserAsync(user, newRole, requestedBy);

					if (result.Success)
					{
						successCount++;
						// 진행 상황 로깅 (30명마다 로그 출력)
						if (processedUsers % 30 == 0 || processedUsers == totalUsers)
						{
							Logger.Print($"역할 추가 진행 중: {processedUsers}/{totalUsers} 완료");
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
				await FollowupAsync($"역할 변경 완료: 총 {totalUsers}명 중 {successCount}명 성공, {errorCount}명 실패", ephemeral: true);
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
