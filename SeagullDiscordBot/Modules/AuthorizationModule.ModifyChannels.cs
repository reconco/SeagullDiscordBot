using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SeagullDiscordBot.Services;

namespace SeagullDiscordBot.Modules
{
	public partial class AuthorizationModule : InteractionModuleBase<SocketInteractionContext>
	{
		// 채널 권한 수정 버튼 클릭 시 실행될 메서드
		[ComponentInteraction("modify_channel_button")]
		public async Task ModifyChannelButton()
		{
			await RespondAsync("기존 채널들의 권한을 수정합니다...\n완료 메시지가 나타날때까지 기다려주세요.", ephemeral: true);
			
			Logger.Print($"서버 '{Context.Guild.Name}'({Context.Guild.Id})에서 '{Context.User.Username}'님이 채널 권한 수정 버튼을 클릭했습니다.");

			var everyoneRole = Context.Guild.EveryoneRole;
			
			// 현재 서버의 설정 가져오기
			var settings = Config.GetSettings(Context.Guild.Id);
			
			// 인증된 사용자 역할을 가져옴
			var verifiedRole = Context.Guild.Roles.FirstOrDefault(r => r.Id == settings.AutoRoleId);
			if (verifiedRole == null)
			{
				await FollowupAsync("인증된 사용자 역할을 찾을 수 없습니다.", ephemeral: true);
				return;
			}

			List<SocketGuildChannel> channels = Context.Guild.Channels.ToList();
			
			foreach (var channel in channels)
			{
				if (channel is ITextChannel textChannel)
				{
					// everyone 역할의 기존 권한을 가져옴 (null이면 기본값 사용)
					var basePermissions = textChannel.GetPermissionOverwrite(everyoneRole)
						.GetValueOrDefault(OverwritePermissions.InheritAll);

					PermValue sendMsg = basePermissions.SendMessages;

					if (sendMsg == PermValue.Allow || sendMsg == PermValue.Inherit)
					{
						// verifiedRole: 기존 권한에서 메시지 전송만 허용으로 변경  
						var verifiedPermissions = CreatePermissionsWithSendMessages(basePermissions, sendMsg);
						await textChannel.AddPermissionOverwriteAsync(verifiedRole, verifiedPermissions);
					
						// everyone 역할: 기존 권한에서 메시지 전송만 거부로 변경
						var everyonePermissions = CreatePermissionsWithSendMessages(basePermissions, PermValue.Deny);
						await textChannel.AddPermissionOverwriteAsync(everyoneRole, everyonePermissions);
					}

					Logger.Print($"'{textChannel.Name}' 채널에 everyone 메시지 전송 거부, '{verifiedRole.Name}' 역할 권한 설정 완료");
				}
				else if (channel is IVoiceChannel voiceChannel)
				{
					// 음성 채널: everyone 권한을 verifiedRole에 복사
					var basePermissions = voiceChannel.GetPermissionOverwrite(everyoneRole)
						.GetValueOrDefault(OverwritePermissions.InheritAll);

					PermValue sendMsg = basePermissions.SendMessages;

					if (sendMsg == PermValue.Allow || sendMsg == PermValue.Inherit)
					{
						// everyone 역할: 기존 권한에서 메시지 전송만 거부로 변경
						var everyonePermissions = CreatePermissionsWithSendMessages(basePermissions, PermValue.Deny);
						await voiceChannel.AddPermissionOverwriteAsync(everyoneRole, everyonePermissions);

						// verifiedRole: 기존 권한에서 메시지 전송만 허용으로 변경  
						var verifiedPermissions = CreatePermissionsWithSendMessages(basePermissions, sendMsg);
						await voiceChannel.AddPermissionOverwriteAsync(verifiedRole, verifiedPermissions);
					}
					
					Logger.Print($"'{voiceChannel.Name}' 음성 채널에 '{verifiedRole.Name}' 역할 권한이 everyone과 동일하게 설정되었습니다.");
				}
			}

			await FollowupAsync($"기존 채널들의 권한 변경 완료! (활동 가능 채널에서 Everyone 역할: 메시지 전송 불가, {verifiedRole.Name} 역할: 메시지 전송 허용)", ephemeral: true);
			Logger.Print($"총 모든 채널의 권한이 수정되었습니다. Everyone 메시지 전송 불가, {verifiedRole.Name} 메시지 전송 허용");
		}

		/// <summary>
		/// 기존 권한에서 SendMessages 권한만 변경한 새로운 OverwritePermissions를 생성합니다.
		/// </summary>
		private static OverwritePermissions CreatePermissionsWithSendMessages(OverwritePermissions basePermissions, PermValue sendMessagesValue)
		{
			return new OverwritePermissions(
				createInstantInvite: basePermissions.CreateInstantInvite,
				manageChannel: basePermissions.ManageChannel,
				addReactions: basePermissions.AddReactions,
				viewChannel: basePermissions.ViewChannel,
				sendMessages: sendMessagesValue, // 이 권한만 변경
				sendTTSMessages: basePermissions.SendTTSMessages,
				manageMessages: basePermissions.ManageMessages,
				embedLinks: basePermissions.EmbedLinks,
				attachFiles: basePermissions.AttachFiles,
				readMessageHistory: basePermissions.ReadMessageHistory,
				mentionEveryone: basePermissions.MentionEveryone,
				useExternalEmojis: basePermissions.UseExternalEmojis,
				useExternalStickers: basePermissions.UseExternalStickers,
				sendMessagesInThreads: basePermissions.SendMessagesInThreads,
				createPublicThreads: basePermissions.CreatePublicThreads,
				createPrivateThreads: basePermissions.CreatePrivateThreads,
				useApplicationCommands: basePermissions.UseApplicationCommands
			);
		}
	}
}