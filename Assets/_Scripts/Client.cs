using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class User
{
    public string userName;
    public GameObject avatar;
    public int connectionId;
}

public class Client : MonoBehaviour
{
    public Text debugOutput;

    public Button connectButton;
    public Button disconnectButton;
    public Text connectionInfo;

    private const int MAX_CONNECTIONS = 30;

    private int port = 5839;

    private int hostId;
    private int webHostId;

    private int reliableChannel;
    private int unreliableChannel;

    private int connectionId;
    private int ourClientId;

    private float connectionTime;
    private bool isConnected = false;
    private bool isStarted = false;
    private byte error;

    private string userName;
    private string connectionIp;    //"192.168.112.206" 192.168.43.48

    //public GameObject playerPrefab;
    public List<User> users = new List<User>();

    public void Connect()
    {
        string cIP = GameObject.Find("IpInput").GetComponent<InputField>().text;
        if (cIP == "")
        {
            debugOutput.text = "IP eingeben";
            return;
        }
        connectionIp = cIP;

        string uName = GameObject.Find("NameInput").GetComponent<InputField>().text;
        if(uName == "")
        {
            debugOutput.text = "Name eingeben";
            return;
        }
        userName = uName;

        NetworkTransport.Init();
        ConnectionConfig cc = new ConnectionConfig();

        reliableChannel = cc.AddChannel(QosType.Reliable);
        unreliableChannel = cc.AddChannel(QosType.Unreliable);

        HostTopology topo = new HostTopology(cc, MAX_CONNECTIONS);

        hostId = NetworkTransport.AddHost(topo, 0);
        connectionId = NetworkTransport.Connect(hostId, connectionIp, port, 0, out error);

        debugOutput.text = "Connection Request sent";

        connectionTime = Time.time;
        isConnected = true;
    }

    public void Disconnect()
    {

        //Disconnect
        connectButton.gameObject.SetActive(true);
        disconnectButton.gameObject.SetActive(false);
        connectionInfo.gameObject.SetActive(false);
        isConnected = false;
    }

    private void Update()
    {
        if (!isConnected)
            return;

        int recHostId;
        int connectionId;
        int channelId;
        byte[] recBuffer = new byte[1024];
        int bufferSize = 1024;
        int dataSize;
        byte error;
        NetworkEventType recData = NetworkTransport.Receive(out recHostId, out connectionId, out channelId, recBuffer, bufferSize, out dataSize, out error);
        switch (recData)
        {
            case NetworkEventType.DataEvent:
                string msg = Encoding.Unicode.GetString(recBuffer, 0, dataSize);
                debugOutput.text = "received:" + msg;
                Debug.Log(msg);
                string[] splitData = msg.Split('|');

                switch(splitData[0])
                {
                    case "ASKNAME":
                        //Server Asks for your Name
                        OnAskName(splitData);
                        break;

                    case "CNN":
                        //Someone Connected
                        CreateOtherUser(splitData[1], int.Parse(splitData[2]));
                        break;

                    case "DC":
                        //Someone Disconnected
                        UserDisconnected(int.Parse(splitData[1]));
                        break;

                    case "AKSPOSITION":
                        //not called yet
                        OnAskPosition(splitData);
                        break;

                    case "OBJECTPOSITION":
                        OnObjectPosition(splitData[1], float.Parse(splitData[2]), float.Parse(splitData[3]), float.Parse(splitData[4]));
                        break;

                    case "OBJECTROTATION":
                        OnObjectRotation(splitData[1], float.Parse(splitData[2]), float.Parse(splitData[3]), float.Parse(splitData[4]));
                        break;

                    case "OBJECCTSCALING":
                        OnObjectScaling(splitData[1], float.Parse(splitData[2]), float.Parse(splitData[3]), float.Parse(splitData[4]));
                        break;

                    default:
                        Debug.Log("invalid massage: " + msg);
                        break;
                }
                    

                break;

            case NetworkEventType.BroadcastEvent:
                break;
        }
    }

