// Test_Server.cs

using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

public class Server
{
    //------------------------フィールド-------------------------------------------
    public System.Net.Sockets.TcpListener listener { get; set; }
    public System.Net.Sockets.NetworkStream ns { get; set; }
    public System.Net.Sockets.TcpClient client { get; set; }
    public Encoding enc { get; set; }
    public bool disconnected { get; set; }
    public string resMsg {get; set;}

    //-----------------------接続設定------------------------------------
    //コンストラクタ
    public Server(string ipString, int port)
    {
        //ListenするIPアドレス
        IPAddress ipAdd = IPAddress.Parse(ipString);

        //TcpListenerオブジェクトを作成する
        listener = new System.Net.Sockets.TcpListener(ipAdd, port);

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

        //読み取り、書き込みのタイムアウトを10秒にする
        //デフォルトはInfiniteで、タイムアウトしない
        //ns.ReadTimeout = 10000;
        //ns.WriteTimeout = 10000;
    }

    //-----------------------受信設定1------------------------------------

    public string SResveData1()
    {
        //クライアントから送られたデータを受信する
        enc = Encoding.UTF8;
        disconnected = false;
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
        } while (ns.DataAvailable || resBytes[resSize - 1] != '\n');　               //まだ読み取れるデータがあるか、データの最後が\nでない時は、受信を続ける

        //受信したデータを文字列に変換
        string resMsg = enc.GetString(ms.GetBuffer(), 0, (int)ms.Length);
        ms.Close();
        //末尾の\nを削除
        resMsg = resMsg.TrimEnd('\n');
        return resMsg;
    }

    //-----------------------送信設定----------------------------------------

    public string SSendData()
    {
        //クライアントに送信する文字列を作成
        double dresmsg = 30.251; 
        string sendmsg = dresmsg.ToString();
        //文字列をbyte型配列に変換
        byte[] sendbytes = enc.GetBytes(sendmsg + '\n');
        //データを送信する
        ns.Write(sendbytes, 0, sendbytes.Length);
        //console.writeline("double送信：{0}", dresmsg);
        return sendmsg;
    }

    //------------------------受信設定2------------------------------------
    public string SResveData2()
    {

        //データをストリームへ取得
        System.Net.Sockets.NetworkStream stream = client.GetStream();

        //データを受け取るbyte型変数を定義（例では１バイトずつ受け取る）
        byte[] getData = new byte[1];

        //データの取得と同時に、取得したデータのバイト数も得る.引数は（受け皿,格納開始位置,受け取るバイト数）
        int cnt;
        //どれだけもらうかわからないので一時的に格納するリストを定義
        List<byte> bytelist = new List<byte>();
        //cntには受け取ったデータの長さが入る
        while ((cnt = stream.Read(getData, 0, getData.Length)) > 0)
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
        //データの出力  
    }

    //-------------------切断設定--------------------------------------
    public void SClose()
    {
        //閉じる
        ns.Close();
        client.Close();
        Console.WriteLine("クライアントとの接続を閉じました。");

        //リスナを閉じる
        listener.Stop();
        Console.WriteLine("listenerを閉じました。");
        Console.ReadLine();
    }

    //---------------------- Main -----------------------------------------

    public static void Main()
    {
        //接続設定
        var sv = new Server(ipString: "127.0.0.1", port: 2001); //接続処理を回したままにしておく

        //受信設定1
        var SReceivingMsg1 = sv.SResveData1();
        Console.WriteLine("受信：{0}", SReceivingMsg1);

        //送信設定
        //クライアントにデータを送信する
        var SSendMsg = sv.SSendData();
        Console.WriteLine("送信：{0}", SSendMsg);

        //受信設定2
        var SReceivingMsg2 = sv.SResveData2();
        string[] Sdata = SReceivingMsg2.Split(' ');
        foreach (string stData in Sdata)
        {
            Console.WriteLine(stData);
        }

        //切断設定
        sv.SClose();
    }
}