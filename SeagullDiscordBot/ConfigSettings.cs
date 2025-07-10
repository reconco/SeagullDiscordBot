using System.Collections.Generic;

namespace SeagullDiscordBot
{
    /// <summary>
    /// 봇의 사용자 정의 설정을 저장하는 클래스 (텍스트 파일 기반)
    /// </summary>
    public class ConfigSettings
    {
        /// <summary>
        /// 자동 역할 부여 여부
        /// </summary>
        public bool AutoRoleEnabled { get; set; } = false;

        /// <summary>
        /// 자동으로 부여할 역할 ID
        /// </summary>
        public ulong? AutoRoleId { get; set; } = null;

        /// <summary>
        /// 스팸 감지 시간 간격 (초)
        /// </summary>
        public int SpamDetectionInterval { get; set; } = 0;

        /// <summary>
        /// 설정을 딕셔너리로 변환
        /// </summary>
        public Dictionary<string, string> ToDictionary()
        {
            return new Dictionary<string, string>
            {
                ["AutoRoleEnabled"] = AutoRoleEnabled.ToString(),
                ["AutoRoleId"] = AutoRoleId?.ToString() ?? "",
                ["SpamDetectionInterval"] = SpamDetectionInterval.ToString(),
            };
        }

        /// <summary>
        /// 딕셔너리에서 설정값 로드
        /// </summary>
        public void FromDictionary(Dictionary<string, string> dict)
        {
            if (dict.TryGetValue("AutoRoleEnabled", out string? autoRoleEnabled) && bool.TryParse(autoRoleEnabled, out bool autoRoleEnabledValue))
                AutoRoleEnabled = autoRoleEnabledValue;

            if (dict.TryGetValue("AutoRoleId", out string? autoRoleId) && !string.IsNullOrEmpty(autoRoleId) && ulong.TryParse(autoRoleId, out ulong autoRoleIdValue))
                AutoRoleId = autoRoleIdValue;

            if (dict.TryGetValue("SpamDetectionInterval", out string? spamInterval) && int.TryParse(spamInterval, out int spamIntervalValue))
                SpamDetectionInterval = spamIntervalValue;
        }
    }
}