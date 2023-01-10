using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Xml;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class Client : MonoBehaviour
{
    private static string USER_LOGIN_MESSAGE = "1";
    private static string GET_USER_LIST_MESSAGE = "2";
    private static string SEND_USER_LIST_MESSAGE = "3";
    private static string CONNECT_WITH_USER = "4";
    private static string CONNECT_SUCCEED = "5";
    private static string CONNECT_REFUSED = "6";
    private static string CONNECT_FAULT = "7";
    private static string SEND_CARD_ID = "8";

    private Thread thread;
    //��Ϣ��
    [Serializable]
    public class Message
    {
        public string messageType;
        public string jsonContent;
        public void setString(object o)
        {
            jsonContent = JsonConvert.SerializeObject(o);
        }
    }
    
    //�ͻ���������ĵ���
    public static Client clientManager;
    //�ͻ��˵�Socket����
    public static TcpClient client;
    //ip�Ͷ˿�
    public string serverIp;
    public string serverPort;
    public void InitClient()
    {
        //���õ���
        clientManager = GetComponent<Client>();
        //���ͻ��˵�Socket���ӵ�������
        client = new TcpClient();
        client.Connect(IPAddress.Parse(serverIp), Convert.ToInt32(serverPort));
        //����IP�Ͷ˿ڲ�����
        print("���������ӳɹ�");
        //�������߳̽��ܷ���������Ϣ
        thread = new Thread(ReceiveFromServer);
        User user = new User();
        user.userName = GameManager.Gm.userId;
        user.passWord = "";
        thread.Start();
        Message message = new Message();
        message.messageType = "1";
        message.setString(user);
        SendObjectJson(message);
    }
    //���շ���˵���Ϣ
    private void ReceiveFromServer()
    {
        while (true)
        {
            byte[] buffer = new byte[1024];
            int realSize = client.Client.Receive(buffer);
            if (realSize <= 0)
            {
                break;
            }
            string message = Encoding.UTF8.GetString(buffer, 0, realSize);
            print("�յ���Ϣ-->"+message);
            //����Ϣ����
            JObject msgJObject = (JObject)JsonConvert.DeserializeObject(message);
            string messageType = msgJObject["messageType"].ToString();
            //��ȡ�û��б����Ϣ
            if (messageType == SEND_USER_LIST_MESSAGE)
            {
                //����Ϣ�����ݽ���������
                string content = msgJObject["jsonContent"].ToString();
                JArray userJArray = (JArray)JsonConvert.DeserializeObject(content);
                GameManager.Gm.onlineUsers = new List<GameManager.UserItemStruct>();
                //��������ת���ɶ�Ӧ��User����
                for (int i = 0; i < userJArray.Count; i++)
                {
                    //�����û�����״̬
                    string userName = userJArray[i]["userId"].ToString();
                    string status = userJArray[i]["status"].ToString();
                    bool isOnline;
                    if (status == "online") isOnline = true;
                    else isOnline = false;
                    print(userName);
                    print(status);
                    //�����ݴ����б�
                    GameManager.UserItemStruct onlineUserItem = new GameManager.UserItemStruct();
                    onlineUserItem.User = userName;
                    onlineUserItem.IsOnline = isOnline;
                    GameManager.Gm.onlineUsers.Add(onlineUserItem);
                }
                //ͨ���̸߳�����ˢ���б���ʾ
                Loom.QueueOnMainThread((parma)=>
                {
                    GameManager.Gm.RefreshOnlineList();
                },null);
            }
            //�յ��û�����������
            else if (messageType == CONNECT_WITH_USER)
            {
                string opUser = msgJObject["jsonContent"].ToString();
                Loom.QueueOnMainThread((parma)=>
                {
                    DPlayManager.PlayManager.MeetUser(opUser);
                },null);

            }
            //�յ��Է��ܽӵ�����
            else if (messageType == CONNECT_REFUSED)
            {
                Loom.QueueOnMainThread((parma)=>
                {
                    DPlayManager.PlayManager.UserRefuse();
                },null);
            }
            //�յ��Է�ͬ�����ӵ�����
            else if (messageType == CONNECT_SUCCEED)
            {
                string jsonContent = msgJObject["jsonContent"].ToString();
                JObject msgSubJObject = (JObject)JsonConvert.DeserializeObject(jsonContent);
                string randomSeed = msgSubJObject["randomSeed"].ToString();
                int seed = int.Parse(randomSeed);
                Loom.QueueOnMainThread((parma)=>
                {
                    DPlayManager.PlayManager.UserAccept(seed);
                },null);
            }
            //�յ���һ���û��ķ��Ϳ�������
            else if(messageType == SEND_CARD_ID)
            {
                string jsonContent = msgJObject["jsonContent"].ToString();
                JObject msgSubJObject = (JObject)JsonConvert.DeserializeObject(jsonContent);
                string cardId = msgSubJObject["cardId"].ToString();
                print("�Է�ѡ��"+cardId);
                Loom.QueueOnMainThread((parma)=>
                {
                    DPlayManager.PlayManager.FetchCard(cardId,false);
                },null);
            }

        }
    }
    //��Socket���������Ϳ����л��������Json
    public void SendObjectJson(object o)
    {
        string jsonStr = JsonConvert.SerializeObject(o);
        byte[] buffer = Encoding.UTF8.GetBytes(jsonStr);
        client.Client.Send(buffer);
        print("������Ϣ-->"+jsonStr);
    }
    //Send״̬(����)
    [Obsolete]
    private static void SendCallBack(IAsyncResult ar)
    {
        Socket socket = (Socket)ar.AsyncState;
        socket.EndSend(ar);

    }
    //��Socket���������Ϳ����л��������(����)
    [Obsolete]
    public void SendObject(object o)
    {
        MemoryStream memoryStream = new MemoryStream();
        BinaryFormatter binaryFormatter = new BinaryFormatter();
        binaryFormatter.Serialize(memoryStream, o);
        memoryStream.Flush();
        byte[] buffer = new byte[1024];
        memoryStream.Position = 0;
    }
    //�û�������Ϣ��
    [Serializable]
    public class User 
    {
        public string userName;
        public string passWord;
    }
    //�û�ѡ������
    [Serializable]
    public class CardInformation
    {
        public string opUser;
        public string cardId;
    }
    //�������
    public class UserNameAndRandomSeed
    {
        public string opUser;
        public string randomSeed;
    }
    //����������������ȡ�û��б�
    public void GetUserList()
    {
        Message message = new Message();
        message.messageType = GET_USER_LIST_MESSAGE;
        message.jsonContent = "";
        SendObjectJson(message);
    }
    //�����û���������
    public void ConnectWithUser(string userId)
    {
        Message message = new Message();
        message.messageType = CONNECT_WITH_USER;
        message.jsonContent = userId;
        SendObjectJson(message);
    }
    //���;ܽ��û�������
    public void RefuseUser(string user)
    {
        Message message = new Message();
        message.messageType = CONNECT_REFUSED;
        message.jsonContent = user;
        SendObjectJson(message);
    }
    //���ͽ����û�������
    public void AcceptUser(string user,int randomSeed)
    {
        Message message = new Message();
        message.messageType = CONNECT_SUCCEED;
        UserNameAndRandomSeed userNameAndRandomSeed = new UserNameAndRandomSeed();
        userNameAndRandomSeed.opUser = user;
        userNameAndRandomSeed.randomSeed = randomSeed.ToString();
        message.setString(userNameAndRandomSeed);
        SendObjectJson(message);
    }
    //�ر�Socketͨ��
    public void CloseSocket()
    {
        thread.Abort();
        client.Close();
    }
    //��ѡ��Ŀ�����Ϣ���͸��Է�
    public void SendCardId(string cardId)
    {
        if (DPlayManager.PlayManager.status != DPlayManager.WaitingStatus.OnPlaying) return;
        string opUser = DPlayManager.PlayManager.opUser;
        CardInformation cardInformation = new CardInformation();
        cardInformation.cardId = cardId;
        cardInformation.opUser = opUser;
        Message message = new Message();
        message.messageType = SEND_CARD_ID;
        message.setString(cardInformation);
        SendObjectJson(message);
    }
}
