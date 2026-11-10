using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;

namespace CampusFacilitiesIssueRegister
{
    // ADO.NET SQL repository: parameterised statements, one main database table.
    public sealed class IssueRepository
    {
        private const string DbName = "CampusFacilitiesIssuesDB";
        private const string LocalServer = @"(localdb)\MSSQLLocalDB";
        private static string ConnectionString
        {
            get { return @"Data Source=" + LocalServer + ";Initial Catalog=" + DbName + ";Integrated Security=True;Connect Timeout=30;"; }
        }

        public static string DatabaseFilePath { get; private set; }

        public static void InitializeDatabase()
        {
            // In a Visual Studio Debug build, walk up to the project folder.
            string path = AppDomain.CurrentDomain.BaseDirectory;
            DirectoryInfo dir = new DirectoryInfo(path);
            while (dir != null && !File.Exists(Path.Combine(dir.FullName, "CampusFacilitiesIssueRegister.csproj")))
                dir = dir.Parent;
            string projectFolder = dir == null ? path : dir.FullName;
            string folder = Path.Combine(projectFolder, "Database");
            Directory.CreateDirectory(folder);
            DatabaseFilePath = Path.Combine(folder, DbName + ".mdf");

            string master = @"Data Source=" + LocalServer + ";Initial Catalog=master;Integrated Security=True;Connect Timeout=30;";
            using (SqlConnection conn = new SqlConnection(master))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM sys.databases WHERE name = @name", conn))
                {
                    cmd.Parameters.Add("@name", SqlDbType.NVarChar, 128).Value = DbName;
                    bool exists = Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                    if (!exists)
                    {
                        // SQL identifiers and file paths are constants or locally generated, not user input.
                        string mdf = DatabaseFilePath.Replace("'", "''");
                        string ldf = Path.Combine(folder, DbName + "_log.ldf").Replace("'", "''");
                        string sql = "CREATE DATABASE [" + DbName + "] ON PRIMARY " +
                            "(NAME = N'" + DbName + "', FILENAME = N'" + mdf + "') " +
                            "LOG ON (NAME = N'" + DbName + "_log', FILENAME = N'" + ldf + "')";
                        using (SqlCommand create = new SqlCommand(sql, conn)) create.ExecuteNonQuery();
                    }
                }
            }
            using (SqlConnection conn = Open())
            using (SqlCommand cmd = new SqlCommand(@"
IF OBJECT_ID(N'dbo.FacilityIssues', N'U') IS NULL
BEGIN
 CREATE TABLE dbo.FacilityIssues (
  IssueId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
  Title NVARCHAR(100) NOT NULL,
  Location NVARCHAR(100) NOT NULL,
  Category NVARCHAR(50) NOT NULL,
  Priority NVARCHAR(20) NOT NULL,
  Status NVARCHAR(20) NOT NULL,
  ReportedBy NVARCHAR(100) NOT NULL,
  Description NVARCHAR(500) NOT NULL,
  ReportedDate DATE NOT NULL
 )
END", conn)) cmd.ExecuteNonQuery();
        }

        private static SqlConnection Open()
        {
            SqlConnection conn = new SqlConnection(ConnectionString);
            conn.Open();
            return conn;
        }

        public DataTable GetIssues(string search)
        {
            using (SqlConnection conn = Open())
            using (SqlCommand cmd = new SqlCommand(@"
SELECT IssueId, Title, Location, Category, Priority, Status, ReportedBy, Description, ReportedDate
FROM dbo.FacilityIssues
WHERE (@term = '' OR Title LIKE '%' + @term + '%' OR Location LIKE '%' + @term + '%' OR Status LIKE '%' + @term + '%')
ORDER BY IssueId DESC", conn))
            {
                cmd.Parameters.Add("@term", SqlDbType.NVarChar, 100).Value = (search ?? "").Trim();
                DataTable data = new DataTable();
                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd)) adapter.Fill(data);
                return data;
            }
        }

        private static void FillParameters(SqlCommand cmd, FacilityIssue issue)
        {
            cmd.Parameters.Add("@title", SqlDbType.NVarChar, 100).Value = issue.Title;
            cmd.Parameters.Add("@location", SqlDbType.NVarChar, 100).Value = issue.Location;
            cmd.Parameters.Add("@category", SqlDbType.NVarChar, 50).Value = issue.Category;
            cmd.Parameters.Add("@priority", SqlDbType.NVarChar, 20).Value = issue.Priority;
            cmd.Parameters.Add("@status", SqlDbType.NVarChar, 20).Value = issue.Status;
            cmd.Parameters.Add("@reportedBy", SqlDbType.NVarChar, 100).Value = issue.ReportedBy;
            cmd.Parameters.Add("@description", SqlDbType.NVarChar, 500).Value = issue.Description;
            cmd.Parameters.Add("@date", SqlDbType.Date).Value = issue.ReportedDate;
        }

        public void Add(FacilityIssue issue)
        {
            using (SqlConnection conn = Open())
            using (SqlCommand cmd = new SqlCommand(@"
INSERT INTO dbo.FacilityIssues (Title, Location, Category, Priority, Status, ReportedBy, Description, ReportedDate)
VALUES (@title, @location, @category, @priority, @status, @reportedBy, @description, @date)", conn))
            { FillParameters(cmd, issue); cmd.ExecuteNonQuery(); }
        }

        public bool Update(FacilityIssue issue)
        {
            using (SqlConnection conn = Open())
            using (SqlCommand cmd = new SqlCommand(@"
UPDATE dbo.FacilityIssues SET Title=@title, Location=@location, Category=@category, Priority=@priority,
Status=@status, ReportedBy=@reportedBy, Description=@description, ReportedDate=@date WHERE IssueId=@id", conn))
            {
                FillParameters(cmd, issue);
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = issue.IssueId;
                return cmd.ExecuteNonQuery() == 1;
            }
        }

        public bool Delete(int issueId)
        {
            using (SqlConnection conn = Open())
            using (SqlCommand cmd = new SqlCommand("DELETE FROM dbo.FacilityIssues WHERE IssueId=@id", conn))
            {
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = issueId;
                return cmd.ExecuteNonQuery() == 1;
            }
        }
    }
}
