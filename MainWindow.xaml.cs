using CsvHelper.Configuration;
using CsvHelper;
using Microsoft.Data.SqlClient;
using Microsoft.VisualBasic.FileIO;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Transactions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MessageBox = System.Windows.MessageBox;
using AppThongKeDiemLmao_2._0.Entities;
using System.Collections.Generic;

namespace AppThongKeDiemLmao_2._0
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string filePath = "";
        private SqlConnection connection = null;
        private SqlTransaction transaction = null;

        private string connectionString = "Data Source=.;Initial Catalog=VietnamHighschoolExam;User ID=sa;Password=12345;TrustServerCertificate=true";
        public MainWindow()
        {
            InitializeComponent();
            GetSchoolYearComboBox();
            DatabaseConnection();
        }

        private void GetSchoolYearComboBox()
        {
            connection = new SqlConnection(connectionString);
            connection.Open();
            string selectQuery = "SELECT ExamYear FROM SchoolYear";
            using (SqlCommand cmd = new SqlCommand(selectQuery, connection))
            {
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int examYear = reader.GetInt32(reader.GetOrdinal("ExamYear"));
                        ComboBoxItem item = new ComboBoxItem();
                        item.Content = examYear;
                        cbSelectYear.Items.Add(item);
                    }
                }
            }
        }

        private void DatabaseConnection()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    MessageBox.Show("Kết nối đến cơ sở dữ liệu thành công!");
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Lỗi kết nối cơ sở dữ liệu: {ex.Message}");
            }
        }

        public async Task<List<StudentData>> ReadCsvFileAsync(string filepath, string examyear)
        {
            List<StudentData> studentdatalist = new List<StudentData>();
            using (TextFieldParser parser = new TextFieldParser(filepath))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");
                parser.ReadLine();
                while (!parser.EndOfData)
                {
                    string[] fields = parser.ReadFields();
                    if (fields != null && fields[6].Equals(examyear))
                    {
                        for (int i = 1; i <= 10; i++)
                        {
                            if (fields[i] == null || fields[i] == "")
                            {
                                fields[i] = "0";
                            }
                        }
                        // assuming you have a studentdata class to store the data
                        StudentData student = new StudentData
                        {
                            studentCode = fields[0],
                            toan = Convert.ToDouble(fields[1]),
                            van = Convert.ToDouble(fields[2]),
                            ly = Convert.ToDouble(fields[3]),
                            sinh = Convert.ToDouble(fields[4]),
                            ngoaiNgu = Convert.ToDouble(fields[5]),
                            hoa = Convert.ToDouble(fields[7]),
                            su = Convert.ToDouble(fields[8]),
                            dia = Convert.ToDouble(fields[9]),
                            gdcd = Convert.ToDouble(fields[10]),
                            year = Convert.ToInt32(fields[6]),
                            maTinh = Convert.ToInt32(fields[11])
                        };

                        studentdatalist.Add(student);
                    }
                }
            }

            return studentdatalist;
        }


        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
            ofd.Filter = "CSV Files (*.csv)|*.csv|All files (*.*)|*.*";
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                filePath = ofd.FileName;
                tbFolderPath.Text = filePath;

            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (cbSelectYear.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn năm học");
            }
            else
            {
                string selectedExamYear = (cbSelectYear.SelectedItem as ComboBoxItem).Content.ToString();

                // Bắt đầu đo thời gian cho phần đọc CSV
                Stopwatch stopwatchCsv = Stopwatch.StartNew();

                connection = new SqlConnection(connectionString);
                connection.Open();

                transaction = connection.BeginTransaction();
                int examYearId = await GetSchoolYearId(connection, transaction, Int32.Parse(selectedExamYear));
                bool isExist = IsExist();
                List<StudentData> studentDataList = await ReadCsvFileAsync(filePath, selectedExamYear);

                await InsertDataIntoDatabase(studentDataList);

                // Dừng đo thời gian cho phần đọc CSV
                stopwatchCsv.Stop();
                MessageBox.Show($"Thời gian đọc và ghi: {stopwatchCsv.Elapsed.TotalSeconds} giây");
            }
        }

        private static bool IsExist()
        {
            return true;
        }
        //private async void InsertDataIntoDatabase(List<StudentData> studentDataList)
        //{
        //    SqlConnection connection = null;
        //    SqlTransaction transaction = null;

        //    try
        //    {
        //        connection = new SqlConnection(connectionString);
        //        await connection.OpenAsync();

        //        transaction = connection.BeginTransaction();

        //        foreach (StudentData student in studentDataList)
        //        {
        //            int schoolYearId = GetSchoolYearId(connection, transaction, student.year);

        //            int studentId = InsertStudent(connection, transaction, student.studentCode, schoolYearId);

        //            double[] score = new double[9];
        //            score[0] = student.toan;
        //            score[1] = student.van;
        //            score[2] = student.ly;
        //            score[3] = student.sinh;
        //            score[4] = student.ngoaiNgu;
        //            score[5] = student.hoa;
        //            score[6] = student.su;
        //            score[7] = student.dia;
        //            score[8] = student.gdcd;

        //            for (int i = 1; i <= 9; i++)
        //            {
        //                InsertScore(connection, transaction, studentId, i, score[i - 1]);
        //                Console.WriteLine("Success =>>> " + i);
        //            }
        //        }

        //        transaction.Commit();
        //        MessageBox.Show("Dữ liệu đã được chèn thành công!");
        //    }
        //    catch (SqlException ex)
        //    {
        //        if (transaction != null)
        //        {
        //            transaction.Rollback();
        //        }
        //        MessageBox.Show($"Lỗi khi chèn dữ liệu: {ex.Message}");
        //    }
        //    finally
        //    {
        //        if (connection != null && connection.State == ConnectionState.Open)
        //        {
        //            connection.Close();
        //        }
        //    }
        //}

        //private async Task InsertDataIntoDatabase(List<StudentData> studentDataList)
        //{
        //    SqlConnection connection = null;
        //    SqlTransaction transaction = null;

        //    try
        //    {
        //        connection = new SqlConnection(connectionString);
        //        await connection.OpenAsync();

        //        transaction = connection.BeginTransaction();

        //        foreach (StudentData student in studentDataList)
        //        {
        //            int schoolYearId = GetSchoolYearId(connection, transaction, student.year);

        //            int studentId = InsertStudent(connection, transaction, student.studentCode, schoolYearId);

        //            double[] score = new double[9];
        //            score[0] = student.toan;
        //            score[1] = student.van;
        //            score[2] = student.ly;
        //            score[3] = student.sinh;
        //            score[4] = student.ngoaiNgu;
        //            score[5] = student.hoa;
        //            score[6] = student.su;
        //            score[7] = student.dia;
        //            score[8] = student.gdcd;

        //            StringBuilder query = new StringBuilder("INSERT INTO Score (StudentId, SubjectId, Score) VALUES ");

        //            for (int i = 1; i <= 9; i++)
        //            {
        //                query.Append($"({studentId}, {i}, {score[i - 1]}),");
        //            }

        //            // Remove the last comma and execute the query
        //            query.Length--;
        //            using (SqlCommand command = new SqlCommand(query.ToString(), connection, transaction))
        //            {
        //                await command.ExecuteNonQueryAsync();
        //            }
        //        }

        //        transaction.Commit();
        //        MessageBox.Show("Dữ liệu đã được chèn thành công!");
        //    }
        //    catch (SqlException ex)
        //    {
        //        if (transaction != null)
        //        {
        //            transaction.Rollback();
        //        }
        //        MessageBox.Show($"Lỗi khi chèn dữ liệu: {ex.Message}");
        //    }
        //    finally
        //    {
        //        if (connection != null && connection.State == ConnectionState.Open)
        //        {
        //            connection.Close();
        //        }
        //    }
        //}

        // how to insert data to database faster than my code

        private async Task InsertDataIntoDatabase(List<StudentData> studentDataList)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    using (SqlTransaction transaction = connection.BeginTransaction())
                    {
                        using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.TableLock, transaction))
                        {
                            bulkCopy.DestinationTableName = "Score";
                            bulkCopy.BatchSize = 500;
                            bulkCopy.BulkCopyTimeout = 60;
                            DataTable dt = new DataTable();
                            dt.Columns.Add("StudentId", typeof(int));
                            dt.Columns.Add("SubjectId", typeof(int));
                            dt.Columns.Add("Score", typeof(double));

                            foreach (StudentData student in studentDataList)
                            {
                                int schoolYearId = await GetSchoolYearId(connection, transaction, student.year);
                                int studentId = await InsertStudent(connection, transaction, student.studentCode, schoolYearId);

                                double[] score = new double[9];
                                score[0] = student.toan;
                                score[1] = student.van;
                                score[2] = student.ly;
                                score[3] = student.sinh;
                                score[4] = student.ngoaiNgu;
                                score[5] = student.hoa;
                                score[6] = student.su;
                                score[7] = student.dia;
                                score[8] = student.gdcd;

                                for (int i = 1; i <= 9; i++)
                                {
                                    dt.Rows.Add(studentId, i, score[i - 1]);
                                }

                                Console.WriteLine("Success insert =>>> " + student.studentCode);
                            }

                            await bulkCopy.WriteToServerAsync(dt);
                            transaction.Commit();
                            await connection.CloseAsync();
                        }
                    }
                }

                MessageBox.Show("Dữ liệu đã được chèn thành công!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi chèn dữ liệu: {ex.Message}");
            }
        }

        private async Task<int> GetSchoolYearId(SqlConnection connection, SqlTransaction transaction, int examYear)
        {
            string selectQuery = "SELECT Id FROM SchoolYear WHERE ExamYear = @Value";
            using (SqlCommand cmd = new SqlCommand(selectQuery, connection, transaction))
            {
                cmd.Parameters.AddWithValue("@Value", examYear);

                object result = cmd.ExecuteScalar();

                if (result != null)
                {
                    return Convert.ToInt32(result);
                }
                return 0;
            }
        }

        private async Task<int> InsertStudent(SqlConnection connection, SqlTransaction transaction, string studentCode, int schoolYearId)
        {
            string insertQuery = "INSERT INTO Student (StudentCode, SchoolYearId) VALUES (@Value1, @Value2)";
            using (SqlCommand cmd = new SqlCommand(insertQuery, connection, transaction))
            {
                cmd.Parameters.AddWithValue("@Value1", studentCode);
                cmd.Parameters.AddWithValue("@Value2", schoolYearId);

                int rowsAffected = cmd.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    string selectStudentIdQuery = "SELECT Id FROM Student WHERE StudentCode = @Value";
                    using (SqlCommand selectCmd = new SqlCommand(selectStudentIdQuery, connection, transaction))
                    {
                        selectCmd.Parameters.AddWithValue("@Value", studentCode);
                        object studentIdResult = selectCmd.ExecuteScalar();
                        return Convert.ToInt32(studentIdResult);
                    }
                }
                return 0;
            }
        }

        private void InsertScore(SqlConnection connection, SqlTransaction transaction, int studentId, int subjectId, double score)
        {
            string insertQuery = "INSERT INTO Score (StudentId, SubjectId, Score) VALUES (@Value1, @Value2, @Value3)";
            using (SqlCommand cmd = new SqlCommand(insertQuery, connection, transaction))
            {
                cmd.Parameters.AddWithValue("@Value1", studentId);
                cmd.Parameters.AddWithValue("@Value2", subjectId);
                cmd.Parameters.AddWithValue("@Value3", score);

                int rowsAffected = cmd.ExecuteNonQuery();

                if (rowsAffected == 0)
                {
                    throw new Exception("Không thể chèn điểm cho sinh viên.");
                }
            }
        }

    }
}