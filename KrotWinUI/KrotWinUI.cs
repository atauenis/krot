//Пробный GUI для отладки ядра Krot.
//После наладки базовых функций, будет заменён гуём на базе не опубликованных сборок File Commander.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using KrotAPI;

namespace KrotWinUI
{
    public class KrotWinUI : IKrotPlugin
    {
		internal HostCallback Kalbak;

	    public KrotWinUI()
	    {
	    }

	    public string GetName()
	    {
		    return "Krot Windows Forms GUI";
	    }

	    public int Talk(string CmdName, Dictionary<String, object> Arguments)
	    {
		    //HostCmd(CmdName,Arguments);
		    Dictionary<String, Object> dic = new Dictionary<string, object> {{"Command", CmdName},{"Args",Arguments}};
		    object nothing = null;
			HostCmd("krDebugPrint",dic,out nothing);
		    return 0;
	    }

	    public int Talk(string CmdName, Dictionary<string, object> Arguments, ref object Result)
	    {

		    if (CmdName == "uiInit")
		    {
				Form1 form = new Form1(this);
				Application.EnableVisualStyles();
				form.Show();
			    Application.Run(form);
		    }
		    //throw new NotImplementedException();
		    return 0;
	    }

	    public dynamic Callback { set; private get; }
	    public Delegate Host { get; set; }
	    public Delegate Progress { get; set; }
	    public Delegate Log { get; set; }
	    public Delegate Request { get; set; }

	    /// <summary>
		/// Посылка комманды хосту (Krot.exe)
		/// </summary>
		public int HostCmd(string Command, Dictionary<string, object> Arguments, out object Result)
	    {
		    return Callback(Command, Arguments, out Result);
	    }

    }
}
