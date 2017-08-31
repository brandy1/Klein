using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using KClmtrBase;
using KClmtrBase.KClmtrWrapper;
using System.IO;
using System.Threading; //Thread.Sleep 需要引用它
using System.IO.Ports;
using SL_Tek_Studio_Pro;
using System.Collections;
using System.Globalization;
using System.Reflection;

namespace K_80
{
    public partial class MainForm : Form
    {
        //↓↓↓↓↓↓↓↓↓↓全域變數區域↓↓↓↓↓↓↓↓↓↓
        private KClmtrWrap kClmtr;
        private double Y_paramater = 3.4262590996323;
        private SL_ExcuteCmd Device =null;
        private string ELECSBOARD = "0x7422\r";
        SL_Device_Util.ScDeviceInfo[] UsbDeviceInfo;

        private double[] VRA_mapping_Brightness = new double[1024];//將1024組電壓分壓 轉換成亮度表現 提供查表Mapping

        //private double[] VP_voltage = new double[29];
        //private int[] VP_voltage_index = new int[29];
        //private float[] Gamma_curv = new float[256];
        private int[] VP_index = new int[29]  { 0, 1, 3, 5, 7, 9, 11, 13, 15,
                    24, 32, 48, 64, 96, 128, 160, 192, 208, 224, 232,
                    240, 242, 244, 246, 248, 250, 252, 254, 255};


        private double EstimateBrightness_Min;//推算標準Gamma亮度前 先用K80量測面板表現最暗灰階的亮度值 存於此
        private double EstimateBrightness_Max;//推算標準Gamma亮度前 先用K80量測面板表現最亮灰階的亮度值 存於此
        private double[] EstimateBrightness = new double[256];//推算符合Gamma曲線設定的亮度表現
        private double[] Actual_Brightness = new double[256];//實測亮度表現
        private double[] EstimateBrightness_toleranceP = new double[256];//推算符合Gamma曲線設定的亮度表現
        private double[] EstimateBrightness_toleranceN = new double[256];//推算符合Gamma曲線設定的亮度表現
        private double[] Tie_Estimate_Brightness = new double[29];//推算的亮度 計算的斜率
        private double[] Tie_Actual_Brightness = new double[29];//實測的亮度 計算的斜率


        private double[] Actual_Brightness_Slope = new double[29];//實測的亮度 計算的斜率
        private int[] Tie_Index_CalGet = new int[29];//推算的亮度 去找出的綁點的設定值
        private int[] Tie_ParameterSetting_MSB = new int[29];
        private int[] Tie_ParameterSetting_LSB = new int[29];

        private UInt16[] GP_OTM1911A = new UInt16[30]; //GP1~GP29 OTM1911 Gamma1綁點設定值存放處


        public BrightnessTie_struct[] EstimateBrightnessTie_struct = new BrightnessTie_struct[29];
        public BrightnessTie_struct[] ActualBrightnessTie_struct = new BrightnessTie_struct[29];


        public class BrightnessTie_struct
        {
            public int Start_Tie_Index;
            public int End_Tie_Index;
            public double Start_Tie_Brightness;
            public double End_Tie_Brightness;
        }


        private int G_value = 0;
        //↑↑↑↑↑↑↑↑↑↑全域變數區域↑↑↑↑↑↑↑↑↑↑


        public MainForm()
        {
            InitializeComponent();
            kClmtr = new KClmtrWrap();
            InitialSetting();

        }

        //↓↓↓↓↓↓↓↓↓↓公用副程式區域↓↓↓↓↓↓↓↓↓↓

        private  void InitialSetting()
        {
      
            //Create Copy Dll
            string eppdll = Application.StartupPath + "\\EPP2USB_DLL_V12.dll";    
            if (!File.Exists(eppdll))
            {            
                Assembly aObj = Assembly.GetExecutingAssembly();
                Stream sStream = aObj.GetManifestResourceStream("SL_Tek_Studio_Pro.Resources.EPP2USB_DLL_V12.dll");
                
                if (sStream == null)
                {
                    MessageBox.Show("read file error....");
                }
                else
                {
                    byte[] bySave = new byte[sStream.Length];
                    sStream.Read(bySave, 0, bySave.Length);
                    FileStream fsObj = new FileStream(eppdll, FileMode.CreateNew);
                    fsObj.Write(bySave, 0, bySave.Length);
                    fsObj.Close();
                }
            }

            UsbDeviceList();
        }


        private bool errorCheck(int error)
        {
            String stringError = "";
            //Avergring, just needs to display it
            error &= ~(int)KleinsErrorCodes.AVERAGING_LOW_LIGHT;
            //Resetting the FFT data
            error &= ~(int)KleinsErrorCodes.FFT_PREVIOUS_RANGE;
            //The data isn't ready to display yet
            error &= ~(int)KleinsErrorCodes.FFT_INSUFFICIENT_DATA;
            if (false)
            {
                error &= ~(int)KleinsErrorCodes.AIMING_LIGHTS;
            }
            if (true)
            {
                error &= ~(int)KleinsErrorCodes.BOTTOM_UNDER_RANGE;
                error &= ~(int)KleinsErrorCodes.TOP_OVER_RANGE;
                error &= ~(int)KleinsErrorCodes.OVER_HIGH_RANGE;

                error &= ~(int)KleinsErrorCodes.CONVERTED_NM;
                error &= ~(int)KleinsErrorCodes.KELVINS;
            }
            if (error > 0)
            {
                kClmtr.stopMeasuring();

                if ((error & (int)KleinsErrorCodes.CONVERTED_NM) > 0)
                {
                    stringError += "There was an error when coverting to NM with the measurement.\n";
                    error &= ~(int)KleinsErrorCodes.CONVERTED_NM;
                }
                if ((error & (int)KleinsErrorCodes.KELVINS) > 0)
                {
                    stringError += "There was an error when coverting to Kelvins with the measurement.\n";
                    error &= ~(int)KleinsErrorCodes.KELVINS;
                }
                if ((error & (int)KleinsErrorCodes.AIMING_LIGHTS) > 0)
                {
                    stringError += "The Aiming lights are on.\n";
                    error &= ~(int)KleinsErrorCodes.AIMING_LIGHTS;
                }
                if ((error & (int)(KleinsErrorCodes.BOTTOM_UNDER_RANGE
                    | KleinsErrorCodes.TOP_OVER_RANGE | KleinsErrorCodes.OVER_HIGH_RANGE)) > 0)
                {
                    stringError += "There was an error from the Klein device due to the Range switching with the measurement.\n";
                    error &= ~(int)KleinsErrorCodes.BOTTOM_UNDER_RANGE;
                    error &= ~(int)KleinsErrorCodes.TOP_OVER_RANGE;
                    error &= ~(int)KleinsErrorCodes.OVER_HIGH_RANGE;
                }
                if ((error & (int)KleinsErrorCodes.FFT_BAD_STRING) > 0)
                {
                    stringError += "The Flicker string from the Klein device was bad.\n";
                    error &= ~(int)KleinsErrorCodes.FFT_BAD_STRING;
                }
                if ((error & (int)(KleinsErrorCodes.NOT_OPEN
                    | KleinsErrorCodes.TIMED_OUT
                    | KleinsErrorCodes.LOST_CONNECTION)) > 0)
                {

                    kClmtr.closePort();
                    stringError += "The the Klein device as been unplugged\n";
                    error &= ~(int)(KleinsErrorCodes.NOT_OPEN
                        | KleinsErrorCodes.TIMED_OUT
                        | KleinsErrorCodes.LOST_CONNECTION);
                }
                if (error > 0)
                {
                    stringError += "There was an error with the measurement. Error code: " + error + "\n";
                }

                MessageBox.Show(stringError);
                return false;
            }
            else
            {
                return true;
            }
        }

