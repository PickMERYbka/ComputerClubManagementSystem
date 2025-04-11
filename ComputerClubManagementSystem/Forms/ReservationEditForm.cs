using System;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using Npgsql;
using ComputerClubManagementSystem.Models;
using ComputerClubManagementSystem.Managers;

namespace ComputerClubManagementSystem.Forms
{
    public partial class ReservationEditForm : Form
    {
        private readonly string _connectionString;
        private readonly ReservationManager _reservationManager;
        private readonly CustomerManager _customerManager;
        private readonly ComputerManager _computerManager;

        private ComboBox cmbCustomer;
        private ComboBox cmbComputer;
        private DateTimePicker dtpDate;
        private DateTimePicker dtpStartTime;
        private DateTimePicker dtpEndTime;
        private TextBox txtNotes;

        public ReservationEditForm(string connectionString)
        {
            _connectionString = connectionString;
            _reservationManager = new ReservationManager(connectionString);
            _customerManager = new CustomerManager(connectionString);
            _computerManager = new ComputerManager(connectionString);
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = "Создание бронирования";
            this.Size = new Size(500, 400);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Создаем элементы управления
            var lblCustomer = new Label { Text = "Клиент:", Location = new Point(20, 20) };
            cmbCustomer = new ComboBox 
            { 
                Location = new Point(200, 20), 
                Size = new Size(250, 20),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            var lblComputer = new Label { Text = "Компьютер:", Location = new Point(20, 50) };
            cmbComputer = new ComboBox 
            { 
                Location = new Point(200, 50), 
                Size = new Size(250, 20),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            var lblDate = new Label { Text = "Дата:", Location = new Point(20, 80) };
            dtpDate = new DateTimePicker 
            { 
                Location = new Point(200, 80), 
                Size = new Size(250, 20),
                Format = DateTimePickerFormat.Short,
                MinDate = DateTime.Today
            };

            var lblStartTime = new Label { Text = "Время начала:", Location = new Point(20, 110) };
            dtpStartTime = new DateTimePicker 
            { 
                Location = new Point(200, 110), 
                Size = new Size(250, 20),
                Format = DateTimePickerFormat.Time,
                ShowUpDown = true
            };

            var lblEndTime = new Label { Text = "Время окончания:", Location = new Point(20, 140) };
            dtpEndTime = new DateTimePicker 
            { 
                Location = new Point(200, 140), 
                Size = new Size(250, 20),
                Format = DateTimePickerFormat.Time,
                ShowUpDown = true
            };

            var lblNotes = new Label { Text = "Примечания:", Location = new Point(20, 170) };
            txtNotes = new TextBox 
            { 
                Location = new Point(200, 170), 
                Size = new Size(250, 60),
                Multiline = true
            };

            // Кнопки
            var btnSave = new Button
            {
                Text = "Создать",
                DialogResult = DialogResult.OK,
                Location = new Point(200, 250),
                Size = new Size(100, 30)
            };
            btnSave.Click += BtnSave_Click;

            var btnCancel = new Button
            {
                Text = "Отмена",
                DialogResult = DialogResult.Cancel,
                Location = new Point(310, 250),
                Size = new Size(100, 30)
            };

            // Добавляем элементы управления на форму
            this.Controls.AddRange(new Control[] 
            {
                lblCustomer, cmbCustomer,
                lblComputer, cmbComputer,
                lblDate, dtpDate,
                lblStartTime, dtpStartTime,
                lblEndTime, dtpEndTime,
                lblNotes, txtNotes,
                btnSave, btnCancel
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

                // Устанавливаем начальное время
                dtpStartTime.Value = DateTime.Today.AddHours(9); // 9:00
                dtpEndTime.Value = DateTime.Today.AddHours(10); // 10:00
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", 
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnSave_Click(object sender, EventArgs e)
        {
            if (cmbCustomer.SelectedValue == null || cmbComputer.SelectedValue == null)
            {
                MessageBox.Show("Пожалуйста, выберите клиента и компьютер", 
                    "Ошибка валидации", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (dtpStartTime.Value >= dtpEndTime.Value)
            {
                MessageBox.Show("Время окончания должно быть позже времени начала", 
                    "Ошибка валидации", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                int customerId = (int)cmbCustomer.SelectedValue;
                int computerId = (int)cmbComputer.SelectedValue;
                DateTime reservationDate = dtpDate.Value.Date;
                TimeSpan startTime = dtpStartTime.Value.TimeOfDay;
                TimeSpan endTime = dtpEndTime.Value.TimeOfDay;
                string notes = string.IsNullOrWhiteSpace(txtNotes.Text) ? null : txtNotes.Text;

                int reservationId = await _reservationManager.CreateReservationAsync(
                    customerId,
                    computerId,
                    reservationDate,
                    startTime,
                    endTime,
                    notes,
                    CurrentUser.EmployeeId);

                MessageBox.Show($"Бронирование успешно создано. ID бронирования: {reservationId}", 
                    "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании бронирования: {ex.Message}", 
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
} 