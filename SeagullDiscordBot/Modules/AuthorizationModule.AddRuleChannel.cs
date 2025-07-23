using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System.Threading.Tasks;
using SeagullDiscordBot.Services;
using System;
using System.Linq;
using System.IO;

namespace SeagullDiscordBot.Modules
{
	public partial class AuthorizationModule : InteractionModuleBase<SocketInteractionContext>
	{
		private readonly CaptchaService _captchaService = new CaptchaService();

		// 규칙 채널 추가 버튼 클릭 시 실행될 메서드
		[ComponentInteraction("add_rule_channel_button")]
		public async Task AddRuleChannelButton()
		{
			await RespondAsync("규칙 안내 및 인증 채널을 추가합니다...\n완료 메시지가 나타날때까지 기다려주세요.", ephemeral: true);
			// 규칙 채널 추가 기능 구현
			Logger.Print($"'{Context.User.Username}'님이 규칙 채널 추가 버튼을 클릭했습니다.");

			// 채널 서비스를 통해 텍스트 채널 생성 (결과 객체 반환)
			ChannelService channelService = new ChannelService();
			var result = await channelService.CreateTextChannelWithResultAsync(
				Context.Guild,
				"서버 규칙 & 인증",
				Context.User.Username,
				"갈매기"
			);

			if (result.Success)
			{
				// everyone 역할에 대해 대부분의 권한을 거부하여 읽기만 가능하게 설정
				var restrictedPermissions = new OverwritePermissions(
				sendMessages: PermValue.Deny,         // 메시지 전송 거부
				sendTTSMessages: PermValue.Deny,      // TTS 메시지 거부
				embedLinks: PermValue.Deny,           // 링크 임베드 거부
				attachFiles: PermValue.Deny,          // 파일 첨부 거부
				mentionEveryone: PermValue.Deny,      // @everyone 멘션 거부
				useExternalEmojis: PermValue.Deny,    // 외부 이모지 사용 거부
				useExternalStickers: PermValue.Deny,  // 외부 스티커 사용 거부
				addReactions: PermValue.Deny,         // 반응 추가 거부
				sendMessagesInThreads: PermValue.Deny, // 쓰레드 메시지 전송 거부
				createPublicThreads: PermValue.Deny,  // 공개 쓰레드 생성 거부
				createPrivateThreads: PermValue.Deny // 비공개 쓰레드 생성 거부
				);

				var botPermissions = new OverwritePermissions(
				sendMessages: PermValue.Allow,         // 메시지 전송 거부
				//sendTTSMessages: PermValue.Deny      // TTS 메시지 거부
				embedLinks: PermValue.Allow,           // 링크 임베드 거부
				attachFiles: PermValue.Allow,          // 파일 첨부 거부
				mentionEveryone: PermValue.Allow,      // @everyone 멘션 거부
				useExternalEmojis: PermValue.Allow,    // 외부 이모지 사용 거부
				useExternalStickers: PermValue.Allow,  // 외부 스티커 사용 거부
				addReactions: PermValue.Allow,         // 반응 추가 거부
				sendMessagesInThreads: PermValue.Allow, // 쓰레드 메시지 전송 거부
				createPublicThreads: PermValue.Allow,  // 공개 쓰레드 생성 거부
				createPrivateThreads: PermValue.Allow // 비공개 쓰레드 생성 거부
				);


				try
				{
					var everyoneRole = Context.Guild.EveryoneRole;
					await result.Channel.AddPermissionOverwriteAsync(everyoneRole, restrictedPermissions);

					var botRole = Context.Guild.Roles.FirstOrDefault(r => r.Name == "갈매기봇");
					await result.Channel.AddPermissionOverwriteAsync(botRole, botPermissions);

					Config.Settings.AuthChannelId = result.Channel.Id;
					Config.SaveSettings();
				}
				catch (Exception ex)
				{
					Logger.Print($"채널 권한 설정 중 오류 발생: {ex.Message}", LogType.ERROR);
					await FollowupAsync($"채널 권한 설정 중 오류가 발생했습니다: {ex.Message}", ephemeral: true);
					return;
				}

				// 성공 메시지 전송
				await FollowupAsync(result.Message, ephemeral: true);

				await Task.Delay(1000); // 1초 대기

				// 생성된 채널에 규칙 메시지 전송
				var embed = new EmbedBuilder()
					.WithColor(Color.Blue)
					.WithTitle("📜 서버 규칙 안내")
					.WithDescription("이 채널은 서버 규칙을 안내하고 사용자 인증을 위한 채널입니다.\n아래 규칙을 확인해주세요.")
					.AddField("1", "문의 채널에 잡담 금지")
					.AddField("2", "문의 답변에 말로 대답하지 말고 이모지 사용")
					.WithFooter(footer => footer.Text = "인증 후에 메시지 보내기 권한이 활성화 됩니다. 밑에 '인증하기' 버튼을 눌러주세요.")
					.WithCurrentTimestamp()
					.Build();

				// 인증 버튼 추가
				var button = new ComponentBuilder()
					.WithButton("인증하기1", "non_verify_user_button0", ButtonStyle.Success, emote: new Emoji("✅"))
					.WithButton("인증하기2", "non_verify_user_button1", ButtonStyle.Primary, emote: new Emoji("✅"))
					.WithButton("인증하기3", "verify_user_button", ButtonStyle.Danger, emote: new Emoji("✅"))
					.WithButton("인증하기4", "non_verify_user_button2", ButtonStyle.Secondary, emote: new Emoji("✅"));

				// 생성된 채널에 메시지 전송

				await Task.Delay(1000); // 1초 대기
				await result.Channel.SendMessageAsync(embed: embed);
				await result.Channel.SendMessageAsync("아래 버튼을 클릭하여 인증을 완료하세요:", components: button.Build());
				//await result.Channel.SendMessageAsync("Test",embed: embed, components: button.Build());
			}
			else
			{
				// 오류 발생 시 처리
				await FollowupAsync($"채널 생성 중 오류가 발생했습니다: {result.ErrorMessage}", ephemeral: true);
			}

			await FollowupAsync("규칙 안내 및 인증 채널을 추가 완료!", ephemeral: true);
		}

