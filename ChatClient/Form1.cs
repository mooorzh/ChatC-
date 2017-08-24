using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatClient
{
    public partial class Form1 : Form
    {        
        NetworkStream stream;
        TcpClient client;
        public Form1()
        {
            InitializeComponent();
        }
        void stateChange(int state)
        {
            if (state == 1)
            {
                textBox1.Enabled = true;
                textBox2.Enabled = true;
                disconnectToolStripMenuItem.Enabled = true;
                connectToolStripMenuItem.Enabled = false;
                button1.Enabled = true;
            }
            else
            {
                textBox1.Enabled = false;
                textBox2.Enabled = false;
                disconnectToolStripMenuItem.Enabled = false;
                connectToolStripMenuItem.Enabled = true;
                button1.Enabled = false;
            }
        }

        private void connectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            client = new TcpClient();
            try
            {
                client.Connect(Properties.Settings.Default.ip, Properties.Settings.Default.port); //подключение клиента
                stream = client.GetStream(); // получаем поток
                stateChange(1);
                string message = Properties.Settings.Default.userName;
                byte[] data = Encoding.Unicode.GetBytes(message);
                stream.Write(data, 0, data.Length);

                // запускаем новый поток для получения данных
                Thread receiveThread = new Thread(new ThreadStart(ReceiveMessage));
                receiveThread.Start(); //старт потока         
            }
            catch (Exception ex)
            {
                AddText(ex.Message);
            }
            
        }

        private void disconnectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Disconnect();
            stateChange(0);
        }

        private void renameToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        void SendMessage()
        {            
                string message = textBox2.Text;
                textBox2.Text = "";
                byte[] data = Encoding.Unicode.GetBytes(message);
                stream.Write(data, 0, data.Length);            
        }
        
        void ReceiveMessage()
        {
            bool IsConnected = true;   
            while (IsConnected)
            {
                try
                {
                    byte[] data = new byte[64]; // буфер для получаемых данных
                    StringBuilder builder = new StringBuilder();
                    int bytes = 0;
                    do
                    {
                        bytes = stream.Read(data, 0, data.Length);
                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                    }
                    while (stream.DataAvailable);

                    string message = builder.ToString();
                    AddText(message+Environment.NewLine);//вывод сообщения
                }
                catch
                {
                    AddText("Подключение прервано!"); //соединение было прервано     
                    Disconnect();             
                    IsConnected=false;
                }
            }
        }

        delegate void Del();
        void AddText(string text)
        {            
            textBox1.Invoke(new Del(() =>textBox1.Text += text));
        }           

        void Disconnect()
        {
            if (stream != null)
                stream.Close();//отключение потока
            if (client != null)
                client.Close();//отключение клиента            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SendMessage();            
        }        
    }
}
