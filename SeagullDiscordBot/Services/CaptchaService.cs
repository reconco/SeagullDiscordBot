using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using SkiaSharp;

namespace SeagullDiscordBot.Services
{
	public class CaptchaService
	{
		private static readonly Dictionary<ulong, CaptchaData> _pendingCaptchas = new();
		private static readonly Random _random = new();

		// ĸ�� �̹��� ����
		private const int ImageWidth = 180;
		private const int ImageHeight = 60;
		private const int FontSize = 27;

		public class CaptchaData
		{
			public string Answer { get; set; }
			public byte[] ImageData { get; set; }
			public DateTime CreatedAt { get; set; }
			public int AttemptCount { get; set; }
		}

		/// <summary>
		/// ����ڸ� ���� ���ο� �̹��� ĸ���� �����մϴ�.
		/// </summary>
		/// <param name="userId">����� ID</param>
		/// <returns>ĸ�� �̹��� ������</returns>
		public async Task<byte[]> GenerateCaptchaAsync(ulong userId)
		{
			// ���� ĸ���� �ִٸ� ����
			_pendingCaptchas.Remove(userId);

			// ĸ�� �ؽ�Ʈ ����
			string captchaText = GenerateRandomText();
			
			// �̹��� ����
			byte[] imageData = await CreateCaptchaImageAsync(captchaText);
			
			_pendingCaptchas[userId] = new CaptchaData
			{
				Answer = captchaText,
				ImageData = imageData,
				CreatedAt = DateTime.UtcNow,
				AttemptCount = 0
			};

			return imageData;
		}

		/// <summary>
		/// ĸ�� ���� �����մϴ�.
		/// </summary>
		/// <param name="userId">����� ID</param>
		/// <param name="userAnswer">����ڰ� �Է��� ��</param>
		/// <returns>���� ���</returns>
		public CaptchaVerificationResult VerifyCaptcha(ulong userId, string userAnswer)
		{
			if (!_pendingCaptchas.TryGetValue(userId, out var captchaData))
			{
				return new CaptchaVerificationResult
				{
					IsSuccess = false,
					Message = "���� ���� ������ �����ϴ�. �ٽ� ���� ��ư�� �����ּ���.",
					Retry = false
				};
			}

			// �ð� �ʰ� Ȯ�� (5��)
			if (DateTime.UtcNow - captchaData.CreatedAt > TimeSpan.FromMinutes(5))
			{
				_pendingCaptchas.Remove(userId);
				return new CaptchaVerificationResult
				{
					IsSuccess = false,
					Message = "���� �ð��� �ʰ��Ǿ����ϴ�. �ٽ� ���� ��ư�� �����ּ���.",
					Retry = false
				};
			}

			captchaData.AttemptCount++;

			// �ִ� �õ� Ƚ�� Ȯ�� (3ȸ)
			if (captchaData.AttemptCount >= 3)
			{
				_pendingCaptchas.Remove(userId);
				return new CaptchaVerificationResult
				{
					IsSuccess = false,
					Message = "�ִ� �õ� Ƚ���� �ʰ��߽��ϴ�. �ٽ� ���� ��ư�� �����ּ���.",
					Retry = false
				};
			}

			// �� ���� (��ҹ��� ���� ����)
			if (userAnswer?.Trim().Equals(captchaData.Answer, StringComparison.OrdinalIgnoreCase) == true)
			{
				_pendingCaptchas.Remove(userId);
				return new CaptchaVerificationResult
				{
					IsSuccess = true,
					Message = "������ �����߽��ϴ�!",
					Retry = false
				};
			}

			return new CaptchaVerificationResult
			{
				IsSuccess = false,
				Message = $"Ʋ�Ƚ��ϴ�. ({captchaData.AttemptCount}/3) �ٽ� �õ����ּ���.",
				Retry = true
				//RetryImageData = captchaData.ImageData
			};
		}

		/// <summary>
		/// ������� ��� ���� ĸ���� �����մϴ�.
		/// </summary>
		/// <param name="userId">����� ID</param>
		public void RemovePendingCaptcha(ulong userId)
		{
			_pendingCaptchas.Remove(userId);
		}

		/// <summary>
		/// ����� ĸ������ �����մϴ�.
		/// </summary>
		public void CleanupExpiredCaptchas()
		{
			var expiredKeys = new List<ulong>();
			var cutoffTime = DateTime.UtcNow.AddMinutes(-5);

			foreach (var kvp in _pendingCaptchas)
			{
				if (kvp.Value.CreatedAt < cutoffTime)
				{
					expiredKeys.Add(kvp.Key);
				}
			}

			foreach (var key in expiredKeys)
			{
				_pendingCaptchas.Remove(key);
			}
		}

		/// <summary>
		/// ������ ĸ�� �ؽ�Ʈ�� �����մϴ�.
		/// </summary>
		/// <returns>5�ڸ� ���ĺ� + ���� ����</returns>
		private string GenerateRandomText()
		{
			const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // ȥ���ϱ� ���� ���� ���� (I, O, 0, 1)
			const int length = 5;

			char[] result = new char[length];
			for (int i = 0; i < length; i++)
			{
				result[i] = chars[_random.Next(chars.Length)];
			}

			return new string(result);
		}

