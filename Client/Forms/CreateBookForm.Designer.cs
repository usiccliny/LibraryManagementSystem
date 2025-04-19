using Client.Models;

namespace Client.Forms
{
    partial class CreateBookForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.Text = "Создать книгу";
            this.Size = new System.Drawing.Size(300, 200);

            var lblTitle = new Label
            {
                Text = "Название:",
                Location = new System.Drawing.Point(20, 20),
                AutoSize = true
            };

            var txtTitle = new TextBox
            {
                Location = new System.Drawing.Point(20, 40),
                Size = new System.Drawing.Size(250, 20)
            };

            var lblAuthor = new Label
            {
                Text = "Автор:",
                Location = new System.Drawing.Point(20, 70),
                AutoSize = true
            };

            var txtAuthor = new TextBox
            {
                Location = new System.Drawing.Point(20, 90),
                Size = new System.Drawing.Size(250, 20)
            };

            var btnSave = new Button
            {
                Text = "Сохранить",
                Location = new System.Drawing.Point(100, 130),
                Size = new System.Drawing.Size(100, 30)
            };
            btnSave.Click += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(txtTitle.Text) && !string.IsNullOrEmpty(txtAuthor.Text))
                {
                    Book = new Book { Title = txtTitle.Text, Author = txtAuthor.Text };
                    this.DialogResult = DialogResult.OK;
                    Close();
                }
                else
                {
                    MessageBox.Show("Заполните все поля.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            this.Controls.Add(lblTitle);
            this.Controls.Add(txtTitle);
            this.Controls.Add(lblAuthor);
            this.Controls.Add(txtAuthor);
            this.Controls.Add(btnSave);
        }

        #endregion
    }
}