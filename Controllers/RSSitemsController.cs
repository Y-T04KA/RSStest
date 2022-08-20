using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RSStest.Data;
using RSStest.Models;

namespace RSStest.Controllers
{
    public class RSSitemsController : Controller
    {
        private readonly RSStestContext _context;
        private readonly IConfiguration Configuration;
        

        public RSSitemsController(RSStestContext context, IConfiguration configuration)
        {
            _context = context;
            
            Configuration = configuration;
        }

        private System.Timers.Timer timer;

    // GET: RSSitems
    public async Task<IActionResult> Index()
        {
            var data = await _context.RSSitem.ToListAsync();
            data.Sort((x, y) => (DateTime.Compare(x.pubDate, y.pubDate)*-1));//-1 чтобы перевернуть сортировку
            
              return _context.RSSitem != null ? 
                          View(data) :
                          Problem("Entity set 'RSStestContext.RSSitem'  is null.");
        }  
              
        //GET: RSSitems/Update - получить новые сообщения и редирект на список
        public async Task<RedirectToActionResult> Update()
        {
            string feed = Configuration["feeds:feed"];//работает только пока ссылка одна, способа читать произвольное число ссылок я не нашёл
            string useproxy = Configuration["useproxy"];
            var proxyadress = Configuration["proxyadress"];
            var proxylogin = Configuration["proxylogin"];
            var proxypass = Configuration["proxypass"];
            timer = new System.Timers.Timer();
            int gap = Int32.Parse(Configuration["updaterate"]);
            timer.Interval = gap;
            timer.Elapsed += SilentUpdate;
            timer.Start();
            GC.KeepAlive(timer);
            var client = new HttpClient();
            if (useproxy == "1")
            {
                var proxy = new WebProxy
                {
                    Address = new Uri(proxyadress),
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(proxylogin, proxypass)
                };
                var ClientHandler = new HttpClientHandler
                {
                    Proxy = proxy,
                };
                client = new HttpClient(ClientHandler);
            }
            HttpResponseMessage response = await client.GetAsync(feed);
            response.EnsureSuccessStatusCode();
            string rssData = await response.Content.ReadAsStringAsync();
            await TempRSS(rssData);
            await RSSparse();
            System.IO.File.Delete("temp.xml");
            return RedirectToAction(nameof(Index));
        }

       /* public async Task<RedirectToActionResult> TimerUpdate()
        {
            timer = new System.Timers.Timer();
            int gap = Int32.Parse(Configuration["updaterate"]);
           
                timer.Interval = gap;
                timer.Elapsed += SilentUpdate;
                timer.Start();
            
            GC.KeepAlive(timer);
            return RedirectToAction(nameof(Index));
        }*/

        private async void SilentUpdate(object? sender, System.Timers.ElapsedEventArgs e)
        {
            Console.WriteLine("Timer Works!");
            RedirectToAction(nameof(Update));
        }

        private async Task TempRSS(string rssdata)
        {
          using (StreamWriter writer = new StreamWriter("temp.xml"))
            {
                await writer.WriteAsync(rssdata);
            }
        }

        private async Task RSSparse()
        {
            XmlDocument xml = new XmlDocument();
            xml.Load("temp.xml");
            XmlElement? xRoot = xml.DocumentElement;
            XmlNode? channelName = xRoot.SelectSingleNode("//title");//идея в том что он подберёт первый встреченный тег title, что нам и надо
            string source = channelName.InnerText;
            XmlNodeList items = xRoot.SelectNodes("//item");
            if (items != null)
            {
                foreach (XmlNode item in items)
                {
                    //тут начинаем парсить айтем
                    XmlNode Titl = item.SelectSingleNode("title");
                    string Title = Titl.InnerText;
                    Titl = item.SelectSingleNode("link");
                    string Link = Titl.InnerText;
                    Titl = item.SelectSingleNode("description");
                    string Description = Titl.InnerText;
                    Titl = item.SelectSingleNode("pubDate");
                    string date = Titl.InnerText;
                    DateTime pubDate = DateTime.Parse(date);
                    await SendRSSitem(source, Title, Link, Description, pubDate);
                }
            }
        }

        private async Task SendRSSitem(string source, string title, string link, string desc, DateTime pubDate)
        {
            //тут наверное надо проверить, добавлять айтем, или обновить, или ничего
            RSSitem item = new RSSitem();
            item.Source = source;
            item.Title = title;
            item.Link = link;
            item.Description = desc;
            item.pubDate = pubDate;
            item.Source = source;
            item.Creator = "unimplemented";//надо настраивать парсинг неймспейсов, но раз вывода автора нет в задании, то подождёт
            item.isUnread = true;
            int id = FindDuplicates(item);
            if (id==-1)
            {
                _context.Add(item);
                await _context.SaveChangesAsync();
            } else if (id>0)
            {
                var del = await _context.RSSitem.FindAsync(id);//зачем это вообще? на случай если статья была обновлена после выпуска в фид
                _context.RSSitem.Remove(del);
                _context.Add(item);
                await _context.SaveChangesAsync();
            }
        }

        private int FindDuplicates(RSSitem item)
        {
            var data = _context.RSSitem.ToList();//наверное, это не очень эффективно и по памяти, и по скорости, но как quick&dirty сойдёт
            foreach (RSSitem piece in data)
            {
                if (piece.Source == item.Source && piece.Title == item.Title && piece.Description != item.Description) return piece.Id;
                if (piece.Source == item.Source && piece.Title == item.Title && piece.Description == item.Description) return 0;
            }
            return -1;
        }
        
    }
}

