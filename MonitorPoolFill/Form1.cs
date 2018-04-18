using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WiringPi;
using System.Timers;
using System.IO;

namespace MonitorPoolFill
{
    public partial class MonitorPoolFill : Form
    {
        public enum PinMode
        {
            HIGH = 1,
            LOW = 0,
            READ = 0,
            WRITE = 1
        }
        const int meterPin = 31; // GPIO 6, physical pin 31
        static public ulong IntCount = 0, LastCount = 0;
        static bool resetCounts = false;
        static bool flowOn = false;
        static DateTime LastTime, ThisTime, FlowStarted, FlowStopped;
        private static System.Timers.Timer aTimer;
        delegate void SetErrorCallback(string text);
        delegate void SetTextCallback(string text);
        public int rtbLineCount = 0;
        public double FiveMinTotal = 0.0, HourTotal = 0.0, DayTotal = 0.0, FlowThisFill = 0.0;
        public ulong FiveMinCount = 0, HourCount = 0, DayCount = 0;
        public double ScaleFactor = 1800.0f;  // assume 3000 clicks ber gallon changed to 1800 4/2/18 20:54
        string StringToPrint;
        public MonitorPoolFill()
        {
            if (Init.WiringPiSetupPhys() == -1) // The WiringPiSetup method is static and returns either true or false. Calling it in this fashion
            {
                rtb_PoolFill.AppendText("GPIO Init Failed!\n"); //If we reach this point, the GPIO Interface did not successfully initialize
            }

            if (PiThreadInterrupts.wiringPiISR(meterPin,
                (int)PiThreadInterrupts.InterruptLevels.INT_EDGE_FALLING, meterClick) < 0) // Initialize the Interrupt and set the callback to our method above
            {
                throw new Exception("Unable to Initialize ISR");
            }
            InitializeComponent();
            SetTimer();
        }



