using System;
using System.Data;
using Npgsql;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Net;
using Client.Communication;
using Client.Repository;
using Client.Models;

public class MajorServer
{
    private string serverId;
    private string ipAddress;
    private int port;
    private const string ConnectionString = "Host=localhost;Port=5432;Database=library_db;Username=postgres;Password=11299133;";
    private TcpListener listener;
    private BookRepository bookRepository;
    private readonly RabbitMqService _rabbitMqService;

    public MajorServer(string serverId, string ipAddress, int port)
    {
        this.serverId = serverId;
        this.ipAddress = ipAddress;
        this.port = port;
        this.bookRepository = new BookRepository(ConnectionString);
        this._rabbitMqService = new RabbitMqService("localhost", "guest", "guest");

        IPAddress ip = IPAddress.Parse(ipAddress);
        this.listener = new TcpListener(ip, port);
    }

    public void Start()
    {
        RegisterServer(serverId, "major_server", ipAddress, port);
        listener.Start();

        Thread broadcastThread = new Thread(StartBroadcasting)
        {
            IsBackground = true
        };
        broadcastThread.Start();

        Thread acceptThread = new Thread(AcceptClients)
        {
            IsBackground = true
        };
        acceptThread.Start();


        Thread cleanupThread = new Thread(() =>
        {
            while (true)
            {
                RemoveInactiveInstances();
                Thread.Sleep(10000);
            }
        })
        {
            IsBackground = true
        };
        cleanupThread.Start();

    }

    private void AcceptClients()
    {
        while (true)
        {
            try
            {
                var client = listener.AcceptTcpClient();

                Thread clientThread = new Thread(() => HandleClient(client))
                {
                    IsBackground = true
                };
                clientThread.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при принятии подключения: {ex.Message}");
            }
        }
    }

