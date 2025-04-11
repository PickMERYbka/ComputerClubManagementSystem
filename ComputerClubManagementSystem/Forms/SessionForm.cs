using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data;
using Npgsql;
using ComputerClubManagementSystem.Models;
using ComputerClubManagementSystem.Managers;

namespace ComputerClubManagementSystem.Forms
{
    public partial class SessionForm : Form
    {
        private readonly string _connectionString;
        private readonly SessionManager _sessionManager;
        private readonly CustomerManager _customerManager;
        private readonly ComputerManager _computerManager;
        private DataGridView dgvSessions;

        public SessionForm(string connectionString)
        {
            _connectionString = connectionString;
            _sessionManager = new SessionManager(connectionString);
            _customerManager = new CustomerManager(connectionString);
            _computerManager = new ComputerManager(connectionString);
            InitializeComponent();
            LoadSessions();
        }

        private void InitializeComponent()
        {
            this.Text = "Управление сессиями";
            this.Size = new Size(1200, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Создаем DataGridView
            dgvSessions = new DataGridView
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
            Button btnStartSession = new Button
            {
                Text = "Начать сессию",
                Location = new Point(10, 10),
                Size = new Size(150, 30)
            };
            btnStartSession.Click += BtnStartSession_Click;

            Button btnEndSession = new Button
            {
                Text = "Завершить сессию",
                Location = new Point(170, 10),
                Size = new Size(150, 30)
            };
            btnEndSession.Click += BtnEndSession_Click;

            Button btnRefresh = new Button
            {
                Text = "Обновить",
                Location = new Point(330, 10),
                Size = new Size(100, 30)
            };
            btnRefresh.Click += BtnRefresh_Click;

            // Создаем панель для кнопок
            Panel buttonPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50
            };
            buttonPanel.Controls.AddRange(new Control[] { btnStartSession, btnEndSession, btnRefresh });

            // Добавляем элементы управления на форму
            this.Controls.Add(buttonPanel);
            this.Controls.Add(dgvSessions);
        }

        private async void LoadSessions()
        {
            try
            {
                var sessions = await _sessionManager.GetActiveSessionsAsync();
                dgvSessions.DataSource = sessions;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке сессий: {ex.Message}", 
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnStartSession_Click(object sender, EventArgs e)
        {
            using (var startForm = new SessionStartForm(_connectionString))
            {
                if (startForm.ShowDialog() == DialogResult.OK)
                {
                    LoadSessions();
                }
            }
        }

        private async void BtnEndSession_Click(object sender, EventArgs e)
        {
            if (dgvSessions.SelectedRows.Count == 0)
            {
                MessageBox.Show("Пожалуйста, выберите сессию для завершения", 
                    "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var session = (Session)dgvSessions.SelectedRows[0].DataBoundItem;
            var result = MessageBox.Show(
                $"Вы уверены, что хотите завершить сессию для клиента {session.Customer?.FullName ?? "Неизвестный клиент"}?",
                "Подтверждение",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    var totalCost = await _sessionManager.EndSessionAsync(session.SessionId);
                    MessageBox.Show($"Сессия завершена. Общая стоимость: {totalCost:C2}", 
                        "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadSessions();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при завершении сессии: {ex.Message}", 
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            LoadSessions();
        }
    }
} 