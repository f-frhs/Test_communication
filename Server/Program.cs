using System;
using System.Globalization;

namespace Server
{
    class Program
    {
        static void Main()
        {
            var server = new CServer();

            var receivingMsg = server.ReceiveMsg();
            Console.WriteLine($"文字受信: {receivingMsg}");

            //double型にキャストできる文字列を送信する
            server.SendMsg((Math.PI).ToString(CultureInfo.InvariantCulture));

            var receivedListString = server.ReceiveIntList();
            Console.WriteLine($"List<int>受信: {receivedListString}");

            //接続を切断する
            server.CloseConnection();
        }
    }
}
