using SimpleWebScraper;
using HtmlAgilityPack;
using CsvHelper;
using System.Globalization;
using System.Collections.Concurrent;

// Initializing HAP
var web = new HtmlWeb
{
    // Setting a global User-Agent header in HAP 
    UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36"
};

Console.WriteLine("Simple Web Scraper");
Console.WriteLine("Парсер настроен для адреса: https://marketing-tech.ru/company_tags/marketing/");
bool start = false;
while (start is false)
{
    Console.WriteLine("Введите команду:");
    Console.WriteLine("1: Начать парсинг");
    Console.WriteLine("2: Выход");
    var input = Console.ReadKey(true);
    
    switch (input.Key)
    {
        case ConsoleKey.D1:
            {
                Console.WriteLine("Запуск парсера!");
                start = true;
                break;
            }
        case ConsoleKey.NumPad1:
            {
                Console.WriteLine("Запуск парсера!");
                start = true;
                break;
            }
        case ConsoleKey.D2:
            {
                Environment.Exit(0);
                break;
            }
        case ConsoleKey.NumPad2:
            {
                Environment.Exit(0);
                break;
            }
        default:
            Console.WriteLine("Неверная команда, повторите!");
            break;
    }
}

// Load the target web page 
Console.WriteLine("Загружаем главную страницу...");
var mainPage = web.Load("https://marketing-tech.ru/company_tags/marketing/");

// Selecting all required HTML elements from the main page 
var highlightedCompaniesHTMLElements = mainPage.DocumentNode.QuerySelectorAll("section.page-section-compact.archive-item.single-company_highlighted-gray");
var companiesHTMLElements = mainPage.DocumentNode.QuerySelectorAll("section.page-section-compact.archive-item");
var pagingNumbers = mainPage.DocumentNode.QuerySelector("section.page-section-compact.archive-pagination").QuerySelectorAll("a.page-numbers").Last().InnerText;
int pagesNum = int.Parse(pagingNumbers);

Console.WriteLine($"Обнаружено страниц для парсинга: {pagesNum}");
Console.WriteLine("Парсим страницы, подождите...");

// Scrap companies pages
var companies = new ConcurrentBag<Company>();
foreach (var element in highlightedCompaniesHTMLElements)
{
    companies.Add(PageScraper.ScrapPage(element.QuerySelector("a").Attributes["href"].Value));
}
foreach (var element in companiesHTMLElements)
{
    companies.Add(PageScraper.ScrapPage(element.QuerySelector("a").Attributes["href"].Value));
}

// Form the list of remaining pages
var pagesToScrape = new ConcurrentBag<string>();
for (int i = 2; i <= pagesNum; i++)
{
    pagesToScrape.Add($"https://marketing-tech.ru/company_tags/marketing/page/{i}/");
}

// Repeat for remaining pages
Parallel.ForEach(
    pagesToScrape,
    new ParallelOptions { MaxDegreeOfParallelism = 8 },
    currentPage =>
    {
        var currentDocument = web.Load(currentPage);
        var currentDocumentElements = currentDocument.DocumentNode.QuerySelectorAll("section.page-section-compact.archive-item");
        foreach (var element in currentDocumentElements)
        {
            companies.Add(PageScraper.ScrapPage(element.QuerySelector("a").Attributes["href"].Value));
        }
    });

var result = companies.ToList();
result = (List<Company>)[.. result.DistinctBy(x => x.Name).OrderBy(x => x.Name)];
Console.WriteLine("Парсинг завершён, сохраняем выгрузку в файл...");

// Initializing the output file
string fileName = "marketing-companies.csv";
string filePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
using (var writer = new StreamWriter(Path.Combine(filePath, fileName)))

// Initializing the CSV writer
using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
{
    // Write companies to .csv file
    csv.WriteHeader<Company>();
    csv.NextRecord();
    foreach (var company in result)
    {
        csv.WriteRecord(company);
        csv.NextRecord();
    }
}

Console.WriteLine($"Файл сохранён в вашу папку Документы: {Path.Combine(filePath, fileName)}");
Console.WriteLine("Нажмите любую клавишу для выхода...");
Console.ReadKey(true);
Environment.Exit(0);
