using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System.Threading.Tasks;
using SeagullDiscordBot.Services;
using System.Linq;
using System;

namespace SeagullDiscordBot.Modules
{
	public class NewAuthMessageModal : IModal
	{
		public string Title => "사용자 인증 채널 안내 메시지 변경";

		[InputLabel("새로운 안내 메시지")]
		[ModalTextInput("new_auth_message_inputed", TextInputStyle.Paragraph, "여기에 새로운 안내 메시지를 입력하세요.", maxLength: 2000)]
		public string NewMessage { get; set; }
	}

	public partial class NewAuthorizationMessageModule : InteractionModuleBase<SocketInteractionContext>
	{
		// 기본 슬래시 명령어 정의
		[SlashCommand("change_authorization_message", "사용자 인증 채널의 안내 문구 메시지를 변경합니다")]
		[RequireUserPermission(GuildPermission.Administrator)] // 관리자 권한이 있는 사용자만 사용 가능
		public async Task ChangeAuthorizationMessageOnCommand()
		{
			// 버튼 컴포넌트 생성
			var builder = new ComponentBuilder()
				.WithButton("사용자 인증 채널 안내 메시지 변경", "new_auth_message_change_button", ButtonStyle.Primary, emote: new Emoji("✏️"));
			await RespondAsync("사용자 인증 채널의 안내 문구 메시지를 변경합니다.\n", components: builder.Build(), ephemeral: true);

			// 로그 남기기
			Logger.Print($"'{Context.User.Username}'님이 change_authorization_message 명령어를 사용했습니다.");
		}

		// 사용자 인증 채널 안내 메시지 변경 버튼 클릭 시 모달로 메시지 입력 받기
		[ComponentInteraction("new_auth_message_change_button")]
		public async Task ChangeAuthorizationMessageButton()
		{
			Logger.Print($"'{Context.User.Username}'님이 사용자 인증 채널 안내 메시지 변경 버튼을 클릭했습니다.");
			// 모달 생성
			var modal = new ModalBuilder()
				.WithTitle("사용자 인증 채널 안내 메시지 변경")
				.WithCustomId("new_auth_message_modal")
				.AddTextInput("새로운 안내 메시지", "new_auth_message_inputed", TextInputStyle.Paragraph, placeholder: "여기에 새로운 안내 메시지를 입력하세요.", maxLength: 2000)
				.Build();
			await RespondWithModalAsync(modal);
		}

		// 모달 제출 시 처리
		[ModalInteraction("new_auth_message_modal")]
		public async Task MakeNewAuthorizationMessageModal(NewAuthMessageModal modal)
		{
			await DeferAsync(ephemeral: true);

			try
			{
				// 인증 채널 ID가 설정되어 있는지 확인
				if (Config.Settings.AuthChannelId == null)
				{
					await FollowupAsync("인증 채널이 설정되지 않았습니다. 먼저 인증 채널을 생성해주세요.", ephemeral: true);
					Logger.Print($"'{Context.User.Username}'님이 인증 메시지 변경을 시도했지만 인증 채널이 설정되지 않았습니다.", LogType.WARNING);
					return;
				}

				// 인증 채널 가져오기
				var authChannel = Context.Guild.GetChannel(Config.Settings.AuthChannelId.Value) as ITextChannel;
				if (authChannel == null)
				{
					await FollowupAsync("인증 채널을 찾을 수 없습니다. 채널이 삭제되었거나 봇이 접근할 수 없습니다.", ephemeral: true);
					Logger.Print($"'{Context.User.Username}'님이 인증 메시지 변경을 시도했지만 인증 채널(ID: {Config.Settings.AuthChannelId})을 찾을 수 없습니다.", LogType.ERROR);
					return;
				}

				// 모달에서 입력받은 새로운 메시지
				string newMessage = modal.NewMessage;

				// 기존 메시지들 삭제 (봇이 보낸 메시지만)
				var messages = await authChannel.GetMessagesAsync(50).FlattenAsync();
				var botMessages = messages.Where(m => m.Author.Id == Context.Client.CurrentUser.Id);

				foreach (var message in botMessages)
				{
					try
					{
						await message.DeleteAsync();
						await Task.Delay(100); // API 요청 제한 방지를 위한 짧은 지연
					}
					catch (Exception ex)
					{
						Logger.Print($"메시지 삭제 중 오류 발생: {ex.Message}", LogType.WARNING);
					}
				}

				await Task.Delay(1000); // 1초 대기



				// 인증 메시지, 버튼 추가
				var embed = AuthorizationModule.CreateAuthorizationEmbed(newMessage);
				var button = AuthorizationModule.CreateAuthorizationButton();

				// 새로운 메시지 전송
				await authChannel.SendMessageAsync(embed: embed);
				await authChannel.SendMessageAsync("아래 버튼을 클릭하여 인증을 완료하세요:", components: button.Build());

				// 성공 메시지
				await FollowupAsync($"인증 채널의 안내 메시지가 성공적으로 변경되었습니다.\n채널: {authChannel.Name}", ephemeral: true);
				
				// 로그 남기기
				Logger.Print($"'{Context.User.Username}'님이 인증 채널의 안내 메시지를 변경했습니다.");
			}
			catch (Exception ex)
			{
				Logger.Print($"인증 메시지 변경 중 오류 발생: {ex.Message}", LogType.ERROR);
				await FollowupAsync($"인증 메시지 변경 중 오류가 발생했습니다: {ex.Message}", ephemeral: true);
			}
		}
	}
}
