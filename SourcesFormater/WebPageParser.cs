using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace SourcesFormater
{
    public class WebPageParser
    {
        private string _URL;
        public string Title
        { 
            get 
            { 
                return GetPageTitle(_URL); 
            } 
        }
        public string URL 
        { 
            get 
            { 
                return _URL;
            } 
            set 
            { 
                _URL = value; 
            } 
        }
        public string GOST 
        { 
            get
            {
                string currentDate = DateTime.Now.ToString("dd.MM.yyyy");
                return (this.Title + ". [Электронный ресурс]. URL: " + _URL + " Дата обращения: " + currentDate); 
            }
        }
        public WebPageParser(string url)
        {
            _URL = url;
        }
        private string GetPageTitle(string url)
        {
            HtmlWeb web = new HtmlWeb();
            HtmlDocument document = web.Load(url);
            try
            {
                HtmlNode titleNode = document.DocumentNode.SelectSingleNode("//title");

                if (titleNode != null)
                {
                    return titleNode.InnerText;
                }

                return "Невозможно получить заголовок";
            }
            catch(Exception ex)
            { 
                return ("Исключение: "+ex.ToString()+"\n");
            }
        }
    }
}
