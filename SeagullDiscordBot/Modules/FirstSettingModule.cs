using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace SeagullDiscordBot.Modules
{
	// 새로운 사용자 환영 기능을 담당하는 모듈
	public class FirstSettingModule : InteractionModuleBase<SocketInteractionContext>
	{
		//봇이 처음 서버에 입장하고 나서 서버 초기설정을 하는 모듈
		//1. 새로운 맴버 룰 추가
		//2. 기존의 모든 맴버 룰 변경
		//3. 기존의 모든 채널 룰 변경
		//4. 규칙 & 인증 채널 생성
		//5. 규칙 & 인증 채널에 캡챠 혹은 이모티콘 클릭으로 인증받는 메시지 추가
		//6. 캡챠 혹은 이모티콘 클릭하면 인증받고 맴버의 룰 변경 
		//7. 관리자 전용 봇 조작 채널 추가

		// 기본 슬래시 명령어 정의
		[SlashCommand("first_setting", "첫 설정을 합니다.")]
		[RequireUserPermission(GuildPermission.Administrator)] // 관리자 권한이 있는 사용자만 사용 가능
		public async Task FirstSettingCommand()
		{
			await Context.Interaction.RespondWithModalAsync<FirstSettingModal>("food_menu");

			// 로그 남기기
			Logger.Print($"'{Context.User.Username}'님이 hello 명령어를 사용했습니다.");
		}

		// Responds to the modal.
		[ModalInteraction("food_menu")]
		public async Task ModalResponse(FirstSettingModal modal)
		{

			// Build the message to send.
			string message = $"New Channel {modal.ChannelName} on {modal.Category}";

			// Specify the AllowedMentions so we don't actually ping everyone.
			AllowedMentions mentions = new();
			mentions.AllowedTypes = AllowedMentionTypes.Users;

			// Respond to the modal.
			await RespondAsync(message, allowedMentions: mentions, ephemeral: true);
		}
	}

	// Defines the modal that will be sent.
	public class FirstSettingModal : IModal
	{
		public string Title => "첫 설정";
		// Strings with the ModalTextInput attribute will automatically become components.
		[InputLabel("채널명")]
		[ModalTextInput("new_channel_name", placeholder: "환영 & 규칙", maxLength: 20)]
		public string ChannelName { get; set; }

		//[RequiredInput(false)]
		[InputLabel("카테고리")]
		[ModalTextInput("category", TextInputStyle.Paragraph, "Test", maxLength: 500)]
		public string Category { get; set; }

	}
}
