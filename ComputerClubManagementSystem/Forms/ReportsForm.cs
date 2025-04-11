using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data;
using Npgsql;
using ComputerClubManagementSystem.Models;
using ComputerClubManagementSystem.Managers;

namespace ComputerClubManagementSystem.Forms
{
    public partial class ReportsForm : Form
    {
        private readonly string _connectionString;
        private readonly ReportingManager _reportingManager;

        private TabControl tabControl;
        private TabPage tabRevenue;
        private TabPage tabUtilization;
        private TabPage tabCustomers;
        private TabPage tabProducts;

        private DataGridView dgvRevenue;
        private DataGridView dgvUtilization;
        private DataGridView dgvCustomers;
        private DataGridView dgvProducts;

        private DateTimePicker dtpStartDate;
        private DateTimePicker dtpEndDate;
        private NumericUpDown numDays;
        private Button btnRefresh;

        public ReportsForm(string connectionString)
        {
            _connectionString = connectionString;
            _reportingManager = new ReportingManager(connectionString);
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = "Отчеты";
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Создаем TabControl
            tabControl = new TabControl
            {
                Dock = DockStyle.Fill
            };

            // Создаем вкладки
            tabRevenue = new TabPage("Выручка");
            tabUtilization = new TabPage("Использование компьютеров");
            tabCustomers = new TabPage("Активность клиентов");
            tabProducts = new TabPage("Продажи товаров");

            // Добавляем вкладки
            tabControl.TabPages.AddRange(new TabPage[] {
                tabRevenue,
                tabUtilization,
                tabCustomers,
                tabProducts
            });

            // Создаем элементы управления для фильтров
            var filterPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                Padding = new Padding(10)
            };

            var lblDays = new Label
            {
                Text = "Количество дней:",
                Location = new Point(10, 20),
                AutoSize = true
            };

            numDays = new NumericUpDown
            {
                Location = new Point(120, 18),
                Minimum = 1,
                Maximum = 365,
                Value = 7,
                Width = 60
            };

            var lblStartDate = new Label
            {
                Text = "Начальная дата:",
                Location = new Point(200, 20),
                AutoSize = true
            };

            dtpStartDate = new DateTimePicker
            {
                Location = new Point(290, 18),
                Width = 150
            };

            var lblEndDate = new Label
            {
                Text = "Конечная дата:",
                Location = new Point(460, 20),
                AutoSize = true
            };

            dtpEndDate = new DateTimePicker
            {
                Location = new Point(550, 18),
                Width = 150
            };

            btnRefresh = new Button
            {
                Text = "Обновить",
                Location = new Point(720, 16),
                Width = 100
            };
            btnRefresh.Click += BtnRefresh_Click;

            filterPanel.Controls.AddRange(new Control[] {
                lblDays, numDays,
                lblStartDate, dtpStartDate,
                lblEndDate, dtpEndDate,
                btnRefresh
            });

            // Создаем DataGridView для каждого отчета
            dgvRevenue = CreateDataGridView();
            dgvUtilization = CreateDataGridView();
            dgvCustomers = CreateDataGridView();
            dgvProducts = CreateDataGridView();

            // Добавляем DataGridView на вкладки
            tabRevenue.Controls.Add(dgvRevenue);
            tabUtilization.Controls.Add(dgvUtilization);
            tabCustomers.Controls.Add(dgvCustomers);
            tabProducts.Controls.Add(dgvProducts);

            // Добавляем элементы на форму
            this.Controls.Add(filterPanel);
            this.Controls.Add(tabControl);

            // Устанавливаем начальные даты
            dtpEndDate.Value = DateTime.Today;
            dtpStartDate.Value = DateTime.Today.AddDays(-7);
        }

        private DataGridView CreateDataGridView()
        {
            var dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            return dgv;
        }

        private async void LoadData()
        {
            try
            {
                // Загружаем отчет о выручке
                var revenueData = await _reportingManager.GetDailyRevenueAsync((int)numDays.Value);
                dgvRevenue.DataSource = revenueData;

                // Загружаем отчет об использовании компьютеров
                var utilizationData = await _reportingManager.GetComputerUtilizationAsync(
                    dtpStartDate.Value,
                    dtpEndDate.Value
                );
                dgvUtilization.DataSource = utilizationData;

                // Загружаем отчет об активности клиентов
                var customerData = await _reportingManager.GetCustomerActivityAsync(
                    dtpStartDate.Value,
                    dtpEndDate.Value
                );
                dgvCustomers.DataSource = customerData;

                // Загружаем отчет о продажах товаров
                var productData = await _reportingManager.GetProductSalesReportAsync(
                    dtpStartDate.Value,
                    dtpEndDate.Value
                );
                dgvProducts.DataSource = productData;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке отчетов: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            LoadData();
        }
    }
} 