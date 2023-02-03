using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar;
using System.Xml.Linq;

namespace AutoBackupRevisedEdition
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            //WindowChrome
            this.FormBorderStyle = FormBorderStyle.None;
            int radius = 12;
            int diameter = radius * 2;
            System.Drawing.Drawing2D.GraphicsPath gp = new System.Drawing.Drawing2D.GraphicsPath();

            // 左上
            gp.AddPie(0, 0, diameter, diameter, 180, 90);
            // 右上
            gp.AddPie(this.Width - diameter, 0, diameter, diameter, 270, 90);
            // 左下
            gp.AddPie(0, this.Height - diameter, diameter, diameter, 90, 90);
            // 右下
            gp.AddPie(this.Width - diameter, this.Height - diameter, diameter, diameter, 0, 90);
            // 中央
            gp.AddRectangle(new Rectangle(radius, 0, this.Width - diameter, this.Height));
            // 左
            gp.AddRectangle(new Rectangle(0, radius, radius, this.Height - diameter));
            // 右
            gp.AddRectangle(new Rectangle(this.Width - radius, radius, radius, this.Height - diameter));

            this.Region = new Region(gp);
        }

        public string WorldName;

        public string copyfromdirectory;

        public string copytodirectory;

        public string DestDirectory;

        public int BackupedMaxCount = 5;

        public int BackupedCounting = -1;

        public string[] BackupedDir = new string[5];

        public bool MaxCountOn;

        public string latestver;

        public int AnimeLabelDelay1 = -8;

        public string[] LoadWorld;

        public long LoadedFilesSize = 0;

        public int NowAppVersion = 110;

        private void button2_Click(object sender, EventArgs e)
        {
            CommonOpenFileDialog SelectCopyFrom = new CommonOpenFileDialog
            {
                Title = "バックアップをとるワールドフォルダを選択してください",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\AppData\Local\Packages\Microsoft.MinecraftUWP_8wekyb3d8bbwe\LocalState\games\com.mojang\minecraftWorlds",
                IsFolderPicker = true
            };
            try
            {
                if (SelectCopyFrom.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    copyfromdirectory = SelectCopyFrom.FileName;
                    textBox1.Text = new DirectoryInfo(copyfromdirectory).Name;
                    string imageLocation = copyfromdirectory + "\\world_icon.jpeg";
                    string path = copyfromdirectory + "\\levelname.txt";
                    StreamReader streamReader = new StreamReader(path, Encoding.GetEncoding("UTF-8"));
                    WorldName = streamReader.ReadToEnd();
                    streamReader.Close();
                    if(WorldName.Length> 18) AnimeLabel1.Start();
                    else AnimeLabel1.Stop();
                    label2.Text = "【" + WorldName + "】";
                    pictureBox1.ImageLocation = imageLocation;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            CommonOpenFileDialog SelectCopyTo = new CommonOpenFileDialog
            {
                Title = "バックアップの保存先のフォルダを選択してください",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                IsFolderPicker = true
            };
            try
            {
                if (SelectCopyTo.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    copytodirectory = ((CommonFileDialog)SelectCopyTo).FileName;
                    string name = new DirectoryInfo(copytodirectory).Name;
                    textBox2.Text = name;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void InterbalTime_Scroll(object sender, EventArgs e)
        {
            int num = InterbalTime.Value * 30;
            textBox3.Text = num.ToString();
        }

        private void EnterDelay(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Return)
            {
                return;
            }
            string s = textBox3.Text;
            if (int.TryParse(s, out int result))
            {
                if (result >= 30 && result <= 3600)
                {
                    InterbalTime.Value = result / 30;
                    return;
                }
                else if (result > 3600)
                {
                    MessageBox.Show("入力された値は大きすぎます。\n3600以下の値を入力してください。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                }
                else if (result < 30)
                {
                    MessageBox.Show("入力された値は小さすぎます。\n30以上の値を入力してください。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                }
            }
            else
            {
                MessageBox.Show("使用できない文字が含まれています。\n正しい値を入力してください。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
            this.ActiveControl = null;
        }

        private void EnterMaxCount(object sender, KeyEventArgs e)
        {

            if (e.KeyCode != Keys.Return)
            {
                return;
            }
            string s = textBox5.Text;
            if (int.TryParse(s, out int res))
            {
                if (res >= 3)
                {
                    BackupedMaxCount = res;
                }
                else
                {
                    MessageBox.Show("入力された値は小さすぎます。\n3以上の値を入力してください。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                }
                }
            else
            {
                MessageBox.Show("使用できない文字が含まれています。\n正しい値を入力してください。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string text = button1.Text;
            if (text.Contains("終了"))
            {
                Backup.Stop();
                button1.Text = "開始";
            }
            else
            {
                Backup.Interval = int.Parse(textBox3.Text) * 1000;
                MaxCountOn = checkBox1.Checked;
                Array.Resize(ref BackupedDir, BackupedMaxCount);
                Backup.Start();
                button1.Text = "終了";
            }
        }

        private void Backup_Tick(object sender, EventArgs e)
        {
            try
            {
                DateTime now = DateTime.Now;
                DestDirectory = copytodirectory + "\\" + textBox1.Text + "-" + now.Year + "Y" + now.Month + "M" + now.Day + "D" + now.Hour + "H" + now.Minute + "Min" + now.Second + "Sec-";
                CopyDirectory(copyfromdirectory, DestDirectory);
                if (MaxCountOn)
                {
                    BackupedCounting++;
                    if (BackupedCounting == BackupedMaxCount) BackupedCounting = 0;
                    if (BackupedDir[BackupedCounting] != null)
                    {
                        Directory.Delete(BackupedDir[BackupedCounting], true);
                    }
                    BackupedDir[BackupedCounting] = DestDirectory;
                }
                string name = new DirectoryInfo(DestDirectory).Name;
                textBox4.Text = textBox4.Text + "\r\n \r\n-" + now.Day + "日" + now.Hour + "時" + now.Minute + "分" + now.Second + "秒 ：\r\n" + textBox1.Text + " を " + copytodirectory + " に" + name + " としてコピーしました。";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public static void CopyDirectory(string copytodirectory, string DestDirectory)
        {
            if (!Directory.Exists(DestDirectory))
            {
                Directory.CreateDirectory(DestDirectory);
            }
            File.SetAttributes(DestDirectory, File.GetAttributes(copytodirectory));
            if (!DestDirectory.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                DestDirectory += Path.DirectorySeparatorChar;
            }
            string[] files = Directory.GetFiles(copytodirectory);
            string[] array = files;
            foreach (string text in array)
            {
                File.Copy(text, DestDirectory + Path.GetFileName(text), overwrite: true);
            }
            string[] directories = Directory.GetDirectories(copytodirectory);
            string[] array2 = directories;
            foreach (string path in array2)
            {
                CopyDirectory(path, DestDirectory + Path.GetFileName(path));
            }
        }

        private void ワールドの保存場所ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string arguments = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\AppData\\Local\\Packages\\Microsoft.MinecraftUWP_8wekyb3d8bbwe\\LocalState\\games\\com.mojang\\minecraftWorlds";
            Process.Start("EXPLORER.EXE", arguments);
        }

        private void 設定ファイルToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\J.S.\\AutoBackup.set";
                copyfromdirectory = File.ReadLines(path).Skip(0).First();
                copytodirectory = File.ReadLines(path).Skip(1).First();
                textBox3.Text = File.ReadAllLines(path).Skip(2).First();
                textBox1.Text = new DirectoryInfo(copyfromdirectory).Name;
                string imageLocation = copyfromdirectory + "\\world_icon.jpeg";
                string path2 = copyfromdirectory + "\\levelname.txt";
                StreamReader streamReader = new StreamReader(path2, Encoding.GetEncoding("UTF-8"));
                string text = streamReader.ReadToEnd();
                streamReader.Close();
                label2.Text = "【" + text + "】";
                pictureBox1.ImageLocation = imageLocation;
                string name = new DirectoryInfo(copytodirectory).Name;
                textBox2.Text = name;
            }
            catch (Exception ex)
            {
                textBox4.Text = textBox4.Text + "\r\n" + ex.Message;
            }
        }

        private void 保存SToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string text = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\J.S.";
            if (!Directory.Exists(text))
            {
                Directory.CreateDirectory(text);
            }
            File.WriteAllText(text + "\\AutoBackup.set", copyfromdirectory + "\r\n" + copytodirectory + "\r\n" + textBox3.Text);
        }

        private void ヘルプToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            AboutBox1 aboutBox = new AboutBox1();
            aboutBox.Show();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            WebClient webClient = new WebClient();
            try
            {
                webClient.Encoding = Encoding.UTF8;
                string text = webClient.DownloadString("https://jun-suzu.net/appver");
                int num = text.IndexOf("<AutoBackup>") + 12;
                int length = text.IndexOf("</AutoBackup>") - num;
                latestver = text.Substring(num, length);
                アップデートToolStripMenuItem.Text = "アップデート  (最新 " + latestver + " )";
            }
            catch (WebException ex)
            {
                アップデートToolStripMenuItem.Text = "アップデート  " + ex.Message;
            }
        }

        private void 終了XToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void アップデートToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int num = int.Parse(latestver.Replace(".", ""));
            if (num > NowAppVersion)
            {
                DialogResult dialogResult = MessageBox.Show("最新版があります。更新しますか？", "確認", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dialogResult == DialogResult.Yes)
                {
                    UpdateApplication();
                }
            }
            else if (num == NowAppVersion)
            {
                MessageBox.Show("このアプリケーションは最新です。\nアップデートをする必要はございません。\nもし問題が発生している場合は公式サイトをご確認ください。", "アップデートは不要です。", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
            else
            {
                MessageBox.Show("サーバー側で予期しない問題が起きているようです。\n時間が経ってからやり直してください。", "サーバーエラー", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
        }

        public void UpdateApplication()
        {
            string text = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\J.S.";
            string location = Assembly.GetExecutingAssembly().Location;
            if (!Directory.Exists(text))
            {
                Directory.CreateDirectory(text);
            }
            File.WriteAllText(text + "\\AutoBackupOutdated.bat", "@echo off\ndel " + location + "\ndel %0");
            ProcessStartInfo processStartInfo = new ProcessStartInfo();
            processStartInfo.FileName = @"C:\Windows\System32\\notepad.exe";
            processStartInfo.CreateNoWindow = true; // コマンドプロンプトを表示
            processStartInfo.UseShellExecute = false; // シェル機能オフ

            Process.Start(processStartInfo);
        }

        private void AnimeLabel1_Tick(object sender, EventArgs e)
        {
            AnimeLabelDelay1++;
            if (AnimeLabelDelay1 > WorldName.Length) AnimeLabelDelay1 = -4;
            if (AnimeLabelDelay1 < 0) {
                label2.Text= "【" + WorldName + "】";
                return; 
            }
            string lb2txt;
            if (AnimeLabelDelay1 + 15 >= WorldName.Length) lb2txt = WorldName.Substring(AnimeLabelDelay1);
            else lb2txt = WorldName.Substring(AnimeLabelDelay1,15);
            label2.Text = "【" + lb2txt + "】";
        }

        private Point mousePoint;

        private void menuStrip1_MouseDown(object sender, MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
            {
                //位置を記憶する
                mousePoint = new Point(e.X, e.Y);
            }
        }

        private void menuStrip1_MouseMove(object sender, MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
            {
                this.Location = new Point(
                    this.Location.X + e.X - mousePoint.X,
                    this.Location.Y + e.Y - mousePoint.Y);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            if (backgroundWorker1.IsBusy)
            {
                MessageBox.Show("現在別のワールドをロード中です。\nしばらくお待ちください。", "しばらくお待ちください。", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                LoadWorld = (string[])e.Data.GetData(DataFormats.FileDrop, false);
                userControl11.Visible = true;
                backgroundWorker1.RunWorkerAsync();
            }
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.All;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            string WorldDirectorysAddress = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\AppData\\Local\\Packages\\Microsoft.MinecraftUWP_8wekyb3d8bbwe\\LocalState\\games\\com.mojang\\minecraftWorlds";
            int LoadedFoldersCount = 0;
            LoadedFilesSize = 0;
            userControl11.FoldersCount = LoadWorld.Length;
            userControl11.FoldersC(0);
            userControl11.FilesPerSize = 0;
            foreach (string foldername in LoadWorld)
            {
                userControl11.FilesPerSize += GetDirectoryFileSize(foldername);
            }
            userControl11.ProgressC();
            userControl11.FilesPerS(0);
            foreach (string foldername in LoadWorld)
            {
                string foldersname = new DirectoryInfo(foldername).Name;
                string WorldDirectory = WorldDirectorysAddress + "\\" + foldersname;
                LoadDirectory(foldername, WorldDirectory);
                LoadedFoldersCount++;
                userControl11.FoldersC(LoadedFoldersCount);
            }
            userControl11.Visible = false;
        }
        public static long GetDirectoryFileSize(string stDirPath)
        {
            return GetDirectoryFileSize(new System.IO.DirectoryInfo(stDirPath));
        }
        public static long GetDirectoryFileSize(System.IO.DirectoryInfo hDirectoryInfo)
        {
            long lTotalSize = 0;

            // ディレクトリ内のすべてのファイルサイズを加算する
            foreach (System.IO.FileInfo cFileInfo in hDirectoryInfo.GetFiles())
            {
                lTotalSize += cFileInfo.Length;
            }

            // サブディレクトリ内のすべてのファイルサイズを加算する (再帰)
            foreach (System.IO.DirectoryInfo hDirInfo in hDirectoryInfo.GetDirectories())
            {
                lTotalSize += GetDirectoryFileSize(hDirInfo);
            }

            // 合計ファイルサイズを返す
            return lTotalSize;
        }

        public void LoadDirectory(string loadfromdirectory, string DestDirectory)
        {
            if (!Directory.Exists(DestDirectory))
            {
                Directory.CreateDirectory(DestDirectory);
            }
            File.SetAttributes(DestDirectory, File.GetAttributes(loadfromdirectory));
            if (!DestDirectory.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                DestDirectory += Path.DirectorySeparatorChar;
            }
            string[] files = Directory.GetFiles(loadfromdirectory);
            string[] array = files;
            foreach (string text in array)
            {
                File.Copy(text, DestDirectory + Path.GetFileName(text), overwrite: true);
                FileInfo filesize = new FileInfo(text);
                LoadedFilesSize += filesize.Length;
                userControl11.FilesPerS(LoadedFilesSize);
            }
            string[] directories = Directory.GetDirectories(loadfromdirectory);
            string[] array2 = directories;
            foreach (string path in array2)
            {
                LoadDirectory(path, DestDirectory + Path.GetFileName(path));
            }
        }
    }
}
