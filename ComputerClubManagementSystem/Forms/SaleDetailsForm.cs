using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data;
using Npgsql;
using ComputerClubManagementSystem.Models;
using ComputerClubManagementSystem.Managers;

namespace ComputerClubManagementSystem.Forms
{
    public partial class SaleDetailsForm : Form
    {
        private readonly string _connectionString;
        private readonly SalesManager _salesManager;
        private readonly int _saleId;
        private DataGridView dgvSaleDetails;

        public SaleDetailsForm(string connectionString, int saleId)
        {
            _connectionString = connectionString;
            _salesManager = new SalesManager(connectionString);
            _saleId = saleId;
            InitializeComponent();
            LoadSaleDetails();
        }

        private void InitializeComponent()
        {
            this.Text = "Детали продажи";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterParent;

            // Создаем DataGridView
            dgvSaleDetails = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            // Добавляем кнопку закрытия
            Button btnClose = new Button
            {
                Text = "Закрыть",
                DialogResult = DialogResult.OK,
                Location = new Point(350, 520),
                Size = new Size(100, 30)
            };
            btnClose.Click += (s, e) => this.Close();

            // Добавляем элементы управления на форму
            this.Controls.Add(dgvSaleDetails);
            this.Controls.Add(btnClose);
        }

        private async void LoadSaleDetails()
        {
            try
            {
                var sale = await _salesManager.GetSaleByIdAsync(_saleId);
                if (sale != null)
                {
                    var dt = new DataTable();
                    dt.Columns.Add("Параметр", typeof(string));
                    dt.Columns.Add("Значение", typeof(string));

                    dt.Rows.Add("ID продажи", sale.SaleId);
                    dt.Rows.Add("Клиент", sale.CustomerName);
                    dt.Rows.Add("Товар", sale.ProductName);
                    dt.Rows.Add("Количество", sale.Quantity);
                    dt.Rows.Add("Цена за единицу", sale.Price.ToString("C"));
                    dt.Rows.Add("Общая сумма", sale.TotalAmount.ToString("C"));
                    dt.Rows.Add("Дата продажи", sale.SaleDate.ToString("dd.MM.yyyy HH:mm"));
                    dt.Rows.Add("Сотрудник", sale.EmployeeName);
                    if (!string.IsNullOrEmpty(sale.Notes))
                        dt.Rows.Add("Примечания", sale.Notes);

                    dgvSaleDetails.DataSource = dt;
                }
                else
                {
                    MessageBox.Show("Продажа не найдена", "Ошибка", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке деталей продажи: {ex.Message}", 
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }
        }
    }
} 