//Пробный GUI для отладки ядра Krot.
//После наладки базовых функций, будет заменён гуём на базе не опубликованных сборок File Commander.
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using KrotAPI;

namespace KrotWinUI
{
	public partial class Form1 : Form
	{
		private KrotWinUI PluginCore;
		public Form1(KrotWinUI plugincore)
		{
			PluginCore = plugincore;
			InitializeComponent();
		}

		private void cmdGo_Click(object sender, EventArgs e)
		{
			listBox1.Items.Clear();

			object FirstItemInfo;
			object Enumeracija;
			PluginCore.HostCmd("fs000findfirst", null, out FirstItemInfo);
			Enumeracija = ((object[])FirstItemInfo)[0];

			object NextItemInfo = "заглушка";
			while (NextItemInfo != null)
			{
				PluginCore.HostCmd("fs000findnext", new Dictionary<string, object> {{"Enumeration", Enumeracija}},
					out NextItemInfo);
				FindData fd = new FindData();
				if (NextItemInfo != null)
				{
					fd = (FindData)NextItemInfo;
					listBox1.Items.Add(fd.FileName);
				}
			}
		}

		private void Form1_Load(object sender, EventArgs e)
		{
		
		}
	}
}
