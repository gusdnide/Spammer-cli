using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Mail;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.IO;
namespace Spammer
{
    class Program
    {
        static cConfig Config;
        static string strCorpo;
        static string strAssunto;
        static List<Thread> ListaThreads = new List<Thread>();
        struct Conta
        {
            public string Senha { get; set; }
            public string Email { get; set; }
        }
        [STAThread]
        static void Main(string[] args)
        {
            Console.Title = "gusd SPAM";
            Aviso("Lendo configuraçoes");
            if (!File.Exists("config.json"))
            {
                cTools.Escrever("%4Digite seu %2email%4: ");
                string email = Console.ReadLine();
                cTools.Escrever("%4Digite sua %2senha%4: ");
                string senha = Console.ReadLine();
                cTools.Escrever("%4Digite o %2smtp%4: ");
                string smtp = Console.ReadLine();
                cTools.Escrever("%4Digite a %2porta%4: ");
                int saida = -1;
                if (!int.TryParse(Console.ReadLine(), out saida))
                {
                    Error("Voce digitou uma porta invalida!");
                    return;
                }

                Config = new cConfig(email, senha, saida, smtp);
                cConfig.Salvar(Config);
            }
            else
            {
                Config = cConfig.Carregar();
            }
            Aviso("Verificando arquivos");
            if (!File.Exists("body.html"))
            {
                Error("Arquivo body.html nao encontrado!");
                return;
            }
            if (!File.Exists("emails.txt"))
                File.Create("emails.txt");
            if (!File.Exists("contas.txt"))
                File.Create("contas.txt");
            strCorpo = File.ReadAllText("body.html");

            cTools.Escrever("%4Digite o %2assunto%4: ");
            strAssunto = Console.ReadLine();

            Aviso("%3Tudo Pronto, Iniciando... %4(Pressione alguma tecla para iniciar)");
            Console.ReadLine();
            new Thread(Iniciar).Start();
        }
        static void Aviso(string Texto)
        {
            cTools.EscreverLinha($"%2[Aviso] %2{Texto}");
        }
        static void Error(string Texto)
        {
            cTools.EscreverLinha($"%1[Error] %2{Texto}");
            Console.ReadLine();
        }
        static bool EnviarEmail(string Email)
        {
            try
            {
                EmailAtual = Email;

                SmtpClient SmtpServer = new SmtpClient(Config.Smtp);
                var mail = new MailMessage();
                mail.From = new MailAddress(Config.Email);
                mail.To.Add(Email);
                mail.Subject = strAssunto;
                mail.IsBodyHtml = true;
                mail.Body = strCorpo;
                SmtpServer.Port = Config.Porta;
                SmtpServer.UseDefaultCredentials = false;
                SmtpServer.Credentials = new System.Net.NetworkCredential(Config.Email, Config.Senha);
                SmtpServer.EnableSsl = true;
                SmtpServer.Send(mail);
                cTools.EscreverLinha($"%3Email enviado para %1 {Email}");
                return true;
            }
            catch
            {
                return false;
            }
        }
        static int Pagina = 0;
        static string EmailAtual = "";
        static int EmailsPegos = 0;
        static int ContasPegas = 0;
        static void Stats()
        {
            while (true)
            {
                Console.Title = $"[{DateTime.Now.ToString()}] gusdSpammer-CLI [{ListaThreads.Count} Threads] | [Email Atual: {EmailAtual}] | [Pagina do Google: {Pagina}] [{EmailsPegos} Emails Pegos] | [{ContasPegas} Contas Pegas! ] ";
                Thread.Sleep(500);
            }
        }
        static void Iniciar()
        {
            new Thread(Stats).Start();
            
            while (Pagina <= 30)
            {
                try
                {
                    Console.Clear();
                    List<string> Links = new List<string>();
                    Links.AddRange(PasteBin.PegarArchives(Config.DorkGoogle, Pagina));
                    string[] OutrosLink = new string[1];
                    if (File.Exists("links.txt"))
                    {
                        OutrosLink = File.ReadAllLines("links.txt");
                    }
                    cTools.EscreverLinha($"Foram encontrados { Links.Count} links");
                    for (int i = Links.Count - 1; i >= 0; i--)
                    {
                        try
                        {
                            string link = Links[i];
                            if (OutrosLink.Length > 0 && OutrosLink.Contains(link))
                            {
                                Links.RemoveAt(i);
                                continue;
                            }
                            File.WriteAllText("links.txt", cTools.RetArray(OutrosLink) + cTools.RetArray(Links.ToArray()));
                            Aviso("Buscando emails...");
                            string Response = cTools.getResponse(link);
                            string[] Emails = cTools.getRegex(PasteBin.sRegex.Email, Response);
                            string[] Contas = cTools.getRegex(PasteBin.sRegex.EmailSenha1, Response);
                            string[] Contas2 = cTools.getRegex(PasteBin.sRegex.EmailSenha2, Response);
                            if (Contas.Length > 0)
                            {
                                if (File.Exists("contas.txt"))
                                {
                                    File.WriteAllText("contas.txt", File.ReadAllText("contas.txt") + cTools.RetArray(Contas));
                                }
                                else
                                {
                                    File.WriteAllText("contas.txt", cTools.RetArray(Contas));
                                }
                            }
                            if (Contas2.Length > 0)
                            {
                                if (File.Exists("contas.txt"))
                                {
                                    File.WriteAllText("contas.txt", File.ReadAllText("contas.txt") + cTools.RetArray(Contas2));
                                }
                                else
                                {
                                    File.WriteAllText("contas.txt", cTools.RetArray(Contas2));
                                }
                            }
                            int ContasEncontradas = Contas.Length + Contas2.Length;
                            ContasPegas += ContasEncontradas;
                            cTools.EscreverLinha($"%2Foram encontrados {ContasEncontradas} emails com senha!");
                            if (Emails.Length <= 0)
                                continue;
                            cTools.EscreverLinha($"%3Foram encontrados %4{Emails.Length} emails.");
                            EmailsPegos += Emails.Length;
                            foreach (string Email in Emails)
                            {
                                try
                                {
                                    while (ListaThreads.Count >= Config.MaxThreads)
                                    {
                                        for (int tID = ListaThreads.Count - 1; tID >= 0; tID--)
                                        {
                                            Thread t = ListaThreads[tID];
                                            if (!t.IsAlive)
                                            {
                                                ListaThreads.RemoveAt(tID);
                                            }
                                        }
                                        Thread.Sleep(500);
                                    }
                                    if (!File.ReadAllText("emails.txt").Contains(Email))
                                    {
                                        Thread tEmail = new Thread(() => EnviarEmail(Email));
                                        tEmail.Start();
                                        ListaThreads.Add(tEmail);
                                    }
                                }
                                catch
                                {

                                }
                            }
                            if (File.Exists("emails.txt"))
                            {
                                File.WriteAllText("emails.txt", File.ReadAllText("emails.txt") + cTools.RetArray(Emails));
                            }
                            else
                            {
                                File.WriteAllText("emails.txt", cTools.RetArray(Emails));
                            }
                        }catch
                        {

                        }
                        Thread.Sleep(5000);
                    }
                }
                catch
                {

                }
                Thread.Sleep(15000);
                Pagina++;
            }
        }

    }

