using System;
using System.Drawing;
using System.Windows.Forms;
using Npgsql;
using ComputerClubManagementSystem.Models;
using ComputerClubManagementSystem.Managers;

namespace ComputerClubManagementSystem.Forms
{
    public partial class CustomerEditForm : Form
    {
        private readonly string _connectionString;
        private readonly CustomerManager _customerManager;
        private readonly Customer _customer;
        private readonly bool _isEdit;

        private TextBox txtFirstName;
        private TextBox txtLastName;
        private TextBox txtEmail;
        private TextBox txtPhone;
        private DateTimePicker dtpDateOfBirth;
        private ComboBox cmbMembershipStatus;
        private TextBox txtNotes;

        public CustomerEditForm(string connectionString, Customer customer = null)
        {
            _connectionString = connectionString;
            _customerManager = new CustomerManager(connectionString);
            _customer = customer;
            _isEdit = customer != null;
            InitializeComponent();
            if (_isEdit)
            {
                LoadCustomerData();
            }
        }

        private void InitializeComponent()
        {
            this.Text = _isEdit ? "Редактирование клиента" : "Новый клиент";
            this.Size = new Size(400, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Создаем элементы управления
            var lblFirstName = new Label { Text = "Имя:", Location = new Point(20, 20) };
            txtFirstName = new TextBox { Location = new Point(150, 20), Size = new Size(200, 20) };

            var lblLastName = new Label { Text = "Фамилия:", Location = new Point(20, 50) };
            txtLastName = new TextBox { Location = new Point(150, 50), Size = new Size(200, 20) };

            var lblEmail = new Label { Text = "Email:", Location = new Point(20, 80) };
            txtEmail = new TextBox { Location = new Point(150, 80), Size = new Size(200, 20) };

            var lblPhone = new Label { Text = "Телефон:", Location = new Point(20, 110) };
            txtPhone = new TextBox { Location = new Point(150, 110), Size = new Size(200, 20) };

            var lblDateOfBirth = new Label { Text = "Дата рождения:", Location = new Point(20, 140) };
            dtpDateOfBirth = new DateTimePicker { Location = new Point(150, 140), Size = new Size(200, 20) };

            var lblMembershipStatus = new Label { Text = "Статус:", Location = new Point(20, 170) };
            cmbMembershipStatus = new ComboBox 
            { 
                Location = new Point(150, 170), 
                Size = new Size(200, 20),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbMembershipStatus.Items.AddRange(new string[] { "Standard", "Premium", "VIP" });

            var lblNotes = new Label { Text = "Примечания:", Location = new Point(20, 200) };
            txtNotes = new TextBox 
            { 
                Location = new Point(150, 200), 
                Size = new Size(200, 60),
                Multiline = true
            };

            // Кнопки
            var btnSave = new Button
            {
                Text = "Сохранить",
                DialogResult = DialogResult.OK,
                Location = new Point(150, 300),
                Size = new Size(100, 30)
            };
            btnSave.Click += BtnSave_Click;

            var btnCancel = new Button
            {
                Text = "Отмена",
                DialogResult = DialogResult.Cancel,
                Location = new Point(260, 300),
                Size = new Size(100, 30)
            };

            // Добавляем элементы управления на форму
            this.Controls.AddRange(new Control[] 
            {
                lblFirstName, txtFirstName,
                lblLastName, txtLastName,
                lblEmail, txtEmail,
                lblPhone, txtPhone,
                lblDateOfBirth, dtpDateOfBirth,
                lblMembershipStatus, cmbMembershipStatus,
                lblNotes, txtNotes,
                btnSave, btnCancel
            });
        }

        private void LoadCustomerData()
        {
            txtFirstName.Text = _customer.FirstName;
            txtLastName.Text = _customer.LastName;
            txtEmail.Text = _customer.Email;
            txtPhone.Text = _customer.Phone;
            if (_customer.DateOfBirth.HasValue)
            {
                dtpDateOfBirth.Value = _customer.DateOfBirth.Value;
            }
            cmbMembershipStatus.SelectedItem = _customer.MembershipStatus;
            txtNotes.Text = _customer.Notes;
        }

        private async void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFirstName.Text) || string.IsNullOrWhiteSpace(txtLastName.Text))
            {
                MessageBox.Show("Пожалуйста, заполните обязательные поля (Имя и Фамилия)", 
                    "Ошибка валидации", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var customer = _isEdit ? _customer : new Customer();
                customer.FirstName = txtFirstName.Text;
                customer.LastName = txtLastName.Text;
                customer.Email = string.IsNullOrWhiteSpace(txtEmail.Text) ? null : txtEmail.Text;
                customer.Phone = string.IsNullOrWhiteSpace(txtPhone.Text) ? null : txtPhone.Text;
                customer.DateOfBirth = dtpDateOfBirth.Value;
                customer.MembershipStatus = cmbMembershipStatus.SelectedItem?.ToString() ?? "Standard";
                customer.Notes = string.IsNullOrWhiteSpace(txtNotes.Text) ? null : txtNotes.Text;

                if (_isEdit)
                {
                    await _customerManager.UpdateCustomerAsync(customer);
                }
                else
                {
                    await _customerManager.CreateCustomerAsync(customer);
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