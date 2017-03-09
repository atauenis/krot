/* Krot file manager. The XWT user interface.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xwt;

namespace Krot.GUI
{
	/// <summary>
	/// Main Window (XWT edition)
	/// </summary>
	class MainWindow : Window
	{
		public new Menu MainMenu = new Menu();

		public MainWindow() {
			Title = "Krot";

			//Size = new Size(500, 500);

			Content = new Label("GUI находится в разработке, используйте консоль.\nHelp'a нет. :)");

			base.MainMenu = this.MainMenu;
			PopulateMainMenu();
		}

		/// <summary>
		/// Populates main window's menu with items, according to MNU file
		/// </summary>
		public void PopulateMainMenu() {
			MenuLoader mldr = new MenuLoader();
			List<MenuItemInfo> DefaultMenu = mldr.MenuItems;
			ParseMenu(DefaultMenu, MainMenu);
		}

		internal void ParseMenu(List<MenuItemInfo> MNU, Menu GUIMenu, int StartIndex = 0, int StopIndex = -1) {
			int i = StartIndex;
			if (StopIndex == -1) StopIndex = MNU.Count;
			for(i = 0; i<StopIndex; i++) {
				switch(MNU[i].Type) {
					case MenuItemType.MenuItem:
						PopulateMenu(GUIMenu, MNU[i]);
						break;
					case MenuItemType.Popup:
						Menu subMenu = new Menu();
						MenuItem subMenuRoot = CreateMenuItem(MNU[i]);
						subMenuRoot.SubMenu = subMenu;
						List<MenuItemInfo> SubMenuItems = new List<MenuItemInfo>();


						while(true) {
							i++;
							if (i >= MNU.Count) break;
							if (MNU[i].Type == MenuItemType.EndPopup) break;
							SubMenuItems.Add(MNU[i]);
						}
						ParseMenu(SubMenuItems, subMenu);
						GUIMenu.Items.Add(subMenuRoot);
						break;
				}
			}
		}
		
		/// <summary>
		/// Populate the XWT Menu with a simple item
		/// </summary>
		/// <param name="what">The XWT Menu</param>
		/// <param name="with">The item</param>
		//простые пункты и сепараторы
		internal void PopulateMenu(Menu what, MenuItemInfo with) {
			if(with.Type == MenuItemType.Separator) {
				what.Items.Add(new SeparatorMenuItem());
				return;
			}
			MenuItem NewMenuItem = CreateMenuItem(with);
			what.Items.Add(NewMenuItem);
		}

		/// <summary>
		/// Creates a XWT MenuItem from data
		/// </summary>
		/// <param name="MII"></param>
		/// <returns>The XWT MenuItem</returns>
		internal MenuItem CreateMenuItem(MenuItemInfo MII) {
			MenuItem mi = new MenuItem() { Label = MII.Title, Tag = MII.Tag };
			mi.Clicked += (o, ea) => { RunMenuItem(mi, o, ea); };
			return mi;
		}

		/// <summary>
		/// Event handler for menu item clicks
		/// </summary>
		internal void RunMenuItem(MenuItem which, object sender, EventArgs ea) {
			if (which == null) return;
			if (which.Tag == null) return;
			PluginManager.Launch(which.Tag.ToString(), null);
		}


		//информация о пунктах меню
		//в будущем надо будет вынести в KrotAPI
		public struct MenuItemInfo {
			public MenuItemType Type;
			public string Title;
			public string Tag;
			//todo: public Xwt.Drawing.Image Icon; //Xwt? А как же android/ios версии? Подумать!
		}

		public enum MenuItemType {
		Popup, EndPopup, MenuItem, Separator, No
		}
	}
}
