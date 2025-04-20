using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

public class RabbitMqService
{
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public RabbitMqService(string hostName, string username, string password)
    {
        var factory = new ConnectionFactory() { HostName = hostName, VirtualHost = "/", UserName = username, Password = password };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.QueueDeclare(queue: "book_events", durable: false, exclusive: false, autoDelete: false, arguments: null);
    }

    // Метод для публикации сообщений (используется мажорным сервером)
    public void PublishMessage(string message)
    {
        var body = Encoding.UTF8.GetBytes(message);
        _channel.BasicPublish(exchange: "", routingKey: "book_events", basicProperties: null, body: body);
    }

    public void SubscribeToQueue(Action<string> onMessageReceived)
    {
        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            // Вызываем обработчик сообщения
            onMessageReceived?.Invoke(message);
        };

        _channel.BasicConsume(queue: "book_events", autoAck: true, consumer: consumer);
    }

    public void Close()
    {
        _channel.Close();
        _connection.Close();
    }
}