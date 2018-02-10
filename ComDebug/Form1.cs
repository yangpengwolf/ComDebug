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
using System.Threading;


namespace ComDebug
{
    public partial class Form1 : Form
    {
        public Form1()
        {

            sp.DataReceived += new SerialDataReceivedEventHandler(sp_DataReceived);  
  
            InitializeComponent();
        }
        SerialPort sp = new SerialPort();//实例化串口通讯类
        private void Form1_Load(object sender, EventArgs e)
        {
            this.btn_close.Enabled = false;
            this.btn_send.Enabled = false;

            string[] ports = SerialPort.GetPortNames();

           

            //波特率
            cmbBaudRate.Items.Add("1200");
            cmbBaudRate.Items.Add("2400");
            cmbBaudRate.Items.Add("4800");
            cmbBaudRate.Items.Add("9600");
            cmbBaudRate.Items.Add("19200");
            cmbBaudRate.Items.Add("38400");
            cmbBaudRate.Items.Add("57600");
            cmbBaudRate.Items.Add("115200");
            cmbBaudRate.Items.Add("230400");
        
            cmbBaudRate.SelectedIndex = 3;

            //数据位
            cmbDataBits.Items.Add("5");
            cmbDataBits.Items.Add("6");
            cmbDataBits.Items.Add("7");
            cmbDataBits.Items.Add("8");
            cmbDataBits.SelectedIndex = 3;

            //停止位
            cmbStopBit.Items.Add("1");
            cmbStopBit.SelectedIndex = 0;

            //佼验位
            cmbParity.Items.Add("无");
            cmbParity.SelectedIndex = 0;

            foreach (string port in ports)
            {
                cmbPort.Items.Add(port);
            }
            cmbPort.SelectedIndex = 0;
           

        }

        private void btn_open_Click(object sender, EventArgs e)
        {
            if (cmbPort.SelectedIndex == -1) { MessageBox.Show("COM口不能为空，不要逗我好吗，请检查COM口"); return; } 
            sp.PortName = cmbPort.Text;//串口号
            sp.BaudRate = int.Parse(cmbBaudRate.Text);//波特率
            sp.DataBits = int.Parse(cmbDataBits.Text);//数据位
            sp.StopBits = (StopBits)int.Parse(cmbStopBit.Text);//停止位
            sp.ReadTimeout = 500;//读取数据的超时时间，引发ReadExisting异常
            try
            {
                if (sp.IsOpen)
                {
                    sp.Close();
                    sp.Open();//打开串口
                }
                else
                {
                    sp.Open();//打开串口
                }
                this.btn_open.Enabled = false;
                this.btn_close.Enabled = true;
                this.btn_send.Enabled = true;
            }
            catch (Exception ex)
            {
                //捕获到异常信息，创建一个新的comm对象，之前的不能用了。   
                sp = new SerialPort();
                //将异常信息传递给用户。   
                MessageBox.Show(ex.Message);
            }       
        }
    
        private void btn_send_Click(object sender, EventArgs e)
        {
            if (btn_send.Text == "开始盘点")
            {
                this.SendTimer.Enabled = true;
                btn_send.Text = "停止";
                res = "";
            }
            else 
            {
                this.SendTimer.Enabled = false;
                btn_send.Text = "开始盘点";
            } 
        }

        private void btn_close_Click(object sender, EventArgs e)
        {
            if (sp.IsOpen)
                sp.Close();
            this.btn_open.Enabled = true;
            this.btn_close.Enabled = false;
            this.btn_send.Enabled = false;
        }



        string res = "";
      
        private void sp_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            //使用委托进行跨线程读取数据
           // this.Invoke(disp_delegate, InputBuf); 
            Invoke
             (new EventHandler
               (delegate
               {
                   //Byte[] OutputBuf = new Byte[2];
                  // sp.Read(OutputBuf, 0, 1);
                  // this.rtx_rec.Text = OutputBuf[0].ToString();
                   this.L01.BackColor = Color.Red;
                   this.L02.BackColor = Color.Red;
                   this.L03.BackColor = Color.Red;
                   this.L04.BackColor = Color.Red;
                   StringBuilder sb = new StringBuilder(); 
                   //sb.Replace(res, res);
                   sb.Append(sp.ReadExisting());
                   res = sb.ToString(); 
                   if (res.Contains("a")) this.L01.BackColor = Color.Green;
                   if (res.Contains("b")) this.L02.BackColor = Color.Green;
                   if (res.Contains("c")) this.L03.BackColor = Color.Green;
                   if (res.Contains("d")) this.L04.BackColor = Color.Green;
                   
                   this.rtx_rec.Text = res;
               }
               )
              );
        }

        void send_data() 
        {
            byte[] b = { 0x0, 0x01, 0x17, 0x61 };
            for(int i=0;i<4;i++)
            {
                b[1] = Convert.ToByte(i+0x01);
                b[3] = Convert.ToByte(i +0x61);
                sp.Write(b, 0, b.Length);
                Thread.Sleep(500);
            } 

        }

        private void SendTimer_Tick(object sender, EventArgs e)
        {
            if (sp.IsOpen)
            {
                try
                {

                    send_data();
                   
                    //byte[] b = { 0x0, 0x02, 0x17, 0x62};
                    //sp.Write(b, 0, b.Length);//发送数据  
                   
                }
                catch (Exception ex)
                {
                    MessageBox.Show("错误：" + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("请先打开串口！");
            }
        }

    }
}
