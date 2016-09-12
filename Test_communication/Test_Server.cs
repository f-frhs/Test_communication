// Test_Server.cs

using System;
using System.Collections.Generic;

// ---流れ---
// TcpListenerクラスのStartメソッドによりListen（監視）を開始し、クライアントからの接続要求を待機します
// 接続要求があれば、AcceptTcpClientメソッドにより、クライアントの接続要求を受け入れます
// AcceptTcpClientメソッドで返されるTcpClientオブジェクトのGetStreamメソッドにより、NetworkStreamを取得します。データの送受信には、このNetworkStreamを使用します
// Listenを続けるのであれば、再びAcceptTcpClientメソッドで接続を待機します。Listenを終了するのであれば、Stopメソッドを呼び出します
//
// 参照URL http://dobon.net/vb/dotnet/internet/tcpclientserver.html
//
// 追加: クライアントからdoubleを受信したのち、List<int>を送信

public class Server
{
    public static void Main()
    {
        //ListenするIPアドレス
        string ipString = "127.0.0.1";
        System.Net.IPAddress ipAdd = System.Net.IPAddress.Parse(ipString);

        //ホスト名からIPアドレスを取得する時は、次のようにする
        //string host = "localhost";
        //System.Net.IPAddress ipAdd =
        //    System.Net.Dns.GetHostEntry(host).AddressList[0];
        //.NET Framework 1.1以前では、以下のようにする
        //System.Net.IPAddress ipAdd =
        //    System.Net.Dns.Resolve(host).AddressList[0];

        //Listenするポート番号
        int port = 2001;

        //TcpListenerオブジェクトを作成する
        System.Net.Sockets.TcpListener listener = new System.Net.Sockets.TcpListener(ipAdd, port);

        //Listenを開始する
        listener.Start();
        Console.WriteLine("Listenを開始しました({0}:{1})。", ((System.Net.IPEndPoint)listener.LocalEndpoint).Address,((System.Net.IPEndPoint)listener.LocalEndpoint).Port);

        //接続要求があったら受け入れる
        System.Net.Sockets.TcpClient client = listener.AcceptTcpClient();
        Console.WriteLine("クライアント({0}:{1})と接続しました。",((System.Net.IPEndPoint)client.Client.RemoteEndPoint).Address, ((System.Net.IPEndPoint)client.Client.RemoteEndPoint).Port);

        //NetworkStreamを取得
        System.Net.Sockets.NetworkStream ns = client.GetStream();

        //読み取り、書き込みのタイムアウトを10秒にする
        //デフォルトはInfiniteで、タイムアウトしない
        //(.NET Framework 2.0以上が必要)
        ns.ReadTimeout = 10000;
        ns.WriteTimeout = 10000;

        //クライアントから送られたデータを受信する
        System.Text.Encoding enc = System.Text.Encoding.UTF8;
        bool disconnected = false;
        System.IO.MemoryStream ms = new System.IO.MemoryStream();
        byte[] resBytes = new byte[256];
        int resSize = 0;
        do
        {
            //データの一部を受信する
            resSize = ns.Read(resBytes, 0, resBytes.Length);
            //Readが0を返した時はクライアントが切断したと判断
            if (resSize == 0)
            {
                disconnected = true;
                Console.WriteLine("クライアントが切断しました。");
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
        resMsg = resMsg.TrimEnd('\n');
        Console.WriteLine(resMsg);

        //クライアントにデータを送信する
        if (!disconnected)
        {
            //クライアントに送信する文字列を作成
            string sendMsg = resMsg.Length.ToString();
            //文字列をByte型配列に変換
            byte[] sendBytes = enc.GetBytes(sendMsg + '\n');
            //データを送信する
            ns.Write(sendBytes, 0, sendBytes.Length);
            Console.WriteLine(sendMsg);
        };

        //データを受け取るbyte型変数を定義（例では１バイトずつ受け取る）
        byte[] getData = new byte[1];

        //データの取得と同時に、取得したデータのバイト数も得る
        //引数は（受け皿,格納開始位置,受け取るバイト数）
        int cnt;
        //どれだけもらうかわからないので一時的に格納するリストを定義
        List<byte> bytelist = new List<byte>();
        //cntには受け取ったデータの長さが入る
        while ((cnt = ns.Read(getData, 0, getData.Length)) > 0)
        {
            //データをリストに追加していく
            bytelist.Add(getData);
        }

        //リストに入った分だけ配列を定義
        byte[] result = new byte[bytelist.Count];

        for (int i = 0; i < result.Length; i++)
        {
            result[i] = bytelist[i];
        }
        //文字列にエンコード
        string data = System.Text.Encoding.UTF8.GetString(result);
        //データの出力
        Console.WriteLine("受信結果:{0}", data);

        //閉じる
        ns.Close();
        client.Close();
        Console.WriteLine("クライアントとの接続を閉じました。");

        //リスナを閉じる
        listener.Stop();
        Console.WriteLine("Listenerを閉じました。");

        Console.ReadLine();
    }
}