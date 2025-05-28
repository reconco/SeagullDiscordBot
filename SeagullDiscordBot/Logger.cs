using System;
using System.Collections.Generic;
using System.IO;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace SeagullDiscordBot
{
	
	enum LogType
	{
		ONLY_LOG, ONLY_CONSOLE, STATUS, NORMAL, WARNING, ERROR
	}

	static internal class Logger
	{
		static StreamWriter logWriter = null;

		const int LOG_FILE_COUNT = 64;
		const string LogDirectory = "logs";

		static string FileName = "";

		public static void CreateLogFile()
		{
			if (!Directory.Exists(LogDirectory))
			{
				Directory.CreateDirectory(LogDirectory);
			}

			try
			{
				 // "(yyyy)-(mm)-(dd)_(hh):(mm):(ss).txt" 형식으로 로그 파일 이름 생성
				string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
				FileName = Path.Combine(LogDirectory, $"{timestamp}.txt");
				logWriter = File.CreateText(FileName);
				Print($"Create log file : {FileName}");
			}
			catch (Exception ex)
			{
				 // 로그 파일 쓰기 오류를 콘솔에 출력하지만 예외는 발생시키지 않음
				Print($"Failed to write log to file: {ex.Message}", LogType.ERROR);
			}

			RemoveOldLog();
		}

		public static void RemoveOldLog()
		{
			DirectoryInfo logsDirectoryInfo = new DirectoryInfo("Logs");
			if (!logsDirectoryInfo.Exists)
				return;

			FileInfo[] fileInfos = logsDirectoryInfo.GetFiles("*.txt");
			FileInfo[] sortedFileInfos = fileInfos.OrderBy(fi => fi.CreationTime).ToArray();

			if (sortedFileInfos.Length > LOG_FILE_COUNT)
			{
				int deleteCount = sortedFileInfos.Length - LOG_FILE_COUNT;
				for (int i = 0; i < deleteCount; i++)
				{
					sortedFileInfos[i].Delete();
				}
			}
		}
		public static Task LogAsync(LogMessage message)
		{
			string logText;
			if (message.Exception is CommandException cmdException)
			{
				//Console.WriteLine($"[Command/{message.Severity}] {cmdException.Command.Aliases.First()}"
				//	+ $" failed to execute in {cmdException.Context.Channel}.");
				logText = $"[Command/{message.Severity}] {cmdException.Command.Aliases.First()}"
					+ $" failed to execute in {cmdException.Context.Channel}.";
				Print(logText, LogType.ERROR);

				Print(cmdException.ToString(), LogType.ERROR);
				//Print($"Error Code : {cmdException.HResult}", LogType.ERROR);
				//Print(cmdException.Message, LogType.ERROR);

			}
			else
			{
				//Console.WriteLine($"[General/{message.Severity}] {message}");
				logText = $"[General/{message.Severity}] {message}";
				Print(logText);
			}

			return Task.CompletedTask;
		}

		public static void Print(string logString, LogType logType = LogType.NORMAL)
		{
			string timeStr = DateTime.Now.ToString("[yyyy-MM-dd HH:mm:ss] ");

			string preStr = "";
			if (logType == LogType.WARNING)
				preStr = "Warning! ";
			else if (logType == LogType.ERROR)
				preStr = "Error! ";

			ConsoleColor fontColor = ConsoleColor.Yellow;
			if (logType == LogType.ERROR)
				fontColor = ConsoleColor.Red;
			else if (logType == LogType.STATUS)
				fontColor = ConsoleColor.Green;
			else if (logType == LogType.ONLY_CONSOLE)
				fontColor = ConsoleColor.DarkGreen;

			string printStr = timeStr + preStr + logString;

			if (logType != LogType.ONLY_LOG)
			{
				Console.ForegroundColor = fontColor;
				Console.WriteLine(printStr);
				Console.ForegroundColor = ConsoleColor.White;
			}

			//출력
			if (logType != LogType.ONLY_CONSOLE)
			{
				if (logWriter != null)
				{
					logWriter.WriteLine(printStr);
					logWriter.Flush();
				}
			}
		}

		public static void CloseWriteStream()
		{
			if (logWriter != null)
				logWriter.Close();
		}
	}
}
