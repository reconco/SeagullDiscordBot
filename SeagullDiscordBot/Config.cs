using System;
using System.Collections.Generic;
using System.IO;

namespace SeagullDiscordBot
{
	public static class Config
	{
		private const string TokenFile = "token.txt";
		private const string ConfigFile = "config.txt";
		private static ConfigSettings? _settings;
		
		/// <summary>
		/// 현재 설정을 가져옵니다. 설정이 로드되지 않았다면 자동으로 로드합니다.
		/// </summary>
		public static ConfigSettings Settings
		{
			get	
			{
				if (_settings == null)	
					LoadSettings();
				return _settings!;
			}
		}

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
		/// 설정 파일을 로드합니다. 파일이 없으면 기본 설정으로 새 파일을 생성합니다.
		/// </summary>
		public static void LoadSettings()
		{
			try
			{
				if (File.Exists(ConfigFile))
				{
					var configDict = ParseConfigFile(ConfigFile);
					_settings = new ConfigSettings();
					_settings.FromDictionary(configDict);
					Logger.Print("설정 파일을 성공적으로 로드했습니다.", LogType.STATUS);
				}
				else
				{
					Logger.Print("설정 파일이 존재하지 않습니다. 기본 설정으로 새 파일을 생성합니다.", LogType.WARNING);
					_settings = new ConfigSettings();
					SaveSettings();
				}
			}
			catch (Exception ex)
			{
				Logger.Print($"설정 파일 로드 중 오류 발생: {ex.Message}", LogType.ERROR);
				_settings = new ConfigSettings();
				SaveSettings();
			}
		}

		/// <summary>
		/// 현재 설정을 파일에 저장합니다.
		/// </summary>
		public static void SaveSettings()
		{
			try
			{
				if (_settings == null)
					_settings = new ConfigSettings();

				WriteConfigFile(ConfigFile, _settings.ToDictionary());
				Logger.Print("설정 파일을 성공적으로 저장했습니다.", LogType.STATUS);
			}
			catch (Exception ex)
			{
				Logger.Print($"설정 파일 저장 중 오류 발생: {ex.Message}", LogType.ERROR);
			}
		}

		/// <summary>
		/// 특정 설정값을 업데이트하고 파일에 저장합니다.
		/// </summary>
		/// <param name="updateAction">설정을 업데이트하는 액션</param>
		public static void UpdateSetting(Action<ConfigSettings> updateAction)
		{
			try
			{
				updateAction(Settings);
				SaveSettings();
			}
			catch (Exception ex)
			{
				Logger.Print($"설정 업데이트 중 오류 발생: {ex.Message}", LogType.ERROR);
			}
		}

		/// <summary>
		/// 설정 파일을 기본값으로 재설정합니다.
		/// </summary>
		public static void ResetToDefault()
		{
			_settings = new ConfigSettings();
			SaveSettings();
			Logger.Print("설정이 기본값으로 재설정되었습니다.", LogType.STATUS);
		}

		/// <summary>
		/// 현재 설정을 문자열로 반환합니다.
		/// </summary>
		public static string GetSettingsInfo()
		{
			try
			{
				var dict = Settings.ToDictionary();
				var lines = new List<string>();
				
				lines.Add("=== 현재 봇 설정 ===");
				lines.Add($"자동 역할 부여: {dict["AutoRoleEnabled"]}");
				lines.Add($"자동 역할 ID: {(string.IsNullOrEmpty(dict["AutoRoleId"]) ? "설정되지 않음" : dict["AutoRoleId"])}");
				lines.Add($"스팸 감지 간격 (초): {dict["SpamDetectionInterval"]}");
				
				return string.Join("\n", lines);
			}
			catch (Exception ex)
			{
				Logger.Print($"설정 정보 조회 중 오류 발생: {ex.Message}", LogType.ERROR);
				return "설정 정보를 가져올 수 없습니다.";
			}
		}

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
			
			lines.Add("# 역할 설정");
			lines.Add($"AutoRoleEnabled={configDict["AutoRoleEnabled"]}");
			lines.Add($"AutoRoleId={configDict["AutoRoleId"]}");
			lines.Add("");
			
			lines.Add("# 도배 감지 설정");	
			lines.Add($"SpamDetectionInterval={configDict["SpamDetectionInterval"]}");
			
			File.WriteAllLines(filePath, lines);
		}
	}
}