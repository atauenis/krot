/* Krot file manager. Wrapper for plug-in modules.
 */
using System;
using System.Collections.Generic;
using System.Reflection;
using KrotAPI;

namespace Krot
{
	/// <summary>
	/// Wrapper for Krot plugins. It is used for talking with plugins.
	/// </summary>
	class PluginWrapper
	{
		public IKrotPlugin Plugin;
		public Type PluginType;
		public Assembly PluginAssembly;

		public delegate void ProgressChangedDelegate(int Percents, string Status, double MaxValue = 100, double CurValue = 0);

		public PluginWrapper(string PluginPath, string PluginName)
		{
			try
			{
				Console.WriteLine("Loading plugin: " + PluginName);
				PluginAssembly = Assembly.LoadFrom(PluginPath);

				Plugin = LoadPlugin(PluginAssembly);
				
				Console.WriteLine("Plugin {0} has been loaded.", Plugin.GetName());
			}
			catch (Exception ex)
			{
				string innererr = "";
				if (ex.InnerException != null)
				{
					innererr = String.Format("\nInner exception is {0} \\ ({1}), caused by {2}", ex.InnerException.GetType(), ex.InnerException.Message, ex.InnerException.Source);
				}
				Console.WriteLine("Can't load pugin: {0} {1}\n{2}", ex.GetType(), ex.Message, innererr);
				throw;
			}
			ProgressChangeHandler = PrgChng; //undone: возможно, заглушка это плохо.
		}

		IKrotPlugin LoadPlugin(Assembly asm)
		{
			foreach (Type type in asm.GetTypes())
			{
				if (type.GetInterface("KrotAPI.IKrotPlugin") != null)
				{
					PluginType = type;
					dynamic inst = Activator.CreateInstance(type);
					IKrotPlugin inst2 = inst as IKrotPlugin;
					if (inst2 == null) return inst;
					Console.WriteLine("Есть привязка к стандартному API.");
					return inst2;
				}
			}
			throw new Exception("Нет плагинов Крота в сборке.");
		}

		public int SendCommand(string Cmd, Dictionary<string, object> Args)
		{
			object nothing = null;
			return SendCommand(Cmd, Args, ref nothing);
		}

		public int SendCommand(string Cmd, Dictionary<string, object> Args, ref object Ret)
		{
			int retcode = Plugin.Talk(Cmd, Args, ref Ret);
			//int retcode = (int)PluginType.InvokeMember("Talk", BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.Public, null, this,
			//	new object[] {Cmd,Args,ref Ret});
			return retcode;
		}
		

		private void PrgChng(int Percents, string Status, double MaxValue = 100, double CurValue = 0)
		{
			Console.WriteLine("Прогресс есть - {0}", Status);
		}

		public ProgressChangedDelegate ProgressChangeHandler { get; set; }
	}
}
