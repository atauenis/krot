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
	class KFileList : Canvas
	{
		private List<DrawScript.GuiElement> GUI = new List<DrawScript.GuiElement>(); //draw sript
		public VScrollbar VScroll = new VScrollbar();

		public int Pointer; //pointer position
		public int[] SelectedItems; //numbers of items that are under pointer

		protected int Ypos = 0; //V position in pixels
		protected int Yesize = 16; //element V size in pixels
		protected int Ycapacity = 10; //V capacity in rows
		protected int Ystart = 0; //first row to show

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

			FSID = fsid;
			FS = PluginManager.FSPlugins[FSID];

			Dictionary<string, object> args = new Dictionary<string, object>();
			args.Add("To", @"D:\сашины\");
			int retn = FS.SendCommand("fsCwd", args);

			AddChild(VScroll);
			CanGetFocus = true;
			SetFocus();
		}

		public void PopulateList() {
			FileCache.Clear();
			CachePosition = 0;
			for (int i = 0; i < Ycapacity; i++)
			{
				if (i == 0) { DrawFile(RqFirst()); continue; }
				FindData? nextfile = RqNext();
				if (nextfile == null) break;
				//DrawFile(nextfile);
			}
			Draw();

			PopulateCache();
			#if DEBUG
			Console.WriteLine("\nКэш набит на {0} элементов.", FileCache.Count());
			#endif
		}


		protected void Draw() {
			//здесь будут функции по расчёту и отрисовке виджета
			GUI.Clear();
			Ypos = 0;

			for (int i = Ystart; i < Ystart + Ycapacity; i++)
			{
				if(FileCache.Count > i)
				if (i == Pointer)
				{
						GUI.Add(new DrawScript.dsRectangle(0, Ypos, Yesize, Size.Width - VScroll.Size.Width));
						GUI.Add(new DrawScript.dsSetLineWidth(1));
						GUI.Add(new DrawScript.dsSetLineDash(0, 1, 1));
						GUI.Add(new DrawScript.dsStroke());
				}
				DrawFile(FileCache[i]);
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

		//populate cache in the background
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
		/// <param name="fd"></param>
		protected void DrawFile(FindData? fd) {
			if (fd == null) return;
			GUI.Add(new DrawScript.dsTextLayout(fd.Value.FileName,0,Ypos));
			Ypos += Yesize;
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
			switch (e.Button) {
				case PointerButton.Left:
					Pointer = Ystart + (int)(e.Y / Yesize);
					break;
			}
			Draw();
		}


		private void KFileList_KeyPressed(object sender, KeyEventArgs e)
		{
			switch(e.Key) {
				case Key.Up:
					if (Pointer > 0) Pointer--;
					ScrollTo(Pointer);
					break;
				case Key.Down:
					if (Pointer < FileCache.Count-1) Pointer++;
					ScrollTo(Pointer);
					break;
				//todo: add more difficult pgup/pgdown handing, with jump to 1st/last row on current screen on first press
				//not only go 1 page up and 1 page down. то есть, как в других файловых менеджерах.
				case Key.PageUp:
					int pup;
					if (Ystart - Ycapacity >= 0) pup = Ystart - Ycapacity;
					else pup = 0;
					Pointer = pup;
					ScrollTo(pup);
					break;
				case Key.PageDown:
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
			}
			e.Handled = true;
			Draw();
		}

		protected void KFileList_BoundsChanged(object sender, EventArgs e)
		{
			Rectangle vsr = new Rectangle(new Point(Size.Width - VScroll.Size.Width, 0), new Size(VScroll.Size.Width, Size.Height));
			SetChildBounds(VScroll,vsr);
			VScroll.UpperValue = FileCache.Count() - Ycapacity;

			Ycapacity = (int) (Size.Height / Yesize);
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
}
