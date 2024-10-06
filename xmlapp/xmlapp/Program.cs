using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

class Client
{
    public string FIO { get; set; }
    public string RegNumber { get; set; }
    public string DiasoftID { get; set; }
    public string Registrator { get; set; }
    public int RegistratorID { get; set; }
}

class Program
{
    static void Main(string[] args)
    {
        // Путь к входному XML-файлу
        string inputFilePath = "C:\\Users\\Public\\Documents\\Clients.xml";
        string outputFilePath = "C:\\Users\\Public\\Documents\\clients_output.xml";
        string registratorsFilePath = "C:\\Users\\Public\\Documents\\registrators_output.xml";
        string errorFilePath = "C:\\Users\\Public\\Documents\\error_report.txt";


        var clients = LoadClientsFromXml(inputFilePath);


        var (validClients, errorCounts) = ValidateClients(clients);


        CreateXml(validClients, outputFilePath);

        var registrators = CreateRegistratorsXml(validClients, registratorsFilePath);

        CreateErrorReport(errorCounts, errorFilePath);

        Console.WriteLine($"XML файл с валидными клиентами успешно создан по указаному пути: {outputFilePath}");
        Console.WriteLine($"XML файл со списком регистраторов успешно создан по указаному пути: {registratorsFilePath}");
        Console.WriteLine($"Текстовый файл с отчетом об ошибках успешно создан по указаному пути: {errorFilePath}");
    }

    static List<Client> LoadClientsFromXml(string filePath)
    {
        var clients = new List<Client>();
        var xdoc = XDocument.Load(filePath);

        foreach (var clientElement in xdoc.Descendants("Client"))
        {
            var client = new Client
            {
                FIO = (string)clientElement.Element("FIO"),
                RegNumber = (string)clientElement.Element("RegNumber"),
                DiasoftID = (string)clientElement.Element("DiasoftID"),
                Registrator = (string)clientElement.Element("Registrator")
            };

            clients.Add(client);
        }

        return clients;
    }

    static (List<Client>, Dictionary<string, int>) ValidateClients(List<Client> clients)
    {
        var validClients = new List<Client>();
        var errorCounts = new Dictionary<string, int>
        {
            { "Не указано FIO", 0 },
            { "Не указан RegNumber", 0 },
            { "Не указан DiasoftID", 0 },
            { "Не указан Registrator", 0 },
        };

        var registrators = new Dictionary<string, int>();
        int registratorId = 1;

        foreach (var client in clients)
        {
            if (!string.IsNullOrEmpty(client.FIO) &&
                !string.IsNullOrEmpty(client.RegNumber) &&
                !string.IsNullOrEmpty(client.DiasoftID) &&
                !string.IsNullOrEmpty(client.Registrator))
            {
                // Проверка регистратора в словаре
                if (!registrators.ContainsKey(client.Registrator))
                {
                    registrators[client.Registrator] = registratorId++;
                }

                // Добавляем клиента в валидный список
                client.RegistratorID = registrators[client.Registrator];
                validClients.Add(client);
            }
            else
            {
                if (string.IsNullOrEmpty(client.FIO))
                    errorCounts["Не указано FIO"]++;
                if (string.IsNullOrEmpty(client.RegNumber))
                    errorCounts["Не указан RegNumber"]++;
                if (string.IsNullOrEmpty(client.DiasoftID))
                    errorCounts["Не указан DiasoftID"]++;
                if (string.IsNullOrEmpty(client.Registrator))
                    errorCounts["Не указан Registrator"]++;
            }
        }

        return (validClients, errorCounts);
    }

    static void CreateXml(List<Client> clients, string filePath)
    {
        var xdoc = new XDocument(new XElement("Clients"));

        foreach (var client in clients)
        {
            var clientElement = new XElement("Client",
                new XElement("FIO", client.FIO),
                new XElement("RegNumber", client.RegNumber),
                new XElement("DiasoftID", client.DiasoftID),
                new XElement("Registrator", client.Registrator),
                new XElement("RegistratorID", client.RegistratorID));

            xdoc.Root.Add(clientElement);
        }

        xdoc.Save(filePath);
    }

    static Dictionary<string, int> CreateRegistratorsXml(List<Client> clients, string filePath)
    {
        var registrators = new Dictionary<string, int>();

        foreach (var client in clients)
        {
            if (!registrators.ContainsKey(client.Registrator))
            {
                registrators[client.Registrator] = client.RegistratorID;
            }
        }

        var xdoc = new XDocument(new XElement("Registrators"));

        foreach (var registrator in registrators)
        {
            var registratorElement = new XElement("Registrator",
                new XElement("Name", registrator.Key),
                new XElement("ID", registrator.Value));

            xdoc.Root.Add(registratorElement);
        }

        xdoc.Save(filePath);

        return registrators;
    }

    static void CreateErrorReport(Dictionary<string, int> errorCounts, string filePath)
    {
        using (var writer = new StreamWriter(filePath))
        {
            int totalErrors = errorCounts.Values.Sum();


            foreach (var error in errorCounts.OrderByDescending(e => e.Value))
            {
                if (error.Value > 0)
                {
                    writer.WriteLine($"{error.Key}: {error.Value} записей");
                }
            }

            writer.WriteLine($"Всего ошибочных записей: {totalErrors}");
        }
    }
}