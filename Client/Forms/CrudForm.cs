using System;
using System.Collections.Generic;
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

        public CrudForm(string minorServerIp, int minorServerPort)
        {
            InitializeComponent();
            _restClient = new RestClient($"http://{minorServerIp}:{minorServerPort}");
            LoadBooks(); ;
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
                        MessageBox.Show("Книга успешно добавлена.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadBooks(); // Обновляем список книг
                    }
                    else
                    {
                        MessageBox.Show("Не удалось добавить книгу.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

                    try
                    {
                        bool isSuccess = await _restClient.UpdateBookAsync(updatedBook);
                        if (isSuccess)
                        {
                            MessageBox.Show("Книга успешно обновлена.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadBooks(); // Обновляем список книг
                        }
                        else
                        {
                            MessageBox.Show("Не удалось обновить книгу.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                        MessageBox.Show("Книга успешно удалена.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadBooks(); // Обновляем список книг
                    }
                    else
                    {
                        MessageBox.Show("Не удалось удалить книгу.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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