using Discord;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SeagullDiscordBot.Services
{
	public class ServiceResult
	{
		public bool Success { get; set; }
		public string Message { get; set; }
		public string ErrorMessage { get; set; }

		public static ServiceResult Successful(string message)
		{
			return new ServiceResult
			{
				Success = true,
				Message = message
			};
		}

		public static ServiceResult Failed(string errorMessage)
		{
			return new ServiceResult
			{
				Success = false,
				ErrorMessage = errorMessage
			};
		}
	}

	public class ChannelResult : ServiceResult
	{
		public ITextChannel Channel { get; set; }

		public static ChannelResult Successful(ITextChannel channel, string message)
		{
			return new ChannelResult
			{
				Success = true,
				Channel = channel,
				Message = message
			};
		}
		public static ChannelResult Failed(string errorMessage)
		{
			return new ChannelResult
			{
				Success = false,
				ErrorMessage = errorMessage
			};
		}
	}
	public class RoleResult : ServiceResult
	{
		public IRole Role { get; set; }

		public static RoleResult Successful(IRole role, string message)
		{
			return new RoleResult
			{
				Success = true,
				Role = role,
				Message = message
			};
		}

		public static RoleResult Failed(string errorMessage)
		{
			return new RoleResult
			{
				Success = false,
				ErrorMessage = errorMessage
			};
		}
	}
}
