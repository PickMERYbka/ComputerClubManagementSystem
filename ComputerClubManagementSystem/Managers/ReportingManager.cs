using System;
using System.Data;
using System.Threading.Tasks;
using Npgsql;

namespace ComputerClubManagementSystem.Managers
{
    public class ReportingManager : BaseManager
    {
        public ReportingManager(string connectionString) : base(connectionString)
        {
        }

        public async Task<DataTable> GetDailyRevenueAsync(int days)
        {
            var dt = new DataTable();
            using (var connection = await GetConnectionAsync())
            {
                using (var command = new NpgsqlCommand(
                    @"SELECT date_trunc('day', sale_date) as date,
                             SUM(total_amount) as revenue
                      FROM sales
                      WHERE sale_date >= CURRENT_DATE - @days
                      GROUP BY date_trunc('day', sale_date)
                      ORDER BY date DESC",
                    connection))
                {
                    command.Parameters.AddWithValue("@days", days);
                    using (var adapter = new NpgsqlDataAdapter(command))
                    {
                        await Task.Run(() => adapter.Fill(dt));
                    }
                }
            }
            return dt;
        }

        public async Task<DataTable> GetComputerUtilizationAsync(DateTime startDate, DateTime endDate)
        {
            var dt = new DataTable();
            using (var connection = await GetConnectionAsync())
            {
                using (var command = new NpgsqlCommand(
                    @"SELECT c.station_name,
                             COUNT(s.session_id) as total_sessions,
                             SUM(EXTRACT(EPOCH FROM (s.end_time - s.start_time))/3600) as total_hours
                      FROM computers c
                      LEFT JOIN sessions s ON c.computer_id = s.computer_id
                      WHERE s.start_time BETWEEN @startDate AND @endDate
                      GROUP BY c.station_name
                      ORDER BY total_hours DESC",
                    connection))
                {
                    command.Parameters.AddWithValue("@startDate", startDate);
                    command.Parameters.AddWithValue("@endDate", endDate);
                    using (var adapter = new NpgsqlDataAdapter(command))
                    {
                        await Task.Run(() => adapter.Fill(dt));
                    }
                }
            }
            return dt;
        }

        public async Task<DataTable> GetCustomerActivityAsync(DateTime startDate, DateTime endDate)
        {
            var dt = new DataTable();
            using (var connection = await GetConnectionAsync())
            {
                using (var command = new NpgsqlCommand(
                    @"SELECT c.first_name || ' ' || c.last_name as customer_name,
                             COUNT(s.session_id) as total_sessions,
                             SUM(s.total_cost) as total_spent
                      FROM customers c
                      LEFT JOIN sessions s ON c.customer_id = s.customer_id
                      WHERE s.start_time BETWEEN @startDate AND @endDate
                      GROUP BY c.customer_id, c.first_name, c.last_name
                      ORDER BY total_spent DESC",
                    connection))
                {
                    command.Parameters.AddWithValue("@startDate", startDate);
                    command.Parameters.AddWithValue("@endDate", endDate);
                    using (var adapter = new NpgsqlDataAdapter(command))
                    {
                        await Task.Run(() => adapter.Fill(dt));
                    }
                }
            }
            return dt;
        }

        public async Task<DataTable> GetProductSalesReportAsync(DateTime startDate, DateTime endDate)
        {
            var dt = new DataTable();
            using (var connection = await GetConnectionAsync())
            {
                using (var command = new NpgsqlCommand(
                    @"SELECT p.name as product_name,
                             COUNT(s.sale_id) as total_sales,
                             SUM(s.quantity) as total_quantity,
                             SUM(total_amount) as total_revenue
                      FROM products p
                      LEFT JOIN sales s ON p.product_id = s.product_id
                      WHERE s.sale_date BETWEEN @startDate AND @endDate
                      GROUP BY p.product_id, p.name
                      ORDER BY total_revenue DESC",
                    connection))
                {
                    command.Parameters.AddWithValue("@startDate", startDate);
                    command.Parameters.AddWithValue("@endDate", endDate);
                    using (var adapter = new NpgsqlDataAdapter(command))
                    {
                        await Task.Run(() => adapter.Fill(dt));
                    }
                }
            }
            return dt;
        }
    }
} 