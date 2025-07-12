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
		[SlashCommand("add_all_user_role", "서버의 모든 사용자에게 역할을 추가합니다.(관리자와 봇제외)")]
		[RequireUserPermission(GuildPermission.Administrator)]
		public async Task ChangeAllUserRoleCommand([Summary("role", "추가할 역할의 이름")] string role)
		{
			var targetRole = _roleService.FindExistingRole(Context.Guild, role);
			if (targetRole == null)
			{
				await RespondAsync($"서버에서 '{role}' 역할을 찾을 수 없습니다. 역할 이름을 정확히 입력해주세요.", ephemeral: true);
				return;
			}

			var builder = new ComponentBuilder()
						.WithButton("모든 사용자의 역할 추가", $"add_all_user_role_button:{role}", ButtonStyle.Danger);

			await RespondAsync($"서버의 모든 사용자에게 역할 '{role}'을 추가하겠습니까?\n※ 봇과 관리자는 제외됩니다.", components: builder.Build(), ephemeral: true);
			Logger.Print($"'{Context.User.Username}'님이 모든 사용자의 역할 추가를 요청했습니다. 대상 역할: '{role}'");
		}

		[SlashCommand("remove_all_user_role", "서버의 모든 사용자에게서 역할을 제거합니다.(관리자와 봇제외)")]
		[RequireUserPermission(GuildPermission.Administrator)]
		public async Task RemoveAllUserRoleCommand([Summary("role", "제거할 역할의 이름")] string role)
		{
			var targetRole = _roleService.FindExistingRole(Context.Guild, role);
			if (targetRole == null)
			{
				await RespondAsync($"서버에서 '{role}' 역할을 찾을 수 없습니다. 역할 이름을 정확히 입력해주세요.", ephemeral: true);
				return;
			}

			var builder = new ComponentBuilder()
						.WithButton("모든 사용자의 역할 제거", $"remove_all_user_role_button:{role}", ButtonStyle.Danger);

			await RespondAsync($"서버의 모든 사용자에게서 '{role}' 역할을 제거하겠습니까?\n※ 봇과 관리자는 제외됩니다.", components: builder.Build(), ephemeral: true);
			Logger.Print($"'{Context.User.Username}'님이 모든 사용자의 역할 제거를 요청했습니다. 대상 역할: '{role}'");
		}

		[ComponentInteraction("add_all_user_role_button:*")]
		public async Task AddAllUserRoleButton(string roleName)
		{
			await RespondAsync("모든 사용자들에게 역할을 추가합니다...\n완료 메시지가 나타날때까지 기다려주세요.(1초당 1명 처리)", ephemeral: true);
			Logger.Print($"'{Context.User.Username}'님이 사용자 역할 추가 버튼을 클릭했습니다.");

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

			try
			{
				var newRole = _roleService.FindExistingRole(guild, roleName);
				if (newRole == null)
				{
					await FollowupAsync($"{roleName} 역할이 없어 진행할 수 없습니다.", ephemeral: true);
					return;
				}

				int totalUsers = targetUsers.Count;
				int processedUsers = 0;

				await FollowupAsync($"총 {totalUsers}명의 사용자에게 역할을 추가합니다... (봇 및 관리자 {excludedCount}명 제외)", ephemeral: true);

				foreach (var user in targetUsers)
				{
					processedUsers++;

					if (user.Roles.Any(r => r.Id == newRole.Id))
					{
						Logger.Print($"사용자 '{user.Username}'은(는) 이미 '{roleName}' 역할을 가지고 있습니다.");
						continue;
					}

					var result = await _roleService.AddRoleToUserAsync(user, newRole, requestedBy);

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

					await Task.Delay(1000);
				}

				await FollowupAsync($"역할 추가 완료: 총 {totalUsers}명 중 {successCount}명 성공, {errorCount}명 실패\n(관리자 및 봇 {excludedCount}명 제외됨)", ephemeral: true);
			}
			catch (Exception ex)
			{
				Logger.Print($"사용자 역할 추가 중 오류 발생: {ex.Message}", LogType.ERROR);
				await FollowupAsync($"역할 추가 중 오류가 발생했습니다: {ex.Message}", ephemeral: true);
			}

			await FollowupAsync("기존 사용자들의 역할 추가 완료!", ephemeral: true);
		}

		[ComponentInteraction("remove_all_user_role_button:*")]
		public async Task RemoveAllUserRoleButton(string roleName)
		{
			await RespondAsync("모든 사용자들에서 역할을 제거합니다...\n완료 메시지가 나타날때까지 기다려주세요.(1초당 1명 처리)", ephemeral: true);
			Logger.Print($"'`{Context.User.Username}'님이 모든 사용자 역할 제거 버튼을 클릭했습니다.`");

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
			try
			{
				var targetRole = _roleService.FindExistingRole(guild, roleName);
				if (targetRole == null)
				{
					await FollowupAsync($"{roleName} 역할이 없어 진행할 수 없습니다.`", ephemeral: true);
					return;
				}

				var usersWithRole = targetUsers.Where(user => user.Roles.Any(r => r.Id == targetRole.Id)).ToList();
				int totalUsers = usersWithRole.Count;
				int processedUsers = 0;

				if (totalUsers == 0)
				{
					await FollowupAsync($"'{roleName}' 역할을 가진 사용자가 없습니다. (봇 및 관리자 {excludedCount}명 제외)`", ephemeral: true);
					return;
				}

				await FollowupAsync($"총 {totalUsers}명의 사용자에게서 역할을 제거합니다... (봇 및 관리자 {excludedCount}명 제외)`", ephemeral: true);

				foreach (var user in usersWithRole)
				{
					processedUsers++;

					var result = await _roleService.RemoveRoleFromUserAsync(user, targetRole, requestedBy);
					if (result.Success)
					{
						successCount++;
						if (processedUsers % 50 == 0 || processedUsers == totalUsers)
						{
							Logger.Print($"역할 제거 진행 중: {processedUsers}/{totalUsers} 완료 (관리자 및 봇 {excludedCount}명 제외)`");
							await FollowupAsync($"진행 상황: {processedUsers}/{totalUsers} 사용자 처리 완료`", ephemeral: true);
						}
					}
					else
					{
						errorCount++;
						Logger.Print($"사용자 '{user.Username}'에게서 역할 제거 실패: {result.ErrorMessage}`", LogType.ERROR);
					}

					await Task.Delay(1000);
				}

				await FollowupAsync($"역할 제거 완료: 총 {totalUsers}명 중 {successCount}명 성공, {errorCount}명 실패\n(관리자 및 봇 {excludedCount}명 제외됨)`", ephemeral: true);
			}
			catch (Exception ex)
			{
				Logger.Print($"사용자 역할 제거 중 오류 발생: {ex.Message}`", LogType.ERROR);
				await FollowupAsync($"역할 제거 중 오류가 발생했습니다: {ex.Message}`", ephemeral: true);
			}

			await FollowupAsync($"모든 사용자들의 {roleName} 역할 제거 완료!", ephemeral: true);
		}
	}
}