        private void SetSeriesPort()
        {
            string[] Ports = SerialPort.GetPortNames();
            int PortCount = Ports.Length;
            if (PortCount > 0)
            {
                foreach (string port in Ports)
                {
                    ComPortSel_comboBox.Items.Add(port);
                    cbo_elecsport.Items.Add(port);
                }
            }
            else
            {
                ComPortSel_comboBox.Items.Add("Null");
                cbo_elecsport.Items.Add("Null");
            }
            ComPortSel_comboBox.SelectedIndex = 0;
            cbo_elecsport.SelectedIndex = 0;

        }

        //↑↑↑↑↑↑↑↑↑↑公用副程式區域↑↑↑↑↑↑↑↑↑↑

        private void Form1_Load(object sender, EventArgs e)
        {
            SetSeriesPort();
        }

        private void ComPortCheck_Button_Click(object sender, EventArgs e)
        {
            string SelStr = ComPortSel_comboBox.Text;
            if (String.IsNullOrEmpty(SelStr) || SelStr.CompareTo("Null") == 0) {ComPortState_label.Text = "No Deviice"; return; }

            Int32 ComPortNumber = Convert.ToInt32(SelStr.Substring(3));
            string[] test;

            ComPortState_label.Text = "Please wait for the connection!!";
            ComPortState_label.ForeColor = Color.Gray;

            kClmtr.connect(ComPortNumber);
            test = kClmtr.CalFileList;
            if(test[0] == "0: Factory Cal File")//土砲判斷K-80連線機制
            {
                ComPortState_label.Text = "K80 Already Connection!!";
                ComPortState_label.ForeColor = Color.Green;
                ComPortCheck_Button.BackColor = SystemColors.Control;
            }
            else
            {
                ComPortState_label.Text = "Please Check Again K-80 State";
                ComPortState_label.ForeColor = Color.Red;
                ComPortCheck_Button.BackColor = Color.GreenYellow;
            }
        }






        private void btn_oepnelecs_Click(object sender, EventArgs e)
        {
            Device = new SL_ExcuteCmd();
            string RdStr = null;
            string SelStr = cbo_elecsport.SelectedItem.ToString();
            if (String.IsNullOrEmpty(SelStr) || SelStr.CompareTo("Null") == 0) { lbl_elecs_status.Text = "No Deviice"; return; }

            if (Device.Open(SelStr))
            {
                Device.WriteRead("id",ref  RdStr);
                if(RdStr.Trim().CompareTo(ELECSBOARD) == 0)
                { 
                    lbl_elecs_status.Text = "E7422 Connect";
                    btn_oepnelecs.BackColor = Color.GreenYellow;
                }
            }
            else
            { 
                lbl_elecs_status.Text = "E7422 Not Connect";
                btn_oepnelecs.BackColor = SystemColors.Control;
                Device.Close();
                Device = null;
            }
        }

        private void btn_write_Click(object sender, EventArgs e)
        {



            
            if(Device != null ||  !Device.Status())
            {           
                Device.Write("mipi.write 0x05 0x28");
                Device.Write("mipi.write 0x05 0x10");
                Thread.Sleep(500);
                Device.Write("mipi.video.disable");
                Device.Write("mipi.clock.disable");
                Device.Write("gpio.write 0x0F");
                Thread.Sleep(100);
                Device.Write("gpio.write 0x07");
                Thread.Sleep(100);
                Device.Write("gpio.write 0x00 ");
                Thread.Sleep(100);
                Device.Write("power.off all");         
            }
        }


        private void GetEstimateBrightness_button_Click(object sender, EventArgs e)
        {
            int dive = comboBox1.SelectedIndex + 1;
            SL_WhiskyComm_Util WhiskeyUtil = new SL_WhiskyComm_Util();
            string rdstr = null;

            /*推算標準Gamma亮度前 先用K80量測面板表現最暗與最亮灰階的亮度值*/

            WhiskeyUtil.ImageFill(0, 0, 0);
            Thread.Sleep(1000);

            EstimateBrightness_Min = Math.Round(K80_Trigger_Measurement(dive), 4);//取到小數點第4位
            YMin_label.Text = "YMin=" + Convert.ToString(EstimateBrightness_Min);


            WhiskeyUtil.ImageFill(255, 255, 255);
            Thread.Sleep(1000);

            EstimateBrightness_Max = Math.Round(K80_Trigger_Measurement(dive), 4);//取到小數點第4位
            YMax_label.Text = "YMax=" + Convert.ToString(EstimateBrightness_Max);

            /*套用設定的Gamma值 推算出符合標準Gamma曲線答案的亮度表現*/

            double Gamma_set;
            double Gamma_set_tolerance;
            double temp, temp2;

            Gamma_set = Convert.ToDouble(GammaSet_textBox.Text);

            //EstimateBrightness_Max = 50;
            //EstimateBrightness_Min = 0.5;

            chart1.Series[0].Points.Clear();
            chart1.Series[1].Points.Clear();
            chart1.Series[2].Points.Clear();

            for (int Brightness = 0; Brightness < 256; Brightness++)
            {
                temp = (double)(256 - Brightness) / 256;

                EstimateBrightness[Brightness] = Math.Round(EstimateBrightness_Max * (float)Math.Pow(temp, Gamma_set), 4);

                chart1.Series[0].Points.AddXY(Brightness, EstimateBrightness[Brightness]);
            }

            //計算斜率
            for (int tie = 0; tie < 29; tie++)
            { EstimateBrightnessTie_struct[tie] = new BrightnessTie_struct(); }

            //for (int tie = 0; tie < 28; tie++)
            //{
            //    EstimateBrightnessTie_struct[tie].Start_Tie_Index = VP_index[tie];
            //    EstimateBrightnessTie_struct[tie].End_Tie_Index = VP_index[(tie + 1)];
            //    EstimateBrightnessTie_struct[tie].Start_Tie_Brightness = EstimateBrightness[VP_index[tie]];
            //    EstimateBrightnessTie_struct[tie].End_Tie_Brightness = EstimateBrightness[VP_index[(tie + 1)]];
            //}


            //for (int num = 0; num < 28; num++)
            //{
            //    //num=0 表示tie0~tie1的斜率
            //    //num=1 表示tie1~tie2的斜率...以此類推
            //    temp = EstimateBrightnessTie_struct[num].End_Tie_Brightness - EstimateBrightnessTie_struct[num].Start_Tie_Brightness;
            //    temp2 = EstimateBrightnessTie_struct[num].End_Tie_Index - EstimateBrightnessTie_struct[num].Start_Tie_Index;
            //    Estimate_Brightness_Slope[num] = Math.Round(temp / temp2,4);
            //}
            ////斜率取得完畢


            //計算誤差上界 並繪出圖
            Gamma_set_tolerance = Gamma_set + Convert.ToDouble(Gamma_set_tolerance_textBox.Text);
            for (int Brightness = 0; Brightness < 256; Brightness++)
            {
                temp = (double)(256 - Brightness) / 256;

                EstimateBrightness_toleranceP[Brightness] = Math.Round(EstimateBrightness_Max * (float)Math.Pow(temp, Gamma_set_tolerance), 4);

                chart1.Series[1].Points.AddXY(Brightness, EstimateBrightness_toleranceP[Brightness]);
            }

            //計算誤差下界 並繪出圖
            Gamma_set_tolerance = Gamma_set - Convert.ToDouble(Gamma_set_tolerance_textBox.Text);
            for (int Brightness = 0; Brightness < 256; Brightness++)
            {
                temp = (double)(256 - Brightness) / 256;

                EstimateBrightness_toleranceN[Brightness] = Math.Round(EstimateBrightness_Max * (float)Math.Pow(temp, Gamma_set_tolerance), 4);

                chart1.Series[2].Points.AddXY(Brightness, EstimateBrightness_toleranceN[Brightness]);
            }


            //透過上面的步驟可以計算出 符合Gamma曲線的亮度表現 
            //因為我們都是用亮度去做計算與評估 因此要把1024階電阻分壓選擇 
            //轉換成 這些分壓可以轉變成1024階亮度表現
            //因此 下面運算 取最大亮度與最小亮度表現
            //套入分壓階層 模擬出1024個Source電壓階層能產生出的相對應1024階亮度表現
            for (int num = 0; num < 1024; num++)
            {
                VRA_mapping_Brightness[num] = Math.Round(EstimateBrightness_Max * ((double)(1024 - num) / 1024), 5);//取到小數點第5位
            }

            //利用找最小值方式 去找尋
            double min_Y = EstimateBrightness_Max;
            double temp_a;

            for (int tie_cnt = 0; tie_cnt < 29; tie_cnt++)
            {
                min_Y = EstimateBrightness_Max;

                for (int num = 0; num < 1024; num++)
                {
                    temp_a = Math.Abs(EstimateBrightness[VP_index[tie_cnt]] - VRA_mapping_Brightness[num]);

                    if (temp_a <= min_Y)
                    {
                        min_Y = temp_a;
                        Tie_Index_CalGet[tie_cnt] = num;
                    }
                }
                Tie_ParameterSetting_MSB[tie_cnt] = Tie_Index_CalGet[tie_cnt] >> 8;
                Tie_ParameterSetting_LSB[tie_cnt] = Tie_Index_CalGet[tie_cnt] & 0x00FF;
            }

            Tie_ParameterSetting_to_LoadVP_TextData();
        }


