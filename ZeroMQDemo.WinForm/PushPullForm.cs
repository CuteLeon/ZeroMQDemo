using System;
using System.Threading;
using System.Windows.Forms;

using ZeroMQ;

namespace ZeroMQDemo.WinForm
{
    // Ventilator
    // Worker
    // Sink

    public partial class PushPullForm : Form
    {
        private readonly int ventilatorPort;
        private readonly string ventilatorAddress;

        private readonly int workerPort;
        private readonly string workerAddress;

        private ZSocket ventilatorSocket;
        private ZContext workerContext;
        private ZSocket workerReceiveSocket;
        private ZSocket workerSendSocket;
        private ZSocket sinkSocket;

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
            this.ventilatorSocket = new ZSocket(ZSocketType.PUSH);
            this.ventilatorSocket.Bind(this.ventilatorAddress);
        }

        public void LaunchWorker()
        {
            this.workerContext = new ZContext();
            this.workerReceiveSocket = new ZSocket(this.workerContext, ZSocketType.PULL);
            this.workerReceiveSocket.Connect(this.ventilatorAddress);
            this.workerSendSocket = new ZSocket(this.workerContext, ZSocketType.PUSH);
            this.workerSendSocket.Bind(this.workerAddress);

            while (true)
            {
                try
                {
                    using (var response = this.workerReceiveSocket.ReceiveFrame())
                    {
                        string message = response.ReadString();
                        this.AppendMessage(this.textBox2, $"收到消息：{message}");
                        this.AppendMessage(this.textBox2, $"继续下发：{message}");
                        this.workerSendSocket.Send(new ZFrame(message));
                    }
                }
                catch
                {
                    break;
                }
            }
        }

        public void LaunchSink()
        {
            this.sinkSocket = new ZSocket(ZSocketType.PULL);
            this.sinkSocket.Connect(this.workerAddress);

            while (true)
            {
                try
                {
                    using (var response = this.sinkSocket.ReceiveFrame())
                    {
                        string message = response.ReadString();
                        this.AppendMessage(this.textBox3, $"收到消息：{message}");
                    }
                }
                catch
                {
                    break;
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
                    this.ventilatorSocket.Send(new ZFrame($"发布任务-{index}"));
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

            this.workerContext?.Dispose();
        }
    }
}
