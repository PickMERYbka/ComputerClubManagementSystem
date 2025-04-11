using System;
using System.Drawing;
using System.Windows.Forms;
using Npgsql;
using ComputerClubManagementSystem.Models;
using ComputerClubManagementSystem.Managers;

namespace ComputerClubManagementSystem.Forms
{
    public partial class ComputerEditForm : Form
    {
        private readonly string _connectionString;
        private readonly ComputerManager _computerManager;
        private readonly Computer _computer;
        private readonly bool _isEdit;

        private TextBox txtStationName;
        private ComboBox cmbComputerType;
        private TextBox txtSpecifications;
        private NumericUpDown numHourlyRate;
        private ComboBox cmbStatus;
        private DateTimePicker dtpLastMaintenance;
        private DateTimePicker dtpPurchaseDate;
        private CheckBox chkIsActive;

        public ComputerEditForm(string connectionString, Computer computer = null)
        {
            _connectionString = connectionString;
            _computerManager = new ComputerManager(connectionString);
            _computer = computer;
            _isEdit = computer != null;
            InitializeComponent();
            if (_isEdit)
            {
                LoadComputerData();
            }
        }

        private void InitializeComponent()
        {
            this.Text = _isEdit ? "Редактирование компьютера" : "Новый компьютер";
            this.Size = new Size(500, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Создаем элементы управления
            var lblStationName = new Label { Text = "Название станции:", Location = new Point(20, 20) };
            txtStationName = new TextBox { Location = new Point(200, 20), Size = new Size(250, 20) };

            var lblComputerType = new Label { Text = "Тип компьютера:", Location = new Point(20, 50) };
            cmbComputerType = new ComboBox 
            { 
                Location = new Point(200, 50), 
                Size = new Size(250, 20),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbComputerType.Items.AddRange(new string[] { "Gaming", "Standard", "Workstation" });

            var lblSpecifications = new Label { Text = "Характеристики:", Location = new Point(20, 80) };
            txtSpecifications = new TextBox 
            { 
                Location = new Point(200, 80), 
                Size = new Size(250, 60),
                Multiline = true
            };

            var lblHourlyRate = new Label { Text = "Стоимость в час:", Location = new Point(20, 150) };
            numHourlyRate = new NumericUpDown 
            { 
                Location = new Point(200, 150), 
                Size = new Size(100, 20),
                DecimalPlaces = 2,
                Minimum = 0,
                Maximum = 1000
            };

            var lblStatus = new Label { Text = "Статус:", Location = new Point(20, 180) };
            cmbStatus = new ComboBox 
            { 
                Location = new Point(200, 180), 
                Size = new Size(250, 20),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbStatus.Items.AddRange(new string[] { "Available", "In Use", "Maintenance", "Offline" });

            var lblLastMaintenance = new Label { Text = "Последнее обслуживание:", Location = new Point(20, 210) };
            dtpLastMaintenance = new DateTimePicker 
            { 
                Location = new Point(200, 210), 
                Size = new Size(250, 20),
                Format = DateTimePickerFormat.Short
            };

            var lblPurchaseDate = new Label { Text = "Дата покупки:", Location = new Point(20, 240) };
            dtpPurchaseDate = new DateTimePicker 
            { 
                Location = new Point(200, 240), 
                Size = new Size(250, 20),
                Format = DateTimePickerFormat.Short
            };

            var lblIsActive = new Label { Text = "Активен:", Location = new Point(20, 270) };
            chkIsActive = new CheckBox 
            { 
                Location = new Point(200, 270), 
                Size = new Size(250, 20),
                Checked = true
            };

            // Кнопки
            var btnSave = new Button
            {
                Text = "Сохранить",
                DialogResult = DialogResult.OK,
                Location = new Point(200, 320),
                Size = new Size(100, 30)
            };
            btnSave.Click += BtnSave_Click;

            var btnCancel = new Button
            {
                Text = "Отмена",
                DialogResult = DialogResult.Cancel,
                Location = new Point(310, 320),
                Size = new Size(100, 30)
            };

            // Добавляем элементы управления на форму
            this.Controls.AddRange(new Control[] 
            {
                lblStationName, txtStationName,
                lblComputerType, cmbComputerType,
                lblSpecifications, txtSpecifications,
                lblHourlyRate, numHourlyRate,
                lblStatus, cmbStatus,
                lblLastMaintenance, dtpLastMaintenance,
                lblPurchaseDate, dtpPurchaseDate,
                lblIsActive, chkIsActive,
                btnSave, btnCancel
            });
        }

        private void LoadComputerData()
        {
            txtStationName.Text = _computer.StationName;
            cmbComputerType.SelectedItem = _computer.ComputerType;
            txtSpecifications.Text = _computer.Specifications;
            numHourlyRate.Value = _computer.HourlyRate;
            cmbStatus.SelectedItem = _computer.Status;
            if (_computer.LastMaintenanceDate.HasValue)
            {
                dtpLastMaintenance.Value = _computer.LastMaintenanceDate.Value;
            }
            if (_computer.PurchaseDate.HasValue)
            {
                dtpPurchaseDate.Value = _computer.PurchaseDate.Value;
            }
            chkIsActive.Checked = _computer.IsActive;
        }

        private async void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtStationName.Text))
            {
                MessageBox.Show("Пожалуйста, введите название станции", "Ошибка", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cmbComputerType.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, выберите тип компьютера", 
                    "Ошибка валидации", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var computer = _isEdit ? _computer : new Computer();
                computer.StationName = txtStationName.Text;
                computer.ComputerType = cmbComputerType.SelectedItem?.ToString() ?? "Standard";
                computer.Specifications = string.IsNullOrWhiteSpace(txtSpecifications.Text) ? null : txtSpecifications.Text;
                computer.HourlyRate = numHourlyRate.Value;
                computer.Status = cmbStatus.SelectedItem?.ToString() ?? "Available";
                computer.LastMaintenanceDate = dtpLastMaintenance.Value;
                computer.PurchaseDate = dtpPurchaseDate.Value;
                computer.IsActive = chkIsActive.Checked;

                if (_isEdit)
                {
                    await _computerManager.UpdateComputerAsync(computer);
                }
                else
                {
                    await _computerManager.CreateComputerAsync(computer);
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении данных: {ex.Message}", 
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
} 