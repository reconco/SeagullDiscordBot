using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using SeagullDiscordBot.Services;
using System;
using System.Threading.Tasks;

namespace SeagullDiscordBot.Modules
{
	// 새로운 채널 생성 기능을 담당하는 모듈
	public class CreateNewChannelModule : InteractionModuleBase<SocketInteractionContext>
	{
		private readonly ChannelService _channelService;

		const string PublicChannelName = "문의";
		const string AdminChannelName = "공지";

		public CreateNewChannelModule()
		{
			_channelService = new ChannelService();
		}

		// 텍스트 채널 생성 명령어
		[SlashCommand("create_text_channel", "새로운 텍스트 채널을 생성합니다.\n'문의'채널과 동일한 권한과 규칙으로 생성합니다")]
		[RequireUserPermission(GuildPermission.Administrator)] // 관리자 권한이 있는 사용자만 사용 가능
		public async Task CreateTextChannelCommand(
			[Summary("name", "생성할 채널 이름")] string channelName,
			[Summary("category", "채널이 속할 카테고리 이름 (선택 사항)")] string? categoryName = null)
		{
			await DeferAsync(ephemeral: true);

			var everyoneRole = Context.Guild.EveryoneRole;

			SocketGuildChannel targetChannel = FindChannelByName(Context, PublicChannelName);
			if(targetChannel == null)
			{
				await FollowupAsync($"권한을 가져올 '{PublicChannelName}' 채널을 찾을 수 없습니다.", ephemeral: true);
				Logger.Print($"권한을 가져올 '{PublicChannelName}' 채널을 찾을 수 없습니다.");
				return;
			}

			var permissions = targetChannel.PermissionOverwrites.ToList();

			// 채널 서비스를 통해 텍스트 채널 생성 (결과 객체 반환)
			var result = await _channelService.CreateTextChannelWithResultAsync(
				Context.Guild,
				channelName,
				Context.User.Username,
				categoryName
			);

			if (result.Success)
			{
				foreach (var permission in permissions)
				{
					await result.Channel.AddPermissionOverwriteAsync(everyoneRole, permission.Permissions);
				}

				// 성공 메시지 전송
				await FollowupAsync(result.Message, ephemeral: true);
			}
			else
			{
				// 오류 발생 시 처리
				await FollowupAsync($"채널 생성 중 오류가 발생했습니다: {result.ErrorMessage}", ephemeral: true);
			}

			await FollowupAsync($"'{channelName}'채널 생성 완료!", ephemeral: true);

			// 로그 남기기
			Logger.Print($"'{Context.User.Username}'님이 '{channelName}'채널을 생성였습니다.");
		}

		// 텍스트 채널 생성 명령어
		[SlashCommand("create_admin_text_channel", "새로운 텍스트 채널을 생성합니다.\n'공지'채널과 동일한 권한과 규칙으로 생성합니다")]
		[RequireUserPermission(GuildPermission.Administrator)] // 관리자 권한이 있는 사용자만 사용 가능
		public async Task CreateAdminTextChannelCommand(
			[Summary("name", "생성할 채널 이름")] string channelName,
			[Summary("category", "채널이 속할 카테고리 이름 (선택 사항)")] string? categoryName = null)
		{
			await DeferAsync(ephemeral: true);

			var everyoneRole = Context.Guild.EveryoneRole;

			SocketGuildChannel targetChannel = FindChannelByName(Context, AdminChannelName);
			if (targetChannel == null)
			{
				await FollowupAsync($"권한을 가져올 '{AdminChannelName}' 채널을 찾을 수 없습니다.", ephemeral: true);
				Logger.Print($"권한을 가져올 '{AdminChannelName}' 채널을 찾을 수 없습니다.");
				return;
			}

			var permissions = targetChannel.PermissionOverwrites.ToList();

			// 채널 서비스를 통해 텍스트 채널 생성 (결과 객체 반환)
			var result = await _channelService.CreateTextChannelWithResultAsync(
				Context.Guild,
				channelName,
				Context.User.Username,
				categoryName
			);

			if (result.Success)
			{
				foreach (var permission in permissions)
				{
					await result.Channel.AddPermissionOverwriteAsync(everyoneRole, permission.Permissions);
				}

				// 성공 메시지 전송
				await FollowupAsync(result.Message, ephemeral: true);
			}
			else
			{
				// 오류 발생 시 처리
				await FollowupAsync($"채널 생성 중 오류가 발생했습니다: {result.ErrorMessage}", ephemeral: true);
			}

			await FollowupAsync($"'{channelName}'채널 생성 완료!", ephemeral: true);

			// 로그 남기기
			Logger.Print($"'{Context.User.Username}'님이 '{channelName}'채널을 생성였습니다.");
		}

		private List<SocketGuildChannel> GetChannelList(SocketInteractionContext socketInteractionContext)
		{
			return socketInteractionContext.Guild.Channels.ToList();
		}

		private SocketGuildChannel FindChannelByName(SocketInteractionContext socketInteractionContext, string channelName)
		{
			var channels = GetChannelList(socketInteractionContext);
			return channels.FirstOrDefault(c => c.Name.Equals(channelName, StringComparison.OrdinalIgnoreCase));
		}
	}
}
