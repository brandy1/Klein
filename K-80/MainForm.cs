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
        private double[] EstimateBrightness_toleranceP = new double[256];//推算符合Gamma曲線設定的亮度表現
        private double[] EstimateBrightness_toleranceN = new double[256];//推算符合Gamma曲線設定的亮度表現
        private double[] Estimate_Brightness_Slope = new double[29];//推算的亮度 計算的斜率
        private double[] Tie_Actual_Brightness = new double[29];//實測的亮度 計算的斜率

        private double[] Actual_Brightness_Slope = new double[29];//實測的亮度 計算的斜率
        private int[] Tie_Index_CalGet = new int[29];//推算的亮度 去找出的綁點的設定值
        private int[] Tie_ParameterSetting_MSB = new int[29];
        private int[] Tie_ParameterSetting_LSB = new int[29];


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

            /*推算標準Gamma亮度前 先用K80量測面板表現最暗與最亮灰階的亮度值*/

            //■■■■■■■■■■■■
            //  LCD點最暗灰階
            //  LCM Control Code Here !!
            //■■■■■■■■■■■■

            EstimateBrightness_Min = Math.Round(K80_Trigger_Measurement(dive), 4);//取到小數點第4位
            YMin_label.Text = "YMin=" + Convert.ToString(EstimateBrightness_Min);


            //■■■■■■■■■■■■
            //  LCD點最亮灰階
            //  LCM Control Code Here !!
            //■■■■■■■■■■■■

            EstimateBrightness_Max = Math.Round(K80_Trigger_Measurement(dive), 4);//取到小數點第4位
            YMax_label.Text = "YMax=" + Convert.ToString(EstimateBrightness_Max);

            /*套用設定的Gamma值 推算出符合標準Gamma曲線答案的亮度表現*/

            double Gamma_set;
            double Gamma_set_tolerance;
            double temp, temp2;

            Gamma_set = Convert.ToDouble(GammaSet_textBox.Text);

            EstimateBrightness_Max = 50;
            EstimateBrightness_Min = 0.5;

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

            for (int tie = 0; tie < 28; tie++)
            {
                EstimateBrightnessTie_struct[tie].Start_Tie_Index = VP_index[tie];
                EstimateBrightnessTie_struct[tie].End_Tie_Index = VP_index[(tie + 1)];
                EstimateBrightnessTie_struct[tie].Start_Tie_Brightness = EstimateBrightness[VP_index[tie]];
                EstimateBrightnessTie_struct[tie].End_Tie_Brightness = EstimateBrightness[VP_index[(tie + 1)]];
            }


            for (int num = 0; num < 28; num++)
            {
                //num=0 表示tie0~tie1的斜率
                //num=1 表示tie1~tie2的斜率...以此類推
                temp = EstimateBrightnessTie_struct[num].End_Tie_Brightness - EstimateBrightnessTie_struct[num].Start_Tie_Brightness;
                temp2 = EstimateBrightnessTie_struct[num].End_Tie_Index - EstimateBrightnessTie_struct[num].Start_Tie_Index;
                Estimate_Brightness_Slope[num] = Math.Round(temp / temp2,4);
            }
            //斜率取得完畢


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
            temp_MSB = Tie_ParameterSetting_MSB[cnt];
            temp_LSB = Tie_ParameterSetting_LSB[cnt];
            temp_MSB <<= 8;
            textBox_VP0.Text = Convert.ToString((temp_MSB + temp_LSB));
            cnt++;

            //VP1
            temp_MSB = Tie_ParameterSetting_MSB[cnt];
            temp_LSB = Tie_ParameterSetting_LSB[cnt];
            temp_MSB <<= 8;
            textBox_VP1.Text = Convert.ToString((temp_MSB + temp_LSB));
            cnt++;

            //VP3
            temp_MSB = Tie_ParameterSetting_MSB[cnt];
            temp_LSB = Tie_ParameterSetting_LSB[cnt];
            temp_MSB <<= 8;
            textBox_VP3.Text = Convert.ToString((temp_MSB + temp_LSB));
            cnt++;

            //VP5
            temp_MSB = Tie_ParameterSetting_MSB[cnt];
            temp_LSB = Tie_ParameterSetting_LSB[cnt];
            temp_MSB <<= 8;
            textBox_VP5.Text = Convert.ToString((temp_MSB + temp_LSB));
            cnt++;

            //VP7
            temp_MSB = Tie_ParameterSetting_MSB[cnt];
            temp_LSB = Tie_ParameterSetting_LSB[cnt];
            temp_MSB <<= 8;
            textBox_VP7.Text = Convert.ToString((temp_MSB + temp_LSB));
            cnt++;

            //VP9
            temp_MSB = Tie_ParameterSetting_MSB[cnt];
            temp_LSB = Tie_ParameterSetting_LSB[cnt];
            temp_MSB <<= 8;
            textBox_VP9.Text = Convert.ToString((temp_MSB + temp_LSB));
            cnt++;

            //VP11
            temp_MSB = Tie_ParameterSetting_MSB[cnt];
            temp_LSB = Tie_ParameterSetting_LSB[cnt];
            temp_MSB <<= 8;
            textBox_VP11.Text = Convert.ToString((temp_MSB + temp_LSB));
            cnt++;

            //VP13
            temp_MSB = Tie_ParameterSetting_MSB[cnt];
            temp_LSB = Tie_ParameterSetting_LSB[cnt];
            temp_MSB <<= 8;
            textBox_VP13.Text = Convert.ToString((temp_MSB + temp_LSB));
            cnt++;

            //VP15
            temp_MSB = Tie_ParameterSetting_MSB[cnt];
            temp_LSB = Tie_ParameterSetting_LSB[cnt];
            temp_MSB <<= 8;
            textBox_VP15.Text = Convert.ToString((temp_MSB + temp_LSB));
            cnt++;

            //VP24
            temp_MSB = Tie_ParameterSetting_MSB[cnt];
            temp_LSB = Tie_ParameterSetting_LSB[cnt];
            temp_MSB <<= 8;
            textBox_VP24.Text = Convert.ToString((temp_MSB + temp_LSB));
            cnt++;

            //VP32
            temp_MSB = Tie_ParameterSetting_MSB[cnt];
            temp_LSB = Tie_ParameterSetting_LSB[cnt];
            temp_MSB <<= 8;
            textBox_VP32.Text = Convert.ToString((temp_MSB + temp_LSB));
            cnt++;

            //VP48
            temp_MSB = Tie_ParameterSetting_MSB[cnt];
            temp_LSB = Tie_ParameterSetting_LSB[cnt];
            temp_MSB <<= 8;
            textBox_VP48.Text = Convert.ToString((temp_MSB + temp_LSB));
            cnt++;

            //VP64
            temp_MSB = Tie_ParameterSetting_MSB[cnt];
            temp_LSB = Tie_ParameterSetting_LSB[cnt];
            temp_MSB <<= 8;
            textBox_VP64.Text = Convert.ToString((temp_MSB + temp_LSB));
            cnt++;

            //VP96
            temp_MSB = Tie_ParameterSetting_MSB[cnt];
            temp_LSB = Tie_ParameterSetting_LSB[cnt];
            temp_MSB <<= 8;
            textBox_VP96.Text = Convert.ToString((temp_MSB + temp_LSB));
            cnt++;

            //VP128
            temp_MSB = Tie_ParameterSetting_MSB[cnt];
            temp_LSB = Tie_ParameterSetting_LSB[cnt];
            temp_MSB <<= 8;
            textBox_VP128.Text = Convert.ToString((temp_MSB + temp_LSB));
            cnt++;

            //VP160
            temp_MSB = Tie_ParameterSetting_MSB[cnt];
            temp_LSB = Tie_ParameterSetting_LSB[cnt];
            temp_MSB <<= 8;
            textBox_VP160.Text = Convert.ToString((temp_MSB + temp_LSB));
            cnt++;

            //VP192
            temp_MSB = Tie_ParameterSetting_MSB[cnt];
            temp_LSB = Tie_ParameterSetting_LSB[cnt];
            temp_MSB <<= 8;
            textBox_VP192.Text = Convert.ToString((temp_MSB + temp_LSB));
            cnt++;

            //VP208
            temp_MSB = Tie_ParameterSetting_MSB[cnt];
            temp_LSB = Tie_ParameterSetting_LSB[cnt];
            temp_MSB <<= 8;
            textBox_VP208.Text = Convert.ToString((temp_MSB + temp_LSB));
            cnt++;

            //VP224
            temp_MSB = Tie_ParameterSetting_MSB[cnt];
            temp_LSB = Tie_ParameterSetting_LSB[cnt];
            temp_MSB <<= 8;
            textBox_VP224.Text = Convert.ToString((temp_MSB + temp_LSB));
            cnt++;

            //VP232
            temp_MSB = Tie_ParameterSetting_MSB[cnt];
            temp_LSB = Tie_ParameterSetting_LSB[cnt];
            temp_MSB <<= 8;
            textBox_VP232.Text = Convert.ToString((temp_MSB + temp_LSB));
            cnt++;

            //VP240
            temp_MSB = Tie_ParameterSetting_MSB[cnt];
            temp_LSB = Tie_ParameterSetting_LSB[cnt];
            temp_MSB <<= 8;
            textBox_VP240.Text = Convert.ToString((temp_MSB + temp_LSB));
            cnt++;

            //VP242
            temp_MSB = Tie_ParameterSetting_MSB[cnt];
            temp_LSB = Tie_ParameterSetting_LSB[cnt];
            temp_MSB <<= 8;
            textBox_VP242.Text = Convert.ToString((temp_MSB + temp_LSB));
            cnt++;

            //VP244
            temp_MSB = Tie_ParameterSetting_MSB[cnt];
            temp_LSB = Tie_ParameterSetting_LSB[cnt];
            temp_MSB <<= 8;
            textBox_VP244.Text = Convert.ToString((temp_MSB + temp_LSB));
            cnt++;

            //VP246
            temp_MSB = Tie_ParameterSetting_MSB[cnt];
            temp_LSB = Tie_ParameterSetting_LSB[cnt];
            temp_MSB <<= 8;
            textBox_VP246.Text = Convert.ToString((temp_MSB + temp_LSB));
            cnt++;

            //VP248
            temp_MSB = Tie_ParameterSetting_MSB[cnt];
            temp_LSB = Tie_ParameterSetting_LSB[cnt];
            temp_MSB <<= 8;
            textBox_VP248.Text = Convert.ToString((temp_MSB + temp_LSB));
            cnt++;

            //VP250
            temp_MSB = Tie_ParameterSetting_MSB[cnt];
            temp_LSB = Tie_ParameterSetting_LSB[cnt];
            temp_MSB <<= 8;
            textBox_VP250.Text = Convert.ToString((temp_MSB + temp_LSB));
            cnt++;

            //VP252
            temp_MSB = Tie_ParameterSetting_MSB[cnt];
            temp_LSB = Tie_ParameterSetting_LSB[cnt];
            temp_MSB <<= 8;
            textBox_VP252.Text = Convert.ToString((temp_MSB + temp_LSB));
            cnt++;

            //VP254
            temp_MSB = Tie_ParameterSetting_MSB[cnt];
            temp_LSB = Tie_ParameterSetting_LSB[cnt];
            temp_MSB <<= 8;
            textBox_VP254.Text = Convert.ToString((temp_MSB + temp_LSB));
            cnt++;

            //VP255
            temp_MSB = Tie_ParameterSetting_MSB[cnt];
            temp_LSB = Tie_ParameterSetting_LSB[cnt];
            temp_MSB <<= 8;
            textBox_VP255.Text = Convert.ToString((temp_MSB + temp_LSB));
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
            Info_textBox.Text = "";
            //Read Gamma Parameter Setting from Gamma Register to Tie_ParameterSettingt[0~28]
            GammaRegister_to_WTie_ParameterSetting();

            //從Tie_ParameterSetting的內容顯示於Form上的Text
            Tie_ParameterSetting_to_LoadVP_TextData();

            Info_textBox.Text = "從IC讀出至控制盤 Done!";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int Gray_scale = 0;
            int dive = comboBox1.SelectedIndex + 1;
            double temp = 0;
            double diff_min = EstimateBrightness_Max;
            double diff_min_last = EstimateBrightness_Max;
            int flag_tunelse = 0;
            int GammaSetting_last;

            Tie_Actual_Brightness[0] = EstimateBrightness_Max;

            for (int tie=1; tie<29; tie++)
            {
                diff_min = EstimateBrightness_Max;
                //(1)針對重點綁點進行量測亮度
                Gray_scale = VP_index[tie];

                //面板點相對應的灰階 灰階=Gray_scale


                //K80量測灰階亮度
                Tie_Actual_Brightness[tie] = Math.Round(K80_Trigger_Measurement(dive), 4);//取到小數點第4位


                //計算本次綁點(tie)亮度與上一個綁點亮度的差異 並且計算出斜率
                //tie=0 表示tie0~tie1的斜率
                //tie=1 表示tie1~tie2的斜率...以此類推
                temp = Tie_Actual_Brightness[tie] - Tie_Actual_Brightness[(tie - 1)];
                temp = temp / (VP_index[tie] - VP_index[tie - 1]);
                Actual_Brightness_Slope[(tie - 1)] = Math.Round(temp, 4);

                //Estimate_Brightness_Slope[] VS Actual_Brightness_Slope[]

                //比較推算出的標準斜率(Estimate_Brightness_Slope[])與實測斜率(Actual_Brightness_Slope[])
                //兩者的差異 作為調適的方向
                diff_min = Math.Abs(Actual_Brightness_Slope[tie - 1] - Estimate_Brightness_Slope[tie - 1]);


                if(Actual_Brightness_Slope[tie - 1] > Estimate_Brightness_Slope[tie - 1])
                {
                    //實測斜率 在標準斜率的右邊 也可能使用者暫存器根本設定錯誤
                    //正確解法=把暫存器值調大 讓對應灰階亮度降下來 斜率往左偏 斜率偏移方向下降
                    flag_tunelse = 1;

                    //紀錄目前的Gamma暫存器設定於GammaSetting_last

                }
                else
                {
                    //實測斜率 在標準斜率的左邊 
                    //正確解法=把暫存器值調小 讓對應灰階亮度提高 斜率往右偏 斜率偏移方向上升 
                    flag_tunelse = 2;

                    //紀錄目前的Gamma暫存器設定於GammaSetting_last
                }






            }


            /*       private int[] VP_index = new int[29]  { 0, 1, 3, 5, 7, 9, 11, 13, 15,
            24, 32, 48, 64, 96, 128, 160, 192, 208, 224, 232,
                    240, 242, 244, 246, 248, 250, 252, 254, 255};*/
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
                    deviceUtil.getDeviceItem(UsbDeviceInfo[0].Description);
                    txtbox_info.Text = UsbDeviceInfo[0].DeviceID;
                    txtbox_vid.Text ="0x" + deviceUtil.getStrVid();
                    txtbox_pid.Text = "0x" + deviceUtil.getStrPid();
                    SL_Comm_Base.Device_Open((ushort)deviceUtil.getShortVid(), (ushort)deviceUtil.getShortPid());
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
                    deviceUtil.getDeviceItem(UsbDeviceInfo[0].Description);
                    txtbox_info.Text = UsbDeviceInfo[0].DeviceID;
                    txtbox_vid.Text = "0x" + deviceUtil.getStrVid();
                    txtbox_pid.Text = "0x" + deviceUtil.getStrPid();
                    SL_Comm_Base.Device_Open((ushort)deviceUtil.getShortVid(), (ushort)deviceUtil.getShortPid());
                }

            }
        }

        private void btn_BringUp_Click(object sender, EventArgs e)
        {
            SL_WhiskyComm_Util WhiskeyUtil = new SL_WhiskyComm_Util();
            string rdstr = null;
            WhiskeyUtil.MipiBridgeSelect(0x01); //Select 2828 Bank
            WhiskeyUtil.SetFpgaTiming(0x33, 0x11, 0x13, 0xff, 0x1E, 0x0f);
            WhiskeyUtil.SetMipiVideo(1920, 1080, 60, 16, 16, 30, 30, 4, 4);
            WhiskeyUtil.SetMipiDsi(4, 700, "syncpulse");
            WhiskeyUtil.i2cWrite(0x01, 0x7c, 0x00, 0x0e);//Poser IC Ctrl
            WhiskeyUtil.i2cWrite(0x01, 0x7c, 0x01, 0x0e);
            WhiskeyUtil.i2cWrite(0x07, 0x7c, 0x00, 0x0e);
            WhiskeyUtil.i2cWrite(0x07, 0x7c, 0xFF, 0x00);

            WhiskeyUtil.GpioCtrl(0x11, 0xff, 0xff); //GPIO RESET
            WhiskeyUtil.GpioCtrl(0x11, 0x00, 0x00);
            Thread.Sleep(10);
            WhiskeyUtil.GpioCtrl(0x11, 0xff, 0xff);

            WhiskeyUtil.MipiWrite(0x05, 0x11);
            Thread.Sleep(100);
            WhiskeyUtil.MipiWrite(0x05, 0x29);

            WhiskeyUtil.MipiRead(0x0a, 1, ref rdstr); // rdstr: 0x9c

            WhiskeyUtil.ImageFill(0, 255, 0);
            Thread.Sleep(1000);
            WhiskeyUtil.ImageFill(255, 0, 0);
            Thread.Sleep(1000);
            WhiskeyUtil.ImageFill(0, 0, 255);
            Thread.Sleep(1000);

        }

        private void btn_imgfill_Click(object sender, EventArgs e)
        {
            SL_WhiskyComm_Util WhiskeyUtil = new SL_WhiskyComm_Util();
            string rdstr = null;
            WhiskeyUtil.MipiBridgeSelect(0x01); //Select 2828 Bank
            WhiskeyUtil.ImageFill(255, 0, 255);
        }

        private void btn_imgShow_Click(object sender, EventArgs e)
        {
            SL_WhiskyComm_Util WhiskeyUtil = new SL_WhiskyComm_Util();
            WhiskeyUtil.MipiBridgeSelect(0x01); //Select 2828 Bank
            WhiskeyUtil.ImageShow("11.bmp");
        }
    }
}
