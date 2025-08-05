using Godot;
using System;
using EOSPluign.addons.eosplugin;
using Riptide;
using Riptide.Transports.EOS;

public partial class Manager : Node2D
{
    
    public Server server { get; private set; }
    public  Client client { get; private set; }
    
    
    public override void _Ready()
    {
        base._Ready();
        EOSInterfaceManager.Instance.AuthService.SmartLogin();
        EOSInterfaceManager.Instance.ConnectService.Login();

        var eosServer = new EOSServer();
        server = new Server(eosServer);
        server.Start(7777, 10);
        server.ClientConnected += ServerOnClientConnected;
        server.ClientDisconnected += ServerOnClientDisconnected;
        
        
        var eosClient = new EOSClient(eosServer); 
        client = new Client(eosClient);
        client.Connect("127.0.0.1", 7);
        client.Connected += DidConnect;
        client.ConnectionFailed += ClientOnConnectionFailed;
        client.ClientDisconnected += ClientOnClientDisconnected;
        client.Disconnected += ClientOnDisconnected;
        
    }

    private void ServerOnClientDisconnected(object sender, ServerDisconnectedEventArgs e)
    {
        throw new NotImplementedException();
    }

    private void ServerOnClientConnected(object sender, ServerConnectedEventArgs e)
    {
        throw new NotImplementedException();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (server.IsRunning)
            server.Update();
        client.Update();
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        StopServer();
        server.ClientConnected -= ServerOnClientConnected;
        server.ClientDisconnected -= ServerOnClientDisconnected;
        client.Connected -= DidConnect;
        client.ConnectionFailed -= ClientOnConnectionFailed;
        client.ClientDisconnected -= ClientOnClientDisconnected;
        client.Disconnected -= ClientOnDisconnected;
    }

    internal void StopServer()
    {
        server.Stop();
        //TODO remove all serverPlayers from Scene
    }
    private void DidConnect(object sender, EventArgs e)
    {
        throw new NotImplementedException();
    }

    private void ClientOnDisconnected(object sender, DisconnectedEventArgs e)
    {
        throw new NotImplementedException();
    }

    private void ClientOnClientDisconnected(object sender, ClientDisconnectedEventArgs e)
    {
        
    }

    private void ClientOnConnectionFailed(object sender, ConnectionFailedEventArgs e)
    {
        
    }
}