        private double K80_Trigger_Measurement(int testcount)
        {
            double Y_Data = 0;
            double Big_Y_total = 0;
            double Big_Y_avg = 0;


            for (int i = 0; i < testcount; i++)
            {
                //↓↓↓↓↓K80 量測↓↓↓↓↓//
                Application.DoEvents();
                wMeasurement measure = kClmtr.getNextMeasurement(-1);
                Application.DoEvents();

                if (errorCheck(measure.errorcode))
                {
                    Y_Data = measure.BigY;
                    Big_Y_total = Y_Data + Big_Y_total;
                }
            }
            Big_Y_avg = Big_Y_total / testcount;

            Application.DoEvents();
            //↑↑↑↑↑K80量測↑↑↑↑↑//
            return Big_Y_avg;
        }


        //從Tie_ParameterSetting的內容顯示於Form上的Text
        private void Tie_ParameterSetting_to_LoadVP_TextData()
        {
            int temp_MSB;
            int temp_LSB;
            int cnt;
            cnt = 0;


            //VP0
            //temp_MSB = Tie_ParameterSetting_MSB[cnt];
            //temp_LSB = Tie_ParameterSetting_LSB[cnt];
            //temp_MSB <<= 8;
            textBox_VP0.Text = Convert.ToString(GP_OTM1911A[1]);
            cnt++;

            //VP1
            textBox_VP1.Text = Convert.ToString(GP_OTM1911A[2]);
            cnt++;

            //VP3
            textBox_VP3.Text = Convert.ToString(GP_OTM1911A[3]);
            cnt++;

            //VP5
            textBox_VP5.Text = Convert.ToString(GP_OTM1911A[4]);
            cnt++;

            //VP7
            textBox_VP7.Text = Convert.ToString(GP_OTM1911A[5]);
            cnt++;

            //VP9
            textBox_VP9.Text = Convert.ToString(GP_OTM1911A[6]);
            cnt++;

            //VP11
            textBox_VP11.Text = Convert.ToString(GP_OTM1911A[7]);
            cnt++;

            //VP13
            textBox_VP13.Text = Convert.ToString(GP_OTM1911A[8]);
            cnt++;

            //VP15
            textBox_VP15.Text = Convert.ToString(GP_OTM1911A[9]);
            cnt++;

            //VP24
            textBox_VP24.Text = Convert.ToString(GP_OTM1911A[10]);
            cnt++;

            //VP32
            textBox_VP32.Text = Convert.ToString(GP_OTM1911A[11]);
            cnt++;

            //VP48
            textBox_VP48.Text = Convert.ToString(GP_OTM1911A[12]);
            cnt++;

            //VP64
            textBox_VP64.Text = Convert.ToString(GP_OTM1911A[13]);
            cnt++;

            //VP96
            textBox_VP96.Text = Convert.ToString(GP_OTM1911A[14]);
            cnt++;

            //VP128
            textBox_VP128.Text = Convert.ToString(GP_OTM1911A[15]);
            cnt++;

            //VP160
            textBox_VP160.Text = Convert.ToString(GP_OTM1911A[16]);
            cnt++;

            //VP192
            textBox_VP192.Text = Convert.ToString(GP_OTM1911A[17]);
            cnt++;

            //VP208
            textBox_VP208.Text = Convert.ToString(GP_OTM1911A[18]);
            cnt++;

            //VP224
            textBox_VP224.Text = Convert.ToString(GP_OTM1911A[19]);
            cnt++;

            //VP232
            textBox_VP232.Text = Convert.ToString(GP_OTM1911A[20]);
            cnt++;

            //VP240
            textBox_VP240.Text = Convert.ToString(GP_OTM1911A[21]);
            cnt++;

            //VP242
            textBox_VP242.Text = Convert.ToString(GP_OTM1911A[22]);
            cnt++;

            //VP244
            textBox_VP244.Text = Convert.ToString(GP_OTM1911A[23]);
            cnt++;

            //VP246
            textBox_VP246.Text = Convert.ToString(GP_OTM1911A[24]);
            cnt++;

            //VP248
            textBox_VP248.Text = Convert.ToString(GP_OTM1911A[25]);
            cnt++;

            //VP250
            textBox_VP250.Text = Convert.ToString(GP_OTM1911A[26]);
            cnt++;

            //VP252
            textBox_VP252.Text = Convert.ToString(GP_OTM1911A[27]);
            cnt++;

            //VP254
            textBox_VP254.Text = Convert.ToString(GP_OTM1911A[28]);
            cnt++;

            //VP255
            textBox_VP255.Text = Convert.ToString(GP_OTM1911A[29]);
            cnt++;
        }


