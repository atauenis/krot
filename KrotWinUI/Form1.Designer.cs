namespace KrotWinUI
{
	partial class Form1
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.txtPath = new System.Windows.Forms.TextBox();
			this.cmdGo = new System.Windows.Forms.Button();
			this.listBox1 = new System.Windows.Forms.ListBox();
			this.SuspendLayout();
			// 
			// txtPath
			// 
			this.txtPath.Location = new System.Drawing.Point(13, 13);
			this.txtPath.Name = "txtPath";
			this.txtPath.Size = new System.Drawing.Size(259, 20);
			this.txtPath.TabIndex = 0;
			// 
			// cmdGo
			// 
			this.cmdGo.Location = new System.Drawing.Point(196, 40);
			this.cmdGo.Name = "cmdGo";
			this.cmdGo.Size = new System.Drawing.Size(75, 23);
			this.cmdGo.TabIndex = 1;
			this.cmdGo.Text = "Панеслася!";
			this.cmdGo.UseVisualStyleBackColor = true;
			this.cmdGo.Click += new System.EventHandler(this.cmdGo_Click);
			// 
			// listBox1
			// 
			this.listBox1.FormattingEnabled = true;
			this.listBox1.Location = new System.Drawing.Point(13, 70);
			this.listBox1.Name = "listBox1";
			this.listBox1.Size = new System.Drawing.Size(257, 173);
			this.listBox1.TabIndex = 2;
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(284, 262);
			this.Controls.Add(this.listBox1);
			this.Controls.Add(this.cmdGo);
			this.Controls.Add(this.txtPath);
			this.Name = "Form1";
			this.Text = "Перечисление списка файлов и подкаталогов";
			this.Load += new System.EventHandler(this.Form1_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox txtPath;
		private System.Windows.Forms.Button cmdGo;
		private System.Windows.Forms.ListBox listBox1;
	}
}