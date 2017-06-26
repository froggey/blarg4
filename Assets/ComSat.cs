using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System;
using ProtoBuf;

[ProtoContract]
public class NetworkMessage {
    public enum Type {
        // Server->client, sends player ID.
        Hello = 1,
        // Server->client, sends player ID.
        PlayerJoin = 2,
        // Server->client, sends player ID.
        PlayerLeave = 3,
        // Bidirectional, sends player ID, name and teamID.
        PlayerUpdate = 4,
        // Client->server, sends player ID.
        KickPlayer = 5,
        // Bidirectional, sends levelName and gameSpeed.
        StartGame = 6,
        // Bidirectional, client sends chat, server relays with chat and player ID.
        // Player ID will be -1 for server-generated messages.
        Chat = 7,
        // Client->server. Optionally sends checksum for sync checking.
        Ready = 8,
        // Server->client.
        NextTurn = 9,

        // Game commands.
        CommandStop = 32,
        CommandMove = 33,
        CommandAttack = 34,
        CommandDeploy = 35,
        CommandBuild = 36,
    }

    public NetworkMessage() {}

    public NetworkMessage(Type type) {
        this.type = type;
    }

    public byte[] Serialize() {
        var stream = new MemoryStream();
        stream.WriteByte(0);
        stream.WriteByte(0);
        Serializer.Serialize(stream, this);
        var buffer = stream.ToArray();
        var len = buffer.Length - 2;
        if(len >= 0x10000) {
            throw new System.Exception("Message exceeds maximum packet length!");
        }
        buffer[0] = (byte)(len & 0xFF);
        buffer[1] = (byte)(len >> 8);
        return buffer;
    }

    static public NetworkMessage Deserialize(byte[] buffer) {
        var len = (int)buffer[0] | ((int)buffer[1] << 8);
        var stream = new MemoryStream(buffer, 2, len);
        return Serializer.Deserialize<NetworkMessage>(stream);
    }

    // Message type.
    [ProtoMember(1)]
    public Type type;

    // Sending player ID. Filled in by the server, not valid on client-generated messages.
    [ProtoMember(2)]
    public int playerId;

    [ProtoMember(3)]
    public string playerName;

    [ProtoMember(4)]
    public int teamId;

    [ProtoMember(5)]
    public string chat;

    [ProtoMember(6)]
    public uint checksum;

    // ID of the entity being ordered.
    [ProtoMember(7)]
    public int entityId;

    [ProtoMember(8)]
    public Game.DVector3 position;

    // ID of the entity being targetted.
    [ProtoMember(9)]
    public int targetId;

    [ProtoMember(10)]
    public int buildId;
}

interface IComSatListener {
    void ComSatConnectionStateChanged(ComSat.ConnectionState newState);
}

class ComSat: MonoBehaviour {
    public static ComSat instance { get; private set; }

    string _localPlayerName;
    public string localPlayerName {
        get { return _localPlayerName; }
        set {
            _localPlayerName = value;
        }
    }

