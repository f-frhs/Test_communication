// Test_Server.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Net.Sockets;

namespace ServerSys
{
    public class Server
    {
        //フィールド
        public TcpListener listener { get; set; }
        public static NetworkStream ns { get; set; }
        public static TcpClient client { get; set; }
        public Encoding enc { get; set; }
        public bool disconnected { get; set; }
        public string resMsg { get; set; }
       
        //接続設定
        //コンストラクタ
        //public Server(string ipString, int port)
        static public void startServer()
        {
            //var sv = new Server(ipString: "127.0.0.1", port: 2001); //接続処理を回したままにしておく
            //ListenするIPアドレス
            IPAddress ipAdd = IPAddress.Parse("127.0.0.1");

            //TcpListenerオブジェクトを作成する
            TcpListener listener = new TcpListener(ipAdd, 2001);

            //Listenを開始する
            listener.Start();
            Console.WriteLine("Listenを開始しました({0}:{1})。",
                ((IPEndPoint)listener.LocalEndpoint).Address,
                ((IPEndPoint)listener.LocalEndpoint).Port);

            //接続要求があったら受け入れる
            client = listener.AcceptTcpClient();
            Console.WriteLine("クライアント({0}:{1})と接続しました。",
                ((IPEndPoint)client.Client.RemoteEndPoint).Address,
                ((IPEndPoint)client.Client.RemoteEndPoint).Port);

            //NetworkStreamを取得
            ns = client.GetStream();
        }

        //受信string：Byteを受信しstringに変換
        public string SresStr()
        {
            //クライアントから送られたデータを受信する
            enc = Encoding.UTF8;
            disconnected = false;
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            byte[] SresBytes = new byte[256];
            int SresSize = 0;
            do
            {
                //データの一部を受信する
                SresSize = ns.Read(SresBytes, 0, SresBytes.Length);

                //Readが0を返した時はクライアントが切断したと判断
                if (SresSize == 0)
                {
                    disconnected = true;
                    Console.WriteLine("クライアントが切断しました。");
                    break;
                }

                //受信したデータを蓄積する
                ms.Write(SresBytes, 0, SresSize);
            } while (ns.DataAvailable || SresBytes[SresSize - 1] != '\n');                //まだ読み取れるデータがあるか、データの最後が\nでない時は、受信を続ける

            //受信したデータ(Byte)をList<int>に変換
            string SresMsg = enc.GetString(ms.GetBuffer(), 0, (int)ms.Length);

            ms.Close();

            //末尾の\nを削除
            SresMsg = SresMsg.TrimEnd('\n');
            return SresMsg;
        }

        //送信double：doubleをstringに変換後、Byteに変換し送信
        public byte[] SsendDou(string remsg)
        {
            //クライアントに送信するdoubleを作成後送信のためstringに変換
            double SsendDou = (remsg.Length) * 0.1234;
            string SsendStr = SsendDou.ToString();
            //文字列をbyte型配列に変換
            byte[] SsenByte = enc.GetBytes(SsendStr + '\n');
            //データを送信する
            ns.Write(SsenByte, 0, SsenByte.Length);

            return SsenByte;
        }

        //受信List：Byteを受信しList<int>に変換
        public List<int> SresList() 
        {
            //データをストリームへ取得
            NetworkStream Sstream = client.GetStream();

            //データを受け取るbyte型変数を定義（例では１バイトずつ受け取る）
            byte[] getData = new byte[1];

            //データの取得と同時に、取得したデータのバイト数も得る.引数は（受け皿,格納開始位置,受け取るバイト数）
            int cnt;

            //どれだけもらうかわからないので一時的に格納するリストを定義
            List<byte> byteList = new List<byte>();

            //cntには受け取ったデータの長さが入る
            while ((cnt = Sstream.Read(getData, 0, getData.Length)) > 0)
            {
                //データをリストに追加していく
                byteList.Add(getData[0]);
            }

            //リストに入った分だけ配列を定義
            byte[] SresListByte = new byte[byteList.Count];
            for (int i = 0; i < SresListByte.Length; i++)
            {
                SresListByte[i] = byteList[i];
            }

            //文字列にエンコード
            string SresStr = Encoding.UTF8.GetString(SresListByte);
            string SresStrNs = SresStr.TrimEnd();     //末尾の\nを除く
            string[] SresStrArr = SresStrNs.Split(' ');

            //string型配列→int型配列
            int[] SresIntArr = new int[SresStrArr.Length];
            for(int i = 0; i < SresIntArr.Length; i++)
            {
                SresIntArr[i] = int.Parse(SresStrArr[i]);
            }

            //int型配列→List<int>
            List<int> SresIntList = new List<int>(SresIntArr);
            return SresIntList;
        }

        //切断設定
        public void Sclose()
        {
            //閉じる
            ns.Close();
            client.Close();
            Console.WriteLine("クライアントとの接続を閉じました。");

            //リスナを閉じる
            listener.Stop();
            Console.WriteLine("listenerを閉じました。");
            Console.WriteLine();
        }
    }

    class ServerMain
    {
        //private Server sv { get; set; }
        //Main
        public static void Main()
        {
            Server.startServer();
            
            //var sv = new Server();
            //接続設定
            Console.WriteLine();

            //try
            //{
            //    //受信string
            //    var SrecMsg1 = sv.SresStr();
            //    Console.WriteLine("文字受信：{0}", SrecMsg1);

            //    //送信double
            //    var SSendMsg = sv.SsendDou(SrecMsg1);
            //    Console.WriteLine("double送信：{0}", SSendMsg);

            //    //受信List
            //    var SrecInt = sv.SresList();
            //    foreach (int stData in SrecInt)
            //    {
            //        Console.WriteLine(stData);
            //    }

            //    //切断設定
            //    sv.Sclose();
            //}

            //catch (Exception)
            //{
            //    sv.Sclose();
            //}
        }
    }
}