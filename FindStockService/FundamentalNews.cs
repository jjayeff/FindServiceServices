using HtmlAgilityPack;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FindStockService
{
    class FundamentalNews
    {
        // = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
        // | Config                                                          |
        // = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
        private static string DatabaseServer = ConfigurationManager.AppSettings["DatabaseServer"];
        private static string Database = ConfigurationManager.AppSettings["Database"];
        private static string Username = ConfigurationManager.AppSettings["DatabaseUsername"];
        private static string Password = ConfigurationManager.AppSettings["DatabasePassword"];
        private static Plog log = new Plog();
        // = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
        // | Model                                                           |
        // = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
        public class News
        {

            public News() { }

            // Properties.
            public string Symbol { get; set; }
            public string DateTime { get; set; }
            public string Source { get; set; }
            public string Headline { get; set; }
            public string Link { get; set; }
        }
        // = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
        // | Main Function                                                   |
        // = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
        public void Run()
        {
            List<string> symbols = new List<string>();

            // Get all symbol
            Symbol(ref symbols, "AGRI");
            Symbol(ref symbols, "FOOD");
            Symbol(ref symbols, "FASHION");
            Symbol(ref symbols, "HOME");
            Symbol(ref symbols, "PERSON");
            Symbol(ref symbols, "BANK");
            Symbol(ref symbols, "FIN");
            Symbol(ref symbols, "INSUR");
            Symbol(ref symbols, "AUTO");
            Symbol(ref symbols, "IMM");
            Symbol(ref symbols, "PAPER");
            Symbol(ref symbols, "PETRO");
            Symbol(ref symbols, "PKG");
            Symbol(ref symbols, "STEEL");
            Symbol(ref symbols, "CONMAT");
            Symbol(ref symbols, "PROP");
            Symbol(ref symbols, "PF%26REIT");
            Symbol(ref symbols, "CONS");
            Symbol(ref symbols, "ENERG");
            Symbol(ref symbols, "MINE");
            Symbol(ref symbols, "COMM");
            Symbol(ref symbols, "HELTH");
            Symbol(ref symbols, "MEDIA");
            Symbol(ref symbols, "PROF");
            Symbol(ref symbols, "TOURISM");
            Symbol(ref symbols, "TRANS");
            Symbol(ref symbols, "ETRON");
            Symbol(ref symbols, "ICT");

            for (var i = 0; i < symbols.Count; i++)
            {
                NewsScraping(symbols[i]);
            }

            log.LOGI("Success Update Data News");
        }
        // = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
        // | ScrapingWeb Function                                            |
        // = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
        private static void Symbol(ref List<string> symbols, string sector)
        {
            var url1 = $"https://marketdata.set.or.th/mkt/sectorquotation.do?sector={sector}&language=th&country=TH";

            // Using HtmlAgilityPack
            var Webget1 = new HtmlWeb();
            var doc1 = Webget1.Load(url1);

            try
            {
                foreach (HtmlNode node in doc1.DocumentNode.SelectNodes("//td//a"))
                {
                    string utf8_String = node.InnerText;
                    byte[] bytes = Encoding.UTF8.GetBytes(utf8_String);
                    utf8_String = Encoding.UTF8.GetString(bytes);
                    utf8_String = utf8_String.Replace("  ", String.Empty);
                    if (utf8_String.IndexOf("\n") >= 0)
                    {
                        utf8_String = utf8_String.Substring(2, utf8_String.Length - 4);
                        symbols.Add(utf8_String);
                    }
                }
            }
            catch
            {
                log.LOGE($"Not scraping data from {url1}");
            }
        }
        static public void NewsScraping(string symbol)
        {
            string symbol_url = symbol;
            if (symbol.IndexOf(" & ") > -1)
                symbol_url = symbol.Replace(" & ", "+%26+");
            else if (symbol.IndexOf("&") > -1)
                symbol_url = symbol.Replace("&", "%26");

            var url = $"https://www.set.or.th/set/companynews.do?symbol={symbol_url}&language=th&currentpage=0&ssoPageId=8&country=TH";

            // Using HtmlAgilityPack
            var Webget1 = new HtmlWeb();
            var doc1 = Webget1.Load(url);

            News result = new News();
            List<News> news = new List<News>();
            int index = 0;
            foreach (HtmlNode node in doc1.DocumentNode.SelectNodes("//table[@class='table table-hover table-info-wrap']"))
            {
                if (index++ == 1)
                    foreach (HtmlNode row in node.SelectNodes(".//tbody//tr"))
                    {
                        result = new News();
                        result.Symbol = symbol;
                        index = 0;
                        foreach (HtmlNode cell in row.SelectNodes(".//td"))
                        {
                            if (index == 0)
                                result.DateTime = ChangeDateFormat3(cell.InnerText.Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace("  ", ""));
                            else if (index == 2)
                                result.Source = cell.InnerText.Replace("\r\n", "").Replace("\n", "").Replace("\r", "");
                            else if (index == 3)
                                result.Headline = cell.InnerText.Replace("\r\n", "").Replace("\n", "").Replace("\r", "");
                            else if (index == 4)
                                result.Link = "https://www.set.or.th" + cell.SelectSingleNode(".//a[@href]").GetAttributeValue("href", string.Empty);
                            index++;
                        }
                        news.Add(result);
                    }

            }

            foreach (var value in news)
            {
                // Insert or Update datebase news
                StatementDatabase(value, "news", $"DateTime='{value.DateTime}' AND Symbol='{value.Symbol}' AND Headline='{value.Headline}'");
            }


            GC.Collect();
        }
        // = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
        // | Database Function                                               |
        // = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
        public static void UpdateDatebase(string sql, MySqlConnection cnn)
        {
            MySqlCommand command;
            MySqlDataAdapter adapter = new MySqlDataAdapter();

            try
            {
                command = new MySqlCommand(sql, cnn);
                adapter.UpdateCommand = new MySqlCommand(sql, cnn);
                adapter.UpdateCommand.ExecuteNonQuery();
                command.Dispose();
            }
            catch
            {
                log.LOGE($"{sql}");
            }
        }
        private static void InsertDatebase(string sql, MySqlConnection cnn)
        {
            MySqlCommand command;
            MySqlDataAdapter adapter = new MySqlDataAdapter();

            try
            {
                command = new MySqlCommand(sql, cnn);
                adapter.InsertCommand = new MySqlCommand(sql, cnn);
                adapter.InsertCommand.ExecuteNonQuery();
                command.Dispose();
            }
            catch
            {
                log.LOGE($"{sql}");
            }
        }
        public static void StatementDatabase(object item, string db, string where)
        {
            string sql = "";
            string connetionString;
            connetionString = $"Persist Security Info=False;server={DatabaseServer};database={Database};uid={Username};password={Password}";
            MySqlConnection cnn = new MySqlConnection(connetionString);
            MySqlCommand command = cnn.CreateCommand();

            sql = $"Select * from {db} where {where}";

            command.CommandText = sql;

            try
            {
                cnn.Open();
            }
            catch (Exception erro)
            {
                log.LOGE("Erro" + erro);
            }

            bool event_case = false;
            using (MySqlDataReader reader = command.ExecuteReader())
            {
                if (!reader.Read())
                {
                    sql = GetInsertSQL(item, db);
                    event_case = true;
                }
            }
            if (event_case)
                InsertDatebase(sql, cnn);

            cnn.Close();
        }
        public static string GetInsertSQL(object item, string db)
        {
            string sql = $"INSERT INTO {db} (:columns:) VALUES (:values:);";

            string[] columns = new string[item.GetType().GetProperties().Count()];
            string[] values = new string[item.GetType().GetProperties().Count()];
            int i = 0;
            foreach (var propertyInfo in item.GetType().GetProperties())
            {
                columns[i] = propertyInfo.Name;
                values[i++] = (string)(propertyInfo.GetValue(item, null));
            }

            //replacing the markers with the desired column names and values
            sql = FillColumnsAndValuesIntoInsertQuery(sql, columns, values);

            return sql;
        }
        public static string GetUpdateSQL(object item, string db, string whare)
        {
            string sql = $"UPDATE {db} SET :update: WHERE {whare} ;";

            string[] columns = new string[item.GetType().GetProperties().Count()];
            string[] values = new string[item.GetType().GetProperties().Count()];
            int i = 0;
            foreach (var propertyInfo in item.GetType().GetProperties())
            {
                columns[i] = propertyInfo.Name;
                values[i++] = (string)(propertyInfo.GetValue(item, null));
            }

            //replacing the markers with the desired column names and values
            sql = FillColumnsAndValuesIntoUpdateQuery(sql, columns, values);

            return sql;
        }
        public static string FillColumnsAndValuesIntoInsertQuery(string query, string[] columns, string[] values)
        {
            //joining the string arrays with a comma character
            string columnnames = string.Join(",", columns);
            //adding values with single quotation marks around them to handle errors related to string values
            string valuenames = ("'" + string.Join("','", values) + "'").Replace("''", "null");
            //replacing the markers with the desired column names and values
            return query.Replace(":columns:", columnnames).Replace(":values:", valuenames);
        }
        public static string FillColumnsAndValuesIntoUpdateQuery(string query, string[] columns, string[] values)
        {
            string result = "";
            for (int i = 0; i < columns.Length; i++)
                if (values[i] != null)
                    result += $"{columns[i]} = '{values[i]}'" + (i + 1 != columns.Length ? ", " : " ");
                else
                    result += $"{columns[i]} = null" + (i + 1 != columns.Length ? ", " : " ");
            //replacing the markers with the desired column names and values
            return query.Replace(":update:", result);
        }
        // = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
        // | Other    Function                                               |
        // = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
        public static string ChangeDateFormat(string date)
        {
            if (date == "null")
                return date;

            var parts = date.Split(' ');
            int mm;
            switch (parts[0])
            {
                case "January":
                    mm = 1;
                    break;
                case "February":
                    mm = 2;
                    break;
                case "March":
                    mm = 3;
                    break;
                case "April":
                    mm = 4;
                    break;
                case "May":
                    mm = 5;
                    break;
                case "June":
                    mm = 6;
                    break;
                case "July":
                    mm = 7;
                    break;
                case "August":
                    mm = 8;
                    break;
                case "September":
                    mm = 9;
                    break;
                case "October":
                    mm = 10;
                    break;
                case "November":
                    mm = 11;
                    break;
                default:
                    mm = 12;
                    break;
            }
            int dd = Convert.ToInt32(parts[1]);
            int yy = Convert.ToInt32(parts[2]);

            return $"{yy}-{mm}-{dd}";
        }
        public static string ChangeDateFormat3(string date)
        {
            if (date == "null")
                return date;

            var parts = date.Split(' ');
            int mm;
            switch (parts[1])
            {
                case "ม.ค.":
                    mm = 1;
                    break;
                case "ก.พ.":
                    mm = 2;
                    break;
                case "มี.ค.":
                    mm = 3;
                    break;
                case "เม.ย.":
                    mm = 4;
                    break;
                case "พ.ค.":
                    mm = 5;
                    break;
                case "มิ.ย.":
                    mm = 6;
                    break;
                case "ก.ค.":
                    mm = 7;
                    break;
                case "ส.ค.":
                    mm = 8;
                    break;
                case "ก.ย.":
                    mm = 9;
                    break;
                case "ต.ค.":
                    mm = 10;
                    break;
                case "พ.ย.":
                    mm = 11;
                    break;
                default:
                    mm = 12;
                    break;
            }
            int dd = Convert.ToInt32(parts[0]);
            int yy = Convert.ToInt32(parts[2]) - 543;

            if (parts.Length > 3)
                return $"{yy}-{mm}-{dd} {parts[3]}:00.000";

            else

                return $"{yy}-{mm}-{dd}";
        }
        public static string ChangeKaohoonDataIdFormat(string date, string symbol)
        {
            var parts = date.Split(' ');
            int mm;
            switch (parts[0])
            {
                case "January":
                    mm = 1;
                    break;
                case "February":
                    mm = 2;
                    break;
                case "March":
                    mm = 3;
                    break;
                case "April":
                    mm = 4;
                    break;
                case "May":
                    mm = 5;
                    break;
                case "June":
                    mm = 6;
                    break;
                case "July":
                    mm = 7;
                    break;
                case "August":
                    mm = 8;
                    break;
                case "September":
                    mm = 9;
                    break;
                case "October":
                    mm = 10;
                    break;
                case "November":
                    mm = 11;
                    break;
                default:
                    mm = 12;
                    break;
            }
            int dd = Convert.ToInt32(parts[1]);
            int yy = Convert.ToInt32(parts[2]);

            return $"{dd}{mm}{yy}{symbol}";
        }
        public static string[] CutString(string input)
        {

            var parts = input.Split(' ');

            return parts;
        }
    }
}
