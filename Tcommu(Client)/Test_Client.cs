// Test_Client.cs

using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Net.Sockets;

public class Client
{
    //フィールド
    public NetworkStream ns { get; set; }
    public TcpClient tcp { get; set; }
    public string sendStr { get; set; }
    public Encoding enco { get; set; }
    public string cv{get; set;}
    public byte[] CsendListByte{get; set; }

    //接続設定:サーバと接続できるまで繰り返す
    //コンストラクタ
    //接続設定
    //--//public Client()
    public Client(string ipOrHost, int port)
    {
        bool again;
        do
        {
            again = false;
            try
            {
                //TcpClientを作成し、サーバーと接続する
                //--//tcp = new TcpClient("127.0.0.1",2001);
                tcp = new TcpClient(ipOrHost, port);

                Console.WriteLine("サーバー({0}:{1})と接続しました({2}:{3})。",
                    ((IPEndPoint)tcp.Client.RemoteEndPoint).Address,
                    ((IPEndPoint)tcp.Client.RemoteEndPoint).Port,
                    ((IPEndPoint)tcp.Client.LocalEndPoint).Address,
                    ((IPEndPoint)tcp.Client.LocalEndPoint).Port);
                //NetworkStreamを取得する
                ns = tcp.GetStream();
            }
            catch (SocketException)
            {
                again = true;
                Console.WriteLine("Connection Error:Serverを起動してください");
                System.Threading.Thread.Sleep(1000);
            }
        } while (again);
        //var cv = new Client();
    }

    //入力：任意の文字列を入力stringを出力
    public string Comment()
    {
        Console.WriteLine("文字列を入力し、Enterキーを押してください。");
        sendStr = Console.ReadLine();

        //何も入力されなかった時（null or 文字数が0）は終了
        if (sendStr == null || sendStr.Length == 0)
        {
            return string.Empty;
        }

        return sendStr;
    }

    //送信string：stringをByteに変換し送信 
    public byte[] CsendStr(string sData)
    {
        //文字列をByte型配列に変換
        enco = Encoding.UTF8;
        byte[] CsendBytes = enco.GetBytes(sendStr + '\n');

        //データを送信
        ns.Write(CsendBytes, 0, CsendBytes.Length);

        return CsendBytes;
    }

    //受信double：Byteを受信しdoubleに変換
    public double CresceiveData()
    {
        //サーバーから送られたデータを受信する
        System.IO.MemoryStream ms = new System.IO.MemoryStream();
        byte[] CresByte = new byte[256];
        int CresSize = 0;
        do
        {
            //データの一部を受信する
            CresSize = ns.Read(CresByte, 0, CresByte.Length);

            //Readが0を返した時はサーバーが切断したと判断
            if (CresSize == 0)
            {
                Console.WriteLine("サーバーが切断しました。");
                break;
            }

            //受信したデータを蓄積する
            ms.Write(CresByte, 0, CresSize);
        } while (ns.DataAvailable || CresByte[CresSize - 1] != '\n');           //まだ読み取れるデータがあるか、データの最後が\nでない時は、受信を続ける

        //受信したデータをstringに変換
        string CresStr = enco.GetString(ms.GetBuffer(), 0, (int)ms.Length);
        ms.Close();

        //末尾の\nを削除doubleに変換
        double CresDou = double.Parse(CresStr.TrimEnd('\n'));

        return CresDou;
    }

    //送信List：List<int> をByteに変換し送信 
    public byte[] CsendList(List<int> sendList)
    {
        for (int i = 0; i < sendList.Count; i++)
        {
            //stringをByte型配列に変換
            string strList = (sendList[i] + " ");
            byte[] CsendListByte = enco.GetBytes(strList);

            //データ送信
            ns.Write(CsendListByte, 0, CsendListByte.Length);
            Console.WriteLine(strList);
        }

        return CsendListByte;
    }

    //切断設定
    public void Cclose()
    {
        ns.Close();
        tcp.Close();
        Console.WriteLine("切断しました。");
        Console.WriteLine();
    }
}

class ClientMain
{ 
    //Main
    public static void Main()
    {
        //Client cv;
        var cv = new Client(ipOrHost: "127.0.0.1", port: 2001);

        //入力設定
        var CComent = cv.Comment();
        Console.WriteLine();
        if (cv.sendStr != string.Empty)
        {
            //送信string
            var CSendMsg1 = cv.CsendStr(CComent);
            Console.WriteLine("文字送信：{0}", CSendMsg1);

            //受信double
            var CReceiveMsg = cv.CresceiveData();
            Console.WriteLine("double受信:{0}", CReceiveMsg);

            //送信List
            var SsendList = new List<int>() { 1, 2, 3, 4, 5 };
            Console.WriteLine("List<int>送信：");
            var CSendMsg2 = cv.CsendList(SsendList);
        }

        //切断設定
        Console.WriteLine();
        cv.Cclose();
    }
}