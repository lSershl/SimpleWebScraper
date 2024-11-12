using HtmlAgilityPack;

namespace SimpleWebScraper
{
    public static class PageScraper
    {
        public static Company ScrapPage(string pageUrl)
        {
            // Initializing HAP 
            var web = new HtmlWeb
            {
                // Setting a global User-Agent header in HAP 
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36"
            };

            // Loading the target web page
            var companyPage = web.Load($"{pageUrl}");

            // Select HTML elements from page
            var companyElements = companyPage.DocumentNode.QuerySelectorAll("div.container.wrap-main.company-page-2024");

            // Scrap required data
            string name = HtmlEntity.DeEntitize(companyPage.QuerySelector("span.breadcrumb_last").InnerText).Trim();

            string companyFlow = string.Empty;
            var companyFlowElement = companyPage.QuerySelector("div.company-flow");
            if (companyFlowElement is not null)
            {
                companyFlow = HtmlEntity.DeEntitize(companyFlowElement.FirstChild.NextSibling.InnerText).Trim();
            }

            string trustIndex = string.Empty;
            var trustIndexElement = companyPage.QuerySelector("div.mt-trust-index");
            if (trustIndexElement is not null)
            {
                trustIndex = HtmlEntity.DeEntitize(trustIndexElement.FirstChild.NextSibling.InnerText).Trim();
            }

            string found = string.Empty;
            string staff = string.Empty;
            string cases = string.Empty;
            var aboutCompanyTable = companyPage.QuerySelector("div.basic-information-table__column.basic-information-table__column_about").QuerySelectorAll("div.table-row");
            if (aboutCompanyTable is not null)
            {
                foreach (var element in aboutCompanyTable)
                {
                    switch (element.ChildNodes.ElementAt(1).InnerText.Trim())
                    {
                        case "Основана":
                            found = HtmlEntity.DeEntitize(element.ChildNodes.ElementAt(3).InnerText).Trim();
                            break;
                        case "Штат":
                            staff = HtmlEntity.DeEntitize(element.ChildNodes.ElementAt(3).InnerText).Trim();
                            break;
                        case "Кейсы":
                            cases = HtmlEntity.DeEntitize(element.ChildNodes.ElementAt(3).InnerText).Trim();
                            break;
                        default:
                            break;
                    }
                }
            }

            string specializations = string.Empty;
            var specializationElements = companyPage.QuerySelector("div.basic-information-table__column.basic-information-table__column_specials").QuerySelectorAll("a.column-link");
            if (specializationElements is not null)
            {
                foreach (var element in specializationElements)
                {
                    specializations = specializations + " " + HtmlEntity.DeEntitize(element.InnerText).Trim();
                }
            }

            string services = string.Empty;
            var servicesElements = companyPage.QuerySelector("div.basic-information-table__column.basic-information-table__column_services").QuerySelectorAll("a.column-link");
            if (servicesElements is not null)
            {
                foreach (var element in servicesElements)
                {
                    services = services + " " + HtmlEntity.DeEntitize(element.InnerText).Trim();
                }
            }
            
            string website = string.Empty;
            string address = string.Empty;
            string phone = string.Empty;
            string socialMediaLinks = string.Empty;
            string presentation = string.Empty;
            var contactElements = companyPage.QuerySelector("div.company-basics__table").QuerySelectorAll("div.table-row");
            if (contactElements is not null)
            {
                foreach (var element in contactElements)
                {
                    switch (element.ChildNodes.ElementAt(1).InnerText.Trim())
                    {
                        case "Сайт":
                            website = HtmlEntity.DeEntitize(element.ChildNodes.ElementAt(3).InnerText).Trim();
                            break;
                        case "Телефон":
                            phone = HtmlEntity.DeEntitize(element.ChildNodes.ElementAt(3).InnerText).Trim().Substring(18).Trim();
                            break;
                        case "Соц сети":
                            {
                                var socialMediaElements = element.QuerySelectorAll("a");
                                foreach (var link in socialMediaElements)
                                {
                                    socialMediaLinks = socialMediaLinks + " " + element.QuerySelector("a").Attributes["href"].Value;
                                }
                            }
                            break;
                        case "Адрес":
                            address = HtmlEntity.DeEntitize(element.ChildNodes.ElementAt(3).InnerText).Trim();
                            break;
                        case "Презентация":
                            presentation = element.QuerySelector("a").Attributes["href"].Value;
                            break;
                        default:
                            break;
                    }
                }
            }

            string aboutCompany = string.Empty;
            var aboutCompanyElements = companyPage.QuerySelector("div.tab-content").QuerySelector("p");
            if (aboutCompanyElements is not null)
            {
                aboutCompany = HtmlEntity.DeEntitize(aboutCompanyElements.InnerText.Trim());
            }

            return new Company
            {
                Name = name,
                Address = address,
                About = aboutCompany,
                Phone = phone,
                Website = website,
                SocialMedia = socialMediaLinks,
                Services = services,
                Specializations = specializations,
                Presentation = presentation,
                CompanyFlow = companyFlow,
                TrustIndex = trustIndex,
                Found = found,
                Staff = staff,
                Cases = cases
            };
        }
    }
}
