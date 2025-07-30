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
		// 규칙 안내 및 인증 채널 제거 버튼 클릭 시 실행될 메서드 (authorization_off 4번)
		[ComponentInteraction("auth_off_remove_rule_channel_button")]
		public async Task AuthOffRemoveRuleChannelButton()
		{
			await RespondAsync("사용자 인증 채널을 제거합니다...\n완료 메시지가 나타날때까지 기다려주세요.", ephemeral: true);
			Logger.Print($"서버 '{Context.Guild.Name}'({Context.Guild.Id})에서 '{Context.User.Username}'님이 규칙 채널 제거 버튼을 클릭했습니다.");

			var guild = Context.Guild;

			try
			{
				// 현재 서버의 설정 가져오기
				var settings = Config.GetSettings(Context.Guild.Id);
				
				// Config.Settings.AuthChannelId로 인증 채널 찾기
				if (settings.AuthChannelId == null)
				{
					await FollowupAsync("인증 채널 ID가 설정되어 있지 않습니다. 이미 삭제되었거나 설정되지 않았을 수 있습니다.", ephemeral: true);
					Logger.Print("인증 채널 ID가 설정되어 있지 않습니다.", LogType.WARNING);
					return;
				}

				var ruleChannel = guild.GetTextChannel(settings.AuthChannelId.Value);

				if (ruleChannel == null)
				{
					// 채널을 찾을 수 없더라도 Config 설정은 초기화
					Config.UpdateSetting(Context.Guild.Id, configSettings =>
					{
						configSettings.AuthChannelId = null;
					});

					await FollowupAsync("인증 채널 설정을 초기화했습니다.", ephemeral: true);
					return;
				}

				string channelName = ruleChannel.Name;
				string categoryInfo = ruleChannel.Category != null ? $" (카테고리: {ruleChannel.Category.Name})" : "";

				// 삭제 전에 카테고리 정보 저장
				var category = ruleChannel.Category;

				// 채널 삭제
				await ruleChannel.DeleteAsync();

				await FollowupAsync($"'{channelName}' 채널이 성공적으로 삭제되었습니다.{categoryInfo}", ephemeral: true);
				Logger.Print($"서버 '{Context.Guild.Name}'({Context.Guild.Id})에서 '{Context.User.Username}'님이 '{channelName}' 채널을 삭제했습니다.{categoryInfo}");

				// 현재 서버의 Config 설정 초기화
				Config.UpdateSetting(Context.Guild.Id, configSettings =>
				{
					configSettings.AuthChannelId = null;
				});
			}
			catch (Exception ex)
			{
				Logger.Print($"서버 {Context.Guild.Id} 규칙 채널 삭제 중 오류 발생: {ex.Message}", LogType.ERROR);
				await FollowupAsync($"채널 삭제 중 오류가 발생했습니다: {ex.Message}", ephemeral: true);
			}

			await FollowupAsync("사용자 인증 채널 제거 완료!", ephemeral: true);
		}
	}
}