// Test_Client.cs

using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

public class Client
{
    public static void Main()
    {
        //サーバーに送信するデータを入力してもらう
        Console.WriteLine("文字列を入力し、Enterキーを押してください。");
        string sendMsg = Console.ReadLine();
        //何も入力されなかった時（null or 文字数が0）は終了
        if (sendMsg == null || sendMsg.Length == 0)
        {
            return;
        }

        //サーバーのIPアドレス（または、ホスト名）とポート番号
        string ipOrHost = "127.0.0.1";
        int port = 2001;

        //TcpClientを作成し、サーバーと接続する
        System.Net.Sockets.TcpClient tcp = new System.Net.Sockets.TcpClient(ipOrHost, port);
        Console.WriteLine("サーバー({0}:{1})と接続しました({2}:{3})。",
            ((IPEndPoint)tcp.Client.RemoteEndPoint).Address,
            ((IPEndPoint)tcp.Client.RemoteEndPoint).Port,
            ((IPEndPoint)tcp.Client.LocalEndPoint).Address,
            ((IPEndPoint)tcp.Client.LocalEndPoint).Port);

        //NetworkStreamを取得する
        System.Net.Sockets.NetworkStream ns = tcp.GetStream();

        //読み取り、書き込みのタイムアウトを10秒にする
        //デフォルトはInfiniteで、タイムアウトしない
        //(.NET Framework 2.0以上が必要)
        ns.ReadTimeout = 10000;
        ns.WriteTimeout = 10000;

        //サーバーにデータを送信する
        //文字列をByte型配列に変換
        Encoding enc = Encoding.UTF8;
        byte[] sendBytes = enc.GetBytes(sendMsg + '\n');
        //データを送信する
        ns.Write(sendBytes, 0, sendBytes.Length);
        Console.WriteLine("文字送信:{0}",sendMsg);

        //サーバーから送られたデータを受信する
        System.IO.MemoryStream ms = new System.IO.MemoryStream();
        byte[] resBytes = new byte[256];
        int resSize = 0;
        do
        {
            //データの一部を受信する
            resSize = ns.Read(resBytes, 0, resBytes.Length);
            //Readが0を返した時はサーバーが切断したと判断
            if (resSize == 0)
            {
                Console.WriteLine("サーバーが切断しました。");
                break;
            }
            //受信したデータを蓄積する
            ms.Write(resBytes, 0, resSize);
            //まだ読み取れるデータがあるか、データの最後が\nでない時は、
            // 受信を続ける
        } while (ns.DataAvailable || resBytes[resSize - 1] != '\n');
        //受信したデータを文字列に変換
        string resMsg = enc.GetString(ms.GetBuffer(), 0, (int)ms.Length);
        ms.Close();
        //末尾の\nを削除
        double dresMsg = double.Parse(resMsg.TrimEnd('\n'));
        Console.WriteLine("double受信:{0}",dresMsg);
        //データをストリームへ取得
        System.Net.Sockets.NetworkStream stream = tcp.GetStream();


        // List<T>クラスのインスタンス化
        List<int> intList = new List<int>();
        // 要素の追加
        intList.Add(1000);
        intList.Add(2000);

        Console.WriteLine("List<int>送信");
        for (int i = 0; i < intList.Count; i++)
        {
            string greeting = (intList[i] + " ") ; // キャスト不要
            byte[] Gdata = Encoding.UTF8.GetBytes(greeting);
            ns.Write(Gdata, 0, Gdata.Length);
            Console.WriteLine(intList[i]);
        }

        //閉じる
        ns.Close();
        tcp.Close();
        Console.WriteLine("切断しました。");

        Console.ReadLine();
    }
}