/* Krot file manager. The core functionality (backend).
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Krot
{
	public static class Kernel
	{
		public delegate void LogDelegate(string Text, int Importance = 3);
		public delegate void ProgressDelegate(int Percents, string Status, double MaxValue = 0, double CurValue = 0);
		public delegate object RequestDelegate(int RequestType, string Question, params object[] Anwsers);

		/// <summary>
		/// Save an occured exception into log (or report to console)
		/// </summary>
		/// <param name="ex">The exception</param>
		public static void LogException(Exception ex)
		{
			Console.ForegroundColor = ConsoleColor.Yellow;
			string innererr = "";
			if (ex.InnerException != null)
			{
				innererr = String.Format("\nInner exception is {0} \\ ({1}), caused by {2}", ex.InnerException.GetType(),
					ex.InnerException.Message, ex.InnerException.Source);
			}
			Console.WriteLine("Critical error: {0} {1}\nBuggy plugin: {2}{3}\n{4}", ex.GetType(), ex.Message,
				ex.Source, innererr, ex.StackTrace);
			Console.ForegroundColor = ConsoleColor.Gray;
		}

		/// <summary>
		/// Print debug info on console
		/// </summary>
		/// <param name="what">What to print</param>
		internal static void krDebugPrint(object what)
		{
			if (what is Dictionary<string, object>)
				Console.Write(ArgDic2String((what as Dictionary<string, object>),4,false));
			else
				Console.WriteLine("Debug print:{0}",what);
		}

		/// <summary>
		/// Load the specifed filesystem plug-in module and attach it into FS Manager.
		/// </summary>
		/// <param name="File">Path to the plug-in module</param>
		/// <param name="Class">what class should be initialized as the plug-in's core</param>
		internal static void krLoadFS(string File, string Class)
		{
			PluginWrapper FsWrapper = new PluginWrapper(File, Class);

			ProgressDelegate PrDel = LogProgress;
			LogDelegate LogDel = LogLog;
			RequestDelegate ReqDel = ShowRequest;
			FsWrapper.Plugin.Progress = PrDel;
			FsWrapper.Plugin.Log = LogDel;

			PluginManager.FSPlugins.Add(FsWrapper);
			Console.WriteLine("FS # {0} is {1}",PluginManager.FSPlugins.Count,Class);
		}

		/// <summary>
		/// Convert Krot command's argument list to a formatted string
		/// </summary>
		private static string ArgDic2String(Dictionary<string, object> ArgDict, int Tab = 1, bool PrintPreword = true)
		{
			string result = PrintPreword ? "Dictionary:" : "";
			foreach (KeyValuePair<string, object> kvp in ArgDict)
			{
				if(kvp.Value is Dictionary<string, object>)
					result += "\n" + new String(' ', Tab) + kvp.Key + "=" + ArgDic2String(kvp.Value as Dictionary<string, object>, Tab + 5) + "\n";
				else
					result += "\n" + new String(' ', Tab) + kvp.Key + "=" + kvp.Value;
			}
			return result;
		}

		/// <summary>
		/// "Ухо" хоста (приёмник для команд, высылаемых плагином UI)
		/// </summary>
		/// <param name="Cmd"></param>
		/// <param name="Arguments"></param>
		private static int HostEar(string Cmd, Dictionary<string, object> Arguments, out object Result)
		{
			try
			{
				return PluginManager.Launch(Cmd, Arguments, out Result);
			}
			catch (Exception ex)
			{
				LogException(ex);
				Result = null;
				return 4;
			}
		}

		//временная функция для отладки передачи прогресса операций плагином хосту
		private static void LogProgress(int Percents, string Status, double MaxValue = 0, double CurValue = 0)
		{
			Console.WriteLine("Что-то идёт: {0}, и даже достигло {1}%", Status, Percents);
		}

		//временная функция для отладки передачи отчётов о событиях файловых систем
		//второй аргумент - значение из апи тотал коммандера msgtype_*
		private static void LogLog(string Text, int Importance = 3)
		{
			PluginManager.Launch("krDebugPrint",new Dictionary<string, object>(){{"Отчёт плагина ФС",Text},{"Важность:",Importance}});
		}

		//временная функция для отладки взаимодействия с пользователем; пока нет GUI, будет заглушкой
		//первый аргумент - значение, аналогичное RT_* из апи тотал коммандера
		private static object ShowRequest(int RequestType, string Question, params object[] Anwsers)
		{
			throw new NotImplementedException("");
			return null;
		}
	}
}
