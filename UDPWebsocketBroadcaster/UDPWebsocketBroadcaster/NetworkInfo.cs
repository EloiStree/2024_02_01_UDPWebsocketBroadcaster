using System.Net;
using System.Net.NetworkInformation;

class NetworkInfo
{


    static string GetRouterPublicIpAddress()
    {
        try
        {
            using (WebClient webClient = new WebClient())
            {
                // Make a request to a service that echoes back the public IP
                string response = webClient.DownloadString("https://api64.ipify.org?format=json");

                // Parse the JSON response to extract the public IP
                int startIndex = response.IndexOf("\"ip\":") + 6;
                int endIndex = response.IndexOf("\"", startIndex);
                string routerPublicIpAddress = response.Substring(startIndex, endIndex - startIndex);

                return routerPublicIpAddress;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving router's public IP address: {ex.Message}");
            return "Error";
        }
    }
    public static void DisplayConnectedLocalIPs()
    {
        Console.WriteLine("Public IP of the network:" + GetRouterPublicIpAddress());
        Console.WriteLine("Connected Local IP Addresses:");

        // Get all network interfaces on the computer
        NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

        foreach (NetworkInterface nic in networkInterfaces)
        {
            // Check if the network interface is up and operational
            if (nic.OperationalStatus == OperationalStatus.Up)
            {
                // Display information for the connected network interface
                Console.WriteLine($"Interface: {nic.Description}");
                Console.WriteLine($"  Type: {nic.NetworkInterfaceType}");

                // Display all connected IP addresses for the current network interface
                IPInterfaceProperties ipProperties = nic.GetIPProperties();
                foreach (UnicastIPAddressInformation ip in ipProperties.UnicastAddresses)
                {
                    Console.WriteLine($"  IP Address: {ip.Address}");
                }

                Console.WriteLine();
            }
        }
    }

    // Check if an IP address is public
    private static bool IsPublicIPAddress(IPAddress ipAddress)
    {
        byte[] ipBytes = ipAddress.GetAddressBytes();

        // Check if the IP address falls into one of the reserved ranges for private IPs
        if ((ipBytes[0] == 10) ||
            (ipBytes[0] == 172 && ipBytes[1] >= 16 && ipBytes[1] <= 31) ||
            (ipBytes[0] == 192 && ipBytes[1] == 168))
        {
            return false; // Private IP
        }

        // Check if the IP address is loopback
        if (IPAddress.IsLoopback(ipAddress))
        {
            return false; // Loopback IP
        }

        // Check if the IP address is link-local or site-local
        if (ipAddress.IsIPv6LinkLocal || ipAddress.IsIPv6SiteLocal)
        {
            return false; // Link-local or site-local IP
        }

        return true; // Public IP
    }

}
