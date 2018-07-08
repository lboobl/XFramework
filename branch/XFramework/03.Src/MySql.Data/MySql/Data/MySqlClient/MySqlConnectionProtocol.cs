namespace MySql.Data.MySqlClient
{
    using System;

    public enum MySqlConnectionProtocol
    {
        Sockets,
        NamedPipe,
        UnixSocket,
        SharedMemory
    }
}

