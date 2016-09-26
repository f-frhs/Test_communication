// Test_Server.cs

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

// ---流れ---
// TcpListenerクラスのStartメソッドによりListen（監視）を開始し、クライアントからの接続要求を待機します
// 接続要求があれば、AcceptTcpClientメソッドにより、クライアントの接続要求を受け入れます
// AcceptTcpClientメソッドで返されるTcpClientオブジェクトのGetStreamメソッドにより、NetworkStreamを取得します。データの送受信には、このNetworkStreamを使用します
// Listenを続けるのであれば、再びAcceptTcpClientメソッドで接続を待機します。Listenを終了するのであれば、Stopメソッドを呼び出します
//
// 参照URL http://dobon.net/vb/dotnet/internet/tcpclientserver.html
//
// 追加: クライアントからdoubleを受信したのち、List<int>を送信

public class CServer
{
    private readonly TcpClient _client;
    private readonly NetworkStream _ns;
    private readonly Encoding _enc = Encoding.UTF8;
    private bool _isConnected;
    private readonly TcpListener _listener;
    private static NetworkStream _stream;

    //コンストラクタ
    public CServer()
    {
        //サーバの設定
        string ipString = "127.0.0.1";  //ListenするIPアドレス
        int port = 2001;  //Listenするポート番号

        //TcpListenerオブジェクトを作成する
        IPAddress ipAdd = IPAddress.Parse(ipString);
        _listener = new TcpListener(ipAdd, port);

        //Listenを開始する
        _listener.Start();
        Console.WriteLine("Listenを開始しました({0}:{1})。",
            ((IPEndPoint)_listener.LocalEndpoint).Address,
            ((IPEndPoint)_listener.LocalEndpoint).Port);

        //接続要求があったら受け入れる
        _client = _listener.AcceptTcpClient();
        Console.WriteLine("クライアント({0}:{1})と接続しました。",
            ((IPEndPoint)_client.Client.RemoteEndPoint).Address,
            ((IPEndPoint)_client.Client.RemoteEndPoint).Port);

        //NetworkStreamを取得・設定
        _ns = _client.GetStream();
        _ns.ReadTimeout = 10000;  //10sec
        _ns.WriteTimeout = 10000; //10sec
    }

    //clientから_streamを経由して、int[]を受け取り、string[]として返す
    public string ReceiveIntList()
    {
        //データをストリームへ取得
        _stream = _client.GetStream();

        //データを受け取るbyte型変数を定義（例では１バイトずつ受け取る）
        byte[] getData = new byte[1];

        //データの取得と同時に、取得したデータのバイト数も得る
        //引数は（受け皿,格納開始位置,受け取るバイト数）
        //どれだけもらうかわからないので一時的に格納するリストを定義
        List<byte> bytelist = new List<byte>();
        //cntには受け取ったデータの長さが入る
        while ((_stream.Read(getData, 0, getData.Length)) > 0)
        {
            //データをリストに追加していく
            bytelist.Add(getData[0]);
        }

        //リストに入った分だけ配列を定義
        byte[] result = new byte[bytelist.Count];

        for (int i = 0; i < result.Length; i++)
        {
            result[i] = bytelist[i];
        }
        //文字列にエンコード
        string data = Encoding.UTF8.GetString(result);
        return data;
    }

    //接続を閉じる
    public void CloseConnection()
    {
        _ns.Close();
        _client.Close();
        Console.WriteLine("クライアントとの接続を閉じました。");

        //リスナを閉じる
        _listener.Stop();
        Console.WriteLine("Listenerを閉じました。");

        Console.ReadLine();
    }

    //クライアントに文字列データを送信する
    public void SendMsg(string stringToSend)
    {
        if (_isConnected)
        {
            //クライアントに送信する文字列を作成
            //文字列をByte型配列に変換
            byte[] sendBytes = _enc.GetBytes(stringToSend + '\n');
            //データを送信する
            _ns.Write(sendBytes, 0, sendBytes.Length);
            Console.WriteLine($"double送信：{stringToSend}");
        }
    }

    //クライアントから送られたデータを受信する
    public string ReceiveMsg()
    {
        _isConnected = true;
        System.IO.MemoryStream ms = new System.IO.MemoryStream();
        byte[] resBytes = new byte[256];
        int resSize;
        do
        {
            //データの一部を受信する
            resSize = _ns.Read(resBytes, 0, resBytes.Length);

            //Readが0を返した時はクライアントが切断したと判断
            if (resSize == 0)
            {
                _isConnected = true;
                Console.WriteLine("クライアントが切断しました。");
                break;
            }

            //受信したデータを蓄積する
            ms.Write(resBytes, 0, resSize);

            //まだ読み取れるデータがあるか、データの最後が\nでない時は、
            // 受信を続ける
        } while (_ns.DataAvailable || resBytes[resSize - 1] != '\n');

        //受信したデータを文字列に変換
        var resMsg = _enc.GetString(ms.GetBuffer(), 0, (int)ms.Length);
        ms.Close();

        //末尾の\nを削除
        resMsg = resMsg.TrimEnd('\n');
        return resMsg;
    }
}
