namespace Client.Forms
{
    partial class CrudForm
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
            this.Text = "CRUD Операции";
            this.Size = new System.Drawing.Size(600, 400);

            var lblTitle = new Label
            {
                Text = "Управление книгами",
                Location = new System.Drawing.Point(50, 20),
                AutoSize = true,
                Font = new System.Drawing.Font("Segoe UI", 14, System.Drawing.FontStyle.Bold)
            };

            var btnCreate = new Button
            {
                Text = "Создать книгу",
                Location = new System.Drawing.Point(50, 70),
                Size = new System.Drawing.Size(150, 40)
            };
            btnCreate.Click += BtnCreate_Click;

            var btnRead = new Button
            {
                Text = "Просмотреть книги",
                Location = new System.Drawing.Point(50, 120),
                Size = new System.Drawing.Size(150, 40)
            };
            btnRead.Click += BtnRead_Click;

            var btnUpdate = new Button
            {
                Text = "Обновить книгу",
                Location = new System.Drawing.Point(50, 170),
                Size = new System.Drawing.Size(150, 40)
            };
            btnUpdate.Click += BtnUpdate_Click;

            var btnDelete = new Button
            {
                Text = "Удалить книгу",
                Location = new System.Drawing.Point(50, 220),
                Size = new System.Drawing.Size(150, 40)
            };
            btnDelete.Click += BtnDelete_Click;

            var dgvBooks = new DataGridView
            {
                Location = new System.Drawing.Point(250, 70),
                Size = new System.Drawing.Size(300, 250),
                ReadOnly = true,
                ColumnCount = 3
            };
            dgvBooks.Columns[0].Name = "ID";
            dgvBooks.Columns[1].Name = "Название";
            dgvBooks.Columns[2].Name = "Автор";

            this.Controls.Add(lblTitle);
            this.Controls.Add(btnCreate);
            this.Controls.Add(btnRead);
            this.Controls.Add(btnUpdate);
            this.Controls.Add(btnDelete);
            this.Controls.Add(dgvBooks);

            this.dgvBooks = dgvBooks;
        }

        #endregion
    }
}