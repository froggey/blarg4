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
        // Bidirectional, sends player ID, name and team ID.
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

    [ProtoMember(11)]
    public bool lobbyReady;

    [ProtoMember(12)]
    public int factionId;
}

[Serializable] // So it shows up in the inspector.
public class Player {
    public Player(int id, bool localPlayer) {
        this.id = id;
        this.team = -1;
        this.name = "<Player " + id + ">";
        this.localPlayer = localPlayer;
    }
    public int id;
    public int team;
    public string name;
    public bool localPlayer;
    public bool lobbyReady;
    public int faction;
}

[Serializable] // So it shows up in the inspector.
public class NetworkClient {
    public NetworkClient(int id, Socket socket) {
        this.id = id;
        this.socket = socket;
        this.buffer = new byte[0x10000 + 2]; // max message size plus length

        this.team = -1;
        this.name = "<Player " + id + ">";
    }

    public int id; // ID for communication purposes.
    public int team;
    public string name;
    public bool lobbyReady;
    public int faction;
    public bool ready;
    public uint lastChecksum;
    public string lastState;

    public Socket socket;
    [HideInInspector] // Hide the giant buffer from the inspector.
    public byte[] buffer; // Receive buffer.
    public int messageLength; // Size of current message being received.
    public int bytesRead; // Total number of message bytes received so far (including header).
    public IAsyncResult activeReceive; // Currently active async receive call.
}

interface IComSatListener {
    void ComSatConnectionStateChanged(ComSat.ConnectionState newState);
    void ComSatPlayerJoined(Player player);
    void ComSatPlayerChanged(Player player);
    void ComSatPlayerLeft(Player player);
}

class ComSat: MonoBehaviour {
    public static ComSat instance { get; private set; }

    string _localPlayerName;
    public string localPlayerName {
        get { return _localPlayerName; }
        set {
            UpdateLocalPlayerName(value);
        }
    }

    void Awake() {
        if(ComSat.instance == null) {
            ComSat.instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }

        _localPlayerName = Environment.UserName;
    }

    List<IComSatListener> listeners = new List<IComSatListener>();

    public void AddListener(IComSatListener l) {
        listeners.Add(l);
    }

    public void RemoveListener(IComSatListener l) {
        listeners.Remove(l);
    }

    int ticksPerTurn = 3;
    bool goForNextTurn = false;
    List<NetworkMessage> pendingCommands = new List<NetworkMessage>();
    List<NetworkMessage> pendingFutureCommands = new List<NetworkMessage>();
    float timeSlop = 0;
    int remainingTicks = 0;
    int currentTurn = 0;

    public bool checkTeam = true;

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

    uint expectedChecksum;

    void Update() {
        InvokeUnityActions();

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

        timeSlop += Time.deltaTime;
        while(timeSlop > (float)Game.World.deltaTime) {
            timeSlop -= (float)Game.World.deltaTime;
            if(isHost && connectedClients.All(c => c.ready)) {
                // Verify checksums.
                foreach(var c in connectedClients) {
                    if(c.lastChecksum != localClient.lastChecksum) {
                        Debug.LogError("Checksum mismatch beween client " + c.id + "(" + c.name + ") and local client! Expected " + localClient.lastChecksum + ", got " + c.lastChecksum);
                    }
                }
                var m = new NetworkMessage(NetworkMessage.Type.NextTurn);
                SendMessageToAll(m);
                foreach(var client in connectedClients) {
                    client.ready = false;
                }
            }

            if(remainingTicks != 0) {
                Game.World.current.Tick();
                remainingTicks -= 1;
            }
            if(remainingTicks == 0 && goForNextTurn) {
                currentTurn += 1;
                IssuePendingCommands();
                SendReadyUp();
                expectedChecksum = Game.World.current.Checksum();
                goForNextTurn = false;
                remainingTicks = ticksPerTurn;
            }
        }
    }

    void SendReadyUp() {
        var message = new NetworkMessage(NetworkMessage.Type.Ready);
        message.checksum = Game.World.current.Checksum();
        SendMessageToServer(message);
    }

    // Commands.
    public void SendStopCommand(int eid) {
        var message = new NetworkMessage(NetworkMessage.Type.CommandStop);
        message.entityId = eid;
        SendMessageToServer(message);
    }

