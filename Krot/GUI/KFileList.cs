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

		protected int Ypos = 0; //V position in pixels
		protected int Ycapacity = 10; //V capacity in rows
		protected int Yesize = 16; //element V size in pixels

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

			FSID = fsid;
			FS = PluginManager.FSPlugins[FSID];

			FileCache.Clear();
			Draw();
		}

		protected void Draw() {
			//здесь будут функции по расчёту и отрисовке виджета

			GUI.Clear();
			Ypos = 0;

			CachePosition = 0;
			for (int i = 0; i < Ycapacity; i++)
			{
				if (i == 0) { DrawFile(RqFirst()); continue; }
				FindData? nextfile = RqNext();
				if (nextfile == null) break;
				DrawFile(nextfile);
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

		/// <summary>
		/// Draw the FS entry on the screen
		/// </summary>
		/// <param name="fd"></param>
		protected void DrawFile(FindData? fd) {
			if (fd == null) return;
			GUI.Add(new DrawScript.dsTextLayout() { Point0 = new Point(0, Ypos), TextLayout = new TextLayout() { Text = fd.Value.FileName } });
			Ypos += Yesize;
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

		protected void KFileList_BoundsChanged(object sender, EventArgs e)
		{
			Ycapacity = (int) (Size.Height / Yesize);
			Draw();
		}
	}
}
