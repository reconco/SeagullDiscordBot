using Discord;
using Discord.Interactions;
using System.Threading.Tasks;
using System.Linq;
using System;
using SeagullDiscordBot.Services;
using Discord.WebSocket;

namespace SeagullDiscordBot.Modules
{
	// InteractionModuleBase를 상속받아 슬래시 명령어 모듈 생성
	public partial class ChangeRoleModule : InteractionModuleBase<SocketInteractionContext>
	{
		private readonly RoleService _roleService;

		// 생성자를 통해 RoleService 초기화
		public ChangeRoleModule()
		{
			_roleService = new RoleService();
		}

		[SlashCommand("add_user_role", "특정 사용자의 역할을 추가합니다.")]
		[RequireUserPermission(GuildPermission.Administrator)] // 관리자 권한이 있는 사용자만 사용 가능
		public async Task AddUserRoleCommand(
			[Summary(description: "역할을 추가할 사용자의 이름")] string username,
			[Summary("role", "추가할 역할의 이름")] string role)
		{
			// 역할이 서버에 존재하는지 확인
			var targetRole = _roleService.FindExistingRole(Context.Guild, role);
			if (targetRole == null)
			{
				await RespondAsync($"서버에서 '{role}' 역할을 찾을 수 없습니다. 역할 이름을 정확히 입력해주세요.", ephemeral: true);
				return;
			}

			// 사용자 이름으로 일치하는 사용자를 서버에서 찾기
			var matchingUsers = Context.Guild.Users
				.Where(u => u.DisplayName.Equals(username, StringComparison.OrdinalIgnoreCase))
				.ToList();

			if (!matchingUsers.Any())
			{
				await RespondAsync($"서버에서 '{username}' 사용자를 찾을 수 없습니다.", ephemeral: true);
				return;
			}

			// 사용자가 한 명만 있는 경우 바로 확인 버튼 표시
			if (matchingUsers.Count == 1)
			{
				var user = matchingUsers.First();
				var builder = new ComponentBuilder()
							.WithButton("역할 추가", $"add_role_confirm_button:{user.Id}:{role}", ButtonStyle.Danger);

				await RespondAsync($"'{user.DisplayName}({user.Username}, {user.Id})' 사용자의 역할에 '{role}'을 추가하시겠습니까?", components: builder.Build(), ephemeral: true);

				// 로그 남기기
				Logger.Print($"'{Context.User.Username}'님이 '{user.Username}' 사용자의 역할 추가를 요청했습니다. 대상 역할: '{role}'");
			}
			// 사용자가 여러 명인 경우 선택 메뉴 표시
			else
			{
				var selectBuilder = new SelectMenuBuilder()
					.WithPlaceholder("역할 추가할 사용자를 선택하세요")
					.WithCustomId($"user_selection_menu:{role}")
					.WithMinValues(1)
					.WithMaxValues(1);

				// 최대 25개까지 옵션 추가 가능 (Discord API 제한)
				foreach (var user in matchingUsers.Take(25))
				{
					selectBuilder.AddOption(
						$"{user.DisplayName}({user.Username}, {user.Id})",
						user.Id.ToString(),
						$"서버 가입일: {user.JoinedAt}"
					);
				}

				var builder = new ComponentBuilder()
					.WithSelectMenu(selectBuilder);

				await RespondAsync($"'{username}' 이름을 가진 사용자가 여러 명 있습니다. 역할을 추가할 사용자를 선택하세요:", components: builder.Build(), ephemeral: true);

				// 로그 남기기
				Logger.Print($"'{Context.User.Username}'님이 '{username}' 사용자 검색 시 여러 사용자가 발견되어 선택 메뉴를 표시했습니다.");
			 }
		}

