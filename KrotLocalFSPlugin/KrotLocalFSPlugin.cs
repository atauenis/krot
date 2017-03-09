/* Krot file manager. Local filesystem plug-in module.
 * Будет включён в Krot.exe как только код загрузки DLL точно будет стабилен.
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;
using KrotAPI;

namespace KrotLocalFSPlugin
{
    public class KrotLocalFSPlugin : IKrotPlugin
    {
		//private Delegate Host;
		public delegate void ProgressChangeDelegate(int Percents, string Status, double MaxValue = 100, double CurValue = 0);

	    private string CurrentWorkingDirectory = "";

		#region API
	    public KrotLocalFSPlugin()
	    {
		    //конструктор
	    }

	    public string GetName()
	    {
		    return "Krot Local FS";
	    }

		//функции без возврата данных
		public int Talk(string CmdName, Dictionary<String, object> Arguments)
	    {
			object nothing = null;
			return Talk(CmdName,Arguments,ref nothing);
	    }

		//функции, возращающие данные
		public int Talk(string CmdName, Dictionary<String, Object> Arguments, ref object Result)
		{
			Dictionary<String, Object> dic = new Dictionary<string, object> { { "From", "LocalFS" }, { "Command", CmdName }, { "Args", Arguments } };
			object nothing;
			HostCmd("krDebugPrint", dic, out nothing);

			switch (CmdName.ToLower())
			{
				//латинский алфавит: A B C D E F G H I J K L M N O P Q R S T U V W X Y Z
				case "fscwd":
					//fsCWD. Change the current Working Directory.
					//Arguments:
					//"To"=The path (relative or absolute), to which we should go.
					//Result: Path to the new working directory.
					if(!Arguments.ContainsKey("To")) throw new ArgumentException("Where you want to go?");
					Directory.SetCurrentDirectory(Arguments["To"].ToString());
					Result = fsCWD(Directory.GetCurrentDirectory());
					return 0;
				case "fsfindfirst":
					//fsFindFirst. Find first file in the directory.
					//Result: Enumeration of all files in the directory.
					return fsFindFirst(Arguments, ref Result);
				case "fsfindnext":
					//fsFindNext. Find the next file in the directory.
					//Arguments:
					//"Emuneration"=The enumeration of files, getted from fsFindFirst.
					//Result: Metadata of the next file.
					return fsFindNext(Arguments, ref Result);
				case "fsgwd":
					//fsGWD. Get the Current Working Directory.
					//Result: Path to the working directory.
					Result = Directory.GetCurrentDirectory();
					return 0;
				case "fsisbusy":
					//fsIsBusy. Check if the filesystem ready for new operations, or it's busy.
					//Result: true if the plug-in module is busy and can't do something else than current operation, or false if not.
					Progress.DynamicInvoke(50, "Всё ещё впереди",1,0.5);
					//int Percents, string Status, int MaxValue = 100, int CurValue = 0
					Result = false;
					return 0;
				case "fsls":
					//fsLs. Lists the contents of the current directory.
					//Result: Array of file names (string[]).
					Result = Directory.GetFileSystemEntries(Directory.GetCurrentDirectory());
					return 0;
				case "fsinit":
					//fsInit. Initialize the plugin.
					Result = null;
					return 0;
				default:
					Result = null;
					return 2;
			}
		}

		public HostCallback Callback { set; private get; }

	    /// <summary>
		/// Посылка комманды хосту (Krot.exe)
		/// </summary>
		public int HostCmd(string Command, Dictionary<string, object> Arguments, out object Result)
	    {
		    return Callback(Command, Arguments, out Result);
	    }

	    public Delegate Host { get; set; }

		public Delegate Progress { get; set; }

		public Delegate Log { get; set; }

		public Delegate Request { get; set; }
		#endregion API

		#region Internal stuff
		private string fsCWD(string To)
		{
			Directory.SetCurrentDirectory(To);
			CurrentWorkingDirectory = Directory.GetCurrentDirectory();
			return CurrentWorkingDirectory;
		}

	    private int fsFindFirst(Dictionary<String, Object> Arguments, ref object Result)
	    {
			IEnumerator dirItemEnum = Directory.GetFileSystemEntries(Directory.GetCurrentDirectory()).GetEnumerator();
			if (dirItemEnum.MoveNext())
			{
				string fsEntry = (string)dirItemEnum.Current;
				//todo: сделать вывод свойств файла

				object[] ToOut = new object[2];
				ToOut[0] = dirItemEnum; //enumeration of directory items
				ToOut[1] = GetFindData(fsEntry); //first file's properties and metadata
				Result = ToOut;
			}
			else
			{
				throw new NotImplementedException();
				//Result = null; //подумать: а может ли код зайти сюда?
			}
		    return 0;
	    }

	    private int fsFindNext(Dictionary<String, Object> Arguments, ref object Result)
	    {
			object o = Arguments["Enumeration"];
			if (!(o is IEnumerator))
				throw new ArgumentException("Тут нужна энумерация файлов от fsFindFirst");
			IEnumerator fsEnum = (IEnumerator)o;
			if (fsEnum.MoveNext())
			{
				object current = fsEnum.Current;
				if (current != null)
				{
					Result = GetFindData(current.ToString());
					return 0;
				}
			}
			Result = null;
			return 0;
	    }

		/// <summary>
		/// Gets the metadata for fsFindNext and fsFindFirst
		/// </summary>
		/// <param name="Path">The path to the file</param>
		/// <returns>Directory Entrie's metadata</returns>
	    private FindData GetFindData(string Path)
	    {
		    FindData fd = new FindData();

			if (Directory.Exists(Path))
			{
				DirectoryInfo di = new DirectoryInfo(Path);

				fd.FileName = di.Name;
				fd.AlternateFileName = di.Name; //todo:add 8.3 (DOS) filename getting
				fd.FileSize = -1;
				fd.FileAttributes = di.Attributes;
				fd.CreationTime = di.CreationTime;
				fd.LastWriteTime = di.LastWriteTime;
				fd.LastAccessTime = di.LastAccessTime;
				fd.AccessRights = GetUnixAccessRights(di);

				return fd;			
			}
			else if (File.Exists(Path))
			{
				FileInfo fi = new FileInfo(Path);

				fd.FileName = fi.Name;
				fd.AlternateFileName = fi.Name; //todo:add 8.3 (DOS) filename getting
				fd.FileSize = fi.Length;
				fd.FileAttributes = fi.Attributes;
				fd.CreationTime = fi.CreationTime;
				fd.LastWriteTime = fi.LastWriteTime;
				fd.LastAccessTime = fi.LastAccessTime;
				fd.AccessRights = GetUnixAccessRights(fi);

				return fd;
			}
			throw new FileNotFoundException("No directory entry found at " + Path);
	    }

	    private char[] GetUnixAccessRights(DirectoryInfo di)
	    {
			bool Read = true;
			bool Write = true;
			const bool Execute = false; //in Windows, directories aren't executable; todo: add support for POSIX rights

			if (di.Attributes == FileAttributes.ReadOnly) Write = false;
			if (di.Attributes == FileAttributes.Hidden) Read = false;

			char[] retvalue = new char[3];
			retvalue[0] = Read ? 'r' : '-';
			retvalue[1] = Write ? 'w' : '-';
// ReSharper disable once UnreachableCode
			retvalue[2] = Execute ? 'x' : '-';
			return retvalue;
	    }

	    private char[] GetUnixAccessRights(FileInfo fi)
	    {
		    bool Read = true;
		    bool Write = true;
		    const bool Execute = true; //in Windows, all files are executable; todo: add support for POSIX rights

		    if (fi.IsReadOnly) Write = false;
		    if (fi.Attributes == FileAttributes.Hidden) Read = false;

			char[] retvalue = new char[3];
		    retvalue[0] = Read ? 'r' : '-';
			retvalue[1] = Write ? 'w' : '-';
// ReSharper disable once UnreachableCode
			retvalue[2] = Execute ? 'x' : '-';
		    return retvalue;
	    }
		#endregion Internal stuff
	}
}
