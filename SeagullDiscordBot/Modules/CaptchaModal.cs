using Discord;
using Discord.Interactions;

namespace SeagullDiscordBot.Modules
{
	public class CaptchaModal : IModal
	{
		public string Title => "����� ����";

		[InputLabel("�亯")]
		[ModalTextInput("captcha_answer", TextInputStyle.Short, "���� �Է��ϼ���", maxLength: 50)]
		public string Answer { get; set; }
	}
}