        //從Form上的Text擷取Data去Tie_ParameterSetting
        private void LoadVP_TextData_to_Tie_ParameterSetting()
        {
            int temp_MSB;
            int temp_LSB;

            int cnt;
            cnt = 0;

            //VP0
            temp_MSB = Convert.ToInt16(textBox_VP0.Text);
            temp_MSB >>= 8;//轉出來為16進制
            Tie_ParameterSetting_MSB[cnt] = temp_MSB;

            temp_LSB = Convert.ToInt16(textBox_VP0.Text);
            temp_LSB = temp_LSB & 0x00FF;//轉出來為16進制
            Tie_ParameterSetting_LSB[cnt] = temp_LSB;
            //MSB = "0x"+Convert.ToString(temp_MSB,16);
            //LSB = "0x" + Convert.ToString(temp_LSB,16);
            cnt = cnt + 1;

            //VP1
            temp_MSB = Convert.ToInt16(textBox_VP1.Text);
            temp_MSB >>= 8;//轉出來為16進制
            Tie_ParameterSetting_MSB[cnt] = temp_MSB;

            temp_LSB = Convert.ToInt16(textBox_VP1.Text);
            temp_LSB = temp_LSB & 0x00FF;//轉出來為16進制
            Tie_ParameterSetting_LSB[cnt] = temp_LSB;
            cnt = cnt + 1;

            //VP3
            temp_MSB = Convert.ToInt16(textBox_VP3.Text);
            temp_MSB >>= 8;//轉出來為16進制
            Tie_ParameterSetting_MSB[cnt] = temp_MSB;

            temp_LSB = Convert.ToInt16(textBox_VP3.Text);
            temp_LSB = temp_LSB & 0x00FF;//轉出來為16進制
            Tie_ParameterSetting_LSB[cnt] = temp_LSB;
            cnt = cnt + 1;

            //VP5
            temp_MSB = Convert.ToInt16(textBox_VP5.Text);
            temp_MSB >>= 8;//轉出來為16進制
            Tie_ParameterSetting_MSB[cnt] = temp_MSB;

            temp_LSB = Convert.ToInt16(textBox_VP5.Text);
            temp_LSB = temp_LSB & 0x00FF;//轉出來為16進制
            Tie_ParameterSetting_LSB[cnt] = temp_LSB;
            cnt = cnt + 1;

            //VP7
            temp_MSB = Convert.ToInt16(textBox_VP7.Text);
            temp_MSB >>= 8;//轉出來為16進制
            Tie_ParameterSetting_MSB[cnt] = temp_MSB;

            temp_LSB = Convert.ToInt16(textBox_VP7.Text);
            temp_LSB = temp_LSB & 0x00FF;//轉出來為16進制
            Tie_ParameterSetting_LSB[cnt] = temp_LSB;
            cnt = cnt + 1;

            //VP9
            temp_MSB = Convert.ToInt16(textBox_VP9.Text);
            temp_MSB >>= 8;//轉出來為16進制
            Tie_ParameterSetting_MSB[cnt] = temp_MSB;

            temp_LSB = Convert.ToInt16(textBox_VP9.Text);
            temp_LSB = temp_LSB & 0x00FF;//轉出來為16進制
            Tie_ParameterSetting_LSB[cnt] = temp_LSB;
            cnt = cnt + 1;

            //VP11
            temp_MSB = Convert.ToInt16(textBox_VP11.Text);
            temp_MSB >>= 8;//轉出來為16進制
            Tie_ParameterSetting_MSB[cnt] = temp_MSB;

            temp_LSB = Convert.ToInt16(textBox_VP11.Text);
            temp_LSB = temp_LSB & 0x00FF;//轉出來為16進制
            Tie_ParameterSetting_LSB[cnt] = temp_LSB;
            cnt = cnt + 1;

            //VP13
            temp_MSB = Convert.ToInt16(textBox_VP13.Text);
            temp_MSB >>= 8;//轉出來為16進制
            Tie_ParameterSetting_MSB[cnt] = temp_MSB;

            temp_LSB = Convert.ToInt16(textBox_VP13.Text);
            temp_LSB = temp_LSB & 0x00FF;//轉出來為16進制
            Tie_ParameterSetting_LSB[cnt] = temp_LSB;
            cnt = cnt + 1;

            //VP15
            temp_MSB = Convert.ToInt16(textBox_VP15.Text);
            temp_MSB >>= 8;//轉出來為16進制
            Tie_ParameterSetting_MSB[cnt] = temp_MSB;

            temp_LSB = Convert.ToInt16(textBox_VP15.Text);
            temp_LSB = temp_LSB & 0x00FF;//轉出來為16進制
            Tie_ParameterSetting_LSB[cnt] = temp_LSB;
            cnt = cnt + 1;

            //VP24
            temp_MSB = Convert.ToInt16(textBox_VP24.Text);
            temp_MSB >>= 8;//轉出來為16進制
            Tie_ParameterSetting_MSB[cnt] = temp_MSB;

            temp_LSB = Convert.ToInt16(textBox_VP24.Text);
            temp_LSB = temp_LSB & 0x00FF;//轉出來為16進制
            Tie_ParameterSetting_LSB[cnt] = temp_LSB;
            cnt = cnt + 1;

            //VP32
            temp_MSB = Convert.ToInt16(textBox_VP32.Text);
            temp_MSB >>= 8;//轉出來為16進制
            Tie_ParameterSetting_MSB[cnt] = temp_MSB;

            temp_LSB = Convert.ToInt16(textBox_VP32.Text);
            temp_LSB = temp_LSB & 0x00FF;//轉出來為16進制
            Tie_ParameterSetting_LSB[cnt] = temp_LSB;
            cnt = cnt + 1;

            //VP48
            temp_MSB = Convert.ToInt16(textBox_VP48.Text);
            temp_MSB >>= 8;//轉出來為16進制
            Tie_ParameterSetting_MSB[cnt] = temp_MSB;

            temp_LSB = Convert.ToInt16(textBox_VP48.Text);
            temp_LSB = temp_LSB & 0x00FF;//轉出來為16進制
            Tie_ParameterSetting_LSB[cnt] = temp_LSB;
            cnt = cnt + 1;

            //VP64
            temp_MSB = Convert.ToInt16(textBox_VP64.Text);
            temp_MSB >>= 8;//轉出來為16進制
            Tie_ParameterSetting_MSB[cnt] = temp_MSB;

            temp_LSB = Convert.ToInt16(textBox_VP64.Text);
            temp_LSB = temp_LSB & 0x00FF;//轉出來為16進制
            Tie_ParameterSetting_LSB[cnt] = temp_LSB;
            cnt = cnt + 1;

            //VP96
            temp_MSB = Convert.ToInt16(textBox_VP96.Text);
            temp_MSB >>= 8;//轉出來為16進制
            Tie_ParameterSetting_MSB[cnt] = temp_MSB;

            temp_LSB = Convert.ToInt16(textBox_VP96.Text);
            temp_LSB = temp_LSB & 0x00FF;//轉出來為16進制
            Tie_ParameterSetting_LSB[cnt] = temp_LSB;
            cnt = cnt + 1;

            //VP128
            temp_MSB = Convert.ToInt16(textBox_VP128.Text);
            temp_MSB >>= 8;//轉出來為16進制
            Tie_ParameterSetting_MSB[cnt] = temp_MSB;

            temp_LSB = Convert.ToInt16(textBox_VP128.Text);
            temp_LSB = temp_LSB & 0x00FF;//轉出來為16進制
            Tie_ParameterSetting_LSB[cnt] = temp_LSB;
            cnt = cnt + 1;

            //VP160
            temp_MSB = Convert.ToInt16(textBox_VP160.Text);
            temp_MSB >>= 8;//轉出來為16進制
            Tie_ParameterSetting_MSB[cnt] = temp_MSB;

            temp_LSB = Convert.ToInt16(textBox_VP160.Text);
            temp_LSB = temp_LSB & 0x00FF;//轉出來為16進制
            Tie_ParameterSetting_LSB[cnt] = temp_LSB;
            cnt = cnt + 1;

            //VP192
            temp_MSB = Convert.ToInt16(textBox_VP192.Text);
            temp_MSB >>= 8;//轉出來為16進制
            Tie_ParameterSetting_MSB[cnt] = temp_MSB;

            temp_LSB = Convert.ToInt16(textBox_VP192.Text);
            temp_LSB = temp_LSB & 0x00FF;//轉出來為16進制
            Tie_ParameterSetting_LSB[cnt] = temp_LSB;
            cnt = cnt + 1;

            //VP208
            temp_MSB = Convert.ToInt16(textBox_VP208.Text);
            temp_MSB >>= 8;//轉出來為16進制
            Tie_ParameterSetting_MSB[cnt] = temp_MSB;

            temp_LSB = Convert.ToInt16(textBox_VP208.Text);
            temp_LSB = temp_LSB & 0x00FF;//轉出來為16進制
            Tie_ParameterSetting_LSB[cnt] = temp_LSB;
            cnt = cnt + 1;

            //VP224
            temp_MSB = Convert.ToInt16(textBox_VP224.Text);
            temp_MSB >>= 8;//轉出來為16進制
            Tie_ParameterSetting_MSB[cnt] = temp_MSB;

            temp_LSB = Convert.ToInt16(textBox_VP224.Text);
            temp_LSB = temp_LSB & 0x00FF;//轉出來為16進制
            Tie_ParameterSetting_LSB[cnt] = temp_LSB;
            cnt = cnt + 1;

            //VP232
            temp_MSB = Convert.ToInt16(textBox_VP232.Text);
            temp_MSB >>= 8;//轉出來為16進制
            Tie_ParameterSetting_MSB[cnt] = temp_MSB;

            temp_LSB = Convert.ToInt16(textBox_VP232.Text);
            temp_LSB = temp_LSB & 0x00FF;//轉出來為16進制
            Tie_ParameterSetting_LSB[cnt] = temp_LSB;
            cnt = cnt + 1;

            //VP240
            temp_MSB = Convert.ToInt16(textBox_VP240.Text);
            temp_MSB >>= 8;//轉出來為16進制
            Tie_ParameterSetting_MSB[cnt] = temp_MSB;

            temp_LSB = Convert.ToInt16(textBox_VP240.Text);
            temp_LSB = temp_LSB & 0x00FF;//轉出來為16進制
            Tie_ParameterSetting_LSB[cnt] = temp_LSB;
            cnt = cnt + 1;

            //VP242
            temp_MSB = Convert.ToInt16(textBox_VP242.Text);
            temp_MSB >>= 8;//轉出來為16進制
            Tie_ParameterSetting_MSB[cnt] = temp_MSB;

            temp_LSB = Convert.ToInt16(textBox_VP242.Text);
            temp_LSB = temp_LSB & 0x00FF;//轉出來為16進制
            Tie_ParameterSetting_LSB[cnt] = temp_LSB;
            cnt = cnt + 1;

            //VP244
            temp_MSB = Convert.ToInt16(textBox_VP244.Text);
            temp_MSB >>= 8;//轉出來為16進制
            Tie_ParameterSetting_MSB[cnt] = temp_MSB;

            temp_LSB = Convert.ToInt16(textBox_VP244.Text);
            temp_LSB = temp_LSB & 0x00FF;//轉出來為16進制
            Tie_ParameterSetting_LSB[cnt] = temp_LSB;
            cnt = cnt + 1;

            //VP246
            temp_MSB = Convert.ToInt16(textBox_VP246.Text);
            temp_MSB >>= 8;//轉出來為16進制
            Tie_ParameterSetting_MSB[cnt] = temp_MSB;

            temp_LSB = Convert.ToInt16(textBox_VP246.Text);
            temp_LSB = temp_LSB & 0x00FF;//轉出來為16進制
            Tie_ParameterSetting_LSB[cnt] = temp_LSB;
            cnt = cnt + 1;

            //VP248
            temp_MSB = Convert.ToInt16(textBox_VP248.Text);
            temp_MSB >>= 8;//轉出來為16進制
            Tie_ParameterSetting_MSB[cnt] = temp_MSB;

            temp_LSB = Convert.ToInt16(textBox_VP248.Text);
            temp_LSB = temp_LSB & 0x00FF;//轉出來為16進制
            Tie_ParameterSetting_LSB[cnt] = temp_LSB;
            cnt = cnt + 1;

            //VP250
            temp_MSB = Convert.ToInt16(textBox_VP250.Text);
            temp_MSB >>= 8;//轉出來為16進制
            Tie_ParameterSetting_MSB[cnt] = temp_MSB;

            temp_LSB = Convert.ToInt16(textBox_VP250.Text);
            temp_LSB = temp_LSB & 0x00FF;//轉出來為16進制
            Tie_ParameterSetting_LSB[cnt] = temp_LSB;
            cnt = cnt + 1;

            //VP252
            temp_MSB = Convert.ToInt16(textBox_VP252.Text);
            temp_MSB >>= 8;//轉出來為16進制
            Tie_ParameterSetting_MSB[cnt] = temp_MSB;

            temp_LSB = Convert.ToInt16(textBox_VP252.Text);
            temp_LSB = temp_LSB & 0x00FF;//轉出來為16進制
            Tie_ParameterSetting_LSB[cnt] = temp_LSB;
            cnt = cnt + 1;

            //VP254
            temp_MSB = Convert.ToInt16(textBox_VP254.Text);
            temp_MSB >>= 8;//轉出來為16進制
            Tie_ParameterSetting_MSB[cnt] = temp_MSB;

            temp_LSB = Convert.ToInt16(textBox_VP254.Text);
            temp_LSB = temp_LSB & 0x00FF;//轉出來為16進制
            Tie_ParameterSetting_LSB[cnt] = temp_LSB;
            cnt = cnt + 1;

            //VP255
            temp_MSB = Convert.ToInt16(textBox_VP255.Text);
            temp_MSB >>= 8;//轉出來為16進制
            Tie_ParameterSetting_MSB[cnt] = temp_MSB;

            temp_LSB = Convert.ToInt16(textBox_VP255.Text);
            temp_LSB = temp_LSB & 0x00FF;//轉出來為16進制
            Tie_ParameterSetting_LSB[cnt] = temp_LSB;
            cnt = cnt + 1;
        }


