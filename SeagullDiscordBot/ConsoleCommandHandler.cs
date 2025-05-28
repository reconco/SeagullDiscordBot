using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeagullDiscordBot
{
	internal class ConsoleCommandHandler
	{

		private void Help()
		{
			Logger.Print("=== SeagullDiscordBot Console Commands ===", LogType.ONLY_CONSOLE);
			Logger.Print("Available commands:", LogType.ONLY_CONSOLE);
			Logger.Print("  help                 - Displays this help message", LogType.ONLY_CONSOLE);
			Logger.Print("  status               - Shows the current bot status", LogType.ONLY_CONSOLE);
			Logger.Print("  update-commands      - Updates and registers Discord commands", LogType.ONLY_CONSOLE);
			Logger.Print("  clear-commands       - Removes all registered Discord commands", LogType.ONLY_CONSOLE);
			Logger.Print("  quit                 - Exits the application", LogType.ONLY_CONSOLE);
			Logger.Print("  exit                 - Exits the application", LogType.ONLY_CONSOLE);
			Logger.Print("=======================================", LogType.ONLY_CONSOLE);
		}

		private void Status()
		{
		}

		public void WatingUserCommand()
		{
			while (true)
			{
				string input = Console.ReadLine();
				Logger.Print(input, LogType.ONLY_LOG);

				string[] command = input.Split(' ');
				if (command.Length == 0)
				{
					continue;
				}

				switch (command[0])
				{
					case "status":
						Status();
						break;

					case "help":
						Help();
						break;

					//case "storage_manager":
					//    break;
					case "update-commands":
						UpdateCommands();
						break;

					case "clear-commands":
						ClearCommands();
						break;

					case "quit":
					case "exit":
						return;
					//break;

					default:
						PrintWrongCommand();
						break;
				}
			}
		}

		private void PrintWrongCommand()
		{
			Logger.Print("Wrong command");
		}

		private void UpdateCommands()
		{
			if (Program.InteractionHandler == null)
			{
				Logger.Print("InteractionHandler is not initialized", LogType.ERROR);
				return;
			}

			Logger.Print("Updating interaction commands...", LogType.NORMAL);
			//Program.InteractionHandler.ClearAllGlobalCommands().GetAwaiter().GetResult();
			Program.InteractionHandler.RegisterCommandsToGuildAsync().GetAwaiter().GetResult();
			Logger.Print("Interaction commands updated successfully", LogType.NORMAL);
		}

		private void ClearCommands()
		{
			if (Program.InteractionHandler == null)
			{
				Logger.Print("InteractionHandler is not initialized", LogType.ERROR);
				return;
			}

			Logger.Print("clearing interaction commands...", LogType.NORMAL);
			Program.InteractionHandler.ClearAllGlobalCommands().GetAwaiter().GetResult();
			Logger.Print("Interaction commands cleared successfully", LogType.NORMAL);
		}

	}
}
