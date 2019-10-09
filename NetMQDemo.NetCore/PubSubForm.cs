using System;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using NetMQ;
using NetMQ.Sockets;

namespace NetMQDemo.NetCore
{
    public partial class PubSubForm : Form
    {
        private readonly int port;
        private readonly string address;

        private PublisherSocket publisherSocket = new PublisherSocket();
        private SubscriberSocket subscriberSocket1;
        private SubscriberSocket subscriberSocket2;

        private readonly string[] topics = new[] { "life.food", "life.weather", "fun.game", "learn.book", "work.c#" };

        public PubSubForm()
        {
            this.port = new Random().Next(5000, 65535);
            this.address = $"tcp://127.0.0.1:{this.port}";

            this.InitializeComponent();
        }

        public void LaunchSubscriber1()
        {
            if (this.subscriberSocket1 == null)
            {
                this.subscriberSocket1 = new SubscriberSocket(this.address);
                this.topics.Skip(2).Take(1).Union(new[] { "life" }).ToList().ForEach((topic) =>
                 {
                     this.AppendMessage(this.textBox2, $"订阅主题：{topic}");
                     this.subscriberSocket1.Subscribe(topic);
                 });
            }
            else
            {
                this.subscriberSocket1.Connect(this.address);
            }

            while (true)
            {
                try
                {
                    string message = this.subscriberSocket1.ReceiveFrameString();
                    this.AppendMessage(this.textBox2, $"收到消息：{message}");
                }
                catch (Exception ex)
                {
                    this.AppendMessage(this.textBox2, $"接收消息异常：{ex.Message}");
                    break;
                }
            }
        }

        public void LaunchSubscriber2()
        {
            this.subscriberSocket2 = new SubscriberSocket(this.address);
            this.topics.Skip(2).Take(3).ToList().ForEach((topic) =>
            {
                this.AppendMessage(this.textBox3, $"订阅主题：{topic}");
                this.subscriberSocket2.Subscribe(topic);
            });

            while (true)
            {
                try
                {
                    string message = this.subscriberSocket2.ReceiveFrameString();
                    this.AppendMessage(this.textBox3, $"收到消息：{message}");
                }
                catch (Exception ex)
                {
                    this.AppendMessage(this.textBox3, $"接收消息异常：{ex.Message}");
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
            this.button5.Enabled = true;

            this.publisherSocket.Bind(this.address);

            this.AppendMessage(this.textBox1, $"开始监听 {this.address}");
            this.button3.Enabled = true;
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback((x) =>
            {
                foreach (var topic in this.topics)
                {
                    this.AppendMessage(this.textBox1, $"正在发布消息：主题={topic}");
                    this.publisherSocket.SendFrame($"{topic} {DateTime.Now.Millisecond}");
                }
            }));
        }

        private void PubSubForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.subscriberSocket1 != null)
            {
                this.subscriberSocket1.Disconnect(this.address);
                this.subscriberSocket1.Close();
                this.subscriberSocket1.Dispose();
            }

            if (this.publisherSocket != null)
            {
                this.publisherSocket.Unbind(this.address);
                this.publisherSocket.Close();
                this.publisherSocket.Dispose();
            }
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            this.button2.Enabled = false;
            this.button6.Enabled = true;
            this.button2.Text = this.address;
            ThreadPool.QueueUserWorkItem(new WaitCallback((x) => this.LaunchSubscriber1()));
            this.AppendMessage(this.textBox2, $"已经连接 {this.address}");
        }

        private void Button4_Click(object sender, EventArgs e)
        {
            this.button4.Enabled = false;
            this.button4.Text = this.address;
            ThreadPool.QueueUserWorkItem(new WaitCallback((x) => this.LaunchSubscriber2()));
            this.AppendMessage(this.textBox3, $"已经连接 {this.address}");
        }

        private void Button5_Click(object sender, EventArgs e)
        {
            this.button5.Enabled = false;
            this.button1.Enabled = true;

            /* Close() 方法会自动 Dispose()，继续 Send() 将会报错；下次使用应 new 新的对象；期间订阅者不用进行任何操作，也不用重新连接发布者；
             * Disconnect() 方法仅仅断开连接，继续 Send() 不会报错；下次使用重新 Bind() 即可；期间订阅者不用进行任何操作，也不用重新连接发布者；
             */
            this.publisherSocket.Disconnect(this.address);
        }

        private void Button6_Click(object sender, EventArgs e)
        {
            this.button6.Enabled = false;
            this.button2.Enabled = true;

            // 订阅者可以随时断开连接并重新连接
            this.subscriberSocket1.Disconnect(this.address);
        }
    }
}
