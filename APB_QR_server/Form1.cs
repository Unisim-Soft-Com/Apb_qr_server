using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace APB_QR_server
{
    public partial class Form1 : Form
    {
        private ApbQRServer _apbQrServer;
        public Form1()
        {
            InitializeComponent();
            Logger.InitLogger();
        }

        private void _apbQrServer_ServerStateChange(bool state)
        {
            if (state)
            {
                this.Invoke(new Action(() =>
                {
                    buttonStartServer.Enabled = false;
                    buttonStopServer.Enabled = true;
                    labelServerState.Text = "Сервер запущен!";
                    labelServerState.ForeColor = Color.Green;
                    notifyIcon1.Text = "Сервер: Запущен!";
                    notifyIcon1.BalloonTipText = "Сервер был запущен!";
                    notifyIcon1.ShowBalloonTip(1000);
                }));
            }
            else
            {
                this.Invoke(new Action(() =>
                {
                    buttonStartServer.Enabled = true;
                    buttonStopServer.Enabled = false;
                    labelServerState.Text = "Сервер остановлен!";
                    labelServerState.ForeColor = Color.Red;
                    notifyIcon1.Text = "Сервер: Остановлен!";
                    notifyIcon1.BalloonTipText = "Сервер был остановлен!";
                    notifyIcon1.ShowBalloonTip(1000);
                }));
                
            }
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ExpandForm();
        }

        private void CollapseForm()
        {
            this.Hide();
            notifyIcon1.Visible = true;
            notifyIcon1.ShowBalloonTip(1000);
        }

        private void ExpandForm()
        {
            this.Show();
            notifyIcon1.Visible = false;
            WindowState = FormWindowState.Normal;
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                CollapseForm();
            }
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
            _apbQrServer = new ApbQRServer();
            _apbQrServer.ServerStateChange += _apbQrServer_ServerStateChange;
            _apbQrServer.StartServerAsync();
        }

        private void buttonStartServer_Click(object sender, EventArgs e)
        {
            _apbQrServer.StartServerAsync();
        }

        private void buttonStopServer_Click(object sender, EventArgs e)
        {
            _apbQrServer.StopServer();
        }

        private void closeProgrammToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Environment.Exit(1);
        }
    }
}
