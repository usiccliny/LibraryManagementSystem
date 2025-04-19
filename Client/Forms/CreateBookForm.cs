using Client.Models;
using System;
using System.Windows.Forms;

namespace Client.Forms
{
    public partial class CreateBookForm : Form
    {
        public Book Book { get; private set; }

        public CreateBookForm()
        {
            InitializeComponent();
        }
    }
}