using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace Client
{
    public class Program
    {
        public static void Main()
        {
            var client = new CClient();

            //サーバーに送信するデータを入力してもらう
            Console.WriteLine("文字列を入力し、Enterキーを押してください。");
            string sendMsg = Console.ReadLine();
            
            //何も入力されなかった時（null or 文字数が0）は終了
            if (string.IsNullOrEmpty(sendMsg))
            {
                client.CloseConnection();
                return;
            }

            client.SendStringToServer(sendMsg);

            var resMsg = client.RecieveStringFromServer();

            //末尾の\nを削除して、 double 型としてパース
            double dresMsg = double.Parse(resMsg.TrimEnd('\n'));
            Console.WriteLine("double受信:{0}", dresMsg);

            // 送信用データ List<int> を準備
            var intListToSend = new List<int>
            {
                100,
                200,
                300,
            };

            Console.WriteLine("List<int>送信");
            client.SendIntListToServer(intListToSend);

            client.CloseConnection();

            Console.ReadLine();
        }

    }
}