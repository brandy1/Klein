namespace K_80
{
    partial class MainForm
    {
        /// <summary>
        /// 設計工具所需的變數。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清除任何使用中的資源。
        /// </summary>
        /// <param name="disposing">如果應該處置 Managed 資源則為 true，否則為 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 設計工具產生的程式碼

        /// <summary>
        /// 此為設計工具支援所需的方法 - 請勿使用程式碼編輯器修改
        /// 這個方法的內容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series2 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series3 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series4 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.ComPortState_label = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.ComPortSel_comboBox = new System.Windows.Forms.ComboBox();
            this.ComPortCheck_Button = new System.Windows.Forms.Button();
            this.gbx_openelecs = new System.Windows.Forms.GroupBox();
            this.lbl_elecs_status = new System.Windows.Forms.Label();
            this.btn_oepnelecs = new System.Windows.Forms.Button();
            this.cbo_elecsport = new System.Windows.Forms.ComboBox();
            this.lbl_elecs = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.YMin_label = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.YMax_label = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.GetEstimateBrightness_button = new System.Windows.Forms.Button();
            this.Gamma_set_tolerance_textBox = new System.Windows.Forms.TextBox();
            this.GammaSet_textBox = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.GMDarker_checkBox = new System.Windows.Forms.CheckBox();
            this.RdRegisteer2Text_but = new System.Windows.Forms.Button();
            this.GMBrighter_checkBox = new System.Windows.Forms.CheckBox();
            this.WrText2GammaRegister_but = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.Info_textBox = new System.Windows.Forms.TextBox();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.chart1 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.groupBox7 = new System.Windows.Forms.GroupBox();
            this.button6 = new System.Windows.Forms.Button();
            this.button7 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.deviceTimer = new System.Windows.Forms.Timer(this.components);
            this.gbx_usb = new System.Windows.Forms.GroupBox();
            this.txtbox_info = new System.Windows.Forms.TextBox();
            this.lbl_info = new System.Windows.Forms.Label();
            this.txtbox_pid = new System.Windows.Forms.TextBox();
            this.lbl_pid = new System.Windows.Forms.Label();
            this.txtbox_vid = new System.Windows.Forms.TextBox();
            this.lbl_vid = new System.Windows.Forms.Label();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.button8 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.gbx_openelecs.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox6.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).BeginInit();
            this.groupBox7.SuspendLayout();
            this.gbx_usb.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.SuspendLayout();
            // 
            // ComPortState_label
            // 
            this.ComPortState_label.AutoSize = true;
            this.ComPortState_label.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.ComPortState_label.ForeColor = System.Drawing.Color.Red;
            this.ComPortState_label.Location = new System.Drawing.Point(576, 60);
            this.ComPortState_label.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.ComPortState_label.Name = "ComPortState_label";
            this.ComPortState_label.Size = new System.Drawing.Size(197, 25);
            this.ComPortState_label.TabIndex = 2;
            this.ComPortState_label.Text = "K80 Not Connect!!";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label1.Location = new System.Drawing.Point(576, 26);
            this.label1.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(118, 25);
            this.label1.TabIndex = 1;
            this.label1.Text = "COM Port :";
            // 
            // ComPortSel_comboBox
            // 
            this.ComPortSel_comboBox.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold);
            this.ComPortSel_comboBox.FormattingEnabled = true;
            this.ComPortSel_comboBox.Location = new System.Drawing.Point(706, 24);
            this.ComPortSel_comboBox.Margin = new System.Windows.Forms.Padding(6, 7, 6, 7);
            this.ComPortSel_comboBox.Name = "ComPortSel_comboBox";
            this.ComPortSel_comboBox.Size = new System.Drawing.Size(113, 27);
            this.ComPortSel_comboBox.TabIndex = 1;
            // 
            // ComPortCheck_Button
            // 
            this.ComPortCheck_Button.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ComPortCheck_Button.Location = new System.Drawing.Point(836, 19);
            this.ComPortCheck_Button.Margin = new System.Windows.Forms.Padding(6, 7, 6, 7);
            this.ComPortCheck_Button.Name = "ComPortCheck_Button";
            this.ComPortCheck_Button.Size = new System.Drawing.Size(100, 59);
            this.ComPortCheck_Button.TabIndex = 0;
            this.ComPortCheck_Button.Text = "SET OK";
            this.ComPortCheck_Button.UseVisualStyleBackColor = true;
            this.ComPortCheck_Button.Click += new System.EventHandler(this.ComPortCheck_Button_Click);
            // 
            // gbx_openelecs
            // 
            this.gbx_openelecs.Controls.Add(this.lbl_elecs_status);
            this.gbx_openelecs.Controls.Add(this.btn_oepnelecs);
            this.gbx_openelecs.Controls.Add(this.cbo_elecsport);
            this.gbx_openelecs.Controls.Add(this.lbl_elecs);
            this.gbx_openelecs.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold);
            this.gbx_openelecs.Location = new System.Drawing.Point(8, 878);
            this.gbx_openelecs.Margin = new System.Windows.Forms.Padding(6, 7, 6, 7);
            this.gbx_openelecs.Name = "gbx_openelecs";
            this.gbx_openelecs.Padding = new System.Windows.Forms.Padding(6, 7, 6, 7);
            this.gbx_openelecs.Size = new System.Drawing.Size(972, 113);
            this.gbx_openelecs.TabIndex = 2;
            this.gbx_openelecs.TabStop = false;
            this.gbx_openelecs.Text = "非必要使用(E7422)";
            // 
            // lbl_elecs_status
            // 
            this.lbl_elecs_status.AutoSize = true;
            this.lbl_elecs_status.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lbl_elecs_status.ForeColor = System.Drawing.Color.Red;
            this.lbl_elecs_status.Location = new System.Drawing.Point(273, 41);
            this.lbl_elecs_status.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lbl_elecs_status.Name = "lbl_elecs_status";
            this.lbl_elecs_status.Size = new System.Drawing.Size(199, 25);
            this.lbl_elecs_status.TabIndex = 3;
            this.lbl_elecs_status.Text = "E7422 Not Connect";
            // 
            // btn_oepnelecs
            // 
            this.btn_oepnelecs.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_oepnelecs.Location = new System.Drawing.Point(151, 22);
            this.btn_oepnelecs.Margin = new System.Windows.Forms.Padding(6, 7, 6, 7);
            this.btn_oepnelecs.Name = "btn_oepnelecs";
            this.btn_oepnelecs.Size = new System.Drawing.Size(100, 64);
            this.btn_oepnelecs.TabIndex = 4;
            this.btn_oepnelecs.Text = "SET OK";
            this.btn_oepnelecs.UseVisualStyleBackColor = true;
            this.btn_oepnelecs.Click += new System.EventHandler(this.btn_oepnelecs_Click);
            // 
            // cbo_elecsport
            // 
            this.cbo_elecsport.FormattingEnabled = true;
            this.cbo_elecsport.Location = new System.Drawing.Point(17, 59);
            this.cbo_elecsport.Margin = new System.Windows.Forms.Padding(6, 7, 6, 7);
            this.cbo_elecsport.Name = "cbo_elecsport";
            this.cbo_elecsport.Size = new System.Drawing.Size(113, 27);
            this.cbo_elecsport.TabIndex = 3;
            // 
            // lbl_elecs
            // 
            this.lbl_elecs.AutoSize = true;
            this.lbl_elecs.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lbl_elecs.Location = new System.Drawing.Point(12, 27);
            this.lbl_elecs.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lbl_elecs.Name = "lbl_elecs";
            this.lbl_elecs.Size = new System.Drawing.Size(118, 25);
            this.lbl_elecs.TabIndex = 2;
            this.lbl_elecs.Text = "COM Port :";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.YMin_label);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.YMax_label);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.GetEstimateBrightness_button);
            this.groupBox2.Controls.Add(this.Gamma_set_tolerance_textBox);
            this.groupBox2.Controls.Add(this.GammaSet_textBox);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.comboBox1);
            this.groupBox2.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold);
            this.groupBox2.Location = new System.Drawing.Point(170, 99);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(331, 169);
            this.groupBox2.TabIndex = 21;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "取得標準Gamma曲線";
            // 
            // YMin_label
            // 
            this.YMin_label.AutoSize = true;
            this.YMin_label.Font = new System.Drawing.Font("微軟正黑體", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.YMin_label.Location = new System.Drawing.Point(11, 142);
            this.YMin_label.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
            this.YMin_label.Name = "YMin_label";
            this.YMin_label.Size = new System.Drawing.Size(98, 18);
            this.YMin_label.TabIndex = 123;
            this.YMin_label.Text = "實測亮度Min?";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("微軟正黑體", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label4.Location = new System.Drawing.Point(10, 96);
            this.label4.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(121, 18);
            this.label4.TabIndex = 129;
            this.label4.Text = "Gamma容許誤差:";
            // 
            // YMax_label
            // 
            this.YMax_label.AutoSize = true;
            this.YMax_label.Font = new System.Drawing.Font("微軟正黑體", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.YMax_label.Location = new System.Drawing.Point(11, 123);
            this.YMax_label.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
            this.YMax_label.Name = "YMax_label";
            this.YMax_label.Size = new System.Drawing.Size(100, 18);
            this.YMax_label.TabIndex = 121;
            this.YMax_label.Text = "實測亮度Max?";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label3.Location = new System.Drawing.Point(10, 25);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(141, 19);
            this.label3.TabIndex = 124;
            this.label3.Text = "K80量測後平均次數";
            // 
            // GetEstimateBrightness_button
            // 
            this.GetEstimateBrightness_button.Enabled = false;
            this.GetEstimateBrightness_button.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.GetEstimateBrightness_button.Location = new System.Drawing.Point(232, 22);
            this.GetEstimateBrightness_button.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.GetEstimateBrightness_button.Name = "GetEstimateBrightness_button";
            this.GetEstimateBrightness_button.Size = new System.Drawing.Size(91, 135);
            this.GetEstimateBrightness_button.TabIndex = 122;
            this.GetEstimateBrightness_button.Text = "取得標準Gamma曲線";
            this.GetEstimateBrightness_button.UseVisualStyleBackColor = true;
            this.GetEstimateBrightness_button.Click += new System.EventHandler(this.GetEstimateBrightness_button_Click);
            // 
            // Gamma_set_tolerance_textBox
            // 
            this.Gamma_set_tolerance_textBox.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Gamma_set_tolerance_textBox.Location = new System.Drawing.Point(158, 87);
            this.Gamma_set_tolerance_textBox.Name = "Gamma_set_tolerance_textBox";
            this.Gamma_set_tolerance_textBox.Size = new System.Drawing.Size(67, 27);
            this.Gamma_set_tolerance_textBox.TabIndex = 128;
            this.Gamma_set_tolerance_textBox.Text = "0.1";
            // 
            // GammaSet_textBox
            // 
            this.GammaSet_textBox.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.GammaSet_textBox.Location = new System.Drawing.Point(158, 54);
            this.GammaSet_textBox.Name = "GammaSet_textBox";
            this.GammaSet_textBox.Size = new System.Drawing.Size(67, 27);
            this.GammaSet_textBox.TabIndex = 127;
            this.GammaSet_textBox.Text = "2.2";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("微軟正黑體", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label5.Location = new System.Drawing.Point(10, 59);
            this.label5.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(79, 18);
            this.label5.TabIndex = 126;
            this.label5.Text = "Gamma值:";
            // 
            // comboBox1
            // 
            this.comboBox1.Font = new System.Drawing.Font("微軟正黑體", 9F);
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Items.AddRange(new object[] {
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "10"});
            this.comboBox1.Location = new System.Drawing.Point(158, 22);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(67, 27);
            this.comboBox1.TabIndex = 125;
            this.comboBox1.Text = "1";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.GMDarker_checkBox);
            this.groupBox3.Controls.Add(this.RdRegisteer2Text_but);
            this.groupBox3.Controls.Add(this.GMBrighter_checkBox);
            this.groupBox3.Controls.Add(this.WrText2GammaRegister_but);
            this.groupBox3.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold);
            this.groupBox3.Location = new System.Drawing.Point(7, 274);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(493, 94);
            this.groupBox3.TabIndex = 22;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "手動控制讀寫";
            // 
            // GMDarker_checkBox
            // 
            this.GMDarker_checkBox.AutoSize = true;
            this.GMDarker_checkBox.Location = new System.Drawing.Point(355, 57);
            this.GMDarker_checkBox.Name = "GMDarker_checkBox";
            this.GMDarker_checkBox.Size = new System.Drawing.Size(131, 23);
            this.GMDarker_checkBox.TabIndex = 124;
            this.GMDarker_checkBox.Text = "Gamma趨勢▼";
            this.GMDarker_checkBox.UseVisualStyleBackColor = true;
            // 
            // RdRegisteer2Text_but
            // 
            this.RdRegisteer2Text_but.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.RdRegisteer2Text_but.Location = new System.Drawing.Point(136, 25);
            this.RdRegisteer2Text_but.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.RdRegisteer2Text_but.Name = "RdRegisteer2Text_but";
            this.RdRegisteer2Text_but.Size = new System.Drawing.Size(120, 60);
            this.RdRegisteer2Text_but.TabIndex = 119;
            this.RdRegisteer2Text_but.Text = "從IC讀出目前\r\n設定至控制盤";
            this.RdRegisteer2Text_but.UseVisualStyleBackColor = true;
            this.RdRegisteer2Text_but.Click += new System.EventHandler(this.button5_Click);
            // 
            // GMBrighter_checkBox
            // 
            this.GMBrighter_checkBox.AutoSize = true;
            this.GMBrighter_checkBox.Checked = true;
            this.GMBrighter_checkBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.GMBrighter_checkBox.Location = new System.Drawing.Point(355, 28);
            this.GMBrighter_checkBox.Name = "GMBrighter_checkBox";
            this.GMBrighter_checkBox.Size = new System.Drawing.Size(131, 23);
            this.GMBrighter_checkBox.TabIndex = 123;
            this.GMBrighter_checkBox.Text = "Gamma趨勢▲";
            this.GMBrighter_checkBox.UseVisualStyleBackColor = true;
            // 
            // WrText2GammaRegister_but
            // 
            this.WrText2GammaRegister_but.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.WrText2GammaRegister_but.Location = new System.Drawing.Point(9, 24);
            this.WrText2GammaRegister_but.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.WrText2GammaRegister_but.Name = "WrText2GammaRegister_but";
            this.WrText2GammaRegister_but.Size = new System.Drawing.Size(120, 60);
            this.WrText2GammaRegister_but.TabIndex = 118;
            this.WrText2GammaRegister_but.Text = "寫入控制盤設定值至IC";
            this.WrText2GammaRegister_but.UseVisualStyleBackColor = true;
            this.WrText2GammaRegister_but.Click += new System.EventHandler(this.button2_Click);
            // 
            // button4
            // 
            this.button4.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button4.Location = new System.Drawing.Point(973, 24);
            this.button4.Margin = new System.Windows.Forms.Padding(6, 7, 6, 7);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(120, 60);
            this.button4.TabIndex = 4;
            this.button4.Text = "TEST BUTTON A";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // Info_textBox
            // 
            this.Info_textBox.Location = new System.Drawing.Point(6, 28);
            this.Info_textBox.Multiline = true;
            this.Info_textBox.Name = "Info_textBox";
            this.Info_textBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.Info_textBox.Size = new System.Drawing.Size(481, 360);
            this.Info_textBox.TabIndex = 24;
            // 
            // groupBox6
            // 
            this.groupBox6.Controls.Add(this.Info_textBox);
            this.groupBox6.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold);
            this.groupBox6.Location = new System.Drawing.Point(7, 474);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(493, 394);
            this.groupBox6.TabIndex = 25;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "文字資訊";
            // 
            // chart1
            // 
            chartArea1.Name = "ChartArea1";
            this.chart1.ChartAreas.Add(chartArea1);
            legend1.Name = "Legend1";
            this.chart1.Legends.Add(legend1);
            this.chart1.Location = new System.Drawing.Point(509, 113);
            this.chart1.Name = "chart1";
            series1.ChartArea = "ChartArea1";
            series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            series1.Legend = "Legend1";
            series1.LegendText = "推算的標準亮度";
            series1.Name = "Series1";
            series2.ChartArea = "ChartArea1";
            series2.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            series2.Legend = "Legend1";
            series2.LegendText = "標準亮度誤差上限";
            series2.Name = "Series2";
            series3.ChartArea = "ChartArea1";
            series3.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            series3.Legend = "Legend1";
            series3.LegendText = "標準亮度誤差下限";
            series3.Name = "Series3";
            series4.ChartArea = "ChartArea1";
            series4.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            series4.Legend = "Legend1";
            series4.LegendText = "實測亮度表現";
            series4.Name = "Series4";
            this.chart1.Series.Add(series1);
            this.chart1.Series.Add(series2);
            this.chart1.Series.Add(series3);
            this.chart1.Series.Add(series4);
            this.chart1.Size = new System.Drawing.Size(742, 749);
            this.chart1.TabIndex = 29;
            this.chart1.Text = "chart1";
            // 
            // groupBox7
            // 
            this.groupBox7.Controls.Add(this.button6);
            this.groupBox7.Controls.Add(this.button7);
            this.groupBox7.Controls.Add(this.button1);
            this.groupBox7.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold);
            this.groupBox7.Location = new System.Drawing.Point(7, 374);
            this.groupBox7.Name = "groupBox7";
            this.groupBox7.Size = new System.Drawing.Size(493, 94);
            this.groupBox7.TabIndex = 30;
            this.groupBox7.TabStop = false;
            this.groupBox7.Text = "調適與驗證功能";
            // 
            // button6
            // 
            this.button6.Enabled = false;
            this.button6.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.button6.Location = new System.Drawing.Point(136, 23);
            this.button6.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(120, 60);
            this.button6.TabIndex = 121;
            this.button6.Text = "量測256階灰階\r\n並繪曲線圖";
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Click += new System.EventHandler(this.button6_Click);
            // 
            // button7
            // 
            this.button7.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.button7.Location = new System.Drawing.Point(263, 23);
            this.button7.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.button7.Name = "button7";
            this.button7.Size = new System.Drawing.Size(120, 60);
            this.button7.TabIndex = 122;
            this.button7.Text = "點漸層灰階圖\r\n並觀察表現";
            this.button7.UseVisualStyleBackColor = true;
            this.button7.Click += new System.EventHandler(this.button7_Click);
            // 
            // button1
            // 
            this.button1.Enabled = false;
            this.button1.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.button1.Location = new System.Drawing.Point(9, 23);
            this.button1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(120, 60);
            this.button1.TabIndex = 120;
            this.button1.Text = "自動調適";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // deviceTimer
            // 
            this.deviceTimer.Enabled = true;
            this.deviceTimer.Interval = 3000;
            this.deviceTimer.Tick += new System.EventHandler(this.deviceTimer_Tick);
            // 
            // gbx_usb
            // 
            this.gbx_usb.Controls.Add(this.ComPortState_label);
            this.gbx_usb.Controls.Add(this.txtbox_info);
            this.gbx_usb.Controls.Add(this.ComPortCheck_Button);
            this.gbx_usb.Controls.Add(this.ComPortSel_comboBox);
            this.gbx_usb.Controls.Add(this.label1);
            this.gbx_usb.Controls.Add(this.lbl_info);
            this.gbx_usb.Controls.Add(this.txtbox_pid);
            this.gbx_usb.Controls.Add(this.lbl_pid);
            this.gbx_usb.Controls.Add(this.txtbox_vid);
            this.gbx_usb.Controls.Add(this.lbl_vid);
            this.gbx_usb.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold);
            this.gbx_usb.Location = new System.Drawing.Point(8, 5);
            this.gbx_usb.Name = "gbx_usb";
            this.gbx_usb.Size = new System.Drawing.Size(945, 88);
            this.gbx_usb.TabIndex = 31;
            this.gbx_usb.TabStop = false;
            this.gbx_usb.Text = "STEP1: USB & K-80 Device Online";
            // 
            // txtbox_info
            // 
            this.txtbox_info.Location = new System.Drawing.Point(55, 26);
            this.txtbox_info.Name = "txtbox_info";
            this.txtbox_info.Size = new System.Drawing.Size(204, 27);
            this.txtbox_info.TabIndex = 5;
            // 
            // lbl_info
            // 
            this.lbl_info.AutoSize = true;
            this.lbl_info.Location = new System.Drawing.Point(11, 30);
            this.lbl_info.Name = "lbl_info";
            this.lbl_info.Size = new System.Drawing.Size(38, 19);
            this.lbl_info.TabIndex = 4;
            this.lbl_info.Text = "Info";
            // 
            // txtbox_pid
            // 
            this.txtbox_pid.Location = new System.Drawing.Point(458, 26);
            this.txtbox_pid.Name = "txtbox_pid";
            this.txtbox_pid.Size = new System.Drawing.Size(90, 27);
            this.txtbox_pid.TabIndex = 3;
            // 
            // lbl_pid
            // 
            this.lbl_pid.AutoSize = true;
            this.lbl_pid.Location = new System.Drawing.Point(416, 30);
            this.lbl_pid.Name = "lbl_pid";
            this.lbl_pid.Size = new System.Drawing.Size(36, 19);
            this.lbl_pid.TabIndex = 2;
            this.lbl_pid.Text = "Pid:";
            // 
            // txtbox_vid
            // 
            this.txtbox_vid.Location = new System.Drawing.Point(320, 27);
            this.txtbox_vid.Name = "txtbox_vid";
            this.txtbox_vid.Size = new System.Drawing.Size(90, 27);
            this.txtbox_vid.TabIndex = 1;
            // 
            // lbl_vid
            // 
            this.lbl_vid.AutoSize = true;
            this.lbl_vid.Location = new System.Drawing.Point(274, 31);
            this.lbl_vid.Name = "lbl_vid";
            this.lbl_vid.Size = new System.Drawing.Size(37, 19);
            this.lbl_vid.TabIndex = 0;
            this.lbl_vid.Text = "Vid:";
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.button8);
            this.groupBox5.Controls.Add(this.button3);
            this.groupBox5.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold);
            this.groupBox5.Location = new System.Drawing.Point(8, 99);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(156, 169);
            this.groupBox5.TabIndex = 32;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Load Initial Code";
            // 
            // button8
            // 
            this.button8.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.button8.Location = new System.Drawing.Point(9, 97);
            this.button8.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.button8.Name = "button8";
            this.button8.Size = new System.Drawing.Size(120, 63);
            this.button8.TabIndex = 123;
            this.button8.Text = "Try Image Fill";
            this.button8.UseVisualStyleBackColor = true;
            this.button8.Click += new System.EventHandler(this.button8_Click_1);
            // 
            // button3
            // 
            this.button3.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button3.ForeColor = System.Drawing.Color.Black;
            this.button3.Location = new System.Drawing.Point(9, 30);
            this.button3.Margin = new System.Windows.Forms.Padding(6, 7, 6, 7);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(120, 60);
            this.button3.TabIndex = 3;
            this.button3.Text = "Load Initial Code";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(1270, 920);
            this.Controls.Add(this.groupBox5);
            this.Controls.Add(this.gbx_usb);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.groupBox7);
            this.Controls.Add(this.chart1);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.gbx_openelecs);
            this.Controls.Add(this.groupBox6);
            this.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(6, 7, 6, 7);
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.Text = " ";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.gbx_openelecs.ResumeLayout(false);
            this.gbx_openelecs.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox6.ResumeLayout(false);
            this.groupBox6.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).EndInit();
            this.groupBox7.ResumeLayout(false);
            this.gbx_usb.ResumeLayout(false);
            this.gbx_usb.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox ComPortSel_comboBox;
        private System.Windows.Forms.Button ComPortCheck_Button;
        private System.Windows.Forms.Label ComPortState_label;
        private System.Windows.Forms.GroupBox gbx_openelecs;
        private System.Windows.Forms.Button btn_oepnelecs;
        private System.Windows.Forms.ComboBox cbo_elecsport;
        private System.Windows.Forms.Label lbl_elecs;
        private System.Windows.Forms.Label lbl_elecs_status;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label YMin_label;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label YMax_label;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button GetEstimateBrightness_button;
        private System.Windows.Forms.TextBox Gamma_set_tolerance_textBox;
        private System.Windows.Forms.TextBox GammaSet_textBox;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Button RdRegisteer2Text_but;
        private System.Windows.Forms.Button WrText2GammaRegister_but;
        private System.Windows.Forms.TextBox Info_textBox;
        private System.Windows.Forms.GroupBox groupBox6;
        private System.Windows.Forms.DataVisualization.Charting.Chart chart1;
        private System.Windows.Forms.GroupBox groupBox7;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Timer deviceTimer;
        private System.Windows.Forms.GroupBox gbx_usb;
        private System.Windows.Forms.TextBox txtbox_pid;
        private System.Windows.Forms.Label lbl_pid;
        private System.Windows.Forms.TextBox txtbox_vid;
        private System.Windows.Forms.Label lbl_vid;
        private System.Windows.Forms.TextBox txtbox_info;
        private System.Windows.Forms.Label lbl_info;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.Button button7;
        private System.Windows.Forms.Button button8;
        private System.Windows.Forms.CheckBox GMDarker_checkBox;
        private System.Windows.Forms.CheckBox GMBrighter_checkBox;
    }
}

