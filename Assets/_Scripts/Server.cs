using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ServerClient
{
    public int connectionId;
    public string userName;
    public GameObject avatar;
}

public class Server : MonoBehaviour
{
    public Text debugOutput;
    private const int MAX_CONNECTIONS = 30;

    private int port = 5839;

    private int hostId;
    private int webHostId;

    private int reliableChannel;
    private int unreliableChannel;

    private bool isStarted = false;
    private byte error;

    private string userName;

    private List<ServerClient> clients = new List<ServerClient>();

    //User Position Update
    private float lastPositionUpdate = 0.0f;
    private float positionUpdateRate = 0.1f;

    public void Start()
    {
        //StartSever();
    }

    public void StartServer()
    {
        string uName = GameObject.Find("NameInput").GetComponent<InputField>().text;
        if (uName == "")
        {
            debugOutput.text = "Name eingeben";
            return;
        }
        userName = uName;

        ServerClient hostsc = new ServerClient();
        hostsc.connectionId = 0;
        hostsc.userName = userName;
        hostsc.avatar = new GameObject();//GameObject.CreatePrimitive(PrimitiveType.Cylinder); 
        clients.Add(hostsc);

        NetworkTransport.Init();
        ConnectionConfig cc = new ConnectionConfig();

        reliableChannel = cc.AddChannel(QosType.Reliable);
        unreliableChannel = cc.AddChannel(QosType.Unreliable);

        HostTopology topo = new HostTopology(cc, MAX_CONNECTIONS);

        hostId = NetworkTransport.AddHost(topo, port, null);
        webHostId = NetworkTransport.AddWebsocketHost(topo, port, null);
        debugOutput.text = "Server Started";


        isStarted = true;
    }

    private void Update()
    {
        if (!isStarted)
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
            case NetworkEventType.Nothing:
                break;

            case NetworkEventType.ConnectEvent:
                debugOutput.text = "User " + connectionId + " has connected.";
                OnConnection(connectionId);
                break;

            case NetworkEventType.DataEvent:
                string msg = Encoding.Unicode.GetString(recBuffer, 0, dataSize);
                debugOutput.text = "User " + connectionId + " send: " + msg;

                string[] splitData = msg.Split('|');

                switch (splitData[0])
                {
                    case "NAMEIS":
                        OnNameIs(connectionId, splitData[1]);
                        break;

                        
                    case "MYPOSITION":
                        //Update a users position
                        OnMyPosition(connectionId, float.Parse(splitData[1]), float.Parse(splitData[2]), float.Parse(splitData[3]), float.Parse(splitData[4]), float.Parse(splitData[5]), float.Parse(splitData[6]));
                        break;


                        //OBJECT PARAMETER
                    case "POSITIONUPDATE":
                        OnPositionUpdate(connectionId, splitData[1], float.Parse(splitData[2]), float.Parse(splitData[3]), float.Parse(splitData[4]));
                        break;

                    case "ROTATIONUPDATE":
                        OnRotationUpdate(connectionId, splitData[1], float.Parse(splitData[2]), float.Parse(splitData[3]), float.Parse(splitData[4]));
                        break;

                    case "SCALINGUPDATE":
                        OnScalingUpdate(connectionId, splitData[1], float.Parse(splitData[2]), float.Parse(splitData[3]), float.Parse(splitData[4]));
                        break;

                    default:
                        Debug.Log("invalid message: " + msg);
                        break;
                }

                break;

            case NetworkEventType.DisconnectEvent:
                Debug.Log("User " + connectionId + " has disconnected.");
                OnDisconnection(connectionId);
                break;
        }
        //Ask User for their Position
        
