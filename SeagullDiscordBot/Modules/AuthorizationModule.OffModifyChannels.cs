using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SeagullDiscordBot.Services;

//AI코드 확인 완료(테스트 필요)
namespace SeagullDiscordBot.Modules
{
	public partial class AuthorizationModule : InteractionModuleBase<SocketInteractionContext>
	{
		// 채널 권한 원복 버튼 클릭 시 실행될 메서드 (authorization_off 1번)
		[ComponentInteraction("auth_off_modify_channel_button")]
		public async Task AuthOffModifyChannelButton()
		{
			await RespondAsync("기존 채널들의 갈매기 권한을 제거하고, 갈매기 역할의 메시지 전송 권한이 유효했던 채널에서 everyone 역할에게 메시지 전송 권한을 복구합니다...\n완료 메시지가 나타날때까지 기다려주세요.", ephemeral: true);
			
			Logger.Print($"서버 '{Context.Guild.Name}'({Context.Guild.Id})에서 '{Context.User.Username}'님이 채널 권한 원복 버튼을 클릭했습니다.");

			var everyoneRole = Context.Guild.EveryoneRole;
			
			// 현재 서버의 설정 가져오기
			var settings = Config.GetSettings(Context.Guild.Id);
			
			// 인증된 사용자 역할을 가져옴
			var verifiedRole = Context.Guild.Roles.FirstOrDefault(r => r.Id == settings.AutoRoleId);
			if (verifiedRole == null)
			{
				await FollowupAsync("갈매기 역할을 찾을 수 없습니다. 이미 삭제되었거나 설정되지 않았을 수 있습니다. 작업을 종료합니다.", ephemeral: true);
				Logger.Print("갈매기 역할을 찾을 수 없어 채널 권한 원복을 종료합니다.", LogType.WARNING);
				return;
			}

			List<SocketGuildChannel> channels = Context.Guild.Channels.ToList();

			foreach (var channel in channels)
			{
				if (channel is ITextChannel textChannel)
				{
					// 갈매기 역할의 기존 권한을 가져옴 (null이면 기본값 사용)
					var permissionsNullable = textChannel.GetPermissionOverwrite(verifiedRole);
					if (permissionsNullable.HasValue)
					{
						var permissions = permissionsNullable.Value;
						// 권한이 설정되어 있는 경우의 처리
					}
					else
					{
						// 권한이 설정되어 있지 않은 경우의 처리
						continue;
					}

					PermValue sendMsg = permissionsNullable.Value.SendMessages;

					if (sendMsg == PermValue.Allow || sendMsg == PermValue.Inherit)
					{
						// everyone : 메시지 전송 가능하도록 설정
						var everyonePermissions = CreatePermissionsWithSendMessages(permissionsNullable.Value, sendMsg);
						await textChannel.AddPermissionOverwriteAsync(everyoneRole, everyonePermissions);

						await textChannel.RemovePermissionOverwriteAsync(verifiedRole);
					}

					Logger.Print($"'{textChannel.Name}' 채널에 everyone 메시지 전송 허용");
				}
				else if (channel is IVoiceChannel voiceChannel)
				{
					// 갈매기 역할의 기존 권한을 가져옴 (null이면 기본값 사용)
					var permissionsNullable = voiceChannel.GetPermissionOverwrite(verifiedRole);
					if (permissionsNullable.HasValue)
					{
						var permissions = permissionsNullable.Value;
						// 권한이 설정되어 있는 경우의 처리
					}
					else
					{
						// 권한이 설정되어 있지 않은 경우의 처리
						continue;
					}

					PermValue sendMsg = permissionsNullable.Value.SendMessages;

					if (sendMsg == PermValue.Allow || sendMsg == PermValue.Inherit)
					{
						// everyone : 메시지 전송 가능하도록 설정
						var everyonePermissions = CreatePermissionsWithSendMessages(permissionsNullable.Value, sendMsg);
						await voiceChannel.AddPermissionOverwriteAsync(everyoneRole, everyonePermissions);

						await voiceChannel.RemovePermissionOverwriteAsync(verifiedRole);
					}

					Logger.Print($"'{voiceChannel.Name}' 음성 채널에 everyone 메시지 전송 허용");
				}
			}

			await FollowupAsync($"기존 채널들의 권한 변경 완료! (활동 가능 채널에서 Everyone 역할: 메시지 전송 허용)", ephemeral: true);
			Logger.Print($"총 모든 채널의 권한이 수정되었습니다. Everyone 메시지 전송 허용");
		}
	}
}