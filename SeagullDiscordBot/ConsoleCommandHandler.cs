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
					case "upload":
						break;

					case "quit":
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


	}
}