        if(Time.time - lastPositionUpdate > positionUpdateRate)
        {
            lastPositionUpdate = Time.time;

            string m = "ASKPOSITION|";
            foreach(ServerClient sc in clients)
            {
                m += sc.connectionId.ToString() + '%' + sc.avatar.transform.position.x.ToString() + '%' + sc.avatar.transform.position.y.ToString() + '%' + sc.avatar.transform.position.z.ToString() + '%' + sc.avatar.transform.rotation.eulerAngles.x.ToString() + '%' + sc.avatar.transform.rotation.eulerAngles.y.ToString() + '%' + sc.avatar.transform.rotation.eulerAngles.z.ToString() + '|';
            }
            m = m.Trim('|');
            Send(m, unreliableChannel, clients);
        }
        /**/
    }
    private void OnConnection(int cnnId)
    {
        ServerClient c = new ServerClient();
        c.connectionId = cnnId;
        c.userName = "TEMP";
        c.avatar = GameObject.CreatePrimitive(PrimitiveType.Capsule);//new GameObject(); 
        c.avatar.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);

        clients.Add(c);

        string msg = "ASKNAME|" + cnnId + '|';
        foreach(ServerClient sc in clients)
            msg += sc.userName + '%' + sc.connectionId + '|';

        msg = msg.Trim('|');

        // ASKNAME|2|Soeren%1|TEMP%2
        debugOutput.text = "Server send: " + msg;
        Send(msg, reliableChannel, cnnId);
    }

    private void OnDisconnection(int cnnId)
    {
        clients.Remove(clients.Find(x => x.connectionId == cnnId));

        Send("DC|" + cnnId, reliableChannel, clients);
    }


    private void OnNameIs(int cnnId, string userName)
    {
        //Link Name to connectionId
        clients.Find(x => x.connectionId == cnnId).userName = userName;

        //tell everyone that someone has connected
        Send("CNN|" + userName + "|" + cnnId, reliableChannel, clients);
    }


    //Set new Position of a soezific User-Avatar
    private void OnMyPosition(int cnnId, float x, float y, float z, float a, float b, float c)
    {
        ServerClient sc = clients.Find(n => n.connectionId == cnnId);
        sc.avatar.transform.position = new Vector3(x, y, z);
        sc.avatar.transform.rotation = Quaternion.Euler(a, b, c);
    }




    /****************** HANDLE POS, ROT AND SCALE UPDATE ***********************/
    //get and send changed POSITION
    private void OnPositionUpdate(int cnnId, string objectname, float x, float y, float z)
    {
        GameObject.Find(objectname).transform.position = new Vector3(x, y, z);

        List<ServerClient> scList = new List<ServerClient>();
        foreach(ServerClient client in clients)
        {
            if(client.connectionId != cnnId)
            {
                scList.Add(client);
            }
        }

        //Send all other users the position Update
        positionUpdate(GameObject.Find(objectname), scList);
    }

    //get and send changed ROTATION
    private void OnRotationUpdate(int cnnId, string objectname, float x, float y, float z)
    {
        GameObject.Find(objectname).transform.rotation = Quaternion.Euler(x, y, z);

        List<ServerClient> scList = new List<ServerClient>();
        foreach (ServerClient client in clients)
        {
            if (client.connectionId != cnnId)
            {
                scList.Add(client);
            }
        }

        //Send all other users the position Update
        rotationUpdate(GameObject.Find(objectname), scList);
    }

    //get and send changed SCALING
    private void OnScalingUpdate(int cnnId, string objectname, float x, float y, float z)
    {
        GameObject.Find(objectname).transform.localScale = new Vector3(x, y, z);

        List<ServerClient> scList = new List<ServerClient>();
        foreach (ServerClient client in clients)
        {
            if (client.connectionId != cnnId)
            {
                scList.Add(client);
            }
        }

        //Send all other users the position Update
        scalingUpdate(GameObject.Find(objectname), scList);
    }







    /**********  SEND POS, ROT AND SCALE UPDATE**************/
    //send the new POSITION of an Object
    public void positionUpdate(GameObject go)
    {
        Vector3 position = go.transform.position;
        string msg = "OBJECTPOSITION|" + go.name + '|' + position.x.ToString() + '|' + position.y.ToString() + '|' + position.z.ToString();
        Send(msg, reliableChannel, clients);
    }
    public void positionUpdate(GameObject go, List<ServerClient> scList)
    {
        Vector3 position = go.transform.position;
        string msg = "OBJECTPOSITION|" + go.name + '|' + position.x.ToString() + '|' + position.y.ToString() + '|' + position.z.ToString();
        Send(msg, reliableChannel, scList);
    }

    //send the new ROTATION of an Object
    public void rotationUpdate(GameObject go)
    {
        Vector3 rotation = go.transform.rotation.eulerAngles;
        string msg = "OBJECTROTATION|" + go.name + '|' + rotation.x.ToString() + '|' + rotation.y.ToString() + '|' + rotation.z.ToString();
        Send(msg, reliableChannel, clients);
    }
    public void rotationUpdate(GameObject go, List<ServerClient> scList)
    {
        Vector3 rotation = go.transform.rotation.eulerAngles;
        string msg = "OBJECTROTATION|" + go.name + '|' + rotation.x.ToString() + '|' + rotation.y.ToString() + '|' + rotation.z.ToString();
        Send(msg, reliableChannel, scList);
    }

    //send the new SCALING of an Object
    public void scalingUpdate(GameObject go)
    {
        Vector3 scale = go.transform.localScale;
        string msg = "OBJECTSCALING|" + go.name + '|' + scale.x.ToString() + '|' + scale.y.ToString() + '|' + scale.z.ToString();
        Send(msg, reliableChannel, clients);
    }
    public void scalingUpdate(GameObject go, List<ServerClient> scList)
    {
        Vector3 scale = go.transform.localScale;
        string msg = "OBJECTSCALING|" + go.name + '|' + scale.x.ToString() + '|' + scale.y.ToString() + '|' + scale.z.ToString();
        Send(msg, reliableChannel, scList);
    }




    //Update My AVATAR Position
    public void updateAvatar(GameObject go)
    {
        ServerClient sc = clients.Find(x => x.connectionId == 0);
        sc.avatar.transform.position = go.transform.position;
        sc.avatar.transform.rotation = Quaternion.Euler(go.transform.rotation.eulerAngles);
    }



    //Send Method: send message to one client
    private void Send(string message, int channelId, int cnnId)
    {
        List<ServerClient> c = new List<ServerClient>();
        c.Add(clients.Find(x => x.connectionId == cnnId));
        Send(message, channelId, c);
    }

    //Send method Overload: send message to every client
    private void Send(string message, int channelId, List<ServerClient> c)
    {
        byte[] msg = Encoding.Unicode.GetBytes(message);
        foreach(ServerClient sc in c)
        {
            if(sc.connectionId != 0)
                NetworkTransport.Send(hostId, sc.connectionId, channelId, msg, message.Length * sizeof(char), out error);
        }
    }

    public bool getIsStarted()
    {
        return isStarted;
    }
}
