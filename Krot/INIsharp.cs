/* Krot file manager. INI file parser.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Krot
{
	/// <summary>
	/// INI file parser and settings storage provider
	/// </summary>
	public class INIsharp
	{
		public Dictionary<string, Dictionary<string, string>> Sections = new Dictionary<string, Dictionary<string, string>>();
		public string INIcontent = "";

		public INIsharp() {

		}

		/// <summary>
		/// Load INI file
		/// </summary>
		/// <param name="INIStream">A Stream, which contains INI file</param>
		public void Load(Stream INIStream) {
			StreamReader sr = new StreamReader(INIStream);
			while(!sr.EndOfStream) {
				INIcontent += sr.ReadLine() + Environment.NewLine;
			}

			string SectionName = "INI#";
			Dictionary<string, string> SectionContent = new Dictionary<string, string>();

			foreach (string str in INIcontent.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries)) {
				//Console.WriteLine(str);

				if (str.StartsWith(";")) continue;
				
				if (str.StartsWith("[")) {
					if(SectionName != "INI#" && SectionContent.Count > 0) Sections.Add(SectionName, SectionContent);
					SectionName = str.Substring(1,str.Length-2);
					if (Sections.ContainsKey(SectionName)) SectionName = SectionName + "dupl" + new Random().Next();
					SectionContent = new Dictionary<string, string>();
					continue;
				}

				string OptionName = "";
				string OptionValue = "";
				bool OnValue = false;
				for(int pos=0;pos<str.Length;pos++) {
					if (str[pos] == '=') {
						OnValue = true; continue;
					}
					else {
						if (OnValue)
						{
							OptionValue += str[pos];
							//add parsing for quotes and comments (напр.: name="value\"part in quotes\"";comment )
						}
						else OptionName += str[pos];
					}
				}
				if (OptionName == "") OptionName = "noname" + new Random().Next();

				if(!SectionContent.ContainsKey(OptionName))
					SectionContent.Add(OptionName, OptionValue);
				else
					SectionContent.Add(OptionName + "=" + new Random().Next(), OptionValue);
			}
			Sections.Add(SectionName, SectionContent);
		}
		
		/// <summary>
		/// Save settings to a INI file
		/// </summary>
		/// <param name="INIstream"></param>
		public void Save(Stream INIstream) {
			throw new NotImplementedException();
			//функция сохранения ини файлов. Прежде, чем её писать, надо придумать как не затирать комментарии и пробелы
		}

		/// <summary>
		/// Get a option value
		/// </summary>
		/// <param name="Section">[SECTION NAME]</param>
		/// <param name="Option">OPTION NAME=</param>
		/// <returns></returns>
		public string this[string Section, string Option] {
			get { throw new NotImplementedException(); }
		}
	}
}