    void Awake() {
        if(ComSat.instance == null) {
            ComSat.instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
    }

    List<IComSatListener> listeners = new List<IComSatListener>();

    public void AddListener(IComSatListener l) {
        listeners.Add(l);
    }

    public void RemoveListener(IComSatListener l) {
        listeners.Remove(l);
    }

    public int localPlayerId;

    void HandleMessageClient(NetworkMessage message) {
        switch(message.type) {
        case NetworkMessage.Type.Hello:
            localPlayerId = message.playerId;
            break;
        case NetworkMessage.Type.PlayerJoin:
            break;
        // Server->client, sends player ID.
        case NetworkMessage.Type.PlayerLeave:
            break;
        // Bidirectional, sends player ID, name and teamID.
        case NetworkMessage.Type.PlayerUpdate:
            break;
        // Bidirectional, sends levelName and gameSpeed.
        case NetworkMessage.Type.StartGame:
            break;
        // Bidirectional, client sends chat, server relays with chat and player ID.
        // Player ID will be -1 for server-generated messages.
        case NetworkMessage.Type.Chat:
            break;
        // Server->client.
        case NetworkMessage.Type.NextTurn:
            goForNextTurn = true;
            break;

        // Game commands.
        case NetworkMessage.Type.CommandStop:
        case NetworkMessage.Type.CommandMove:
        case NetworkMessage.Type.CommandAttack:
        case NetworkMessage.Type.CommandDeploy:
        case NetworkMessage.Type.CommandBuild:
            // Commands received after the NextTurn command must be issued with the next batch of
            // commands, not the current batch.
            if(goForNextTurn) {
                pendingFutureCommands.Add(message);
            } else {
                pendingCommands.Add(message);
            }
            break;
        }
    }

    int ticksPerTurn = 3;
    bool goForNextTurn = false;
    List<NetworkMessage> pendingCommands = new List<NetworkMessage>();
    List<NetworkMessage> pendingFutureCommands = new List<NetworkMessage>();
    float timeSlop = 0;
    int remainingTicks = 0;
    int currentTurn = 0;

    public bool checkTeam = false;

    // Commit pending commands to the game world.
    void IssuePendingCommands() {
        foreach(var cmd in pendingCommands) {
            var entity = Game.World.current.IdToEntity(cmd.entityId);
            if(entity == null) {
                continue;
            }
            if(checkTeam && entity.team != cmd.teamId) {
                // Teams don't match, skip.
                continue;
            }
            Game.Entity target = null;
            if(cmd.targetId != 0) {
                target = Game.World.current.IdToEntity(cmd.targetId);
                if(target == null) {
                    continue;
                }
            }
            switch(cmd.type) {
            case NetworkMessage.Type.CommandStop:
                entity.StopCommand();
                break;
            case NetworkMessage.Type.CommandMove:
                entity.MoveCommand(cmd.position);
                break;
            case NetworkMessage.Type.CommandAttack:
                entity.AttackCommand(target);
                break;
            case NetworkMessage.Type.CommandDeploy:
                entity.DeployCommand(cmd.position);
                break;
            case NetworkMessage.Type.CommandBuild:
                entity.BuildCommand(cmd.buildId, cmd.position);
                break;
            }
        }
        pendingCommands.Clear();
        var x = pendingCommands;
        pendingCommands = pendingFutureCommands;
        pendingFutureCommands = x;
    }

    void Update() {
        if(!inGame) {
            return;
        }

        if(mustSendInitialReadyUp) {
            // augh. wait for testshit to create the world.
            var testshit = UnityEngine.Object.FindObjectOfType<Testshit>();
            if(testshit == null || testshit.world == null) {
                return;
            }
            SendReadyUp();
            mustSendInitialReadyUp = false;
        }

        if(fakeLocalReady) {
            var m = new NetworkMessage(NetworkMessage.Type.NextTurn);
            HandleMessageClient(m);
            fakeLocalReady = false;
        }

        timeSlop += Time.deltaTime;
        while(timeSlop > (float)Game.World.deltaTime) {
            timeSlop -= (float)Game.World.deltaTime;
            if(remainingTicks != 0) {
                Game.World.current.Tick();
                remainingTicks -= 1;
            }
            if(remainingTicks == 0 && goForNextTurn) {
                currentTurn += 1;
                IssuePendingCommands();
                SendReadyUp();
                goForNextTurn = false;
                remainingTicks = ticksPerTurn;
            }
        }
    }

    public int fakeLocalTeam = 1;
    public bool fakeLocalReady = false;

    void SendToServer(NetworkMessage message) {
        // server emulation.
        if(message.type == NetworkMessage.Type.Ready) {
            if(fakeLocalReady) {
                Debug.LogWarning("Multiple ready messages");
            }
            fakeLocalReady = true;
            return;
        }
        // only bounce back commands.
        if((int)message.type < 32) {
            return;
        }
        var bytes = message.Serialize();
        var recv_message = NetworkMessage.Deserialize(bytes);

        recv_message.playerId = localPlayerId;
        recv_message.teamId = fakeLocalTeam;

        HandleMessageClient(recv_message);
    }

    // Commands.
    public void SendReadyUp() {
        var message = new NetworkMessage(NetworkMessage.Type.Ready);
        message.checksum = Game.World.current.Checksum();
        Debug.Log("Checksum is " + message.checksum);
        SendToServer(message);
    }

    public void SendStopCommand(int eid) {
        var message = new NetworkMessage(NetworkMessage.Type.CommandStop);
        message.entityId = eid;
        SendToServer(message);
    }

    public void SendMoveCommand(int eid, Game.DVector3 point) {
        var message = new NetworkMessage(NetworkMessage.Type.CommandMove);
        message.entityId = eid;
        message.position = point;
        SendToServer(message);
    }

    public void SendAttackCommand(int eid, int targetEid) {
        var message = new NetworkMessage(NetworkMessage.Type.CommandAttack);
        message.entityId = eid;
        message.targetId = targetEid;
        SendToServer(message);
    }

    public void SendDeployCommand(int eid, Game.DVector3 point) {
        var message = new NetworkMessage(NetworkMessage.Type.CommandDeploy);
        message.entityId = eid;
        message.position = point;
        SendToServer(message);
    }

    public void SendBuildCommand(int eid, int buildId, Game.DVector3 point) {
        var message = new NetworkMessage(NetworkMessage.Type.CommandBuild);
        message.entityId = eid;
        message.buildId = buildId;
        message.position = point;
        SendToServer(message);
    }

    //
    // NETWORKING CRAP.
    //

    public enum ConnectionState {
        Disconnected,
        Connecting,
        Connected,
    }

    ConnectionState _connectionState = ConnectionState.Disconnected;
    public ConnectionState connectionState {
        get { return _connectionState; }
        private set {
            _connectionState = value;
            foreach(var l in listeners) {
                l.ComSatConnectionStateChanged(value);
            }
        }
    }

    public bool isHost { get; private set; }
    public bool inGame { get; private set; }

    bool mustSendInitialReadyUp;

    // SERVER-SPECIFIC NETWORKING CRAP.
    private int nextClientID;
    private Socket listenSocket;

    // Public interface, the lobby calls this to host.
    public void Host(int port) {
        if(connectionState != ConnectionState.Disconnected) {
            Debug.LogError("Tried to host a server when already connected.");
            return;
        }

        var localEP = new IPEndPoint(IPAddress.Any, port);
        listenSocket = new Socket(localEP.Address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        Debug.Log("Listening on " + localEP);

        try {
            listenSocket.Bind(localEP);
            listenSocket.Listen(10);
            //listenSocket.BeginAccept(new AsyncCallback(AsyncClientConnect), listenSocket);
        } catch(Exception e) {
            Debug.LogError("Failed to start server on " + localEP);
            Debug.LogException(e, this);
            listenSocket = null;
            throw;
        }

        isHost = true;
        connectionState = ConnectionState.Connected;

        nextClientID = 1;
    }

    // Returns true if all players have readied in the lobby.
    public bool PlayersAreReady() {
        return true; // host always ready.
    }

    public void StartGame() {
        if(!PlayersAreReady()) {
            return;
        }

        Application.LoadLevel("Game");
        inGame = true;
        mustSendInitialReadyUp = true;
    }

    // CLIENT-SPECIFIC NETWORKING CRAP.
    public void Connect(string host, int port) {
    }

    public void ToggleReady() {
    }

    // Drop from the server and return to the lobby.
    // Kills the server if hosting.
    public void Disconnect() {
        if(connectionState == ConnectionState.Disconnected) {
            return;
        }
        if(isHost) {
            listenSocket.Close();
            listenSocket = null;
        } else {
        }

        connectionState = ConnectionState.Disconnected;
        inGame = false;
        Application.LoadLevel("Lobby");
    }
}
