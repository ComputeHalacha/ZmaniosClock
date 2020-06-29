using JewishCalendar;
using System;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;


namespace ZmaniosClock
{
    public partial class Form1 : Form
    {
        private PrivateFontCollection _pvc = new PrivateFontCollection();
        StringFormat _format = new StringFormat();        
        private Location _location;
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
            var name = Properties.Settings.Default.LocationName;
            this._location = Program.LocationsList.FirstOrDefault(l => l.Name == name);
            int fontLength = Properties.Resources.Quartz.Length;
            byte[] fontdata = Properties.Resources.Quartz;
            IntPtr data = System.Runtime.InteropServices.Marshal.AllocCoTaskMem(fontLength);
            System.Runtime.InteropServices.Marshal.Copy(fontdata, 0, data, fontLength);
            this._pvc.AddMemoryFont(data, fontLength);
            this._format.LineAlignment = StringAlignment.Center;
            this._format.Alignment = StringAlignment.Center;

            if (this._location == null)
            {
                this._location = Program.LocationsList.FirstOrDefault(l => l.Name == "Jerusalem");
            }
            this.cmbLocations.DataSource = Program.LocationsList;
            this.cmbLocations.SelectedItem = this._location;
            this.Text = "Zmanios Clock - " + this._location.Name;

        }


        private void Form1_Load(object sender, EventArgs e)
        {
            this.timer2.Start();
            this.SetZmanim();
            this.cmbLocations.SelectedIndexChanged += new System.EventHandler(this.cmbLocations_SelectedIndexChanged);
            this.panel3.Font = new Font(this._pvc.Families[0], this.panel3.Font.Size);
            this.panel1.Font = new Font(this._pvc.Families[0], 82);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void cmbLocations_SelectedIndexChanged(object sender, EventArgs e)
        {
            var location = Program.LocationsList.FirstOrDefault(l => l.Name == this.cmbLocations.Text);
            if (location != null && location.Name != this._location.Name)
            {
                this._location = location;
                Properties.Settings.Default.LocationName = location.Name;
                Properties.Settings.Default.Save();
                this.Text = "Zmanios Clock - " + this._location.Name;
                this.SetZmanim();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (this.panel2.Visible)
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

            this.panel1.Invalidate();
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            this.panel3.Invalidate();
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
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
            e.Graphics.DrawString($"{(int)hour:D2}:{(int)minute:D2}:{(int)second:D2}",
                this.panel1.Font, Brushes.Red, this.panel1.ClientRectangle, this._format);
        }

        private void panel3_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawString(this._now.ToString("HH:mm:ss"), this.panel3.Font, Brushes.Lime,this.panel3.ClientRectangle, this._format);
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
