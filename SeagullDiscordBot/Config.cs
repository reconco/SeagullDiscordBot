using System;
using System.IO;

namespace SeagullDiscordBot
{
	public static class Config
	{
		private const string TokenFile = "token.txt";

		public static string GetToken()
		{
			if (!File.Exists(TokenFile))
			{
				Logger.Print("token.txt 파일이 존재하지 않습니다.", LogType.ERROR);
				return string.Empty;
			}

			return File.ReadAllText(TokenFile).Trim();
		}
	}
}