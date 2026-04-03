using System;
using System.Collections.Generic;

namespace AntConfigurer
{
    class Strings
    {
        protected static List<String> LastErrors = new List<string>();

        public static Boolean ValidateIpV4(String ip)
        {
            List<String> errors = new List<string>();
            ip = ip.Trim();

            if (String.IsNullOrWhiteSpace(ip))
                errors.Add("Please, provide an IP address");

            char delimeter = '.';
            String[] octets = ip.Split(delimeter);

            if (octets.Length != 4)
                errors.Add("Incorrect IP address");
            
            else
            {
                Boolean correct = true;
                Int16 maxValue = 255;
                Int32 tmp;
                
                foreach (String octet in octets)
                {
                    if (octet.Length > 3)
                    {
                        correct = false;
                        break;
                    }

                    try
                    {
                        tmp = Int32.Parse(octet);
                        
                        if (tmp < 0 || tmp > maxValue)
                        {
                            correct = false;
                            break;
                        }
                    } 
                    catch(Exception)
                    {
                        correct = false;
                        break;
                    }
                }

                if (!correct)
                    errors.Add("Incorrect IP address");
            }

            Strings.LastErrors = errors;
            return errors.Count < 1;
        }

        public static String ZeroFill(String stringToFill, Int32 length = 3)
        {
            if (stringToFill.Length >= length)
                return stringToFill;

            while(stringToFill.Length < length)
                stringToFill = "0" + stringToFill;

            return stringToFill;
        }

        public static List<String> GetLastErrors()
        {
            return Strings.LastErrors;
        }

        public static Boolean CastStringToBoolean(String stringToCast)
        {
            if (String.IsNullOrWhiteSpace(stringToCast))
                return false;

            switch (stringToCast.ToLower())
            {
                case "true": return true;                    
            }

            return false;
        }
    }
}
