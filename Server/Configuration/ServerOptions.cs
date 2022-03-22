public class ServerOptions
{
    /// <summary>
    /// The hostname or ip of the TWS server
    /// </summary>
    public string Server { get; set; }

    /// <summary>
    /// Port used by TWS server to listen for inbound connections
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    /// The client ID used when connecting to TWS
    /// </summary>
    public int ClientId { get; set; }
}
