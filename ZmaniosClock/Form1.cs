using System;
using System.Drawing;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using JewishCalendar;


namespace ZmaniosClock
{
    public partial class Form1 : Form
    {
        private readonly Location _location;
        private DateTime _today = DateTime.Now;
        private DateTime _now;
        private JewishDate _jdate;
        private int _netzSeconds, _shkiaSeconds;
        private double _zmaniosDaytimeSecondsPerHour,
                    _zmaniosDaytimeSecondsPerMinute,
                    _zmaniosDaytimeSecondsPerSecond,
                    _zmaniosNighttimeSecondsPerHour,
                    _zmaniosNighttimeSecondsPerMinute,
                    _zmaniosNighttimeSecondsPerSecond;

        //Double buffers everything
        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                // Turn on WS_EX_COMPOSITED
                cp.ExStyle |= 0x02000000;
                return cp;
            }
        }        
        public Form1()
        {
            InitializeComponent();
            SetDoubleBuffered(this.panel1);
            SetDoubleBuffered(this.panel2);
            SetDoubleBuffered(this.panel3);
            this._now = this._today;
            _location = new Location("Modiin Illit", 2, 31.932622, -35.042898)
            {
                Elevation = 340,
                IsInIsrael = true
            };
        }

        
        private void Form1_Load(object sender, EventArgs e)
        {
            this.timer2.Start();
            this.SetZmanim();            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if(this.panel2.Visible)
            {
                this.panel2.Visible = false;
                this.Height -= (panel2.Height + 20);
            }
            else
            {
                this.panel2.Visible = true;
                this.Height += (panel2.Height + 20);
            }
        }


        private void SetZmanim()

        {
            this.timer1.Stop();

            this._jdate = new JewishDate(this._today);
            var ns = Zmanim.GetNetzShkia(DateTime.Now, this._location);
            this._netzSeconds = ns[0].TotalSeconds;
            this._shkiaSeconds = ns[1].TotalSeconds;

            var totalDaySeconds = this._shkiaSeconds - this._netzSeconds;
            this._zmaniosDaytimeSecondsPerHour = totalDaySeconds / 12d;
            this._zmaniosDaytimeSecondsPerMinute = totalDaySeconds / 720d;
            this._zmaniosDaytimeSecondsPerSecond = totalDaySeconds / 43200d;

            var totalNightSeconds = 86400 - totalDaySeconds;
            this._zmaniosNighttimeSecondsPerHour = totalNightSeconds / 12d;
            this._zmaniosNighttimeSecondsPerMinute = totalNightSeconds / 720d;
            this._zmaniosNighttimeSecondsPerSecond = totalNightSeconds / 43200d;


            var nowSeconds = DateTime.Now.TimeOfDay.TotalSeconds;

            this.richTextBox1.Clear();

            this.richTextBox1.Text += $@"{this._jdate.ToLongDateString()}
{this._today.ToLongDateString()}
Hanetz Hachama: {ns[0].ToString24H(true)}
Shkias Hachama: {ns[1].ToString24H(true)}
Hour Zmanios Day: {this._zmaniosDaytimeSecondsPerHour / 60:N2} minutes
Hour Zmanios Night: {this._zmaniosNighttimeSecondsPerHour / 60:N2}  minutes";


            if (this._netzSeconds <= nowSeconds && this._shkiaSeconds > nowSeconds)
            {
                //DayTime
                this.timer1.Interval = Convert.ToInt32(this._zmaniosDaytimeSecondsPerSecond * 1000);
            }
            else
            {
                this.timer1.Interval = Convert.ToInt32(this._zmaniosNighttimeSecondsPerSecond * 1000);
            }

            this.timer1.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            
            double hour, minute, second;
            this._now = DateTime.Now;
            if (this._now.Day != this._today.Day)
            {
                this._today = this._now;
                this.SetZmanim();
            }

            var nowSeconds = this._now.TimeOfDay.TotalSeconds;

            if (this._netzSeconds <= nowSeconds && this._shkiaSeconds > nowSeconds)
            {
                //DayTime
                double sinceNetz = nowSeconds - this._netzSeconds;
                hour = sinceNetz / this._zmaniosDaytimeSecondsPerHour;
                minute = (sinceNetz % this._zmaniosDaytimeSecondsPerHour) /
                    this._zmaniosDaytimeSecondsPerMinute;
                second = (sinceNetz % this._zmaniosDaytimeSecondsPerMinute) /
                    this._zmaniosDaytimeSecondsPerSecond;
            }
            else
            {
                double sinceShkia;
                if (nowSeconds > this._shkiaSeconds)
                {
                    sinceShkia = nowSeconds - this._shkiaSeconds;
                }
                else
                {
                    sinceShkia = 86400 - this._shkiaSeconds + nowSeconds;
                }
                hour = sinceShkia / this._zmaniosNighttimeSecondsPerHour;
                minute = (sinceShkia % this._zmaniosNighttimeSecondsPerHour) /
                    this._zmaniosNighttimeSecondsPerMinute;
                second = (sinceShkia % this._zmaniosNighttimeSecondsPerMinute) /
                    this._zmaniosNighttimeSecondsPerSecond;
            }
            this.label1.Text = $"{(int)hour:D2}:{(int)minute:D2}:{(int)second:D2}";            
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            this.panel3.Invalidate();
        }
        private void panel3_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawString(this._now.ToString("HH:mm:ss"), this.panel3.Font, Brushes.Lime, 0, 0);
        }

        private static void SetDoubleBuffered(Control c)
        {
            //Taxes: Remote Desktop Connection and painting
            //http://blogs.msdn.com/oldnewthing/archive/2006/01/03/508694.aspx
            if (SystemInformation.TerminalServerSession)
                return;

            PropertyInfo aProp =
                  typeof(Control).GetProperty(
                        "DoubleBuffered",
                        BindingFlags.NonPublic |
                        BindingFlags.Instance);

            aProp.SetValue(c, true, null);
        }

    }
}
