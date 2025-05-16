using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Client.Models;

namespace Client.Communication
{
    public class RestClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseAddress;

        public RestClient(string baseAddress)
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(baseAddress);
            _baseAddress = baseAddress;
        }

        /// <summary>
        /// Получает список всех книг.
        /// </summary>
        /// <returns>Список книг.</returns>
        public async Task<List<Book>> GetBooksAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/books");
                response.EnsureSuccessStatusCode(); // Проверка успешности запроса
                var books = await response.Content.ReadFromJsonAsync<List<Book>>();
                return books ?? new List<Book>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении списка книг: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Добавляет новую книгу.
        /// </summary>
        /// <param name="book">Книга для добавления.</param>
        /// <returns>True, если книга успешно добавлена.</returns>
        public async Task<bool> AddBookAsync(Book book)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/books", book);
                response.EnsureSuccessStatusCode(); // Проверка успешности запроса
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при добавлении книги: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Обновляет существующую книгу.
        /// </summary>
        /// <param name="book">Книга для обновления.</param>
        /// <returns>True, если книга успешно обновлена.</returns>
        public async Task<bool> UpdateBookAsync(Book book)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/books/{book.Id}", book);
                response.EnsureSuccessStatusCode(); // Проверка успешности запроса
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при обновлении книги: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Удаляет книгу по её идентификатору.
        /// </summary>
        /// <param name="id">Идентификатор книги.</param>
        /// <returns>True, если книга успешно удалена.</returns>
        public async Task<bool> DeleteBookAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/books/{id}");
                response.EnsureSuccessStatusCode(); // Проверка успешности запроса
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при удалении книги: {ex.Message}");
                throw;
            }
        }
    }
}