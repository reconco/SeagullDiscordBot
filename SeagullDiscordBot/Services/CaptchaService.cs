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

		// 캡차 이미지 설정
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
		/// 사용자를 위한 새로운 이미지 캡차를 생성합니다.
		/// </summary>
		/// <param name="userId">사용자 ID</param>
		/// <returns>캡차 이미지 데이터</returns>
		public async Task<byte[]> GenerateCaptchaAsync(ulong userId)
		{
			// 기존 캡차가 있다면 제거
			_pendingCaptchas.Remove(userId);

			// 캡차 텍스트 생성
			string captchaText = GenerateRandomText();
			
			// 이미지 생성
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
		/// 캡차 답을 검증합니다.
		/// </summary>
		/// <param name="userId">사용자 ID</param>
		/// <param name="userAnswer">사용자가 입력한 답</param>
		/// <returns>검증 결과</returns>
		public CaptchaVerificationResult VerifyCaptcha(ulong userId, string userAnswer)
		{
			if (!_pendingCaptchas.TryGetValue(userId, out var captchaData))
			{
				return new CaptchaVerificationResult
				{
					IsSuccess = false,
					Message = "진행 중인 인증이 없습니다. 다시 인증 버튼을 눌러주세요.",
					Retry = false
				};
			}

			// 시간 초과 확인 (5분)
			if (DateTime.UtcNow - captchaData.CreatedAt > TimeSpan.FromMinutes(5))
			{
				_pendingCaptchas.Remove(userId);
				return new CaptchaVerificationResult
				{
					IsSuccess = false,
					Message = "인증 시간이 초과되었습니다. 다시 인증 버튼을 눌러주세요.",
					Retry = false
				};
			}

			captchaData.AttemptCount++;

			// 최대 시도 횟수 확인 (3회)
			if (captchaData.AttemptCount >= 3)
			{
				_pendingCaptchas.Remove(userId);
				return new CaptchaVerificationResult
				{
					IsSuccess = false,
					Message = "최대 시도 횟수를 초과했습니다. 다시 인증 버튼을 눌러주세요.",
					Retry = false
				};
			}

			// 답 검증 (대소문자 구분 없음)
			if (userAnswer?.Trim().Equals(captchaData.Answer, StringComparison.OrdinalIgnoreCase) == true)
			{
				_pendingCaptchas.Remove(userId);
				return new CaptchaVerificationResult
				{
					IsSuccess = true,
					Message = "인증에 성공했습니다!",
					Retry = false
				};
			}

			return new CaptchaVerificationResult
			{
				IsSuccess = false,
				Message = $"틀렸습니다. ({captchaData.AttemptCount}/3) 다시 시도해주세요.",
				Retry = true
				//RetryImageData = captchaData.ImageData
			};
		}

		/// <summary>
		/// 사용자의 대기 중인 캡차를 제거합니다.
		/// </summary>
		/// <param name="userId">사용자 ID</param>
		public void RemovePendingCaptcha(ulong userId)
		{
			_pendingCaptchas.Remove(userId);
		}

		/// <summary>
		/// 만료된 캡차들을 정리합니다.
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
		/// 랜덤한 캡차 텍스트를 생성합니다.
		/// </summary>
		/// <returns>5자리 알파벳 + 숫자 조합</returns>
		private string GenerateRandomText()
		{
			const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // 혼동하기 쉬운 문자 제외 (I, O, 0, 1)
			const int length = 5;

			char[] result = new char[length];
			for (int i = 0; i < length; i++)
			{
				result[i] = chars[_random.Next(chars.Length)];
			}

			return new string(result);
		}

		/// <summary>
		/// SkiaSharp를 사용하여 캡차 이미지를 생성합니다.
		/// </summary>
		/// <param name="text">캡차 텍스트</param>
		/// <returns>PNG 이미지 바이트 배열</returns>
		private async Task<byte[]> CreateCaptchaImageAsync(string text)
		{
			return await Task.Run(() =>
			{
				using var bitmap = new SKBitmap(ImageWidth, ImageHeight);
				using var canvas = new SKCanvas(bitmap);

				// 배경 그리기
				canvas.Clear(SKColors.White);

				// 배경 노이즈 추가
				AddBackgroundNoise(canvas);

				// 텍스트 그리기
				DrawCaptchaText(canvas, text);

				// 전경 노이즈 추가
				AddForegroundNoise(canvas);

				// 이미지를 jpg 바이트 배열로 변환
				using var image = SKImage.FromBitmap(bitmap);
				using var data = image.Encode(SKEncodedImageFormat.Jpeg, 85);
				return data.ToArray();
			});
		}

		/// <summary>
		/// 배경 노이즈를 추가합니다.
		/// </summary>
		private void AddBackgroundNoise(SKCanvas canvas)
		{
			using var paint = new SKPaint
			{
				Color = SKColor.FromHsv(_random.Next(360), 30, 240),
				StrokeWidth = 1
			};

			// 배경 라인들
			for (int i = 0; i < 8; i++)
			{
				var startX = _random.Next(ImageWidth);
				var startY = _random.Next(ImageHeight);
				var endX = _random.Next(ImageWidth);
				var endY = _random.Next(ImageHeight);

				paint.Color = SKColor.FromHsv(_random.Next(360), 30, 220);
				canvas.DrawLine(startX, startY, endX, endY, paint);
			}

			// 배경 원들
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
		/// 캡차 텍스트를 그립니다.
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

			// 텍스트 너비 계산
			var textBounds = new SKRect();
			paint.MeasureText(text, ref textBounds);

			// 각 문자를 개별적으로 그리기 (위치와 회전을 약간씩 변경)
			float totalWidth = textBounds.Width;
			float startX = (ImageWidth - totalWidth) / 2;
			float baseY = ImageHeight / 2 + textBounds.Height / 2;

			float currentX = startX;

			for (int i = 0; i < text.Length; i++)
			{
				char ch = text[i];
				
				// 각 문자의 색상을 다르게
				paint.Color = SKColor.FromHsv(_random.Next(360), 70, _random.Next(50, 150));
				
				// 문자 위치에 약간의 변화 추가
				float charX = currentX + _random.Next(-3, 4);
				float charY = baseY + _random.Next(-5, 6);

				// 문자 회전
				canvas.Save();
				canvas.RotateDegrees(_random.Next(-15, 16), charX, charY);
				
				canvas.DrawText(ch.ToString(), charX, charY, paint);
				canvas.Restore();

				// 다음 문자 위치 계산
				var charWidth = paint.MeasureText(ch.ToString());
				currentX += charWidth;
			}
		}

		/// <summary>
		/// 전경 노이즈를 추가합니다.
		/// </summary>
		private void AddForegroundNoise(SKCanvas canvas)
		{
			using var paint = new SKPaint
			{
				StrokeWidth = 1
			};

			// 전경 점들
			for (int i = 0; i < 50; i++)
			{
				var x = _random.Next(ImageWidth);
				var y = _random.Next(ImageHeight);
				
				paint.Color = SKColor.FromHsv(_random.Next(360), 50, _random.Next(100, 200));
				canvas.DrawPoint(x, y, paint);
			}

			// 전경 라인들
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