        private void WTie_ParameterSettingt_to_GammaRegister()
        {
            //↓↓↓↓↓Gamma R+ Register Setting↓↓↓↓↓//
            //
            //↑↑↑↑↑Gamma R+ Register Setting↑↑↑↑↑//

            //↓↓↓↓↓Gamma R- Register Setting↓↓↓↓↓//
            //
            //↑↑↑↑↑Gamma R- Register Setting↑↑↑↑↑//


            //↓↓↓↓↓Gamma G+ Register Setting↓↓↓↓↓//
            //
            //↑↑↑↑↑Gamma G+ Register Setting↑↑↑↑↑//

            //↓↓↓↓↓Gamma G- Register Setting↓↓↓↓↓//
            //
            //↑↑↑↑↑Gamma G- Register Setting↑↑↑↑↑//


            //↓↓↓↓↓Gamma B+ Register Setting↓↓↓↓↓//
            //
            //↑↑↑↑↑Gamma B+ Register Setting↑↑↑↑↑//

            //↓↓↓↓↓Gamma B- Register Setting↓↓↓↓↓//
            //
            //↑↑↑↑↑Gamma B- Register Setting↑↑↑↑↑//
        }

        //Read Gamma Parameter Setting from Gamma Register to Tie_ParameterSettingt[0~28]
        private void GammaRegister_to_WTie_ParameterSetting()
        {


            //↓↓↓↓↓Gamma R+ Register Setting↓↓↓↓↓//
            //
            //↑↑↑↑↑Gamma R+ Register Setting↑↑↑↑↑//

            //↓↓↓↓↓Gamma R- Register Setting↓↓↓↓↓//
            //
            //↑↑↑↑↑Gamma R- Register Setting↑↑↑↑↑//


            //↓↓↓↓↓Gamma G+ Register Setting↓↓↓↓↓//
            //
            //↑↑↑↑↑Gamma G+ Register Setting↑↑↑↑↑//

            //↓↓↓↓↓Gamma G- Register Setting↓↓↓↓↓//
            //
            //↑↑↑↑↑Gamma G- Register Setting↑↑↑↑↑//


            //↓↓↓↓↓Gamma B+ Register Setting↓↓↓↓↓//
            //
            //↑↑↑↑↑Gamma B+ Register Setting↑↑↑↑↑//

            //↓↓↓↓↓Gamma B- Register Setting↓↓↓↓↓//
            //
            //↑↑↑↑↑Gamma B- Register Setting↑↑↑↑↑//

        }



