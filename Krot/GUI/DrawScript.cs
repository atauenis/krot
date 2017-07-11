/* Krot file manager. The XWT user interface.
 * Описания рисовальных действий для функций отрисовки по скриптам - ПЕРЕВЕСТИ с деревенского на англ.
 */
 
using System;
using Xwt;
using Xwt.Drawing;
using Color = Xwt.Drawing.Color;
using Image = Xwt.Drawing.Image;

namespace Krot.GUI
{
	/// <summary>
	/// This class is intended for use in graphical tasks to store a database of
	/// graphical elements, which should be drawn by XWT when the paintbrush is
	/// ready and the paints are buyed.
	/// Also, it can be a foreman on XWT drawing works.
	/// </summary>
	static class DrawScript
	{
		#region Elements for database
		public abstract class GuiElement
		{
			public object tag;
		}
#pragma warning disable 0649
		public class dsImage : GuiElement
		{
			public Image Image;
			public Point Point0;
			public Size Size;
			public double Alpha;
		}

		public class dsTextLayout : GuiElement
		{
			public TextLayout TextLayout;
			public Point Point0;
			public dsTextLayout(int Xpos, int Ypos, TextLayout textLayout) {
				Point0 = new Point(Xpos, Ypos);
				TextLayout = textLayout;
			}
			public dsTextLayout(string Text, int Xpos, int Ypos, params TextAttribute[] Attribs) {
				Point0 = new Point(Xpos, Ypos);
				TextLayout = new TextLayout();
				TextLayout.Text = Text;
				foreach(TextAttribute ta in Attribs) {
					TextLayout.AddAttribute(ta);
				}
			}
		}

		public class dsFill : GuiElement
		{
			public bool Preserve = false;
		}

		public class dsLineTo : GuiElement
		{
			public Point Point;
			public dsLineTo(double X, double Y) {
				Point = new Point(X, Y);
			}
		}

		public class dsMoveTo : GuiElement
		{
			public Point Point;
			public dsMoveTo(double X, double Y) {
				Point = new Point(X, Y);
			}
		}

		public class dsRectangle : GuiElement
		{
			public Point Point0;
			public Size Size;
			public dsRectangle(double Xpos, double Ypos, double Vsize, double Hsize) {
				Point0 = new Point(Xpos, Ypos);
				Size = new Size(Hsize, Vsize);
			}
		}

		public class dsRotate : GuiElement
		{
			public double Angle;
			public dsRotate(double angle) {
				Angle = angle;
			}
		}

		public class dsRoundRectangle : GuiElement //may be deprecated in current xwts, разобраться!
		{
			public Point Point0;
			public Size Size;
			public double Radius;
		}

		public class dsSetColor : GuiElement
		{
			public Color Colour;
			public dsSetColor (Color colour) {
				Colour = colour;
			}
		}

		public class dsSetLineDash : GuiElement
		{
			public double Offset;
			public double[] Pattern;
			public dsSetLineDash(double offset, params double[] pattern) {
				Offset = offset;
				Pattern = pattern;
			}
		}

		public class dsSetLineWidth : GuiElement
		{
			public double Width;
			public dsSetLineWidth(double width) {
				Width = width;
			}
		}

		public class dsStroke : GuiElement
		{
			public bool Preserve = false;
		}

		public class dsRestore : GuiElement
		{

		}
		#endregion

		#region The drawing subprogram
		public static void Draw(GuiElement ds, Context ctx, Rectangle dirtyRect) {
			switch (ds.GetType().ToString())
			{
				case "Krot.GUI.DrawScript+dsTextLayout":
					DrawScript.dsTextLayout dtl = (ds as DrawScript.dsTextLayout);
					ctx.DrawTextLayout(dtl.TextLayout, dtl.Point0);
					break;
				case "Krot.GUI.DrawScript+dsSetColor":
					DrawScript.dsSetColor dsc = ds as DrawScript.dsSetColor;
					ctx.SetColor(dsc.Colour);
					break;
				case "Krot.GUI.DrawScript+dsSetLineWidth":
					DrawScript.dsSetLineWidth dlw = ds as DrawScript.dsSetLineWidth;
					ctx.SetLineWidth(dlw.Width);
					break;
				case "Krot.GUI.DrawScript+dsSetLineDash":
					DrawScript.dsSetLineDash dld = ds as DrawScript.dsSetLineDash;
					ctx.SetLineDash(dld.Offset, dld.Pattern);
					break;
				case "Krot.GUI.DrawScript+dsFill":
					DrawScript.dsFill df = ds as DrawScript.dsFill;
					if (df.Preserve) ctx.FillPreserve(); else ctx.Fill();
					break;
				case "Krot.GUI.DrawScript+dsStroke":
					DrawScript.dsStroke dss = ds as DrawScript.dsStroke;
					if (dss.Preserve) ctx.StrokePreserve(); else ctx.Stroke();
					break;
				case "Krot.GUI.DrawScript+dsRectangle":
					DrawScript.dsRectangle dr = ds as DrawScript.dsRectangle;
					ctx.Rectangle(dr.Point0, dr.Size.Width, dr.Size.Height);
					break;
				case "Krot.GUI.DrawScript+dsRestore":
					ctx.Restore();
					break;
				default:
					try
					{
						MessageDialog.ShowWarning("Данный тип элемента GUI ещё не поддерживается\n" + ds.GetType().ToString());
					}
					catch (Exception ex)
					{
						Console.ForegroundColor = ConsoleColor.Magenta;
						Console.WriteLine("Тип элемента DrawScript {0} не поддерживается!\nНевозможно вывести окно ошибки: {1} в {2}.", ds.GetType().ToString(), ex.GetType().ToString(), ex.Source);
						Console.ForegroundColor = ConsoleColor.Gray;
					}
					throw new NotImplementedException("Данный тип элемента GUI ещё не поддерживается");
			}

		}
		#endregion
	}
}
