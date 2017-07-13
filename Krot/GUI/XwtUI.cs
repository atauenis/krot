/* Krot file manager. The XWT user interface.
 * This file, along with other xwt-related files, may be removed if you're compiling an build with a non-XWT-based UI.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KrotAPI;
using Krot.GUI;
using Xwt;

namespace Krot
{
	/// <summary>
	/// Graphical User Interface
	/// </summary>
	
	class XwtUI : IUIPlugin
	{
		MainWindow mw;// = new MainWindow();
		public XwtUI() {
			Application.Initialize(ToolkitType.Wpf);
			mw = new MainWindow();
		}

		public HostCallback Callback { set; private get; }

		//not required as the GUI plugins' are working within core's code
		public Delegate Host { get; set; }

		public Delegate Progress { get; set; }

		public Delegate Log { get; set; }

		public Delegate Request { get; set; }

		public string GetName()
		{
			throw new NotImplementedException();
		}

		public void Show() {
			mw.Show();
			Application.Run();
		}

		public void SetLayout() {
			mw.Panels.Clear();
			mw.HP.Panel1.Content = mw.HP.Panel2.Content = null;

			mw.Panels.Add(new KPanel(0, Kernel.ConfigMgr.Sections["Panel1"]));
			mw.Panels.Add(new KPanel(1, Kernel.ConfigMgr.Sections["Panel2"]));
			mw.HP.Panel1.Content = mw.Panels[0];
			mw.HP.Panel2.Content = mw.Panels[1];
			mw.HP.Panel1.Shrink = true;
			mw.HP.Panel2.Shrink = true;
			mw.Content = mw.HP;

			mw.Panels[0].GoTo(Kernel.ConfigMgr.Sections["Panels"]["1"]);
			mw.Panels[1].GoTo(Kernel.ConfigMgr.Sections["Panels"]["2"]);
		}

		public int Talk(string CmdName, Dictionary<string, object> Arguments, ref object Result)
		{
			switch(CmdName) {
				case "uiShow": //start XWT and show the MainWindow
					Console.WriteLine("Show UI.");
					Show();
					return 0;
				case "uiAbout": //show About dialog
					MessageDialog.ShowMessage("Файловый менеджер Крот.\nВерсия "+KrotBase.KrotVersion+".\n\nИспользуйте консоль для отладки ядра и плагинов Крота.");
					return 0;
				case "uiDebugConsole": //start debug console
					Console.WriteLine("\nDebug Console is started. The GUI has temporarly frozen (due to XWT limitations).");
					Console.WriteLine("Use 'return' command to return to GUI.");
					KrotBase.CmdPrompt();
					return 0;
				case "uiSetLayout": //set UI widget layout
					SetLayout();
					return 0;
			}
			return 1;
		}
	}
}
