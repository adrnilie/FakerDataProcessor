using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using FakerDataProcessor.Core;
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
                    var data = JsonConvert.DeserializeObject<List<User>>(responseBody);

                    Console.WriteLine("Adding Users to Database");
                    InsertDataIntoDb(data);
                    Console.WriteLine("Done. Press any key to exit.");
                }
            }
        }

        public static void InsertDataIntoDb(List<User> usersToInsert)
        {
            using (var context = new WebApiDbEntities())
            {
                foreach (var user in usersToInsert)
                {
                    context.Users.Add(user);
                    context.SaveChanges();
                }
            }
        }
    }
}
