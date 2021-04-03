using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OMS
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });

        public async static Task WriteLogFile(Exception exception)
        {
            if(exception == null)
            {
                ArgumentNullException argumentNull = new ArgumentNullException(nameof(exception), "Log File trying to be written to with no exception!");
                await WriteLogFile(argumentNull);
                return;
            }
            string log = DateTime.Now.ToString();
            log += ":\tType: " + exception.GetType().ToString();
            log += "\n\n" + exception.Message;
            string filename = "error.log";
            FileStream fileStream = new FileStream(filename, FileMode.Append, FileAccess.Write);
            char[] logChar = log.ToCharArray();
            byte[] data = new byte[logChar.Length];
            for (int i = 0; i < logChar.Length; i++)
            {
                data[i] = (byte)logChar[i];
            }
            await fileStream.WriteAsync(data);
            fileStream.Close();
        }
    }
}