    private void HandleClient(TcpClient client)
    {
        try
        {
            using (client)
            {
                var stream = client.GetStream();
                var buffer = new byte[1024];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead).TrimEnd('\0');

                var parts = message.Split('|');
                if (parts[0] == "REGISTER_MINOR")
                {
                    string serverId = parts[1];
                    string serverIp = parts[2];
                    int port = int.Parse(parts[3]);

                    RegisterServer(serverId, "minor_server", serverIp, port);
                    string role = AssignRoleToMinorServer(serverId);

                    string response = $"ACK|{role}";
                    byte[] responseData = Encoding.UTF8.GetBytes(response);
                    stream.Write(responseData, 0, responseData.Length);
                }
                else if (parts[0] == "HEARTBEAT")
                {
                    string serverId = parts[1];
                    UpdateLastSeen(serverId);
                }
                else if (parts[0] == "REGISTER_CLIENT")
                {
                    string clientId = parts[1];
                    RegisterClient(clientId);
                    stream.Write(Encoding.UTF8.GetBytes("ACK"), 0, 3);
                }
                else if (parts[0] == "GET_SERVERS_WITH_ROLE" && parts.Length >= 2)
                {
                    string requestedRole = parts[1];

                    string serverWithRole = ReassignRoleIfNeeded(requestedRole);

                    var serverInfo = GetServerInfoById(serverWithRole);
                    string response = $"{serverInfo.IpAddress}:{serverInfo.Port}";
                    byte[] responseData = Encoding.UTF8.GetBytes(response);
                    stream.Write(responseData, 0, responseData.Length);
                }
                else if (parts[0] == "GET_BOOKS")
                {
                    var books = bookRepository.GetBooks();
                    string response = Newtonsoft.Json.JsonConvert.SerializeObject(books);
                    stream.Write(Encoding.UTF8.GetBytes(response), 0, response.Length);
                }
                else if (parts[0] == "ADD_BOOK")
                {
                    var book = Newtonsoft.Json.JsonConvert.DeserializeObject<Book>(parts[1]);
                    book.Id = bookRepository.AddBook(book);
                    stream.Write(Encoding.UTF8.GetBytes("ACK"), 0, 3);
                    _rabbitMqService.PublishMessage($"BOOK_ADDED|{Newtonsoft.Json.JsonConvert.SerializeObject(book)}");
                }
                else if (parts[0] == "UPDATE_BOOK")
                {
                    var book = Newtonsoft.Json.JsonConvert.DeserializeObject<Book>(parts[2]);
                    bookRepository.UpdateBook(book);
                    stream.Write(Encoding.UTF8.GetBytes("ACK"), 0, 3);
                    _rabbitMqService.PublishMessage($"BOOK_UPDATED|{Newtonsoft.Json.JsonConvert.SerializeObject(book)}");
                }
                else if (parts[0] == "DELETE_BOOK")
                {
                    int id = int.Parse(parts[1]);
                    bookRepository.DeleteBook(id);
                    stream.Write(Encoding.UTF8.GetBytes("ACK"), 0, 3);
                    _rabbitMqService.PublishMessage($"BOOK_DELETED|{id}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при обработке клиента: {ex.Message}");
        }
    }

    private void UpdateLastSeen(string instanceId)
    {
        using (var connection = new NpgsqlConnection(ConnectionString))
        {
            connection.Open();
            var cmd = new NpgsqlCommand(@"
            UPDATE ActiveInstances
            SET last_seen = CURRENT_TIMESTAMP
            WHERE instance_id = @instanceId;", connection);
            cmd.Parameters.AddWithValue("instanceId", instanceId);
            cmd.ExecuteNonQuery();
        }

        using (var connection = new NpgsqlConnection(ConnectionString))
        {
            connection.Open();
            var cmd = new NpgsqlCommand(@"
            UPDATE MinorServerRoles
            SET last_seen = CURRENT_TIMESTAMP
            WHERE instance_id = @instanceId;", connection);
            cmd.Parameters.AddWithValue("instanceId", instanceId);
            cmd.ExecuteNonQuery();
        }
    }

    private void RegisterServer(string serverId, string role, string ipAddress, int port)
    {
        using (var connection = new NpgsqlConnection(ConnectionString))
        {
            connection.Open();
            var command = new NpgsqlCommand(
                "INSERT INTO ActiveInstances (instance_id, role, last_seen, ip_address, port, is_client) " +
                "VALUES (@instanceId, @role, CURRENT_TIMESTAMP, @ipAddress, @port, FALSE) " +
                "ON CONFLICT (instance_id) DO UPDATE SET " +
                "last_seen = EXCLUDED.last_seen, " +
                "ip_address = EXCLUDED.ip_address, " +
                "port = EXCLUDED.port;",
                connection);
            command.Parameters.AddWithValue("instanceId", serverId);
            command.Parameters.AddWithValue("role", role);
            command.Parameters.AddWithValue("ipAddress", ipAddress);
            command.Parameters.AddWithValue("port", port);
            command.ExecuteNonQuery();
        }
    }

    private string AssignRoleToMinorServer(string serverId)
    {
        // Получаем список активных минорных серверов и их ролей
        var activeServers = new Dictionary<string, string>();
        using (var connection = new NpgsqlConnection(ConnectionString))
        {
            connection.Open();
            using (var command = new NpgsqlCommand(
                "SELECT instance_id, role FROM MinorServerRoles WHERE last_seen > NOW() - INTERVAL '10 seconds';",
                connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        activeServers[reader.GetString(0)] = reader.GetString(1);
                    }
                }
            }
        }

        // Определяем роль для нового сервера
        string role = DetermineRole(activeServers);

        // Сохраняем роль в базе данных
        using (var connection = new NpgsqlConnection(ConnectionString))
        {
            connection.Open();
            using (var insertCommand = new NpgsqlCommand(
                "INSERT INTO MinorServerRoles (instance_id, role, last_seen) VALUES (@instanceId, @role, CURRENT_TIMESTAMP) " +
                "ON CONFLICT (instance_id) DO UPDATE SET role = EXCLUDED.role, last_seen = EXCLUDED.last_seen;",
                connection))
            {
                insertCommand.Parameters.AddWithValue("instanceId", serverId);
                insertCommand.Parameters.AddWithValue("role", role);
                insertCommand.ExecuteNonQuery();
            }
        }

        return role;
    }

    private string DetermineRole(Dictionary<string, string> activeServers)
    {
        bool hasCrud = activeServers.Values.Contains("crud");
        bool hasEvent = activeServers.Values.Contains("event");
        bool hasCache = activeServers.Values.Contains("cache");

        if (!hasCrud) return "crud";
        if (!hasEvent) return "event";
        if (!hasCache) return "cache";


        return "fallback";
    }

    private void RegisterClient(string clientId)
    {
        using (var connection = new NpgsqlConnection(ConnectionString))
        {
            connection.Open();
            var command = new NpgsqlCommand(
                "INSERT INTO ActiveInstances (instance_id, role, last_seen, is_client) " +
                "VALUES (@instanceId, 'client', CURRENT_TIMESTAMP, TRUE) " +
                "ON CONFLICT (instance_id) DO UPDATE SET " +
                "last_seen = EXCLUDED.last_seen;",
                connection);
            command.Parameters.AddWithValue("instanceId", clientId);
            command.ExecuteNonQuery();
        }
    }

    private void StartBroadcasting()
    {
        var udpClient = new UdpDiscoveryClient(9999);
        string message = $"{ipAddress}:{port}";

        var activeIPs = ArpScanner.GetActiveIPs();

        while (true)
        {
            try
            {
                udpClient.SendBroadcastMessage(activeIPs, message);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при отправке широковещательного сообщения: {ex.Message}");
            }

            Thread.Sleep(2000);
        }
    }

    private void RemoveInactiveInstances()
    {
        using (var connection = new NpgsqlConnection(ConnectionString))
        {
            connection.Open();
            var cmd = new NpgsqlCommand(@"
            DELETE FROM ActiveInstances
            WHERE last_seen < NOW() - INTERVAL '10 seconds' AND (instance_id <> 'major_server');", connection);
            cmd.ExecuteNonQuery();
        }

        using (var connection = new NpgsqlConnection(ConnectionString))
        {
            connection.Open();
            var cmd = new NpgsqlCommand(@"
            DELETE FROM MinorServerRoles
            WHERE last_seen < NOW() - INTERVAL '10 seconds';", connection);
            cmd.ExecuteNonQuery();
        }
    }

    private string ReassignRoleIfNeeded(string requestedRole)
    {
        using (var connection = new NpgsqlConnection(ConnectionString))
        {
            connection.Open();

            // Проверяем, есть ли активный сервер с запрошенной ролью
            var checkCommand = new NpgsqlCommand(
                "SELECT instance_id FROM MinorServerRoles WHERE role = @role AND last_seen > NOW() - INTERVAL '5 seconds';",
                connection);
            checkCommand.Parameters.AddWithValue("role", requestedRole);
            var existingServer = checkCommand.ExecuteScalar();

            if (existingServer != null)
            {
                // Сервер с запрошенной ролью уже существует
                return existingServer.ToString();
            }
        }

        using (var connection1 = new NpgsqlConnection(ConnectionString))
        {

            connection1.Open();

            // Если сервера с запрошенной ролью нет, ищем сервер для переназначения
            var findServerCommand = new NpgsqlCommand(
                "SELECT instance_id FROM MinorServerRoles WHERE last_seen > NOW() - INTERVAL '5 seconds' LIMIT 1;",
                connection1);
            var serverToReassign = findServerCommand.ExecuteScalar();

            if (serverToReassign == null)
            {
                throw new Exception("Нет доступных минорных серверов для переназначения роли.");
            }

            // Переназначаем роль найденному серверу
            var reassignCommand = new NpgsqlCommand(
                "UPDATE MinorServerRoles SET role = @newRole, last_seen = CURRENT_TIMESTAMP WHERE instance_id = @instanceId;",
                connection1);
            reassignCommand.Parameters.AddWithValue("newRole", requestedRole);
            reassignCommand.Parameters.AddWithValue("instanceId", serverToReassign.ToString());
            reassignCommand.ExecuteNonQuery();

            return serverToReassign.ToString();
        }
    }

    private (string IpAddress, int Port) GetServerInfoById(string serverId)
    {
        using (var connection = new NpgsqlConnection(ConnectionString))
        {
            connection.Open();
            var command = new NpgsqlCommand(
                "SELECT ip_address, port FROM ActiveInstances WHERE instance_id = @instanceId;",
                connection);
            command.Parameters.AddWithValue("instanceId", serverId);
            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    return (reader.GetString(0), reader.GetInt32(1));
                }
                throw new Exception("Сервер с указанным ID не найден.");
            }
        }
    }
}