		[SlashCommand("remove_user_role", "특정 사용자의 역할을 제거합니다.")]
		[RequireUserPermission(GuildPermission.Administrator)] // 관리자 권한이 있는 사용자만 사용 가능
		public async Task RemoveUserRoleCommand(
			[Summary(description: "역할을 제거할 사용자의 이름")] string username,
			[Summary("role", "제거할 역할의 이름")] string role)
		{
			// 역할이 서버에 존재하는지 확인
			var targetRole = _roleService.FindExistingRole(Context.Guild, role);
			if (targetRole == null)
			{
				await RespondAsync($"서버에서 '{role}' 역할을 찾을 수 없습니다. 역할 이름을 정확히 입력해주세요.", ephemeral: true);
				return;
			}

			// 사용자 이름으로 일치하는 사용자를 서버에서 찾기
			var matchingUsers = Context.Guild.Users
				.Where(u => u.DisplayName.Equals(username, StringComparison.OrdinalIgnoreCase))
				.ToList();

			if (!matchingUsers.Any())
			{
				await RespondAsync($"서버에서 '{username}' 사용자를 찾을 수 없습니다.", ephemeral: true);
				return;
			}

			// 사용자가 한 명만 있는 경우 바로 확인 버튼 표시
			if (matchingUsers.Count == 1)
			{
				var user = matchingUsers.First();
				var builder = new ComponentBuilder()
							.WithButton("역할 제거", $"remove_role_confirm_button:{user.Id}:{role}", ButtonStyle.Danger);

				await RespondAsync($"'{user.DisplayName}({user.Username}, {user.Id})' 사용자의 '{role}' 역할을 제거하시겠습니까?", components: builder.Build(), ephemeral: true);

				// 로그 남기기
				Logger.Print($"'{Context.User.Username}'님이 '{user.Username}' 사용자의 역할 제거를 요청했습니다. 대상 역할: '{role}'");
			}
			// 사용자가 여러 명인 경우 선택 메뉴 표시
			else
			{
				var selectBuilder = new SelectMenuBuilder()
					.WithPlaceholder("역할 제거할 사용자를 선택하세요")
					.WithCustomId($"remove_role_user_selection_menu:{role}")
					.WithMinValues(1)
					.WithMaxValues(1);

				// 최대 25개까지 옵션 추가 가능 (Discord API 제한)
				foreach (var user in matchingUsers.Take(25))
				{
					selectBuilder.AddOption(
						$"{user.DisplayName}({user.Username}, {user.Id})",
						user.Id.ToString(),
						$"서버 가입일: {user.JoinedAt}"
					);
				}

				var builder = new ComponentBuilder()
					.WithSelectMenu(selectBuilder);

				await RespondAsync($"'{username}' 이름을 가진 사용자가 여러 명 있습니다. 역할을 제거할 사용자를 선택하세요:", components: builder.Build(), ephemeral: true);

				// 로그 남기기
				Logger.Print($"'{Context.User.Username}'님이 '{username}' 사용자 검색 시 여러 사용자가 발견되어 역할 제거 선택 메뉴를 표시했습니다.");
			}
		}

		// 사용자 ID와 역할 이름으로 사용자와 역할을 검증하고 가져오는 공통 함수
		private async Task<(SocketGuildUser user, IRole role, bool isValid)> ValidateUserAndRoleAsync(string userId, string roleName)
		{
			// 사용자 ID 파싱
			if (!ulong.TryParse(userId, out var userIdParsed))
			{
				await FollowupAsync("잘못된 사용자 ID입니다.", ephemeral: true);
				return (null, null, false);
			}

			// 사용자 찾기
			var targetUser = Context.Guild.GetUser(userIdParsed);
			if (targetUser == null)
			{
				await FollowupAsync("해당 사용자를 찾을 수 없습니다.", ephemeral: true);
				return (null, null, false);
			}

			// 역할 존재 재확인
			var targetRole = _roleService.FindExistingRole(Context.Guild, roleName);
			if (targetRole == null)
			{
				await FollowupAsync($"'{roleName}' 역할을 찾을 수 없습니다.", ephemeral: true);
				return (null, null, false);
			}

			return (targetUser, targetRole, true);
		}

		// 역할 추가 확인 버튼 클릭 시 실행될 메서드
		[ComponentInteraction("add_role_confirm_button:*:*")]
		public async Task AddRoleConfirmButton(string userId, string roleName)
		{
			await DeferAsync(ephemeral: true);

			try
			{
				var (targetUser, targetRole, isValid) = await ValidateUserAndRoleAsync(userId, roleName);
				if (!isValid) return;

				// 사용자가 이미 해당 역할을 가지고 있는지 확인
				if (targetUser.Roles.Any(r => r.Id == targetRole.Id))
				{
					await FollowupAsync($"'{targetUser.DisplayName}'님은 이미 '{roleName}' 역할을 가지고 있습니다.", ephemeral: true);
					return;
				}

				// 역할 추가
				var result = await _roleService.AddRoleToUserAsync(targetUser, targetRole, Context.User.Username);

				if (result.Success)
				{
					await FollowupAsync($"✅ {result.Message}", ephemeral: true);
					Logger.Print($"'{Context.User.Username}'님이 '{targetUser.Username}' 사용자에게 '{roleName}' 역할을 성공적으로 추가했습니다.");
				}
				else
				{
					await FollowupAsync($"❌ 역할 추가 실패: {result.ErrorMessage}", ephemeral: true);
					Logger.Print($"역할 추가 실패: {result.ErrorMessage}", LogType.ERROR);
				}
			}
			catch (Exception ex)
			{
				await FollowupAsync("역할 추가 중 오류가 발생했습니다.", ephemeral: true);
				Logger.Print($"역할 추가 중 오류 발생: {ex.Message}", LogType.ERROR);
			}
		}

