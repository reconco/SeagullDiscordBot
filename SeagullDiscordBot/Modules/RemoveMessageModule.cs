using Discord;
using Discord.Interactions;
using System.Threading.Tasks;

namespace SeagullDiscordBot.Modules
{
	// InteractionModuleBase를 상속받아 슬래시 명령어 모듈 생성
	public class RemoveMessageModule : InteractionModuleBase<SocketInteractionContext>
	{
		// 기본 슬래시 명령어 정의
		[SlashCommand("remove_user_messages", "현재 채널에 있는 특정 사용자의 모든 메시지를 삭제합니다.")]
		[RequireUserPermission(GuildPermission.Administrator)] // 관리자 권한이 있는 사용자만 사용 가능
		public async Task RemoveUserMessagesCommand([Summary(description: "메시지를 삭제할 사용자의 이름")] string username)
		{
			// 사용자 이름으로 일치하는 사용자를 서버에서 찾기
			var matchingUsers = Context.Guild.Users
				.Where(u => u.DisplayName.Equals(username, StringComparison.OrdinalIgnoreCase))
				.ToList();

			//// 닉네임으로 일치하는 사용자도 찾기
			//var nicknameMatchingUsers = Context.Guild.Users
			//	.Where(u => u.Nickname != null && u.Nickname.Equals(username, StringComparison.OrdinalIgnoreCase))
			//	.ToList();

			//matchingUsers.AddRange(nicknameMatchingUsers);

			if (!matchingUsers.Any())
			{
				await RespondAsync($"서버에서 '{username}' 사용자를 찾을 수 없습니다.", ephemeral: true);
				return;
			}

			// 사용자가 한 명만 있는 경우 바로 확인 버튼 표시
			if (matchingUsers.Count == 1)
			{
				var user = matchingUsers.First();
				var builder = new ComponentBuilder()
							.WithButton("메시지 삭제 시작", $"remove_user_messages_button:{user.Id}", ButtonStyle.Danger);

				await RespondAsync($"'{Context.Channel.Name}' 채널에서 '{user.Username}#{user.Discriminator}' 사용자의 모든 메시지를 삭제하시겠습니까?", components: builder.Build(), ephemeral: true);

				// 로그 남기기
				Logger.Print($"'{Context.User.Username}'님이 '{user.Username}' 사용자의 메시지 삭제 명령어를 사용했습니다.");
			}
			// 사용자가 여러 명인 경우 선택 메뉴 표시
			else
			{
				var selectBuilder = new SelectMenuBuilder()
					.WithPlaceholder("삭제할 메시지의 사용자를 선택하세요")
					.WithCustomId("user_selection_menu")
					.WithMinValues(1)
					.WithMaxValues(1);

				// 최대 25개까지 옵션 추가 가능 (Discord API 제한)
				foreach (var user in matchingUsers.Take(25))
				{
					selectBuilder.AddOption(
						$"{user.Username}#{user.Discriminator}",
						user.Id.ToString(),
						$"서버 닉네임: {user.Nickname ?? "없음"}"
					);
				}

				var builder = new ComponentBuilder()
					.WithSelectMenu(selectBuilder);

				await RespondAsync($"'{username}' 이름을 가진 사용자가 여러 명 있습니다. 메시지를 삭제할 사용자를 선택하세요:", components: builder.Build(), ephemeral: true);

				// 로그 남기기
				Logger.Print($"'{Context.User.Username}'님이 '{username}' 사용자 검색 시 여러 사용자가 발견되어 선택 메뉴를 표시했습니다.");
			}
		}
		// 사용자 선택 메뉴 핸들러
		[ComponentInteraction("user_selection_menu")]
		public async Task HandleUserSelection(string[] selectedValues)
		{
			if (selectedValues.Length == 0)
			{
				await RespondAsync("사용자를 선택하지 않았습니다.", ephemeral: true);
				return;
			}

			// 선택된 사용자 ID 가져오기
			string userId = selectedValues[0];

			// 해당 ID로 사용자 찾기
			var user = Context.Guild.GetUser(ulong.Parse(userId));
			if (user == null)
			{
				await RespondAsync("선택한 사용자를 찾을 수 없습니다.", ephemeral: true);
				return;
			}

			var builder = new ComponentBuilder()
				.WithButton("메시지 삭제 시작", $"remove_user_messages_button:{user.Id}", ButtonStyle.Danger);

			await RespondAsync($"'{Context.Channel.Name}' 채널에서 '{user.Username}#{user.Discriminator}' 사용자의 모든 메시지를 삭제하시겠습니까?", components: builder.Build(), ephemeral: true);

			// 로그 남기기
			Logger.Print($"'{Context.User.Username}'님이 선택 메뉴에서 '{user.Username}' 사용자를 선택했습니다.");
		}