    public void SendMoveCommand(int eid, Game.DVector3 point) {
        var message = new NetworkMessage(NetworkMessage.Type.CommandMove);
        message.entityId = eid;
        message.position = point;
        SendMessageToServer(message);
    }

    public void SendAttackCommand(int eid, int targetEid) {
        var message = new NetworkMessage(NetworkMessage.Type.CommandAttack);
        message.entityId = eid;
        message.targetId = targetEid;
        SendMessageToServer(message);
    }

    public void SendDeployCommand(int eid, Game.DVector3 point) {
        var message = new NetworkMessage(NetworkMessage.Type.CommandDeploy);
        message.entityId = eid;
        message.position = point;
        SendMessageToServer(message);
    }

    public void SendBuildCommand(int eid, int buildId, Game.DVector3 point) {
        var message = new NetworkMessage(NetworkMessage.Type.CommandBuild);
        message.entityId = eid;
        message.buildId = buildId;
        message.position = point;
        SendMessageToServer(message);
    }

    // The async networking stuff uses this to run in a Unity-safe way.
    // These callbacks get run the next time Unity calls Update.
    List<Action> pendingUnityActions = new List<Action>();

    void AddUnityAction(Action action) {
        lock(pendingUnityActions) {
            pendingUnityActions.Add(action);
        }
    }
    void InvokeUnityActions() {
        List<Action> actions;
        lock(pendingUnityActions) {
            actions = new List<Action>(pendingUnityActions);
            pendingUnityActions.Clear();
        }
        foreach(var act in actions) {
            try {
                act();
            } catch(Exception e) {
                UnityEngine.Debug.LogException(e, null);
            }
        }
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
    int nextClientId;
    Socket listenSocket;
    NetworkClient localClient;
    // Public so it shows up in the inspector.
    public List<NetworkClient> connectedClients = new List<NetworkClient>();

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
            listenSocket.BeginAccept(new AsyncCallback(AsyncClientConnect), listenSocket);
        } catch(Exception e) {
            Debug.LogError("Failed to start server on " + localEP);
            Debug.LogException(e, this);
            listenSocket = null;
            throw;
        }

        isHost = true;
        connectionState = ConnectionState.Connected;

