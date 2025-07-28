using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System.Linq;
using System.Threading.Tasks;
using SeagullDiscordBot.Services;
using System;

namespace SeagullDiscordBot.Modules
{
	public partial class AuthorizationModule : InteractionModuleBase<SocketInteractionContext>
	{
		// ��Ģ �ȳ� �� ���� ä�� ���� ��ư Ŭ�� �� ����� �޼��� (authorization_off 4��)
		[ComponentInteraction("auth_off_remove_rule_channel_button")]
		public async Task AuthOffRemoveRuleChannelButton()
		{
			await RespondAsync("����� ���� ä���� �����մϴ�...\n�Ϸ� �޽����� ��Ÿ�������� ��ٷ��ּ���.", ephemeral: true);
			Logger.Print($"'{Context.User.Username}'���� ��Ģ ä�� ���� ��ư�� Ŭ���߽��ϴ�.");

			var guild = Context.Guild;

			try
			{
				// Config.Settings.AuthChannelId�� ���� ä�� ã��
				if (Config.Settings.AuthChannelId == null)
				{
					await FollowupAsync("���� ä�� ID�� �����Ǿ� ���� �ʽ��ϴ�. �̹� �����Ǿ��ų� �������� �ʾ��� �� �ֽ��ϴ�.", ephemeral: true);
					Logger.Print("���� ä�� ID�� �����Ǿ� ���� �ʽ��ϴ�.", LogType.WARNING);
					return;
				}

				var ruleChannel = guild.GetTextChannel(Config.Settings.AuthChannelId.Value);

				if (ruleChannel == null)
				{

					// ä���� ã�� �� ������ Config ������ �ʱ�ȭ
					Config.Settings.AuthChannelId = null;
					Config.SaveSettings();

					await FollowupAsync("���� ä�� ������ �ʱ�ȭ�߽��ϴ�.", ephemeral: true);
					return;
				}

				string channelName = ruleChannel.Name;
				string categoryInfo = ruleChannel.Category != null ? $" (ī�װ�: {ruleChannel.Category.Name})" : "";

				// ���� ���� ī�װ� ���� ����
				var category = ruleChannel.Category;

				// ä�� ����
				await ruleChannel.DeleteAsync();

				await FollowupAsync($"'{channelName}' ä���� ���������� �����Ǿ����ϴ�.{categoryInfo}", ephemeral: true);
				Logger.Print($"'{Context.User.Username}'���� '{channelName}' ä���� �����߽��ϴ�.{categoryInfo}");

				// Config ���� �ʱ�ȭ
				Config.Settings.AuthChannelId = null;
				Config.SaveSettings();
			}
			catch (Exception ex)
			{
				Logger.Print($"��Ģ ä�� ���� �� ���� �߻�: {ex.Message}", LogType.ERROR);
				await FollowupAsync($"ä�� ���� �� ������ �߻��߽��ϴ�: {ex.Message}", ephemeral: true);
			}

			await FollowupAsync("����� ���� ä�� ���� �Ϸ�!", ephemeral: true);
		}
	}
}