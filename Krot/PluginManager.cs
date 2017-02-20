/* Krot file manager. Plug-in instance manager.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;

namespace Krot
{
	/// <summary>
	/// Менеджер загруженных плагинов
	/// </summary>
	static class PluginManager
	{
		public static List<PluginWrapper> FSPlugins = new List<PluginWrapper>();
		public static List<PluginWrapper> ArchivePlugins = new List<PluginWrapper>();
		public static List<PluginWrapper> VEPlugins = new List<PluginWrapper>();
		public static List<PluginWrapper> MetadataPlugins = new List<PluginWrapper>(); 
		public static PluginWrapper UIPlugin;

		private static object nothing;
		public static int Launch(string Command, Dictionary<String, Object> Arguments, out object Result)
		{
			/* Command prefixes in Krot 1.0.1702:
			 * kr* - Kernel commands (managed by the core)
			 * ui* - commands for UI plug-in module
			 * bc* - broadcast commands (will be sent to everyone)
			 * fsXX* - file system plug-in module commands
			 * arXX* - archive plug-in module commands
			 * veXX* - viewer/editor plug-in module commands
			 * mdXX* - metadata plug-in module commands
			 * 
			 * Some commands are instance-specific, the number of plugin instance is specifed by XX.
			 */
			Result = null;
			switch (Command.Substring(0, 2))
			{
				case "kr": //a kernel command
					switch (Command)
					{
						case "krDebugPrint":
							//krDebugPrint. Print a debug message on the debug console.
							//Arguments: any (every argument will be printed)
							Console.Write("Debug print: ");
							Kernel.krDebugPrint(Arguments);
							return 0;
							break;
						case "krLoadFS":
							//krLoadFS. Load the specifed filesystem plug-in module and attach it into FS Manager.
							//Arguments:
							//"File"=path to the plug-in module
							//"ClassName"=what class should be initialized as the plug-in's core
							Kernel.krLoadFS(Arguments["File"].ToString(), Arguments["ClassName"].ToString());
							Result = null;
							return 0;
							break;
						case "krQuit":
							//krQuit. Exit from Krot.
							Environment.Exit(0);
							break;
					}
					break;
				case "ui": //a command for UI plugin
					return UIPlugin.SendCommand(Command, Arguments);
					break;
				case "bc": //a broadcast command, send to all plugins
					break;
				case "fs": //a command for specific file system plugin
					if (FSPlugins.Count < Convert.ToInt32(Command.Substring(2, 3)))
						throw new ArgumentOutOfRangeException("Command", "This file system is not currently loaded (only " + FSPlugins.Count + " FS ready).");
					PluginWrapper FS = FSPlugins[Convert.ToInt32(Command.Substring(2, 3))];
					string cmd = "fs"+Command.Substring(5);
					object retdata = null;
					int retcode = FS.SendCommand(cmd, Arguments,ref retdata);
					switch (retcode)
					{
						case 0:
							Console.WriteLine("Return: Ok");
							break;
						case 1:
							Console.WriteLine("Return: только без претензий, лады?");
							break;
						case 2:
							Console.WriteLine("Return: command wasn't understanded.");
							break;
						default:
							Console.WriteLine("Return: {0}", retcode);
							break;
					}
					Console.WriteLine("Result: {0}",retdata);
					Result = retdata;
					return retcode;
					break;
				case "ar": //a command for specific archive plugin
					break;
				case "ve": //a command for specific viewing/editing plugin
					break;
				case "md": //a command for specific metadata plugin
					break;
				default:
					if (Command.ToLowerInvariant() == "quit" || Command.ToLowerInvariant() == "exit") Launch("krQuit",Arguments,out nothing);
 					throw new InvalidOperationException("The command is not applicable to this version of Krot: " + Command);
			}
			return 2;
		}

		public static void Launch(string Command, Dictionary<String, Object> Arguments)
		{
			Launch(Command,Arguments,out nothing);
		}
	}
}
