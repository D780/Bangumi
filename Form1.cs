using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace Bangumi
{
    public partial class Form1 : Form
    {
        WebClient client = new WebClient();
        string pageHtmlNewest;
        MatchCollection matchsNewest;
        string pageHtmlHistory;
        MatchCollection matchsHistory;
        string CurrentSite;
        Regex regex = new Regex(@"<td( title=""(?<fulltime>.*)"")?>(?<time>.*)</td>\s*(<td><a href="".*"">(?<sort>.*)</a></td>\s*)?<td class=""ltext ttitle""><a href=""(?<down>/down/\w*/\w*\.torrent)"" class=""quick-down cmbg""></a><a href="".*"" target=""_blank"">(?<name>.*)</a>(<span>.*</span>)?</td>\s*<td>(?<size>\w*.\w*)</td>\s*<td class=""bts-\d"">(?<bts>\d*)</td>\s*<td class=""btl-\d"">(?<btl>\d*)</td>(\s*<td class=""btc-\d"">(?<btc>\d*)</td>)?", RegexOptions.Multiline);
        Regex regexPage = new Regex(@"<a href="".*"" class=""pager-last active"" target=""_self"">(?<MaxPage>\d*)</a>", RegexOptions.Multiline);
        public Form1()
        {
            InitializeComponent();
            client.Credentials = CredentialCache.DefaultCredentials;
            tSCBNewestSort.Items.AddRange(new Link[] { new Link( "全部动画", "http://bt.ktxp.com/sort-1-1.html"), 
                                                                                 new Link( "新番连载","http://bt.ktxp.com/sort-12-1.html"),
                                                                                 new Link( "完整动画","http://bt.ktxp.com/sort-28-1.html"),
                                                                                 new Link( "剧场版","http://bt.ktxp.com/sort-39-1.html"),
                                                                                 new Link( "DVDRIP","http://bt.ktxp.com/sort-14-1.html"),
                                                                                 new Link( "BDRIP","http://bt.ktxp.com/sort-50-1.html"),
                                                                 });
        }

        class Link
        {
            private string key;
            public string Key
            {
                get { return key; }
            }
            private string value;
            public string Value
            {
                get { return value; }
            }

            public Link(string key, string value)
            {
                this.key = key;
                this.value = value;
            }
            public Link(int key, string value)
            {
                this.key = key.ToString(); ;
                this.value = value;
            }
            public override string ToString()
            {
                return key;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            tSCBNewestSort.SelectedIndex = 0;
        }

        private void dGVNewest_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex==0)
            {
                byte[] torrent=client.DownloadData("http://bt.ktxp.com/" + matchsNewest[e.RowIndex].Groups["down"].ToString());
               
                FileStream fs=null;
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter ="Torrent种子(*.torrent)|*.torrent";
                sfd.FileName = matchsNewest[e.RowIndex].Groups["name"].ToString();
                sfd.DefaultExt = "torrent"; 
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        fs = new FileStream(sfd.FileName, FileMode.OpenOrCreate);
                        fs.Write(torrent, 0, torrent.Length);

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }
                    finally
                    {
                        fs.Close();
                    }
                }
            }
        }

        private void tSCBNewestSort_SelectedIndexChanged(object sender, EventArgs e)
        {            
            tSCBNewestPage.Items.Clear();
            Link theLink = (Link)tSCBNewestSort.SelectedItem;
            CurrentSite = theLink.Value;
            Byte[] pageData = client.DownloadData(CurrentSite);
            pageHtmlNewest = Encoding.UTF8.GetString(pageData);
            
            Match matchPage = regexPage.Match(pageHtmlNewest);
            for (int i = 1; i <= Convert.ToInt32(matchPage.Groups["MaxPage"].ToString()); i++)
            {
                tSCBNewestPage.Items.Add(new Link(i, theLink.Value.Substring(0,theLink.Value.LastIndexOf('-')+1)+i+".html"));
            }
            tSCBNewestPage.SelectedIndex = 0;
        }

        private void tSCBNewestPage_SelectedIndexChanged(object sender, EventArgs e)
        {
            dGVNewest.Rows.Clear();
            Link theLink = (Link)tSCBNewestPage.SelectedItem;
            CurrentSite = theLink.Value;
            Byte[] pageData = client.DownloadData(CurrentSite);
            pageHtmlNewest = Encoding.UTF8.GetString(pageData);
            matchsNewest = regex.Matches(pageHtmlNewest);
            foreach (Match match in matchsNewest)
            {
                GroupCollection groups = match.Groups;
                object[] str = {new Link("↓",groups["down"].ToString()), groups["time"].ToString(), 
                                   groups["sort"].ToString() .Length==0?tSCBNewestSort.SelectedItem:groups["sort"].ToString() , 
                                   groups["name"].ToString(), groups["size"].ToString(), groups["btl"].ToString(), 
                                   groups["bts"].ToString(), };
                dGVNewest.Rows.Add(str);
            }
        }

    }
}
