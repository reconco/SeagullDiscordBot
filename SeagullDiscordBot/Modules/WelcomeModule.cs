using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace SeagullDiscordBot.Modules
{
    // 새로운 사용자 환영 기능을 담당하는 모듈
    public class WelcomeModule : InteractionModuleBase<SocketInteractionContext>
    {
        // 사용자가 서버에 입장했을 때 환영 메시지를 보내는 메서드
        public static async Task SendWelcomeMessageAsync(SocketGuildUser user)
        {
            // 환영 메시지를 보낼 채널 (일반적으로 일반 채팅 채널이나 환영 채널)
            var channel = user.Guild.DefaultChannel as ISocketMessageChannel;
            
            // 기본 채널이 없거나 접근할 수 없는 경우 텍스트 채널 중 첫 번째를 찾음
            if (channel == null)
            {
                foreach (var ch in user.Guild.TextChannels)
                {
                    if (ch.Name.ToLower().Contains("general") || ch.Name.ToLower().Contains("welcome"))
                    {
                        channel = ch;
                        break;
                    }
                }
                
                // 여전히 채널을 찾지 못한 경우 첫 번째 텍스트 채널 사용
                if (channel == null && user.Guild.TextChannels.Count > 0)
                {
                    channel = user.Guild.TextChannels.First();
                }
            }

            // 채널이 유효한 경우에만 메시지 전송
            if (channel != null)
            {
				// 봇이 채널에 메시지를 보낼 수 있는지 권한 확인
				var currentUser = user.Guild.GetUser(user.Guild.CurrentUser.Id);
				var permissions = currentUser.GetPermissions(channel as IGuildChannel);

				if (!permissions.SendMessages)
				{
					Logger.Print($"'{user.Username}'님이 '{user.Guild.Name}' 서버에 입장했으나 봇이 '{channel.Name}' 채널에 메시지를 보낼 권한이 없습니다.", LogType.WARNING);
					return;
				}

				// 환영 메시지 생성 및 전송
				var embed = new EmbedBuilder()
                    .WithColor(Color.Green)
                    .WithTitle("🎉 새로운 멤버 환영합니다!")
                    .WithDescription($"{user.Mention}님이 서버에 참가하셨습니다. 환영합니다!")
                    .WithThumbnailUrl(user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl())
                    .WithCurrentTimestamp()
                    .WithFooter(footer => footer.Text = $"{user.Guild.Name}에 오신 것을 환영합니다")
                    .Build();


                //왜 메시지 전송이 안되는지 모르겠음
				await channel.SendMessageAsync(embed: embed);

				// 로그 남기기
				Logger.Print($"'{user.Username}'님이 '{user.Guild.Name}' 서버에 입장했습니다. {channel.Name}에 환영 메시지를 전송했습니다.");
            }
            else
            {
                // 채널을 찾지 못한 경우 로그만 남김
                Logger.Print($"'{user.Username}'님이 '{user.Guild.Name}' 서버에 입장했으나 환영 메시지를 보낼 채널을 찾지 못했습니다.", LogType.WARNING);
            }
        }
	}
}