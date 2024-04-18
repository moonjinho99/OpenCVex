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


namespace OpenCVEx02
{

    public enum DataPacketType
    {
        IMAGE_END
    }

    public partial class Form1 : Form
    {
        private UdpClient client;
        private IPEndPoint serverEP;
        private VideoCapture capture;

        public Form1()
        {
            InitializeComponent();
        }

        private void startBtn_Click(object sender, EventArgs e)
        {
            client = new UdpClient();
            serverEP = new IPEndPoint(IPAddress.Parse("192.168.56.1"), 9050);
            capture = new VideoCapture(0);

            timer1.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            using (Mat frame = new Mat())
            {
                capture.Read(frame);
                if(!frame.Empty())
                {
                    SendVideo(frame);
                    pictureBox1.Image = BitmapConverter.ToBitmap(frame);
                    //Console.WriteLine("이미지 데이터 : " + frame);
                }
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            capture.Release();
        }


        /* private void SendVideo(Mat frame)
         {
             int ChunkSize = 1024;

             try
             {
                 byte[] imageData = frame.ToBytes(".jpg");

                 client.Send(imageData, imageData.Length, serverEP);
             } catch(Exception ex)
             {
                 MessageBox.Show(ex.Message);
             }

         }*/

        private void SendVideo(Mat frame)
        {
            int chunkSize = 1024; // 조각의 크기
            byte[] imageData = frame.ToBytes(".jpg"); // 전체 이미지 데이터

            // 전체 이미지 데이터의 크기
            int totalSize = imageData.Length;

            // 이미지 데이터를 조각으로 나누어 전송
            for (int offset = 0; offset < totalSize; offset += chunkSize)
            {
                int size = Math.Min(chunkSize, totalSize - offset); // 나눌 조각의 크기
                byte[] chunkData = new byte[size];
                Array.Copy(imageData, offset, chunkData, 0, size); // 조각 데이터 복사

                // 이미지의 끝을 나타내는 마커를 추가
                bool isLastChunk = (offset + size) >= totalSize;
                if (isLastChunk)
                {
                    byte[] markedChunkData = new byte[size + 1];
                    Array.Copy(chunkData, markedChunkData, size);
                    markedChunkData[size] = (byte)DataPacketType.IMAGE_END; // 이미지의 끝을 나타내는 마커 추가
                    chunkData = markedChunkData;
                }

                // TODO: chunkData를 전송
                try
                {
                    client.Send(chunkData, chunkData.Length, serverEP);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }


    }
}
