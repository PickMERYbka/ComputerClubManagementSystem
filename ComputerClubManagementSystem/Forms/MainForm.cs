using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data;
using Npgsql;
using ComputerClubManagementSystem.Models;
using ComputerClubManagementSystem.Managers;
using System.Threading.Tasks;

namespace ComputerClubManagementSystem.Forms
{
    public partial class MainForm : Form
    {
        private readonly string _connectionString;
        private readonly User _currentUser;
        private readonly CustomerManager _customerManager;
        private readonly ComputerManager _computerManager;
        private readonly SessionManager _sessionManager;
        private readonly ReservationManager _reservationManager;
        private readonly SalesManager _salesManager;
        private readonly ReportingManager _reportingManager;

        private TabControl tabControl;
        private TabPage tabSessions;
        private TabPage tabReservations;
        private TabPage tabCustomers;
        private TabPage tabComputers;
        private TabPage tabSales;
        private TabPage tabReports;

        public MainForm(string connectionString, User currentUser)
        {
            InitializeComponent();
            _currentUser = currentUser;
            _connectionString = connectionString;
            
            _customerManager = new CustomerManager(_connectionString);
            _computerManager = new ComputerManager(_connectionString);
            _sessionManager = new SessionManager(_connectionString);
            _reservationManager = new ReservationManager(_connectionString);
            _salesManager = new SalesManager(_connectionString);
            _reportingManager = new ReportingManager(_connectionString);

            InitializeTabs();
            _ = LoadData();
            timer.Start();
        }

        private void InitializeTabs()
        {
            tabControl = new TabControl
            {
                Dock = DockStyle.Fill
            };

            tabSessions = new TabPage("Сессии");
            tabReservations = new TabPage("Бронирования");
            tabCustomers = new TabPage("Клиенты");
            tabComputers = new TabPage("Компьютеры");
            tabSales = new TabPage("Продажи");
            tabReports = new TabPage("Отчеты");

            tabControl.TabPages.Add(tabSessions);
            tabControl.TabPages.Add(tabReservations);
            tabControl.TabPages.Add(tabCustomers);
            tabControl.TabPages.Add(tabComputers);
            tabControl.TabPages.Add(tabSales);

            if (_currentUser.Role == "admin")
            {
                tabControl.TabPages.Add(tabReports);
            }

            Controls.Add(tabControl);
        }

        private async Task LoadData()
        {
            try
            {
                var sessions = await _sessionManager.GetActiveSessionsAsync();
                var reservations = await _reservationManager.GetActiveReservationsAsync();
                var customers = await _customerManager.GetAllCustomersAsync();
                var computers = await _computerManager.GetAllComputersAsync();
                var sales = await _salesManager.GetAllSalesAsync();

                // TODO: Обновить DataGridView для каждой вкладки
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void timer_Tick(object sender, EventArgs e)
        {
            await LoadData();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            timer.Stop();
        }
    }
} 