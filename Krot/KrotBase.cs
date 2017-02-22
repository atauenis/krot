/* Krot file manager. The simple console-based UI for debug purposes.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using KrotAPI;

namespace Krot
{
	public static class KrotBase
	{
		private static PluginWrapper GuiWrapper; //обёртка к UI

		private static object nothing = null;

		private static void Main(string[] args) //the EXE's entry point
		{
			Console.Title = "Krot console - version 0.0.0pre";
			Console.WriteLine("Krot has been launched.");
			try
			{
				Console.WriteLine("Loading GUI...");

				GuiWrapper = new PluginWrapper(@"..\..\..\KrotWinUI\bin\Debug\KrotWinUI.dll", "KrotWinUI.KrotWinUI");
				HostCallback hcb = (string Command, Dictionary<string, object> Arguments, out object Result) =>
				{
					return HostEar(Command, Arguments, out Result);
				};
				GuiWrapper.Plugin.Callback = hcb;

				PluginManager.UIPlugin = GuiWrapper;

				//Console.WriteLine("Где-то здесь должен инициализироваться гуи");
				//PluginManager.Launch("uiinit",  new Dictionary<string, object>());

				//Dictionary<string, object> UiArgDic = new Dictionary<string, object> {{"Имя агрумента", "значение"}};
				//PluginManager.UIPlugin.SendCommand("krEcho", UiArgDic);

				Console.WriteLine("Load first FS...");

				Kernel.krLoadFS(@"..\..\..\KrotLocalFSPlugin\bin\Debug\KrotLocalFSPlugin.dll", "KrotLocalFSPlugin.KrotLocalFSPlugin");
				PluginManager.FSPlugins[0].Plugin.Callback = hcb;

				PluginManager.Launch("uiInit",null);

				Console.WriteLine("Запущена консоль");
				while(true) CmdPrompt();
			}
			catch (Exception ex)
			{
				Kernel.LogException(ex);
			}
			finally
			{
				Console.WriteLine("Krot is disposing.");
			}
			Console.WriteLine("That's all, press any key to exit.");
			Console.ReadKey();
		}
		
		/// <summary>
		/// Show the command line prompt, and run the entered command.
		/// </summary>
		private static void CmdPrompt()
		{
			Console.Write("Krot console>");
			string UserCommand = Console.ReadLine();
			if (UserCommand == "") return;
			RunCmd(UserCommand);
		}

		/// <summary>
		/// Parce the specifed command and run it
		/// </summary>
		/// <param name="cmd">The command with arguments: NAME ARG1_NAME ARG1_VALUE ARG2_NAME ARG2_VALUE</param>
		private static void RunCmd(string cmd)
		{
			//временный код, надо научить видеть экранирование и сделать нормальный ввод агрументов
			string[] parts = cmd.Split(' ');
			Dictionary<String,Object> cmdarg = new Dictionary<string, object>();

			for (int i = 1; i < parts.Length; i+=2)
			{
				cmdarg.Add(parts[i],parts[i+1]);
			}
			PluginManager.Launch(parts[0], cmdarg);
		}

		/// <summary>
		/// "Ухо" хоста (приёмник для команд, высылаемых плагином UI)
		/// </summary>
		private static int HostEar(string Cmd, Dictionary<string, object> Arguments, out object Result)
		{
			try
			{
				return PluginManager.Launch(Cmd, Arguments, out Result);
			}
			catch (Exception ex)
			{
				Kernel.LogException(ex);
				Result = null;
				return 4;
			}
		}
	}
}
