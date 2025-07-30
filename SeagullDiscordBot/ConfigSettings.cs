using System.Collections.Generic;

namespace SeagullDiscordBot
{
    /// <summary>
    /// 봇의 사용자 정의 설정을 저장하는 클래스 (각 서버별로 설정 저장)
    /// </summary>
    public class ConfigSettings
    {
        /// <summary>
        /// 서버 ID (Guild ID)
        /// </summary>
        public ulong GuildId { get; set; }

        /// <summary>
        /// 자동 역할 부여 여부
        /// </summary>
        public bool AutoRoleEnabled
        {
            get
            {
                if(AutoRoleId != null && AuthChannelId != null)
                    return true;
                else
                    return false;
			}
        }

        /// <summary>
        /// 자동으로 부여할 역할 ID
        /// </summary>
        public ulong? AutoRoleId { get; set; } = null;

		/// <summary>
		/// 인증 채널 ID
		/// </summary>
        public ulong? AuthChannelId { get; set; } = null;

		/// <summary>
		/// 스팸 감지 시간 간격 (초)
		/// </summary>
		public int SpamDetectionInterval { get; set; } = 0;

        /// <summary>
        /// 기본 생성자
        /// </summary>
        public ConfigSettings()
        {
        }

        /// <summary>
        /// 서버 ID를 지정하는 생성자
        /// </summary>
        /// <param name="guildId">서버 ID</param>
        public ConfigSettings(ulong guildId)
        {
            GuildId = guildId;
        }

        /// <summary>
        /// 설정을 딕셔너리로 변환
        /// </summary>
        public Dictionary<string, string> ToDictionary()
        {
            return new Dictionary<string, string>
            {
                ["GuildId"] = GuildId.ToString(),
                ["AutoRoleId"] = AutoRoleId?.ToString() ?? "",
				["AuthChannelId"] = AuthChannelId?.ToString() ?? "",
				["SpamDetectionInterval"] = SpamDetectionInterval.ToString(),
            };
        }

        /// <summary>
        /// 딕셔너리에서 설정값 로드
        /// </summary>
        public void FromDictionary(Dictionary<string, string> dict)
        {
            if (dict.TryGetValue("GuildId", out string? guildId) && ulong.TryParse(guildId, out ulong guildIdValue))
                GuildId = guildIdValue;

            if (dict.TryGetValue("AutoRoleId", out string? autoRoleId) && !string.IsNullOrEmpty(autoRoleId) && ulong.TryParse(autoRoleId, out ulong autoRoleIdValue))
                AutoRoleId = autoRoleIdValue;

			if (dict.TryGetValue("AuthChannelId", out string? authChannelId) && !string.IsNullOrEmpty(authChannelId) && ulong.TryParse(authChannelId, out ulong authChannelIdValue))
				AuthChannelId = authChannelIdValue;

			if (dict.TryGetValue("SpamDetectionInterval", out string? spamInterval) && int.TryParse(spamInterval, out int spamIntervalValue))
                SpamDetectionInterval = spamIntervalValue;
        }
    }
}