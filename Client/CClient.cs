using System;
using System.IO;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Client
{
    class CClient
    {
        private readonly NetworkStream _ns;
        private readonly TcpClient _tcp;
        private readonly Encoding _encoding = Encoding.UTF8;

        public CClient()
        {
            //サーバーのIPアドレス（または、ホスト名）とポート番号
            var ipAddress = "127.0.0.1";
            var port = 2001;

            //TcpClientを作成し、サーバーと接続する
            _tcp = new TcpClient(ipAddress, port);

            //接続状態を表示
            Console.WriteLine("サーバー({0}:{1})と接続しました(クライアント {2}:{3})。",
                ((IPEndPoint)_tcp.Client.RemoteEndPoint).Address,
                ((IPEndPoint)_tcp.Client.RemoteEndPoint).Port,
                ((IPEndPoint)_tcp.Client.LocalEndPoint).Address,
                ((IPEndPoint)_tcp.Client.LocalEndPoint).Port);

            //NetworkStreamを取得する
            _ns = _tcp.GetStream();

            //下記設定は、(.NET Framework 2.0以上が必要)
            _ns.ReadTimeout = 10000;  // 10 sec
            _ns.WriteTimeout = 10000; // 10 sec
        }

        //サーバに文字列 sendMsg を送信する
        public void SendStringToServer(string sendMsg)
        {
            //文字列をByte型配列に変換
            byte[] sendBytes = _encoding.GetBytes(sendMsg + '\n');

            //データを送信する
            _ns.Write(sendBytes, 0, sendBytes.Length);
            Console.WriteLine("文字送信: {0}", sendMsg);
        }

        //サーバーから送られたデータを受信し、文字列 resMsg として返す
        public string RecieveStringFromServer()
        {
            var ms = new MemoryStream();
            var resBytes = new byte[256];
            int resSize;
            do
            {
                //データの一部を受信する
                resSize = _ns.Read(resBytes, 0, resBytes.Length);

                //Readが0を返した時はサーバーが切断したと判断
                if (resSize == 0)
                {
                    Console.WriteLine("サーバーが切断しました。");
                    break;
                }

                //受信したデータを蓄積する
                ms.Write(resBytes, 0, resSize);

                //まだ読み取れるデータがあるか、データの最後が\nでない時は、受信を続ける
            } while (_ns.DataAvailable || resBytes[resSize - 1] != '\n');

            //受信したデータを文字列に変換
            string resMsg = _encoding.GetString(ms.GetBuffer(), 0, (int) ms.Length);
            return resMsg;
        }

        //intList をサーバに送信する 
        public void SendIntListToServer(List<int> intList)
        {
            foreach (int ele in intList)
            {
                string elementString = (ele + " ");
                byte[] byteData = _encoding.GetBytes(elementString);
                _ns.Write(byteData, 0, byteData.Length);
                Console.WriteLine(ele);
            }
            Console.WriteLine("送信完了: intList");
        }

        //接続を閉じる
        public void CloseConnection()
        {
            _ns.Close();
            _tcp.Close();
            Console.WriteLine("切断しました。");
        }
    }
}