		[ComponentInteraction("remove_user_messages_button:*")]
		public async Task RemoveUserMessagesButton(string userId)
		{
			// userId로 사용자 찾기
			var user = Context.Guild.GetUser(ulong.Parse(userId));
			if (user == null)
			{
				await RespondAsync("선택한 사용자를 찾을 수 없습니다.", ephemeral: true);
				return;
			}

			await RespondAsync($"'{Context.Channel.Name}' 채널에서 '{user.Username}#{user.Discriminator}' 사용자의 모든 메시지를 삭제합니다...\n완료 메시지가 나타날때까지 기다려주세요.", ephemeral: true);

			Logger.Print($"'{Context.User.Username}'님이 '{user.Username}' 사용자의 메시지 삭제 버튼을 클릭했습니다.");

			try
			{
				// 채널을 ITextChannel로 캐스팅
				var channel = Context.Channel as ITextChannel;
				if (channel == null)
				{
					await FollowupAsync("이 명령어는 텍스트 채널에서만 사용할 수 있습니다.", ephemeral: true);
					return;
				}

				// 삭제된 메시지 수 카운트
				int deletedCount = 0;
				int totalChecked = 0;
				bool hasMoreMessages = true;
				ulong? lastMessageId = null;

				// 메시지 삭제 작업 시작
				while (hasMoreMessages)
				{
					// 최대 100개 메시지를 한 번에 가져옴 (Discord API 제한)
					var messages = lastMessageId == null
						? await channel.GetMessagesAsync(100).FlattenAsync()
						: await channel.GetMessagesAsync(lastMessageId.Value, Direction.Before, 100).FlattenAsync();

					var messagesList = messages.ToList();

					if (messagesList.Count == 0)
					{
						// 더 이상 메시지가 없음
						hasMoreMessages = false;
					}
					else
					{
						// 마지막 메시지 ID 업데이트
						lastMessageId = messagesList.Last().Id;

						// 해당 사용자의 메시지만 필터링
						var userMessages = messagesList.Where(msg => msg.Author.Id == user.Id).ToList();
						totalChecked += userMessages.Count;

						if (userMessages.Any())
						{
							// 14일 이내의 메시지만 일괄 삭제 가능 (Discord API 제한)
							var recentMessages = userMessages
								.Where(msg => (DateTimeOffset.UtcNow - msg.Timestamp).TotalDays < 14)
								.ToList();

							var oldMessages = userMessages
								.Where(msg => (DateTimeOffset.UtcNow - msg.Timestamp).TotalDays >= 14)
								.ToList();

							// 최근 메시지(14일 이내) 일괄 삭제
							if (recentMessages.Count > 0)
							{
								await channel.DeleteMessagesAsync(recentMessages);
								deletedCount += recentMessages.Count;

								// 너무 빠른 요청으로 인한 API 제한 방지를 위한 딜레이
								await Task.Delay(1000);
							}

							// 오래된 메시지(14일 이상) 개별 삭제
							foreach (var message in oldMessages)
							{
								try
								{
									await message.DeleteAsync();
									deletedCount++;

									// 너무 빠른 요청으로 인한 API 제한 방지를 위한 딜레이
									await Task.Delay(1000);
								}
								catch (Exception ex)
								{
									Logger.Print($"메시지 삭제 중 오류 발생: {ex.Message}", LogType.ERROR);
								}
							}
						}

						// 메시지가 적거나 1000개 이상 체크했으면 진행 상황 업데이트
						if (messagesList.Count < 20 || totalChecked % 1000 == 0)
						{
							await FollowupAsync($"'{user.Username}' 사용자의 메시지 삭제 중... 현재 {totalChecked}개의 메시지를 확인했고, {deletedCount}개의 메시지를 삭제했습니다.", ephemeral: true);
						}
					}
				}

				await FollowupAsync($"삭제 완료! '{user.Username}' 사용자의 메시지 {deletedCount}개를 삭제했습니다. (총 {totalChecked}개의 메시지 확인)", ephemeral: true);
				Logger.Print($"'{Context.User.Username}'님이 '{Context.Channel.Name}' 채널에서 '{user.Username}' 사용자의 메시지 {deletedCount}개를 삭제했습니다.");
			}
			catch (Exception ex)
			{
				await FollowupAsync($"메시지 삭제 중 오류가 발생했습니다: {ex.Message}", ephemeral: true);
				Logger.Print($"메시지 삭제 중 오류 발생: {ex.Message}", LogType.ERROR);
			}
		}
	}
}