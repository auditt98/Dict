namespace Dictianory
{
    partial class OtherLanguages
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
            this.lblSrcLang = new System.Windows.Forms.Label();
            this.txtDestLang = new System.Windows.Forms.TextBox();
            this.comboAvailableLangs = new System.Windows.Forms.ComboBox();
            this.btnTranslate = new System.Windows.Forms.Button();
            this.btnAC = new System.Windows.Forms.Button();
            this.btnDetectSrcLang = new System.Windows.Forms.Button();
            this.txtSrc = new System.Windows.Forms.TextBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblSrcLang
            // 
            this.lblSrcLang.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSrcLang.ForeColor = System.Drawing.Color.LightYellow;
            this.lblSrcLang.Location = new System.Drawing.Point(512, 4);
            this.lblSrcLang.Name = "lblSrcLang";
            this.lblSrcLang.Size = new System.Drawing.Size(106, 35);
            this.lblSrcLang.TabIndex = 13;
            this.lblSrcLang.Text = "Source Language";
            this.lblSrcLang.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // txtDestLang
            // 
            this.txtDestLang.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtDestLang.Location = new System.Drawing.Point(0, 0);
            this.txtDestLang.Multiline = true;
            this.txtDestLang.Name = "txtDestLang";
            this.txtDestLang.Size = new System.Drawing.Size(800, 192);
            this.txtDestLang.TabIndex = 12;
            // 
            // comboAvailableLangs
            // 
            this.comboAvailableLangs.FormattingEnabled = true;
            this.comboAvailableLangs.Location = new System.Drawing.Point(342, 44);
            this.comboAvailableLangs.Name = "comboAvailableLangs";
            this.comboAvailableLangs.Size = new System.Drawing.Size(164, 21);
            this.comboAvailableLangs.TabIndex = 11;
            // 
            // btnTranslate
            // 
            this.btnTranslate.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.btnTranslate.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnTranslate.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnTranslate.Location = new System.Drawing.Point(275, 71);
            this.btnTranslate.Name = "btnTranslate";
            this.btnTranslate.Size = new System.Drawing.Size(231, 35);
            this.btnTranslate.TabIndex = 10;
            this.btnTranslate.Text = "Translate";
            this.btnTranslate.UseVisualStyleBackColor = false;
            this.btnTranslate.Click += new System.EventHandler(this.btnTranslate_Click);
            // 
            // btnAC
            // 
            this.btnAC.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.btnAC.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnAC.Location = new System.Drawing.Point(275, 44);
            this.btnAC.Name = "btnAC";
            this.btnAC.Size = new System.Drawing.Size(61, 21);
            this.btnAC.TabIndex = 9;
            this.btnAC.Text = "AC";
            this.btnAC.UseVisualStyleBackColor = false;
            this.btnAC.Click += new System.EventHandler(this.btnAC_Click);
            // 
            // btnDetectSrcLang
            // 
            this.btnDetectSrcLang.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.btnDetectSrcLang.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnDetectSrcLang.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDetectSrcLang.Location = new System.Drawing.Point(275, 3);
            this.btnDetectSrcLang.Name = "btnDetectSrcLang";
            this.btnDetectSrcLang.Size = new System.Drawing.Size(231, 35);
            this.btnDetectSrcLang.TabIndex = 8;
            this.btnDetectSrcLang.Text = "Detect Language";
            this.btnDetectSrcLang.UseVisualStyleBackColor = false;
            this.btnDetectSrcLang.Click += new System.EventHandler(this.btnDetectSrcLang_Click);
            // 
            // txtSrc
            // 
            this.txtSrc.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtSrc.Location = new System.Drawing.Point(0, 0);
            this.txtSrc.Multiline = true;
            this.txtSrc.Name = "txtSrc";
            this.txtSrc.Size = new System.Drawing.Size(800, 141);
            this.txtSrc.TabIndex = 7;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.txtSrc);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(800, 141);
            this.panel1.TabIndex = 14;
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(49)))));
            this.panel2.Controls.Add(this.btnDetectSrcLang);
            this.panel2.Controls.Add(this.btnAC);
            this.panel2.Controls.Add(this.comboAvailableLangs);
            this.panel2.Controls.Add(this.lblSrcLang);
            this.panel2.Controls.Add(this.btnTranslate);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(0, 141);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(800, 117);
            this.panel2.TabIndex = 15;

            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.txtDestLang);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.Location = new System.Drawing.Point(0, 258);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(800, 192);
            this.panel3.TabIndex = 16;
            // 
            // OtherLanguages
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Name = "OtherLanguages";
            this.Text = "OtherLanguages";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lblSrcLang;
        private System.Windows.Forms.TextBox txtDestLang;
        private System.Windows.Forms.ComboBox comboAvailableLangs;
        private System.Windows.Forms.Button btnTranslate;
        private System.Windows.Forms.Button btnAC;
        private System.Windows.Forms.Button btnDetectSrcLang;
        private System.Windows.Forms.TextBox txtSrc;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel3;
    }
}