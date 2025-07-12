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
		[SlashCommand("add_all_user_role", "������ ��� ����ڿ��� ������ �߰��մϴ�.(�����ڿ� ������)")]
		[RequireUserPermission(GuildPermission.Administrator)]
		public async Task ChangeAllUserRoleCommand([Summary("role", "�߰��� ������ �̸�")] string role)
		{
			var targetRole = _roleService.FindExistingRole(Context.Guild, role);
			if (targetRole == null)
			{
				await RespondAsync($"�������� '{role}' ������ ã�� �� �����ϴ�. ���� �̸��� ��Ȯ�� �Է����ּ���.", ephemeral: true);
				return;
			}

			var builder = new ComponentBuilder()
						.WithButton("��� ������� ���� �߰�", $"add_all_user_role_button:{role}", ButtonStyle.Danger);

			await RespondAsync($"������ ��� ����ڿ��� ���� '{role}'�� �߰��ϰڽ��ϱ�?\n�� ���� �����ڴ� ���ܵ˴ϴ�.", components: builder.Build(), ephemeral: true);
			Logger.Print($"'{Context.User.Username}'���� ��� ������� ���� �߰��� ��û�߽��ϴ�. ��� ����: '{role}'");
		}

		[SlashCommand("remove_all_user_role", "������ ��� ����ڿ��Լ� ������ �����մϴ�.(�����ڿ� ������)")]
		[RequireUserPermission(GuildPermission.Administrator)]
		public async Task RemoveAllUserRoleCommand([Summary("role", "������ ������ �̸�")] string role)
		{
			var targetRole = _roleService.FindExistingRole(Context.Guild, role);
			if (targetRole == null)
			{
				await RespondAsync($"�������� '{role}' ������ ã�� �� �����ϴ�. ���� �̸��� ��Ȯ�� �Է����ּ���.", ephemeral: true);
				return;
			}

			var builder = new ComponentBuilder()
						.WithButton("��� ������� ���� ����", $"remove_all_user_role_button:{role}", ButtonStyle.Danger);

			await RespondAsync($"������ ��� ����ڿ��Լ� '{role}' ������ �����ϰڽ��ϱ�?\n�� ���� �����ڴ� ���ܵ˴ϴ�.", components: builder.Build(), ephemeral: true);
			Logger.Print($"'{Context.User.Username}'���� ��� ������� ���� ���Ÿ� ��û�߽��ϴ�. ��� ����: '{role}'");
		}

		[ComponentInteraction("add_all_user_role_button:*")]
		public async Task AddAllUserRoleButton(string roleName)
		{
			await RespondAsync("��� ����ڵ鿡�� ������ �߰��մϴ�...\n�Ϸ� �޽����� ��Ÿ�������� ��ٷ��ּ���.(1�ʴ� 1�� ó��)", ephemeral: true);
			Logger.Print($"'{Context.User.Username}'���� ����� ���� �߰� ��ư�� Ŭ���߽��ϴ�.");

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
					await FollowupAsync($"{roleName} ������ ���� ������ �� �����ϴ�.", ephemeral: true);
					return;
				}

				int totalUsers = targetUsers.Count;
				int processedUsers = 0;

				await FollowupAsync($"�� {totalUsers}���� ����ڿ��� ������ �߰��մϴ�... (�� �� ������ {excludedCount}�� ����)", ephemeral: true);

				foreach (var user in targetUsers)
				{
					processedUsers++;

					if (user.Roles.Any(r => r.Id == newRole.Id))
					{
						Logger.Print($"����� '{user.Username}'��(��) �̹� '{roleName}' ������ ������ �ֽ��ϴ�.");
						continue;
					}

					var result = await _roleService.AddRoleToUserAsync(user, newRole, requestedBy);

					if (result.Success)
					{
						successCount++;
						if (processedUsers % 50 == 0 || processedUsers == totalUsers)
						{
							Logger.Print($"���� �߰� ���� ��: {processedUsers}/{totalUsers} �Ϸ� (������ �� �� {excludedCount}�� ����)");
							await FollowupAsync($"���� ��Ȳ: {processedUsers}/{totalUsers} ����� ó�� �Ϸ�", ephemeral: true);
						}
					}
					else
					{
						errorCount++;
						Logger.Print($"����� '{user.Username}'���� ���� �߰� ����: {result.ErrorMessage}", LogType.ERROR);
					}

					await Task.Delay(1000);
				}

				await FollowupAsync($"���� �߰� �Ϸ�: �� {totalUsers}�� �� {successCount}�� ����, {errorCount}�� ����\n(������ �� �� {excludedCount}�� ���ܵ�)", ephemeral: true);
			}
			catch (Exception ex)
			{
				Logger.Print($"����� ���� �߰� �� ���� �߻�: {ex.Message}", LogType.ERROR);
				await FollowupAsync($"���� �߰� �� ������ �߻��߽��ϴ�: {ex.Message}", ephemeral: true);
			}

			await FollowupAsync("���� ����ڵ��� ���� �߰� �Ϸ�!", ephemeral: true);
		}

		[ComponentInteraction("remove_all_user_role_button:*")]
		public async Task RemoveAllUserRoleButton(string roleName)
		{
			await RespondAsync("��� ����ڵ鿡�� ������ �����մϴ�...\n�Ϸ� �޽����� ��Ÿ�������� ��ٷ��ּ���.(1�ʴ� 1�� ó��)", ephemeral: true);
			Logger.Print($"'`{Context.User.Username}'���� ��� ����� ���� ���� ��ư�� Ŭ���߽��ϴ�.`");

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
					await FollowupAsync($"{roleName} ������ ���� ������ �� �����ϴ�.`", ephemeral: true);
					return;
				}

				var usersWithRole = targetUsers.Where(user => user.Roles.Any(r => r.Id == targetRole.Id)).ToList();
				int totalUsers = usersWithRole.Count;
				int processedUsers = 0;

				if (totalUsers == 0)
				{
					await FollowupAsync($"'{roleName}' ������ ���� ����ڰ� �����ϴ�. (�� �� ������ {excludedCount}�� ����)`", ephemeral: true);
					return;
				}

				await FollowupAsync($"�� {totalUsers}���� ����ڿ��Լ� ������ �����մϴ�... (�� �� ������ {excludedCount}�� ����)`", ephemeral: true);

				foreach (var user in usersWithRole)
				{
					processedUsers++;

					var result = await _roleService.RemoveRoleFromUserAsync(user, targetRole, requestedBy);
					if (result.Success)
					{
						successCount++;
						if (processedUsers % 50 == 0 || processedUsers == totalUsers)
						{
							Logger.Print($"���� ���� ���� ��: {processedUsers}/{totalUsers} �Ϸ� (������ �� �� {excludedCount}�� ����)`");
							await FollowupAsync($"���� ��Ȳ: {processedUsers}/{totalUsers} ����� ó�� �Ϸ�`", ephemeral: true);
						}
					}
					else
					{
						errorCount++;
						Logger.Print($"����� '{user.Username}'���Լ� ���� ���� ����: {result.ErrorMessage}`", LogType.ERROR);
					}

					await Task.Delay(1000);
				}

				await FollowupAsync($"���� ���� �Ϸ�: �� {totalUsers}�� �� {successCount}�� ����, {errorCount}�� ����\n(������ �� �� {excludedCount}�� ���ܵ�)`", ephemeral: true);
			}
			catch (Exception ex)
			{
				Logger.Print($"����� ���� ���� �� ���� �߻�: {ex.Message}`", LogType.ERROR);
				await FollowupAsync($"���� ���� �� ������ �߻��߽��ϴ�: {ex.Message}`", ephemeral: true);
			}

			await FollowupAsync($"��� ����ڵ��� {roleName} ���� ���� �Ϸ�!", ephemeral: true);
		}
	}
}