        private void button2_Click(object sender, EventArgs e)
        {
            Info_textBox.Text = "";
            //從Form上的Text擷取Data去Tie_ParameterSetting
            LoadVP_TextData_to_Tie_ParameterSetting();

            //Load Gamma Parameter Setting from Tie_ParameterSettingt[0~28] to Gamma Register 
            WTie_ParameterSettingt_to_GammaRegister();

            Info_textBox.Text = "從控制盤載入IC暫存器完畢!";
        }

        private void button5_Click(object sender, EventArgs e)
        {
            SL_WhiskyComm_Util WhiskeyUtil = new SL_WhiskyComm_Util();
            WhiskeyUtil.MipiWrite(0x05, 0x10);
            WhiskeyUtil.MipiWrite(0x05, 0x28);

            Info_textBox.Text = "";
            //Read Gamma Parameter Setting from Gamma Register to Tie_ParameterSettingt[0~28]
            //GammaRegister_to_WTie_ParameterSetting();
            OTM1911A_GammaReadRegister(0xE100);


            //從Tie_ParameterSetting的內容顯示於Form上的Text
            Tie_ParameterSetting_to_LoadVP_TextData();

            Info_textBox.Text = "從IC讀出至控制盤 Done!";

            //WhiskeyUtil.MipiWrite(0x05, 0x29);
            //WhiskeyUtil.ImageFill(0, 255, 0);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int dive = comboBox1.SelectedIndex + 1;
            double diff_min = EstimateBrightness_Max;
            double diff_min_last = EstimateBrightness_Max;
            byte tie_gray = 0;
            byte flag=0;

            SL_WhiskyComm_Util WhiskeyUtil = new SL_WhiskyComm_Util();

            OTM1911A_GammaReadRegister(0xE100);

            Tie_Actual_Brightness[0] = EstimateBrightness_Max;

            //Tie_Actual_Brightness[tie] 實測亮度 @ tie灰階上
            //VP_index[tie] 
            //{0, 1, 3, 5, 7, 9, 11, 13, 15,
            //        24, 32, 48, 64, 96, 128, 160, 192, 208, 224, 232,
            //        240, 242, 244, 246, 248, 250, 252, 254, 255};
            //Tie_Estimate_Brightness

            //先把綁點推算的亮度從EstimateBrightness DataBase放到變數Tie_Estimate_Brightness去計算
            for (int tie=0; tie<29; tie++) //tir=0 時亮度最亮
            {
                Tie_Estimate_Brightness[tie] = EstimateBrightness[VP_index[tie]];
            }

            double min = 0, temp = 0 ;
            //依序點綁點處的灰階 並且用K80量測
            for (int tie = 0; tie < 29; tie++) //tir=0 時亮度最亮
            {
                min = EstimateBrightness_Max;
                ReTry:

                tie_gray = Convert.ToByte(255-VP_index[tie]);

                WhiskeyUtil.ImageFill(tie_gray, tie_gray, tie_gray);
                Thread.Sleep(1000);

                Tie_Actual_Brightness[tie] = Math.Round(K80_Trigger_Measurement(dive), 4);//取到小數點第4位

                //請注意使用OTM1911A_GammaSetRegisterMapping 功能時 其TieNum為1~29 從1開始計算
                //因此tie使用時 與OTM1911A的TieNum 請注意序列安排

                temp = Math.Abs(Tie_Actual_Brightness[tie] - Tie_Estimate_Brightness[tie]);

                if (min > temp)
                {
                    min = temp;
                    if (Tie_Actual_Brightness[tie] > Tie_Estimate_Brightness[tie])
                    {//實測亮度 大於標準答案亮度 處置~>應該調降亮度
                        if(flag == 0x10)//表示剛剛為亮度反折點
                        {   goto nextTest;  }
                        else
                        {
                            OTM1911A_GammaSetRegisterMapping(0xE100, tie + 1, GP_OTM1911A[tie + 1]++);
                            flag = 0x01;
                            goto ReTry;
                        }
                        
                    }
                    else if(Tie_Actual_Brightness[tie] < Tie_Estimate_Brightness[tie])
                    {//實測亮度 小於標準答案亮度 處置~>應該提高亮度
                        if (flag == 0x01)//表示剛剛為亮度反折點
                        {   goto nextTest;  }
                        else
                        {
                            OTM1911A_GammaSetRegisterMapping(0xE100, tie + 1, GP_OTM1911A[tie + 1]--);
                            flag = 0x10;
                            goto ReTry;
                        }
                    }
                    else//實測亮度 = 標準答案亮度 進行下一個綁點測試
                    {
                        OTM1911A_GammaSetRegisterMapping(0xE100, tie + 1, GP_OTM1911A[tie + 1]);
                        goto nextTest;
                    }
                }
                nextTest:

                flag = 0;
            }
        }

        
        private void OTM1911A_GammaSetRegisterMapping(int StartAddr, int TieNum, int GammaValueSet)
        {
            byte[] RegData = new byte[37];
            int StartAddress = (StartAddr & 0xFF00);
            byte[] receiver = new byte[37];
            byte addr_MSB = 0;


            SL_WhiskyComm_Util WhiskeyUtil = new SL_WhiskyComm_Util();

            OTM1911A_GammaReadRegister(Convert.ToUInt16(StartAddr));

            //STEP3: 針對想設定的值去設定
            GP_OTM1911A[TieNum] = Convert.ToUInt16(GammaValueSet);


            //STEP4: 把GP[1]~GP[29] 填到預備要寫入暫存器的空間(20170830驗證功能正確)
            int cnt = 1;
            int temp1 = 0, temp2 = 0;

            int MSB = 0;
            for (int i = 0; i < 7; i++)
            {
                MSB = 0;
                temp1 = GP_OTM1911A[cnt] & 0x00FF;
                temp2 = GP_OTM1911A[cnt] & 0x0F00;
                temp2 >>= 8;
                MSB = temp2 & 0x03;
                RegData[(4 + (i * 5))] = Convert.ToByte(MSB);
                RegData[(0 + (i * 5))] = Convert.ToByte(temp1);
                cnt++;

                temp1 = GP_OTM1911A[cnt] & 0x00FF;
                temp2 = GP_OTM1911A[cnt] & 0x0F00;
                temp2 >>= 8;
                MSB = MSB + ((temp2 & 0x03)<<2);
                RegData[(4 + (i * 5))] = Convert.ToByte(MSB);
                RegData[(1 + (i * 5))] = Convert.ToByte(temp1);
                cnt++;

                temp1 = GP_OTM1911A[cnt] & 0x00FF;
                temp2 = GP_OTM1911A[cnt] & 0x0F00;
                temp2 >>= 8;
                MSB = MSB + ((temp2 & 0x03) << 4);
                RegData[(4 + (i * 5))] = Convert.ToByte(MSB);
                RegData[(2 + (i * 5))] = Convert.ToByte(temp1);
                cnt++;

                temp1 = GP_OTM1911A[cnt] & 0x00FF;
                temp2 = GP_OTM1911A[cnt] & 0x0F00;
                temp2 >>= 8;
                MSB = MSB + ((temp2 & 0x03) << 6);
                RegData[(4 + (i * 5))] = Convert.ToByte(MSB);
                RegData[(3 + (i * 5))] = Convert.ToByte(temp1);
                cnt++;
            }

            temp1 = GP_OTM1911A[29] & 0x00FF;
            temp2 = GP_OTM1911A[29] & 0x0F00;
            temp2 >>= 8;
            MSB = temp2 & 0x03;
            RegData[36] = Convert.ToByte(MSB);
            RegData[35] = Convert.ToByte(temp1);

            //STEP5: 將預備要寫入暫存器的空間 填入IC中的暫存器
            for (byte reg = 0x00; reg < 0x25; reg++)
            {
                WhiskeyUtil.MipiWrite(0x23, 0x00, reg);
                WhiskeyUtil.MipiWrite(0x23, addr_MSB, RegData[reg]);
            }
        }

