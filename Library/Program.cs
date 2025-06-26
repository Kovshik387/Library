using System;
using System.Data.OleDb;
using Library.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Library
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var services = new ServiceCollection();
            
            services.AddRepositories();
            services.AddServices();
            services.AddConfiguration();
            
            var serviceProvider = services.BuildServiceProvider();
            
            var service = serviceProvider.GetService<IDictionaryService>();
            service.Rebuild();
            
            Ui();
        }

        private static void Ui()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Построение словарей завершено успешно!");
            Console.ReadKey();
        } 
    }
    
}