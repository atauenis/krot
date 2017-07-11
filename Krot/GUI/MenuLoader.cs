/* Krot file manager. The XWT user interface.
 * Loader of menu files
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Krot.Properties;

namespace Krot.GUI
{
	class MenuLoader
	{
		public string DefaultMenu = Resources.KROT_ENG.ToString();

		String[] Rows;
		public List<MainWindow.MenuItemInfo> MenuItems = new List<MainWindow.MenuItemInfo>();

		public MenuLoader() {
			Rows = DefaultMenu.Replace("\r\n","\n").Split('\n');
			foreach(string str in Rows) {
				if (str.StartsWith("#")) continue;
				string[] SubItems = str.Split(',');
				MainWindow.MenuItemInfo MII = new MainWindow.MenuItemInfo();
				for (int sino=0;sino<SubItems.Count();sino++) {
					switch(sino) {
							case 0://type
								MII.Type = GetMIT(SubItems[sino]);
								break;
							case 1://text
								MII.Title = SubItems[sino];
								break;
							case 2://what is this
								MII.Tag = SubItems[sino];
								break;
							case 3://icon
								throw new NotImplementedException();
					}
				}
				MenuItems.Add(MII);
			}
		}

		MainWindow.MenuItemType GetMIT(string type) {
			switch(type) {
					case "POPUP": return MainWindow.MenuItemType.Popup;
					case "ENDPOPUP": return MainWindow.MenuItemType.EndPopup;
					case "SEPARATOR": return MainWindow.MenuItemType.Separator;
					case "MENUITEM": return MainWindow.MenuItemType.MenuItem;
					default: return MainWindow.MenuItemType.No;
			}
		}
	}
}