        nextClientId = 1;
        // Create client for local connection.
        localClient = new NetworkClient(0, null);
        localClient.name = localPlayerName;
        connectedClients.Add(localClient);
        var hello = new NetworkMessage(NetworkMessage.Type.Hello);
        hello.playerId = localClient.id;
        SendMessageToClient(localClient, hello);
        var newJoin = new NetworkMessage(NetworkMessage.Type.PlayerJoin);
        newJoin.playerId = localClient.id;
        newJoin.playerName = localPlayerName;
        newJoin.teamId = localClient.team;
        newJoin.factionId = localClient.faction;
        newJoin.lobbyReady = localClient.lobbyReady;
        SendMessageToAll(newJoin);
        OnConnected();
    }

    // Called when a client connects to this server.
    // Not called for the local client!
    // Async, may be called on another thread!
    void AsyncClientConnect(IAsyncResult result) {
        // listener may have been closed and listenSocket nulled out.
        var serverSocket = result.AsyncState as Socket;
        try {
            var socket = serverSocket.EndAccept(result);
            AddUnityAction(() => {
                    OnClientConnect(socket);
                });
        } catch(ObjectDisposedException) {
            // Connection closed, don't care no more.
            return;
        } catch(Exception e) {
            AddUnityAction(() => { UnityEngine.Debug.LogError("Failed to accept incoming connection: " + e); });
        }

        // Restart the accept.
        try {
            serverSocket.BeginAccept(new AsyncCallback(AsyncClientConnect), serverSocket);
        } catch(Exception e) {
            AddUnityAction(() => { UnityEngine.Debug.LogError("Failed when starting new accept process: " + e); });
        }
    }

    // Called when reading the length header from the client.
    // Async, may be called on another thread!
    void AsyncReceiveClientHeader(IAsyncResult info) {
        var client = info.AsyncState as NetworkClient;

        try {
            var bytesRead = client.socket.EndReceive(info);
            if(bytesRead == 0) {
                // Connection closed.
                AddUnityAction(() => { OnClientDisconnect(client); });
                return;
            }
            client.bytesRead += bytesRead;
            if(client.bytesRead == 2) {
                client.messageLength = (int)client.buffer[0] | ((int)client.buffer[1] << 8);
                client.bytesRead = 0;
                // Read the whole header, read the payload.
                client.activeReceive = client.socket.BeginReceive(client.buffer, 2, client.messageLength, SocketFlags.None, AsyncReceiveClientMessage, client);
            } else {
                // Short read. restart.
                client.activeReceive = client.socket.BeginReceive(client.buffer, client.bytesRead, 2 - client.bytesRead,
                                                                  SocketFlags.None, AsyncReceiveClientHeader, client);
            }
        } catch(ObjectDisposedException) {
            // Socket closed.
            AddUnityAction(() => {
                    OnClientDisconnect(client);
                });
        } catch(Exception e) {
            AddUnityAction(() => {
                    UnityEngine.Debug.LogException(e, this);
                    OnClientDisconnect(client);
                });
        }
    }

    // Called when reading the length header from the client.
    // Async, may be called on another thread!
    void AsyncReceiveClientMessage(IAsyncResult info) {
        var client = info.AsyncState as NetworkClient;

        try {
            var bytesRead = client.socket.EndReceive(info);
            if(bytesRead == 0) {
                // Connection closed.
                AddUnityAction(() => { OnClientDisconnect(client); });
                return;
            }
            client.bytesRead += bytesRead;
            if(client.bytesRead == client.messageLength) {
                var message = NetworkMessage.Deserialize(client.buffer);
                AddUnityAction(() => { HandleMessageServer(message, client); });
                // Finished reading payload, start reading the next header.
                StartAsyncClientRead(client);
            } else {
                // Short read. restart.
                client.activeReceive = client.socket.BeginReceive(client.buffer, 2 + client.bytesRead, client.messageLength - client.bytesRead,
                                                                  SocketFlags.None, AsyncReceiveClientMessage, client);
            }
        } catch(ObjectDisposedException) {
            // Socket closed.
            AddUnityAction(() => {
                    OnClientDisconnect(client);
                });
        } catch(Exception e) {
            AddUnityAction(() => {
                    UnityEngine.Debug.LogException(e, this);
                    OnClientDisconnect(client);
                });
        }
    }

    void StartAsyncClientRead(NetworkClient client) {
        client.bytesRead = 0;
        client.activeReceive = client.socket.BeginReceive(client.buffer, 0, 2, SocketFlags.None, AsyncReceiveClientHeader, client);
    }

    void OnClientConnect(Socket socket) {
        Debug.Log("Connection from " + socket);
        var client = new NetworkClient(nextClientId, socket);
        nextClientId += 1;
        connectedClients.Add(client);
        StartAsyncClientRead(client);

        Debug.Log("Client " + client.id + " connected from " + socket);

        var hello = new NetworkMessage(NetworkMessage.Type.Hello);
        hello.playerId = client.id;
        SendMessageToClient(client, hello);
        // Notify all players of the client joining.
        var newJoin = new NetworkMessage(NetworkMessage.Type.PlayerJoin);
        newJoin.playerId = client.id;
        newJoin.playerName = client.name;
        newJoin.teamId = client.team;
        newJoin.factionId = localClient.faction;
        newJoin.lobbyReady = client.lobbyReady;
        SendMessageToAll(newJoin);

        // Send it details of other players.
        foreach(var p in connectedClients) {
            if(p == client) {
                continue;
            }
            var join = new NetworkMessage(NetworkMessage.Type.PlayerJoin);
            join.playerId = p.id;
            join.playerName = p.name;
            join.teamId = p.team;
            join.factionId = p.faction;
            join.lobbyReady = p.lobbyReady;
            SendMessageToClient(client, join);
        }
    }

    void OnClientDisconnect(NetworkClient client) {
        Debug.Log("Client " + client.id + " disconnected.");
        connectedClients.Remove(client);
        var update = new NetworkMessage(NetworkMessage.Type.PlayerLeave);
        update.playerId = client.id;
        SendMessageToAll(update);
    }

    void AsyncSendClientComplete(IAsyncResult info) {
        try {
            var client = info.AsyncState as NetworkClient;
            client.socket.EndSend(info);
        } catch(Exception e) {
            AddUnityAction(() => { UnityEngine.Debug.LogException(e, this); });
        }
    }

    void SendMessageToClient(NetworkClient client, NetworkMessage message) {
        //Debug.Log("Send message " + message.type + " to client " + client.id);
        if(client == localClient) {
            // Defer this.
            AddUnityAction(() => { HandleMessageClient(message); });
        } else {
            var buffer = message.Serialize();
            client.socket.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, AsyncSendClientComplete, client);
        }
    }

    void SendMessageToAll(NetworkMessage message) {
        //Debug.Log("Send message " + message.type + " to all clients");
        foreach(var client in connectedClients) {
            SendMessageToClient(client, message);
        }
    }

    void ServerPlayerUpdate(NetworkMessage message, NetworkClient client) {
        if(message.playerId != client.id && !isHost) {
            return;
        }
        // Find the real client.
        client = connectedClients.Single(c => c.id == message.playerId);
        client.lobbyReady = message.lobbyReady;
        if(message.playerName != null) {
            // TODO: Sanitize names.
            // Nothing ending in "(n)" to avoid conflicting with duplicates.
            // No empty names, etc.
            var new_name = message.playerName;
            var duplicates = 0;
            foreach(var p in connectedClients) {
                if(p != client && p.name == new_name) {
                    duplicates += 1;
                }
            }
            if(duplicates != 0) {
                new_name = new_name + " (" + duplicates + ")";
            }
            message.playerName = new_name;
            client.name = new_name;
        }
        if(message.teamId == -1 ||
           (message.teamId >= 1 && message.teamId <= 4)) {
            client.team = message.teamId;
        } else {
            message.teamId = 0;
        }
        if(message.factionId >= 1 && message.factionId <= 2) {
            client.faction = message.factionId;
        } else {
            message.factionId = 0;
        }
        SendMessageToAll(message);
    }

    void HandleMessageServer(NetworkMessage message, NetworkClient client) {
        switch(message.type) {
        case NetworkMessage.Type.PlayerUpdate:
            ServerPlayerUpdate(message, client);
            break;
        case NetworkMessage.Type.StartGame:
            if(client == localClient && !inGame && PlayersAreLobbyReady()) {
                SendMessageToAll(message);
            }
            break;
        case NetworkMessage.Type.Chat:
            break;
        case NetworkMessage.Type.Ready:
            if(client.ready) {
                Debug.LogWarning("Multiple ready messages from client " + client.id);
            }
            client.lastChecksum = message.checksum;
            client.ready = true;
            break;

        // Game commands.
        case NetworkMessage.Type.CommandStop:
        case NetworkMessage.Type.CommandMove:
        case NetworkMessage.Type.CommandAttack:
        case NetworkMessage.Type.CommandDeploy:
        case NetworkMessage.Type.CommandBuild:
            // Commands received after the NextTurn command must be issued with the next batch of
            // commands, not the current batch.
            message.playerId = client.id;
            message.teamId = client.team;
            SendMessageToAll(message);
            break;
        }
    }

    // Returns true if all players have readied in the lobby.
    public bool PlayersAreLobbyReady() {
        foreach(var client in connectedClients) {
            if(!client.lobbyReady) {
                return false;
            }
        }
        return true;
    }

    public void StartGame() {
        if(!isHost || !PlayersAreLobbyReady() || inGame) {
            return;
        }

        var m = new NetworkMessage(NetworkMessage.Type.StartGame);
        SendMessageToServer(m);
    }

    void DoStartGame() {
        SceneManager.LoadScene("Game");
        inGame = true;
        mustSendInitialReadyUp = true;
    }

    void OnDestroy() {
        if(instance != this) {
            return;
        }
        instance = null;
        Disconnect();
    }

    void OnApplicationQuit() {
        Disconnect();
    }

    // CLIENT-SPECIFIC NETWORKING CRAP.
    Socket clientSocket;
    int clientMessageLength;
    int clientBytesRead;
    byte[] clientBuffer = new byte[0x10000 + 2];

    public int localPlayerId;
    public List<Player> players;
    public Player localPlayer {
        get {
            foreach(var p in players) {
                if(p.id == localPlayerId) {
                    return p;
                }
            }
            return null;
        }
    }

    public void Connect(string host, int port) {
        if(isHost) {
            return;
        }

        if(connectionState != ConnectionState.Disconnected) {
            Debug.LogError("Tried to connect when already connected.");
            return;
        }

        clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        try {
            clientSocket.BeginConnect(host, port, AsyncConnect, clientSocket);
            isHost = false;
            connectionState = ConnectionState.Connecting;
        } catch(Exception) {
            clientSocket = null;
            throw;
        }
    }

    public void ToggleReady() {
        var update = new NetworkMessage(NetworkMessage.Type.PlayerUpdate);
        update.playerId = localPlayer.id;
        update.lobbyReady = !localPlayer.lobbyReady;
        SendMessageToServer(update);
    }

    void AsyncConnect(IAsyncResult info) {
        try {
            var socket = info.AsyncState as Socket;
            socket.EndConnect(info);
            AddUnityAction(() => {
                    OnConnected();
                    clientBytesRead = 0;
                    clientSocket.BeginReceive(clientBuffer, 0, 2, SocketFlags.None, AsyncReceiveServerHeader, socket);
                });
        } catch(Exception e) {
            AddUnityAction(() => {
                    UnityEngine.Debug.LogException(e, this);
                    connectionState = ConnectionState.Disconnected;
                    clientSocket = null;
                });
        }
    }

    void AsyncSendServerComplete(IAsyncResult info) {
        try {
            var socket = info.AsyncState as Socket;
            socket.EndSend(info);
        } catch(Exception e) {
            AddUnityAction(() => {
                    UnityEngine.Debug.LogException(e, this);
                    OnDisconnected();
                });
        }
    }

    void AsyncReceiveServerHeader(IAsyncResult info) {
        try {
            var socket = info.AsyncState as Socket;
            var bytesRead = socket.EndReceive(info);
            if(bytesRead == 0) {
                AddUnityAction(() => OnDisconnected());
                return;
            }
            clientBytesRead += bytesRead;
            if(clientBytesRead == 2) {
                clientMessageLength = (int)clientBuffer[0] | ((int)clientBuffer[1] << 8);
                clientBytesRead = 0;
                socket.BeginReceive(clientBuffer, 2, clientMessageLength, SocketFlags.None, AsyncReceiveServerMessage, socket);
            } else {
                socket.BeginReceive(clientBuffer, clientBytesRead, 2 - clientBytesRead,
                                    SocketFlags.None, AsyncReceiveServerHeader, socket);
            }
        } catch(ObjectDisposedException) {
            // Socket closed.
            AddUnityAction(() => {
                    OnDisconnected();
                });
        } catch(Exception e) {
            AddUnityAction(() => {
                    UnityEngine.Debug.LogException(e, this);
                    OnDisconnected();
                });
        }
    }

    void AsyncReceiveServerMessage(IAsyncResult info) {
        try {
            var socket = info.AsyncState as Socket;
            var bytesRead = socket.EndReceive(info);
            if(bytesRead == 0) {
                AddUnityAction(() => { OnDisconnected(); });
                return;
            }

            clientBytesRead += bytesRead;
            if(clientBytesRead == clientMessageLength) {
                var message = NetworkMessage.Deserialize(clientBuffer);
                AddUnityAction(() => { HandleMessageClient(message); });
                clientBytesRead = 0;
                socket.BeginReceive(clientBuffer, 0, 2, SocketFlags.None, AsyncReceiveServerHeader, socket);
            } else {
                socket.BeginReceive(clientBuffer, 2 + clientBytesRead, clientMessageLength - clientBytesRead,
                                    SocketFlags.None, AsyncReceiveServerMessage, socket);
            }
        } catch(ObjectDisposedException) {
            // Socket closed.
            AddUnityAction(() => {
                    OnDisconnected();
                });
        } catch(Exception e) {
            AddUnityAction(() => {
                    UnityEngine.Debug.LogException(e, this);
                    OnDisconnected();
                });
        }
    }


    void HandleMessageClient(NetworkMessage message) {
        switch(message.type) {
        case NetworkMessage.Type.Hello:
            localPlayerId = message.playerId;
            break;
        case NetworkMessage.Type.PlayerJoin:
            PlayerUpdate(message, true);
            break;
        case NetworkMessage.Type.PlayerLeave:
            PlayerLeave(message.playerId);
            break;
        case NetworkMessage.Type.PlayerUpdate:
            PlayerUpdate(message, false);
            break;
        // Bidirectional, sends levelName and gameSpeed.
        case NetworkMessage.Type.StartGame:
            DoStartGame();
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

    // Called by clients when they connect to a server.
    void OnConnected() {
        Debug.Log("Connected to server.");
        players = new List<Player>();
        connectionState = ConnectionState.Connected;
    }

    void OnDisconnected() {
        Debug.Log("Disconnected from server.");
        Disconnect();
    }

    Player GetPlayerById(int id) {
        return players.Single(p => p.id == id);
    }

    int PickUnusedTeam() {
        var teams = new List<int>();
        for(int team = 1; team <= 4; team += 1) {
            teams.Add(team);
        }
        foreach(var p in players) {
            teams.Remove(p.team);
        }
        if(teams.Count == 0) {
            return -1;
        } else {
            return teams[0];
        }
    }

    void PlayerUpdate(NetworkMessage message, bool isJoin) {
        if(isJoin) {
            Debug.Log("Player " + message.playerId + " joined");
            if(message.playerId == localPlayerId) {
                var update = new NetworkMessage(NetworkMessage.Type.PlayerUpdate);
                update.playerId = message.playerId;
                update.playerName = localPlayerName;
                SendMessageToServer(update);
            }

            var newp = new Player(message.playerId, message.playerId == localPlayerId);
            if(isHost) {
                // Automatically assign a team.
                var update = new NetworkMessage(NetworkMessage.Type.PlayerUpdate);
                update.playerId = newp.id;
                update.factionId = 1;
                update.teamId = PickUnusedTeam();
                SendMessageToServer(update);
            }
            players.Add(newp);
            players.Sort((p1, p2) => p1.id - p2.id);
            foreach(var l in listeners) {
                l.ComSatPlayerJoined(newp);
            }
        }

        var p = GetPlayerById(message.playerId);
        if(message.playerName != null) {
            p.name = message.playerName;
            if(message.playerId == localPlayerId) {
                _localPlayerName = message.playerName;
            }
        }
        if(message.teamId != 0) {
            p.team = message.teamId;
        }
        if(message.factionId != 0) {
            p.faction = message.factionId;
        }
        p.lobbyReady = message.lobbyReady;

        foreach(var l in listeners) {
            l.ComSatPlayerChanged(p);
        }
    }

    void PlayerLeave(int id) {
        Debug.Log("Player " + id + " left");
        if(id == localPlayerId) {
            Debug.LogWarning("Saw myself leave?");
        }
        var player = GetPlayerById(id);
        foreach(var l in listeners) {
            l.ComSatPlayerLeft(player);
        }
        players.Remove(player);
    }

    public void SetPlayerTeam(Player player, int team) {
        var update = new NetworkMessage(NetworkMessage.Type.PlayerUpdate);
        update.playerId = player.id;
        update.teamId = team;
        SendMessageToServer(update);
    }

    public void SetPlayerFaction(Player player, int faction) {
        var update = new NetworkMessage(NetworkMessage.Type.PlayerUpdate);
        update.playerId = player.id;
        update.factionId = faction;
        SendMessageToServer(update);
    }

    void UpdateLocalPlayerName(string name) {
        if(connectionState == ConnectionState.Connected) {
            var update = new NetworkMessage(NetworkMessage.Type.PlayerUpdate);
            update.playerName = name;
            SendMessageToServer(update);
        } else {
            _localPlayerName = name;
        }
    }

    void SendMessageToServer(NetworkMessage message) {
        //Debug.Log("Send message " + message.type + " to server");
        if(isHost) {
            // Defer this.
            AddUnityAction(() => { HandleMessageServer(message, localClient); });
        } else {
            var buffer = message.Serialize();
            clientSocket.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, AsyncSendServerComplete, clientSocket);
        }
    }

    // Drop from the server and return to the lobby.
    // Kills the server if hosting.
    public void Disconnect() {
        if(connectionState == ConnectionState.Disconnected) {
            return;
        }

        if(connectionState == ConnectionState.Connected) {
            foreach(var p in players) {
                foreach(var l in listeners) {
                    l.ComSatPlayerLeft(p);
                }
            }
        }

        if(isHost) {
            foreach(var client in connectedClients) {
                if(client != localClient) {
                    client.socket.Shutdown(SocketShutdown.Both);
                    client.socket.Close();
                }
            }
            connectedClients.Clear();

            listenSocket.Close();
            listenSocket = null;
            localClient = null;
        } else {
            try {
                clientSocket.Shutdown(SocketShutdown.Both);
                clientSocket.Close();
            } catch(SocketException) {
            }
            clientSocket = null;
        }

        connectionState = ConnectionState.Disconnected;
        if(inGame) {
            inGame = false;
            SceneManager.LoadScene("Lobby");
        }
    }
}
