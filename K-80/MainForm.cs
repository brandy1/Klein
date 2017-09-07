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
        private uint[] Tie_Index_CalGet = new uint[29];//推算的亮度 去找出的綁點的設定值
        private uint[] Tie_ParameterSetting_MSB = new uint[29];
        private uint[] Tie_ParameterSetting_LSB = new uint[29];

        private uint[] TieRegisterSetting = new uint[29]; //目前讀出的的綁點設定值使用 或是用於讀取控制盤上的設定值 準備寫給IC用

        private uint[] GP_OTM1911A = new uint[30]; //GP1~GP29 OTM1911 Gamma1綁點設定值存放處


        public BrightnessTie_struct[] EstimateBrightnessTie_struct = new BrightnessTie_struct[29];
        public BrightnessTie_struct[] ActualBrightnessTie_struct = new BrightnessTie_struct[29];


        public class BrightnessTie_struct
        {
            public int Start_Tie_Index;
            public int End_Tie_Index;
            public double Start_Tie_Brightness;
            public double End_Tie_Brightness;
        }


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
                GetEstimateBrightness_button.Enabled = true;
                button1.Enabled = true;
                button6.Enabled = true;
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
            this.GetEstimateBrightness_button.ForeColor = Color.Green;
            Application.DoEvents();
            int dive = comboBox1.SelectedIndex + 1;
            SL_WhiskyComm_Util WhiskeyUtil = new SL_WhiskyComm_Util();



            /*推算標準Gamma亮度前 先用K80量測面板表現最暗與最亮灰階的亮度值*/

            WhiskeyUtil.ImageFill(0, 0, 0);
            Thread.Sleep(1000);

            EstimateBrightness_Min = Math.Round(K80_Trigger_Measurement(dive), 4);//取到小數點第4位
            YMax_label.Text = "YMin=" + Convert.ToString(EstimateBrightness_Min);

            WhiskeyUtil.ImageFill(255, 255, 255);
            Thread.Sleep(1000);

            EstimateBrightness_Max = Math.Round(K80_Trigger_Measurement(dive), 4);//取到小數點第4位
            YMin_label.Text = "YMax=" + Convert.ToString(EstimateBrightness_Max) ;

            /*套用設定的Gamma值 推算出符合標準Gamma曲線答案的亮度表現*/

            double Gamma_set;
            double Gamma_set_tolerance;
            double temp;

            double.TryParse(GammaSet_textBox.Text, out Gamma_set);

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
            double.TryParse(Gamma_set_tolerance_textBox.Text, out Gamma_set_tolerance);
            Gamma_set_tolerance = Gamma_set + Gamma_set_tolerance;
            //Gamma_set_tolerance = Gamma_set + Convert.ToDouble(Gamma_set_tolerance_textBox.Text);
            for (int Brightness = 0; Brightness < 256; Brightness++)
            {
                temp = (double)(256 - Brightness) / 256;

                EstimateBrightness_toleranceP[Brightness] = Math.Round(EstimateBrightness_Max * (float)Math.Pow(temp, Gamma_set_tolerance), 4);

                chart1.Series[1].Points.AddXY(Brightness, EstimateBrightness_toleranceP[Brightness]);
            }

            //計算誤差下界 並繪出圖
            double.TryParse(Gamma_set_tolerance_textBox.Text, out Gamma_set_tolerance);
            Gamma_set_tolerance = Gamma_set - Gamma_set_tolerance;
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

            //兩端Gamma電壓表現皆給予設定極限值 無須經過計算
            TieRegisterSetting[0] = 0;
            TieRegisterSetting[28] = 1023;

            for (uint tie_cnt = 1; tie_cnt < 28; tie_cnt++)
            {
                min_Y = EstimateBrightness_Max;

                for (uint num = 0; num < 1024; num++)
                {
                    temp_a = Math.Abs(EstimateBrightness[VP_index[tie_cnt]] - VRA_mapping_Brightness[num]);

                    if (temp_a <= min_Y)
                    {
                        min_Y = temp_a;
                        TieRegisterSetting[tie_cnt] = num;
                    }
                }
            }

            Tie_ParameterSetting_to_LoadVP_TextData(TieRegisterSetting);


            this.GetEstimateBrightness_button.ForeColor = Color.Black;
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
        private void Tie_ParameterSetting_to_LoadVP_TextData(uint[] registersetting)
        {
            string textdata = null;
            int cnt = 0;

            textdata = "Gamma Register Setting\r\n";
            Info_textBox.AppendText(textdata);

            //VP0
            textdata = "VP__0 Setting=" + Convert.ToString(registersetting[cnt]) + "\r\n";
            Info_textBox.AppendText(textdata);
            cnt++;

            //VP1
            textdata = "VP__1 Setting=" + Convert.ToString(registersetting[cnt]) + "\r\n";
            Info_textBox.AppendText(textdata);
            cnt++;

            //VP3
            textdata = "VP__3 Setting=" + Convert.ToString(registersetting[cnt]) + "\r\n";
            Info_textBox.AppendText(textdata);
            cnt++;

            //VP5
            textdata = "VP__5 Setting=" + Convert.ToString(registersetting[cnt]) + "\r\n";
            Info_textBox.AppendText(textdata);
            cnt++;

            //VP7
            textdata = "VP__7 Setting=" + Convert.ToString(registersetting[cnt]) + "\r\n";
            Info_textBox.AppendText(textdata);
            cnt++;

            //VP9
            textdata = "VP__9 Setting=" + Convert.ToString(registersetting[cnt]) + "\r\n";
            Info_textBox.AppendText(textdata);
            cnt++;

            //VP11
            textdata = "VP_11 Setting=" + Convert.ToString(registersetting[cnt]) + "\r\n";
            Info_textBox.AppendText(textdata);
            cnt++;

            //VP13
            textdata = "VP_13 Setting=" + Convert.ToString(registersetting[cnt]) + "\r\n";
            Info_textBox.AppendText(textdata);
            cnt++;

            //VP15
            textdata = "VP_15 Setting=" + Convert.ToString(registersetting[cnt]) + "\r\n";
            Info_textBox.AppendText(textdata);
            cnt++;

            //VP24
            textdata = "VP_24 Setting=" + Convert.ToString(registersetting[cnt]) + "\r\n";
            Info_textBox.AppendText(textdata);
            cnt++;

            //VP32
            textdata = "VP_32 Setting=" + Convert.ToString(registersetting[cnt]) + "\r\n";
            Info_textBox.AppendText(textdata);
            cnt++;

            //VP48
            textdata = "VP_48 Setting=" + Convert.ToString(registersetting[cnt]) + "\r\n";
            Info_textBox.AppendText(textdata);
            cnt++;

            //VP64
            textdata = "VP_64 Setting=" + Convert.ToString(registersetting[cnt]) + "\r\n";
            Info_textBox.AppendText(textdata);
            cnt++;

            //VP96
            textdata = "VP_96 Setting=" + Convert.ToString(registersetting[cnt]) + "\r\n";
            Info_textBox.AppendText(textdata);
            cnt++;

            //VP128
            textdata = "VP128 Setting=" + Convert.ToString(registersetting[cnt]) + "\r\n";
            Info_textBox.AppendText(textdata);
            cnt++;

            //VP160
            textdata = "VP160 Setting=" + Convert.ToString(registersetting[cnt]) + "\r\n";
            Info_textBox.AppendText(textdata);
            cnt++;

            //VP192
            textdata = "VP192 Setting=" + Convert.ToString(registersetting[cnt]) + "\r\n";
            Info_textBox.AppendText(textdata);
            cnt++;

            //VP208
            textdata = "VP208 Setting=" + Convert.ToString(registersetting[cnt]) + "\r\n";
            Info_textBox.AppendText(textdata);
            cnt++;

            //VP224
            textdata = "VP224 Setting=" + Convert.ToString(registersetting[cnt]) + "\r\n";
            Info_textBox.AppendText(textdata);
            cnt++;

            //VP232
            textdata = "VP232 Setting=" + Convert.ToString(registersetting[cnt]) + "\r\n";
            Info_textBox.AppendText(textdata);
            cnt++;

            //VP240
            textdata = "VP240 Setting=" + Convert.ToString(registersetting[cnt]) + "\r\n";
            Info_textBox.AppendText(textdata);
            cnt++;

            //VP242
            textdata = "VP242 Setting=" + Convert.ToString(registersetting[cnt]) + "\r\n";
            Info_textBox.AppendText(textdata);
            cnt++;

            //VP244
            textdata = "VP244 Setting=" + Convert.ToString(registersetting[cnt]) + "\r\n";
            Info_textBox.AppendText(textdata);
            cnt++;

            //VP246
            textdata = "VP246 Setting=" + Convert.ToString(registersetting[cnt]) + "\r\n";
            Info_textBox.AppendText(textdata);
            cnt++;

            //VP248
            textdata = "VP248 Setting=" + Convert.ToString(registersetting[cnt]) + "\r\n";
            Info_textBox.AppendText(textdata);
            cnt++;

            //VP250
            textdata = "VP250 Setting=" + Convert.ToString(registersetting[cnt]) + "\r\n";
            Info_textBox.AppendText(textdata);
            cnt++;

            //VP252
            textdata = "VP252 Setting=" + Convert.ToString(registersetting[cnt]) + "\r\n";
            Info_textBox.AppendText(textdata);
            cnt++;

            //VP254
            textdata = "VP254 Setting=" + Convert.ToString(registersetting[cnt]) + "\r\n";
            Info_textBox.AppendText(textdata);
            cnt++;

            //VP255
            textdata = "VP255 Setting=" + Convert.ToString(registersetting[cnt]) + "\r\n";
            Info_textBox.AppendText(textdata);
        }


        //從Form上的Text擷取Data去Tie_ParameterSetting
        private bool LoadVP_TextData_to_Tie_ParameterSetting(uint[] registersetting)
        {

            string infotxt = null;
            string Title = null, Value = null;
            int test = 0;

            infotxt = Info_textBox.Lines[0];

            test = Info_textBox.Text.Length;
           
            if(infotxt.CompareTo("Gamma Register Setting") != 0)
            {
                return false;
            }
            else
            {
                for (int i = 1; i < (Info_textBox.Lines.Length-1); i++)
                {
                    infotxt = Info_textBox.Lines[i];
                    string[] innerTxt = infotxt.Split('=');
                    Title = innerTxt[0].Substring(4, 1);
                    Value = innerTxt[1];

                    uint.TryParse(Value, out registersetting[(i - 1)]);


                }
                return true;
            }
        }

        //寫
        private void WriteGammaSettingAlltheSame_to_SSD2130( uint[] gammasetting)
        {
            byte TieCnt = 0;
            byte page = 0x00;
            uint temp = 0;
            byte RegisterSetting = 0x00;
            SL_WhiskyComm_Util WhiskeyUtil = new SL_WhiskyComm_Util();

            //切換SSD2130 PassWord & page
            //Page31 R+   Page32 R-   Page33 G+   Page34 G-   Page35 B+   Page36 B-
            for (page = 31; page<=36; page++)
            {
                WhiskeyUtil.MipiWrite(0x29, 0xFF, 0x21, 0x30, page);


                for(TieCnt=0; TieCnt < 29; TieCnt++)
                {
                    uint temp2 = TieCnt;
                    byte addr = 0;

                    temp2 = temp2 * 2;
                    addr = Convert.ToByte(temp2);

                    temp = gammasetting[TieCnt];
                    temp >>= 8;
                    temp = temp & 0x03;
                    RegisterSetting = Convert.ToByte(temp);
                    WhiskeyUtil.MipiWrite(0x23, addr, RegisterSetting);


                    temp2 = TieCnt;
                    temp2 = (temp2 * 2)+1;
                    addr = Convert.ToByte(temp2);

                    temp = gammasetting[TieCnt];
                    temp = temp & 0xFF;
                    RegisterSetting = Convert.ToByte(temp);
                    WhiskeyUtil.MipiWrite(0x23, addr, RegisterSetting);
                }
            }
            WhiskeyUtil.MipiWrite(0x29, 0xFF, 0x21, 0x30, 0x00);
        }

        //Read Gamma Parameter Setting from Gamma Register to Tie_ParameterSettingt[0~28]
        private void ReadGammaSettingAll_from_SSD2130(uint[] gammasetting)
        {
            byte TieCnt = 0;

            SL_WhiskyComm_Util WhiskeyUtil = new SL_WhiskyComm_Util();

            WhiskeyUtil.MipiWrite(0x29, 0xFF, 0x21, 0x30, 0x1F); //Page31 R+ 僅讀回一筆及代表所有Gamma

            for (TieCnt = 0; TieCnt < 29; TieCnt++)
            {
                uint temp2 = TieCnt;
                byte addr = 0;
                uint RegisterRead = 0x00;
                byte[] RdVal = new byte[1];

                temp2 = temp2 * 2;
                addr = Convert.ToByte(temp2);
                WhiskeyUtil.MipiRead(addr, 1, ref RdVal); 
                RegisterRead = RdVal[0];
                RegisterRead <<= 8;

                temp2 = TieCnt;

                temp2 = (temp2 * 2) + 1;
                WhiskeyUtil.MipiRead(addr, 1, ref RdVal); 
                RegisterRead = RegisterRead + Convert.ToUInt16(RdVal[0]);
                gammasetting[TieCnt] = RegisterRead;
            }
            WhiskeyUtil.MipiWrite(0x29, 0xFF, 0x21, 0x30, 0x00);
        }



        private void button2_Click(object sender, EventArgs e)
        {
            bool status  = false;
            this.WrText2GammaRegister_but.ForeColor = Color.Green;
            Application.DoEvents();

            //從Form上的Text擷取Data去Tie_ParameterSetting
            status = LoadVP_TextData_to_Tie_ParameterSetting(TieRegisterSetting);
            if(status == false)//判斷是否為Gamma寫入允許的格式
            {
                MessageBox.Show("目前textbox內容並非用於設定Gamma使用! 將重新載入Gamma目前設定值(從變數TieRegisterSetting中取得)");
                Info_textBox.Text = "";
                Tie_ParameterSetting_to_LoadVP_TextData(TieRegisterSetting);
            }
            else
            {
                //Load Gamma Parameter Setting from Tie_ParameterSettingt[0~28] to Gamma Register 
                WriteGammaSettingAlltheSame_to_SSD2130(TieRegisterSetting);
                Info_textBox.AppendText("從控制盤載入IC暫存器完畢!");
            }

            this.WrText2GammaRegister_but.ForeColor = Color.Black;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            this.RdRegisteer2Text_but.ForeColor = Color.Green;
            Application.DoEvents();
            SL_WhiskyComm_Util WhiskeyUtil = new SL_WhiskyComm_Util();

            Info_textBox.Text = "";

            //Read Gamma Parameter Setting from Gamma Register to Tie_ParameterSettingt[0~28]
            ReadGammaSettingAll_from_SSD2130(TieRegisterSetting);


            //從Tie_ParameterSetting的內容顯示於Form上的Text
            Tie_ParameterSetting_to_LoadVP_TextData(TieRegisterSetting);

            this.RdRegisteer2Text_but.ForeColor = Color.Black;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.button1.ForeColor = Color.Green;
            Application.DoEvents();

            int dive = comboBox1.SelectedIndex + 1;
            double diff_min = EstimateBrightness_Max;
            double diff_min_last = EstimateBrightness_Max;
            byte tie_gray = 0;
            byte[] track_flag = new byte[2];
            uint[] Brighter_GammaRegister = new uint[29];
            uint[] Darker_GammaRegister = new uint[29];
            uint[] temp = new uint[29];


            SL_WhiskyComm_Util WhiskeyUtil = new SL_WhiskyComm_Util();


            //★步驟1:讀回目前Gamma設定值 存放於TieRegisterSetting[]變數之中
            ReadGammaSettingAll_from_SSD2130(TieRegisterSetting);
            Tie_ParameterSetting_to_LoadVP_TextData(TieRegisterSetting);
            for (uint tie = 0; tie < 29; tie++)
            {   temp[tie] = TieRegisterSetting[tie];    }


            //★步驟2:實際點根據綁點所在 套用設定值去點面板 
            //透過K80量測出實測亮度 之後存放於Tie_Actual_Brightness[] 
            //再與Gamma2.2曲線推估的標準亮度 EstimateBrightness[] 進行比較

            //TieRegisterSetting[0] = 0; // 直接把綁點0(最亮)位置 直接寫入亮度最大值
            //Tie_Actual_Brightness[0] = EstimateBrightness_Max;//實測綁點位置0 亮度直接設定亮度最大值


            //★步驟3:先把綁點推算的亮度從EstimateBrightness DataBase放到變數Tie_Estimate_Brightness去
            //因為EstimateBrightness[]裡面是存放推算出的256階灰階亮度對應表現
            //Tie_Estimate_Brightness[]也是存放推算的值 但是是專門存放幾個綁點 提供自動調整比較使用
            for (uint tie = 0; tie < 29; tie++) //tir=0 時亮度最亮
            {   Tie_Estimate_Brightness[tie] = EstimateBrightness[VP_index[tie]];   }





            //★步驟4:依序點綁點處的灰階 並且用K80量測
            for (uint tie = 0; tie < 29; tie++) //tir=0 時亮度最亮
            {
                track_flag[0] = 0x00;//本次測試的Flag狀態 清除
                track_flag[1] = 0x00;//上次測試的Flag狀態 清除

                RETRY:

                //面板點目前要測試亮度的灰階
                tie_gray = Convert.ToByte(255 - VP_index[tie]);
                WhiskeyUtil.ImageFill(tie_gray, tie_gray, tie_gray);
                Thread.Sleep(1000);

                //K80量測亮度表現
                Tie_Actual_Brightness[tie] = Math.Round(K80_Trigger_Measurement(dive), 4);//取到小數點第4位
                

                if (Tie_Actual_Brightness[tie] > Tie_Estimate_Brightness[tie])
                {//本次實際量測亮度比推算的亮度較亮 處置~暫存器-- 讓亮度降低
                    track_flag[0] = 0x01;   //實測較推算的亮 給予Flag = 0x01

                    if (track_flag[1] == 0x10)
                    {//表示上一次為實測亮度較暗 為反折點

                        TieRegisterSetting[tie] = temp[tie];
                        //保存目前設定值與上次設定值 備用 並跳出這個綁點的測試
                        goto TieTestDone;

                    }
                    else if(track_flag[1] == 0x01)
                    {
                        //表示連續兩次調整暫存器後亮度測試都偏亮
                        //處置方式 請持續將暫存器設置變大以 降低亮度 
                        
                        if (temp[tie] >= 1023)
                        {
                            TieRegisterSetting[tie] = temp[tie];
                            goto TieTestDone;
                        }
                        temp[tie]++;
                        //單獨針對想改變的Gamma暫存器設定副程式
                        WriteGammaPartialSetting_to_SSD2130(tie, temp[tie], TieRegisterSetting);

                        goto RETRY;
                    }
                    else
                    { goto RETRY;   }

                }
                else if (Tie_Actual_Brightness[tie] < Tie_Estimate_Brightness[tie])
                {//本次實際量測亮度比推算的亮度較暗 處置~暫存器++ 讓亮度提高
                    track_flag[0] = 0x10;   //實測較推算的暗 給予Flag = 0x10

                    if (track_flag[1] == 0x01)
                    {//表示上一次為實測亮度較亮 為反折點

                        TieRegisterSetting[tie] = temp[tie];
                        //保存目前設定值與上次設定值 備用 並跳出這個綁點的測試
                        goto TieTestDone;
                    }
                    else if(track_flag[1] == 0x10)
                    {   //表示連續兩次調整暫存器後亮度測試都偏暗
                        //處置方式 請持續將暫存器設置變小以 提高亮度 
                        
                        if(temp[tie] <= 0)
                        {
                            TieRegisterSetting[tie] = temp[tie];
                            goto TieTestDone;
                        }
                        temp[tie]--;
                        //單獨針對想改變的Gamma暫存器設定副程式
                        WriteGammaPartialSetting_to_SSD2130(tie, temp[tie], TieRegisterSetting);
                        

                        goto RETRY;
                    }
                    else
                    { goto RETRY; }

                }
                else
                {//本次實際量測亮度與推算的亮度兩者一致
                    TieRegisterSetting[tie] = temp[tie];
                    track_flag[0] = 0x00;
                    track_flag[1] = 0x00;
                }
                TieTestDone:
                track_flag[0] = 0x00;//本次測試的Flag狀態 清除
                track_flag[1] = 0x00;//上次測試的Flag狀態 清除
            }


            //★步驟5:根據GMDarker_checkBox & GMBrighter_checkBox 進行判斷曲線應表現如何
            if (GMBrighter_checkBox.Checked == true )
            {
                GMDarker_checkBox.Checked = false;

                for (uint tie = 0; tie < 29; tie++)
                {//使用者亮度表現希望在標準線之上
                    if(TieRegisterSetting[tie] == 0)
                    {   TieRegisterSetting[tie] = 0;    }
                    else
                    {   TieRegisterSetting[tie] = TieRegisterSetting[tie] - 1;  }
                    
                }
            }
            else if(GMDarker_checkBox.Checked == true)
            {
                GMBrighter_checkBox.Checked = false;
                for (uint tie = 0; tie < 29; tie++)
                {//使用者亮度表現希望在標準線之下
                    if (TieRegisterSetting[tie] >= 1023)
                    { TieRegisterSetting[tie] = 1023; }
                    else
                    { TieRegisterSetting[tie] = TieRegisterSetting[tie] + 1; }
                }
            }


            //★步驟6:將選定的值 寫入暫存器中
            //Load Gamma Parameter Setting from Tie_ParameterSettingt[0~28] to Gamma Register 
            WriteGammaSettingAlltheSame_to_SSD2130(TieRegisterSetting);

            //從Tie_ParameterSetting的內容顯示於Form上的Text
            Tie_ParameterSetting_to_LoadVP_TextData(TieRegisterSetting);
            Info_textBox.AppendText("從控制盤載入IC暫存器完畢!");


            this.button4.ForeColor = Color.Black;
        }

        
        private void OTM1911A_GammaSetRegisterMapping(uint StartAddr, uint TieNum, uint GammaValueSet)
        {
            byte[] RegData = new byte[37];
            uint StartAddress = (StartAddr & 0xFF00);
            byte[] receiver = new byte[37];
            byte addr_MSB = 0;


            SL_WhiskyComm_Util WhiskeyUtil = new SL_WhiskyComm_Util();

            ReadGammaSettingAll_from_SSD2130(TieRegisterSetting);

            //STEP3: 針對想設定的值去設定
            GP_OTM1911A[TieNum] = Convert.ToUInt16(GammaValueSet);


            //STEP4: 把GP[1]~GP[29] 填到預備要寫入暫存器的空間(20170830驗證功能正確)
            uint cnt = 1;
            uint temp1 = 0, temp2 = 0;

            uint MSB = 0;
            for (uint i = 0; i < 7; i++)
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

        private void OTT1911A_CMD2_and_PassWord_Enable()
        {
            SL_WhiskyComm_Util WhiskeyUtil = new SL_WhiskyComm_Util();
            byte[] RdVal = new byte[6];
            string rdstr = null;

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
            //Info_textBox.Text +=  rdstr + "\r\n"; 
        }

        private void Vset_but_Click(object sender, EventArgs e)
        {

        }


        private void WHISKY_FPGA_InitialSetting()
        {
            SL_WhiskyComm_Util WhiskeyUtil = new SL_WhiskyComm_Util();
            WhiskeyUtil.MipiBridgeSelect(0x01); //Select 2828 Bank

            WhiskeyUtil.SetFpgaTiming(0x33, 0x11, 0x13, 0xff, 0x1E, 0x0f);

            WhiskeyUtil.SetMipiVideo(1920, 1080, 60, 16, 16, 30, 30, 4, 4);

            WhiskeyUtil.SetMipiDsi(4, 700, "syncpulse");
            uint data = 0;
            SL_Comm_Base.SPI_ReadReg(0xbb, ref data, 2);

            DSV_Setting();

            WhiskeyUtil.GpioCtrl(0x11, 0xff, 0xff); //GPIO RESET
            WhiskeyUtil.GpioCtrl(0x11, 0x00, 0x00);
            Thread.Sleep(100);
            WhiskeyUtil.GpioCtrl(0x11, 0xff, 0xff);
        }


        private void button3_Click(object sender, EventArgs e)
        {
            this.button3.ForeColor = Color.Green;
            Application.DoEvents();

            SL_WhiskyComm_Util WhiskeyUtil = new SL_WhiskyComm_Util();

            Info_textBox.Text = "";

            WHISKY_FPGA_InitialSetting();//Include FPGA initial、2828 initial、DSV initial and Driver reset setting 


            OTT1911A_CMD2_and_PassWord_Enable();
            //SSD2123_InitialCode_forAUO_nmosTypeA();


            WhiskeyUtil.MipiWrite(0x05, 0x11);//Sleep-Out
            Thread.Sleep(100);
            WhiskeyUtil.MipiWrite(0x05, 0x29);//Display-On

            this.button3.ForeColor = Color.Black;
        }


        private void SSD2123_InitialCode_forAUO_nmosTypeA()
        {
            SL_WhiskyComm_Util WhiskeyUtil = new SL_WhiskyComm_Util();
            WhiskeyUtil.MipiWrite(0x29, 0xff, 0x21, 0x30, 0x10);
            WhiskeyUtil.MipiWrite(0x23, 0x00, 0x14);            //[5:0]t8_de
            WhiskeyUtil.MipiWrite(0x23, 0x01, 0x00);            //[5:0]t7p_de
            WhiskeyUtil.MipiWrite(0x23, 0x02, 0x0C);            //[7:4]t9p_de, [3:0]t9_de
            WhiskeyUtil.MipiWrite(0x23, 0x03, 0x2B);            //[5:0]t7_de
            WhiskeyUtil.MipiWrite(0x23, 0x54, 0x0C);            //[5:0]SD-CKH  Setup time, refer to t9_de
            WhiskeyUtil.MipiWrite(0x23, 0x05, 0x11);            //[7:4]ckh_vbp, [3:0]ckh_vfp
            WhiskeyUtil.MipiWrite(0x23, 0x0D, 0x82);            //[7]CKH_VP_Full, [6:5]CKH2_RGB_Sel, [4]CKH_VP_REG_EN, [3]CKH_RGB_Zigazg, [2]CKH_321_Frame, [1]CKH_321_Line, [0]CKH_321

            WhiskeyUtil.MipiWrite(0x23, 0x20, 0x41);            //[7:6]STV_A_Rise[6:5], [4:0]STV_A_Rise[4:0]
            WhiskeyUtil.MipiWrite(0x23, 0x21, 0x29);            //[5]FTI_A_Rise_mode, [4]FTI_A_Fall_mode, [3:2]Phase_STV_A, [1:0]Overlap_STV_A
            WhiskeyUtil.MipiWrite(0x23, 0x22, 0x62);            //[7:0]FTI_A_Rise
            WhiskeyUtil.MipiWrite(0x23, 0x23, 0x62);            //[7:0]FTI_A_Fall
            WhiskeyUtil.MipiWrite(0x23, 0x25, 0x02);            //[7:6]STV_B_Rise[6:5], [4:0]STV_B_Rise[4:0]
            WhiskeyUtil.MipiWrite(0x23, 0x26, 0x2E);            //[5]FTI_B_Rise_mode, [4]FTI_B_Fall_mode, [3:2]Phase_STV_B, [1:0]Overlap_STV_B
            WhiskeyUtil.MipiWrite(0x23, 0x27, 0x62);            //[7:0]FTI_B_Rise
            WhiskeyUtil.MipiWrite(0x23, 0x28, 0x62);            //[7:0]FTI_B_Fall
            WhiskeyUtil.MipiWrite(0x23, 0x2A, 0x02);            //[7:6]STV_C_Rise[6:5], [4:0]STV_C_Rise[4:0]
            WhiskeyUtil.MipiWrite(0x23, 0x2B, 0x29);            //[5]FTI_C_Rise_mode, [4]FTI_C_Fall_mode, [3:2]Phase_STV_C, [1:0]Overlap_STV_C
            WhiskeyUtil.MipiWrite(0x23, 0x2C, 0x62);            //[7:0]FTI_C_Rise
            WhiskeyUtil.MipiWrite(0x23, 0x2D, 0x62);            //[7:0]FTI_C_Fall

            WhiskeyUtil.MipiWrite(0x23, 0x30, 0x81);            //[7]CLK_A_Rise[5], [4:0]CLK_A_Rise[4:0]
            WhiskeyUtil.MipiWrite(0x23, 0x31, 0x01);            //[7]CLK_A_Fall[5], [4:0]CLK_A_Fall[4:0]
            WhiskeyUtil.MipiWrite(0x23, 0x32, 0x11);            //[7:4]Phase_CLK_A, [3:0]Overlap_CLK_A
            WhiskeyUtil.MipiWrite(0x23, 0x33, 0x31);            //[7]CLK_A_inv, [6]CLK_A_stop_level, [5] CLK_A_ct_mode, [4] CLK_A_Keep, [1]CLW_A_Rise_mode, [0]CLW_A_Fall_mode
            WhiskeyUtil.MipiWrite(0x23, 0x34, 0x0C);            //[7:0]CLW_A1_Rise
            WhiskeyUtil.MipiWrite(0x23, 0x35, 0x0C);            //[7:0]CLW_A2_Rise
            WhiskeyUtil.MipiWrite(0x23, 0x36, 0x00);            //[7:0]CLW_A1_Fall
            WhiskeyUtil.MipiWrite(0x23, 0x37, 0x00);            //[7:0]CLW_A2_Fall
            WhiskeyUtil.MipiWrite(0x23, 0x38, 0x00);            //[7:0]CLK_A_Rise_eqt1
            WhiskeyUtil.MipiWrite(0x23, 0x39, 0x00);            //[7:0]CLK_A_Rise_eqt2
            WhiskeyUtil.MipiWrite(0x23, 0x3A, 0x00);            //[7:0]CLK_A_Fall_eqt1
            WhiskeyUtil.MipiWrite(0x23, 0x3B, 0x00);            //[7:0]CLK_A_Fall_eqt2
            WhiskeyUtil.MipiWrite(0x23, 0x3C, 0x20);            //[5]CLK_A_VBP_Keep_gs_Chg, [4]CLK_A_VFP_Keep_gs_Chg, [3:2]CLK_A_Keep_Pos2_gs_Chg, [1:0]CLK_A_Keep_Pos1_gs_Chg
            WhiskeyUtil.MipiWrite(0x23, 0x3D, 0x08);            //[7:6]CLK_A4_Stop_Level_gs_Chg, [5:4] CLK_A3_Stop_Level_gs_Chg, [3:2]CLK_A2_Stop_Level_gs_Chg, [1:0]CLK_A1_Stop_Level_gs_Chg
            WhiskeyUtil.MipiWrite(0x23, 0x3E, 0x00);            //[7]CLK_A_Keep_Pos1[5], [4:0]CLK_A_Keep_Pos1[4:0]
            WhiskeyUtil.MipiWrite(0x23, 0x3F, 0x00);            //[7]CLK_A_Keep_Pos2[5], [4:0]CLK_A_Keep_Pos2[4:0]
            WhiskeyUtil.MipiWrite(0x23, 0x40, 0x00);            //[7]CLK_B_Rise[5], [4:0]CLK_B_Rise[4:0]
            WhiskeyUtil.MipiWrite(0x23, 0x41, 0x00);            //[7]CLK_B_Fall[5], [4:0]CLK_B_Fall[4:0]
            WhiskeyUtil.MipiWrite(0x23, 0x42, 0x00);            //[7:4]Phase_CLK_B, [3:0]Overlap_CLK_B
            WhiskeyUtil.MipiWrite(0x23, 0x43, 0x00);            //[7]CLK_B_inv, [6]CLK_B_stop_level, [5] CLK_B_ct_mode, [4] CLK_B_Keep, [1]CLW_B_Rise_mode, [0]CLW_B_Fall_mode
            WhiskeyUtil.MipiWrite(0x23, 0x44, 0x00);            //[7:0]CLW_B1_Rise
            WhiskeyUtil.MipiWrite(0x23, 0x45, 0x00);            //[7:0]CLW_B2_Rise
            WhiskeyUtil.MipiWrite(0x23, 0x46, 0x00);            //[7:0]CLW_B1_Fall
            WhiskeyUtil.MipiWrite(0x23, 0x47, 0x00);            //[7:0]CLW_B2_Fall
            WhiskeyUtil.MipiWrite(0x23, 0x48, 0x00);            //[7:0]CLK_B_Rise_eqt1
            WhiskeyUtil.MipiWrite(0x23, 0x49, 0x00);            //[7:0]CLK_B_Rise_eqt2
            WhiskeyUtil.MipiWrite(0x23, 0x4A, 0x00);            //[7:0]CLK_B_Fall_eqt1
            WhiskeyUtil.MipiWrite(0x23, 0x4B, 0x00);            //[7:0]CLK_B_Fall_eqt2
            WhiskeyUtil.MipiWrite(0x23, 0x4C, 0x00);            //[5]CLK_B_VBP_Keep_gs_Chg, [4]CLK_B_VFP_Keep_gs_Chg, [3:2]CLK_B_Keep_Pos2_gs_Chg, [1:0]CLK_B_Keep_Pos1_gs_Chg
            WhiskeyUtil.MipiWrite(0x23, 0x4D, 0x00);            //[7:6]CLK_B4_Stop_Level_gs_Chg, [5:4] CLK_B3_Stop_Level_gs_Chg, [3:2]CLK_B2_Stop_Level_gs_Chg, [1:0]CLK_B1_Stop_Level_gs_Chg
            WhiskeyUtil.MipiWrite(0x23, 0x4E, 0x00);            //[7]CLK_B_Keep_Pos1[5], [4:0]CLK_B_Keep_Pos1[4:0]
            WhiskeyUtil.MipiWrite(0x23, 0x4F, 0x00);            //[7]CLK_B_Keep_Pos2[5], [4:0]CLK_B_Keep_Pos2[4:0]

            WhiskeyUtil.MipiWrite(0x23, 0x70, 0x06);            //GOUT_R_01_FW
            WhiskeyUtil.MipiWrite(0x23, 0x71, 0x0E);            //GOUT_R_02_FW
            WhiskeyUtil.MipiWrite(0x23, 0x72, 0x37);            //GOUT_R_03_FW
            WhiskeyUtil.MipiWrite(0x23, 0x73, 0x36);            //GOUT_R_04_FW
            WhiskeyUtil.MipiWrite(0x23, 0x74, 0x0A);            //GOUT_R_05_FW
            WhiskeyUtil.MipiWrite(0x23, 0x75, 0x2A);            //GOUT_R_06_FW
            WhiskeyUtil.MipiWrite(0x23, 0x76, 0x2A);            //GOUT_R_07_FW
            WhiskeyUtil.MipiWrite(0x23, 0x77, 0x10);            //GOUT_R_08_FW
            WhiskeyUtil.MipiWrite(0x23, 0x78, 0x11);            //GOUT_R_09_FW
            WhiskeyUtil.MipiWrite(0x23, 0x79, 0x00);            //GOUT_R_10_FW
            WhiskeyUtil.MipiWrite(0x23, 0x7A, 0x00);            //GOUT_R_11_FW
            WhiskeyUtil.MipiWrite(0x23, 0x7B, 0x00);            //GOUT_R_12_FW
            WhiskeyUtil.MipiWrite(0x23, 0x7C, 0x00);            //GOUT_R_13_FW
            WhiskeyUtil.MipiWrite(0x23, 0x7D, 0x30);            //GOUT_R_14_FW
            WhiskeyUtil.MipiWrite(0x23, 0x7E, 0x31);            //GOUT_R_15_FW
            WhiskeyUtil.MipiWrite(0x23, 0x7F, 0x32);            //GOUT_R_16_FW
            WhiskeyUtil.MipiWrite(0x23, 0x80, 0x06);            //GOUT_L_01_FW
            WhiskeyUtil.MipiWrite(0x23, 0x81, 0x0E);            //GOUT_L_02_FW
            WhiskeyUtil.MipiWrite(0x23, 0x82, 0x37);            //GOUT_L_03_FW
            WhiskeyUtil.MipiWrite(0x23, 0x83, 0x36);            //GOUT_L_04_FW
            WhiskeyUtil.MipiWrite(0x23, 0x84, 0x0A);            //GOUT_L_05_FW
            WhiskeyUtil.MipiWrite(0x23, 0x85, 0x2A);            //GOUT_L_06_FW
            WhiskeyUtil.MipiWrite(0x23, 0x86, 0x2A);            //GOUT_L_07_FW
            WhiskeyUtil.MipiWrite(0x23, 0x87, 0x10);            //GOUT_L_08_FW
            WhiskeyUtil.MipiWrite(0x23, 0x88, 0x11);            //GOUT_L_09_FW
            WhiskeyUtil.MipiWrite(0x23, 0x89, 0x00);            //GOUT_L_10_FW
            WhiskeyUtil.MipiWrite(0x23, 0x8A, 0x00);            //GOUT_L_11_FW
            WhiskeyUtil.MipiWrite(0x23, 0x8B, 0x00);            //GOUT_L_12_FW
            WhiskeyUtil.MipiWrite(0x23, 0x8C, 0x00);            //GOUT_L_13_FW
            WhiskeyUtil.MipiWrite(0x23, 0x8D, 0x30);            //GOUT_L_14_FW
            WhiskeyUtil.MipiWrite(0x23, 0x8E, 0x31);            //GOUT_L_15_FW
            WhiskeyUtil.MipiWrite(0x23, 0x8F, 0x32);            //GOUT_L_16_FW
            WhiskeyUtil.MipiWrite(0x23, 0x90, 0x0E);            //GOUT_R_01_BW
            WhiskeyUtil.MipiWrite(0x23, 0x91, 0x06);            //GOUT_R_02_BW
            WhiskeyUtil.MipiWrite(0x23, 0x92, 0x37);            //GOUT_R_03_BW
            WhiskeyUtil.MipiWrite(0x23, 0x93, 0x36);            //GOUT_R_04_BW
            WhiskeyUtil.MipiWrite(0x23, 0x94, 0x0A);            //GOUT_R_05_BW
            WhiskeyUtil.MipiWrite(0x23, 0x95, 0x2A);            //GOUT_R_06_BW
            WhiskeyUtil.MipiWrite(0x23, 0x96, 0x2A);            //GOUT_R_07_BW
            WhiskeyUtil.MipiWrite(0x23, 0x97, 0x11);            //GOUT_R_08_BW
            WhiskeyUtil.MipiWrite(0x23, 0x98, 0x10);            //GOUT_R_09_BW
            WhiskeyUtil.MipiWrite(0x23, 0x99, 0x00);            //GOUT_R_10_BW
            WhiskeyUtil.MipiWrite(0x23, 0x9A, 0x00);            //GOUT_R_11_BW
            WhiskeyUtil.MipiWrite(0x23, 0x9B, 0x00);            //GOUT_R_12_BW
            WhiskeyUtil.MipiWrite(0x23, 0x9C, 0x00);            //GOUT_R_13_BW
            WhiskeyUtil.MipiWrite(0x23, 0x9D, 0x30);            //GOUT_R_14_BW
            WhiskeyUtil.MipiWrite(0x23, 0x9E, 0x31);            //GOUT_R_15_BW
            WhiskeyUtil.MipiWrite(0x23, 0x9F, 0x32);            //GOUT_R_16_BW
            WhiskeyUtil.MipiWrite(0x23, 0xA0, 0x0E);            //GOUT_L_01_BW
            WhiskeyUtil.MipiWrite(0x23, 0xA1, 0x06);            //GOUT_L_02_BW
            WhiskeyUtil.MipiWrite(0x23, 0xA2, 0x37);            //GOUT_L_03_BW
            WhiskeyUtil.MipiWrite(0x23, 0xA3, 0x36);            //GOUT_L_04_BW
            WhiskeyUtil.MipiWrite(0x23, 0xA4, 0x0A);            //GOUT_L_05_BW
            WhiskeyUtil.MipiWrite(0x23, 0xA5, 0x2A);            //GOUT_L_06_BW
            WhiskeyUtil.MipiWrite(0x23, 0xA6, 0x2A);            //GOUT_L_07_BW
            WhiskeyUtil.MipiWrite(0x23, 0xA7, 0x11);            //GOUT_L_08_BW
            WhiskeyUtil.MipiWrite(0x23, 0xA8, 0x10);            //GOUT_L_09_BW
            WhiskeyUtil.MipiWrite(0x23, 0xA9, 0x00);            //GOUT_L_10_BW
            WhiskeyUtil.MipiWrite(0x23, 0xAA, 0x00);            //GOUT_L_11_BW
            WhiskeyUtil.MipiWrite(0x23, 0xAB, 0x00);            //GOUT_L_12_BW
            WhiskeyUtil.MipiWrite(0x23, 0xAC, 0x00);            //GOUT_L_13_BW
            WhiskeyUtil.MipiWrite(0x23, 0xAD, 0x30);            //GOUT_L_14_BW
            WhiskeyUtil.MipiWrite(0x23, 0xAE, 0x31);            //GOUT_L_15_BW
            WhiskeyUtil.MipiWrite(0x23, 0xAF, 0x32);            //GOUT_L_16_BW

            WhiskeyUtil.MipiWrite(0x23, 0xC7, 0x22);            //[7:4]Blank_Frame_OPT1[3:0], [3:0]Blank_Frame_OPT2[3:0]
            WhiskeyUtil.MipiWrite(0x23, 0xC8, 0x57);            //[7:6]SRC_Front_Blank_Sel, [5:4]SRC_Mid_Blank_Sel, [3:2]SRC_Back_Blank_Sel
            WhiskeyUtil.MipiWrite(0x23, 0xCB, 0x00);            //[5:4]GOUT_LVD, [3:2]GOUT_SO, [1:0]GOUT_SI
            WhiskeyUtil.MipiWrite(0x23, 0xD0, 0x11);            //[5:4]ONSeq_Ext, [2:0] OFFSeq_Ext
            WhiskeyUtil.MipiWrite(0x23, 0xD2, 0x79);            //[7:6]CLK_B_ONSeq_Ext, [5]CLK_A_ONSeq_Ext, [4] STV_C_ONSeq_Ext, [3:2]STV_B_ONSeq_Ext, STV_A_ONSeq_Ext, 00:ori, 01:VGL, 10:VGH, 11:GND
            WhiskeyUtil.MipiWrite(0x23, 0xD3, 0x19);            //[5:4]CKH_ONSeq_Ext, [3]CLK_D_ONSeq_Ext, [1:0]CLK_C_ONSeq_Ext
            WhiskeyUtil.MipiWrite(0x23, 0xD4, 0x10);            //[6]RESET_ONSeq_Ext, [4]CLK_E_ONSeq_Ext, [2]GAS_ONSeq_Ext, [1:0]FWBW_ONSeq_Ext
            WhiskeyUtil.MipiWrite(0x23, 0xD6, 0x00);            //[7:6]CLK_A_OFFSeq_Ext, [5:4]STV_B_OFFSeq_Ext, [3:2]STV_B_OFFSeq_Ext, [1:0]STV_A_OFFSeq_Ext
            WhiskeyUtil.MipiWrite(0x23, 0xD7, 0x00);            //[7:6]CKH_OFFSeq_Ext, [5:4]CLK_D_OFFSeq_Ext, [3:2]CLK_C_OFFSeq_Ext, [1:0]CLK_B_OFFSeq_Ext
            WhiskeyUtil.MipiWrite(0x23, 0xD8, 0x00);            //[7:6]CLK_E_OFFSeq_Ext, [4]Reset_OFFSet_Ext, [2]GAS_OFFSeq_Ext, [1:0]FWBW_OFFSeq_Ext
            WhiskeyUtil.MipiWrite(0x23, 0xDA, 0xFF);            //[7]CKH_AbnSeq, [6]CLK_D_AbnSeq, [5]CLK_C_AbnSeq, [4]CLK_B_AbnSeq, [3]CLK_A_AbnSeq, [2]STV_C_AbnSeq, [1]STV_B_AbnSeq, [0]STV_A_AbnSeq, 0:VGL, 1:VGH
            WhiskeyUtil.MipiWrite(0x23, 0xDB, 0x1A);            //[4]CLK_E_AbnSeq, [3]Reset_AbnSeq, [2]GAS_AbnSeq, [1:0]FWBW_AbnSeq, 00:norm, 01:VGL, 10:VGH
            WhiskeyUtil.MipiWrite(0x23, 0xE0, 0x54);            //[7:6]STV_A_ONSeq, [5:4]STV_B_ONSeq, [3:2]STV_C_ONSeq, 00:ori, 01:VGL, 10:VGH, 11:GND
            WhiskeyUtil.MipiWrite(0x23, 0xE1, 0x15);            //[6:4]CLK_A_ONSeq, [4:3]CLK_B_ONSeq, [1:0]CLK_C_ONSeq, 00:ori, 01:VGL, 10:VGH
            WhiskeyUtil.MipiWrite(0x23, 0xE2, 0x19);            //[7:6]CLK_D_ONSeq, [5:4]CLK_E_ONSeq, [3:2]CKH_ONSeq, [1:0]FWBW_ONSeq
            WhiskeyUtil.MipiWrite(0x23, 0xE3, 0x00);            //[3:2]GAS_ONSeq, [1:0]Reset_ONSeq
            WhiskeyUtil.MipiWrite(0x23, 0xE4, 0x00);            //[7:6]STV_A_OFFSeq, [5:4]STV_B_OFFSeq, [3:2]STV_C_OFFSeq, [1:0]CLK_A_OFFSeq, 00:ori, 01:VGL, 10:VGH
            WhiskeyUtil.MipiWrite(0x23, 0xE5, 0x00);            //[7:6]CLK_B_OFFSeq, [5:4]CLK_C_OFFSeq, [3:2]CLK_D_OFFSeq, [1:0]CLK_E_OFFSeq
            WhiskeyUtil.MipiWrite(0x23, 0xE6, 0x10);            //[7:6]CKH_OFFSeq, [5:4]FWBW_OFFSeq, [3:2]Reset_OFFSet, [1:0]GAS_OFFSeq
            WhiskeyUtil.MipiWrite(0x23, 0xE7, 0x75);            //[6]SRC_ONSeq_OPT, [5:4]VCM_ONSeq_OPT, [2]SRC_OFFSeq_OPT, [1:0]VCM_OFFSeq_OPT
            WhiskeyUtil.MipiWrite(0x23, 0xEA, 0x00);            //[7:4]STV_Onoff_Seq_dly, [3:0]VCK_A_Onoff_Seq_dly
            WhiskeyUtil.MipiWrite(0x23, 0xEB, 0x00);            //[7:4]VCK_B_Onoff_Seq_dly, [3:0]VCK_C_Onoff_Seq_dly
            WhiskeyUtil.MipiWrite(0x23, 0xEC, 0x00);            //[7:4]CKH_Onoff_Seq_dly, [3:0]GAS_Onoff_Seq_dly
            WhiskeyUtil.MipiWrite(0x23, 0xF2, 0x00);            //[7]GS_Sync_2frm_opt
            WhiskeyUtil.MipiWrite(0x23, 0xF5, 0x43);            //[7:6]RST_Each_Frame, [5]GIP_RST_INV, [4]PWRON_RST_OPT, [3:0]GRST_WID_ONSeq_EXT[11:8]

            WhiskeyUtil.MipiWrite(0x29, 0xff, 0x21, 0x30, 0x11);
            WhiskeyUtil.MipiWrite(0x23, 0x60, 0x00);            // Panel Scheme Selection
            WhiskeyUtil.MipiWrite(0x23, 0x62, 0x20);            // Column Inversion
            WhiskeyUtil.MipiWrite(0x23, 0x65, 0x11);            //[6:4]VFP_CKH_DUM, [2:0]VBP_CKH_DUM

            WhiskeyUtil.MipiWrite(0x29, 0xff, 0x21, 0x30, 0x12);
            WhiskeyUtil.MipiWrite(0x23, 0x11, 0x8D);            //[7]VGH_ratio, [5:0]VGHS, default:12v; AUO:8.5v; CSOT:9v
            WhiskeyUtil.MipiWrite(0x23, 0x12, 0x0F);            //[7]VGL_ratio, [5:0]VGLS, default:-8v; AUO:-8v; CSOT:-7v

            WhiskeyUtil.MipiWrite(0x29, 0xff, 0x21, 0x30, 0x00);
        }




        /*
         * WriteGammaPartialSetting_to_SSD2130 說明
         * 功能:使用者能針對自己想設定的Gamma進行改變 並且寫入暫存器所有Page31 R+   Page32 R-   Page33 G+   Page34 G-   Page35 B+   Page36 B-
         * uint TieValue:0~29個綁點 看要想改變哪個
         * uint GammaWantSetting:想改變的綁點 想填入多少的Gamma設定值
         * uint[] GammaNowSetting:目前Gamma全部0~28的設定值為多少
         * 請注意! SSD2130 寫入Gamma設定值 應該是0~28個綁點所有設定都寫入後 IC會判斷最後一個暫存器寫入值後才會Trigger一次整個Page套用新值
        */
        private void WriteGammaPartialSetting_to_SSD2130(uint TieValue, uint GammaWantSetting, uint[] GammaNowSetting)
        {
            byte TieCnt = 0;
            byte page = 0x00;
            uint temp = 0;
            byte RegisterSetting = 0x00;
            SL_WhiskyComm_Util WhiskeyUtil = new SL_WhiskyComm_Util();

            //切換SSD2130 PassWord & page
            //Page31 R+   Page32 R-   Page33 G+   Page34 G-   Page35 B+   Page36 B-
            for (page = 31; page <= 36; page++)
            {
                WhiskeyUtil.MipiWrite(0x29, 0xFF, 0x21, 0x30, page);


                for (TieCnt = 0; TieCnt < 29; TieCnt++)
                {
                    uint temp2 = TieCnt;
                    byte addr = 0;

                    if(TieCnt == TieValue)
                    {   addr = 0;   }


                    temp2 = temp2 * 2;
                    addr = Convert.ToByte(temp2);

                    if(TieValue == TieCnt)
                    {   temp = GammaWantSetting; }
                    else
                    {   temp = GammaNowSetting[TieCnt]; }
                    temp >>= 8;
                    temp = temp & 0x03;
                    RegisterSetting = Convert.ToByte(temp);
                    WhiskeyUtil.MipiWrite(0x23, addr, RegisterSetting);



                    temp2 = TieCnt;
                    temp2 = (temp2 * 2) + 1;
                    addr = Convert.ToByte(temp2);

                    if (TieValue == TieCnt)
                    { temp = GammaWantSetting; }
                    else
                    { temp = GammaNowSetting[TieCnt]; }
                    temp = temp & 0xFF;
                    RegisterSetting = Convert.ToByte(temp);
                    WhiskeyUtil.MipiWrite(0x23, addr, RegisterSetting);
                }
            }
            WhiskeyUtil.MipiWrite(0x29, 0xFF, 0x21, 0x30, 0x00);
        }


        private void button4_Click(object sender, EventArgs e)
        {

            SL_WhiskyComm_Util WhiskeyUtil = new SL_WhiskyComm_Util();
            WhiskeyUtil.MipiWrite(0x23, 0x2E, 0x01);
            WhiskeyUtil.MipiWrite(0x23, 0x3E, 0x01);


            byte[] RdVal = new byte[1];

            WhiskeyUtil.MipiRead(0x52, 1, ref RdVal);
            WhiskeyUtil.MipiRead(0x54, 1, ref RdVal);
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
            this.button6.ForeColor = Color.Green;
            Application.DoEvents();

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
            this.button6.ForeColor = Color.Black;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            this.button7.ForeColor = Color.Green;
            Application.DoEvents();
            SL_WhiskyComm_Util WhiskeyUtil = new SL_WhiskyComm_Util();
            WhiskeyUtil.MipiBridgeSelect(0x01); //Select 2828 Bank
            WhiskeyUtil.ImageShow("VG.bmp");
            this.button4.ForeColor = Color.Black;
        }

        private void button8_Click(object sender, EventArgs e)
        {
            byte[] receiver_cmp = new byte[38];
            byte[] receiver = new byte[38];
            SL_WhiskyComm_Util WhiskeyUtil = new SL_WhiskyComm_Util();

            for(int time=0; time<5000; time++)
            {
                WhiskeyUtil.MipiWrite(0x23, 0x00, 0x00);
                WhiskeyUtil.MipiRead(0xE1, 38, ref receiver);

                receiver_cmp[0] = 0;
                receiver_cmp[1] = 115;
                receiver_cmp[2] = 207;
                receiver_cmp[3] = 16;
                receiver_cmp[4] = 64;
                receiver_cmp[5] = 53;
                receiver_cmp[6] = 91;
                receiver_cmp[7] = 156;
                receiver_cmp[8] = 204;
                receiver_cmp[9] = 85;
                receiver_cmp[10] = 184;
                receiver_cmp[11] = 229;
                receiver_cmp[12] = 19;
                receiver_cmp[13] = 53;
                receiver_cmp[14] = 165;
                receiver_cmp[15] = 96;
                receiver_cmp[16] = 130;
                receiver_cmp[17] = 244;
                receiver_cmp[18] = 25;
                receiver_cmp[19] = 154;
                receiver_cmp[20] = 56;
                receiver_cmp[21] = 89;
                receiver_cmp[22] = 120;
                receiver_cmp[23] = 152;
                receiver_cmp[24] = 170;
                receiver_cmp[25] = 192;
                receiver_cmp[26] = 193;
                receiver_cmp[27] = 243;
                receiver_cmp[28] = 45;
                receiver_cmp[29] = 234;
                receiver_cmp[30] = 78;
                receiver_cmp[31] = 116;
                receiver_cmp[32] = 175;
                receiver_cmp[33] = 232;
                receiver_cmp[34] = 255;
                receiver_cmp[35] = 255;
                receiver_cmp[36] = 3;
                receiver_cmp[37] = 3;

                for (int i = 0; i < 38; i++)
                {
                    if (receiver_cmp[i] != receiver[i])
                    {
                        Info_textBox.Text = "READ ERROR !!!";

                    }
                }
                Info_textBox.Text = "測試次數="+Convert.ToString(time);
                Application.DoEvents();
            }
        }

        private void button8_Click_1(object sender, EventArgs e)
        {
            SL_WhiskyComm_Util WhiskeyUtil = new SL_WhiskyComm_Util();

            
            WhiskeyUtil.ImageFill(255, 255, 0);
            Thread.Sleep(1000);
        }
    }
}
