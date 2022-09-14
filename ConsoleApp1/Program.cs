// See https://aka.ms/new-console-template for more information
using CommunicationLib.Core.Network;
using System.Net;

DdkTcpClient ddkTcpClient = new DdkTcpClient(123);
ddkTcpClient.Connect(new System.Net.IPEndPoint(IPAddress.Parse("127.0.0.1"), 2333));

while(true)
{
    Thread.Sleep(1000);
}

