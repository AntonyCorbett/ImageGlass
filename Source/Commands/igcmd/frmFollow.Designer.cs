﻿namespace igcmd
{
    partial class frmFollow
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmFollow));
            this.web1 = new System.Windows.Forms.WebBrowser();
            this.SuspendLayout();
            // 
            // web1
            // 
            this.web1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.web1.IsWebBrowserContextMenuEnabled = false;
            this.web1.Location = new System.Drawing.Point(0, 0);
            this.web1.MinimumSize = new System.Drawing.Size(20, 20);
            this.web1.Name = "web1";
            this.web1.ScrollBarsEnabled = false;
            this.web1.Size = new System.Drawing.Size(304, 212);
            this.web1.TabIndex = 0;
            this.web1.Url = new System.Uri("http://www.imageglass.org/app/follow", System.UriKind.Absolute);
            this.web1.WebBrowserShortcutsEnabled = false;
            // 
            // frmFollow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(304, 212);
            this.Controls.Add(this.web1);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(320, 250);
            this.Name = "frmFollow";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.TopMost = true;
            this.Load += new System.EventHandler(this.frmFollow_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.WebBrowser web1;

    }
}