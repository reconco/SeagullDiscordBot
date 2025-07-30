using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SeagullDiscordBot.Services;

//AI�ڵ� Ȯ�� �Ϸ�(�׽�Ʈ �ʿ�)
namespace SeagullDiscordBot.Modules
{
	public partial class AuthorizationModule : InteractionModuleBase<SocketInteractionContext>
	{
		// ä�� ���� ���� ��ư Ŭ�� �� ����� �޼��� (authorization_off 1��)
		[ComponentInteraction("auth_off_modify_channel_button")]
		public async Task AuthOffModifyChannelButton()
		{
			await RespondAsync("���� ä�ε��� ���ű� ������ �����ϰ�, ���ű� ������ �޽��� ���� ������ ��ȿ�ߴ� ä�ο��� everyone ���ҿ��� �޽��� ���� ������ �����մϴ�...\n�Ϸ� �޽����� ��Ÿ�������� ��ٷ��ּ���.", ephemeral: true);
			
			Logger.Print($"���� '{Context.Guild.Name}'({Context.Guild.Id})���� '{Context.User.Username}'���� ä�� ���� ���� ��ư�� Ŭ���߽��ϴ�.");

			var everyoneRole = Context.Guild.EveryoneRole;
			
			// ���� ������ ���� ��������
			var settings = Config.GetSettings(Context.Guild.Id);
			
			// ������ ����� ������ ������
			var verifiedRole = Context.Guild.Roles.FirstOrDefault(r => r.Id == settings.AutoRoleId);
			if (verifiedRole == null)
			{
				await FollowupAsync("���ű� ������ ã�� �� �����ϴ�. �̹� �����Ǿ��ų� �������� �ʾ��� �� �ֽ��ϴ�. �۾��� �����մϴ�.", ephemeral: true);
				Logger.Print("���ű� ������ ã�� �� ���� ä�� ���� ������ �����մϴ�.", LogType.WARNING);
				return;
			}

			List<SocketGuildChannel> channels = Context.Guild.Channels.ToList();

			foreach (var channel in channels)
			{
				if (channel is ITextChannel textChannel)
				{
					// ���ű� ������ ���� ������ ������ (null�̸� �⺻�� ���)
					var permissionsNullable = textChannel.GetPermissionOverwrite(verifiedRole);
					if (permissionsNullable.HasValue)
					{
						var permissions = permissionsNullable.Value;
						// ������ �����Ǿ� �ִ� ����� ó��
					}
					else
					{
						// ������ �����Ǿ� ���� ���� ����� ó��
						continue;
					}

					PermValue sendMsg = permissionsNullable.Value.SendMessages;

					if (sendMsg == PermValue.Allow || sendMsg == PermValue.Inherit)
					{
						// everyone : �޽��� ���� �����ϵ��� ����
						var everyonePermissions = CreatePermissionsWithSendMessages(permissionsNullable.Value, sendMsg);
						await textChannel.AddPermissionOverwriteAsync(everyoneRole, everyonePermissions);

						await textChannel.RemovePermissionOverwriteAsync(verifiedRole);
					}

					Logger.Print($"'{textChannel.Name}' ä�ο� everyone �޽��� ���� ���");
				}
				else if (channel is IVoiceChannel voiceChannel)
				{
					// ���ű� ������ ���� ������ ������ (null�̸� �⺻�� ���)
					var permissionsNullable = voiceChannel.GetPermissionOverwrite(verifiedRole);
					if (permissionsNullable.HasValue)
					{
						var permissions = permissionsNullable.Value;
						// ������ �����Ǿ� �ִ� ����� ó��
					}
					else
					{
						// ������ �����Ǿ� ���� ���� ����� ó��
						continue;
					}

					PermValue sendMsg = permissionsNullable.Value.SendMessages;

					if (sendMsg == PermValue.Allow || sendMsg == PermValue.Inherit)
					{
						// everyone : �޽��� ���� �����ϵ��� ����
						var everyonePermissions = CreatePermissionsWithSendMessages(permissionsNullable.Value, sendMsg);
						await voiceChannel.AddPermissionOverwriteAsync(everyoneRole, everyonePermissions);

						await voiceChannel.RemovePermissionOverwriteAsync(verifiedRole);
					}

					Logger.Print($"'{voiceChannel.Name}' ���� ä�ο� everyone �޽��� ���� ���");
				}
			}

			await FollowupAsync($"���� ä�ε��� ���� ���� �Ϸ�! (Ȱ�� ���� ä�ο��� Everyone ����: �޽��� ���� ���)", ephemeral: true);
			Logger.Print($"�� ��� ä���� ������ �����Ǿ����ϴ�. Everyone �޽��� ���� ���");
		}
	}
}