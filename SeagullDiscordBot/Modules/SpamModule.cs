using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace SeagullDiscordBot.Modules
{
	public class MessageInfo
	{
		public string LastMessageContent { get; set; } = ""; // 마지막 메시지 내용
		public DateTime NextMessageTime { get; set; } = new DateTime(); // 다음 반복 메시지 가능 시간
	}

	public class SpamModule : InteractionModuleBase<SocketInteractionContext>
	{
		private static float _jaccardSimilarityThreshold = 0.85f; // Jaccard 유사도 임계값 (0.0 ~ 1.0)
		private static readonly ConcurrentDictionary<ulong, MessageInfo> _spamMessageCooldowns = new ConcurrentDictionary<ulong, MessageInfo>();

		private static readonly object _cleanupLock = new object();
		private static DateTime _lastCleanupTime = DateTime.MinValue;
		private static readonly TimeSpan _cleanupInterval = TimeSpan.FromMinutes(60); // 60분마다 정리

		private static int SpamMessageCooldownTime
		{
			get => Config.Settings.SpamDetectionInterval;
			set
			{
				if (value < 0)
					Config.Settings.SpamDetectionInterval = 0;
				else
					Config.Settings.SpamDetectionInterval = value;
			}
		}

		[SlashCommand("set_spam_message_cooldown_time", "반복 메시지 쿨다운 시간을 정합니다(0초 설정시 Off)")]
		[RequireUserPermission(GuildPermission.Administrator)] // 관리자 권한이 있는 사용자만 사용 가능
		public async Task SetSpamMessageCooldownTime(
			[Summary("time", "도배 방지 시간(초)")] int time)
		{
			SpamMessageCooldownTime = time;

			await RespondAsync($"반복 메시지 쿨다운 시간이 {SpamMessageCooldownTime}초로 지정 되었습니다.", ephemeral: true);

			// 로그 남기기
			Logger.Print($"'{Context.User.Username}'님이 반복 메시지 쿨다운 시간을 {SpamMessageCooldownTime}초로 지정하였습니다.");

			Config.SaveSettings(); // 설정 저장
		}

		[SlashCommand("get_spam_message_cooldown_time", "반복 메시지 쿨다운 시간을 확인합니다")]
		[RequireUserPermission(GuildPermission.Administrator)] // 관리자 권한이 있는 사용자만 사용 가능
		public async Task GetSpamMessageCooldownTime()
		{
			if (SpamMessageCooldownTime < 0)
			{
				await RespondAsync("반복 메시지 쿨다운 시간이 설정되어 있지 않습니다(0초).", ephemeral: true);
				return;
			}
			else
			{
				await RespondAsync($"현재 반복 메시지 쿨다운 시간은 {SpamMessageCooldownTime}초입니다.", ephemeral: true);
			}

			// 로그 남기기
			Logger.Print($"'{Context.User.Username}'님이 반복 메시지 쿨다운 시간을 {SpamMessageCooldownTime}초인 것을 확인하였습니다.");
		}

		// 메시지 받았을 때 처리하는 정적 메서드
		public static async Task HandleMessageAsync(SocketMessage message)
		{
			if (!ShouldProcessMessage(message))
				return;

			var userId = message.Author.Id;
			var messageContent = message.Content;
			var currentTime = DateTime.Now;
			var nextCooldownTime = currentTime.AddSeconds(SpamMessageCooldownTime);

			// 사용자의 메시지 정보 가져오기 또는 생성
			var messageInfo = _spamMessageCooldowns.AddOrUpdate(
				userId,
				// 새 사용자인 경우 새 MessageInfo 생성
				new MessageInfo
				{
					LastMessageContent = messageContent,
					NextMessageTime = nextCooldownTime
				},
				// 기존 사용자인 경우 업데이트 로직
				(key, existingInfo) =>
				{
					lock (existingInfo)
					{
						// 쿨다운 시간 중인지 확인
						if (currentTime < existingInfo.NextMessageTime)
						{
							// 유사도 검사 수행
							double similarity = CalculateJaccardSimilarity(existingInfo.LastMessageContent, messageContent);

							if (similarity >= _jaccardSimilarityThreshold)
							{
								// 유사한 메시지 감지 - 쿨다운 연장
								existingInfo.NextMessageTime = nextCooldownTime;

								// 비동기 작업을 위해 Task.Run 사용
								_ = Task.Run(async () => await HandleSpamDetected(message, similarity));
								
								return existingInfo; // 기존 정보 유지 (LastMessageContent는 변경하지 않음)
							}
						}

						// 쿨다운이 끝났거나 유사도가 낮은 경우 - 메시지 정보 업데이트
						existingInfo.LastMessageContent = messageContent;
						existingInfo.NextMessageTime = nextCooldownTime;
						return existingInfo;
					}
				}
			);

			// 주기적으로 오래된 메시지 정보 정리
			TryCleanupOldMessages();
		}

		/// <summary>
		/// 스팸 메시지가 감지되었을 때 처리합니다.
		/// </summary>
		private static async Task HandleSpamDetected(SocketMessage message, double similarity)
		{
			try
			{
				await message.DeleteAsync();

				// 사용자에게 경고 메시지 전송
				Embed embed = new EmbedBuilder()
					.WithTitle("도배 방지 처리된 메시지")
					.WithDescription(message.Content)
					.WithColor(Color.Red)
					.Build();

				await message.Author.SendMessageAsync(
					"유사한 메시지를 반복적으로 전송하여 메시지가 삭제되었습니다.\n잠시 후 메시지를 전송해주세요.", 
					embed: embed);

				Logger.Print($"유사한 메시지 감지: '{message.Author.Username}' - 유사도: {similarity:P2}, 쿨다운 연장");
			}
			catch (Exception ex)
			{
				Logger.Print($"스팸 감지 처리 중 오류 발생: {ex.Message}", LogType.ERROR);
			}
		}

		/// <summary>
		/// 주기적으로 오래된 메시지 정보를 정리합니다. (Thread-safe)
		/// </summary>
		private static void TryCleanupOldMessages()
		{
			var currentTime = DateTime.Now;
			
			// 이미 최근에 정리했다면 스킵
			if (currentTime - _lastCleanupTime < _cleanupInterval)
				return;

			// 락을 시도하되, 이미 다른 스레드가 정리 중이면 스킵
			if (!Monitor.TryEnter(_cleanupLock))
				return;

			try
			{
				// 더블 체크 (락을 얻는 동안 다른 스레드가 정리했을 수 있음)
				if (currentTime - _lastCleanupTime < _cleanupInterval)
					return;

				RemoveOldUserMessages();
				_lastCleanupTime = currentTime;
			}
			finally
			{
				Monitor.Exit(_cleanupLock);
			}
		}

		/// <summary>
		/// 1시간 이상 지난 메시지 정보를 제거합니다. (Thread-safe)
		/// </summary>
		private static void RemoveOldUserMessages()
		{
			const int RemoveTimeMinutes = 60;
			var cutoffTime = DateTime.Now.AddMinutes(-RemoveTimeMinutes);

			var keysToRemove = new List<ulong>();

			// ConcurrentDictionary의 모든 항목을 안전하게 순회
			foreach (var kvp in _spamMessageCooldowns)
			{
				// MessageInfo에 대한 락을 걸고 확인
				lock (kvp.Value)
				{
					if (kvp.Value.NextMessageTime <= cutoffTime)
					{
						keysToRemove.Add(kvp.Key);
					}
				}
			}

			// 제거할 키들을 안전하게 제거
			foreach (var key in keysToRemove)
			{
				_spamMessageCooldowns.TryRemove(key, out _);
			}

			if (keysToRemove.Count > 0)
			{
				Logger.Print($"오래된 메시지 정보 {keysToRemove.Count}개 정리 완료");
			}
		}

		private static bool ShouldProcessMessage(SocketMessage message)
		{
			// 쿨다운 시간이 설정되어 있지 않으면 아무 작업도 하지 않음
			if (SpamMessageCooldownTime <= 0)
				return false; 

			// 봇 메시지는 무시
			if (message.Author.IsBot)
				return false;

			//// 관리자의 메시지 무시
			//if (message.Author is SocketGuildUser guildUser)
			//{
			//	if (guildUser.GuildPermissions.Administrator)// || guildUser.GuildPermissions.ManageChannels)
			//	{
			//		return false;
			//	}
			//}

			// 빈 메시지 무시
			if (string.IsNullOrWhiteSpace(message.Content))
				return false;

			// 슬래시 명령어는 무시
			if (message.Content.StartsWith("/"))
				return false;

			return true;
		}

		/// <summary>
		/// 두 문자열 간의 Jaccard 유사도를 계산합니다.
		/// </summary>
		/// <param name="text1">첫 번째 문자열</param>
		/// <param name="text2">두 번째 문자열</param>
		/// <returns>0.0에서 1.0 사이의 유사도 값</returns>
		private static double CalculateJaccardSimilarity(string text1, string text2)
		{
			if (string.IsNullOrWhiteSpace(text1) && string.IsNullOrWhiteSpace(text2))
				return 1.0; // 둘 다 비어있으면 완전히 같은 것으로 간주

			if (string.IsNullOrWhiteSpace(text1) || string.IsNullOrWhiteSpace(text2))
				return 0.0; // 하나만 비어있으면 완전히 다른 것으로 간주

			// 문자열을 소문자로 변환하고 공백으로 분할하여 단어 집합 생성
			var words1 = text1.ToLowerInvariant()
				.Split(new char[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
				.ToHashSet();

			var words2 = text2.ToLowerInvariant()
				.Split(new char[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
				.ToHashSet();

			// 교집합과 합집합 계산
			var intersection = words1.Intersect(words2).Count();
			var union = words1.Union(words2).Count();

			// Jaccard 유사도 = |교집합| / |합집합|
			return union == 0 ? 0.0 : (double)intersection / union;
		}

	}
}