		// 채널 권한 수정 버튼 클릭 시 실행될 메서드
		[ComponentInteraction("verify_user_button")]
		public async Task VerifyUserButton()
		{
			Logger.Print($"'{Context.User.Username}'님이 캡챠 인증을 시작했습니다.");

			try
			{
				// Config.Settings.AutoRoleId가 설정되어 있는지 확인
				if (Config.Settings.AutoRoleId == null)
				{
					await RespondAsync("갈매기 역할이 설정되지 않았습니다. 관리자에게 문의하세요.", ephemeral: true);
					return;
				}

				// 사용자가 이미 갈매기 역할을 가지고 있는지 확인
				var guildUser = Context.User as SocketGuildUser;
				if (guildUser == null)
				{
					await RespondAsync("사용자 정보를 가져올 수 없습니다.", ephemeral: true);
					return;
				}

				// 사용자가 이미 갈매기 역할을 가지고 있는지 확인
				var userSeagullRole = guildUser.Roles.FirstOrDefault(r => r.Id == Config.Settings.AutoRoleId);
				if (userSeagullRole != null)  // ✅ 역할이 있으면
				{
					await RespondAsync("이미 인증된 사용자입니다.", ephemeral: true);
					return;
				}

				await DeferAsync(ephemeral: true);

				// 이미지 캡차 생성
				var captchaImageData = await _captchaService.GenerateCaptchaAsync(Context.User.Id);

				// 캡차 이미지와 함께 모달 버튼 표시
				var embed = new EmbedBuilder()
					.WithColor(Color.Orange)
					.WithTitle("🔐 사용자 인증")
					.WithDescription("아래 이미지에 표시된 문자를 정확히 입력해주세요.\n**(대소문자 구분 없음)**")
					.WithImageUrl("attachment://captcha.jpg")
					.WithFooter("5분 내에 인증을 완료해주세요. 최대 3회까지 시도할 수 있습니다.")
					.WithCurrentTimestamp()
					.Build();

				var button = new ComponentBuilder()
					.WithButton("답변 입력", "captcha_input_button", ButtonStyle.Primary, emote: new Emoji("✏️"));

				// 이미지를 스트림으로 전송
				using var stream = new MemoryStream(captchaImageData);
				await FollowupWithFileAsync(stream, "captcha.jpg", embed: embed, components: button.Build(), ephemeral: true);
			}
			catch (Exception ex)
			{
				Logger.Print($"인증 처리 중 오류 발생: {ex.Message}", LogType.ERROR);
				await FollowupAsync("인증 처리 중 오류가 발생했습니다. 잠시 후 다시 시도해주세요.", ephemeral: true);
			}
		}

		// 캡차 입력 버튼 클릭 시 모달 표시
		[ComponentInteraction("captcha_input_button")]
		public async Task CaptchaInputButton()
		{
			var modal = new ModalBuilder()
				.WithTitle("사용자 인증 - 답변 입력")
				.WithCustomId("captcha_modal")
				.AddTextInput("이미지에 표시된 문자를 입력하세요", "captcha_answer", TextInputStyle.Short, 
					"예: ABC123", maxLength: 10, required: true);

			await RespondWithModalAsync(modal.Build());
		}

