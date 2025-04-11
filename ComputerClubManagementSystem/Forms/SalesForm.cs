using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Npgsql;
using ComputerClubManagementSystem.Models;
using ComputerClubManagementSystem.Managers;

namespace ComputerClubManagementSystem.Forms
{
    public partial class SalesForm : Form
    {
        private readonly string _connectionString;
        private readonly SalesManager _salesManager;
        private readonly ProductManager _productManager;
        private readonly CustomerManager _customerManager;
        private DataGridView dgvSales;
        private ComboBox cmbProducts;
        private ComboBox cmbCustomers;
        private NumericUpDown nudQuantity;
        private Button btnAddSale;
        private Button btnViewDetails;
        private Button btnClose;

        public SalesForm(string connectionString)
        {
            _connectionString = connectionString;
            _salesManager = new SalesManager(connectionString);
            _productManager = new ProductManager(connectionString);
            _customerManager = new CustomerManager(connectionString);
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = "Управление продажами";
            this.Size = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterParent;

            // Создаем панель для элементов управления
            Panel controlPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 100
            };

            // Комбобокс для выбора товара
            Label lblProduct = new Label
            {
                Text = "Товар:",
                Location = new Point(10, 10),
                AutoSize = true
            };

            cmbProducts = new ComboBox
            {
                Location = new Point(10, 30),
                Width = 200,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            // Комбобокс для выбора клиента
            Label lblCustomer = new Label
            {
                Text = "Клиент:",
                Location = new Point(220, 10),
                AutoSize = true
            };

            cmbCustomers = new ComboBox
            {
                Location = new Point(220, 30),
                Width = 200,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            // NumericUpDown для количества
            Label lblQuantity = new Label
            {
                Text = "Количество:",
                Location = new Point(430, 10),
                AutoSize = true
            };

            nudQuantity = new NumericUpDown
            {
                Location = new Point(430, 30),
                Width = 100,
                Minimum = 1,
                Maximum = 1000,
                Value = 1
            };

            // Кнопка добавления продажи
            btnAddSale = new Button
            {
                Text = "Добавить продажу",
                Location = new Point(540, 30),
                Width = 120
            };
            btnAddSale.Click += BtnAddSale_Click;

            // Кнопка просмотра деталей
            btnViewDetails = new Button
            {
                Text = "Просмотр деталей",
                Location = new Point(670, 30),
                Width = 120
            };
            btnViewDetails.Click += BtnViewDetails_Click;

            // Кнопка закрытия
            btnClose = new Button
            {
                Text = "Закрыть",
                DialogResult = DialogResult.OK,
                Location = new Point(800, 30),
                Width = 100
            };
            btnClose.Click += (s, e) => this.Close();

            // Добавляем элементы управления на панель
            controlPanel.Controls.AddRange(new Control[] 
            { 
                lblProduct, cmbProducts,
                lblCustomer, cmbCustomers,
                lblQuantity, nudQuantity,
                btnAddSale, btnViewDetails, btnClose
            });

            // Создаем DataGridView
            dgvSales = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            // Добавляем элементы управления на форму
            this.Controls.Add(controlPanel);
            this.Controls.Add(dgvSales);
        }

        private async void LoadData()
        {
            try
            {
                // Загружаем список товаров
                var products = await _productManager.GetAllProductsAsync();
                cmbProducts.DataSource = products;
                cmbProducts.DisplayMember = "Name";
                cmbProducts.ValueMember = "ProductId";

                // Загружаем список клиентов
                var customers = await _customerManager.GetAllCustomersAsync();
                cmbCustomers.DataSource = customers;
                cmbCustomers.DisplayMember = "FullName";
                cmbCustomers.ValueMember = "CustomerId";

                // Загружаем список продаж
                await RefreshSalesGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", 
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task RefreshSalesGrid()
        {
            var sales = await _salesManager.GetAllSalesAsync();
            dgvSales.DataSource = sales;
        }

        private async void BtnAddSale_Click(object sender, EventArgs e)
        {
            try
            {
                if (cmbProducts.SelectedValue == null || cmbCustomers.SelectedValue == null)
                {
                    MessageBox.Show("Пожалуйста, выберите товар и клиента", 
                        "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var productId = (int)cmbProducts.SelectedValue;
                var customerId = (int)cmbCustomers.SelectedValue;
                var quantity = (int)nudQuantity.Value;

                await _salesManager.AddSaleAsync(productId, customerId, quantity);
                await RefreshSalesGrid();

                MessageBox.Show("Продажа успешно добавлена", 
                    "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении продажи: {ex.Message}", 
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnViewDetails_Click(object sender, EventArgs e)
        {
            if (dgvSales.SelectedRows.Count == 0)
            {
                MessageBox.Show("Пожалуйста, выберите продажу для просмотра деталей", 
                    "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var sale = (SaleDetail)dgvSales.SelectedRows[0].DataBoundItem;
            using (var detailsForm = new SaleDetailsForm(_connectionString, sale.SaleId))
            {
                await Task.Run(() => detailsForm.ShowDialog());
            }
        }
    }
} 