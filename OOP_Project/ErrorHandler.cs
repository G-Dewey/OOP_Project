using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ErrorOr;

namespace OOP_Project
{
    internal class ErrorHandler
    {
        public static bool CheckError(ErrorOr<object> item, char severity)
        {
            if (item.IsError)
            {
                var errorMessage = string.Join(", ", item.Errors.Select(e => e.Description));
                switch (severity)
                {
                    case 'C':
                        Critical(errorMessage);
                        return true;
                    case 'W':
                        Warning(errorMessage);
                        return true;
                    default:
                        Console.WriteLine($"Unknown error severity: {severity}");
                        return true;
                }
            }
            return false;
        }

        public static void Critical(string message)
        {
            Console.WriteLine($"CRITICAL ERROR: {message}");
            // Additional logic for critical errors (e.g., logging, alerting) can be added here
        }

        public static void Warning(string message)
        {
            Console.WriteLine($"WARNING: {message}");
            // Additional logic for warnings (e.g., logging) can be added here
        }
    }
}
