using Discord;
using Discord.Interactions;

namespace SeagullDiscordBot.Modules
{
	public class CaptchaModal : IModal
	{
		public string Title => "사용자 인증";

		[InputLabel("답변")]
		[ModalTextInput("captcha_answer", TextInputStyle.Short, "답을 입력하세요", maxLength: 50)]
		public string Answer { get; set; }
	}
}