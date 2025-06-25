using Discord;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SeagullDiscordBot.Services
{


	public class ChannelService
	{
		/// <summary>
		/// 채널 이름이 이미 존재하는지 확인합니다.
		/// </summary>
		/// <param name="guild">확인할 길드</param>
		/// <param name="channelName">확인할 채널 이름</param>
		/// <param name="categoryId">특정 카테고리 내에서만 확인할 경우 카테고리 ID (선택 사항)</param>
		/// <returns>채널이 존재하면 true, 아니면 false</returns>
		public bool ChannelExists(SocketGuild guild, string channelName, ulong? categoryId = null)
		{
			// 대소문자 구분 없이 비교하기 위해 소문자로 변환
			channelName = channelName.ToLowerInvariant();

			// 카테고리 ID가 제공된 경우 해당 카테고리 내에서만 검색
			if (categoryId.HasValue)
			{
				// 해당 카테고리에 있는 텍스트 채널 중 같은 이름이 있는지 확인
				return guild.TextChannels
					.Any(c => c.Name.ToLowerInvariant() == channelName && c.CategoryId == categoryId.Value);
			}
			else
			{
				// 모든 텍스트 채널 중 같은 이름이 있는지 확인
				return guild.TextChannels
					.Any(c => c.Name.ToLowerInvariant() == channelName);
			}
		}

		/// <summary>
		/// 이미 존재하는 채널을 찾습니다.
		/// </summary>
		/// <param name="guild">검색할 길드</param>
		/// <param name="channelName">찾을 채널 이름</param>
		/// <param name="categoryId">특정 카테고리 내에서만 검색할 경우 카테고리 ID (선택 사항)</param>
		/// <returns>찾은 채널, 없으면 null</returns>
		public SocketTextChannel FindExistingChannel(SocketGuild guild, string channelName, ulong? categoryId = null)
		{
			// 대소문자 구분 없이 비교하기 위해 소문자로 변환
			channelName = channelName.ToLowerInvariant();

			// 카테고리 ID가 제공된 경우 해당 카테고리 내에서만 검색
			if (categoryId.HasValue)
			{
				// 해당 카테고리에 있는 텍스트 채널 중 같은 이름이 있는지 확인
				return guild.TextChannels
					.FirstOrDefault(c => c.Name.ToLowerInvariant() == channelName && c.CategoryId == categoryId.Value);
			}
			else
			{
				// 모든 텍스트 채널 중 같은 이름이 있는지 확인
				return guild.TextChannels
					.FirstOrDefault(c => c.Name.ToLowerInvariant() == channelName);
			}
		}

		public async Task<ITextChannel> CreateTextChannelAsync(SocketGuild guild, string channelName, string categoryName = null)
		{
			try
			{
				// 카테고리 ID 초기화
				ulong? categoryId = null;
				string categoryMessage = string.Empty;

				// 카테고리 이름이 제공된 경우, 해당 카테고리 찾기
				if (!string.IsNullOrEmpty(categoryName))
				{
					var categories = guild.CategoryChannels;
					var category = categories.FirstOrDefault(c => c.Name.Equals(categoryName, StringComparison.OrdinalIgnoreCase));

					if (category != null)
					{
						categoryId = category.Id;
						categoryMessage = $"카테고리 '{categoryName}'에 ";
					}
					else
					{
						// 카테고리가 없으면 생성
						var newCategory = await guild.CreateCategoryChannelAsync(categoryName);
						categoryId = newCategory.Id;
						Logger.Print($"'{categoryName}' 카테고리가 생성되었습니다.");
						categoryMessage = $"새로 생성된 카테고리 '{categoryName}'에 ";
					}
				}

				// 같은 이름의 채널이 이미 존재하는지 확인
				var existingChannel = FindExistingChannel(guild, channelName, categoryId);
				if (existingChannel != null)
				{
					Logger.Print($"'{channelName}' 텍스트 채널이 이미 존재합니다. 기존 채널을 반환합니다.", LogType.WARNING);
					return existingChannel;
				}

				// 채널 생성
				var channel = await guild.CreateTextChannelAsync(channelName, properties => 
				{
					if (categoryId.HasValue)
					{
						properties.CategoryId = categoryId.Value;
					}
				});
				
				// 로그 남기기
				Logger.Print($"'{channel.Name}' 텍스트 채널이 {categoryMessage}생성되었습니다.");

				return channel;
			}
			catch (Exception ex)
			{
				Logger.Print($"채널 생성 중 오류 발생: {ex.Message}", LogType.ERROR);
				throw;
			}
		}

		public async Task<ChannelResult> CreateTextChannelWithResultAsync(SocketGuild guild, string channelName, string username, string categoryName = null)
		{
			try
			{
				// 카테고리 ID 초기화
				ulong? categoryId = null;
				string categoryMessage = string.Empty;
				string resultMessage = string.Empty;

				// 카테고리 이름이 제공된 경우, 해당 카테고리 찾기
				if (!string.IsNullOrEmpty(categoryName))
				{
					var categories = guild.CategoryChannels;
					var category = categories.FirstOrDefault(c => c.Name.Equals(categoryName, StringComparison.OrdinalIgnoreCase));

					if (category != null)
					{
						categoryId = category.Id;
						categoryMessage = $"카테고리 '{categoryName}'에 ";
					}
					else
					{
						// 카테고리가 없으면 생성
						var newCategory = await guild.CreateCategoryChannelAsync(categoryName);
						categoryId = newCategory.Id;
						Logger.Print($"'{categoryName}' 카테고리가 생성되었습니다.");
						categoryMessage = $"새로 생성된 카테고리 '{categoryName}'에 ";
						resultMessage = $"카테고리 '{categoryName}'이(가) 생성되었고, ";
					}
				}

				// 같은 이름의 채널이 이미 존재하는지 확인
				var existingChannel = FindExistingChannel(guild, channelName, categoryId);
				if (existingChannel != null)
				{
					Logger.Print($"'{username}'님이 요청한 '{channelName}' 텍스트 채널이 이미 존재합니다.", LogType.WARNING);
					
					return ChannelResult.Successful(
						existingChannel,
						$"텍스트 채널 '{existingChannel.Name}'이(가) 이미 {categoryMessage}존재합니다."
					);
				}

				// 채널 생성
				var channel = await guild.CreateTextChannelAsync(channelName, properties =>
				{
					if (categoryId.HasValue)
					{
						properties.CategoryId = categoryId.Value;
					}
				});

				// 로그 남기기
				Logger.Print($"'{username}'님이 '{channel.Name}' 텍스트 채널을 {categoryMessage}생성했습니다.");

				return ChannelResult.Successful(
					channel, 
					$"{resultMessage}텍스트 채널 '{channel.Name}'이(가) {categoryMessage}성공적으로 생성되었습니다."
				);
			}
			catch (Exception ex)
			{
				Logger.Print($"채널 생성 중 오류 발생: {ex.Message}", LogType.ERROR);
				return ChannelResult.Failed(ex.Message);
			}
		}

		/// <summary>
		/// 채널에서 특정 역할의 권한을 가져옵니다.
		/// </summary>
		/// <param name="channel">권한을 확인할 채널</param>
		/// <param name="role">확인할 역할</param>
		/// <returns>해당 역할의 채널 권한</returns>
		public OverwritePermissions GetRolePermissions(IGuildChannel channel, IRole role)
		{
			// 채널의 권한 오버라이드 중 해당 역할에 대한 권한 찾기
			var permissionOverwrite = channel.GetPermissionOverwrite(role);

			// 권한 오버라이드가 없으면 기본값 반환
			return permissionOverwrite ?? OverwritePermissions.InheritAll;
		}

		/// <summary>
		/// 채널에서 특정 역할의 권한을 설정합니다.
		/// </summary>
		/// <param name="channel">권한을 설정할 채널</param>
		/// <param name="role">권한을 설정할 역할</param>
		/// <param name="permissions">설정할 권한</param>
		/// <param name="requestedBy">권한 변경을 요청한 사용자 이름</param>
		/// <returns>권한 설정 결과</returns>
		public async Task<ServiceResult> SetRolePermissionsAsync(IGuildChannel channel, IRole role, OverwritePermissions permissions, string requestedBy)
		{
			try
			{
				// 채널에 역할 권한 설정
				await channel.AddPermissionOverwriteAsync(role, permissions);

				Logger.Print($"'{requestedBy}'님이 '{channel.Name}' 채널에서 '{role.Name}' 역할의 권한을 변경했습니다.");
				return ServiceResult.Successful($"'{channel.Name}' 채널에서 '{role.Name}' 역할의 권한이 성공적으로 변경되었습니다.");
			}
			catch (Exception ex)
			{
				Logger.Print($"채널 권한 설정 중 오류 발생: {ex.Message}", LogType.ERROR);
				return ServiceResult.Failed(ex.Message);
			}
		}

		/// <summary>
		/// 채널에서 특정 역할에 읽기/쓰기 권한을 설정합니다.
		/// </summary>
		/// <param name="channel">권한을 설정할 채널</param>
		/// <param name="role">권한을 설정할 역할</param>
		/// <param name="canRead">읽기 권한 여부</param>
		/// <param name="canWrite">쓰기 권한 여부</param>
		/// <param name="requestedBy">권한 변경을 요청한 사용자 이름</param>
		/// <returns>권한 설정 결과</returns>
		public async Task<ServiceResult> SetRoleReadWritePermissionsAsync(
			IGuildChannel channel,
			IRole role,
			bool canRead,
			bool canWrite,
			string requestedBy)
		{
			try
			{
				// 권한 객체 생성
				var permissions = new OverwritePermissions(
					viewChannel: canRead ? PermValue.Allow : PermValue.Deny,
					sendMessages: canWrite ? PermValue.Allow : PermValue.Deny
				);

				// 채널에 권한 설정
				await channel.AddPermissionOverwriteAsync(role, permissions);

				// 로그 메시지 구성
				string permissionMsg = canRead && canWrite
					? "읽기/쓰기 권한을 부여"
					: canRead
						? "읽기 권한만 부여"
						: canWrite
							? "쓰기 권한만 부여"
							: "읽기/쓰기 권한을 제거";

				Logger.Print($"'{requestedBy}'님이 '{channel.Name}' 채널에서 '{role.Name}' 역할에 {permissionMsg}했습니다.");
				return ServiceResult.Successful($"'{channel.Name}' 채널에서 '{role.Name}' 역할에 {permissionMsg}되었습니다.");
			}
			catch (Exception ex)
			{
				Logger.Print($"채널 권한 설정 중 오류 발생: {ex.Message}", LogType.ERROR);
				return ServiceResult.Failed(ex.Message);
			}
		}
	}

}