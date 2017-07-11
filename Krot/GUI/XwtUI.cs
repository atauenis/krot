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
			mw.Panels[0].KFL.PopulateList();
			Application.Run();
		}

		public int Talk(string CmdName, Dictionary<string, object> Arguments, ref object Result)
		{
			switch(CmdName) {
				case "uiShow":
					Show();
					return 0;
				case "uiAbout":
					MessageDialog.ShowMessage("Файловый менеджер Крот.\nВерсия "+KrotBase.KrotVersion+".\n\nИспользуйте консоль для отладки ядра и плагинов Крота.");
					return 0;
				case "uiDebugConsole":
					Console.WriteLine("\nDebug Console is started. The GUI has temporarly frozen (due to XWT limitations).");
					Console.WriteLine("Use 'return' command to return to GUI.");
					KrotBase.CmdPrompt();
					return 0;
				//case "uiSetPanelLayout":
				case "uiSetPanel":
					//temporary code below. todo: add panel configuration
					mw.Panels.Add(new KPanel(0, Kernel.ConfigMgr.Sections["Panel1"]));
					mw.Content = mw.Panels[0];
					return 0;
			}
			return 1;
			//throw new NotImplementedException();
		}
	}
}
