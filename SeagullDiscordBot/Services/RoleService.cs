using Discord;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SeagullDiscordBot.Services
{
	public class RoleService
	{
		/// <summary>
		/// 역할 이름이 이미 존재하는지 확인합니다.
		/// </summary>
		/// <param name="guild">확인할 길드</param>
		/// <param name="roleName">확인할 역할 이름</param>
		/// <returns>역할이 존재하면 true, 아니면 false</returns>
		public bool RoleExists(IGuild guild, string roleName)
		{
			// 대소문자 구분 없이 비교하기 위해 소문자로 변환
			roleName = roleName.ToLowerInvariant();

			// 모든 역할 중 같은 이름이 있는지 확인
			return guild.Roles.Any(r => r.Name.ToLowerInvariant() == roleName);
		}

		/// <summary>
		/// 이미 존재하는 역할을 찾습니다.
		/// </summary>
		/// <param name="guild">검색할 길드</param>
		/// <param name="roleName">찾을 역할 이름</param>
		/// <returns>찾은 역할, 없으면 null</returns>
		public IRole FindExistingRole(IGuild guild, string roleName)
		{
			// 대소문자 구분 없이 비교하기 위해 소문자로 변환
			roleName = roleName.ToLowerInvariant();

			// 모든 역할 중 같은 이름이 있는지 확인
			return guild.Roles.FirstOrDefault(r => r.Name.ToLowerInvariant() == roleName);
		}

		/// <summary>
		/// 새로운 역할을 생성합니다.
		/// </summary>
		/// <param name="guild">역할을 생성할 길드</param>
		/// <param name="roleName">생성할 역할 이름</param>
		/// <param name="color">역할 색상 (기본값: 파란색)</param>
		/// <param name="permissions">역할 권한 (기본값: 없음)</param>
		/// <param name="isHoisted">역할을 사용자 목록에서 분리하여 표시할지 여부 (기본값: false)</param>
		/// <param name="isMentionable">역할을 멘션할 수 있는지 여부 (기본값: true)</param>
		/// <returns>생성된 역할</returns>
		public async Task<IRole> CreateRoleAsync(
			IGuild guild,
			string roleName,
			Color? color = null,
			GuildPermissions? permissions = null,
			bool isHoisted = false,
			bool isMentionable = true)
		{
			try
			{
				// 같은 이름의 역할이 이미 존재하는지 확인
				var existingRole = FindExistingRole(guild, roleName);
				if (existingRole != null)
				{
					Logger.Print($"'{roleName}' 역할이 이미 존재합니다. 기존 역할을 반환합니다.", LogType.WARNING);
					return existingRole;
				}

				// 역할 생성
				var newRole = await guild.CreateRoleAsync(
					roleName,
					permissions ?? GuildPermissions.None,
					color ?? Color.Blue,
					isHoisted,
					isMentionable);

				// 로그 남기기
				Logger.Print($"'{newRole.Name}' 역할이 생성되었습니다.");

				return newRole;
			}
			catch (Exception ex)
			{
				Logger.Print($"역할 생성 중 오류 발생: {ex.Message}", LogType.ERROR);
				throw;
			}
		}

		/// <summary>
		/// 새로운 역할을 생성하고 결과를 반환합니다.
		/// </summary>
		/// <param name="guild">역할을 생성할 길드</param>
		/// <param name="roleName">생성할 역할 이름</param>
		/// <param name="username">요청한 사용자 이름</param>
		/// <param name="color">역할 색상 (기본값: 파란색)</param>
		/// <param name="permissions">역할 권한 (기본값: 없음)</param>
		/// <param name="isHoisted">역할을 사용자 목록에서 분리하여 표시할지 여부 (기본값: false)</param>
		/// <param name="isMentionable">역할을 멘션할 수 있는지 여부 (기본값: true)</param>
		/// <returns>역할 생성 결과</returns>
		public async Task<RoleResult> CreateRoleWithResultAsync(
			IGuild guild,
			string roleName,
			string username,
			Color? color = null,
			GuildPermissions? permissions = null,
			bool isHoisted = false,
			bool isMentionable = true)
		{
			try
			{
				// 같은 이름의 역할이 이미 존재하는지 확인
				var existingRole = FindExistingRole(guild, roleName);
				if (existingRole != null)
				{
					Logger.Print($"'{username}'님이 요청한 '{roleName}' 역할이 이미 존재합니다.", LogType.WARNING);

					return RoleResult.Successful(
						existingRole,
						$"역할 '{existingRole.Name}'이(가) 이미 존재합니다. 이 역할을 사용하겠습니다."
					);
				}

				// 역할 생성
				var newRole = await guild.CreateRoleAsync(
					roleName,
					permissions ?? GuildPermissions.None,
					color ?? Color.Blue,
					isHoisted,
					isMentionable);

				// 로그 남기기
				Logger.Print($"'{username}'님이 '{newRole.Name}' 역할을 생성했습니다.");

				return RoleResult.Successful(
					newRole,
					$"역할 '{newRole.Name}'이(가) 성공적으로 생성되었습니다."
				);
			}
			catch (Exception ex)
			{
				Logger.Print($"역할 생성 중 오류 발생: {ex.Message}", LogType.ERROR);
				return RoleResult.Failed(ex.Message);
			}
		}

		/// <summary>
		/// 사용자에게 역할을 추가합니다.
		/// </summary>
		/// <param name="user">역할을 추가할 사용자</param>
		/// <param name="role">추가할 역할</param>
		/// <param name="requestedBy">역할 추가를 요청한 사용자 이름</param>
		/// <returns>역할 추가 결과</returns>
		public async Task<ServiceResult> AddRoleToUserAsync(IGuildUser user, IRole role, string requestedBy)
		{
			try
			{
				// 사용자가 이미 해당 역할을 가지고 있는지 확인
				if (user.RoleIds.Contains(role.Id))
				{
					Logger.Print($"'{requestedBy}'님이 요청한 '{role.Name}' 역할은 이미 '{user.Username}'님에게 부여되어 있습니다.", LogType.WARNING);
					return ServiceResult.Successful($"'{user.Username}'님은 이미 '{role.Name}' 역할을 가지고 있습니다.");
				}

				// 사용자에게 역할 추가
				await user.AddRoleAsync(role);

				// 로그 남기기
				Logger.Print($"'{requestedBy}'님이 '{user.Username}'님에게 '{role.Name}' 역할을 추가했습니다.");

				return ServiceResult.Successful($"'{user.Username}'님에게 '{role.Name}' 역할을 성공적으로 추가했습니다.");
			}
			catch (Exception ex)
			{
				Logger.Print($"역할 추가 중 오류 발생: {ex.Message}", LogType.ERROR);
				return ServiceResult.Failed(ex.Message);
			}
		}

		/// <summary>
		/// 사용자에게서 역할을 제거합니다.
		/// </summary>
		/// <param name="user">역할을 제거할 사용자</param>
		/// <param name="role">제거할 역할</param>
		/// <param name="requestedBy">역할 제거를 요청한 사용자 이름</param>
		/// <returns>역할 제거 결과</returns>
		public async Task<ServiceResult> RemoveRoleFromUserAsync(IGuildUser user, IRole role, string requestedBy)
		{
			try
			{
				// 사용자가 해당 역할을 가지고 있는지 확인
				if (!user.RoleIds.Contains(role.Id))
				{
					Logger.Print($"'{requestedBy}'님이 요청한 '{role.Name}' 역할은 '{user.Username}'님에게 부여되어 있지 않습니다.", LogType.WARNING);
					return ServiceResult.Successful($"'{user.Username}'님은 '{role.Name}' 역할을 가지고 있지 않습니다.");
				}

				// 사용자에게서 역할 제거
				await user.RemoveRoleAsync(role);

				// 로그 남기기
				Logger.Print($"'{requestedBy}'님이 '{user.Username}'님에게서 '{role.Name}' 역할을 제거했습니다.");

				return ServiceResult.Successful($"'{user.Username}'님에게서 '{role.Name}' 역할을 성공적으로 제거했습니다.");
			}
			catch (Exception ex)
			{
				Logger.Print($"역할 제거 중 오류 발생: {ex.Message}", LogType.ERROR);
				return ServiceResult.Failed(ex.Message);
			}
		}

		/// <summary>
		/// 역할 이름으로 사용자에게 역할을 추가합니다. 역할이 존재하지 않으면 생성합니다.
		/// </summary>
		/// <param name="guild">역할이 속한 길드</param>
		/// <param name="user">역할을 추가할 사용자</param>
		/// <param name="roleName">추가할 역할 이름</param>
		/// <param name="requestedBy">역할 추가를 요청한 사용자 이름</param>
		/// <param name="color">역할이 존재하지 않을 경우 생성할 역할의 색상</param>
		/// <returns>역할 추가 결과</returns>
		public async Task<ServiceResult> AddRoleToUserByNameAsync(
			IGuild guild,
			IGuildUser user,
			string roleName,
			string requestedBy,
			Color? color = null)
		{
			try
			{
				// 역할 찾기 또는 생성
				var role = FindExistingRole(guild, roleName);
				if (role == null)
				{
					// 역할이 존재하지 않으면 새로 생성
					role = await CreateRoleAsync(guild, roleName, color);
					Logger.Print($"'{requestedBy}'님이 '{roleName}' 역할을 생성했습니다.");
				}

				// 사용자에게 역할 추가
				return await AddRoleToUserAsync(user, role, requestedBy);
			}
			catch (Exception ex)
			{
				Logger.Print($"역할 추가 중 오류 발생: {ex.Message}", LogType.ERROR);
				return ServiceResult.Failed(ex.Message);
			}
		}

		/// <summary>
		/// 특정 사용자의 역할을 변경합니다. (기존 역할 제거 후 새 역할 추가)
		/// </summary>
		/// <param name="guild">역할이 속한 길드</param>
		/// <param name="user">역할을 변경할 사용자</param>
		/// <param name="oldRoleName">제거할 역할 이름</param>
		/// <param name="newRoleName">추가할 역할 이름</param>
		/// <param name="requestedBy">역할 변경을 요청한 사용자 이름</param>
		/// <param name="color">새 역할이 존재하지 않을 경우 생성할 역할의 색상</param>
		/// <returns>역할 변경 결과</returns>
		public async Task<ServiceResult> ChangeUserRoleAsync(
			IGuild guild,
			IGuildUser user,
			string oldRoleName,
			string newRoleName,
			string requestedBy,
			Color? color = null)
		{
			try
			{
				// 기존 역할 찾기
				var oldRole = FindExistingRole(guild, oldRoleName);
				if (oldRole == null)
				{
					return ServiceResult.Failed($"'{oldRoleName}' 역할을 찾을 수 없습니다.");
				}

				// 새 역할 찾기 또는 생성
				var newRole = FindExistingRole(guild, newRoleName);
				if (newRole == null)
				{
					// 역할이 존재하지 않으면 새로 생성
					newRole = await CreateRoleAsync(guild, newRoleName, color);
					Logger.Print($"'{requestedBy}'님이 '{newRoleName}' 역할을 생성했습니다.");
				}

				// 사용자가 기존 역할을 가지고 있는지 확인
				if (!user.RoleIds.Contains(oldRole.Id))
				{
					Logger.Print($"'{user.Username}'님은 '{oldRoleName}' 역할을 가지고 있지 않습니다.", LogType.WARNING);
				}
				else
				{
					// 기존 역할 제거
					await user.RemoveRoleAsync(oldRole);
					Logger.Print($"'{requestedBy}'님이 '{user.Username}'님에게서 '{oldRoleName}' 역할을 제거했습니다.");
				}

				// 사용자가 새 역할을 이미 가지고 있는지 확인
				if (user.RoleIds.Contains(newRole.Id))
				{
					Logger.Print($"'{user.Username}'님은 이미 '{newRoleName}' 역할을 가지고 있습니다.", LogType.WARNING);
					return ServiceResult.Successful($"'{user.Username}'님의 역할이 변경되었습니다. ('{oldRoleName}' → '{newRoleName}')");
				}

				// 새 역할 추가
				await user.AddRoleAsync(newRole);
				Logger.Print($"'{requestedBy}'님이 '{user.Username}'님에게 '{newRoleName}' 역할을 추가했습니다.");

				return ServiceResult.Successful($"'{user.Username}'님의 역할이 성공적으로 변경되었습니다. ('{oldRoleName}' → '{newRoleName}')");
			}
			catch (Exception ex)
			{
				Logger.Print($"역할 변경 중 오류 발생: {ex.Message}", LogType.ERROR);
				return ServiceResult.Failed(ex.Message);
			}
		}
	}
}