using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Lesson3_CNLTWeb.Models;

namespace Lesson3_CNLTWeb.Data
{
    public static class BookRepository
    {
        private static string masterConnectionString;
        private static string connectionString;

        // Hàm này để nhận chuỗi kết nối từ file appsettings.json truyền vào
        public static void Initialize(string masterConn, string mainConn)
        {
            masterConnectionString = masterConn;
            connectionString = mainConn;

            // Tự động dựng DB và Bảng ngay khi ứng dụng kích hoạt
            InitializeDatabase();
        }

        private static void InitializeDatabase()
        {
            // 1. Tạo CSDL BookManagement nếu chưa có
            string createDbQuery = "IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'BookManagement') BEGIN CREATE DATABASE BookManagement; END";
            using (SqlConnection masterConn = new SqlConnection(masterConnectionString))
            {
                masterConn.Open();
                using (SqlCommand cmd = new SqlCommand(createDbQuery, masterConn)) { cmd.ExecuteNonQuery(); }
            }

            // 2. Tạo bảng Book nếu chưa có (Id, Name, Author, Price)
            string createTableQuery = @"
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Book')
                BEGIN
                    CREATE TABLE Book (
                        Id INT IDENTITY(1,1) PRIMARY KEY,
                        Name NVARCHAR(200) NOT NULL,
                        Author NVARCHAR(200) NULL,
                        Price DECIMAL(18,2) NOT NULL
                    );
                END";
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(createTableQuery, conn)) { cmd.ExecuteNonQuery(); }

                // Add Author column if table exists but column missing (for upgrades)
                string addAuthorColumn = @"
                    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Book')
                    BEGIN
                        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Book') AND name = 'Author')
                        BEGIN
                            ALTER TABLE Book ADD Author NVARCHAR(200) NULL;
                        END
                    END";
                using (SqlCommand cmd2 = new SqlCommand(addAuthorColumn, conn)) { cmd2.ExecuteNonQuery(); }
            }
        }

        // --- CÁC HÀM CRUD ---
        // Get all books with optional search (by name) and sorting by price
        public static List<Book> GetAllBooks(string? search = null, string? sortOrder = null)
        {
            List<Book> books = new List<Book>();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                // Build SQL with optional WHERE and ORDER BY
                string sql = "SELECT Id, Name, Author, Price FROM Book";
                if (!string.IsNullOrWhiteSpace(search))
                {
                    sql += " WHERE Name LIKE @search";
                }
                if (!string.IsNullOrWhiteSpace(sortOrder))
                {
                    if (sortOrder == "price_asc") sql += " ORDER BY Price ASC";
                    else if (sortOrder == "price_desc") sql += " ORDER BY Price DESC";
                }

                SqlCommand cmd = new SqlCommand(sql, conn);
                if (!string.IsNullOrWhiteSpace(search))
                {
                    cmd.Parameters.AddWithValue("@search", "%" + search + "%");
                }

                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        books.Add(new Book
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            Name = reader["Name"].ToString(),
                            Author = reader["Author"] == DBNull.Value ? null : reader["Author"].ToString(),
                            Price = Convert.ToDecimal(reader["Price"])
                        });
                    }
                }
            }
            return books;
        }

        public static Book? GetBookById(int id)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("SELECT Id, Name, Author, Price FROM Book WHERE Id = @Id", conn);
                cmd.Parameters.AddWithValue("@Id", id);
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new Book
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            Name = reader["Name"].ToString(),
                            Author = reader["Author"] == DBNull.Value ? null : reader["Author"].ToString(),
                            Price = Convert.ToDecimal(reader["Price"])
                        };
                    }
                }
            }
            return null;
        }

        public static bool AddBook(Book book)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "INSERT INTO Book (Name, Author, Price) VALUES (@Name, @Author, @Price)";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Name", book.Name ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Author", book.Author ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Price", book.Price);
                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public static bool UpdateBook(Book book)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "UPDATE Book SET Name = @Name, Author = @Author, Price = @Price WHERE Id = @Id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", book.Id);
                cmd.Parameters.AddWithValue("@Name", book.Name ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Author", book.Author ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Price", book.Price);
                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public static bool DeleteBook(int id)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("DELETE FROM Book WHERE Id = @Id", conn);
                cmd.Parameters.AddWithValue("@Id", id);
                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }
    }
}