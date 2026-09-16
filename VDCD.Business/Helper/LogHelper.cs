using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDCD.Business.Helper
{
	public static class LogHelper
	{
		private static readonly object _lock = new object();

		/// <summary>
		/// Ghi log lỗi
		/// </summary>
		public static void LogError(Exception ex, string? customMessage = null)
		{
			try
			{
				string logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
				if (!Directory.Exists(logDir))
				{
					Directory.CreateDirectory(logDir);
				}

				string fileName = $"error_{DateTime.Now:yyyyMMdd}.log";
				string filePath = Path.Combine(logDir, fileName);

				var sb = new StringBuilder();
				sb.AppendLine("=================================================");
				sb.AppendLine($"Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");

				if (!string.IsNullOrEmpty(customMessage))
					sb.AppendLine($"Message: {customMessage}");

				sb.AppendLine($"Exception: {ex.Message}");
				sb.AppendLine($"StackTrace: {ex.StackTrace}");

				if (ex.InnerException != null)
				{
					sb.AppendLine("----- Inner Exception -----");
					sb.AppendLine(ex.InnerException.Message);
					sb.AppendLine(ex.InnerException.StackTrace);
				}

				sb.AppendLine("=================================================");

				lock (_lock)
				{
					File.AppendAllText(filePath, sb.ToString());
				}
			}
			catch
			{
				// Tránh crash nếu log lỗi tiếp
			}
		}

		/// <summary>
		/// Ghi log thường (info/debug)
		/// </summary>
		public static void LogInfo(string message)
		{
			try
			{
				string logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
				if (!Directory.Exists(logDir))
				{
					Directory.CreateDirectory(logDir);
				}

				string fileName = $"info_{DateTime.Now:yyyyMMdd}.log";
				string filePath = Path.Combine(logDir, fileName);

				string content = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}\n";

				lock (_lock)
				{
					File.AppendAllText(filePath, content);
				}
			}
			catch
			{
			}
		}
	}
}
