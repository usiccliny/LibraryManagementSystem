using Client.Communication;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Net.Sockets;
using Client.Repository;
using Client.Models;

public class Server
{
    private string serverId;
    private string serverIp;
    private string majorServerIp;
    private int majorServerPort;
    private string role;
    private HttpListener httpListener;

    private readonly List<WebSocket> _clients = new List<WebSocket>();
    private RabbitMqService _rabbitMqService;

    private const string ConnectionString = "Host=localhost;Port=5432;Database=library_db;Username=postgres;Password=11299133;";
    private BookRepository bookRepository;

    public Server(string serverId, string serverIp, string majorServerIp, int majorServerPort)
    {
        this.serverId = serverId;
        this.serverIp = serverIp;
        this.majorServerIp = majorServerIp;
        this.majorServerPort = majorServerPort;
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
            bookRepository = new BookRepository(ConnectionString);
            _rabbitMqService = new RabbitMqService("localhost", "guest", "guest");
        }

        if (role == "event")
        {
            _rabbitMqService = new RabbitMqService(majorServerIp, "user1", "user1");

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

        Console.WriteLine($"Сервер запущен с ролью {role}. Нажмите '0', чтобы завершить работу.");
        while (true)
        {
            if (Console.ReadKey(true).KeyChar == '0')
            {
                Console.WriteLine("Завершение работы сервера...");
                break;
            }
        }
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
            Console.WriteLine($"Ошибка при запуске WebSocket-сервера: {ex.Message}");
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
                    context.Response.StatusCode = 400;
                    context.Response.Close();
                }
            }
            catch (HttpListenerException ex)
            {
                Console.WriteLine($"Ошибка HttpListener: {ex.Message}");
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Неожиданная ошибка: {ex.Message}");
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
            Console.WriteLine($"Ошибка при работе с WebSocket: {ex.Message}");
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
                Console.WriteLine($"Ошибка при обработке HTTP-запроса: {ex.Message}");
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

            string responseMessage = HandleRequest(method, path, requestData);

            SendResponse(response, responseMessage);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при обработке HTTP-запроса: {ex.Message}");
            SendErrorResponse(response, "INTERNAL_SERVER_ERROR");
        }
        finally
        {
            response.OutputStream.Close();
        }
    }

    private string HandleRequest(string method, string path, string requestData)
    {
        if (path == "/api/books")
        {
            return HandleBooksRequest(method, requestData);
        }
        else if (path.StartsWith("/api/books/") && path.Split('/').Length == 4)
        {
            string id = path.Split('/')[3];
            return HandleBookByIdRequest(method, id, requestData);
        }
        else
        {
            return "UNKNOWN_REQUEST";
        }
    }
    private string HandleBooksRequest(string method, string requestData)
    {
        switch (method)
        {
            case "GET":
                var books = bookRepository.GetBooks(); 
                return Newtonsoft.Json.JsonConvert.SerializeObject(books);

            case "POST":
                var newBook = Newtonsoft.Json.JsonConvert.DeserializeObject<Book>(requestData); 
                if (newBook == null) return "INVALID_BOOK_DATA";

                int isAdded = bookRepository.AddBook(newBook);
                _rabbitMqService.PublishMessage($"BOOK_ADDED|{Newtonsoft.Json.JsonConvert.SerializeObject(newBook)}");
                return isAdded != null ? "BOOK_ADDED" : "FAILED_TO_ADD_BOOK";

            default:
                return "UNSUPPORTED_METHOD";
        }
    }

    private string HandleBookByIdRequest(string method, string id, string requestData)
    {
        if (!int.TryParse(id, out int bookId)) return "INVALID_BOOK_ID";

        switch (method)
        {
            case "PUT":
                var updatedBook = Newtonsoft.Json.JsonConvert.DeserializeObject<Book>(requestData);
                if (updatedBook == null || updatedBook.Id != bookId) return "INVALID_BOOK_DATA";

                bookRepository.UpdateBook(updatedBook);
                _rabbitMqService.PublishMessage($"BOOK_UPDATED|{Newtonsoft.Json.JsonConvert.SerializeObject(updatedBook)}");
                return "BOOK_UPDATED";

            case "DELETE":
                bookRepository.DeleteBook(bookId);
                _rabbitMqService.PublishMessage($"BOOK_DELETED|{bookId}");
                return "BOOK_DELETED";

            default:
                return "UNSUPPORTED_METHOD";
        }
    }

    private void SendResponse(HttpListenerResponse response, string responseMessage)
    {
        byte[] buffer = Encoding.UTF8.GetBytes(responseMessage);
        response.ContentLength64 = buffer.Length;
        response.OutputStream.Write(buffer, 0, buffer.Length);
    }

    private void SendErrorResponse(HttpListenerResponse response, string errorMessage)
    {
        string errorResponse = $"{{\"error\": \"{errorMessage}\"}}";
        byte[] buffer = Encoding.UTF8.GetBytes(errorResponse);
        response.StatusCode = (int)HttpStatusCode.InternalServerError;
        response.ContentLength64 = buffer.Length;
        response.OutputStream.Write(buffer, 0, buffer.Length);
    }

    private string RegisterWithMajorServer(int port)
    {
        var socketClient = new SocketClient(majorServerIp, majorServerPort);
        string message = $"REGISTER_SERVICE|{serverId}|{serverIp}|{port}";
        string response = socketClient.SendMessageAndGetResponse(message);

        var parts = response.Split('|');
        if (parts[0] == "ACK" && parts.Length >= 2)
        {
            string role = parts[1];
            Console.WriteLine($"Минорный сервер успешно зарегистрирован с ролью: {role}");
            return role;
        }
        else
        {
            Console.WriteLine("Ошибка регистрации минорного сервера.");
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
                Console.WriteLine($"Ошибка при отправке состояния: {ex.Message}");
            }
        }
    }
}