using System.Collections.Generic;

namespace SeagullDiscordBot
{
    /// <summary>
    /// ���� ����� ���� ������ �����ϴ� Ŭ���� (�� �������� ���� ����)
    /// </summary>
    public class ConfigSettings
    {
        /// <summary>
        /// ���� ID (Guild ID)
        /// </summary>
        public ulong GuildId { get; set; }

        /// <summary>
        /// �ڵ� ���� �ο� ����
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
        /// �ڵ����� �ο��� ���� ID
        /// </summary>
        public ulong? AutoRoleId { get; set; } = null;

		/// <summary>
		/// ���� ä�� ID
		/// </summary>
        public ulong? AuthChannelId { get; set; } = null;

		/// <summary>
		/// ���� ���� �ð� ���� (��)
		/// </summary>
		public int SpamDetectionInterval { get; set; } = 0;

        /// <summary>
        /// �⺻ ������
        /// </summary>
        public ConfigSettings()
        {
        }

        /// <summary>
        /// ���� ID�� �����ϴ� ������
        /// </summary>
        /// <param name="guildId">���� ID</param>
        public ConfigSettings(ulong guildId)
        {
            GuildId = guildId;
        }

        /// <summary>
        /// ������ ��ųʸ��� ��ȯ
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
        /// ��ųʸ����� ������ �ε�
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