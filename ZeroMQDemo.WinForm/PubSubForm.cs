using System;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

using ZeroMQ;

namespace ZeroMQDemo.WinForm
{
    public partial class PubSubForm : Form
    {
        private readonly int port;
        private readonly string address;

        private ZSocket publisherSocket;
        private ZSocket subscriberSocket;

        private string[] topics = new[] { "life.food", "life.weather", "fun.game", "learn.book", "work.c#" };

        public PubSubForm()
        {
            this.port = new Random().Next(5000, 65535);
            this.address = $"tcp://127.0.0.1:{this.port}";

            this.InitializeComponent();
        }

        public void LaunchPublisher()
        {
            this.publisherSocket = new ZSocket(ZSocketType.PUB);
            this.publisherSocket.Bind(this.address);
        }

        public void LaunchSubscriber()
        {
            this.subscriberSocket = new ZSocket(ZSocketType.SUB);

            this.subscriberSocket.Connect(this.address);
            this.topics.Skip(2).Take(1).Union(new[] { "life" }).ToList().ForEach((topic) =>
             {
                 this.AppendMessage(this.textBox2, $"订阅主题：{topic}");
                 this.subscriberSocket.Subscribe(topic);
             });

            while (true)
            {
                try
                {
                    using (var response = this.subscriberSocket.ReceiveFrame())
                    {
                        string message = response.ReadString();
                        this.AppendMessage(this.textBox2, $"收到消息：{message}");
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
            this.button1.Text = this.address;
            ThreadPool.QueueUserWorkItem(new WaitCallback((x) => this.LaunchPublisher()));
            this.AppendMessage(this.textBox1, $"开始监听 {this.address}");
            this.button3.Enabled = true;
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            this.button2.Enabled = false;
            this.button2.Text = this.address;
            ThreadPool.QueueUserWorkItem(new WaitCallback((x) => this.LaunchSubscriber()));
            this.AppendMessage(this.textBox2, $"已经连接 {this.address}");
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback((x) =>
            {
                foreach (var topic in this.topics)
                {
                    this.AppendMessage(this.textBox1, $"正在发布消息：主题={topic}");
                    this.publisherSocket.Send(new ZFrame($"{topic} {DateTime.Now.Millisecond}"));
                }
            }));
        }

        private void CSForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.subscriberSocket.Disconnect(this.address);
            this.subscriberSocket.Close();
            this.subscriberSocket.Dispose();
            this.publisherSocket.Unbind(this.address);
            this.publisherSocket.Close();
            this.publisherSocket.Dispose();
        }
    }
}
