using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data;
using Npgsql;
using ComputerClubManagementSystem.Models;
using ComputerClubManagementSystem.Managers;

namespace ComputerClubManagementSystem.Forms
{
    public partial class SaleEditForm : Form
    {
        private readonly string _connectionString;
        private readonly SalesManager _salesManager;
        private readonly ProductManager _productManager;
        private readonly CustomerManager _customerManager;

        private ComboBox cmbCustomer;
        private ComboBox cmbProduct;
        private NumericUpDown numQuantity;
        private TextBox txtNotes;
        private Button btnSave;
        private Button btnCancel;

        public SaleEditForm(string connectionString)
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
            this.Text = "Новая продажа";
            this.Size = new Size(400, 300);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Создаем элементы управления
            Label lblCustomer = new Label
            {
                Text = "Клиент:",
                Location = new Point(20, 20),
                Size = new Size(100, 20)
            };

            cmbCustomer = new ComboBox
            {
                Location = new Point(130, 20),
                Size = new Size(240, 20),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            Label lblProduct = new Label
            {
                Text = "Товар:",
                Location = new Point(20, 60),
                Size = new Size(100, 20)
            };

            cmbProduct = new ComboBox
            {
                Location = new Point(130, 60),
                Size = new Size(240, 20),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            Label lblQuantity = new Label
            {
                Text = "Количество:",
                Location = new Point(20, 100),
                Size = new Size(100, 20)
            };

            numQuantity = new NumericUpDown
            {
                Location = new Point(130, 100),
                Size = new Size(100, 20),
                Minimum = 1,
                Maximum = 1000,
                Value = 1
            };

            Label lblNotes = new Label
            {
                Text = "Примечания:",
                Location = new Point(20, 140),
                Size = new Size(100, 20)
            };

            txtNotes = new TextBox
            {
                Location = new Point(130, 140),
                Size = new Size(240, 60),
                Multiline = true
            };

            btnSave = new Button
            {
                Text = "Сохранить",
                DialogResult = DialogResult.OK,
                Location = new Point(130, 220),
                Size = new Size(100, 30)
            };
            btnSave.Click += BtnSave_Click;

            btnCancel = new Button
            {
                Text = "Отмена",
                DialogResult = DialogResult.Cancel,
                Location = new Point(240, 220),
                Size = new Size(100, 30)
            };

            // Добавляем элементы управления на форму
            this.Controls.AddRange(new Control[] {
                lblCustomer, cmbCustomer,
                lblProduct, cmbProduct,
                lblQuantity, numQuantity,
                lblNotes, txtNotes,
                btnSave, btnCancel
            });
        }

        private async void LoadData()
        {
            try
            {
                var customers = await _customerManager.GetAllCustomersAsync();
                var products = await _productManager.GetAllProductsAsync();

                cmbCustomer.DataSource = customers;
                cmbCustomer.DisplayMember = "FullName";
                cmbCustomer.ValueMember = "CustomerId";

                cmbProduct.DataSource = products;
                cmbProduct.DisplayMember = "Name";
                cmbProduct.ValueMember = "ProductId";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }
        }

        private async void BtnSave_Click(object sender, EventArgs e)
        {
            if (cmbCustomer.SelectedValue == null || cmbProduct.SelectedValue == null)
            {
                MessageBox.Show("Пожалуйста, выберите клиента и товар",
                    "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var sale = new Sale
                {
                    CustomerId = (int)cmbCustomer.SelectedValue,
                    ProductId = (int)cmbProduct.SelectedValue,
                    Quantity = (int)numQuantity.Value,
                    Notes = txtNotes.Text,
                    SaleDate = DateTime.Now
                };

                await _salesManager.CreateSaleAsync(sale);
                MessageBox.Show("Продажа успешно создана",
                    "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании продажи: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
} 