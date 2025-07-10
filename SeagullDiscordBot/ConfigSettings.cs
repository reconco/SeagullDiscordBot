using System.Collections.Generic;

namespace SeagullDiscordBot
{
    /// <summary>
    /// ���� ����� ���� ������ �����ϴ� Ŭ���� (�ؽ�Ʈ ���� ���)
    /// </summary>
    public class ConfigSettings
    {
        /// <summary>
        /// �ڵ� ���� �ο� ����
        /// </summary>
        public bool AutoRoleEnabled { get; set; } = false;

        /// <summary>
        /// �ڵ����� �ο��� ���� ID
        /// </summary>
        public ulong? AutoRoleId { get; set; } = null;

        /// <summary>
        /// ���� ���� �ð� ���� (��)
        /// </summary>
        public int SpamDetectionInterval { get; set; } = 0;

        /// <summary>
        /// ������ ��ųʸ��� ��ȯ
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
        /// ��ųʸ����� ������ �ε�
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