		// 캡차 모달 제출 처리
		[ModalInteraction("captcha_modal")]
		public async Task HandleCaptchaModal(CaptchaModal modal)
		{
			await DeferAsync(ephemeral: true);

			try
			{
				// 캡차 검증
				var verificationResult = _captchaService.VerifyCaptcha(Context.User.Id, modal.Answer);

				if (verificationResult.IsSuccess)
				{
					// 인증 성공 - 갈매기 역할 부여
					var seagullRole = Context.Guild.Roles.FirstOrDefault(r => r.Id == Config.Settings.AutoRoleId);
					
					if (seagullRole == null)
					{
						await FollowupAsync($"갈매기 역할이 존재하지 않아 인증을 진행할 수 없습니다. 관리자에게 문의하십시오.", ephemeral: true);
						Logger.Print($"'{Context.User.Username}'님이 인증을 시도했지만 갈매기 역할이 존재하지 않아 실패했습니다.", LogType.WARNING);
					}

					// 사용자에게 역할 부여
					if (Context.User is SocketGuildUser guildUser)
					{
						var addRoleResult = await _roleService.AddRoleToUserAsync(guildUser, seagullRole, "갈매기봇");
						
						if (addRoleResult.Success)
						{
							var successEmbed = new EmbedBuilder()
								.WithColor(Color.Green)
								.WithTitle("✅ 인증 완료")
								.WithDescription("사용자 인증이 성공적으로 완료되었습니다!")
								.WithCurrentTimestamp()
								.Build();

							await FollowupAsync(embed: successEmbed, ephemeral: true);
							
							Logger.Print($"'{Context.User.Username}'님이 캡차 인증에 성공하여 갈매기 역할을 부여받았습니다.");
						}
						else
						{
							await FollowupAsync($"역할 부여에 실패했습니다: {addRoleResult.ErrorMessage}", ephemeral: true);
						}
					}
					else
					{
						await FollowupAsync("사용자 정보를 가져올 수 없습니다.", ephemeral: true);
					}
				}
				else
				{
					// 인증 실패 - 새로운 이미지와 함께 실패 메시지 표시
					var failEmbed = new EmbedBuilder()
						.WithColor(Color.Red)
						.WithTitle("❌ 인증 실패")
						.WithDescription(verificationResult.Message)
						.WithCurrentTimestamp();

					//// 재시도 가능한 경우 새로운 캡차 이미지 표시
					//if (verificationResult.RetryImageData != null)
					//{
					//	failEmbed.WithImageUrl("attachment://captcha_retry.jpg");
					//	failEmbed.WithDescription($"{verificationResult.Message}\n\n**다시 시도해주세요.**");

					//	var retryButton = new ComponentBuilder()
					//		.WithButton("다시 입력", "captcha_input_button", ButtonStyle.Primary, emote: new Emoji("🔄"));

					//	using var stream = new MemoryStream(verificationResult.RetryImageData);
					//	await FollowupWithFileAsync(stream, "captcha_retry.jpg", embed: failEmbed.Build(), components: retryButton.Build(), ephemeral: true);
					//}

					if (verificationResult.Retry)
					{
						failEmbed.WithDescription(verificationResult.Message);
						await FollowupAsync(embed: failEmbed.Build(), ephemeral: true);
					}
					else
					{
						// 최대 시도 횟수 초과 또는 시간 초과
						await FollowupAsync(embed: failEmbed.Build(), ephemeral: true);
					}
					
					Logger.Print($"'{Context.User.Username}'님의 캡차 인증 실패: {verificationResult.Message}");
				}
			}
			catch (Exception ex)
			{
				Logger.Print($"캡차 모달 처리 중 오류 발생: {ex.Message}", LogType.ERROR);
				await FollowupAsync("인증 처리 중 오류가 발생했습니다. 잠시 후 다시 시도해주세요.", ephemeral: true);
			}
		}


		// 채널 권한 수정 버튼 클릭 시 실행될 메서드
		[ComponentInteraction("non_verify_user_button0")]
		public async Task NonVerifyUserButton()
		{
			// 채널 권한 수정 기능 구현
			Logger.Print($"'{Context.User.Username}'님이 비인증 버튼을 클릭했습니다.");

			await RespondAsync("인증 불가", ephemeral: true);
		}
		[ComponentInteraction("non_verify_user_button1")]
		public async Task NonVerifyUserButton1()
		{
			// 채널 권한 수정 기능 구현
			Logger.Print($"'{Context.User.Username}'님이 비인증 버튼을 클릭했습니다.");

			await RespondAsync("인증 불가", ephemeral: true);
		}
		[ComponentInteraction("non_verify_user_button2")]
		public async Task NonVerifyUserButton2()
		{
			// 채널 권한 수정 기능 구현
			Logger.Print($"'{Context.User.Username}'님이 비인증 버튼을 클릭했습니다.");

			await RespondAsync("인증 불가", ephemeral: true);
		}
	}
}