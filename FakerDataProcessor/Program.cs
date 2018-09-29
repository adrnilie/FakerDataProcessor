using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Net.Http;
using System.Threading.Tasks;
using Dapper;
using FakerDataProcessor.Models;
using Newtonsoft.Json;

namespace FakerDataProcessor
{
    public class Program
    {
        private static readonly string FakerUsersUrl = "http://localhost:3000/Users";

        public static void Main(string[] args)
        {
            Task T = new Task(FakerCall);
            T.Start();
            Console.WriteLine("Json Data");
            Console.ReadLine();
        }

        public static async void FakerCall()
        {
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(FakerUsersUrl);
                response.EnsureSuccessStatusCode();

                using (var content = response.Content)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    var data = JsonConvert.DeserializeObject<List<Users>>(responseBody);

                    Console.WriteLine("Adding Users to Database");
                    InsertDataIntoDb(data);
                }
            }
        }

        public static void InsertDataIntoDb(List<Users> usersToInsert)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["DotNetWebApiDb"].ConnectionString;

            using (var db = new SqlConnection(connectionString))
            {
                foreach (var userToInsert in usersToInsert)
                {
                    var param = new DynamicParameters();
                    param.Add("firstName", userToInsert.FirstName);
                    param.Add("lastName", userToInsert.LastName);
                    param.Add("email", userToInsert.Email);
                    param.Add("username", userToInsert.Username);
                    param.Add("password", userToInsert.Password);

                    db.Execute(@"INSERT INTO [Users](FirstName, LastName, Email, Username, Password)
                                        VALUES(@firstName, @lastName, @email, @username, @password)", param, null, null, CommandType.Text);
                }
            }
        }
    }
}
