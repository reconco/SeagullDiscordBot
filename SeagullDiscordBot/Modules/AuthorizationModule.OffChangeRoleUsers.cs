using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System.Linq;
using System.Threading.Tasks;
using SeagullDiscordBot.Services;
using System;

//AI�ڵ� Ȯ�� �Ϸ�(�׽�Ʈ �ʿ�)
namespace SeagullDiscordBot.Modules
{
	public partial class AuthorizationModule : InteractionModuleBase<SocketInteractionContext>
	{
		// ����ڵ鿡�Լ� ���ű� ���� ���� ��ư Ŭ�� �� ����� �޼��� (authorization_off 2��)
		[ComponentInteraction("auth_off_change_role_users_button")]
		public async Task AuthOffChangeRoleUsersButton()
		{
			await RespondAsync("��� ����ڵ鿡�Լ� ���ű� ������ �����մϴ�...\n�Ϸ� �޽����� ��Ÿ�������� ��ٷ��ּ���.(1�ʴ� 1�� ó��)", ephemeral: true);
			Logger.Print($"���� '{Context.Guild.Name}'({Context.Guild.Id})���� '{Context.User.Username}'���� ����� ���� ���� ��ư�� Ŭ���߽��ϴ�.");

			var guild = Context.Guild;
			var requestedBy = Context.User.Username;

			await guild.DownloadUsersAsync();
			var allUsers = guild.Users;

			// ���� ������ ���� ��������
			var settings = Config.GetSettings(Context.Guild.Id);
			var targetRole = guild.Roles.FirstOrDefault(r => r.Id == settings.AutoRoleId);
			
			if(targetRole == null)
			{
				await FollowupAsync("���ű� ������ �����Ǿ� ���� �ʰų� �̹� �����Ǿ����ϴ�.", ephemeral: true);
				return;
			}

			// ���ű� ������ ���� ����ڵ鸸 ���͸�
			var usersWithRole = allUsers.Where(user =>	
				!user.IsBot && 
				user.Roles.Any(r => r.Id == targetRole.Id)
			).ToList();

			int successCount = 0;
			int errorCount = 0;
			int totalUsers = usersWithRole.Count;

			if (totalUsers == 0)
			{
				await FollowupAsync("���ű� ������ ���� ����ڰ� �����ϴ�.", ephemeral: true);
				return;
			}

			try
			{
				int processedUsers = 0;

				await FollowupAsync($"�� {totalUsers}���� ����ڿ��Լ� ���ű� ������ �����մϴ�...", ephemeral: true);

				foreach (var user in usersWithRole)
				{
					processedUsers++;

					var result = await _roleService.RemoveRoleFromUserAsync(user, targetRole, requestedBy);

					if (result.Success)
					{
						successCount++;
						if (processedUsers % 50 == 0 || processedUsers == totalUsers)
						{
							Logger.Print($"���� ���� ���� ��: {processedUsers}/{totalUsers} �Ϸ�");
							await FollowupAsync($"���� ��Ȳ: {processedUsers}/{totalUsers} ����� ó�� �Ϸ�", ephemeral: true);
						}
					}
					else
					{
						errorCount++;
						Logger.Print($"����� '{user.Username}'���Լ� ���� ���� ����: {result.ErrorMessage}", LogType.ERROR);
					}

					// API ������ ���ϱ� ���� 0.5�� ���
					await Task.Delay(500);
				}

				await FollowupAsync($"���ű� ���� ���� �Ϸ�: �� {totalUsers}�� �� {successCount}�� ����, {errorCount}�� ����", ephemeral: true);
			}
			catch (Exception ex)
			{
				Logger.Print($"���� {Context.Guild.Id} ����� ���� ���� �� ���� �߻�: {ex.Message}", LogType.ERROR);
				await FollowupAsync($"���� ���� �� ������ �߻��߽��ϴ�: {ex.Message}", ephemeral: true);
			}

			await FollowupAsync("��� ����ڵ��� ���ű� ���� ���� �Ϸ�!", ephemeral: true);
		}
	}
}