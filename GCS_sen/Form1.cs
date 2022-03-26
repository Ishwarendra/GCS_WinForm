using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using System.IO;



namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        
       public new struct packets
        {
           public  int teamID,missiontime,packetcount,pressure,flightstate, pitch, roll;
           public  decimal temperature, voltage, gpsalt, gpslat, gpslong,altitude;
           
        };
        public int f1 = 0, f2 = 0, f3 = 0, f4 = 0, f5 = 0, f6 = 0;
        public  packets data;
        public Int16 fl = 0;
        public System.IO.TextWriter file;
        public Form1()       {
            InitializeComponent();
            foreach (string s in System.IO.Ports.SerialPort.GetPortNames())
            {
                comselect.Items.Add(s);
            }
        }
        private delegate void EventHandle();

        private void Form1_Load(object sender, EventArgs e)
        {
            file = new StreamWriter("D:/CANSAT2018_TLM_1731_GARUDA.csv");
            file.WriteLine("Team ID,Mission Time (seconds),Packet Count,Altitude (m),Pressure (Pa),Temperature (C),Voltage (V),GPS Time (HH:MM:SS),GPS Latitude (Degrees),GPS Longitude (Degrees),GPS Altitude (m), GPS Sats.,Pitch (Degrees),Roll (Degrees),Blade Spin Rate (RPM),Software State, Camera Direction (Degrees)");
            file.Close();
            

        }

        private void connect_Click(object sender, EventArgs e)
        {
            try
            {
                port.PortName = comselect.Text;
                port.Open();
                rtb.AppendText("Connection to GCS Arduino Established");
            }
            catch
            {
                rtb.AppendText("Error Connecting to GCS arduino");
            }
            


        }
        private void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string packet = "";

            rtb.Invoke(new Action(() => { rtb.AppendText("Recieved"); }));
            fl++;
            if(fl<4)
            {
                string i;
                i = Convert.ToString(port.ReadExisting());

                return;

            }
            
            
            while (true)
            {
                var c = port.ReadChar();
                if(c == '#')
                    break;
            }

            packet = port.ReadTo("$");

            


            if (packet.Length < 4)
            {
                data.flightstate = Convert.ToInt32(packet);
                goto FLIGHTSTATE;
            }
           



                else if(packet.Length > 40)
            {
                
               string[] pack = packet.Split(',');
                
                data.missiontime = Convert.ToUInt16(pack[1]);
                
                    data.packetcount = Convert.ToInt32(pack[2]);
                    data.altitude = Convert.ToDecimal(pack[3]);
                    data.pressure = Convert.ToInt32(pack[4]);
                    data.temperature = Convert.ToDecimal(pack[5]);
                    data.voltage = Convert.ToDecimal(pack[6]);
                    data.gpslat = Convert.ToDecimal(pack[8]);
                    data.gpslong = Convert.ToDecimal(pack[9]);
                    data.gpslong = Convert.ToDecimal(pack[10]);
                    data.roll = Convert.ToInt32(pack[12]);
                    data.pitch = Convert.ToInt32(pack[13]);
                    data.flightstate = Convert.ToInt32(pack[15]);


                dgv.Invoke(new Action(() => { dgv.Rows.Add(pack[0], pack[1], pack[2], pack[3], pack[4], pack[5], pack[6], pack[7], pack[8], pack[9], pack[10], pack[11], pack[12], pack[13], pack[14], pack[15], pack[16]); }));
                file = new StreamWriter("D:/CANSAT2018_TLM_1731_GARUDA.csv", true);
                file.WriteLine(packet);
                file.Close();
                altitude.Invoke(new Action(() => { altitude.Series["Series1"].Points.AddXY(data.missiontime, data.altitude); }));
                pressure.Invoke(new Action(() => { pressure.Series["Series1"].Points.AddXY(data.missiontime, data.pressure); }));
                temperature.Invoke(new Action(() => { temperature.Series["Series1"].Points.AddXY(data.missiontime, data.temperature); }));
                voltage.Invoke(new Action(() => { voltage.Series["Series1"].Points.AddXY(data.missiontime, data.voltage); }));
                gpsalt.Invoke(new Action(() => { gpsalt.Series["Series1"].Points.AddXY(data.missiontime, data.gpsalt); }));
                rollpitch.Invoke(new Action(() => { rollpitch.Series["Roll"].Points.AddXY(data.missiontime, data.roll); }));
                rollpitch.Invoke(new Action(() => { rollpitch.Series["Pitch"].Points.AddXY(data.missiontime, data.pitch); }));
                gps.Invoke(new Action(() => { gps.Series["Series1"].Points.AddXY(data.gpslong, data.gpslat); }));
            }
            FLIGHTSTATE:
            switch(data.flightstate)
            {
                case 2:
                    cal.Invoke(new Action(() => { cal.Checked = true; }));
                    if(f2 == 0)
                    {
                        rtb.Invoke(new Action(() => { rtb.AppendText("\nCalibrated....\nWaiting for Launch...."); }));
                        f2++;
                    }
                    break;
                case 3:
                    ld.Invoke(new Action(() => { ld.Checked = true; }));
                    if(f3==0)
                    {
                        rtb.Invoke(new Action(() => { rtb.AppendText("\nLaunch detected....\nAscending...."); }));
                        f3++;
                    }
                    break;
                case 4:
                    Desc.Invoke(new Action(() => { Desc.Checked = true; }));
                    if(f4==0)
                    {
                        rtb.Invoke(new Action(() => { rtb.AppendText("\nParachute Deployed....\nDescending...."); }));
                        f4++;
                    }
                    break;
                case 5:
                    pr.Invoke(new Action(() => { pr.Checked = true; }));
                    cs.Invoke(new Action(() => { cs.Checked = true; }));
                    if(f5==0)
                    {
                        rtb.Invoke(new Action(() => { rtb.AppendText("\nPayload Released"); }));
                        f5++;
                    }
                    break;
                case 6:
                    cs.Checked = false;
                    ab.Checked = true;
                    if(f6==0)
                    {
                        rtb.AppendText("\nLanded");
                        f6++;
                    }
                    break;
                default:
                    break;
            }
        }

        private void calibrate_Click(object sender, EventArgs e)
        {
            try
            {
                port.Write("#CALIBRATE$");
                
                calibrate.Enabled = false;
            }
            catch
            {
                rtb.Invoke(new Action(() => { rtb.AppendText("error calibrating"); }));
            }
        }

        private void restart_Click(object sender, EventArgs e)
        {
            port.Write("#RESTART$");
            file = new StreamWriter("D:/CANSAT2018_TLM_1731_GARUDA.csv", true);
            file.WriteLine("RESTART");
            file.Close();
        }
    }
}
