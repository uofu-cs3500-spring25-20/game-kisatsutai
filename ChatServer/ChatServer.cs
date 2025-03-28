// <copyright file="ChatServer.cs" company="UofU-CS3500">
// Copyright (c) 2024 UofU-CS3500. All rights reserved.
// </copyright>

using CS3500.Networking;
using System.Text;

namespace CS3500.Chatting;

/// <summary>
///   A simple ChatServer that handles clients separately and replies with a static message.
/// </summary>
public partial class ChatServer
{
    /// <summary>
    /// Records all connections with their usernames.
    /// </summary>
    private static readonly Dictionary<NetworkConnection, string> clients = new();

    /// <summary>
    /// used to ensure that operations on the clients dictionary are thread-safe.
    /// </summary>
    private static readonly object clientLock = new object();

    /// <summary>
    ///   The main program.
    /// </summary>
    /// <param name="args"> ignored. </param>
    /// <returns> A Task. Not really used. </returns>
    private static void Main(string[] args)
    {
        Server.StartServer(HandleConnect, 11_000);
        Console.Read(); // don't stop the program.
    }


    /// <summary>
    ///   <pre>
    ///     When a new connection is established, enter a loop that receives from and
    ///     replies to a client.
    ///   </pre>
    /// </summary>
    ///
    private static void HandleConnect(NetworkConnection connection)
    {
        // handle all messages until disconnect.
        try
        {
            string? username = connection.ReadLine();
            if (username == null)
            {
                return;
            }

            lock (clientLock)
            {
                clients[connection] = username;
            }

            Console.WriteLine($"[Server] {username} joined");

            while (true)
            {
                string message = connection.ReadLine();
                string broadMessage = $"{username}:{message}";

                lock (clientLock)
                {
                    foreach (var line in clients)
                    {
                        line.Key.Send(broadMessage);
                    }
                }

                Console.WriteLine($"[Server] Broadcasted from {username}: {message}");
            }

        }
        catch (Exception)
        {
            DisconnectClient(connection);
        }
    }

    /// <summary>
    /// Removes the specified client and disconnects it.
    /// </summary>
    /// <param name="connection"></param>
    private static void DisconnectClient(NetworkConnection connection)
    {
        lock (clientLock)
        {
            if (clients.TryGetValue(connection, out string? username))
            {
                Console.WriteLine($"[Server] {username} disconnected");
                clients.Remove(connection);
            }
        }
        connection.Disconnect();
    }
}