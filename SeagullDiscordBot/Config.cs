using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SeagullDiscordBot
{
	public static class Config
	{
		private const string TokenFile = "token.txt";
		private const string ConfigDirectory = "configs";
		private static readonly ConcurrentDictionary<ulong, ConfigSettings> _serverSettings = new();
		
		/// <summary>
		/// 지정된 서버의 설정을 가져옵니다. 설정이 로드되지 않았다면 자동으로 로드합니다.
		/// </summary>
		/// <param name="guildId">서버 ID</param>
		/// <returns>해당 서버의 설정</returns>
		public static ConfigSettings GetSettings(ulong guildId)
		{
			return _serverSettings.GetOrAdd(guildId, id => LoadServerSettings(id));
		}

		/// <summary>
		/// 이전 버전과의 호환성을 위한 Settings 속성 (deprecated)
		/// </summary>
		[Obsolete("Use GetSettings(guildId) instead")]
		public static ConfigSettings Settings => throw new InvalidOperationException("서버별 설정을 사용해야 합니다. GetSettings(guildId)를 사용하세요.");

		public static string GetToken()
		{
			if (!File.Exists(TokenFile))
			{
				Logger.Print("token.txt 파일이 존재하지 않습니다.", LogType.ERROR);
				return string.Empty;
			}

			return File.ReadAllText(TokenFile).Trim();
		}

		/// <summary>
		/// 모든 서버 설정을 로드합니다.
		/// </summary>
		public static void LoadAllSettings()
		{
			try
			{
				// 설정 디렉토리 생성
				if (!Directory.Exists(ConfigDirectory))
				{
					Directory.CreateDirectory(ConfigDirectory);
					Logger.Print("설정 디렉토리를 생성했습니다.", LogType.STATUS);
				}

				// 모든 설정 파일 로드
				var configFiles = Directory.GetFiles(ConfigDirectory, "*.txt");
				foreach (var configFile in configFiles)
				{
					var fileName = Path.GetFileNameWithoutExtension(configFile);
					if (ulong.TryParse(fileName, out ulong guildId))
					{
						LoadServerSettings(guildId);
					}
				}

				Logger.Print($"총 {_serverSettings.Count}개 서버의 설정을 로드했습니다.", LogType.STATUS);
			}
			catch (Exception ex)
			{
				Logger.Print($"설정 로드 중 오류 발생: {ex.Message}", LogType.ERROR);
			}
		}

		/// <summary>
		/// 특정 서버의 설정을 로드합니다.
		/// </summary>
		/// <param name="guildId">서버 ID</param>
		/// <returns>로드된 설정</returns>
		private static ConfigSettings LoadServerSettings(ulong guildId)
		{
			var configFile = Path.Combine(ConfigDirectory, $"{guildId}.txt");
			
			try
			{
				ConfigSettings settings;
				
				if (File.Exists(configFile))
				{
					var configDict = ParseConfigFile(configFile);
					settings = new ConfigSettings(guildId);
					settings.FromDictionary(configDict);
					Logger.Print($"서버 {guildId}의 설정을 로드했습니다.", LogType.STATUS);
				}
				else
				{
					Logger.Print($"서버 {guildId}의 설정 파일이 없습니다. 기본 설정을 생성합니다.", LogType.WARNING);
					settings = new ConfigSettings(guildId);
					SaveServerSettings(guildId, settings);
				}
				
				return settings;
			}
			catch (Exception ex)
			{
				Logger.Print($"서버 {guildId} 설정 로드 중 오류 발생: {ex.Message}", LogType.ERROR);
				var fallbackSettings = new ConfigSettings(guildId);
				SaveServerSettings(guildId, fallbackSettings);
				return fallbackSettings;
			}
		}

		/// <summary>
		/// 특정 서버의 설정을 저장합니다.
		/// </summary>
		/// <param name="guildId">서버 ID</param>
		/// <param name="settings">저장할 설정 (null이면 현재 메모리의 설정 사용)</param>
		public static void SaveServerSettings(ulong guildId, ConfigSettings? settings = null)
		{
			try
			{
				settings ??= GetSettings(guildId);
				var configFile = Path.Combine(ConfigDirectory, $"{guildId}.txt");
				
				// 설정 디렉토리 생성
				if (!Directory.Exists(ConfigDirectory))
				{
					Directory.CreateDirectory(ConfigDirectory);
				}

				WriteConfigFile(configFile, settings.ToDictionary());
				Logger.Print($"서버 {guildId}의 설정을 저장했습니다.", LogType.STATUS);
			}
			catch (Exception ex)
			{
				Logger.Print($"서버 {guildId} 설정 저장 중 오류 발생: {ex.Message}", LogType.ERROR);
			}
		}

		/// <summary>
		/// 특정 서버의 설정값을 업데이트하고 파일에 저장합니다.
		/// </summary>
		/// <param name="guildId">서버 ID</param>
		/// <param name="updateAction">설정을 업데이트하는 액션</param>
		public static void UpdateSetting(ulong guildId, Action<ConfigSettings> updateAction)
		{
			try
			{
				var settings = GetSettings(guildId);
				updateAction(settings);
				SaveServerSettings(guildId, settings);
			}
			catch (Exception ex)
			{
				Logger.Print($"서버 {guildId} 설정 업데이트 중 오류 발생: {ex.Message}", LogType.ERROR);
			}
		}

		/// <summary>
		/// 특정 서버의 설정을 기본값으로 재설정합니다.
		/// </summary>
		/// <param name="guildId">서버 ID</param>
		public static void ResetToDefault(ulong guildId)
		{
			var settings = new ConfigSettings(guildId);
			_serverSettings.AddOrUpdate(guildId, settings, (key, oldValue) => settings);
			SaveServerSettings(guildId, settings);
			Logger.Print($"서버 {guildId}의 설정이 기본값으로 재설정되었습니다.", LogType.STATUS);
		}

		/// <summary>
		/// 특정 서버의 현재 설정을 문자열로 반환합니다.
		/// </summary>
		/// <param name="guildId">서버 ID</param>
		/// <returns>설정 정보 문자열</returns>
		public static string GetSettingsInfo(ulong guildId)
		{
			try
			{
				var settings = GetSettings(guildId);
				var dict = settings.ToDictionary();
				var lines = new List<string>();
				
				lines.Add($"=== 서버 {guildId} 봇 설정 ===");
				lines.Add($"자동 역할 부여: {settings.AutoRoleEnabled}");
				lines.Add($"자동 역할 ID: {(string.IsNullOrEmpty(dict["AutoRoleId"]) ? "설정되지 않음" : dict["AutoRoleId"])}");
				lines.Add($"인증 채널 ID: {(string.IsNullOrEmpty(dict["AuthChannelId"]) ? "설정되지 않음" : dict["AuthChannelId"])}");
				lines.Add($"스팸 감지 간격 (초): {dict["SpamDetectionInterval"]}");
				
				return string.Join("\n", lines);
			}
			catch (Exception ex)
			{
				Logger.Print($"서버 {guildId} 설정 정보 조회 중 오류 발생: {ex.Message}", LogType.ERROR);
				return "설정 정보를 가져올 수 없습니다.";
			}
		}

		/// <summary>
		/// 이전 버전과의 호환성을 위한 메서드들 (deprecated)
		/// </summary>
		[Obsolete("Use LoadAllSettings() instead")]
		public static void LoadSettings() => LoadAllSettings();

		[Obsolete("Use SaveServerSettings(guildId) instead")]
		public static void SaveSettings() => throw new InvalidOperationException("서버 ID가 필요합니다. SaveServerSettings(guildId)를 사용하세요.");

		/// <summary>
		/// 텍스트 설정 파일을 파싱합니다.
		/// </summary>
		private static Dictionary<string, string> ParseConfigFile(string filePath)
		{
			var configDict = new Dictionary<string, string>();
			
			try
			{
				string[] lines = File.ReadAllLines(filePath);
				
				foreach (string line in lines)
				{
					// 빈 줄이나 주석 줄(#으로 시작) 건너뛰기
					if (string.IsNullOrWhiteSpace(line) || line.Trim().StartsWith("#"))
						continue;
					
					// key=value 형식으로 파싱
					int equalIndex = line.IndexOf('=');
					if (equalIndex > 0)
					{
						string key = line.Substring(0, equalIndex).Trim();
						string value = line.Substring(equalIndex + 1).Trim();
						
						// 따옴표 제거
						if (value.StartsWith("\"") && value.EndsWith("\"") && value.Length >= 2)
						{
							value = value.Substring(1, value.Length - 2);
						}
						
						configDict[key] = value;
					}
				}
			}
			catch (Exception ex)
			{
				Logger.Print($"설정 파일 파싱 중 오류 발생: {ex.Message}", LogType.ERROR);
			}
			
			return configDict;
		}

		/// <summary>
		/// 딕셔너리를 텍스트 설정 파일로 저장합니다.
		/// </summary>
		private static void WriteConfigFile(string filePath, Dictionary<string, string> configDict)
		{
			var lines = new List<string>();
			
			lines.Add($"# 서버 ID: {configDict.GetValueOrDefault("GuildId", "Unknown")}");
			lines.Add($"# 생성일: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
			lines.Add("");
			
			lines.Add("# 서버 정보");
			lines.Add($"GuildId={configDict.GetValueOrDefault("GuildId", "")}");
			lines.Add("");
			
			lines.Add("# 역할 설정");
			lines.Add($"AutoRoleId={configDict.GetValueOrDefault("AutoRoleId", "")}");
			lines.Add($"AuthChannelId={configDict.GetValueOrDefault("AuthChannelId", "")}");
			lines.Add("");
			
			lines.Add("# 도배 감지 설정");	
			lines.Add($"SpamDetectionInterval={configDict.GetValueOrDefault("SpamDetectionInterval", "0")}");
			
			File.WriteAllLines(filePath, lines);
		}
	}
}