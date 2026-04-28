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

        /*
         C - Critical Error: A server issue that will return the algorithm to it's nearest safe state
         S - Selection Error: Issue with data/option selected, will return the algorithm to the selection stage
         W - Warning: A non critical issue that will not affect the algorithm but should be noted
         */
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
