﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
namespace launcher
{
    public partial class Main : Form
    {
        blizzButton.blizzButton btnPlay = new blizzButton.blizzButton();
        string _REGISTER, _ACCOUNT, _FORUM, _NEWS, _CHANGELOG;
        System.Net.WebClient DOWNLOADER = new System.Net.WebClient();
        System.Threading.Thread Thread;
        bool SERVER_ONLINE, PLAYABLE = true;
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case 0x84:
                    base.WndProc(ref m);
                    if ((int)m.Result == 0x1)
                        m.Result = (IntPtr)0x2;
                    return;
            }
            base.WndProc(ref m);
        }
        public Main()
        {
            InitializeComponent();
            this.SetStyle(ControlStyles.DoubleBuffer, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.Opaque, false);
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            this.SetStyle(ControlStyles.ResizeRedraw, true);
            
        }
        private void Main_Load(object sender, EventArgs e)
        {
            loadOnce();
            reloadAllData();
        }
        void reloadAllData()
        {
            SERVER_ONLINE = DB.IsServerOnline();
            DB.GetLinks(out _REGISTER, out _FORUM, out _ACCOUNT, out _CHANGELOG, out _NEWS);
            if (_NEWS != "0")
                lblNewsReadMore.Visible = true;
            else lblNewsReadMore.Visible = false;
            if (_CHANGELOG != "0")
                lblChangelogReadMore.Visible = true;
            else lblChangelogReadMore.Visible = false;
            lblNewsData.Text = DB.GetNews();
            lblChangelogData.Text = DB.GetChangelog();
            lblRealmName.Text = DB.GetRealmName();
            PlayersOnlineToggle(SERVER_ONLINE);
        }
        void loadOnce()
        {
            btnPlay.Parent = this;
            btnPlay.Size = new Size(207, 44);
            btnPlay.Location = new Point(125, 453);
            btnPlay.SetValues(Properties.Resources.play_no_hover1, Properties.Resources.play_take, Properties.Resources.play_hover, Properties.Resources.play_no_active, 18);
            btnPlay.Click += blizzPlay_Click;
        }
        void PlayersOnlineToggle(bool online)
        {
            btnPlay.Text = "Spielen";
            if (online)
            {
                picRealmStatus.Image = Properties.Resources.on;
                lblPlayersOnline.Text = DB.GetOnlinePlayers().ToString();
                if (DB.RequiresUpdate() && !DOWNLOADER.IsBusy) // if requires update
                {
                    btnPlay.Text = "Update";
                    btnPlay.Enabled = true;
                    PLAYABLE = false;
                }
                else if (!DB.RequiresUpdate() && !DOWNLOADER.IsBusy) // if ready
                {
                    btnPlay.Enabled = true;
                    PLAYABLE = true;
                }
                else if (DB.RequiresUpdate() && DOWNLOADER.IsBusy) // if downloading patch
                {
                    btnPlay.Enabled = false;
                    PLAYABLE = false;
                }
            }
            else
            {
                lblPlayersOnline.Text = "0";
                picRealmStatus.Image = Properties.Resources.off;
                PLAYABLE = true;
                btnPlay.Enabled = true;
            }
            btnPlay.Refresh();
        }
        private void lblExit_Click(object sender, EventArgs e)
        {
            Environment.Exit(-1);
        }
        private void lblMinimize_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }
        private void lblMenu_MouseEnter(object sender, EventArgs e)
        {
            Label lbl = (Label)sender;
            lbl.ForeColor = Color.LightGray;
        }
        private void lblMenu_MouseLeave(object sender, EventArgs e)
        {
            Label lbl = (Label)sender;
            lbl.ForeColor = DB.bColor;
        }
        private void lblRegister_Click(object sender, EventArgs e)
        {
            if(_REGISTER!="0")
                System.Diagnostics.Process.Start(_REGISTER);
        }
        private void lblForum_Click(object sender, EventArgs e)
        {
            if (_FORUM != "0")
                System.Diagnostics.Process.Start(_FORUM);
        }
        private void lblAccount_Click(object sender, EventArgs e)
        {
            if (_ACCOUNT != "0")
                System.Diagnostics.Process.Start(_ACCOUNT);
        }
        private void blizzPlay_Click(object sender, EventArgs e)
        {
            Debug.WriteLine(PLAYABLE.ToString());
            if (PLAYABLE)
            {
                try
                {
                    System.Diagnostics.Process.Start("wow.exe");
                    Environment.Exit(-1);
                }
                catch { }
            }
            else if (DB.RequiresUpdate())
            {
                btnPlay.Enabled = false;
                DownloadNewPatches();
            }
        }
        void DownloadNewPatches()
        {
            try
            {
                int new_patch_version; string new_patch_name, patch_link;
                DB.GetPatch(DB.GetNewPatchId(), out new_patch_version, out new_patch_name, out patch_link);
                Debug.WriteLine("----------------\nPatch ID: " + DB.GetNewPatchId().ToString() + "\nPatch Name: " + new_patch_name + "\nPatch Version: " + new_patch_version + "\nPatch Link: " + patch_link + "\n------------------");
                DownloadPatch(DB.GetNewPatchId(), new_patch_version, new_patch_name, patch_link);
            }
            catch { }
        }

        void DownloadPatch(int patch_id,int new_patch_version, string new_patch_name,string new_patch_link)
        {
            if (DOWNLOADER.IsBusy)
            {
                Debug.WriteLine("DOWNLOADER was busy at patch " + patch_id.ToString()+". Waiting...");
                while (DOWNLOADER.IsBusy) ;
            }
            Thread = new System.Threading.Thread(() =>
            {
                DOWNLOADER.DownloadProgressChanged += new System.Net.DownloadProgressChangedEventHandler(DOWNLOADER_DownloadProgressChanged);
                DOWNLOADER.DownloadFileCompleted += new AsyncCompletedEventHandler(DOWNLOADER_DownloadFileCompleted);
                if (new_patch_name.Contains(".MPQ") || new_patch_name.Contains(".mpq"))
                {
                    DOWNLOADER.DownloadFileAsync(new Uri(new_patch_link), "Data\\" + new_patch_name);
                }
                else DOWNLOADER.DownloadFileAsync(new Uri(new_patch_link), new_patch_name);
            });
            Thread.Start();

        }
        void DOWNLOADER_DownloadProgressChanged(object sender, System.Net.DownloadProgressChangedEventArgs e)
        {
            this.BeginInvoke((MethodInvoker)delegate {
                double bytesIn = double.Parse(e.BytesReceived.ToString());
                double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
                double percentage = bytesIn / totalBytes * 100;
                picTop2.BringToFront();
                picTop2.Size = new Size(Convert.ToInt32(6.48 * int.Parse(Math.Truncate(percentage).ToString())), 13) ;
            });
        }

        private void ConnectionCheck_Tick(object sender, EventArgs e)
        {
            reloadAllData();
        }
        private void lblSetRealmlist_Click(object sender, EventArgs e)
        {
            string[] locales = { "frFR", "deDE", "enGB", "enUS", "itIT", "koKR", "zhCN", "zhTW", "ruRU", "esES", "esMX", "ptBR" };
            foreach(string locale in locales)
            {
                if (System.IO.Directory.Exists("Data\\" + locale))
                {
                    System.IO.File.WriteAllText("Data\\" + locale + "\\realmlist.wtf", "set realmlist "+DB.GetRealmlist());
                }
            }
        }
        private void lblDeleteCache_Click(object sender, EventArgs e)
        {
            if (System.IO.Directory.Exists("Cache"))
            {
                System.IO.Directory.Delete("Cache",true);
            }
        }
        void DOWNLOADER_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            this.BeginInvoke((MethodInvoker)delegate {
                picTop.BringToFront();
                DownloadComplete();
            });
        }
        private void picBNet_MouseEnter(object sender, EventArgs e)
        {
            picBNet.Image = Properties.Resources.img_bnetButton_hovered;
        }
        private void picBNet_MouseLeave(object sender, EventArgs e)
        {
            picBNet.Image = Properties.Resources.img_bnetButton_normal;
        }
        private void picPlayersOnline_MouseEnter(object sender, EventArgs e)
        {
            picPlayersOnline.Image = Properties.Resources.online_hover;
        }
        private void picPlayersOnline_MouseLeave(object sender, EventArgs e)
        {
            picPlayersOnline.Image = Properties.Resources.online;
        }
        private void picPlayersOnline_MouseDown(object sender, MouseEventArgs e)
        {
            picPlayersOnline.Image = Properties.Resources.online;
        }
        private void picPlayersOnline_MouseUp(object sender, MouseEventArgs e)
        {
            picPlayersOnline.Image = Properties.Resources.online;
        }

        private void lblChangelogReadMore_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(_CHANGELOG);
        }

        private void lblNewsReadMore_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(_NEWS);
        }

        private void picPlayersOnline_Click(object sender, EventArgs e)
        {
            if (SERVER_ONLINE)
            {
                PlayersOnline src = new PlayersOnline();
                src.ShowDialog();
            }
        }
        private void lblMenu_MouseDown(object sender, EventArgs e)
        {
            Label lbl = (Label)sender;
            lbl.ForeColor = Color.DimGray;
        }
        private void lblMenu_MouseEnter(object sender, MouseEventArgs e)
        {
            Label lbl = (Label)sender;
            lbl.ForeColor = Color.LightGray;
        }
        private void lblMenu_MouseDown(object sender, MouseEventArgs e)
        {
            Label lbl = (Label)sender;
            lbl.ForeColor = Color.DimGray;
        } 
        void DownloadComplete()
        {
            int new_patch_version, patch_id = DB.GetNewPatchId();
            string new_patch_name,  patch_link;
            DB.GetPatch(patch_id, out new_patch_version, out new_patch_name, out patch_link);
            Debug.WriteLine("Updated patch " + patch_id.ToString());
            if (patch_id == 1)
            {
                Properties.Settings.Default.patch_version_1 = new_patch_version;
                Properties.Settings.Default.patch_name_1 = new_patch_name;
                Properties.Settings.Default.Save();
            }
            else if (patch_id == 2)
            {
                Properties.Settings.Default.patch_version_2 = new_patch_version;
                Properties.Settings.Default.patch_name_2 = new_patch_name;
                Properties.Settings.Default.Save();
            }
            else if (patch_id == 3)
            {
                Properties.Settings.Default.patch_version_3 = new_patch_version;
                Properties.Settings.Default.patch_name_3 = new_patch_name;
                Properties.Settings.Default.Save();
            }
            else if (patch_id == 4)
            {
                Properties.Settings.Default.patch_version_4 = new_patch_version;
                Properties.Settings.Default.patch_name_4 = new_patch_name;
                Properties.Settings.Default.Save();
            }
            else if (patch_id == 5)
            {
                Properties.Settings.Default.patch_version_5 = new_patch_version;
                Properties.Settings.Default.patch_name_5 = new_patch_name;
                Properties.Settings.Default.Save();
            }
            if (DB.RequiresUpdate()) DownloadNewPatches();
        }
    }
}
