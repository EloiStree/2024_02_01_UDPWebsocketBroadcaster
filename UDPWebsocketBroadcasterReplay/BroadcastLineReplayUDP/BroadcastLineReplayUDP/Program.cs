using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;


public struct TimeLine
{



    public double m_seconds;
    public string m_line;
}

class Program
{
    public static UdpClient udpClient;
    static void Main()
    {

        System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

        // Create a UDP client
        using (udpClient = new UdpClient())
        {
            // Specify the file path
            string filePath = "C:\\Life\\HelloRC\\HelloQARC\\Replay\\ReplayNew.HeRCReplay";
            string [] fullFile = File.ReadAllLines(filePath);
            List<TimeLine> lines = new List<TimeLine>();
            foreach (string line in fullFile)
            {
                if (line.IndexOf("#") > 0)
                {
                    int index = -1;
                    for (int i = 0; i < line.Length; i++)
                    {
                        if (line[i] == '#')
                        {
                            index = i;
                            break;
                        }
                    }
                    if (index > 0)
                    {
                        string t = line.Substring(0, index);
                        string value = line.Substring(index + 1);
                        if (double.TryParse(t, out double v))
                        {
                            Console.WriteLine($"{v}: {value}");
                            lines.Add(new TimeLine() { m_seconds = v, m_line = value });
                        }
                    }
                }
            }
            lines = lines.OrderBy(k => k.m_seconds).ToList();

            double maxtime = lines[lines.Count - 1].m_seconds + 1;
            try
            {
                DateTime m_start = DateTime.Now;
                DateTime m_current = DateTime.Now;
                double previousRelative = 0;
                double currentRelative=0;
                while (true)
                {
                    m_current = DateTime.Now;
                    double time = (m_current - m_start).TotalSeconds;
                    currentRelative = time % maxtime;
                    currentRelative = Math.Round(currentRelative, 3);

                    if (currentRelative != previousRelative)
                    {
                        foreach (var item in lines.Where(k => k.m_seconds > previousRelative && k.m_seconds <= currentRelative))
                        {
                            Push(item.m_line, currentRelative);
                        }
                        previousRelative = currentRelative;
                    }
                    Thread.Sleep(1);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
    }
    public static void Push(string line, double relativeT)
    {
        byte[] data = Encoding.UTF8.GetBytes(line);
        udpClient.Send(data, data.Length, new IPEndPoint(IPAddress.Parse("127.0.0.1"), 7071));
        Console.WriteLine($"Sent{(int)relativeT}: {line}");

    }
}
