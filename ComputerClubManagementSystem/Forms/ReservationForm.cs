using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data;
using Npgsql;
using ComputerClubManagementSystem.Models;
using ComputerClubManagementSystem.Managers;

namespace ComputerClubManagementSystem.Forms
{
    public partial class ReservationForm : Form
    {
        private readonly string _connectionString;
        private readonly ReservationManager _reservationManager;
        private DataGridView dgvReservations;

        public ReservationForm(string connectionString)
        {
            _connectionString = connectionString;
            _reservationManager = new ReservationManager(connectionString);
            InitializeComponent();
            LoadReservations();
        }

        private void InitializeComponent()
        {
            this.Text = "Управление бронированиями";
            this.Size = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Создаем DataGridView
            dgvReservations = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            // Добавляем кнопки
            Button btnAdd = new Button
            {
                Text = "Создать бронирование",
                Location = new Point(10, 10),
                Size = new Size(180, 30)
            };
            btnAdd.Click += BtnAdd_Click;

            Button btnCancel = new Button
            {
                Text = "Отменить бронирование",
                Location = new Point(200, 10),
                Size = new Size(180, 30)
            };
            btnCancel.Click += BtnCancel_Click;

            Button btnRefresh = new Button
            {
                Text = "Обновить",
                Location = new Point(390, 10),
                Size = new Size(100, 30)
            };
            btnRefresh.Click += BtnRefresh_Click;

            // Создаем панель для кнопок
            Panel buttonPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50
            };
            buttonPanel.Controls.AddRange(new Control[] { btnAdd, btnCancel, btnRefresh });

            // Добавляем элементы управления на форму
            this.Controls.Add(buttonPanel);
            this.Controls.Add(dgvReservations);
        }

        private async void LoadReservations()
        {
            try
            {
                var reservations = await _reservationManager.GetUpcomingReservationsAsync();
                dgvReservations.DataSource = reservations;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке бронирований: {ex.Message}", 
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            using (var addForm = new ReservationEditForm(_connectionString))
            {
                if (addForm.ShowDialog() == DialogResult.OK)
                {
                    LoadReservations();
                }
            }
        }

        private async void BtnCancel_Click(object sender, EventArgs e)
        {
            if (dgvReservations.SelectedRows.Count == 0)
            {
                MessageBox.Show("Пожалуйста, выберите бронирование для отмены", 
                    "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var reservation = (Reservation)dgvReservations.SelectedRows[0].DataBoundItem;
            var result = MessageBox.Show(
                $"Вы уверены, что хотите отменить бронирование для клиента {reservation.CustomerName}?",
                "Подтверждение",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    await _reservationManager.CancelReservationAsync(reservation.ReservationId);
                    MessageBox.Show("Бронирование успешно отменено", 
                        "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadReservations();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при отмене бронирования: {ex.Message}", 
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            LoadReservations();
        }
    }
} 