using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data;
using Npgsql;
using ComputerClubManagementSystem.Models;
using ComputerClubManagementSystem.Managers;

namespace ComputerClubManagementSystem.Forms
{
    public partial class ComputerForm : Form
    {
        private readonly string _connectionString;
        private readonly ComputerManager _computerManager;
        private Button btnAdd;
        private Button btnEdit;
        private Button btnMaintenance;
        private Panel buttonPanel;
        private DataGridView dgvComputers;

        public ComputerForm(string connectionString)
        {
            _connectionString = connectionString;
            _computerManager = new ComputerManager(connectionString);
            InitializeComponent();
            LoadComputers();
        }

        private void InitializeComponent()
        {
            dgvComputers = new DataGridView();
            btnAdd = new Button();
            btnEdit = new Button();
            btnMaintenance = new Button();
            buttonPanel = new Panel();
            ((System.ComponentModel.ISupportInitialize)dgvComputers).BeginInit();
            buttonPanel.SuspendLayout();
            SuspendLayout();
            // 
            // dgvComputers
            // 
            dgvComputers.Location = new Point(0, 0);
            dgvComputers.Name = "dgvComputers";
            dgvComputers.Size = new Size(240, 150);
            dgvComputers.TabIndex = 1;
            // 
            // btnAdd
            // 
            btnAdd.Location = new Point(0, 0);
            btnAdd.Name = "btnAdd";
            btnAdd.Size = new Size(75, 23);
            btnAdd.TabIndex = 0;
            btnAdd.Click += BtnAdd_Click;
            // 
            // btnEdit
            // 
            btnEdit.Location = new Point(0, 0);
            btnEdit.Name = "btnEdit";
            btnEdit.Size = new Size(75, 23);
            btnEdit.TabIndex = 1;
            btnEdit.Click += BtnEdit_Click;
            // 
            // btnMaintenance
            // 
            btnMaintenance.Location = new Point(0, 0);
            btnMaintenance.Name = "btnMaintenance";
            btnMaintenance.Size = new Size(75, 23);
            btnMaintenance.TabIndex = 2;
            btnMaintenance.Click += BtnMaintenance_Click;
            // 
            // buttonPanel
            // 
            buttonPanel.Controls.Add(btnAdd);
            buttonPanel.Controls.Add(btnEdit);
            buttonPanel.Controls.Add(btnMaintenance);
            buttonPanel.Location = new Point(0, 0);
            buttonPanel.Name = "buttonPanel";
            buttonPanel.Size = new Size(200, 100);
            buttonPanel.TabIndex = 0;
            // 
            // ComputerForm
            // 
            ClientSize = new Size(984, 561);
            Controls.Add(buttonPanel);
            Controls.Add(dgvComputers);
            Name = "ComputerForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Управление компьютерами";
            Load += ComputerForm_Load;
            ((System.ComponentModel.ISupportInitialize)dgvComputers).EndInit();
            buttonPanel.ResumeLayout(false);
            ResumeLayout(false);
        }

        private async void LoadComputers()
        {
            try
            {
                var computers = await _computerManager.GetAllComputersAsync();
                dgvComputers.DataSource = computers;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке компьютеров: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            using (var addForm = new ComputerEditForm(_connectionString))
            {
                if (addForm.ShowDialog() == DialogResult.OK)
                {
                    LoadComputers();
                }
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (dgvComputers.SelectedRows.Count == 0)
            {
                MessageBox.Show("Пожалуйста, выберите компьютер для редактирования",
                    "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var computer = (Computer)dgvComputers.SelectedRows[0].DataBoundItem;
            using (var editForm = new ComputerEditForm(_connectionString, computer))
            {
                if (editForm.ShowDialog() == DialogResult.OK)
                {
                    LoadComputers();
                }
            }
        }

        private async void BtnMaintenance_Click(object sender, EventArgs e)
        {
            if (dgvComputers.SelectedRows.Count == 0)
            {
                MessageBox.Show("Пожалуйста, выберите компьютер для отправки на обслуживание",
                    "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var computer = (Computer)dgvComputers.SelectedRows[0].DataBoundItem;
            var result = MessageBox.Show(
                $"Вы уверены, что хотите отправить компьютер {computer.StationName} на обслуживание?",
                "Подтверждение",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    await _computerManager.MarkForMaintenanceAsync(computer.ComputerId);
                    LoadComputers();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при отправке компьютера на обслуживание: {ex.Message}",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ComputerForm_Load(object sender, EventArgs e)
        {

        }
    }
} 