        static void meterClick()
        {
            IntCount++;

        }
        private void SetTimer()
        {
            // Create a timer 
            // timerLength is in seconds. Timer expects milliseconds
            aTimer = new System.Timers.Timer(1000);
            // Hook up the Elapsed event for the timer. 

            aTimer.Elapsed += OnTimedEvent;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
            LastTime = DateTime.Now;
            ThisTime = DateTime.Now;

        }
        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            
            aTimer.Interval = 60000;           
            SaveFlow();
            
        }
        private void SaveFlow( )
        {

            int TimeDelta;
            ulong CountDelta;
            bool printed = false;
            double CountsPerSecond;
            double GalDelta;


            ThisTime = DateTime.Now;
            int ThisMinute, ThisHour;
            ThisMinute = ThisTime.Minute;
            ThisHour = ThisTime.Hour;
            string timeString, CountString, flowString;
            TimeDelta = (int)ThisTime.TimeOfDay.TotalMilliseconds - (int)LastTime.TimeOfDay.TotalMilliseconds;
            CountDelta = (IntCount - LastCount); 
            LastTime = ThisTime;
            GalDelta = (double)CountDelta / ScaleFactor;
            double GalPerMin;
            FiveMinTotal = FiveMinTotal + GalDelta;
            HourTotal = HourTotal + GalDelta;
            DayTotal = DayTotal + GalDelta;
            CountsPerSecond = (double)(CountDelta) * (double)(1000.0f / (double)TimeDelta);
            GalPerMin = (CountsPerSecond * 60.0) / ScaleFactor;

            timeString = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss");
            if((GalPerMin > 0.1) && !flowOn)
            {
                flowOn = true;
                FlowStarted = DateTime.Now;
                tb_FlowStarted.Text = timeString;
                lbl_FlowStatus.Text = "Flow On";
                pnl_Status.BackColor = Color.LightGreen;
                FlowThisFill = 0.0;
                flowString = "Flow Started " + FlowStarted;
                WriteFlowFile(flowString);
            }
            if ((GalPerMin < 0.2) && flowOn)
            {
                flowOn = false;
                FlowStopped = DateTime.Now;
                tb_FlowStopped.Text = timeString;
                lbl_FlowStatus.Text = "Flow Off";
                pnl_Status.BackColor = Color.LightBlue;
                flowString = "Flow Stopped " + timeString + "  " + FlowThisFill.ToString();
                WriteFlowFile(flowString);
            }
            if (flowOn)
            {
                FlowThisFill = FlowThisFill + GalPerMin;
                lbl_FlowStatus.Text = "Flow On";
                pnl_Status.BackColor = Color.LightGreen;
            }
            else
            {
                lbl_FlowStatus.Text = "Flow Off";
                pnl_Status.BackColor = Color.LightBlue;
            }


            tb_FlowThisFIll.Text = FlowThisFill.ToString("F2");

            if (IntCount > LastCount && LastCount != 0 && GalPerMin > 0.2)
            {
                StringToPrint = timeString + " Total  " + IntCount.ToString() + " Delta   "
                    + (IntCount - LastCount).ToString() + " Counts/Sec  " + CountsPerSecond.ToString("F2") + 
                    " Gal Per Min " + GalPerMin.ToString("F2") + "\n";

                SetText(StringToPrint);
                printed = true;
            }

            if ((ThisMinute % 5 == 0 && FiveMinCount == 0 && FiveMinTotal > 0.2) || (ThisMinute == 0 && HourCount == 0))
            {
                if (!printed)
                {
                    StringToPrint = timeString + " Total  " + IntCount.ToString() + " Delta   "
                    + (IntCount - LastCount).ToString() + " Counts/Sec  " + CountsPerSecond.ToString("F2") +
                    " Gal Per Min " + GalPerMin.ToString("F2") + "\n";

                    SetText(StringToPrint);
                }
                CountString = timeString + ", 5Min= " + FiveMinTotal.ToString("F2") + ", Hr= " + HourTotal.ToString("F2") + ", Da= " + DayTotal.ToString("F2");
                WriteFile(CountString);
                FiveMinCount = 1;
                FiveMinTotal = 0;
                if (ThisMinute == 0) 
                {
                    HourTotal = 0;
                    HourCount = 1;
                }
            }
            if (ThisMinute % 5 != 0) FiveMinCount = 0;
            if (ThisMinute != 0) HourCount = 0;
            if (ThisHour == 0 && DayCount == 0)
            {
                DayTotal = 0;
                DayCount = 1;
            }
            if (ThisHour != 0) DayCount = 0;

            if (resetCounts)
            {
                IntCount = 0;
                FiveMinTotal = 0;
                HourTotal = 0;
                DayTotal = 0;
                StringToPrint = " Reset Counts \n";
                SetText(StringToPrint);
                resetCounts = false;
            }
            LastCount = IntCount;
        }
        //*******************************************************************************************
        private void WriteFlowFile(string text)
        {
            try
            {
                string dateString = DateTime.Now.ToString("yy-MM");
                //dateString = DateTime.Now.DayOfYear + "_" + dateString;
                string FileName = "/home/pi/PoolData/" + dateString + "_Flow.txt";

                using (StreamWriter writer = new StreamWriter(FileName, true))
                {
                    writer.WriteLine(text);
                }
            }
            catch (Exception ex)
            {
                SetErrorText("error lin WriteFlowFile " + ex.Message + "\n");
            }
        }
       
        //*******************************************************************************************
        private void WriteFile(string text)
        {
            try
            {
                string dateString = DateTime.Now.ToString("yy-MM-dd");
                dateString = DateTime.Now.DayOfYear + "_" + dateString;
                string FileName = "/home/pi/PoolData/" + dateString + ".txt";

                using (StreamWriter writer = new StreamWriter(FileName, true))
                {
                    writer.WriteLine(text);
                }
            }
            catch (Exception ex)
            {
                SetErrorText("error lin WriteFile " + ex.Message + "\n");
            }
        }

