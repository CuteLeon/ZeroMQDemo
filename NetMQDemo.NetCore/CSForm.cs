using System;
using System.Threading;
using System.Windows.Forms;
using NetMQ;
using NetMQ.Sockets;

namespace NetMQDemo.NetCore
{
    public partial class CSForm : Form
    {
        private readonly int port;
        private readonly string address;

        private ResponseSocket serverSocket;
        private RequestSocket clientSocket;

        public CSForm()
        {
            this.port = new Random().Next(5000, 65535);
            this.address = $"tcp://127.0.0.1:{this.port}";

            this.InitializeComponent();
        }

        public void LaunchServer()
        {
            this.serverSocket = new ResponseSocket(this.address);

            while (true)
            {
                try
                {
                    string message = this.serverSocket.ReceiveFrameString();
                    this.AppendMessage(this.textBox1, $"服务端收到消息：{message}");
                    this.serverSocket.SendFrame($"确认回执：{message.GetHashCode().ToString("X")}");
                }
                catch (Exception ex)
                {
                    this.AppendMessage(this.textBox1, $"服务端接收消息遇到异常：{ex.Message}");
                    break;
                }
            }
        }

        public void LaunchClient()
        {
            this.clientSocket = new RequestSocket(this.address);
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
                try
                {
                    this.clientSocket.SendFrame(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                    string message = this.clientSocket.ReceiveFrameString();
                    this.AppendMessage(this.textBox2, $"客户端收到消息：{message}");
                }
                catch (Exception ex)
                {
                    this.AppendMessage(this.textBox2, $"客户端发送消息失败：{ex.Message}");
                }
            }));
        }

        private void CSForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.clientSocket != null)
            {
                this.clientSocket.Disconnect(this.address);
                this.clientSocket.Close();
                this.clientSocket.Dispose();
            }

            if (this.serverSocket != null)
            {
                this.serverSocket.Unbind(this.address);
                this.serverSocket.Close();
                this.serverSocket.Dispose();
            }
        }
    }
}