		/// <summary>
		/// SkiaSharp�� ����Ͽ� ĸ�� �̹����� �����մϴ�.
		/// </summary>
		/// <param name="text">ĸ�� �ؽ�Ʈ</param>
		/// <returns>PNG �̹��� ����Ʈ �迭</returns>
		private async Task<byte[]> CreateCaptchaImageAsync(string text)
		{
			return await Task.Run(() =>
			{
				using var bitmap = new SKBitmap(ImageWidth, ImageHeight);
				using var canvas = new SKCanvas(bitmap);

				// ��� �׸���
				canvas.Clear(SKColors.White);

				// ��� ������ �߰�
				AddBackgroundNoise(canvas);

				// �ؽ�Ʈ �׸���
				DrawCaptchaText(canvas, text);

				// ���� ������ �߰�
				AddForegroundNoise(canvas);

				// �̹����� jpg ����Ʈ �迭�� ��ȯ
				using var image = SKImage.FromBitmap(bitmap);
				using var data = image.Encode(SKEncodedImageFormat.Jpeg, 85);
				return data.ToArray();
			});
		}

		/// <summary>
		/// ��� ����� �߰��մϴ�.
		/// </summary>
		private void AddBackgroundNoise(SKCanvas canvas)
		{
			using var paint = new SKPaint
			{
				Color = SKColor.FromHsv(_random.Next(360), 30, 240),
				StrokeWidth = 1
			};

			// ��� ���ε�
			for (int i = 0; i < 8; i++)
			{
				var startX = _random.Next(ImageWidth);
				var startY = _random.Next(ImageHeight);
				var endX = _random.Next(ImageWidth);
				var endY = _random.Next(ImageHeight);

				paint.Color = SKColor.FromHsv(_random.Next(360), 30, 220);
				canvas.DrawLine(startX, startY, endX, endY, paint);
			}

			// ��� ����
			for (int i = 0; i < 5; i++)
			{
				var centerX = _random.Next(ImageWidth);
				var centerY = _random.Next(ImageHeight);
				var radius = _random.Next(3, 8);

				paint.Color = SKColor.FromHsv(_random.Next(360), 20, 230);
				canvas.DrawCircle(centerX, centerY, radius, paint);
			}
		}

		/// <summary>
		/// ĸ�� �ؽ�Ʈ�� �׸��ϴ�.
		/// </summary>
		private void DrawCaptchaText(SKCanvas canvas, string text)
		{
			using var paint = new SKPaint
			{
				Color = SKColors.Black,
				TextSize = FontSize,
				IsAntialias = true,
				Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold)
			};

			// �ؽ�Ʈ �ʺ� ���
			var textBounds = new SKRect();
			paint.MeasureText(text, ref textBounds);

			// �� ���ڸ� ���������� �׸��� (��ġ�� ȸ���� �ణ�� ����)
			float totalWidth = textBounds.Width;
			float startX = (ImageWidth - totalWidth) / 2;
			float baseY = ImageHeight / 2 + textBounds.Height / 2;

			float currentX = startX;

			for (int i = 0; i < text.Length; i++)
			{
				char ch = text[i];
				
				// �� ������ ������ �ٸ���
				paint.Color = SKColor.FromHsv(_random.Next(360), 70, _random.Next(50, 150));
				
				// ���� ��ġ�� �ణ�� ��ȭ �߰�
				float charX = currentX + _random.Next(-3, 4);
				float charY = baseY + _random.Next(-5, 6);

				// ���� ȸ��
				canvas.Save();
				canvas.RotateDegrees(_random.Next(-15, 16), charX, charY);
				
				canvas.DrawText(ch.ToString(), charX, charY, paint);
				canvas.Restore();

				// ���� ���� ��ġ ���
				var charWidth = paint.MeasureText(ch.ToString());
				currentX += charWidth;
			}
		}

		/// <summary>
		/// ���� ����� �߰��մϴ�.
		/// </summary>
		private void AddForegroundNoise(SKCanvas canvas)
		{
			using var paint = new SKPaint
			{
				StrokeWidth = 1
			};

			// ���� ����
			for (int i = 0; i < 50; i++)
			{
				var x = _random.Next(ImageWidth);
				var y = _random.Next(ImageHeight);
				
				paint.Color = SKColor.FromHsv(_random.Next(360), 50, _random.Next(100, 200));
				canvas.DrawPoint(x, y, paint);
			}

			// ���� ���ε�
			for (int i = 0; i < 3; i++)
			{
				var startX = _random.Next(ImageWidth);
				var startY = _random.Next(ImageHeight);
				var endX = _random.Next(ImageWidth);
				var endY = _random.Next(ImageHeight);

				paint.Color = SKColor.FromHsv(_random.Next(360), 40, _random.Next(150, 200));
				paint.StrokeWidth = _random.Next(1, 3);
				canvas.DrawLine(startX, startY, endX, endY, paint);
			}
		}
	}

	public class CaptchaVerificationResult
	{
		public bool IsSuccess { get; set; }
		public string Message { get; set; }

		public bool Retry { get; set; }
		//public byte[] RetryImageData { get; set; }
	}
}