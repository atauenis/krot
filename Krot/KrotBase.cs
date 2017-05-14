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
		private static object nothing = null;
		public const string KrotVersion = "0.1.1705"; //todo: replace with version from project properties

		[STAThread]
		private static void Main(string[] args) //the EXE's entry point
		{
			Console.Title = "Krot console - version "+KrotVersion;
			Console.WriteLine("Krot has been launched.");
			try
			{
				Console.WriteLine("Loading GUI...");
				
				HostCallback hcb = (string Command, Dictionary<string, object> Arguments, out object Result) =>
				{
					return HostEar(Command, Arguments, out Result);
				};

				PluginManager.UIPlugin = new PluginWrapper(new XwtUI());
				PluginManager.UIPlugin.Plugin.Callback = hcb;

				Console.WriteLine("Load first FS...");

				Kernel.krLoadFS(@"..\..\..\KrotLocalFSPlugin\bin\Debug\KrotLocalFSPlugin.dll", "KrotLocalFSPlugin.KrotLocalFSPlugin");
				PluginManager.FSPlugins[0].Plugin.Callback = hcb;

				PluginManager.Launch("uiSetPanel", null); //временный код
				PluginManager.Launch("uiShow", null);

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
			Console.Title += " [Exiting]";
			Console.WriteLine("That's all, press any key to exit.");
			Console.ReadKey();
		}
		
		/// <summary>
		/// Show the command line prompt, and run the entered command.
		/// </summary>
		public static void CmdPrompt()
		{
			Console.Write("Krot console>");
			string UserCommand = Console.ReadLine();
			if (UserCommand == "") CmdPrompt();
			if (UserCommand == "return") { Console.WriteLine("Console has been stopped, and the GUI is now active again."); return; };
			RunCmd(UserCommand);
			CmdPrompt();
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
			object result;
			PluginManager.Launch(parts[0], cmdarg, out result);
			Console.WriteLine(result);
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
