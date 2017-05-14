/* Krot file manager. The XWT user interface.
 * Panel for MainWindow (possibly, may be used in plugins too).
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xwt;

namespace Krot.GUI
{
	class KPanel : Widget
	{
		KFileList KFL;// = new KFileList(fsid);
		public KPanel(int fsid) {
			KFL = new GUI.KFileList(fsid);
			Content = KFL;
		}
	}
}
