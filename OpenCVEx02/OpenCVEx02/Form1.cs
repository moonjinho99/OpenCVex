using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.Net;
using System.Net.Sockets;
using System.Threading;


namespace OpenCVEx02
{

    public partial class Form1 : Form
    {
        private const int FrameWidth = 640;
        private const int FrameHeight = 480;
        private const int ChunkSize = 1024;

        private VideoCapture _capture;


        public Form1()
        {
            InitializeComponent();

        }

        private void startBtn_Click(object sender, EventArgs e)
        {

            _capture = new VideoCapture(0); // 내장 카메라 사용
            _capture.FrameWidth = FrameWidth;
            _capture.FrameHeight = FrameHeight;

            // 송신 시작
            Thread senderThread = new Thread(SendVideo);
            senderThread.IsBackground = true;
            senderThread.Start();
        }


        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            _capture.Release();
        }

        private void SendVideo()
        {
            using (Socket senderSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                senderSocket.Connect(IPAddress.Parse("192.168.50.223"), 9050); 

                Mat frame = new Mat();
                byte[] buffer;

                while (true)
                {
                    _capture.Read(frame);

                    if (!frame.Empty())
                    {

                        pictureBox1.Image = BitmapConverter.ToBitmap(frame);

                        // 이미지를 JPEG 형식으로 인코딩하여 바이트 배열로 변환
                        buffer = frame.ToBytes(".jpg");

                        // 전체 데이터 크기 전송
                        senderSocket.Send(BitConverter.GetBytes(buffer.Length));

                        // 데이터를 조각으로 나누어 전송
                        for (int i = 0; i < buffer.Length; i += ChunkSize)
                        {
                            int size = Math.Min(ChunkSize, buffer.Length - i);
                            senderSocket.Send(buffer, i, size, SocketFlags.None);
                        }
                    }

                    Thread.Sleep(30); // 잠시 대기 후 다음 프레임 처리
                }
            }
        }



    }
}
