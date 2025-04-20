using Client.Models;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Repository
{
    internal class BookRepository
    {
        private string ConnectionString;

        public BookRepository(string ConnectionString)
        {
            this.ConnectionString = ConnectionString;
        }

        public List<Book> GetBooks()
        {
            var books = new List<Book>();
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();
                var command = new NpgsqlCommand("SELECT * FROM Books;", connection);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        books.Add(new Book
                        {
                            Id = reader.GetInt32(0),
                            Title = reader.GetString(1),
                            Author = reader.GetString(2)
                        });
                    }
                }
            }
            return books;
        }

        public int AddBook(Book book)
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();
                var command = new NpgsqlCommand(
                    "INSERT INTO Books (title, author) VALUES (@title, @author) RETURNING id;",
                    connection);
                command.Parameters.AddWithValue("title", book.Title);
                command.Parameters.AddWithValue("author", book.Author);

                return book.Id = Convert.ToInt32(command.ExecuteScalar());
            }
        }

        public void UpdateBook(Book book)
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();
                var command = new NpgsqlCommand(
                    "UPDATE Books SET title = @title, author = @author, is_need_to_back = @isNeedToBack WHERE id = @id;",
                    connection);
                command.Parameters.AddWithValue("id", book.Id);
                command.Parameters.AddWithValue("title", book.Title);
                command.Parameters.AddWithValue("author", book.Author);
                command.ExecuteNonQuery();
            }
        }

        public void DeleteBook(int id)
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();
                var command = new NpgsqlCommand("DELETE FROM Books WHERE id = @id;", connection);
                command.Parameters.AddWithValue("id", id);
                command.ExecuteNonQuery();
            }
        }
    }
}
