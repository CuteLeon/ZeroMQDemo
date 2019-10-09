using System;
using System.Threading;
using System.Windows.Forms;
using NetMQ;
using NetMQ.Sockets;

namespace NetMQDemo.NetCore
{
    public partial class PushPullForm : Form
    {
        private readonly int ventilatorPort;
        private readonly string ventilatorAddress;

        private readonly int workerPort;
        private readonly string workerAddress;

        private PushSocket ventilatorSocket;
        private PullSocket workerReceiveSocket;
        private PushSocket workerSendSocket;
        private PullSocket sinkSocket;

        public PushPullForm()
        {
            this.ventilatorPort = new Random().Next(5000, 35000);
            this.ventilatorAddress = $"tcp://127.0.0.1:{this.ventilatorPort}";
            this.workerPort = new Random().Next(35001, 65535);
            this.workerAddress = $"tcp://127.0.0.1:{this.workerPort}";

            this.InitializeComponent();
        }

        public void LaunchVentilator()
        {
            this.ventilatorSocket = new PushSocket(this.ventilatorAddress);
        }

        public void LaunchWorker()
        {
            this.workerReceiveSocket = new PullSocket(this.ventilatorAddress);
            this.workerSendSocket = new PushSocket(this.workerAddress);

            while (true)
            {
                try
                {
                    string message = this.workerReceiveSocket.ReceiveFrameString();
                    this.AppendMessage(this.textBox2, $"收到消息：{message}");
                    this.AppendMessage(this.textBox2, $"继续下发：{message}");
                    this.workerSendSocket.SendFrame(message);
                }
                catch (Exception ex)
                {
                    this.AppendMessage(this.textBox2, $"接收并转发异常：{ex.Message}");
                    break;
                }
            }
        }

        public void LaunchSink()
        {
            this.sinkSocket = new PullSocket(this.workerAddress);

            while (true)
            {
                try
                {
                    string message = this.sinkSocket.ReceiveFrameString();
                    this.AppendMessage(this.textBox3, $"收到消息：{message}");
                }
                catch (Exception ex)
                {
                    this.AppendMessage(this.textBox3, $"接收消息异常：{ex.Message}");
                }
            }
        }

        private void AppendMessage(TextBox textBox, string message)
        {
            void append()
            {
                if (textBox.Text.Length > 1024)
                {
                    textBox.Clear();
                }
                textBox.AppendText($"{message}\n");
                textBox.SelectionStart = textBox.Text.Length;
            }

            if (textBox.InvokeRequired)
            {
                textBox.Invoke(new Action(() =>
                {
                    append();
                }));
            }
            else
            {
                append();
            }
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            this.button1.Enabled = false;
            this.button1.Text = this.ventilatorAddress;
            ThreadPool.QueueUserWorkItem(new WaitCallback((x) => this.LaunchVentilator()));
            this.AppendMessage(this.textBox1, $"开始监听 {this.ventilatorAddress}");
            this.button3.Enabled = true;
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            this.button2.Enabled = false;
            this.button2.Text = this.ventilatorAddress;
            ThreadPool.QueueUserWorkItem(new WaitCallback((x) => this.LaunchWorker()));
            this.AppendMessage(this.textBox2, $"已经连接 {this.ventilatorAddress}");
            this.AppendMessage(this.textBox2, $"开始监听 {this.workerAddress}");
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback((x) =>
            {
                for (int index = 0; index < 5; index++)
                {
                    this.AppendMessage(this.textBox1, $"发布任务：{index}");
                    this.ventilatorSocket.SendFrame($"发布任务-{index}");
                }
            }));
        }

        private void Button4_Click(object sender, EventArgs e)
        {
            this.button4.Enabled = false;
            this.button4.Text = this.workerAddress;
            ThreadPool.QueueUserWorkItem(new WaitCallback((x) => this.LaunchSink()));
            this.AppendMessage(this.textBox3, $"已经连接 {this.workerAddress}");
        }

        private void PushPullForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.sinkSocket != null)
            {
                this.sinkSocket.Disconnect(this.workerAddress);
                this.sinkSocket.Close();
                this.sinkSocket.Dispose();
            }

            if (this.workerSendSocket != null)
            {
                this.workerSendSocket.Unbind(this.workerAddress);
                this.workerSendSocket.Close();
                this.workerSendSocket.Dispose();
            }

            if (this.workerReceiveSocket != null)
            {
                this.workerReceiveSocket.Disconnect(this.ventilatorAddress);
                this.workerReceiveSocket.Close();
                this.workerReceiveSocket.Dispose();
            }

            if (this.ventilatorSocket != null)
            {
                this.ventilatorSocket.Unbind(this.ventilatorAddress);
                this.ventilatorSocket.Close();
                this.ventilatorSocket.Dispose();
            }
        }
    }
}
