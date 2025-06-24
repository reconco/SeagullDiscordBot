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

		public CreateNewChannelModule()
		{
			_channelService = new ChannelService();
		}

		// 텍스트 채널 생성 명령어
		[SlashCommand("create_text_channel", "새로운 텍스트 채널을 생성합니다.")]
		[RequireUserPermission(GuildPermission.Administrator)] // 관리자 권한이 있는 사용자만 사용 가능
		public async Task CreateTextChannelCommand(
			[Summary("name", "생성할 채널 이름")] string channelName,
			[Summary("category", "채널이 속할 카테고리 이름 (선택 사항)")] string? categoryName = null)
		{
			await DeferAsync(ephemeral: true);
			
			// 채널 서비스를 통해 텍스트 채널 생성 (결과 객체 반환)
			var result = await _channelService.CreateTextChannelWithResultAsync(
				Context.Guild, 
				channelName, 
				Context.User.Username, 
				categoryName
			);
			
			if (result.Success)
			{
				// 성공 메시지 전송
				await FollowupAsync(result.Message, ephemeral: true);
			}
			else
			{
				// 오류 발생 시 처리
				await FollowupAsync($"채널 생성 중 오류가 발생했습니다: {result.ErrorMessage}", ephemeral: true);
			}
		}
	}
}
