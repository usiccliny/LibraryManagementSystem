using System;
using System.Net.Sockets;
using System.Text;

namespace Client.Communication
{
    public class SocketClient
    {
        private string serverIp;
        private int serverPort;

        public SocketClient(string serverIp, int serverPort)
        {
            this.serverIp = serverIp;
            this.serverPort = serverPort;
        }

        /// <summary>
        /// Отправляет сообщение на сервер и получает ответ.
        /// </summary>
        /// <param name="message">Сообщение для отправки.</param>
        /// <returns>Ответ от сервера.</returns>
        public string SendMessageAndGetResponse(string message)
        {
            using (TcpClient client = new TcpClient())
            {
                try
                {
                    client.Connect(serverIp, serverPort);
                    NetworkStream stream = client.GetStream();

                    // Отправка сообщения
                    byte[] data = Encoding.UTF8.GetBytes(message);
                    stream.Write(data, 0, data.Length);

                    // Получение ответа
                    byte[] buffer = new byte[1024];
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    return response;
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Отправляет сообщение на сервер без ожидания ответа.
        /// </summary>
        /// <param name="message">Сообщение для отправки.</param>
        public void SendMessage(string message)
        {
            using (TcpClient client = new TcpClient())
            {
                try
                {
                    client.Connect(serverIp, serverPort);
                    NetworkStream stream = client.GetStream();

                    // Отправка сообщения
                    byte[] data = Encoding.UTF8.GetBytes(message);
                    stream.Write(data, 0, data.Length);
                    Console.WriteLine($"Отправлено сообщение: {message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при отправке сообщения: {ex.Message}");
                    throw;
                }
            }
        }
    }
}