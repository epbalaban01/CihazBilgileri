using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.DirectoryServices.AccountManagement;
using System.Threading;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace CihazBilgileri
{
    class Program
    {
        static void Main(string[] args)
        {


            Console.ForegroundColor = ConsoleColor.Red;
            // Yazılım bilgisi
            Console.WriteLine("::============================================================================================::");
            Console.WriteLine("::                                                                                            ::");
            Console.WriteLine("::                                    CİHAZ BİLGİLERİ                                         ::");
            Console.WriteLine("::                                                                                            ::");
            Console.WriteLine("::============================================================================================::");
            Console.WriteLine();
            Console.ResetColor();

            Console.WriteLine("Cihaz Bilgileri yükleniyor...");
            Thread.Sleep(3000); // 3 saniye bekletme
            Console.Clear();

            Console.ForegroundColor = ConsoleColor.Red;
            // Yazılım bilgisi
            Console.WriteLine("::============================================================================================::");
            Console.WriteLine("::                                                                                            ::");
            Console.WriteLine("::                                    CİHAZ BİLGİLERİ                                         ::");
            Console.WriteLine("::                                                                                            ::");
            Console.WriteLine("::============================================================================================::");
            Console.WriteLine();
            Console.ResetColor();

            // Date and time stamp
            string fullstamp = DateTime.Now.ToString("dd-MM-yyyy_HH.mm");

            // Network adaptör bilgilerini alma
            List<string> wifiAdapters = GetNetworkAdapters("Wi-Fi");
            List<string> ethernetAdapters = GetNetworkAdapters("Ethernet");



            string markaName = GetMarkaName();
            string markaModel = GetSystemFamily() + " (" + GetModelName() + ")";
            string markaSerialNumber = GetSerialNumber();


            string domainName = GetDomainName();
            string computerName = Environment.MachineName;
            string userName = Environment.UserName;
            string osName = GetOSName();
            string ipAddress = GetIPAddress();
            string lastBootTime = GetLastBootTime();
            string uptime = GetUptime();
            string biosPostTime = GetBiosPostTime();

            string cpuInfo = GetCpuInfo();
            string cpuUretici = GetCpuUretici();
            string cpuMimarisi = GetCpuMimarisi();
            string cpuCekirdekSayisi = GetCpuCekirdekSayisi();
            string cpuMantiksalSayisi = GetCpuMantiksalSayisi();

            string totalRam = GetTotalRam();
            string totalSlots = GetTotalSlots();
            string emptySlots = GetEmptySlots();
            string ramType = GetRamType();
            string ramSpeed = GetRamSpeed();

            string gpuInfo = GetGpuInfo();
            

            string installDate = GetInstallDate();
            string fulluser = GetFullUserName();

            string computerType = GetComputerType();

            //Disk
            string diskInfoString = GetDiskInfos();

            string separator = new string('-', 95);

            // Başlık
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("::============================================================================================::");
            sb.AppendLine("::                                                                                            ::");
            sb.AppendLine("::                                    CİHAZ BİLGİLERİ                                         ::");
            sb.AppendLine("::                                                                                            ::");
            sb.AppendLine("::============================================================================================::");
            sb.AppendLine();


            string[] messageLines = new string[]
            {

            $"{"Marka Adı:",-30} {markaName}",
            $"{"Model Adı:",-30} {markaModel}",
            $"{"Seri Numarası:",-30} {markaSerialNumber}",
             separator,
            $"{"Domain Adı:",-30} {domainName}",
            $"{"Kullanıcı Adı:",-30} {fulluser}",
            $"{"Bilgisayar İsmi:",-30} {computerName}",
             $"{"Bilgisayar Tipi:",-30} {computerType}",
            $"{"Son Format Tarihi:",-30} {installDate}",
            $"{"İşletim Sistemi:",-30} {osName}",
            $"{"IP Adresi:",-30} {ipAddress}",
            separator,
            $"{"Son Açılış Tarihi:",-30} {lastBootTime}",
            $"{"Çalışma Süresi:",-30} {uptime}",
            $"{"Son BIOS Zamanı Süresi:",-30} {biosPostTime}",
            separator,
            gpuInfo,
            separator,
            diskInfoString,
            separator,
            $"{"İşlemci Üreticisi:",-30} {cpuUretici}",
            $"{"İşlemci İsmi:",-30} {cpuInfo}",
            $"{"CPU Mimarisi:",-30} {cpuMimarisi}",
            $"{"İşlemci Çekirdek Sayısı:",-30} {cpuCekirdekSayisi}",
            $"{"İşlemci Mantıksal Sayısı:",-30} {cpuMantiksalSayisi} ",
            separator,
            $"{"Toplam RAM:",-30} {totalRam} {ramType} {ramSpeed}",
            $"{"Toplam Slot:",-30} {totalSlots}",
            $"{"Boş Slot:",-30} {emptySlots}",
            separator,
            $"{"Wi-Fi:",-30} {string.Join(Environment.NewLine, wifiAdapters)}",
            $"{"Ethernet:", -30} {string.Join(Environment.NewLine, ethernetAdapters)}",


            };

            // Diziyi ekrana yazdırma
            foreach (var line in messageLines)
            {
                Console.WriteLine(line);
                sb.AppendLine(line); // Yazılan her satırı StringBuilder'a ekleme
                Thread.Sleep(100); // Her satır arasında kısa bir bekleme süresi var
            }


            // Dosyayı kaydetme işlemi
            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), $"{computerName}-{fullstamp}.txt");
            File.WriteAllText(filePath, sb.ToString());

            Console.WriteLine();
            Console.WriteLine($"Cihaz bilgileri masaüstüne '{computerName}-{fullstamp}.txt' konumuna kaydedildi.");
            Console.WriteLine("\nKapatmak için herhangi bir tuşa basın veya pencereyi kapatın...");

            Console.ReadKey();



        }

        #region Disk Bilgileri

        static string GetDiskInfos()
        {
            string diskInfos = string.Empty;
            int diskCounter = 1;

            try
            {
                // WMI yönetim kapsamını oluştur
                ManagementScope scope = new ManagementScope(@"\\.\root\Microsoft\Windows\Storage");
                scope.Connect();

                // MSFT_PhysicalDisk sınıfı için sorgu oluştur
                ManagementObjectSearcher searcherPhysical = new ManagementObjectSearcher(scope, new ObjectQuery("SELECT * FROM MSFT_PhysicalDisk"));

                // Win32_DiskDrive sınıfı için sorgu oluştur
                ManagementObjectSearcher searcherWin32 = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive");

                // Disk bilgilerinin saklanacağı bir sözlük
                var diskDict = new Dictionary<string, string>();

                // MSFT_PhysicalDisk bilgilerini al
                foreach (ManagementObject queryObj in searcherPhysical.Get())
                {
                    string name = queryObj["FriendlyName"]?.ToString() ?? "Bilinmiyor";
                    string mediaType = "Bilinmiyor";

                    // Medya türünü belirle
                    switch (Convert.ToInt16(queryObj["MediaType"]))
                    {
                        case 3:
                            mediaType = "HDD";
                            break;

                        case 4:
                            mediaType = "SSD";
                            break;

                        case 5:
                            mediaType = "SCM";
                            break;

                        default:
                            mediaType = "Unspecified";
                            break;
                    }

                    diskDict[name] = mediaType;
                }

                // Win32_DiskDrive bilgilerini al
                foreach (ManagementObject obj in searcherWin32.Get())
                {
                    string name = obj["Model"]?.ToString() ?? "Unknown";
                    string size = (Convert.ToUInt64(obj["Size"]) / (1000 * 1000 * 1000)).ToString() + " GB";

                    // Adı ve türü eşleştir
                    if (diskDict.TryGetValue(name, out string mediaType))
                    {
                        diskInfos += $"{diskCounter + ".Disk Adı:",-30} {name}\n";
                        diskInfos += $"{diskCounter + ".Disk Boyutu:",-30} {size}\n";
                        diskInfos += $"{diskCounter + ".Disk Tipi:",-30} {mediaType}\n";
                        diskInfos += "\n";
                        diskCounter++;
                    }
                    else
                    {
                        diskInfos += $"{diskCounter + ".Disk Adı:",-30} {name}\n";
                        diskInfos += $"{diskCounter + ".Disk Boyutu:",-30} {size}\n";
                        diskInfos += $"{diskCounter + ".Disk Tipi:",-30} Bilinmiyor\n";
                        diskInfos += "\n";
                        diskCounter++;
                    }
                }
            }
            catch (ManagementException e)
            {
                // Hata işleme
                diskInfos = $"WMI Hatası: {e.Message}";
            }

            return diskInfos.TrimEnd('\n'); // Son boş satırı kaldırmak için
        }

        /* 
         static string GetDiskInfos()
        {
            string diskInfos = string.Empty;
            int diskCounter = 1;

        try
        {
        // WMI yönetim kapsamını oluştur
        ManagementScope scope = new ManagementScope(@"\\.\root\Microsoft\Windows\Storage");
        scope.Connect();

        // MSFT_PhysicalDisk sınıfı için sorgu oluştur
        ManagementObjectSearcher searcherPhysical = new ManagementObjectSearcher(scope, new ObjectQuery("SELECT * FROM MSFT_PhysicalDisk"));

        // Win32_DiskDrive sınıfı için sorgu oluştur
        ManagementObjectSearcher searcherWin32 = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive");

        // Disk bilgilerinin saklanacağı bir sözlük
        var diskDict = new Dictionary<string, string>();

        // MSFT_PhysicalDisk bilgilerini al
        foreach (ManagementObject queryObj in searcherPhysical.Get())
        {
            string name = queryObj["FriendlyName"]?.ToString() ?? "Unknown";
            string mediaType = "Unknown";

            // Medya türünü belirle
            switch (Convert.ToInt16(queryObj["MediaType"]))
            {
                case 3:
                    mediaType = "HDD";
                    break;

                case 4:
                    mediaType = "SSD";
                    break;

                case 5:
                    mediaType = "SCM";
                    break;

                default:
                    mediaType = "Unspecified";
                    break;
            }

            diskDict[name] = mediaType;
        }

        // Win32_DiskDrive bilgilerini al
        foreach (ManagementObject obj in searcherWin32.Get())
        {
            string name = obj["Model"]?.ToString() ?? "Unknown";
            string size = (Convert.ToUInt64(obj["Size"]) / (1000 * 1000 * 1000)).ToString() + " GB";

            // Adı ve türü eşleştir
            if (diskDict.TryGetValue(name, out string mediaType))
            {
                diskInfos += $"{diskCounter}. Disk Adı: {name}, Boyutu: {size}, Tipi: {mediaType}\n";
                diskCounter++;
            }
            else
            {
                diskInfos += $"{diskCounter}. Disk Adı: {name}, Boyutu: {size}, Tipi: Unknown\n";
                diskCounter++;
            }
        }
    }
    catch (ManagementException e)
    {
        // Hata işleme
        diskInfos = $"WMI Hatası: {e.Message}";
    }

    return diskInfos.TrimEnd('\n'); // Son boş satırı kaldırmak için
}

             
             
             
             
             */


        #endregion


        #region İnternet Bilgileri

        static string GetIPAddress()
        {
            string ipAddress = "İnternet bağlantısı yok veya sürücü yüklü değil.";
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT IPAddress FROM Win32_NetworkAdapterConfiguration WHERE IPEnabled = True");
                foreach (ManagementObject obj in searcher.Get())
                {
                    string[] addresses = (string[])obj["IPAddress"];
                    if (addresses.Length > 0)
                    {
                        ipAddress = addresses[0];
                    }
                    break;
                }
            }
            catch (Exception ex)
            {
                // Log or display the error message
                ipAddress = $"Hata: {ex.Message}";
            }
            return ipAddress;
        }

        static List<string> GetNetworkAdapters(string adapterType)
        {
            List<string> adapters = new List<string>();
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_NetworkAdapter WHERE NetConnectionID != NULL");
                foreach (ManagementObject obj in searcher.Get())
                {
                    string netConnectionId = obj["NetConnectionID"].ToString();
                    string name = obj["Name"].ToString();

                    // Filter out certain adapters like VirtualBox
                    if (name.Contains("VirtualBox"))
                    {
                        continue;
                    }

                    if (netConnectionId.ToLower().Contains(adapterType.ToLower()))
                    {
                        string macAddress = obj["MACAddress"]?.ToString();
                        if (!string.IsNullOrEmpty(macAddress))
                        {
                            adapters.Add($"{name} > {macAddress}");
                        }
                    }
                }

                if (adapters.Count == 0)
                {
                    adapters.Add("İnternet bağlantısı yok veya sürücü yüklü değil.");
                }
            }
            catch (Exception ex)
            {
                // Log or display the error message
                adapters.Add($"Hata: {ex.Message}");
            }
            return adapters;
        }

        static List<string> GetMacAddresses(string adapterType)
        {
            List<string> macAddresses = new List<string>();
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_NetworkAdapter WHERE NetConnectionID != NULL");
                foreach (ManagementObject obj in searcher.Get())
                {
                    string netConnectionId = obj["NetConnectionID"].ToString();
                    string name = obj["Name"].ToString();

                    // Filter out certain adapters like VirtualBox
                    if (name.Contains("VirtualBox"))
                    {
                        continue;
                    }

                    if (netConnectionId.ToLower().Contains(adapterType.ToLower()))
                    {
                        string macAddress = obj["MACAddress"]?.ToString();
                        if (!string.IsNullOrEmpty(macAddress))
                        {
                            macAddresses.Add(macAddress);
                        }
                    }
                }

                if (macAddresses.Count == 0)
                {
                    macAddresses.Add("İnternet bağlantısı yok veya sürücü yüklü değil.");
                }
            }
            catch (Exception ex)
            {
                // Log or display the error message
                macAddresses.Add($"Hata: {ex.Message}");
            }
            return macAddresses;
        }
        #endregion


        #region Bilgisayar Bilgileri

        static string GetFullUserName()
        {
            string fullName = string.Empty;
            string registryPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Authentication\LogonUI";
            string registryValueName = "LastLoggedOnDisplayName";

            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(registryPath))
                {
                    if (key != null)
                    {
                        // DisplayName anahtarını oku
                        object displayName = key.GetValue(registryValueName);
                        if (displayName != null)
                        {
                            fullName = displayName.ToString();
                        }
                        else
                        {
                            // DisplayName anahtarı null ise
                            fullName = $"'{registryValueName}' değeri bulunamadı.";
                        }
                    }
                    else
                    {
                        fullName = "Kayıt defteri anahtarı bulunamadı.";
                    }
                }
            }
            catch (Exception ex)
            {
                fullName = $"Kayıt defterine erişim hatası: {ex.Message}";
            }

            return fullName;
        }


        static string GetMarkaName()
        {
            string markaName = string.Empty;
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem");
            foreach (ManagementObject obj in searcher.Get())
            {
                markaName = obj["Manufacturer"].ToString();
            }
            return markaName;
        }

        static string GetModelName()
        {
            string modelName = string.Empty;
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem");
            foreach (ManagementObject obj in searcher.Get())
            {
                modelName = obj["Model"].ToString();
            }
            return modelName;
        }

        static string GetSerialNumber()
        {
            string markaSerialNumber = string.Empty;
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_BIOS");
            foreach (ManagementObject obj in searcher.Get())
            {
                markaSerialNumber = obj["SerialNumber"].ToString();
            }
            return markaSerialNumber;
        }

        static string GetSystemFamily()
        {
            string systemFamily = string.Empty;
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem");
            foreach (ManagementObject obj in searcher.Get())
            {
                systemFamily = obj["SystemFamily"].ToString();
            }
            return systemFamily;
        }
        static string GetDomainName()
        {
            string domainName = string.Empty;
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem");
            foreach (ManagementObject obj in searcher.Get())
            {
                domainName = obj["Domain"].ToString();
            }
            return domainName;
        }

        static string GetOSName()
        {
            string osName = string.Empty;
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT Caption FROM Win32_OperatingSystem");
            foreach (ManagementObject obj in searcher.Get())
            {
                osName = obj["Caption"].ToString();

                // "Microsoft" kelimesini çıkar
                osName = osName.Replace("Microsoft", "").Trim();
            }
            return osName;
        }


        static string GetLastBootTime()
        {
            string lastBootUpTime = string.Empty;
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT LastBootUpTime FROM Win32_OperatingSystem");
            foreach (ManagementObject obj in searcher.Get())
            {
                lastBootUpTime = ManagementDateTimeConverter.ToDateTime(obj["LastBootUpTime"].ToString()).ToString();
            }
            return lastBootUpTime;
        }

        static string GetUptime()
        {
            TimeSpan uptime = TimeSpan.Zero;
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT LastBootUpTime FROM Win32_OperatingSystem");
            foreach (ManagementObject obj in searcher.Get())
            {
                DateTime lastBootUpTime = ManagementDateTimeConverter.ToDateTime(obj["LastBootUpTime"].ToString());
                uptime = DateTime.Now - lastBootUpTime;
            }
            return $"{uptime.Days:D2}:{uptime.Hours:D2}:{uptime.Minutes:D2}:{uptime.Seconds:D2} (Gün:Saat:Dakika:Saniye)";
        }

        static string GetBiosPostTime()
        {
            string biosPostTime = string.Empty;
            try
            {
                var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Session Manager\Power");
                if (key != null)
                {
                    biosPostTime = ((int)key.GetValue("FwPOSTTime", 0) / 1000.0).ToString("F1") + " saniye";
                }
            }
            catch (Exception)
            {
                biosPostTime = "N/A";
            }
            return biosPostTime;
        }


        static string GetInstallDate()
        {
            string installDate = string.Empty;
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT InstallDate FROM Win32_OperatingSystem");
            foreach (ManagementObject obj in searcher.Get())
            {
                installDate = ManagementDateTimeConverter.ToDateTime(obj["InstallDate"].ToString()).ToString();
            }
            return installDate;
        }


        static string GetComputerType()
        {
            string computerType = "Bilinmiyor";
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_SystemEnclosure");
            foreach (ManagementObject obj in searcher.Get())
            {
                foreach (int type in (UInt16[])obj["ChassisTypes"])
                {
                    switch (type)
                    {
                        case 3:
                        case 4:
                        case 5:
                        case 6:
                        case 7:
                        case 15:
                        case 23:
                        case 24:
                            computerType = "Desktop";
                            break;
                        case 8:
                        case 9:
                        case 10:
                        case 11:
                        case 12:
                        case 14:
                        case 30:
                            computerType = "Notebook";
                            break;
                    }
                }
            }
            return computerType;
        }


        #endregion


        #region Cpu Bilgileri
        static string GetCpuInfo()
        {
            string cpuInfo = string.Empty;

            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT Name, Manufacturer FROM Win32_Processor");
            foreach (ManagementObject obj in searcher.Get())
            {
                cpuInfo = $"{obj["Name"]}";

            }
            return cpuInfo;
        }



        static string GetCpuUretici()
        {
            string cpuUretici = string.Empty;

            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT Name, Manufacturer FROM Win32_Processor");
            foreach (ManagementObject obj in searcher.Get())
            {
                cpuUretici = $"{obj["Manufacturer"]}";

            }
            return cpuUretici;
        }

        static string GetCpuCekirdekSayisi()
        {
            string cpuCekirdekSayisi = string.Empty;

            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");
            foreach (ManagementObject obj in searcher.Get())
            {
                cpuCekirdekSayisi = $"{obj["NumberOfCores"]}";

            }
            return cpuCekirdekSayisi;
        }
        static string GetCpuMantiksalSayisi()
        {
            string cpuMantiksalSayisi = string.Empty;

            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");
            foreach (ManagementObject obj in searcher.Get())
            {
                cpuMantiksalSayisi = $"{obj["NumberOfLogicalProcessors"]}";

            }
            return cpuMantiksalSayisi;
        }

        static string GetCpuMimarisi()
        {
            string cpuMimarisi = string.Empty;

            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT Architecture FROM Win32_Processor");
            foreach (ManagementObject obj in searcher.Get())
            {
                int architecture = Convert.ToInt32(obj["Architecture"]);
                switch (architecture)
                {
                    case 0:
                        cpuMimarisi = "x86";
                        break;
                    case 1:
                        cpuMimarisi = "MIPS";
                        break;
                    case 2:
                        cpuMimarisi = "Alpha";
                        break;
                    case 3:
                        cpuMimarisi = "PowerPC";
                        break;
                    case 6:
                        cpuMimarisi = "IA64";
                        break;
                    case 9:
                        cpuMimarisi = "x64";
                        break;
                    default:
                        cpuMimarisi = "Bilinmiyor";
                        break;
                }
            }
            return cpuMimarisi;
        }
        #endregion


        #region Ram Bilgileri

        static string GetRamSpeed()
        {
            string ramSpeed = string.Empty;
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMemory");
            foreach (ManagementObject obj in searcher.Get())
            {
                ramSpeed = obj["Speed"].ToString() + " MHz";
            }
            return ramSpeed;
        }


        static string GetRamType()
        {
            string ramType = "Bilinmiyor";
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT SMBIOSMemoryType FROM Win32_PhysicalMemory");
            foreach (ManagementObject obj in searcher.Get())
            {
                int memoryType = Convert.ToInt32(obj["SMBIOSMemoryType"]);

                switch (memoryType)
                {
                    case 20:
                        ramType = "DDR";
                        break;
                    case 21:
                        ramType = "DDR2";
                        break;
                    case 24:
                        ramType = "DDR3";
                        break;
                    case 26:
                        ramType = "DDR4";
                        break;
                    default:
                        ramType = "Bilinmiyor";
                        break;
                }
            }
            return ramType;
        }


        static string GetTotalRam()
        {
            double totalRam = 0;
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT Capacity FROM Win32_PhysicalMemory");
            foreach (ManagementObject obj in searcher.Get())
            {
                totalRam += Convert.ToDouble(obj["Capacity"]);
            }
            return (totalRam / (1024 * 1024 * 1024)).ToString() + "GB";
        }

        static string GetTotalSlots()
        {
            int totalSlots = 0;
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT MemoryDevices FROM Win32_PhysicalMemoryArray");
            foreach (ManagementObject obj in searcher.Get())
            {
                totalSlots = Convert.ToInt32(obj["MemoryDevices"]);
            }
            return totalSlots.ToString();
        }

        static string GetEmptySlots()
        {
            int usedSlots = 0;
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMemory");
            foreach (ManagementObject obj in searcher.Get())
            {
                usedSlots++;
            }

            int totalSlots = Convert.ToInt32(GetTotalSlots());
            int emptySlots = totalSlots - usedSlots;
            return emptySlots.ToString();
        }

        #endregion


        #region Ekran Kartı Bilgileri

        static string GetGpuInfo()
        {
            string gpuInfo = string.Empty;
            string gpuDriver = string.Empty;
            int gpuCounter = 1;

            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT Description, DriverVersion FROM Win32_VideoController");
                foreach (ManagementObject obj in searcher.Get())
                {
                    string description = obj["Description"]?.ToString() ?? "Bilinmiyor";
                    string description1 = obj["DriverVersion"]?.ToString() ?? "Bilinmiyor";

                    // (R) gibi sembolleri temizle
                    description = Regex.Replace(description, @"\(R\)|\(TM\)|\(C\)", "", RegexOptions.IgnoreCase).Trim();

                    gpuInfo += $"{gpuCounter + ". GPU:",-30} {description}\n";
                    gpuInfo += $"{gpuCounter + ". GPU Driver Version:",-30} {description1}\n";
                    gpuCounter++;
                }

                if (gpuCounter == 1) // Hiç GPU bulunamadıysa
                {
                    gpuInfo = "GPU bulunamadı veya sürücü yüklenmedi.";
                }
            }
            catch (ManagementException e)
            {
                // Hata işleme
                gpuInfo = $"WMI Hata: {e.Message}";
            }
            catch (Exception e)
            {
                // Diğer hatalar için genel hata işleme
                gpuInfo = $"Beklenmeyen Hata: {e.Message}";
            }

            return gpuInfo.TrimEnd('\n'); // Son boş satırı kaldırmak için
        }

        

        #endregion


    }
}
