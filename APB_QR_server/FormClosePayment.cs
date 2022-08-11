using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using Timer = System.Threading.Timer;

namespace APB_QR_server
{
    public partial class FormClosePayment : Form
    {
        private Timer timer;
        private DateTime dateTime;
        private bool isTimerTicking;

        public FormClosePayment(string Message, string Title)
        {
            InitializeComponent();
            this.Text = Title;
            this.richTextBox1.Text = Message;

        }

        private void FormClosePayment_Load(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
            this.Show();
            this.WindowState = FormWindowState.Normal;
            Logger.Log.Info("Создаю таймер");

            TimerCallback tcb = new TimerCallback(Timer_Tick);
            timer = new Timer(tcb, null, 1000, 1000);

            dateTime = DateTime.MinValue.AddMinutes(3);
        }

        private void Timer_Tick(object o)
        {
            isTimerTicking = true;
            Logger.Log.Info("Тик таймера");

            try
            {
                dateTime = dateTime.AddSeconds(-1.0);

                SafeInvoke(buttonCancelPayment, new Action(() =>
                {
                    buttonCancelPayment.Text = "Отменить платёж (" + dateTime.ToString("mm:ss") + ")";
                }));
                

                if (dateTime == DateTime.MinValue)
                {
                    Logger.Log.Info("Время вышло. Сбрасываю таймер. Вызываю метод нажатия кнопки отмены платежа");

                    timer.Change(Timeout.Infinite, -1);
                    SafeInvoke(buttonCancelPayment, new Action(() => {
                        buttonCancelPayment.PerformClick();
                    }));
                }

                //throw new Exception("test");
            }
            catch (Exception e)
            {
                Logger.Log.Info("Ошибка во время выполнения тика таймера. Сбрасываю таймер." 
                                + Environment.NewLine 
                                + e.Message);

                timer.Change(Timeout.Infinite, -1);
                Logger.Log.Info("Вызываю метод нажатия кнопки отмены платежа");

                SafeInvoke(buttonCancelPayment, new Action(() => {
                    buttonCancelPayment.PerformClick();
                }));
            }
            finally
            {
                Logger.Log.Info("Тик таймера - конец");
                isTimerTicking = false;
            }

        }

        public void SafeInvoke(Control control, Action action)
        {
            try
            {
                control.Invoke(action);
            }
            catch {}

        }

        

        private void FormClosePayment_FormClosing(object sender, FormClosingEventArgs e)
        {
            Logger.Log.Info("Форма отмены платежа закрывается. Сбрасываю таймер");

            timer.Change(Timeout.Infinite, -1);
            //Logger.Log.Info("Жду пока тик завершит работу");

            ////Ждем пока timer_Tick не закончит работу
            //do
            //{
                
            //} while (isTimerTicking);

            //Logger.Log.Info("Тик завершен. Закрытие формы");

        }
    }
}
