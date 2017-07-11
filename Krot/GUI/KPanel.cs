/* Krot file manager. The XWT user interface.
 * Useful part of MainWindow's panel (possibly, may be used in plugins too).
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xwt;
using Xwt.Drawing;

namespace Krot.GUI
{
	/// <summary>
	/// Useful (for user) part of GUI panel
	/// </summary>
	class KPanel : Widget
	{
		//KPanelBox (simply right or left part of window) will be created soon
		public Table Layout = new Table();
		public HBox ToolBar = new HBox();
		public TextEntry AdresBar = new TextEntry();//later should be replaced with a custom widget
		public Button ToolBarButton = new Button(StockIcons.Information,"Test");
		public KFileList KFL;
		public Label StatsBar = new Label("выбрано КБ/всего КБ в штук/всего файлов, штук/всего каталогов");

		public KPanel(int fsid, Dictionary<string, string> options) {
			BoundsChanged += KPanelUseful_BoundsChanged;
			KFL = new KFileList(fsid, options);
			KFL.ExpandHorizontal = KFL.ExpandVertical = true;

			ToolBarButton.Clicked += ToolBarButton_Clicked;

			KFL.ChangedWorkingDirectory += KFL_ChangedWorkingDirectory;

			ToolBar.PackStart(AdresBar, true);
			ToolBar.PackStart(ToolBarButton);

			Layout.Add(ToolBar, 0, 0);
			Layout.Add(KFL, 0, 1, 1,1,true,true);
			Layout.Add(StatsBar, 0, 2);
			Content = Layout;
			KFL.SetFocus();
		}

		private void ToolBarButton_Clicked(object sender, EventArgs e)
		{
			KFL.CWD(AdresBar.Text);
		}

		private void KFL_ChangedWorkingDirectory(object sender, EventArgs<string> e)
		{
			AdresBar.Text = e.Note;
		}

		private void KPanelUseful_BoundsChanged(object sender, EventArgs e)
		{
			KFL.HeightRequest = 50; //both values are workaarounds for xwt
			KFL.WidthRequest = 50; //bug, real size will be used instead
		}
	}
}
