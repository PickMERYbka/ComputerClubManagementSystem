using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data;
using Npgsql;
using ComputerClubManagementSystem.Models;
using ComputerClubManagementSystem.Managers;

namespace ComputerClubManagementSystem.Forms
{
    public partial class CustomerForm : Form
    {
        private readonly string _connectionString;
        private readonly CustomerManager _customerManager;
        private Button btnAdd;
        private Button btnEdit;
        private Button btnDelete;
        private Panel buttonPanel;
        private DataGridView dgvCustomers;

        public CustomerForm(string connectionString)
        {
            _connectionString = connectionString;
            _customerManager = new CustomerManager(connectionString);
            InitializeComponent();
            LoadCustomers();
        }

        private void InitializeComponent()
        {
            dgvCustomers = new DataGridView();
            btnAdd = new Button();
            btnEdit = new Button();
            btnDelete = new Button();
            buttonPanel = new Panel();
            ((System.ComponentModel.ISupportInitialize)dgvCustomers).BeginInit();
            buttonPanel.SuspendLayout();
            SuspendLayout();
            // 
            // dgvCustomers
            // 
            dgvCustomers.Location = new Point(0, 0);
            dgvCustomers.Name = "dgvCustomers";
            dgvCustomers.Size = new Size(240, 150);
            dgvCustomers.TabIndex = 1;
            // 
            // btnAdd
            // 
            btnAdd.Location = new Point(0, 0);
            btnAdd.Name = "btnAdd";
            btnAdd.Size = new Size(75, 23);
            btnAdd.TabIndex = 0;
            btnAdd.Click += BtnAdd_Click;
            // 
            // btnEdit
            // 
            btnEdit.Location = new Point(0, 0);
            btnEdit.Name = "btnEdit";
            btnEdit.Size = new Size(75, 23);
            btnEdit.TabIndex = 1;
            btnEdit.Click += BtnEdit_Click;
            // 
            // btnDelete
            // 
            btnDelete.Location = new Point(0, 0);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(75, 23);
            btnDelete.TabIndex = 2;
            btnDelete.Click += BtnDelete_Click;
            // 
            // buttonPanel
            // 
            buttonPanel.Controls.Add(btnAdd);
            buttonPanel.Controls.Add(btnEdit);
            buttonPanel.Controls.Add(btnDelete);
            buttonPanel.Location = new Point(0, 0);
            buttonPanel.Name = "buttonPanel";
            buttonPanel.Size = new Size(200, 100);
            buttonPanel.TabIndex = 0;
            // 
            // CustomerForm
            // 
            ClientSize = new Size(784, 561);
            Controls.Add(buttonPanel);
            Controls.Add(dgvCustomers);
            Name = "CustomerForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Управление клиентами";
            Load += CustomerForm_Load;
            ((System.ComponentModel.ISupportInitialize)dgvCustomers).EndInit();
            buttonPanel.ResumeLayout(false);
            ResumeLayout(false);
        }

        private async void LoadCustomers()
        {
            try
            {
                var customers = await _customerManager.GetAllCustomersAsync();
                dgvCustomers.DataSource = customers;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке клиентов: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            using (var addForm = new CustomerEditForm(_connectionString))
            {
                if (addForm.ShowDialog() == DialogResult.OK)
                {
                    LoadCustomers();
                }
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (dgvCustomers.SelectedRows.Count == 0)
            {
                MessageBox.Show("Пожалуйста, выберите клиента для редактирования", "Предупреждение",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var customer = (Customer)dgvCustomers.SelectedRows[0].DataBoundItem;
            using (var editForm = new CustomerEditForm(_connectionString, customer))
            {
                if (editForm.ShowDialog() == DialogResult.OK)
                {
                    LoadCustomers();
                }
            }
        }

        private async void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgvCustomers.SelectedRows.Count == 0)
            {
                MessageBox.Show("Пожалуйста, выберите клиента для удаления", "Предупреждение",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var customer = (Customer)dgvCustomers.SelectedRows[0].DataBoundItem;
            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить клиента {customer.FullName}?",
                "Подтверждение удаления",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    await _customerManager.DeleteCustomerAsync(customer.CustomerId);
                    LoadCustomers();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении клиента: {ex.Message}", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void CustomerForm_Load(object sender, EventArgs e)
        {

        }
    }
} 