    //Tell server your name and get all the other users
    private void OnAskName (string[] data)
    {
        //set this clients ID
        ourClientId = int.Parse(data[1]);

        //send your name to the Server
        Send("NAMEIS|" + userName, reliableChannel);

        //create the other Users (maybe not important)
        for(int i = 2; i < data.Length -1; i++)
        {
            string[] d = data[i].Split('%');
            CreateOtherUser(d[0], int.Parse(d[1]));
        }
    }

    //Not called yet
    private void OnAskPosition(string[] data)
    {
        if (!isStarted)
            return;

        for(int i = 1; i < data.Length-1; i++)
        {
            string[] d = data[i].Split('%');

            //Prevent from updating your Position
            if (ourClientId != int.Parse(d[0]))
            {
                Vector3 position = Vector3.zero;
                position.x = float.Parse(d[1]);
                position.y = float.Parse(d[2]);
                position.z = float.Parse(d[3]);
                users[int.Parse(d[0])].avatar.transform.position = position;
            }
        }

        //Send my Position
        Vector3 myPosition = users.Find(x => x.connectionId == ourClientId).avatar.transform.position;
        string msg = "MYPOSITION|" + myPosition.x.ToString() + '|' + myPosition.y.ToString() + '|' + myPosition.z.ToString();
        Send(msg, unreliableChannel);
    }

    //Show other users and Check own connection
    private void CreateOtherUser (string userName, int cnnId)
    {
        //showing other user as GameObject?
        //GameObject go = Instantiate.(playerPrefab) as GameObject;

        User u = new User();
        u.userName = userName;
        u.connectionId = cnnId;
        //u.avatar = go;

        //add User to User List
        users.Add(u);

        if(cnnId == ourClientId)
        {
            isStarted = true;
            //Connect button to disconnect
            connectButton.gameObject.SetActive(false);
            disconnectButton.gameObject.SetActive(true);
            connectionInfo.gameObject.SetActive(true);
            connectionInfo.text = "Connected to: " + users[0].userName;
        }
    }

    private void UserDisconnected (int cnnId)
    {
        //Destroy(users.Find(x => x.connectionId == cnnId).avatar);
        users.Remove(users.Find(x => x.connectionId == cnnId));
    }

    //send the new POSITION of an Object
    public void positionUpdate (GameObject go)
    {
        Vector3 position = go.transform.position;
        string msg = "POSITIONUPDATE|" + go.name + '|' + position.x.ToString() + '|' + position.y.ToString() + '|' + position.z.ToString();
        Send(msg, reliableChannel);
    }

    //send the new ROTATION of an Object
    public void rotationUpdate(GameObject go)
    {
        Vector3 rotation = go.transform.rotation.eulerAngles;
        string msg = "ROTATIONUPDATE|" + go.name + '|' + rotation.x.ToString() + '|' + rotation.y.ToString() + '|' + rotation.z.ToString();
        Send(msg, reliableChannel);
    }

    //send the new SCALING of an Object
    public void scalingUpdate(GameObject go)
    {
        Vector3 scale = go.transform.localScale;
        string msg = "SCALINGUPDATE|" + go.name + '|' + scale.x.ToString() + '|' + scale.y.ToString() + '|' + scale.z.ToString();
        Send(msg, reliableChannel);
    }

    //Update the POSITION of an Object
    private void OnObjectPosition (string objectname, float x, float y, float z)
    {
        GameObject.Find(objectname).transform.position = new Vector3(x, y, z);
    }

    //Update the ROTATION of an Object
    private void OnObjectRotation(string objectname, float x, float y, float z)
    {
        GameObject.Find(objectname).transform.rotation = Quaternion.Euler(x, y, z);
    }

    //Update the SCALING of an Object
    private void OnObjectScaling(string objectname, float x, float y, float z)
    {
        GameObject.Find(objectname).transform.localScale = new Vector3(x, y, z);
    }
    //Send a  massege to the server
    private void Send(string massage, int channelId)
    {
        debugOutput.text = "I send: " + massage;
        byte[] msg = Encoding.Unicode.GetBytes(massage);
        NetworkTransport.Send(hostId, connectionId, channelId, msg, massage.Length * sizeof(char), out error);
    }
    public bool getIsStarted()
    {
        return isStarted;
    }
}
