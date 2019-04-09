using System;
using System.Threading;
using System.Windows.Forms;

using ZeroMQ;

namespace ZeroMQDemo.WinForm
{
    public partial class CSForm : Form
    {
        private readonly int port;
        private readonly string address;

        private ZSocket serverSocket;
        private ZSocket clientSocket;

        public CSForm()
        {
            this.port = new Random().Next(5000, 65535);
            this.address = $"tcp://127.0.0.1:{this.port}";

            this.InitializeComponent();
        }

        public void LaunchServer()
        {
            using (ZContext context = new ZContext())
            {
                using (this.serverSocket = new ZSocket(context, ZSocketType.REP))
                {
                    this.serverSocket.Bind(this.address);

                    string message = string.Empty;
                    while (true)
                    {
                        try
                        {
                            using (ZFrame request = this.serverSocket.ReceiveFrame())
                            {
                                message = request.ReadString();
                                this.AppendMessage(this.textBox1, $"服务端收到消息：{message}");
                                this.serverSocket.Send(new ZFrame($"确认回执：{message.GetHashCode().ToString("X")}"));
                            }
                        }
                        catch
                        {
                            break;
                        }
                    }
                }
            }
        }

        public void LaunchClient()
        {
            this.clientSocket = new ZSocket(ZSocketType.REQ);
            this.clientSocket.Connect(this.address);
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
            this.button1.Text = this.address;
            ThreadPool.QueueUserWorkItem(new WaitCallback((x) => this.LaunchServer()));
            this.AppendMessage(this.textBox1, $"开始监听 {this.address}");
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            this.button2.Enabled = false;
            this.button2.Text = this.address;
            ThreadPool.QueueUserWorkItem(new WaitCallback((x) => this.LaunchClient()));
            this.AppendMessage(this.textBox2, $"已经连接 {this.address}");
            this.button3.Enabled = true;
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback((x) =>
            {
                this.clientSocket.Send(new ZFrame(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")));
                using (ZFrame response = this.clientSocket.ReceiveFrame())
                {
                    this.AppendMessage(this.textBox2, $"客户端收到消息：{response.ReadString()}");
                }
            }));
        }

        private void CSForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.clientSocket.Disconnect(this.address);
            this.clientSocket.Close();
            this.clientSocket.Dispose();

            this.serverSocket.Unbind(this.address);
            this.serverSocket.Close();
            this.serverSocket.Dispose();
        }
    }
}
