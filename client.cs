using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;

namespace client
{
    public partial class client : Form
    {
        Socket socket = null;
        IPAddress ipAddress = null;

        public client()
        {
            InitializeComponent();
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            this.Text = "클라이언트-윤주혜";
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            IPHostEntry iphe = Dns.GetHostEntry(Dns.GetHostName());
            
            foreach (IPAddress addr in iphe.AddressList)
            {
                if (addr.AddressFamily == AddressFamily.InterNetwork)
                {
                    ipAddress = addr;
                    break;
                }
            }
            ipBox.Mask = ipAddress.ToString();
            ipBox.Text = ipAddress.ToString();

            sendBtn.Enabled = false;
        }

        private void startBtn_Click(object sender, EventArgs e)
        {
            if (ipBox.Text != "" && portBox.Text != "" && nameBox.Text!="")
            {
                if (Convert.ToInt32(portBox.Text) >= 1024 && Convert.ToInt32(portBox.Text) <= 65535)
                {
                    try { socket.Connect(ipBox.Text, Convert.ToInt32(portBox.Text)); }
                    catch (Exception ex)
                    {
                        MessageBox.Show("연결 실패! ");
                        return;
                    }

                    msgBox.Text += "서버와 연결되었습니다.\r\n";

                    startBtn.Enabled = false;
                    sendBtn.Enabled = true;

                    AsyncObject ao = new AsyncObject(4000);
                    ao.WorkingSocket = socket;

                    socket.BeginReceive(ao.Buffer, 0, ao.BufferSize, 0, DataReceived, ao);
                }
                else
                {
                    MessageBox.Show("포트번호는 1024~65535 사용가능");
                }
            }
            else
            {
                MessageBox.Show("IP와 포트와 사용자 이름을 확인해주세요");
            }
        }
        void DataReceived(IAsyncResult ar)
        {

            AsyncObject ao = (AsyncObject)ar.AsyncState;

            int received = ao.WorkingSocket.EndReceive(ar);
            if (received <= 0)
            {
                ao.WorkingSocket.Close();
                return;
            }

            string text = Encoding.UTF8.GetString(ao.Buffer);

            string[] t = text.Split('>');
            string name = t[0];
            string msg = t[1];

            msgBox.Text += "\r\n" + name + ": " + msg;

            ao.ClearBuffer();

            ao.WorkingSocket.BeginReceive(ao.Buffer, 0, 4000, 0, DataReceived, ao);

        }

        private void sendBtn_Click(object sender, EventArgs e)
        {
            if (!socket.IsBound)
            {
                MessageBox.Show("서버가 실행되고 있지 않습니다.");
                return;
            }

            string text = sendBox.Text;
            if (text == "")
            {
                MessageBox.Show("텍스트를 확인해주세요.");
                sendBox.Focus();
                return;
            }

            IPEndPoint ipep = (IPEndPoint)socket.LocalEndPoint;
            string name = nameBox.Text;
            
            byte[] type = Encoding.UTF8.GetBytes(name + '>' + text);
            socket.Send(type);
            msgBox.Text += "\r\n" + name + ": " + text;
            sendBox.Clear();
        }

        private void endBtn_Click(object sender, EventArgs e)
        {
            IPEndPoint ipep = (IPEndPoint)socket.LocalEndPoint;
            string name = nameBox.Text;

            byte[] type = Encoding.UTF8.GetBytes(name + '>' + "연결을 끊었습니다.");
            socket.Send(type);
            msgBox.Text += "\r\n연결을 끊었습니다.";
            sendBox.Clear();

            socket.Shutdown(SocketShutdown.Both);
            Close();
        }
    }
}
