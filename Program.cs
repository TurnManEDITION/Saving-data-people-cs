// Install-package Newtonsoft.Json
using Newtonsoft.Json.Linq;
using System.Net;
using System.Text;
using System.Security.Cryptography;
using System.Globalization;


internal class Program
{
    public static void Main(string[] args)
    {
        string[] status;
        while (true)
        {
            Console.Clear();
            Console.Write("Enter method(Write|Read): ");
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            string answer = Console.ReadLine().ToLower();
            Console.Clear();
            Console.ResetColor();
            if (answer.Equals("write") | answer.Equals("w"))
            {
                status = write();
                if (status[1].Equals("green"))
                {
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                }
                else if (status[1].Equals("red"))
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                }
                Console.Clear();
                Console.WriteLine("Status: " + status[0]);
                Console.ResetColor();
                Thread.Sleep(3000);
                Console.Clear();
            }
            else if (answer.Equals("read") | answer.Equals("r"))
            {
                string[] snp = new string[3];
                byte i = 0;
                while (i != 3)
                {
                    do
                    {
                        string[] snp_choice = { "surname", "name", "patronymic" };
                        Console.Write("Enter " + snp_choice[i] + ": ");
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                        snp[i] = Console.ReadLine();
                        Console.ResetColor();
                        Console.Clear();
                    } while (snp[i].Equals(""));
                    i++;
                }
                string snp_data = snp[0] + snp[1] + snp[2];
                status = read(snp_data);
                if (status[1].Equals("green"))
                {
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                }
                else if (status[1].Equals("red"))
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                }
                Console.WriteLine("Status: " + status[0]);
                Console.ResetColor();
                Thread.Sleep(3000);
            }
        }
    }

    public static string[] write()
    {
        int max_day = 0;
        try
        {
            JObject data = JObject.Parse(File.ReadAllText("days.json"));
        }
        catch (Exception)
        {
            string url = "https://raw.githubusercontent.com/TurnManEDITION/Saving-data-people-cs/refs/heads/main/days.json";
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine("Error! File not found!");
            Console.ResetColor();
            Console.Write("Download file: ");
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine(url);
            Console.ResetColor();
            using (WebClient webclient = new WebClient())
            {
                try
                {
                    using (FileStream fs = File.Create(@"days.json"))
                    {
                        try
                        {
                            byte[] json = new UTF8Encoding(true).GetBytes(webclient.DownloadString(url));
                            fs.Write(json, 0, json.Length);
                            Thread.Sleep(3000);
                            Console.Clear();
                        }
                        catch (WebException)
                        {
                            Thread.Sleep(3000);
                            return ["not connection.", "red"];
                        }
                    }
                }
                catch (Exception)
                {
                    return ["failed to create file.", "red"];
                }
            }
        }
        string[] snp = new string[3];
        int[] dmy = { 0, 0, 0 };
        byte i = 0;
        while (i != 3)
        {
            do
            {
                string[] snp_choice = { "surname", "name", "patronymic" };
                Console.Write("Enter " + snp_choice[i] + ": ");
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                snp[i] = Console.ReadLine();
                Console.ResetColor();
                Console.Clear();
            } while (snp[i].Equals(""));
            i++;
        }
        i = 2;

        while (true)
        {
            string[] dmy_choice = { "day", "month", "year" };
            Console.Write("Enter birth " + dmy_choice[i] + ": ");
            while (true)
            {
                try
                {
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    dmy[i] = int.Parse(Console.ReadLine());
                    Console.ResetColor();
                    Console.Clear();
                    break;
                }
                catch (Exception)
                {
                    Console.Write("Enter birth " + dmy_choice[i] + ": ");
                }
            }
            if (i == 2)
            {
                i--;
            }
            if (i == 1 & 0 < dmy[1] & dmy[1] <= 12)
            {
                JObject data = JObject.Parse(File.ReadAllText("days.json"));
                if (dmy[2] % 4 == 0 && dmy[1] == 2)
                {
                    max_day = int.Parse(data["2"].ToString()) + 1;
                }
                else
                {
                    max_day = int.Parse(data["2"].ToString());
                }
                i--;
            }
            if (i == 0)
            {
                if (0 < dmy[i] & dmy[i] <= max_day)
                {
                    break;
                }
            }
        }
        string addition = "";
        string description = "";
        Console.WriteLine("Description: (exit to exit)");
        while (true)
        {
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            addition = Console.ReadLine();
            Console.ResetColor();
            if (addition.ToLower().Equals("exit"))
            {
                break;
            };
            description += addition + "\n";
        }
        string snp_data = snp[0] + snp[1] + snp[2];
        string name_file = gen_name_file(snp_data);
        string dmy_data = re_dmy(dmy[0].ToString(), dmy[1].ToString(), dmy[2].ToString());

        string all_data = data(snp, dmy_data, description);

        using (var file = File.Create(name_file + ".hex"))
        {
            string hashed_data = hashed(all_data);
            byte[] dataBytes = Encoding.UTF8.GetBytes(hashed_data);
            file.Write(dataBytes.AsSpan());
        }
        return ["the file was saved successfully.", "green"];
    }

    public static string[] read(string snp_data)
    {
        string str = "";
        string name_file = gen_name_file(snp_data);
        try
        {
            string hash = File.ReadAllText(name_file + ".hex");
            str = rehashed(hash);
            Console.WriteLine(str);
            Console.WriteLine("Press any button to exit");
            Console.ReadKey();
            Console.Clear();
            return ["file was read successfully", "green"];
        }
        catch (FileNotFoundException)
        {
            return ["file not found.", "red"];
        }

    }

    public static string gen_name_file(string snp)
    {
        byte i = 0;
        string hash = hash_sha256(snp);
        while (i != 10)
        {
            hash = hash.Replace($"{i}", "");
            i++;
        }
        return hash;
    }

    public static string hash_sha256(string data)
    {
        string hash = "";
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] hashValue = sha256.ComputeHash(Encoding.UTF8.GetBytes(data));
            foreach (byte b in hashValue)
            {
                hash += $"{b:X2}";
            }
        }
        return hash.ToLower();
    }

    public static string re_dmy(string d, string m, string y)
    {
        if (d.Length == 1)
        {
            d = "0" + d;
        }
        if (m.Length == 1)
        {
            m = "0" + m;
        }
        if (y.Length == 1)
        {
            y = "200" + y;
        }
        if (y.Length == 2 & int.Parse(y) <= 25)
        {
            y = "20" + y;
        }
        if (y.Length == 2 & int.Parse(y) > 25)
        {
            y = "19" + y;
        }
        return d + "." + m + "." + y;
    }

    public static string data(string[] snp, string dmy_data, string description)
    {
        return ("Surname: " + snp[0] + "\n" +
                "Name: " + snp[1] + "\n" +
                "Patronymic: " + snp[2] + "\n" +
                "Birth: " + dmy_data) + "\n" +
                "Description: \n" + description;
    }

    public static string hashed(string str)
    {
        string ascii = "";
        string hex = "";
        foreach (char c in str)
        {
            int unicode = c;
            ascii += unicode + " ";
        }
        string[] ascii_char = ascii.Split();
        for (long i = 0; i != ascii_char.Length - 1; i++)
        {
            long n = long.Parse(ascii_char[i]);
            string hexstr = n.ToString("X");
            hex += hexstr + " ";
        }
        return hex;
    }

    public static string rehashed(string hash)
    {
        string str = "";
        string ascii = "";
        string[] chars = hash.Split();
        for (long i = 0; i != chars.Length - 1; i++)
        {
            ascii += int.Parse(chars[i], NumberStyles.HexNumber) + " ";
        }
        chars = ascii.Split();
        for (long i = 0; i != chars.Length - 1; i++)
        {
            str += Convert.ToChar(int.Parse(chars[i]));
        }
        return str;
    }
}
