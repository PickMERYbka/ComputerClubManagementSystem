using System;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using Npgsql;
using ComputerClubManagementSystem.Models;
using ComputerClubManagementSystem.Managers;

namespace ComputerClubManagementSystem.Forms
{
    public partial class SessionStartForm : Form
    {
        private readonly string _connectionString;
        private readonly SessionManager _sessionManager;
        private readonly CustomerManager _customerManager;
        private readonly ComputerManager _computerManager;

        private ComboBox cmbCustomer;
        private ComboBox cmbComputer;
        private TextBox txtNotes;

        public SessionStartForm(string connectionString)
        {
            _connectionString = connectionString;
            _sessionManager = new SessionManager(connectionString);
            _customerManager = new CustomerManager(connectionString);
            _computerManager = new ComputerManager(connectionString);
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = "Начало новой сессии";
            this.Size = new Size(400, 300);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Создаем элементы управления
            var lblCustomer = new Label { Text = "Клиент:", Location = new Point(20, 20) };
            cmbCustomer = new ComboBox 
            { 
                Location = new Point(150, 20), 
                Size = new Size(200, 20),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            var lblComputer = new Label { Text = "Компьютер:", Location = new Point(20, 50) };
            cmbComputer = new ComboBox 
            { 
                Location = new Point(150, 50), 
                Size = new Size(200, 20),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            var lblNotes = new Label { Text = "Примечания:", Location = new Point(20, 80) };
            txtNotes = new TextBox 
            { 
                Location = new Point(150, 80), 
                Size = new Size(200, 60),
                Multiline = true
            };

            // Кнопки
            var btnStart = new Button
            {
                Text = "Начать сессию",
                DialogResult = DialogResult.OK,
                Location = new Point(150, 160),
                Size = new Size(100, 30)
            };
            btnStart.Click += BtnStart_Click;

            var btnCancel = new Button
            {
                Text = "Отмена",
                DialogResult = DialogResult.Cancel,
                Location = new Point(260, 160),
                Size = new Size(100, 30)
            };

            // Добавляем элементы управления на форму
            this.Controls.AddRange(new Control[] 
            {
                lblCustomer, cmbCustomer,
                lblComputer, cmbComputer,
                lblNotes, txtNotes,
                btnStart, btnCancel
            });
        }

        private async void LoadData()
        {
            try
            {
                // Загружаем список клиентов
                var customers = await _customerManager.GetAllCustomersAsync();
                cmbCustomer.DataSource = customers;
                cmbCustomer.DisplayMember = "FullName";
                cmbCustomer.ValueMember = "CustomerId";

                // Загружаем список доступных компьютеров
                var computers = await _computerManager.GetAllComputersAsync();
                var availableComputers = computers.Where(c => c.Status == "Available" && c.IsActive).ToList();
                cmbComputer.DataSource = availableComputers;
                cmbComputer.DisplayMember = "StationName";
                cmbComputer.ValueMember = "ComputerId";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", 
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnStart_Click(object sender, EventArgs e)
        {
            if (cmbCustomer.SelectedValue == null || cmbComputer.SelectedValue == null)
            {
                MessageBox.Show("Пожалуйста, выберите клиента и компьютер", 
                    "Ошибка валидации", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                int customerId = (int)cmbCustomer.SelectedValue;
                int computerId = (int)cmbComputer.SelectedValue;
                string notes = string.IsNullOrWhiteSpace(txtNotes.Text) ? null : txtNotes.Text;

                int sessionId = await _sessionManager.StartSessionAsync(
                    customerId, 
                    computerId, 
                    CurrentUser.EmployeeId, 
                    notes);

                MessageBox.Show($"Сессия успешно начата. ID сессии: {sessionId}", 
                    "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при начале сессии: {ex.Message}", 
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
} 