		// 역할 제거 확인 버튼 클릭 시 실행될 메서드
		[ComponentInteraction("remove_role_confirm_button:*:*")]
		public async Task RemoveRoleConfirmButton(string userId, string roleName)
		{
			await DeferAsync(ephemeral: true);

			try
			{
				var (targetUser, targetRole, isValid) = await ValidateUserAndRoleAsync(userId, roleName);
				if (!isValid) return;

				// 사용자가 해당 역할을 가지고 있는지 확인
				if (!targetUser.Roles.Any(r => r.Id == targetRole.Id))
				{
					await FollowupAsync($"'{targetUser.DisplayName}'님은 '{roleName}' 역할을 가지고 있지 않습니다.", ephemeral: true);
					return;
				}

				// 역할 제거
				var result = await _roleService.RemoveRoleFromUserAsync(targetUser, targetRole, Context.User.Username);

				if (result.Success)
				{
					await FollowupAsync($"✅ {result.Message}", ephemeral: true);
					Logger.Print($"'{Context.User.Username}'님이 '{targetUser.Username}' 사용자에게서 '{roleName}' 역할을 성공적으로 제거했습니다.");
				}
				else
				{
					await FollowupAsync($"❌ 역할 제거 실패: {result.ErrorMessage}", ephemeral: true);
					Logger.Print($"역할 제거 실패: {result.ErrorMessage}", LogType.ERROR);
				}
			}
			catch (Exception ex)
			{
				await FollowupAsync("역할 제거 중 오류가 발생했습니다.", ephemeral: true);
				Logger.Print($"역할 제거 중 오류 발생: {ex.Message}", LogType.ERROR);
			}
		}

		// 사용자 선택 메뉴에서 사용자를 선택했을 때 실행될 메서드 (역할 추가용)
		[ComponentInteraction("user_selection_menu:*")]
		public async Task UserSelectionMenu(string roleName, string[] selectedValues)
		{
			await DeferAsync(ephemeral: true);

			try
			{
				if (selectedValues == null || selectedValues.Length == 0)
				{
					await FollowupAsync("사용자가 선택되지 않았습니다.", ephemeral: true);
					return;
				}

				var selectedUserId = selectedValues[0];
				var (selectedUser, targetRole, isValid) = await ValidateUserAndRoleAsync(selectedUserId, roleName);
				if (!isValid) return;

				// 확인 버튼 표시
				var builder = new ComponentBuilder()
					.WithButton("역할 추가", $"add_role_confirm_button:{selectedUserId}:{roleName}", ButtonStyle.Danger);

				await FollowupAsync($"'{selectedUser.DisplayName}({selectedUser.Username}, {selectedUser.Id})' 사용자의 역할에 '{roleName}'을 추가하시겠습니까?", components: builder.Build(), ephemeral: true);

				// 로그 남기기
				Logger.Print($"'{Context.User.Username}'님이 사용자 선택 메뉴에서 '{selectedUser.Username}' 사용자를 선택했습니다. 대상 역할: '{roleName}'");
			}
			catch (Exception ex)
			{
				await FollowupAsync("사용자 선택 처리 중 오류가 발생했습니다.", ephemeral: true);
				Logger.Print($"사용자 선택 처리 중 오류 발생: {ex.Message}", LogType.ERROR);
			}
		}

		// 사용자 선택 메뉴에서 사용자를 선택했을 때 실행될 메서드 (역할 제거용)
		[ComponentInteraction("remove_role_user_selection_menu:*")]
		public async Task RemoveRoleUserSelectionMenu(string roleName, string[] selectedValues)
		{
			await DeferAsync(ephemeral: true);

			try
			{
				if (selectedValues == null || selectedValues.Length == 0)
				{
					await FollowupAsync("사용자가 선택되지 않았습니다.", ephemeral: true);
					return;
				}

				var selectedUserId = selectedValues[0];
				var (selectedUser, targetRole, isValid) = await ValidateUserAndRoleAsync(selectedUserId, roleName);
				if (!isValid) return;

				// 확인 버튼 표시
				var builder = new ComponentBuilder()
					.WithButton("역할 제거", $"remove_role_confirm_button:{selectedUserId}:{roleName}", ButtonStyle.Danger);

				await FollowupAsync($"'{selectedUser.DisplayName}({selectedUser.Username}, {selectedUser.Id})' 사용자의 '{roleName}' 역할을 제거하시겠습니까?", components: builder.Build(), ephemeral: true);

				// 로그 남기기
				Logger.Print($"'{Context.User.Username}'님이 역할 제거 선택 메뉴에서 '{selectedUser.Username}' 사용자를 선택했습니다. 대상 역할: '{roleName}'");
			}
			catch (Exception ex)
			{
				await FollowupAsync("사용자 선택 처리 중 오류가 발생했습니다.", ephemeral: true);
				Logger.Print($"사용자 선택 처리 중 오류 발생: {ex.Message}", LogType.ERROR);
			}
		}
	}
}