        private void OTM1911A_GammaReadRegister(UInt16 Start_address)
        {
            byte[] RegData = new byte[38];
            int StartAddress = (Start_address & 0xFF00);
            byte[] receiver = new byte[38];
            byte addr_MSB = 0;


            SL_WhiskyComm_Util WhiskeyUtil = new SL_WhiskyComm_Util();

            //STEP1: 從對應的暫存器中讀回暫存器目前設定值
            addr_MSB = Convert.ToByte((StartAddress >>= 8));


            WhiskeyUtil.MipiWrite(0x23, 0x00, 0x00);
            //Thread.Sleep(30);
            WhiskeyUtil.MipiRead(0xE1, 38, ref receiver);
            //WhiskeyUtil.MipiRead(0x0A, 1, ref receiver);
            //Thread.Sleep(30);
            for (int i = 0; i < 38; i++)
            {
                RegData[i] = receiver[i];
            }
            Thread.Sleep(1000);

            //STEP2: 針對暫存器內容 轉成綁點GP內容值

            int cnt = 1;
            int temp1 = 0, temp2 = 0;
            for (int i = 0; i < 7; i++)
            {
                temp1 = RegData[(4 + (i * 5))];
                temp1 = temp1 & 0x03;
                temp2 = RegData[(0 + (i * 5))];
                temp1 = (temp1 << 8) + temp2;
                GP_OTM1911A[cnt] = Convert.ToUInt16(temp1); cnt++;

                temp1 = RegData[(4 + (i * 5))];
                temp1 = (temp1 & 0x0C) >> 2;
                temp2 = RegData[(1 + (i * 5))];
                temp1 = (temp1 << 8) + temp2;
                GP_OTM1911A[cnt] = Convert.ToUInt16(temp1); cnt++;

                temp1 = RegData[(4 + (i * 5))];
                temp1 = (temp1 & 0x30) >> 4;
                temp2 = RegData[(2 + (i * 5))];
                temp1 = (temp1 << 8) + temp2;
                GP_OTM1911A[cnt] = Convert.ToUInt16(temp1); cnt++;

                temp1 = RegData[(4 + (i * 5))];
                temp1 = (temp1 & 0xC0) >> 6;
                temp2 = RegData[(3 + (i * 5))];
                temp1 = (temp1 << 8) + temp2;
                GP_OTM1911A[cnt] = Convert.ToUInt16(temp1); cnt++;
            }
            temp1 = RegData[36];
            temp1 = temp1 & 0x03;
            temp2 = RegData[35];
            temp1 = (temp1 << 8) + temp2;
            GP_OTM1911A[29] = Convert.ToUInt16(temp1);
            //GP[1]~GP[29] 即為每個綁點的值
        }





        private void deviceTimer_Tick(object sender, EventArgs e)
        {
            FindUsb();
        }

        private void UsbDeviceList()
        {
            SL_Device_Util deviceUtil = new SL_Device_Util();
            if (deviceUtil.GetUSBDevices() > 0)
            {
                List<SL_Device_Util.ScDeviceInfo> UsbDevice = deviceUtil.FindScDevice();
                UsbDeviceInfo = UsbDevice.ToArray();
                if (UsbDeviceInfo.Length > 1)
                    txtbox_info.Text = "Much Device,First Connected";
                else
                {
                    if (UsbDeviceInfo.Length == 1)
                    { 
                        deviceUtil.getDeviceItem(UsbDeviceInfo[0].Description);
                        txtbox_info.Text = UsbDeviceInfo[0].DeviceID;
                        txtbox_vid.Text ="0x" + deviceUtil.getStrVid();
                        txtbox_pid.Text = "0x" + deviceUtil.getStrPid();
                        SL_Comm_Base.Device_Open((ushort)deviceUtil.getShortVid(), (ushort)deviceUtil.getShortPid());
                    }
                }
            }

        }

        private void FindUsb()
        {
            SL_Device_Util deviceUtil = new SL_Device_Util();
            if (deviceUtil.GetUSBDevices() > 0)
            {
                List<SL_Device_Util.ScDeviceInfo> UsbDevice = deviceUtil.FindScDevice();
                this.UsbDeviceInfo = UsbDevice.ToArray();

                if (UsbDeviceInfo.Length > 1)
                    txtbox_info.Text = "Much Device,First Connected";
                else
                {
                    if(UsbDeviceInfo.Length ==1)
                    { 
                        deviceUtil.getDeviceItem(UsbDeviceInfo[0].Description);
                        txtbox_info.Text = UsbDeviceInfo[0].DeviceID;
                        txtbox_vid.Text = "0x" + deviceUtil.getStrVid();
                        txtbox_pid.Text = "0x" + deviceUtil.getStrPid();
                        SL_Comm_Base.Device_Open((ushort)deviceUtil.getShortVid(), (ushort)deviceUtil.getShortPid());
                    }
                }

            }
        }




        private void DSV_Setting()
        {
            // SPLC095A 

            SL_Comm_Base.SL_CommBase_WriteReg(0xa0, 0x20);

            SL_Comm_Base.SL_CommBase_WriteReg(0x9b, 0x02);
            SL_Comm_Base.SL_CommBase_WriteReg(0x80, 0x22);
            SL_Comm_Base.SL_CommBase_WriteReg(0x80, 0x0c);
            SL_Comm_Base.SL_CommBase_WriteReg(0x80, 0x48);
            SL_Comm_Base.SL_CommBase_WriteReg(0x9b, 0x01);

            SL_Comm_Base.SL_CommBase_WriteReg(0x9b, 0x02);
            SL_Comm_Base.SL_CommBase_WriteReg(0x80, 0x22);
            SL_Comm_Base.SL_CommBase_WriteReg(0x80, 0x09);
            SL_Comm_Base.SL_CommBase_WriteReg(0x80, 0x0a);
            SL_Comm_Base.SL_CommBase_WriteReg(0x9b, 0x01);

            SL_Comm_Base.SL_CommBase_WriteReg(0x9b, 0x02);
            SL_Comm_Base.SL_CommBase_WriteReg(0x80, 0x22);
            SL_Comm_Base.SL_CommBase_WriteReg(0x80, 0x0d);
            SL_Comm_Base.SL_CommBase_WriteReg(0x80, 0x67);
            SL_Comm_Base.SL_CommBase_WriteReg(0x9b, 0x01);

            SL_Comm_Base.SL_CommBase_WriteReg(0x9b, 0x02);
            SL_Comm_Base.SL_CommBase_WriteReg(0x80, 0x22);
            SL_Comm_Base.SL_CommBase_WriteReg(0x80, 0x0e);
            SL_Comm_Base.SL_CommBase_WriteReg(0x80, 0x55);
            SL_Comm_Base.SL_CommBase_WriteReg(0x9b, 0x01);

            SL_Comm_Base.SL_CommBase_WriteReg(0x9b, 0x02);
            SL_Comm_Base.SL_CommBase_WriteReg(0x80, 0x22);
            SL_Comm_Base.SL_CommBase_WriteReg(0x80, 0x0f);
            SL_Comm_Base.SL_CommBase_WriteReg(0x80, 0x55);
            SL_Comm_Base.SL_CommBase_WriteReg(0x9b, 0x01);

            SL_Comm_Base.SL_CommBase_WriteReg(0x9b, 0x02);
            SL_Comm_Base.SL_CommBase_WriteReg(0x80, 0x22);
            SL_Comm_Base.SL_CommBase_WriteReg(0x80, 0x02);
            SL_Comm_Base.SL_CommBase_WriteReg(0x80, 0x50);
            SL_Comm_Base.SL_CommBase_WriteReg(0x9b, 0x01);

            SL_Comm_Base.SL_CommBase_WriteReg(0x9b, 0x02);
            SL_Comm_Base.SL_CommBase_WriteReg(0x80, 0x22);
            SL_Comm_Base.SL_CommBase_WriteReg(0x80, 0x0a);
            SL_Comm_Base.SL_CommBase_WriteReg(0x80, 0x11);
            SL_Comm_Base.SL_CommBase_WriteReg(0x9b, 0x01);
        }

