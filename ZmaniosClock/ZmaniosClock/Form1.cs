using System;
using System.Windows.Forms;
using JewishCalendar;


namespace ZmaniosClock
{
    public partial class Form1 : Form
    {
        private readonly Location _location;
        private DateTime _today = DateTime.Now;
        private int _netzSeconds, _shkiaSeconds;
        private double _zmaniosDaytimeSecondsPerHour,
                    _zmaniosDaytimeSecondsPerMinute,
                    _zmaniosDaytimeSecondsPerSecond,
                    _zmaniosNighttimeSecondsPerHour,
                    _zmaniosNighttimeSecondsPerMinute,
                    _zmaniosNighttimeSecondsPerSecond;


        public Form1()
        {
            InitializeComponent();
            _location = new Location("Modiin Illit", 2, 31.932622, -35.042898) { 
                Elevation = 340, IsInIsrael = true };
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.SetZmanim();
        }

        private void SetZmanim()

        {
            this.timer1.Stop();

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


            var now = DateTime.Now.TimeOfDay.TotalSeconds;

            if (this._netzSeconds <= now && this._shkiaSeconds > now)
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
            this.SuspendLayout();
            double hour, minute, second;
            var dt = DateTime.Now;
            if (dt.Day != this._today.Day)
            {
                this._today = dt;
                this.SetZmanim();
            }
            var nowSeconds = dt.TimeOfDay.TotalSeconds;

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
            this.ResumeLayout();
        }

    }
}
