using Client.Communication;
using System;
using System.Data;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class MinorServer
{
    private string serverId;
    private string serverIp;
    private string majorServerIp;
    private int majorServerPort;
    private string role;
    private HttpListener httpListener;

    public MinorServer(string serverId, string serverIp, string majorServerIp, int majorServerPort)
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

        httpListener = new HttpListener();
        httpListener.Prefixes.Add($"http://{serverIp}:{port}/api/");
        httpListener.Start();

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
            switch (path)
            {
                case "/api/books":
                    if (method == "GET")
                    {
                        responseMessage = SendMessageToMajorServer("GET_BOOKS");
                    }
                    else if (method == "POST")
                    {
                        responseMessage = SendMessageToMajorServer($"ADD_BOOK|{requestData}");
                    }
                    break;

                case "/api/books/{id}":
                    if (method == "PUT")
                    {
                        responseMessage = SendMessageToMajorServer($"UPDATE_BOOK|{requestData}");
                    }
                    else if (method == "DELETE")
                    {
                        string id = path.Split('/')[3];
                        responseMessage = SendMessageToMajorServer($"DELETE_BOOK|{id}");
                    }
                    break;

                default:
                    responseMessage = "UNKNOWN_REQUEST";
                    break;
            }

            byte[] buffer = Encoding.UTF8.GetBytes(responseMessage);
            response.ContentLength64 = buffer.Length;
            response.OutputStream.Write(buffer, 0, buffer.Length);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при обработке HTTP-запроса: {ex.Message}");
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
                Console.WriteLine($"Ошибка при отправке сообщения на мажорный сервер: {ex.Message}");
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