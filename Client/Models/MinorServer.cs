using Client.Communication;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;

public class MinorServer
{
    private string serverId;
    private string serverIp;
    private string majorServerIp;
    private int majorServerPort;
    private string role;
    private HttpListener httpListener;

    private readonly List<WebSocket> _clients = new List<WebSocket>();
    private readonly RabbitMqService _rabbitMqService;

    public MinorServer(string serverId, string serverIp, string majorServerIp, int majorServerPort)
    {
        this.serverId = serverId;
        this.serverIp = serverIp;
        this.majorServerIp = majorServerIp;
        this.majorServerPort = majorServerPort;
        _rabbitMqService = new RabbitMqService(majorServerIp, "user1", "user1");
    }

    public void Start()
    {
        IPAddress ip = IPAddress.Parse(serverIp);
        TcpListener listener = new TcpListener(ip, 0);
        listener.Start();
        int port = ((IPEndPoint)listener.LocalEndpoint).Port;
        this.role = RegisterWithMajorServer(port);
        listener.Stop();

        if (role == "crud")
        {
            httpListener = new HttpListener();
            httpListener.Prefixes.Add($"http://{serverIp}:{port}/api/");
            httpListener.Start();
        }

        if (role == "event")
        {
            StartWebSocketServer(port);

            _rabbitMqService.SubscribeToQueue(message =>
            {
                foreach (var client in _clients)
                {
                    if (client.State == WebSocketState.Open)
                    {
                        Task.Run(async () =>
                        {
                            await client.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(message)), WebSocketMessageType.Text, true, CancellationToken.None);
                        });
                    }
                }
            });
        }

        Thread httpThread = new Thread(HandleHttpRequests)
        {
            IsBackground = true
        };
        httpThread.Start();

        Thread heartbeatThread = new Thread(() => SendHeartbeat())
        {
            IsBackground = true
        };
        heartbeatThread.Start();
    }

    private void StartWebSocketServer(int port)
    {
        httpListener = new HttpListener();
        httpListener.Prefixes.Add($"http://{serverIp}:{port}/ws/");

        try
        {
            httpListener.Start();
        }
        catch (HttpListenerException ex)
        {
            MessageBox.Show($"Ошибка при запуске WebSocket-сервера: {ex.Message}");
            throw;
        }

        Thread webSocketThread = new Thread(HandleWebSocketConnections)
        {
            IsBackground = true
        };
        webSocketThread.Start();
    }

    private void HandleWebSocketConnections()
    {
        while (true)
        {
            try
            {
                var context = httpListener.GetContext();
                if (context.Request.IsWebSocketRequest)
                {
                    ThreadPool.QueueUserWorkItem(state => AcceptWebSocketConnection(context));
                }
                else
                {
                    context.Response.StatusCode = 400; // Bad Request
                    context.Response.Close();
                }
            }
            catch (HttpListenerException ex)
            {
                MessageBox.Show($"Ошибка HttpListener: {ex.Message}");
                break;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Неожиданная ошибка: {ex.Message}");
            }
        }
    }

    private async void AcceptWebSocketConnection(HttpListenerContext context)
    {
        var webSocketContext = await context.AcceptWebSocketAsync(null);
        var webSocket = webSocketContext.WebSocket;

        AddClient(webSocket);

        try
        {
            var buffer = new byte[1024];
            while (webSocket.State == WebSocketState.Open)
            {
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                    RemoveClient(webSocket);
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при работе с WebSocket: {ex.Message}");
            RemoveClient(webSocket);
        }
    }

    public void AddClient(WebSocket client)
    {
        _clients.Add(client);
    }

    public void RemoveClient(WebSocket client)
    {
        _clients.Remove(client);
    }

    private void HandleHttpRequests()
    {
        while (true)
        {
            try
            {
                var context = httpListener.GetContext();
                ThreadPool.QueueUserWorkItem(state => ProcessHttpRequest(context));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обработке HTTP-запроса: {ex.Message}");
            }

            Thread.Sleep(1000);
        }
    }

    private void ProcessHttpRequest(HttpListenerContext context)
    {
        var request = context.Request;
        var response = context.Response;

        try
        {
            string method = request.HttpMethod;
            string path = request.Url.AbsolutePath;
            string requestData = new System.IO.StreamReader(request.InputStream).ReadToEnd();

            string responseMessage = "";

            // Обработка GET и POST запросов для /api/books
            if (path == "/api/books")
            {
                if (method == "GET")
                {
                    responseMessage = SendMessageToMajorServer("GET_BOOKS");
                }
                else if (method == "POST")
                {
                    responseMessage = SendMessageToMajorServer($"ADD_BOOK|{requestData}");
                }
            }
            // Обработка PUT и DELETE запросов для /api/books/{id}
            else if (path.StartsWith("/api/books/") && path.Split('/').Length == 4)
            {
                string id = path.Split('/')[3];

                if (method == "PUT")
                {
                    responseMessage = SendMessageToMajorServer($"UPDATE_BOOK|{id}|{requestData}");
                }
                else if (method == "DELETE")
                {
                    responseMessage = SendMessageToMajorServer($"DELETE_BOOK|{id}");
                }
            }
            else
            {
                responseMessage = "UNKNOWN_REQUEST";
            }

            byte[] buffer = Encoding.UTF8.GetBytes(responseMessage);
            response.ContentLength64 = buffer.Length;
            response.OutputStream.Write(buffer, 0, buffer.Length);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при обработке HTTP-запроса: {ex.Message}");
        }
        finally
        {
            response.OutputStream.Close();
        }
    }

    private string SendMessageToMajorServer(string message)
    {
        using (TcpClient client = new TcpClient())
        {
            try
            {
                client.Connect(majorServerIp, majorServerPort);
                var stream = client.GetStream();

                // Отправка сообщения
                byte[] data = Encoding.UTF8.GetBytes(message);
                stream.Write(data, 0, data.Length);

                // Получение ответа
                var buffer = new byte[1024];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                return Encoding.UTF8.GetString(buffer, 0, bytesRead).TrimEnd('\0');
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при отправке сообщения на мажорный сервер: {ex.Message}");
                throw;
            }
        }
    }

    private string RegisterWithMajorServer(int port)
    {
        var socketClient = new SocketClient(majorServerIp, majorServerPort);
        string message = $"REGISTER_MINOR|{serverId}|{serverIp}|{port}";
        string response = socketClient.SendMessageAndGetResponse(message);

        var parts = response.Split('|');
        if (parts[0] == "ACK" && parts.Length >= 2)
        {
            string role = parts[1];
            MessageBox.Show($"Минорный сервер успешно зарегистрирован с ролью: {role}");
            return role;
        }
        else
        {
            MessageBox.Show("Ошибка регистрации минорного сервера.");
            return null;
        }
    }

    private void SendHeartbeat()
    {
        var socketClient = new SocketClient(majorServerIp, majorServerPort);
        while (true)
        {
            try
            {
                string message = $"HEARTBEAT|{serverId}";
                socketClient.SendMessage(message);
                Thread.Sleep(2500);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при отправке состояния: {ex.Message}");
            }
        }
    }
}