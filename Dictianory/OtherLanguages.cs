using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Dictianory
{
    public partial class OtherLanguages : Form
    {
        private List<string> AvailLangs;
        public OtherLanguages()
        {
            InitializeComponent();
        }

        private void btnDetectSrcLang_Click(object sender, EventArgs e)
        {
            var response = RequestService(string.Format(AppCache.UrlDetectSrcLanguage,AppCache.API, txtSrc.Text));
            var dict = JsonConvert.DeserializeObject<IDictionary>(response.Content);

            var statusCode = dict["code"].ToString();

            if (statusCode.Equals("200"))
            {
                lblSrcLang.Text = dict["lang"].ToString();
            }
        }

        private void btnAC_Click(object sender, EventArgs e)
        {
            var response = RequestService(string.Format(AppCache.UrlGetAvailableLanguages, AppCache.API, lblSrcLang.Text));
            var dict = JsonConvert.DeserializeObject<IDictionary>(response.Content);
            foreach(DictionaryEntry entry in dict)
            {
                if (entry.Key.Equals("langs"))
                {
                    var availableConverts = (JObject)entry.Value;
                    AvailLangs = new List<string>();

                    comboAvailableLangs.Items.Clear();
                    foreach(var Lang in availableConverts)
                    {
                        if (!Lang.Equals(lblSrcLang.Text))
                        {
                            comboAvailableLangs.Items.Add(Lang.Value);
                            AvailLangs.Add(Lang.Key);
                        }
                    }
                }
            }
        }


        private void btnTranslate_Click(object sender, EventArgs e)
        {
            var response = RequestService(string.Format(AppCache.UrlTranslateLanguage, AppCache.API, txtSrc.Text, AvailLangs[comboAvailableLangs.SelectedIndex]));
            var dict = JsonConvert.DeserializeObject<IDictionary>(response.Content);
            var statusCode = dict["code"].ToString();
            if (statusCode.Equals("200"))
            {
                txtDestLang.Text = string.Join(",", dict["text"]);
            }
        }

        private IRestResponse RequestService(string strUrl)
        {
            var client = new RestClient()
            {
                BaseUrl = new Uri(strUrl)
            };
            var request = new RestRequest()
            {
                Method = Method.GET
            };
             return client.Execute(request);


        }


    }
}
