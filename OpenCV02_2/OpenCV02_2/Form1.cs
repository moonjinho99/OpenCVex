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
    public enum DataPacketType
    {
        IMAGE_END
    }

    public partial class Form1 : Form
    {
        private UdpClient client;
        private const int PORT = 9050;
        private IPEndPoint serverEP;
        private Mat frame;
        private int totalFrameSize = 0;
        private byte[] receivedImageData;

        public Form1()
        {
            InitializeComponent();
            client = new UdpClient(9051);
            serverEP = new IPEndPoint(IPAddress.Parse("192.168.56.1"), 9052);
        }

        // private Mat frame = null;

        private void ReceiveFrames()
        {
            try
            {
                while (true)
                {
                    byte[] imageData = client.Receive(ref serverEP);


                    if (imageData.Length > 0)
                    {
                        if (imageData[imageData.Length - 1] == (byte)DataPacketType.IMAGE_END)
                        {
                            // 이미지의 끝을 나타내는 마커를 제거하고 프레임을 출력
                            Array.Resize(ref imageData, imageData.Length - 1);
                            ProcessReceivedFrame(imageData);
                        }
                        else
                        {
                            // 받은 이미지 데이터를 임시 배열에 추가
                            ProcessReceivedFrame(imageData);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error : " + ex.Message);
            }
            finally
            {
                client.Close();
            }
        }

        private void ProcessReceivedFrame(byte[] imageData)
        {
            if (receivedImageData == null)
            {
                // 첫 번째 패킷을 받았을 때 프레임의 전체 크기를 확인하고 임시 배열 초기화
                totalFrameSize = BitConverter.ToInt32(imageData, 0);

                receivedImageData = new byte[totalFrameSize];
                Array.Copy(imageData, 4, receivedImageData, 0, imageData.Length - 4);
            }
            else
            {
                // 이후 패킷을 받을 때마다 임시 배열에 데이터 추가
                int offset = BitConverter.ToInt32(imageData, 0);
                Array.Copy(imageData, 4, receivedImageData, offset, imageData.Length - 4);
            }


            

            // 전체 프레임을 받았을 때 Mat 형식으로 변환하여 출력
            if (receivedImageData.Length >= totalFrameSize)
            {

                Console.WriteLine(Encoding.UTF8.GetString(receivedImageData));



                // JPEG 형식으로 디코딩하고 Mat으로 로드
                Mat decodedFrame = Cv2.ImDecode(receivedImageData, ImreadModes.Color);

                // 이미지 출력
                if (decodedFrame != null && decodedFrame.Width > 0 && decodedFrame.Height > 0)
                {
                    frame = decodedFrame;
                    Console.WriteLine(Encoding.UTF8.GetString(decodedFrame.ToBytes()));

                    // UI 스레드를 통해 PictureBox에 이미지 출력
                    BeginInvoke(new Action(() =>
                    {

                        ;
                        pictureBox2.Image = BitmapConverter.ToBitmap(frame);
                    }));
                }
                else
                {
                    //MessageBox.Show("유효하지 않은 이미지입니다.");
                }

                // 임시 배열 초기화
                receivedImageData = null;
            }
        }



        private void button1_Click(object sender, EventArgs e)
        {
            // 동영상 프레임 수신을 담당하는 스레드 시작
            Thread receiveThread = new Thread(ReceiveFrames);
            receiveThread.IsBackground = true;
            receiveThread.Start();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            client.Close();
        }
    }
}
