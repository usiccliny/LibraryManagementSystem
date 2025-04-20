using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Client.Communication;
using Client.Models;

namespace Client.Forms
{
    public partial class CrudForm : Form
    {
        private readonly RestClient _restClient;
        private List<Book> books;

        private string _eventsServerIp;
        private int? _eventsServerPort;
        private ClientWebSocket _webSocket;
        private CancellationTokenSource _cancellationTokenSource;

        public CrudForm(string minorServerCrudIp, int minorServerCrudPort, string? eventsServerIp, int? eventsServerPort)
        {
            InitializeComponent();
            _restClient = new RestClient($"http://{minorServerCrudIp}:{minorServerCrudPort}");
            _eventsServerIp = eventsServerIp;
            _eventsServerPort = eventsServerPort;

            LoadBooks();

            _cancellationTokenSource = new CancellationTokenSource();
            Task.Run(() => ConnectToEventsServerAsync(_cancellationTokenSource.Token));
        }

        private async Task ConnectToEventsServerAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    if (_eventsServerIp != null && _eventsServerPort.HasValue)
                    {
                        _webSocket = new ClientWebSocket();
                        await _webSocket.ConnectAsync(new Uri($"ws://{_eventsServerIp}:{_eventsServerPort}/ws/"), cancellationToken);

                        await ListenForMessagesAsync(_webSocket, cancellationToken);
                    }
                }
                catch { }

                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
            }
        }

        private async Task ListenForMessagesAsync(ClientWebSocket webSocket, CancellationToken cancellationToken)
        {
            var buffer = new byte[1024 * 4];
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", cancellationToken);
                        break;
                    }

                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);

                    ProcessMessage(message);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при получении сообщения: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ProcessMessage(string message)
        {
            try
            {
                var parts = message.Split('|');
                if (parts.Length < 2) return;

                string eventType = parts[0];
                string eventData = parts[1];

                switch (eventType)
                {
                    case "BOOK_ADDED":
                        var addedBook = Newtonsoft.Json.JsonConvert.DeserializeObject<Book>(eventData);
                        Invoke((MethodInvoker)(() =>
                        {
                            books.Add(addedBook);
                            UpdateDataGridView();
                        }));
                        break;

                    case "BOOK_UPDATED":
                        var updatedBook = Newtonsoft.Json.JsonConvert.DeserializeObject<Book>(eventData);
                        Invoke((MethodInvoker)(() =>
                        {
                            var bookToUpdate = books.Find(b => b.Id == updatedBook.Id);
                            if (bookToUpdate != null)
                            {
                                bookToUpdate.Title = updatedBook.Title;
                                bookToUpdate.Author = updatedBook.Author;
                            }
                            UpdateDataGridView();
                        }));
                        break;

                    case "BOOK_DELETED":
                        int deletedBookId = int.Parse(eventData);
                        Invoke((MethodInvoker)(() =>
                        {
                            books.RemoveAll(b => b.Id == deletedBookId);
                            UpdateDataGridView();
                        }));
                        break;

                    default:
                        Console.WriteLine($"Неизвестный тип события: {eventType}");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при обработке сообщения: {ex.Message}");
            }
        }

        private async void LoadBooks()
        {
            try
            {
                books = await _restClient.GetBooksAsync();
                UpdateDataGridView();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке книг: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateDataGridView()
        {
            dgvBooks.Rows.Clear();
            foreach (var book in books)
            {
                dgvBooks.Rows.Add(book.Id, book.Title, book.Author);
            }
        }

        private async void BtnCreate_Click(object sender, EventArgs e)
        {
            var createBookForm = new CreateBookForm();
            if (createBookForm.ShowDialog() == DialogResult.OK)
            {
                var newBook = createBookForm.Book;

                try
                {
                    bool isSuccess = await _restClient.AddBookAsync(newBook);
                    if (isSuccess)
                    {
                        LoadBooks(); // Обновляем список книг
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при добавлении книги: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async void BtnRead_Click(object sender, EventArgs e)
        {
            try
            {
                books = await _restClient.GetBooksAsync();
                UpdateDataGridView();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке книг: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnUpdate_Click(object sender, EventArgs e)
        {
            if (dgvBooks.SelectedRows.Count > 0)
            {
                int selectedRowIndex = dgvBooks.SelectedRows[0].Index;
                var selectedBook = books[selectedRowIndex];

                var updateBookForm = new UpdateBookForm(selectedBook);
                if (updateBookForm.ShowDialog() == DialogResult.OK)
                {
                    var updatedBook = updateBookForm.Book;

                    updatedBook.Id = selectedBook.Id;

                    try
                    {
                        bool isSuccess = await _restClient.UpdateBookAsync(updatedBook);
                        if (isSuccess)
                        {
                            LoadBooks(); // Обновляем список книг
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при обновлении книги: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private async void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgvBooks.SelectedRows.Count > 0)
            {
                int selectedRowIndex = dgvBooks.SelectedRows[0].Index;
                var selectedBook = books[selectedRowIndex];

                try
                {
                    bool isSuccess = await _restClient.DeleteBookAsync(selectedBook.Id);
                    if (isSuccess)
                    {
                        LoadBooks(); // Обновляем список книг
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении книги: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private DataGridView dgvBooks;
    }
}