// Test_Client.cs

using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Net.Sockets;

public class Client
{
    //------------------------フィールド-------------------------------------------
    public NetworkStream ns { get; set; }
    public TcpClient tcp { get; set; }
    public string sendMsg { get; set; }
    public Encoding douData { get; set; }

    //------------------------接続設定---------------------------------------------
    //コンストラクタ
    public Client(string ipOrHost, int port)
    {
        bool again;
        do
        {
            again = false;
            try
            {
                //TcpClientを作成し、サーバーと接続する
                tcp = new TcpClient(ipOrHost, port);
                Console.WriteLine("サーバー({0}:{1})と接続しました({2}:{3})。",
                    ((IPEndPoint)tcp.Client.RemoteEndPoint).Address,
                    ((IPEndPoint)tcp.Client.RemoteEndPoint).Port,
                    ((IPEndPoint)tcp.Client.LocalEndPoint).Address,
                    ((IPEndPoint)tcp.Client.LocalEndPoint).Port);
                //NetworkStreamを取得する
                ns = tcp.GetStream();
            }
            catch (System.Net.Sockets.SocketException)
            {
                again = true;
                Console.WriteLine("Connection Error:Serverを起動してください");
                System.Threading.Thread.Sleep(1000);
            }
        } while (again);
    }
    //----------------------------入力設定--------------------------------------------
    //サーバーに送信するデータを入力してもらう
    public string Comment()
    {
        Console.WriteLine("文字列を入力し、Enterキーを押してください。");
        sendMsg = Console.ReadLine();
        //何も入力されなかった時（null or 文字数が0）は終了
        if (sendMsg == null || sendMsg.Length == 0)
        {
            return string.Empty;
        }
        return sendMsg;
    }
    //----------------------------リスト作成-------------------------------------------
    public List<int> LData()
    {
        //データをストリームへ取得
        NetworkStream stream = tcp.GetStream();
        // List<T>クラスのインスタンス化
        List<int> intList = new List<int>();
        // 要素の追加
        intList.Add(1001200);
        intList.Add(2001100);
        intList.Add(3001200);
        return intList;
    }
    //----------------------------送信設定--------------------------------------------
    public string CSendData(string sdata)
    {
        //サーバーにデータを送信する
        //文字列をByte型配列に変換
        douData = Encoding.UTF8;
        byte[] sendBytes = douData.GetBytes(sendMsg + '\n');
        //データを送信する
        ns.Write(sendBytes, 0, sendBytes.Length);
        return sendMsg;
    }

    public List<int> CSendData(List<int> sendMsg)
    {
         for (int i = 0; i < sendMsg.Count; i++)
        {
            string strData = (sendMsg[i] + " ");
            byte[] Gdata = Encoding.UTF8.GetBytes(strData);
            ns.Write(Gdata, 0, Gdata.Length);
            Console.WriteLine(strData);
        }
        return sendMsg;
    }
    //-----------------------------受信設定-----------------------------------------------
    public double CResceiveData()
    {
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
            //まだ読み取れるデータがあるか、データの最後が\nでない時は、受信を続ける
        } while (ns.DataAvailable || resBytes[resSize - 1] != '\n');
        
        //受信したデータを文字列に変換
        string resMsg = douData.GetString(ms.GetBuffer(), 0, (int)ms.Length);
        ms.Close();    
        //末尾の\nを削除
        double dresMsg = double.Parse(resMsg.TrimEnd('\n'));
        return dresMsg;
    }
     //----------------------------------切断設定--------------------------------------------
    public void CClose()
    {
        //閉じる
        ns.Close();
        tcp.Close();
        Console.WriteLine("切断しました。");
        Console.ReadLine();
    }
    //-----------------------------------Main-----------------------------------------------
    public static void Main()
    {
        //接続設定
        var cv = new Client(ipOrHost: "127.0.0.1", port: 2001);

        //入力設定
        var CComent = cv.Comment();
        Console.WriteLine();
        if (cv.sendMsg != string.Empty)
        {
            //送信設定1
            var CSendMsg1 = cv.CSendData(CComent);
            Console.WriteLine("文字送信：{0}", CSendMsg1);
            //受信設定
            var CReceiveMsg = cv.CResceiveData();
            Console.WriteLine("double受信:{0}", CReceiveMsg);
            //送信設定2
            var SLData = cv.LData();
            Console.WriteLine("List<int>送信：");
            var CSendMsg2 = cv.CSendData(SLData);
        }
        //切断設定
        Console.WriteLine();
        cv.CClose();
    }
}