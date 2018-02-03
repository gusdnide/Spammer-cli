using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace Spammer
{
    class cTools
    {
        public static HtmlDocument Str2Html(string html)
        {
            WebBrowser browser = new WebBrowser();
            browser.ScriptErrorsSuppressed = true;
            browser.DocumentText = html;
            browser.Document.OpenNew(true);
            browser.Document.Write(html);
            browser.Refresh();
            return browser.Document;
        }
        public static string RetArray(string[] arr)
        {
            string Saida = "";
            foreach (string s in arr)
                Saida += s + Environment.NewLine;
            return Saida;
        }
        public static void Escrever(string Texto)
        {
            ConsoleColor corPad = Console.ForegroundColor;
            bool cor = false;
            for (int i = 0; i < Texto.Length; i++)
            {

                if ((Texto[i] == '%' && i + 1 < Texto.Length) || cor)
                {

                    if (!cor)
                    {
                        int Saida = -1;
                        if (int.TryParse(Texto[i + 1].ToString(), out Saida))
                        {
                            if (Saida > -1 && Saida <= 5)
                            {
                                switch (Saida)
                                {
                                    case 0:
                                        Console.ForegroundColor = ConsoleColor.Blue;
                                        break;
                                    case 1:
                                        Console.ForegroundColor = ConsoleColor.Red;
                                        break;
                                    case 2:
                                        Console.ForegroundColor = ConsoleColor.Yellow;
                                        break;
                                    case 3:
                                        Console.ForegroundColor = ConsoleColor.Green;
                                        break;
                                    case 4:
                                        Console.ForegroundColor = ConsoleColor.White;
                                        break;
                                    case 5:
                                        Console.ForegroundColor = ConsoleColor.Cyan;
                                        break;
                                    default:
                                        Console.ForegroundColor = ConsoleColor.Gray;
                                        break;
                                }
                            }
                        }
                    }
                    cor = !cor;
                    continue;
                }
                Console.Write(Texto[i]);
            }
            Console.ForegroundColor = corPad;
        }
        public static void EscreverLinha(string Texto)
        {
            ConsoleColor corPad = Console.ForegroundColor;
            bool cor = false;
            for (int i = 0; i < Texto.Length; i++)
            {

                if ((Texto[i] == '%' && i + 1 < Texto.Length) || cor)
                {

                    if (!cor)
                    {
                        int Saida = -1;
                        if (int.TryParse(Texto[i + 1].ToString(), out Saida))
                        {
                            if (Saida > -1 && Saida <= 5)
                            {
                                switch (Saida)
                                {
                                    case 0:
                                        Console.ForegroundColor = ConsoleColor.Blue;
                                        break;
                                    case 1:
                                        Console.ForegroundColor = ConsoleColor.Red;
                                        break;
                                    case 2:
                                        Console.ForegroundColor = ConsoleColor.Yellow;
                                        break;
                                    case 3:
                                        Console.ForegroundColor = ConsoleColor.Green;
                                        break;
                                    case 4:
                                        Console.ForegroundColor = ConsoleColor.White;
                                        break;
                                    case 5:
                                        Console.ForegroundColor = ConsoleColor.Cyan;
                                        break;
                                    default:
                                        Console.ForegroundColor = ConsoleColor.Gray;
                                        break;
                                }
                            }
                        }
                    }
                    cor = !cor;
                    continue;
                }
                Console.Write(Texto[i]);
            }
            Console.WriteLine();
            Console.ForegroundColor = corPad;
        }
        public static string[] getRegex(string reg, string resp)
        {
            List<string> Result = new List<string>();
            Regex r = new Regex(reg);
            MatchCollection ms = r.Matches(resp);
            if (ms.Count > 0)
            {
                foreach (Match m in ms)
                {
                    Result.Add(m.Value);
                }
            }
            return Result.ToArray();
        }

    
        public static string getResponse(string URL)
        {
            try
            {
                WebClient wc = new WebClient();
                wc.Encoding = Encoding.UTF8;
                return wc.DownloadString(URL);
            }
            catch
            {
                return "";
            }
        }
    }
}
