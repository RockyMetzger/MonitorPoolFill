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
        //const int meterPin = 13; // GPIO 27, physical pin 13
        static public ulong IntCount = 0, LastCount = 0, LastShow = 0;
        static public int  intervalInSeconds = 60;
        static bool resetCounts = false;
        static bool flowOn = false;
        static DateTime LastTime, ThisTime, FlowStarted, FlowStopped;
        private static System.Timers.Timer aTimer;
        delegate void SetErrorCallback(string text);
        delegate void SetTextCallback(string text);
        public int rtbLineCount = 0;
        public double FiveMinTotal = 0.0, HourTotal = 0.0, DayTotal = 0.0, FlowThisFill = 0.0;
        public ulong FiveMinCount = 0, HourCount = 0, DayCount = 0;
        public double ScaleFactor = 3012.0f;  // assume 3000 clicks per gallon changed to 1800 4/2/18 20:54
                                                // Changed to 3102 8/5/18
        string StringToPrint;
        static double VersionNumber = 3.01;
        public MonitorPoolFill()
        {
            GPIO.pullUpDnControl(meterPin, 1);   // May not be needed.
            if (Init.WiringPiSetupPhys() == -1) // The WiringPiSetup method is static and returns either true or false. Calling it in this fashion
            //    if (Init.WiringPiSetupGpio() == -1) // The WiringPiSetup method is static and returns either true or false. Calling it in this fashion
                {
                rtb_PoolFill.AppendText("GPIO Init Failed!\n"); //If we reach this point, the GPIO Interface did not successfully initialize
            }

            //GPIO.pinMode(meterPin,0);  // 0 = input
            //GPIO.pullUpDnControl(meterPin, 2);   // May not be needed.

            if (PiThreadInterrupts.wiringPiISR(meterPin,
                (int)PiThreadInterrupts.InterruptLevels.INT_EDGE_BOTH, meterClick) < 0) // Initialize the Interrupt and set the callback to our method above
            {
                throw new Exception("Unable to Initialize ISR");
            }
            InitializeComponent();
            lbl_Version.Text = "Ver " + VersionNumber.ToString("F2");
            SetTimer();
        }



        static void meterClick()
        {
            IntCount++;
          //  Console.WriteLine("IntCount  " + IntCount.ToString() + "\n");
          //  System.Threading.Thread.Sleep(5);
           // GPIO.digitalWrite(meterPin,0);             
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
            intervalInSeconds = Convert.ToInt32(tb_Interval.Text.ToString());
            aTimer.Interval = (intervalInSeconds * 1000);     
           // aTimer.Interval = (6000); 
            SaveFlow();
            
        }
        private void SaveFlow( )
        {

            int TimeDelta;
            ulong CountDelta;
            bool printed = false;
            double CountsPerSecond;
            double GalDelta;

            try
            { 
            ThisTime = DateTime.Now;
            int ThisMinute, ThisHour;
            ThisMinute = ThisTime.Minute;
            ThisHour = ThisTime.Hour;
            string timeString, CountString, flowString;
            TimeDelta = (int)ThisTime.TimeOfDay.TotalMilliseconds - (int)LastTime.TimeOfDay.TotalMilliseconds;
            CountDelta = (IntCount - LastCount);
            CountsPerSecond = (double)(CountDelta) * (double)(1000.0f / (double)TimeDelta);
            ulong SubCounts = 0;
            if (CountsPerSecond > 58  && cb_Noise.Checked == true)
            {
                SubCounts = (ulong)(60 * TimeDelta / 1000);
                IntCount = IntCount - SubCounts;
                if (IntCount < 0) IntCount = 0;
                CountsPerSecond = CountsPerSecond - 60;
                if (CountsPerSecond < 0) CountsPerSecond = 0;
                CountDelta = (IntCount - LastCount);
                if (LastCount > IntCount) CountDelta = 0;
            }
            LastTime = ThisTime;
            GalDelta = (double)CountDelta / ScaleFactor;
            double GalPerMin;
            FiveMinTotal = FiveMinTotal + GalDelta;
            HourTotal = HourTotal + GalDelta;
            DayTotal = DayTotal + GalDelta;
           
           

            GalPerMin = (CountsPerSecond * intervalInSeconds) / ScaleFactor;

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
            if ((GalPerMin < 0.11) && flowOn)
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

            if ((IntCount > LastCount && LastCount != 0 && GalPerMin > 0.2) || cb_Debug.Checked == true)
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
            catch (Exception ex)
            {
                // SetErrorText("error lin WriteFlowFile " + ex.Message + "\n");
                SetErrorText("SaveFlow error " + DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss") + "  " + ex.Message + "\n");
            }
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
               // SetErrorText("error lin WriteFlowFile " + ex.Message + "\n");
                SetErrorText("WriteFlowFile error " + DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss") + "  " + ex.Message + "\n");
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
                //SetErrorText("error lin WriteFile " + ex.Message + "\n");
                SetErrorText("WriteFile error " + DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss") + "  " + ex.Message + "\n");
            }
        }

        //*******************************************************************************************
        private void btn_Reset_Click(object sender, EventArgs e)
        {
            resetCounts = true;
        }
        //*******************************************************************************************
        //void SetCountsText(string text)
        //{
        //    // InvokeRequired compares the thread ID of the
        //    // calling thread to the thread ID of the creating thread.
        //    // If these threads are different, it returns true.
        //    try
        //    {

        //        if (this.tb_Interval.InvokeRequired)
        //        {
        //            SetErrorCallback d = new SetErrorCallback(SetCountsText);
        //            this.Invoke(d, new object[] { text });
        //        }
        //        else
        //        {
        //            tb_Interval.AppendText(text);
        //        }


        //    }
        //    catch (Exception ex)
        //    {
        //        // If we get an error here put it in the datalist
        //        SetText("SetCountsText error " + ex.Message + "\n");
        //    }

        //}
        //*******************************************************************************************
        void SetErrorText(string text)
        {
            // InvokeRequired compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            Console.WriteLine(text + "\n");
           
            if (1 == 1) return;
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
                //SetText("SetText error " + ex.Message + "\n");
                Console.WriteLine("SetErrorText error " + DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss") + "  " + ex.Message + "\n");
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
                    if (rtbLineCount == 4 && intervalInSeconds == 21)
                    {
                        intervalInSeconds = 60;
                        cb_Debug.Checked = false;
                        tb_Interval.Text = "60";
                    }
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
                SetErrorText("SetText error " + DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss")+ "  " + ex.Message + "\n");
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
