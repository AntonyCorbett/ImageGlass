﻿/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2013 DUONG DIEU PHAP
Project homepage: http://imageglass.org

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Diagnostics;
using ImageGlass.Services;

namespace igcmd
{
    public partial class frmCheckForUpdate : Form
    {
        public frmCheckForUpdate()
        {
            InitializeComponent();
        }

        Update up = new Update();

        private void btnClose_Click(object sender, EventArgs e)
        {
            if (File.Exists(Setting.StartUpDir + "update.xml"))
                File.Delete(Setting.StartUpDir + "update.xml");
            Application.Exit();
        }


        private void frmMain_Load(object sender, EventArgs e)
        {
            this.Text = "Connecting to server, please wait...";
            picStatus.Image = igcmd.Properties.Resources.loading;
            Thread t = new Thread(new ThreadStart(CheckForUpdate));
            t.Priority = ThreadPriority.BelowNormal;
            t.IsBackground = true;
            t.Start();

            //CheckForUpdate();

            FileVersionInfo fv = FileVersionInfo.GetVersionInfo(Setting.StartUpDir + "ImageGlass.exe");
            lblCurentVersion.Text = "Version: " + fv.FileVersion;

        }

        private void CheckForUpdate()
        {
            up = new Update(new Uri("http://www.imageglass.org/checkforupdate"), 
                Setting.StartUpDir + "update.xml");

            if (File.Exists(Setting.StartUpDir + "update.xml"))
            {
                File.Delete(Setting.StartUpDir + "update.xml");
            }

            lblUpdateVersion.Text = "Version: " + up.Info.NewVersion.ToString();
            lblUpdateVersionType.Text = "Version type: " + up.Info.VersionType;
            lblUpdateImportance.Text = "Importance: " + up.Info.Level;
            lblUpdateSize.Text = "Size: " + up.Info.Size;
            lblUpdatePubDate.Text = "Publish date: " + up.Info.PublishDate.ToString("MMM d, yyyy");

            this.Text = "";

            if (up.CheckForUpdate(Setting.StartUpDir + "ImageGlass.exe"))
            {
                if (up.Info.VersionType.ToLower() == "stable")
                {
                    this.Text = "Your ImageGlass is outdate!";
                }

                picStatus.Image = igcmd.Properties.Resources.warning;
                btnDownload.Enabled = true;
            }
            else
            {
                btnDownload.Enabled = false;
                picStatus.Image = igcmd.Properties.Resources.ok;
            }
        }

        private void lnkUpdateReadMore_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                Process.Start(up.Info.Decription);
            }
            catch
            {
                MessageBox.Show("Check your Internet connection!");
            }
        }

        private void btnDownload_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(up.Info.Link.ToString());
            }
            catch
            {
                MessageBox.Show("Check your Internet connection!");
            }
        }
    }
}