        private void CMD2_and_FOCAL_Enable()
        {
            SL_WhiskyComm_Util WhiskeyUtil = new SL_WhiskyComm_Util();
            byte[] RdVal = new byte[6];
            string rdstr = null;
            uint test = 0;

            WhiskeyUtil.MipiWrite(0x29, 0xFF, 0x19, 0x11, 0x01);

            WhiskeyUtil.MipiWrite(0x23, 0x00, 0x80);
            WhiskeyUtil.MipiWrite(0x23, 0xFF, 0x19);
            WhiskeyUtil.MipiWrite(0x23, 0x00, 0x81);
            WhiskeyUtil.MipiWrite(0x23, 0xFF, 0x11);




            WhiskeyUtil.MipiWrite(0x23, 0x00, 0x00);
            WhiskeyUtil.MipiWrite(0x23, 0xD0, 0x78);

            WhiskeyUtil.MipiWrite(0x00, 0x00);
            WhiskeyUtil.MipiRead(0xF8, 6, ref RdVal); // rdstr: ID1: 0x40h
            //Info_textBox.Text += rdstr + "\r\n"; rdstr = null;

            WhiskeyUtil.MipiRead(0x0A, 1, ref rdstr); // rdstr: ID1: 0x40h


            WhiskeyUtil.MipiWrite(0x00, 0x00);
            WhiskeyUtil.MipiRead(0xDA, 1, ref rdstr); // rdstr: ID1: 0x40h
            Info_textBox.Text +=  rdstr + "\r\n"; 




        }

        private void Vset_but_Click(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            SL_WhiskyComm_Util WhiskeyUtil = new SL_WhiskyComm_Util();
            string rdstr = null;

            Info_textBox.Text = "";

            WhiskeyUtil.MipiBridgeSelect(0x01); //Select 2828 Bank

            WhiskeyUtil.SetFpgaTiming(0x33, 0x11, 0x13, 0xff, 0x1E, 0x0f);

            WhiskeyUtil.SetMipiVideo(1920, 1080, 60, 16, 16, 30, 30, 4, 4);

            WhiskeyUtil.SetMipiDsi(4, 700, "burst");
            uint data = 0;
            SL_Comm_Base.SPI_ReadReg(0xbb, ref data, 2);

            DSV_Setting();

            WhiskeyUtil.GpioCtrl(0x11, 0xff, 0xff); //GPIO RESET
            WhiskeyUtil.GpioCtrl(0x11, 0x00, 0x00);
            Thread.Sleep(100);
            WhiskeyUtil.GpioCtrl(0x11, 0xff, 0xff);

            



            CMD2_and_FOCAL_Enable();


            WhiskeyUtil.MipiWrite(0x23, 0x00, 0x00);
            WhiskeyUtil.MipiRead(0xF0, 1, ref rdstr);



            WhiskeyUtil.MipiWrite(0x23, 0x00, 0x00);
            WhiskeyUtil.MipiWrite(0x13, 0xE1, 0x0A);


            WhiskeyUtil.MipiWrite(0x23, 0x00, 0x00);
            WhiskeyUtil.MipiRead(0xE1, 1, ref rdstr);






            WhiskeyUtil.MipiWrite(0x05, 0x11);
            Thread.Sleep(100);
            WhiskeyUtil.MipiWrite(0x05, 0x29);


            //WhiskeyUtil.MipiRead(0x0a, 1, ref rdstr); // rdstr: 0x9c

            //WhiskeyUtil.ImageFill(0, 255, 0);
            //Thread.Sleep(1000);
            //WhiskeyUtil.ImageFill(255, 0, 0);
            //Thread.Sleep(1000);
            //WhiskeyUtil.ImageFill(0, 0, 255);
            //Thread.Sleep(1000);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Info_textBox.Text = "";

            OTM1911A_GammaSetRegisterMapping(0xE100,0,0);

            //SL_WhiskyComm_Util WhiskeyUtil = new SL_WhiskyComm_Util();
            //string rdstr = null;

            ////Display OFF
            //WhiskeyUtil.MipiWrite(0x05, 0x28);
            //WhiskeyUtil.MipiWrite(0x05, 0x10);


            //WhiskeyUtil.MipiWrite(0x23, 0x00, 0x00);
            //WhiskeyUtil.MipiRead(0xE1, 1, ref rdstr);
            //Info_textBox.Text += rdstr + "\r\n"; rdstr = null;

            ////WhiskeyUtil.MipiWrite(0x13, 0x00, 0x01);
            ////WhiskeyUtil.MipiWrite(0x13, 0xE0, 0x01);

            //WhiskeyUtil.MipiWrite(0x23, 0x00, 0x00);
            //WhiskeyUtil.MipiRead(0xE2, 1, ref rdstr);
            //Info_textBox.Text += rdstr + "\r\n"; rdstr = null;


            ////Display On
            //WhiskeyUtil.MipiWrite(0x05, 0x11);
            //WhiskeyUtil.MipiWrite(0x05, 0x29);
            //WhiskeyUtil.ImageFill(0, 255, 0);
            //Thread.Sleep(1000);

            ////Display OFF

        }

        private void display_off()
        {
            SL_WhiskyComm_Util WhiskeyUtil = new SL_WhiskyComm_Util();
            WhiskeyUtil.MipiWrite(0x05, 0x28); Thread.Sleep(100);
        }

        private void display_on()
        {
            SL_WhiskyComm_Util WhiskeyUtil = new SL_WhiskyComm_Util();
            WhiskeyUtil.MipiWrite(0x05, 0x29); Thread.Sleep(100);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            int dive = comboBox1.SelectedIndex + 1;

            SL_WhiskyComm_Util WhiskeyUtil = new SL_WhiskyComm_Util();
            byte tie_gray= 0x00;


            //依序點綁點處的灰階 並且用K80量測
            for (int gary = 0; gary < 256; gary++) //tir=0 時亮度最亮
            {
                tie_gray = Convert.ToByte(255 - gary);

                WhiskeyUtil.ImageFill(tie_gray, tie_gray, tie_gray);
                Thread.Sleep(1000);

                Actual_Brightness[gary] = Math.Round(K80_Trigger_Measurement(dive), 4);//取到小數點第4位

                chart1.Series[3].Points.AddXY(gary, Actual_Brightness[gary]);
            }

        }

        private void button7_Click(object sender, EventArgs e)
        {
            SL_WhiskyComm_Util WhiskeyUtil = new SL_WhiskyComm_Util();
            WhiskeyUtil.MipiBridgeSelect(0x01); //Select 2828 Bank
            WhiskeyUtil.ImageShow("VG.bmp");
        }

    }
}
