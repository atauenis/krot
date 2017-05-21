/* API for Krot File Manager plug-in modules.
 * Version: 0.1.1703
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace KrotAPI
{
	//интерфейсы и стандартные энумерации для плагинов.

	public interface IKrotPlugin
	{
		string GetName();

		//функции без возврата данных
		//int Talk(string CmdName, Dictionary<String, object> Arguments);
		//скорее всего надо похерить за ненадобностью.

		//функции, возращающие данные
		int Talk(string CmdName, Dictionary<String, Object> Arguments, ref object Result);

		HostCallback Callback { set; }

		//обратные связи с хостом
		/// <summary>
		/// Gets or sets the host callback.
		/// </summary>
	    Delegate Host { get; set; }
		
		/// <summary>
		/// Inform host about progress change: void(int Percents, string Status, double MaxValue = 0, double CurValue = 0)
		/// </summary>
		Delegate Progress{ get; set; }

		/// <summary>
		/// Write info to host's log: void(string Text, int Importance = 3).
		/// </summary>
		Delegate Log{ get; set; }
		/// <summary>
		/// Ask the user for something: object(int RequestType, string Question, params object[] Anwsers)
		/// </summary>
		Delegate Request{ get; set; }
	}

	public interface IUIPlugin : IKrotPlugin {
		//интерфейсы для плагинов GUI.
		//плагины GUI вшиваются в тело Krot.exe, замена производится при компляции.

		/// <summary>
		/// Show the main window.
		/// </summary>
		void Show();
	}


	/// <summary>
	/// Host's callback delegate (can be used to send a command to host)
	/// </summary>
	/// <param name="Command"></param>
	/// <param name="Arguments"></param>
	/// <param name="Result"></param>
	/// <returns></returns>
	public delegate int HostCallback(string Command, Dictionary<String,Object> Arguments, out object Result);

	/// <summary>
	/// File find result data (file's passport)
	/// </summary>
	public struct FindData
	{
		public FileAttributes FileAttributes;
		public DateTime CreationTime;
		public DateTime LastAccessTime; //if not available
		public DateTime LastWriteTime;
		public long FileSize; //file size in bytes or -1 for directories
		public string FileName;
		public string AlternateFileName; //MS-DOS name or (if there are no 8.3 name) same as FileName
		public char[] AccessRights; //UNIX-style access rights (3 or 9-char long, depending on possibilites of FS)
		public Func<string,object> GetAdditionalField; //gets additional fields
		public string FullPath;
	}
}