    class cConfig
    {
        public string Email { get; set; }
        public string Senha { get; set; }
        public int Porta { get; set; }
        public string Smtp { get; set; }
        public string DorkGoogle { get; set; }
        public int MaxThreads { get; set; }
        public cConfig()
        {

        }
        public static void Salvar(cConfig c)
        {
            File.WriteAllText("config.json", JsonConvert.SerializeObject(c));
        }
        public static cConfig Carregar()
        {
            return JsonConvert.DeserializeObject<cConfig>(File.ReadAllText("config.json"));
        }
        public cConfig(string e, string s, int p, string sm)
        {
            this.Email = e;
            this.Senha = s;
            this.Porta = p;
            this.Smtp = sm;
            this.DorkGoogle = "intext:\"@gmail.com\"+intext:\"@hotmail.com\"+\"intext:\"@yahoo.com.br\"";
            this.MaxThreads = 100;
        }
    }
    class PasteBin
    {
        public struct sRegex
        {

            public static string Email = @"[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])";
            public static string EmailSenha1 = @"[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])\:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*?";
            public static string EmailSenha2 = @"[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])\|[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*?";
        }
        public static bool IsValidURI(string uri)
        {
            if (!Uri.IsWellFormedUriString(uri, UriKind.Absolute))
                return false;
            Uri tmp;
            if (!Uri.TryCreate(uri, UriKind.Absolute, out tmp))
                return false;
            return tmp.Scheme == Uri.UriSchemeHttp || tmp.Scheme == Uri.UriSchemeHttps;
        }
        public static string[] PegarArchives(string Dork, int Pagina)
        {
            List<string> Retorno = new List<string>();
            try
            {
                string Resp = cTools.getResponse($"https://www.google.com.br/search?q={Dork}&start={Pagina}0");
                string m = @"(ftp:\/\/|www\.|https?:\/\/){1}[a-zA-Z0-9u00a1-\uffff0-]{2,}\.[a-zA-Z0-9u00a1-\uffff0-]{2,}(\S*)";
                foreach (string s in cTools.getRegex(m, Resp))
                {
                    if (!s.Contains("https://www.google.com.br"))
                    {
                        string link = s;
                        if (link.IndexOf('<') > -1)
                            link = link.Substring(0, link.IndexOf('<'));
                        if (link.IndexOf('&') > -1)
                            link = link.Substring(0, link.IndexOf('&'));
                        if (!link.Contains("https://") || !link.Contains("http://"))
                            link = "http://" + link;
                        if (IsValidURI(link))
                            Retorno.Add(link);
                    }
                }
            }
            catch
            {
                return Retorno.ToArray();
            }
            return Retorno.ToArray();
        }
        public static string PegarConteudo(string url)
        {
            try
            {
                string Resp = cTools.getResponse(url);
                return Resp;
            }
            catch
            {
                return "";
            }
        }

    }
}
