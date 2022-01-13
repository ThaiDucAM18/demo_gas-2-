// supports by Thai MInh Duc AM18
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.IO.Ports;
using System.IO;
using System.Xml;
using System.Runtime.InteropServices;
using ZedGraph;

namespace demo_gas
{
    public partial class Form1 : Form
    {
        private DateTime datetime;
        private string in_data;
        string recievedata = string.Empty;
        string sentdata = string.Empty;

        float value_ghd = 1500; // muc bat den
        float value_ghq = 3000; // set muc bat quat
        float read_value;
        double sensor_volt, RS_gas;
        double ratio;
        string control = "";

        double Regulatory = 0; // bien hỗ trợ quay quat them 10s sau khi tu giới hạn nguy hiểm xuống mức giới hạn dưới  nó
        double count_time=0;
        int a;
        int time; // khai bao bien dung timer, chay cot thoi gian bang ms 
        public Form1()
        {
            InitializeComponent();
           // ModifyProgressBarColor.SetState(verticalProgressBar1, 1);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if ((comboBox1_port.Text == "") || (comboBox1.Text == ""))
            {
                MessageBox.Show("please, select COM port", "warring", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                try
                {
                    if (serialPort1.IsOpen)
                    {
                        MessageBox.Show("COM port is ready to use", "notice", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else
                    {
                        serialPort1.Open();
                        textbox1.BackColor = Color.Green;
                        textbox1.Text = "Connecting...";
                        comboBox1_port.Enabled = false;
                        progressBar1.Value = 100;
                        //timer3.Start();
                        comboBox1.Enabled = false;

                        control = "Connecting";

                    }
                    // choi t anh vip vậy, thái đó hâ
                }
                catch (Exception)
                {
                    MessageBox.Show("COM port is not found, please check your COM port", "warrring", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            GraphPane mypae = zedGraphControl1.GraphPane;
            mypae.Title.Text = "ĐỒ THỊ ";
            mypae.XAxis.Title.Text = "thời gian (s)";
            mypae.YAxis.Title.Text = "giá trị gas";


            RollingPointPairList list_1 = new RollingPointPairList(60000);
            RollingPointPairList list_2 = new RollingPointPairList(60000);
            RollingPointPairList list_3 = new RollingPointPairList(60000);

            LineItem cur = mypae.AddCurve("giá trị", list_1, Color.Blue, SymbolType.None);
            LineItem cur1 = mypae.AddCurve("upper", list_2, Color.Red, SymbolType.None);
            LineItem cur2 = mypae.AddCurve("lower", list_3, Color.Yellow, SymbolType.None);

            // ok chua ok còn cáiinao nũa // cai auto voi manual a haha
            mypae.YAxis.Scale.Min = 0;
            mypae.YAxis.Scale.Max = 20000; ; // do phan giai vay duoc chua
            // mà lm lol j phan giai cao vay 10000 à pro 
            mypae.YAxis.Scale.MinorStep = 1; // xét bước nhảy
            mypae.YAxis.Scale.MajorStep = 5;

            zedGraphControl1.AxisChange();  // hàm xác định trục



            string[] ports = SerialPort.GetPortNames();
            foreach (string port in ports)
            {
                comboBox1_port.Items.Add(port);
            }
        }
        int tong = 0;
        public void draw(double line1)
        {
            LineItem cur = zedGraphControl1.GraphPane.CurveList[0] as LineItem;
            LineItem cur2 = zedGraphControl1.GraphPane.CurveList[1] as LineItem;
            LineItem cur3 = zedGraphControl1.GraphPane.CurveList[2] as LineItem;
            if (cur == null)
            {
                return;
            }
            IPointListEdit list_1 = cur.Points as IPointListEdit;
            IPointListEdit list_2 = cur2.Points as IPointListEdit;
            IPointListEdit list_3 = cur3.Points as IPointListEdit;
            if (list_1 == null)
            {
                return;
            }
            list_1.Add(tong, line1);
            if (a != 0)
            {
                if (list_2 == null)
                {
                    return;
                }
                list_2.Add(tong, value_ghd);
                if (list_3 == null)
                {
                    return;
                }
                list_3.Add(tong, value_ghq);
            }

            zedGraphControl1.AxisChange();
            zedGraphControl1.Invalidate();
            zedGraphControl1.Refresh();
            tong += 1;

        }
        public void draw_zed(double line2)
        {
            LineItem cur1 = zedGraphControl1.GraphPane.CurveList[0] as LineItem;
            if (cur1 == null)
            {
                return;
            }
            IPointListEdit list2 = cur1.Points as IPointListEdit;
            if (list2 == null)
            {
                return;
            }
            list2.Add(double.Parse(text_ghq.Text), line2);

            zedGraphControl1.AxisChange();
            zedGraphControl1.Invalidate();
            zedGraphControl1.Refresh();

        }
        private void comboBox1_port_SelectedIndexChanged(object sender, EventArgs e)
        {
            serialPort1.PortName = comboBox1_port.Text;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            serialPort1.Close();
            textbox1.BackColor = Color.Red;
            textbox1.Text = "Disconnected";
            comboBox1_port.Enabled = true;
            progressBar1.Value = 0;
            comboBox1.Enabled = true;

            control = "Disconnected";
        }

        private void btn_ghq_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                try
                {
                    if ((double.Parse(text_ghq.Text) >= 300) && ((double.Parse(text_ghq.Text) <= 10000)) && ((double.Parse(text_ghq.Text) > value_ghd)) && (text_ghq.Text.Length <= 4))
                    {
                        value_ghq = float.Parse(text_ghq.Text);
                        serialPort1.Write("#SEQ" + text_ghq.Text + "\n");

                        control = "set limit fan=" + text_ghq.ToString();
                    }
                    else
                    {
                        MessageBox.Show("gia tri vuot gia pham vi set", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        text_ghq.Text = "";
                    }
                }
                catch
                {
                    MessageBox.Show("vui long nhap gia tri thuc", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    text_ghq.Text = "";
                }
            }
        }

        private void btn_ghd_Click(object sender, EventArgs e)
        {

            if (serialPort1.IsOpen)
            {
                try
                {
                    if ((double.Parse(text_ghd.Text) >= 300) && ((double.Parse(text_ghd.Text) <= 10000)) && ((double.Parse(text_ghd.Text) < value_ghq)))
                    {
                        value_ghd = float.Parse(text_ghd.Text);
                        serialPort1.Write("#SED" + text_ghd.Text + "\n");

                        control = "set limet light=" + text_ghd.ToString();
                    }
                    else
                    {
                        MessageBox.Show("gia tri vuot gia pham vi set", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        text_ghd.Text = "";
                    }
                }
                catch
                {
                    MessageBox.Show("vui long nhap gia tri thuc", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    text_ghd.Text = "";
                }
            }
        }

        private void button_stop_Click(object sender, EventArgs e)
        {
            try
            {
                serialPort1.Close();

            }
            catch (Exception ex2)
            {
                MessageBox.Show(ex2.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }
        private void Data_Listview()
        {
            if (serialPort1.IsOpen)
            {
                ListViewItem item = new ListViewItem(label12.Text); // Gán biến realtime vào cột đầu tiên của ListView
                item.SubItems.Add(ate.ToString());
                item.SubItems.Add(control);
                listView1.Items.Add(item); // Gán biến datas vào cột tiếp theo của ListView
                                           // Không nên gán string SDatas vì khi xuất dữ liệu sang Excel sẽ là dạng string, không thực hiện các phép toán được

                listView1.Items[listView1.Items.Count - 1].EnsureVisible(); // Hiện thị dòng được gán gần nhất ở ListView, tức là mình cuộn ListView theo dữ liệu gần nhất đó
            }
        }

        private void button_save_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {

                DialogResult traloi; ;
                traloi = MessageBox.Show("Bạn có muốn lưu số liệu?", "Lưu", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                if (traloi == DialogResult.OK)
                {
                    //SaveToExcel(); // Thực thi hàm lưu ListView sang Excel
                }
            }
        }
        private void bnt3_Click(object sender, EventArgs e)
        {
            try
            {
                if (serialPort1.IsOpen)
                {
                    serialPort1.Write("#FANON" + "\n");
                    pic_fan.Visible = true;
                    pic_fan_of.Visible = false;
                    control = "FAN ON ";
                }
                else
                {
                    MessageBox.Show("COM port is ready to use", "notice", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception)
            {
                MessageBox.Show("COM port isn't found", "error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

        }

        private void bnt4_Click(object sender, EventArgs e)
        {
            try
            {
                if (serialPort1.IsOpen)
                {
                    serialPort1.Write("#FANOF" + "\n");
                    pic_fan.Visible = false;
                    pic_fan_of.Visible = true;
                }
                else
                {
                    MessageBox.Show("COM port is ready to use", "notice", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception)
            {
                MessageBox.Show("COM port isn't found", "error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void bnt6_Click(object sender, EventArgs e)
        {
            try
            {
                if (serialPort1.IsOpen)
                {
                    serialPort1.Write("#COION" + "\n");
                    pic_coi.Visible = true;
                    pic_alarm_off.Visible = false;
                }
                else
                {
                    MessageBox.Show("COM port is ready to use", "notice", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception)
            {
                MessageBox.Show("COM port isn't found", "error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void bnt7_Click(object sender, EventArgs e)
        {
            try
            {
                if (serialPort1.IsOpen)
                {

                    serialPort1.Write("#COIOF" + "\n");
                    pic_coi.Visible = false;
                    pic_alarm_off.Visible = true;

                }
                else
                {
                    MessageBox.Show("COM port is ready to use", "notice", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception)
            {
                MessageBox.Show("COM port isn't found", "error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        // cái này excel của tui ban k phù hợp nên k add nha mấy ppro sửa lai j30s nha
        /*
         private void SaveToExcel()s
             {
                 Microsoft.Office.Interop.Excel.Application xla = new Microsoft.Office.Interop.Excel.Application();
                 xla.Visible = true;
                 Microsoft.Office.Interop.Excel.Workbook wb = xla.Workbooks.Add(Microsoft.Office.Interop.Excel.XlSheetType.xlWorksheet);
                 Microsoft.Office.Interop.Excel.Worksheet ws = (Microsoft.Office.Interop.Excel.Worksheet)xla.ActiveSheet;

                 // Đặt tên cho hai ô A1. B1 lần lượt là "Thời gian (s)" và "Dữ liệu", sau đó tự động dãn độ rộng
                 Microsoft.Office.Interop.Excel.Range rg = (Microsoft.Office.Interop.Excel.Range)ws.get_Range("A1", "B1");
                 ws.Cells[1, 1] = "Thời gian (s)";
                 ws.Cells[1, 2] = "Dữ liệu         ";
             ws.Cells[1, 3] = " control                 ";  // thêm vao muc con trol nha mấy pro
                 rg.Columns.AutoFit();

                 // Lưu từ ô đầu tiên của dòng thứ 2, tức ô A2
                 int i = 2;
                 int j = 1;

                 foreach (ListViewItem comp in listView1.Items)
                 {
                     ws.Cells[i, j] = comp.Text.ToString();
                     foreach (ListViewItem.ListViewSubItem drv in comp.SubItems)
                     {
                         ws.Cells[i, j] = drv.Text.ToString();
                         j++;
                     }
                     j = 1;
                     i++;
                 }
             }
         */
        double ate;

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Start();
            label_ghd.Text = value_ghd.ToString();
            label_ghq.Text = value_ghq.ToString();
            if (serialPort1.IsOpen)
            {
                if (ate != 0)
                {
                    draw(ate);
                    Data_Listview();
                }
            }
            else
            {

            }
            // ham xu li chuoi gui ve dkhien quat va den
            if (ate != 0)
            {
                if (ate >= value_ghd && ate < value_ghq)
                {
                    timer3.Enabled = true;

                    group_control_coi.Enabled = false;
                    ModifyProgressBarColor.SetState(verticalProgressBar1, 3);
                }
                if (ate < value_ghd)
                {
                    timer4.Enabled = true;
                    ModifyProgressBarColor.SetState(verticalProgressBar1, 1); // mau cho thanh progress bar nha
                    group_control_coi.Enabled = true;
                    group_control_quat.Enabled = true;
                }
                if (ate < value_ghq)
                {
                    timer5.Enabled = true;
                }
                if (ate >= value_ghq)
                {
                    group_control_coi.Enabled = false;
                    group_control_quat.Enabled = false;

                    ModifyProgressBarColor.SetState(verticalProgressBar1, 2);
                }
            }

        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            timer2.Start();
            label12.Text = DateTime.Now.ToLongTimeString();
            label13.Text = DateTime.Now.ToLongDateString();

            // ham thuc thi 10s cho ham dieu hoa quat
            if (ate != 0)
            {
                richTextBox2.Text = read_value.ToString() + " PPM";
                richTextBox1.Text = ratio.ToString();
                verticalProgressBar1.Maximum = 10000;
                verticalProgressBar1.Step = 1;
                verticalProgressBar1.Value = a;
                //ham xu lí quat chay them 10s
                if (ate < value_ghq && Regulatory == 1)
                {
                    count_time++;
                    if (count_time >= 10)
                    {
                        serialPort1.Write("#FANOF" + "\n");
                        group_control_quat.Enabled = true;
                        pic_fan.Visible = false;
                        pic_fan_of.Visible = true;
                        Regulatory = 0;
                        count_time = 0;
                    }
                }
            }
        }

        private void timer3_Tick(object sender, EventArgs e)
        {
            timer3.Start();
            if (ate != 0)
            {
                if (ate < value_ghd)
                {
                    serialPort1.Write("#STO" + "\n");
                    timer3.Enabled = false;

                    pic_coi.Visible = false;
                    pic_fan.Visible = false;
                    pic_alarm_off.Visible = true;
                    pic_fan_of.Visible = true;
                }
                else { };
            }
        }

        private void timer4_Tick(object sender, EventArgs e)
        {
            timer4.Start();
            if (ate != 0)
            {
                if (ate >= value_ghd && ate < value_ghq)
                {
                    serialPort1.Write("#ERR" + "\n");
                    timer4.Enabled = false;

                    pic_coi.Visible = true;
                    pic_alarm_off.Visible = false;
                    MessageBox.Show("thong bao", "muc gas dang ro ri"); // rooi cai nay muon tao thong bao j thi tao
                }
            }
        }

        private void timer5_Tick(object sender, EventArgs e)
        {
            timer5.Start();
            if (ate != 0)
            {
                if (ate >= value_ghq)
                {
                    Regulatory = 1;
                    serialPort1.Write("#WAR" + "\n");
                    timer5.Enabled = false;

                    pic_coi.Visible = true;
                    pic_fan.Visible = true;
                    pic_alarm_off.Visible = false;
                    pic_fan_of.Visible = false;
                    MessageBox.Show("thong bao", "muc gas nguy hiem khan cap"); // rooi cai nay muon tao thong bao j thi tao
                }
            }
        }

        private void serialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            in_data = serialPort1.ReadLine();
            float.TryParse(in_data, out read_value);
            ate = Math.Round(read_value, 2);
            float x = float.Parse(in_data);
            int data_value = (int)x;
            a = data_value;

            double analogread =( read_value * 1024 )/ 10000;

            sensor_volt = (analogread * 5.00 )/ 1024 ;
            RS_gas = (5.0-sensor_volt)/sensor_volt;

            ratio = Math.Round( RS_gas / 3.5, 1);   
            // this.Invoke(new EventHandler(displaydata_event));
        }
    }
}
