using Client.Models;
using System;
using System.Windows.Forms;

namespace Client.Forms
{
    public partial class UpdateBookForm : Form
    {
        public Book Book { get; private set; }

        public UpdateBookForm(Book book)
        {
            InitializeComponent();
            txtTitle.Text = book.Title;
            txtAuthor.Text = book.Author;
        }
    }
}