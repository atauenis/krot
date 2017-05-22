/* Krot file manager. The XWT user interface.
 * File list widget.
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KrotAPI;
using Xwt;
using Xwt.Drawing;

namespace Krot.GUI
{
	/// <summary>
	/// List of filesystem entries, very powerful and very fast
	/// </summary>
	internal class KFileList : Canvas
	{
		private List<DrawScript.GuiElement> GUI = new List<DrawScript.GuiElement>(); //draw sript
		public VScrollbar VScroll = new VScrollbar();

		public int Pointer; //pointer position
		public List<int> SelectedItems = new List<int>(); //numbers of items that are under pointer
		public List<ColumnInfo> Columns = new List<ColumnInfo>();

		protected int Ypos = 0; //V position in pixels
		protected int Yesize = 16; //element V size in pixels
		protected int Ycapacity = 10; //V capacity in rows
		protected int Ystart = 0; //first row to show
		protected int Ytop = 16; //start of file list (and end of header)

		protected int FSID;
		protected PluginWrapper FS;
		protected IEnumerator FSFileEnum;
		protected List<FindData> FileCache = new List<FindData>();
		protected int CachePosition = -1;
		protected int FSPosition = -1;

		public event EventHandler<EventArgs> PointerMoved;
		public event EventHandler<EventArgs> SelectionChanged;
		public event EventHandler<EventArgs> FileOpenRequested;


		public KFileList(int fsid) {
			this.BoundsChanged += KFileList_BoundsChanged;
			this.ButtonPressed += KFileList_ButtonPressed;
			this.MouseScrolled += KFileList_MouseScrolled;
			this.KeyPressed += KFileList_KeyPressed;
			VScroll.ValueChanged += VScroll_ValueChanged;

			Columns.Add(new ColumnInfo() { Title="¶", Content = "krot.Icon", Width = 16 });
			Columns.Add(new ColumnInfo() { Title="Расталкивающий столбец", Content="fs.FileName", Width=0, Expand = true});
			Columns.Add(new ColumnInfo() { Title="Размер", Content="fs.Size", Width=50});
			Columns.Add(new ColumnInfo() { Title="Дата", Content="fs.DateTime", Width=110});
			Columns.Add(new ColumnInfo() { Title="Атрибуты", Content="fs.Atribs", Width=100});

			FSID = fsid;
			FS = PluginManager.FSPlugins[FSID];

			Dictionary<string, object> args = new Dictionary<string, object>();
			args.Add("To", @"D:\");
			int retn = FS.SendCommand("fsCwd", args);

			AddChild(VScroll);
			CanGetFocus = true;
			SetFocus();
		}

		/// <summary>
		/// Initialize cache, load and draw first portion of inodes
		/// </summary>
		public void PopulateList() {
			Ystart = 0;
			VScroll.Value = 0;

			FileCache.Clear();
			CachePosition = 0;
			for (int i = 0; i < Ycapacity; i++)
			{
				if (i == 0) { RqFirst(); continue; }
				FindData? nextfile = RqNext();
				if (nextfile == null) break;
				//Application.MainLoop.DispatchPendingEvents();
			}
			Draw();

			PopulateCache(); //should be moved to a background thread
			#if DEBUG
			Console.WriteLine("\nКэш набит на {0} элементов.", FileCache.Count());
			#endif
		}

		/// <summary>
		/// Create draw script and paint it
		/// </summary>
		protected void Draw() {
			GUI.Clear();
			Ypos = Ytop;

			//draw columns
			//установка точных размеров распирающего столбца
			int fatcol = -1, rightsize = 0, leftsize=0;
			for(int col = 0;col<Columns.Count;col++) {
				if (Columns[col].Expand == true) { fatcol = col; break; }
				leftsize += Columns[col].Width;
			}
			if (fatcol > -1)
			{
				rightsize = (int)Size.Width - leftsize;
				for(int otcol = fatcol+1; otcol<=Columns.Count-1;otcol++) {
					//otcol=other columns
					rightsize -= Columns[otcol].Width;
				}
			
				ColumnInfo tolsty = Columns[fatcol];
				tolsty.Width = rightsize;
				Columns[fatcol] = tolsty;
			}
			//отрисовка заголовков столбцов
			int xposc = 0;
			foreach(ColumnInfo ci in Columns) {
				GUI.Add(new DrawScript.dsTextLayout(ci.Title, xposc, 0));
				xposc += ci.Width;
			}

			//draw files and directories
			for (int i = Ystart; i < Ystart + Ycapacity; i++)
			{
				if(FileCache.Count > i)
				DrawFile(FileCache[i], i == Pointer, SelectedItems.Contains(i));
			}

			QueueDraw();
		}
		
		/// <summary>
		/// Require first entry and initialize enumeration (must be called first)
		/// </summary>
		protected FindData? RqFirst() {
			CachePosition = 0;
			if(FileCache.Count > 0) { Console.Write("\nF+"); return FileCache[0]; }

			//if not in cache
			#if DEBUG
			Console.Write("F ");
			#endif
			object resultf = null;
			int retf = FS.SendCommand("fsFindFirst", null, ref resultf);
			FindData firstfile = (FindData)((object[])resultf)[1];
			FSFileEnum = ((object[])resultf)[0] as IEnumerator;
			FileCache.Add(firstfile);
			return firstfile;
		}

		/// <summary>
		/// Require next entry
		/// </summary>
		protected FindData? RqNext() {
			if (FileCache.Count > CachePosition+1)
			{
				CachePosition++;
#if DEBUG
				Console.Write("N+");
#endif
				return FileCache[CachePosition];
			}
#if DEBUG
			Console.Write("N ");
#endif

			//if next file isn't in cache
			object resn = null;
			Dictionary<string, object> args = new Dictionary<string, object>();
			args.Add("Enumeration", FSFileEnum);
			int retn = FS.SendCommand("fsFindNext", args, ref resn);
			if (resn != null)
			{
				FileCache.Add((FindData)resn);
				CachePosition++;
				return (FindData)resn;
			}
			else
			{
				//Console.WriteLine("Возвращён пустой некст файл!");
				return (FindData?)resn; //чую, будут глюки, но пока их не вижу
			}
		}

		//
		/// <summary>
		/// Populate remaining part of inode cache in the background
		/// </summary>
		protected void PopulateCache() {
			bool eschyoest = true;
			while(eschyoest) {
				eschyoest = (RqNext() != null);
			}
			
			VScroll.UpperValue = FileCache.Count() - Ycapacity;
		}

		/// <summary>
		/// Draw the FS entry on the screen
		/// </summary>
		/// <param name="fd">FindData structure of file</param>
		/// <param name="pointed">Is the file under pointer</param>
		/// <param name="selected">Is the file under selection</param>
		protected void DrawFile(FindData? fd, bool pointed, bool selected) {
			if (fd == null) return;

			if(pointed) {
				GUI.Add(new DrawScript.dsRectangle(0, Ypos, Yesize, Size.Width - VScroll.Size.Width));
				GUI.Add(new DrawScript.dsSetLineWidth(1));
				GUI.Add(new DrawScript.dsSetLineDash(0, 1, 1));
				GUI.Add(new DrawScript.dsStroke());
			}

			int xpos = 0;
			foreach(ColumnInfo ci in Columns) {
				if(ci.Content.StartsWith("fs"))
					DrawFileFS(fd, ci.Content, xpos, pointed, selected);
				else
					DrawFileMD(fd, ci.Content, xpos, pointed, selected);
				xpos += ci.Width;
			}
			Ypos += Yesize;
		}

		/// <summary>
		/// Draw FS entry info field using FS plug-in module power
		/// </summary>
		/// <param name="lind">Which column should be used</param>
		protected void DrawFileFS(FindData? fd, string kind, int xpos, bool pointed, bool selected) {

			string toprint="А хуй знает";
			switch(kind) {
				case "fs.FileName":
					toprint = fd.Value.FileName;
					break;
				case "fs.Size":
					toprint = PrepareSize(fd.Value);
					break;
				case "fs.Date":
					toprint = fd.Value.LastWriteTime.ToLongDateString();
					break;
				case "fs.Time":
					toprint = fd.Value.LastWriteTime.ToShortTimeString() ;
					break;
				case "fs.DateTime":
					toprint = fd.Value.LastWriteTime.ToLocalTime().ToString();
					break;
				case "fs.Atribs":
					toprint = fd.Value.FileAttributes.ToString();
					break;
			}
			if(selected)
				GUI.Add(new DrawScript.dsTextLayout(toprint, xpos, Ypos, new ColorTextAttribute() { Color = Colors.Red, StartIndex = 0, Count = toprint.Length}));
			else
				GUI.Add(new DrawScript.dsTextLayout(toprint, xpos, Ypos));
		}

		/// <summary>
		/// Draw FS entry info field using metadata plug-in module or internal power
		/// </summary>
		/// <param name="kind">Which column should be used</param>
		protected void DrawFileMD(FindData? fd, string kind, int xpos, bool pointed, bool selected) {
		}

		protected string PrepareSize(FindData fd) {
			if (fd.FileAttributes == System.IO.FileAttributes.Directory) return "<DIR>";
			else return fd.FileSize.ToString();
			//todo: сделать вывод размера файлов по-человечески
		}

		/// <summary>
		/// Scroll the scroll bar to the specifed position
		/// </summary>
		protected void ScrollTo(int To) {
			int start = Ystart;
			int stop = Ystart + Ycapacity-1;
			if(To < start || To > stop) {
				VScroll.Value = To;
			}
			VScroll_ValueChanged(null, null);
		}

		/// <summary>
		/// Gets number of the item that is under the mouse pointer
		/// </summary>
		protected int GetItemNo(double MousX, double MousY) {
			return Ystart + (int)((MousY - Ytop) / Yesize);
		}

		protected override void OnDraw(Context ctx, Rectangle dirtyRect)
		{
			foreach (DrawScript.GuiElement ds in GUI)
			{
				try
				{
					DrawScript.Draw(ds, ctx, dirtyRect);
				}
				catch { }
			}

			base.OnDraw(ctx, dirtyRect);
		}

		#region Event handlers
		private void KFileList_ButtonPressed(object sender, ButtonEventArgs e)
		{
			//todo: add possibility to configure reaction on button presses with Settings

			if(e.MultiplePress > 1) {
				//DOUBLE CLICK : open dir/file
				Pointer = GetItemNo(e.X,e.Y);
				FindData curfile = FileCache[Pointer];
				if (curfile.FileAttributes == System.IO.FileAttributes.Directory)
				{
					Dictionary<string, object> args = new Dictionary<string, object>();
					args.Add("To", curfile.FullPath);
					int retn = FS.SendCommand("fsCwd", args);
					PopulateList();
					Draw();
				}else
				{ }
				return;
			}

			//SINGLE CLICK
			switch (e.Button) {
				case PointerButton.Left: //LEFT: set pointer
					Pointer = GetItemNo(e.X,e.Y);
					break;
				case PointerButton.Right: //RIGHT: set pointer & switch selection
					Pointer = GetItemNo(e.X, e.Y);
					int itemno = GetItemNo(e.X,e.Y);
					if (SelectedItems.Contains(itemno)) SelectedItems.Remove(itemno);
					else SelectedItems.Add(itemno);
					break;
			}
			Draw();
		}


		private void KFileList_KeyPressed(object sender, KeyEventArgs e)
		{
			switch(e.Key) {
				case Key.Home:
					Pointer = 0;
					ScrollTo(0);
					break;
				case Key.End:
					Pointer = FileCache.Count - 1;
					ScrollTo(Pointer);
					break;
				case Key.Up:
					if (Pointer > 0) Pointer--;
					ScrollTo(Pointer);
					break;
				case Key.Down:
					if (Pointer < FileCache.Count-1) Pointer++;
					ScrollTo(Pointer);
					break;
				case Key.PageUp:
					if(Pointer != Ystart) 
					{
						Pointer = Ystart;
						break;
					}
					
					int pup;
					if (Ystart - Ycapacity >= 0) pup = Ystart - Ycapacity;
					else pup = 0;
					Pointer = pup;
					ScrollTo(pup);
					break;
				case Key.PageDown:
					if(Pointer != Ystart + Ycapacity-1) {
						Pointer = Ystart + Ycapacity-1;
						break;
					}

					int pdn;
					pdn = Ystart;
					if (Ystart + Ycapacity <= FileCache.Count - 1)
					{	//if not near end
						pdn += Ycapacity;
						Pointer = pdn+Ycapacity-1;
						ScrollTo(pdn);
						break;
					}
					else Ystart = FileCache.Count - 1 - Ycapacity;
					{
						//if the end is not too far
						Pointer = FileCache.Count - 1;
						ScrollTo(pdn);
					}
					break;
				case Key.Insert:
					if (SelectedItems.Contains(Pointer)) SelectedItems.Remove(Pointer);
					else SelectedItems.Add(Pointer);
					if(Pointer != FileCache.Count-1) Pointer++;
					break;
			}
			e.Handled = true;
			Draw();
		}

		protected void KFileList_BoundsChanged(object sender, EventArgs e)
		{
			Rectangle vsr = new Rectangle(new Point(Size.Width - VScroll.Size.Width, Ytop), new Size(VScroll.Size.Width, Size.Height - Ytop));
			SetChildBounds(VScroll,vsr);
			VScroll.UpperValue = FileCache.Count() - Ycapacity;

			Ycapacity = (int) ((Size.Height-Ytop) / Yesize);
			Draw();
		}
		
		private void VScroll_ValueChanged(object sender, EventArgs e)
		{
			Ystart = (int)VScroll.Value;
			Draw();
		}


		private void KFileList_MouseScrolled(object sender, MouseScrolledEventArgs e)
		{
			switch(e.Direction) {
				case ScrollDirection.Up:
					VScroll.Value--;
					break;
				case ScrollDirection.Down:
					VScroll.Value++;
					break;
			}
			VScroll_ValueChanged(null, null);
		}
		#endregion
	}

	/// <summary>
	/// Info about filelist table column
	/// </summary>
	internal struct ColumnInfo {
		/// <summary>
		/// The caption (title)
		/// </summary>
		public string Title;
		/// <summary>
		/// Width of the column, in px
		/// </summary>
		public int Width;
		/// <summary>
		/// Should ли the column fit to all available size (only 1 column at once can be such "fat")
		/// </summary>
		public bool Expand;
		/// <summary>
		/// The name of field (file/dir property or metadata), that should be placed under this column
		/// </summary>
		public string Content;
	}
}
