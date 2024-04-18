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
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.Threading;

namespace OpenCV02_2
{
    public enum DataPacketType
    {
        IMAGE_END
    }

    public partial class Form1 : Form
    {
        private UdpClient client;
        private IPEndPoint serverEP;
        private List<byte> imageDataList = new List<byte>();

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                serverEP = new IPEndPoint(IPAddress.Parse("192.168.56.1"), 9050);
                client = new UdpClient(serverEP);
                ReceiveAndDisplayVideo();
            }
            catch (Exception ex)
            {
                Console.WriteLine("에러 : " + ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }

        private object imageDataListLock = new object();

        private void ReceiveAndDisplayVideo()
        {
            try
            {
                while (true)
                {
                    byte[] receivedData = client.Receive(ref serverEP);
                    /*lock (imageDataList) // 데이터에 대한 접근을 동기화합니다.
                    {
                        imageDataList.AddRange(receivedData);

                        // 이미지의 마지막 부분인지 확인
                        if (receivedData[receivedData.Length - 1] == (byte)DataPacketType.IMAGE_END)
                        {
                            byte[] imageDataArray = new byte[imageDataList.Count]; // 새로운 배열 생성
                            imageDataList.CopyTo(imageDataArray); // 데이터 복사

                            ThreadPool.QueueUserWorkItem((state) =>
                            {
                                DisplayImage(imageDataArray); // 새 배열 전달
                                lock (imageDataList) // 데이터에 대한 접근을 동기화합니다.
                                {
                                    imageDataList.Clear();
                                }
                            });
                        }
                    }*/
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }





        private void DisplayImage(byte[] imageData)
        {
            Mat receivedFrame = Mat.FromImageData(imageData);
            Bitmap bitmap = BitmapConverter.ToBitmap(receivedFrame);
            pictureBox2.Image = bitmap;
        }
    }
}