        //*******************************************************************************************
        private void btn_Reset_Click(object sender, EventArgs e)
        {
            resetCounts = true;
        }
        //*******************************************************************************************
        private void SetErrorText(string text)
        {
            // InvokeRequired compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            try
            {

                if (this.tb_ErrorList.InvokeRequired)
                {
                    SetErrorCallback d = new SetErrorCallback(SetErrorText);
                    this.Invoke(d, new object[] { text });
                }
                else
                {
                    this.tb_ErrorList.AppendText(text);
                }


            }
            catch (Exception ex)
            {
                // If we get an error here put it in the datalist
                SetText("SetText error " + ex.Message + "\n");
            }

        }
        //*******************************************************************************************
        private void SetText(string text)
        {
            // InvokeRequired compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            try
            {

                if (this.rtb_PoolFill.InvokeRequired)
                {
                    SetTextCallback d = new SetTextCallback(SetText);
                    this.Invoke(d, new object[] { text });
                }
                else
                {
                    if (rtbLineCount > 20)
                    {
                        rtb_PoolFill.ResetText();
                        rtbLineCount = 0;
                        string dateString = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss");
                        tb_ErrorList.AppendText(dateString + "  Reset rtb_PoolFill \n");
                    }
                    this.rtb_PoolFill.AppendText(text);
                    rtbLineCount++;
                }


            }
            catch (Exception ex)
            {
                SetErrorText("SetText error " + ex.Message + "\n");
            }

        }
        //*******************************************************************************************
        //private void ReadFile()
        //{
        //    try
        //    {
        //        string dateString = DateTime.Now.ToString("yy-MM-dd");
        //        dateString = DateTime.Now.DayOfYear + "_" + dateString;
        //        string FileName = "/home/pi/PoolData/" + dateString + ".txt";

        //        if (onPC)
        //        {
        //            string ComputerName = System.Windows.Forms.SystemInformation.ComputerName;
        //            dateString = "318_17-11-14";
        //            FileName = "G:\\Projects\\C#Projects\\TempMonitorProto\\TempMonitorProto\\" + dateString + ".txt";
        //            jpegFileName = "G:\\Projects\\C#Projects\\TempMonitorProto\\TempMonitorProto\\TempData.jpg";
        //            if (ComputerName == "MCISERVER-64")
        //            {
        //                FileName = "E:\\Projects\\C#Projects\\TempMonitorProto\\TempMonitorProto\\" + dateString + ".txt";
        //                jpegFileName = "E:\\Projects\\C#Projects\\TempMonitorProto\\TempMonitorProto\\TempData.jpg";
        //            }

        //        }
        //        string TempString;
        //        tempCount = 0;
        //        using (StreamReader reader = new StreamReader(FileName, true))
        //        {
        //            while (!reader.EndOfStream && tempCount < 2000)
        //            {
        //                TempString = reader.ReadLine();
        //                SetText(TempString + "\n");
        //                ParseLine(TempString);

        //                for (int j = 0; j < tempFromFileCount; j++)
        //                {
        //                    tempArr[tempCount, j] = tempFromFile[j];
        //                }
        //                dateArrFloat[tempCount] = (float)(timeOfFileReading.Date - new DateTime(2017, 1, 1)).TotalDays * (60.0f * 24.0f)
        //                + (float)(timeOfFileReading.Hour * 60.0f + timeOfFileReading.Minute);

        //                dateArr[tempCount++] = timeOfFileReading;

        //                if (tempCount > shiftPoint)
        //                    ShiftData(shiftAmount);
        //            }
        //        }
        //        if (tempCount > shiftPoint)
        //            ShiftData(shiftAmount);
        //    }
        //    catch (Exception ex)
        //    {
        //        SetErrorText("error line 243 reading file " + ex.Message + "\n");
        //    }
        //}
    }
}
