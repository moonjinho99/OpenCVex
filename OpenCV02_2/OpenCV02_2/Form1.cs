using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.Text;

namespace OpenCV02_2
{
    public partial class Form1 : Form
    {
        private const int FrameWidth = 640;
        private const int FrameHeight = 480;
        private const int ChunkSize = 1024; // 데이터 조각의 크기

        private VideoCapture _capture;

        public Form1()
        {
            InitializeComponent();
        }
        // private Mat frame = null;
        private void button1_Click(object sender, EventArgs e)
        {
            Thread receiverThread = new Thread(ReceiveVideo);
            receiverThread.IsBackground = true;
            receiverThread.Start();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            //client.Close();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
        private void ReceiveVideo()
        {
            using (Socket receiverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                IPEndPoint senderEP = new IPEndPoint(IPAddress.Any, 9050);
                receiverSocket.Bind(senderEP);

                byte[] buffer;
                int totalDataSize;
                int receivedDataSize;
                MemoryStream memoryStream = new MemoryStream();

                while (true)
                {
                    // 전체 데이터 크기 수신
                    buffer = new byte[4];
                    receiverSocket.Receive(buffer);
                    totalDataSize = BitConverter.ToInt32(buffer, 0);

                    // 데이터를 조각으로 받아 메모리 스트림에 저장
                    while (memoryStream.Length < totalDataSize)
                    {
                        buffer = new byte[ChunkSize];
                        receivedDataSize = receiverSocket.Receive(buffer);
                        memoryStream.Write(buffer, 0, receivedDataSize);
                    }

                    // 메모리 스트림에서 이미지로 변환하여 출력
                    if (memoryStream.Length == totalDataSize)
                    {
                        byte[] imageData = memoryStream.ToArray();
                        Mat frame = Cv2.ImDecode(imageData, ImreadModes.Color);

                        if (!frame.Empty())
                        {
                            pictureBox2.Image = BitmapConverter.ToBitmap(frame);
                        }

                        memoryStream.Dispose();
                        memoryStream = new MemoryStream();
                    }
                }
            }
        }
    }
}
