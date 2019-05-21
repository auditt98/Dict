using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Speech.Synthesis;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

//using System.Net.NetworkInformation;

namespace Dictianory
{
    public partial class Main_Form : Form
    {

        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(String sClassName, String sAppName);

        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private IntPtr thisWindow;

        //for offline search
        private DataTable dataTable;

        private Dictionary<string, int> words = new Dictionary<string, int>(StringComparer.CurrentCultureIgnoreCase);
        private BKTree tree = new BKTree();

        //for word of the day
        private string wordOfTheDay;

        //for expanding side menu
        private bool expand = false;

        //for keeping track of search source
        private enum SearchSources
        { Database, Google, Dict }

        private int activeSearchSource;

        //for keeping track of current panel
        private enum Panels
        { Search, Correction, Flashcard, Image, Paragraph, Learn };

        private int activePanel;
        private bool correctionFinished = false;

        //for correction panel
        private ToolStripDropDown popup = new ToolStripDropDown();

        private Tuple<int, int> selectedWordIndexLength;

        // for MyLearningWords 
        private DataTable datatable1;
        List<string> myWordsList;



        public Main_Form()
        {
            InitializeComponent();
        }

        public enum WindowKeys
        {
            Alt = 0x0001,
            Control = 0x0002,
            Shift = 0x0004, // Changes!
            Window = 0x0008,
            NoRepeat = 0x4000
        }

        private void Main_Form_Load(object sender, EventArgs e)
        {
            try
            {
                thisWindow = FindWindow(null, "Dictianory");
                RegisterHotKey(thisWindow, 0, (uint)WindowKeys.Control | (uint)WindowKeys.NoRepeat , (uint)Keys.Q);
                RegisterHotKey(thisWindow, 1, (uint)WindowKeys.Control | (uint)WindowKeys.NoRepeat , (uint)Keys.Y);
                init();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                throw;
            }
        }

        private void Main_Form_FormClosed(object sender, FormClosedEventArgs e)
        {
            UnregisterHotKey(thisWindow, 0);
            UnregisterHotKey(thisWindow, 1);
            dataTable.Dispose();
            popup.Dispose();
        }

        protected override void WndProc(ref Message keyPressed)
        {
            if (keyPressed.Msg == 0x0312)
            {
                if(keyPressed.WParam.ToInt32() == 0)
                {
                    SendKeys.SendWait("^{c}");
                    this.WindowState = FormWindowState.Maximized;
                    Search_Side_Button.PerformClick();
                    Search_TextBox.Text = "";
                    Search_TextBox.Focus();
                    SendKeys.SendWait("^{v}");
                }
                if(keyPressed.WParam.ToInt32() == 1)
                {
                    SendKeys.SendWait("^{c}");
                    this.WindowState = FormWindowState.Maximized;
                    Correction_Side_Button.PerformClick();
                    Correction_RichTextBox.Text = "";
                    Correction_RichTextBox.Focus();

                    SendKeys.SendWait("^{v}");
                }
                
            }
            base.WndProc(ref keyPressed);
        }
        ///

        private void init()
        {
            /*--------init---------- */
            BKTree_BW.RunWorkerAsync();
            StringHelper stringHelper = new StringHelper();
            //set active panel
            activePanel = (int)Panels.Search;
            colorActivePanel();
            Learn_Main_Panel.Hide();
            Paragraph_Main_Panel.Hide();
            Correction_Main_Panel.Hide();
            Image_Main_Panel.Hide();
            Flashcard_Main_Panel.Hide();
            Search_Main_Panel.Dock = DockStyle.Fill;

            //words count for database
            int wordsCount = 1;
            //set search source
            activeSearchSource = (int)SearchSources.Database;

            //hide sidebar

            #region Region SidebarHidden

            Main_Panel.ColumnStyles[1].Width = 0;
            Main_Panel.ColumnStyles[2].Width = 80;

            #endregion Region SidebarHidden

            //set active search source to database
            Source_Database_Button.PerformClick();

            //Add default text to big text box
            Result_RichTextBox.AppendText("Hãy tìm kiếm một từ.");
            //Fill autocomplete source for search bar
            FillAutoComplete(ref wordsCount);

            //Hide the speaker button
            Pronounce_Button.Hide();
            FillWOTD(ref wordsCount);
            setToolTip();

            //for correction panel

            Suggestions_ListBox.Hide();
            Correction_Learn_Popup_Button.Hide();
            Correction_Search_Popup_Button.Hide();
            popup.Margin = Padding.Empty;
            popup.Padding = Padding.Empty;
            Correction_RichTextBox.Text = stringHelper.getCorrectionPanelInitString();
            richTextBox1.Hide();

            // for My Learning Words List
            MyWords_Learning();
            DELETE.Hide();
            //timer1.Interval = 10000;
            //timer1.Start();
        }

        private void setToolTip()
        {
            Source_Database_Tooltip.SetToolTip(Source_Database_Button, "Search using offline database");
            Source_Dictionary_Tooltip.SetToolTip(Source_Dictionary_Button, "Search using Dictionary.com");
            Source_Google_Tooltip.SetToolTip(Source_Google_Button, "Search using Google Translate");
            Pronounce_Button_Tooltip.SetToolTip(Pronounce_Button, "Listen");
            WOTD_Tooltip.SetToolTip(WOTD_Button, "Click to get the meaning of the word");
        }

        private void colorActivePanel()
        {
            switch (activePanel)
            {
                case (int)Panels.Search:
                    Search_Side_Button.BackColor = Color.SteelBlue;
                    Correction_Side_Button.BackColor = Hamburger_Button.BackColor;
                    Paragraph_Side_Button.BackColor = Hamburger_Button.BackColor;
                    Flashcard_Side_Button.BackColor = Hamburger_Button.BackColor;
                    Image_Side_Button.BackColor = Hamburger_Button.BackColor;
                    Learn_Side_Button.BackColor = Hamburger_Button.BackColor;
                    break;

                case (int)Panels.Correction:
                    Correction_Side_Button.BackColor = Color.SteelBlue;
                    Search_Side_Button.BackColor = Hamburger_Button.BackColor;
                    Paragraph_Side_Button.BackColor = Hamburger_Button.BackColor;
                    Flashcard_Side_Button.BackColor = Hamburger_Button.BackColor;
                    Image_Side_Button.BackColor = Hamburger_Button.BackColor;
                    Learn_Side_Button.BackColor = Hamburger_Button.BackColor;
                    break;

                case (int)Panels.Flashcard:
                    Flashcard_Side_Button.BackColor = Color.SteelBlue;
                    Search_Side_Button.BackColor = Hamburger_Button.BackColor;
                    Paragraph_Side_Button.BackColor = Hamburger_Button.BackColor;
                    Correction_Side_Button.BackColor = Hamburger_Button.BackColor;
                    Image_Side_Button.BackColor = Hamburger_Button.BackColor;
                    Learn_Side_Button.BackColor = Hamburger_Button.BackColor;
                    break;

                case (int)Panels.Paragraph:
                    Paragraph_Side_Button.BackColor = Color.SteelBlue;
                    Search_Side_Button.BackColor = Hamburger_Button.BackColor;
                    Flashcard_Side_Button.BackColor = Hamburger_Button.BackColor;
                    Correction_Side_Button.BackColor = Hamburger_Button.BackColor;
                    Image_Side_Button.BackColor = Hamburger_Button.BackColor;
                    Learn_Side_Button.BackColor = Hamburger_Button.BackColor;
                    break;

                case (int)Panels.Image:
                    Image_Side_Button.BackColor = Color.SteelBlue;
                    Search_Side_Button.BackColor = Hamburger_Button.BackColor;
                    Flashcard_Side_Button.BackColor = Hamburger_Button.BackColor;
                    Correction_Side_Button.BackColor = Hamburger_Button.BackColor;
                    Paragraph_Side_Button.BackColor = Hamburger_Button.BackColor;
                    Learn_Side_Button.BackColor = Hamburger_Button.BackColor;
                    break;

                case (int)Panels.Learn:
                    Learn_Side_Button.BackColor = Color.SteelBlue;
                    Search_Side_Button.BackColor = Hamburger_Button.BackColor;
                    Flashcard_Side_Button.BackColor = Hamburger_Button.BackColor;
                    Correction_Side_Button.BackColor = Hamburger_Button.BackColor;
                    Paragraph_Side_Button.BackColor = Hamburger_Button.BackColor;
                    Image_Side_Button.BackColor = Hamburger_Button.BackColor;
                    break;

                default:
                    break;
            }
            this.Refresh();
        }

        #region Region Search

        private void FillAutoComplete(ref int wordsCount)
        {
            Search_TextBox.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            string query;
            SQLiteConnection sqliteConnection = new SQLiteConnection(ConnectionHelper.GetConnectionString());
            sqliteConnection.Open();

            dataTable = new DataTable();
            query = "Select Word,Ipa,Def from Words";

            SQLiteCommand sqliteCommand = new SQLiteCommand(query, sqliteConnection);
            SQLiteDataAdapter dataAdapter = new SQLiteDataAdapter(sqliteCommand);
            dataAdapter.Fill(dataTable);
            foreach (DataRow row in dataTable.Rows)
            {
                Search_TextBox.AutoCompleteCustomSource.Add(row["Word"].ToString());
                wordsCount++;
            }
            Search_TextBox.AutoCompleteSource = AutoCompleteSource.CustomSource;

            dataAdapter.Dispose();
            sqliteConnection.Close();
            sqliteCommand.Dispose();
        }

        private void Search_TextBox_TextChanged(object sender, EventArgs e)
        {
            StringHelper stringHelper = new StringHelper();
            if (activeSearchSource == (int)SearchSources.Database)
            {
                DataRow[] found;
                string filter = "Word = '" + stringHelper.SQLChecker(Search_TextBox.Text) + "'";
                found = dataTable.Select(filter);
                if (found.Length > 0 && Search_TextBox.Text.Length > 0)
                {
                    Result_Word_Label.Text = found[0][0].ToString();
                    Result_RichTextBox.ResetText();
                    Result_RichTextBox.AppendText(found[0][1].ToString());
                    if (found[0][1].ToString().TrimEnd() != "" || found[0][1].ToString().TrimEnd() != "/")
                    {
                        Result_RichTextBox.AppendText(Environment.NewLine);
                    }
                    string def = found[0][2].ToString().Replace('=', '\t');
                    def = def.Replace('!', '\t');
                    def = def.Replace("+", ":");
                    Result_RichTextBox.AppendText(def);
                    colorResultRtb();

                    Pronounce_Button.Show();
                }
                else
                {
                    Result_RichTextBox.Text = stringHelper.errorWordNotFound();
                    Result_Word_Label.Text = "";
                    Pronounce_Button.Hide();
                }
            }
            else
            {

            }
        }

        private void colorResultRtb()
        {
            StringHelper stringHelper = new StringHelper();
            Regex r = new Regex(stringHelper.getRegexSearchResultWordType());
            foreach (Match match in r.Matches(Result_RichTextBox.Text))
            {
                Result_RichTextBox.Select(match.Index, match.Length);
                Result_RichTextBox.SelectionColor = Color.FromArgb(21, 195, 154);
                Result_RichTextBox.SelectionFont = new Font(Result_RichTextBox.SelectionFont, FontStyle.Bold);
            }
            r = new Regex(stringHelper.getRegexSearchResultWordDef());
            foreach (Match match in r.Matches(Result_RichTextBox.Text))
            {
                Result_RichTextBox.Select(match.Index, match.Length);
                Result_RichTextBox.SelectionColor = Color.LightGoldenrodYellow;
                Result_RichTextBox.SelectionFont = new Font(Result_RichTextBox.SelectionFont, FontStyle.Bold);

            }
            r = new Regex(stringHelper.getRegexSearchResultWordExample());
            foreach (Match match in r.Matches(Result_RichTextBox.Text))
            {
                Result_RichTextBox.Select(match.Index, match.Length);
                Result_RichTextBox.SelectionColor = Color.WhiteSmoke;
                Result_RichTextBox.SelectionFont = new Font(Result_RichTextBox.SelectionFont, FontStyle.Italic);
            }
        }

        private void Pronounce_Button_Click(object sender, EventArgs e)
        {
            if (expand == true)
            {
                ToggleSlidingMenu();
            }
            SpeechSynthesizer speech = new SpeechSynthesizer();
            speech.SpeakAsync(Result_Word_Label.Text);
        }

        #endregion Region Search

        #region Region WordOfTheDay

        private int getRandomWordID(ref int wordsCount)
        {
            Random random = new Random();
            int rNumber = random.Next(1, wordsCount);
            return rNumber;
        }

        private void setWOTD(string wotd)
        {
            WOTD_Button.Text = wotd.ToUpper();
        }

        private void FillWOTD(ref int wordsCount)
        {
            try
            {
                StringHelper stringHelper = new StringHelper();
                DateTime today = DateTime.Today;
                string format = stringHelper.getSqlTimeFormat(); //format of sql datetime datatype
                SQLiteConnection cn = new SQLiteConnection(ConnectionHelper.GetConnectionString());
                cn.Open();
                DataTable dateTable = new DataTable();
                string q = "Select * from LastDate";
                SQLiteCommand cmd = new SQLiteCommand(q, cn);
                SQLiteDataAdapter da = new SQLiteDataAdapter(cmd);
                da.Fill(dateTable);

                // if there's no record then create a new record for today

                if (dateTable.Rows.Count == 0)
                {
                    int randomWordID = getRandomWordID(ref wordsCount);
                    string insert = @" insert into LastDate(lDate, wordId) values ('" + today.ToString(format) + "', " + randomWordID.ToString() + ")";
                    cmd = new SQLiteCommand(insert, cn);
                    cmd.ExecuteNonQuery();
                    wordOfTheDay = dataTable.Rows[randomWordID - 1]["Word"].ToString();
                    setWOTD(wordOfTheDay);
                }
                else // check if today is the same as the day stored in database
                {
                    DateTime sqlTime = DateTime.Parse(dateTable.Rows[0]["lDate"].ToString());
                    if (sqlTime.CompareTo(today) == 0) // if it is then just display the word stored
                    {
                        int wordId = int.Parse(dateTable.Rows[0]["wordId"].ToString());
                        wordOfTheDay = dataTable.Rows[wordId - 1]["Word"].ToString();
                        setWOTD(wordOfTheDay);
                    }
                    else // create a random word and update the database for today
                    {
                        int randomWordID = getRandomWordID(ref wordsCount);
                        string update = @"update LastDate set lDate = '" + today.ToString(format) + "', wordId = " + randomWordID.ToString() + " where id = 1;";
                        cmd = new SQLiteCommand(update, cn);
                        cmd.ExecuteNonQuery();
                        wordOfTheDay = dataTable.Rows[randomWordID - 1]["Word"].ToString();
                        setWOTD(wordOfTheDay);
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
                throw;
            }
        }

        #endregion Region WordOfTheDay

        #region Region SideButtonsClick Events

        private void Hamburger_Button_Click(object sender, EventArgs e)
        {
            ToggleSlidingMenu();
        }

        private void Search_Side_Button_Click(object sender, EventArgs e)
        {
            //Hide all other panels
            Learn_Main_Panel.Hide();
            Paragraph_Main_Panel.Hide();
            Correction_Main_Panel.Hide();
            Image_Main_Panel.Hide();
            Flashcard_Main_Panel.Hide();

            //Toggle menu if expanded
            if (expand == true)
            {
                ToggleSlidingMenu();
            }
            //set this as the active panel
            activePanel = (int)Panels.Search;
            //Show needed panel
            Search_Main_Panel.Show();
            //Set dock to fill the screen
            Search_Main_Panel.Dock = DockStyle.Fill;

            colorActivePanel();
        }

        private void Correction_Side_Button_Click(object sender, EventArgs e)
        {
            //Hide all other panels
            Learn_Main_Panel.Hide();
            Paragraph_Main_Panel.Hide();
            Image_Main_Panel.Hide();
            Flashcard_Main_Panel.Hide();
            Search_Main_Panel.Hide();

            //Toggle menu if expanded
            if (expand == true)
            {
                ToggleSlidingMenu();
            }
            //set this as the active panel
            activePanel = (int)Panels.Correction;

            //Show needed panel
            Correction_Main_Panel.Show();
            //Set dock to fill the screen
            Correction_Main_Panel.Dock = DockStyle.Fill;
            colorActivePanel();
        }

        private void Flashcard_Side_Button_Click(object sender, EventArgs e)
        {
            //Hide all other panels
            Learn_Main_Panel.Hide();
            Paragraph_Main_Panel.Hide();
            Correction_Main_Panel.Hide();
            Image_Main_Panel.Hide();
            Search_Main_Panel.Hide();

            //Toggle menu if expanded
            if (expand == true)
            {
                ToggleSlidingMenu();
            }
            //set this as the active panel
            activePanel = (int)Panels.Flashcard;

            //Show needed panel
            Flashcard_Main_Panel.Show();

            //Set dock to fill the screen
            Flashcard_Main_Panel.Dock = DockStyle.Fill;
            colorActivePanel();
        }

        private void Image_Side_Button_Click(object sender, EventArgs e)
        {
            //Hide all other panels
            Learn_Main_Panel.Hide();
            Paragraph_Main_Panel.Hide();
            Flashcard_Main_Panel.Hide();
            Search_Main_Panel.Hide();
            Correction_Main_Panel.Hide();

            //Toggle menu if expanded
            if (expand == true)
            {
                ToggleSlidingMenu();
            }
            //set this as the active panel
            activePanel = (int)Panels.Image;

            //Show needed panel
            Image_Main_Panel.Show();

            //Set dock to fill the screen
            Image_Main_Panel.Dock = DockStyle.Fill;
            colorActivePanel();
        }

        private void Paragraph_Side_Button_Click(object sender, EventArgs e)
        {
            //Hide all other panels
            Learn_Main_Panel.Hide();
            Flashcard_Main_Panel.Hide();
            Search_Main_Panel.Hide();
            Image_Main_Panel.Hide();
            Correction_Main_Panel.Hide();

            //Toggle menu if expanded
            if (expand == true)
            {
                ToggleSlidingMenu();
            }
            //set this as the active panel
            activePanel = (int)Panels.Paragraph;

            //Show needed panel
            Paragraph_Main_Panel.Show();

            //Set dock to fill the screen
            Paragraph_Main_Panel.Dock = DockStyle.Fill;
            colorActivePanel();
        }

        private void Learn_Side_Button_Click(object sender, EventArgs e)
        {
            //Hide all other panels
            Flashcard_Main_Panel.Hide();
            Search_Main_Panel.Hide();
            Image_Main_Panel.Hide();
            Correction_Main_Panel.Hide();
            Paragraph_Main_Panel.Hide();

            //Toggle menu if expanded
            if (expand == true)
            {
                ToggleSlidingMenu();
            }
            //set this as the active panel
            activePanel = (int)Panels.Learn;

            //Show needed panel
            Learn_Main_Panel.Show();

            //Set dock to fill the screen
            Learn_Main_Panel.Dock = DockStyle.Fill;
            colorActivePanel();
        }

        #endregion Region SideButtonsClick Events

        #region Region SideLabelClick Events

        private void Menu_Side_Label_Click(object sender, EventArgs e)
        {
            Hamburger_Button.PerformClick();
        }

        private void Search_Side_Label_Click(object sender, EventArgs e)
        {
            Search_Side_Button.PerformClick();
        }

        private void Correction_Side_Label_Click(object sender, EventArgs e)
        {
            Correction_Side_Button.PerformClick();
        }

        private void Flashcard_Side_Label_Click(object sender, EventArgs e)
        {
            Flashcard_Side_Button.PerformClick();
        }

        private void Image_Side_Label_Click(object sender, EventArgs e)
        {
            Image_Side_Button.PerformClick();
        }

        private void Paragraph_Side_Label_Click(object sender, EventArgs e)
        {
            Paragraph_Side_Button.PerformClick();
        }

        private void Learn_Label_Click(object sender, EventArgs e)
        {
            Learn_Side_Button.PerformClick();
        }

        #endregion Region SideLabelClick Events

        #region Region MainFormToggleSidebarPanel

        private void ToggleSlidingMenu()
        {
            if (expand == true)
            {
                Main_Panel.ColumnStyles[1].Width = 0;
                Main_Panel.ColumnStyles[2].Width = 95;
                expand = false;
            }
            else
            {
                Main_Panel.ColumnStyles[1].Width = 15;
                Main_Panel.ColumnStyles[2].Width = 80;
                expand = true;
            }
        }

        #endregion Region MainFormToggleSidebarPanel

        #region Region SearchSourceButton

        private void Source_Dictionary_Button_Click(object sender, EventArgs e)
        {
            activeSearchSource = (int)SearchSources.Dict;
            Source_Database_Button.BackColor = Color.White;
            Source_Google_Button.BackColor = Color.White;
            Source_Dictionary_Button.BackColor = Color.LightBlue;

            Result_RichTextBox.Text = "Hãy tìm kiếm một từ.";
            Result_Word_Label.Text = "";
            Pronounce_Button.Hide();
            //CALL YOUR FUNCTIONS AND STUFF BELOW
        }

        private void Source_Google_Button_Click(object sender, EventArgs e)
        {
            activeSearchSource = (int)SearchSources.Google;
            Source_Dictionary_Button.BackColor = Color.White;
            Source_Database_Button.BackColor = Color.White;
            Source_Google_Button.BackColor = Color.LightBlue;

            Result_RichTextBox.Text = "Hãy tìm kiếm một từ.";
            Result_Word_Label.Text = "";
            Pronounce_Button.Hide();
            //CALL YOUR FUNCTIONS AND STUFF BELOW
        }

        private void Source_Database_Button_Click(object sender, EventArgs e)
        {
            activeSearchSource = (int)SearchSources.Database;
            Source_Database_Button.BackColor = Color.LightBlue;
            Source_Dictionary_Button.BackColor = Color.White;
            Source_Google_Button.BackColor = Color.White;
        }

        private void WOTD_Label_MouseClick(object sender, MouseEventArgs e)
        {
            Search_TextBox.Text = wordOfTheDay;
        }

        #endregion Region SearchSourceButton

        #region Region Correction Panel

        private void countWordsInFile(string file)
        {
            var content = File.ReadAllText(file, Encoding.UTF8);
            var wordPattern = new Regex(@"\b(\w*[a-zA-Z'-]\w*)\b");
            foreach (Match match in wordPattern.Matches(content))
            {
                if (!words.ContainsKey(match.Value))
                {
                    words.Add(match.Value, 1);
                    tree.Add(match.Value);
                }
                else
                {
                    words[match.Value]++;
                }
            }
        }

        private void Check_TLP_MouseClick(object sender, MouseEventArgs e)
        {
            Correction_RichTextBox.Focus();
        }

        private void BKTree_BW_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            countWordsInFile("DataTraining.txt");
            correctionFinished = true;
            GC.Collect();
        }

        private void Correction_Check_Button_Click(object sender, EventArgs e)
        {
            StringHelper stringHelper = new StringHelper();
            if (correctionFinished == true)
            {
                if (Correction_RichTextBox.Text != stringHelper.getCorrectionPanelInitString())
                {
                    var wordPattern = new Regex(stringHelper.getRegexWordPatern());
                    foreach (Match match in wordPattern.Matches(Correction_RichTextBox.Text))
                    {
                        if (tree.Search(match.Value, 0).Count <= 0)
                        {
                            Correction_RichTextBox.Select(match.Index, match.Length);
                            Correction_RichTextBox.SelectionBackColor = Color.FromArgb(1, 243, 126, 120);
                        }
                        else
                        {
                            Correction_RichTextBox.Select(match.Index, match.Length);
                            Correction_RichTextBox.SelectionBackColor = Color.FromArgb(1, 59, 64, 69);
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show(stringHelper.infoPleaseWait());
            }
        }

        private void Correction_RichTextBox_TextChanged(object sender, EventArgs e)
        {
            Correction_RichTextBox.DeselectAll();
            if (Correction_RichTextBox.Text == "")
            {
                Correction_RichTextBox.SelectionBackColor = Color.FromArgb(1, 59, 64, 69);
                Correction_RichTextBox.ResetText();
            }
        }

        private void Correction_RichTextBox_Enter(object sender, EventArgs e)
        {
            StringHelper stringHelper = new StringHelper();
            if (Correction_RichTextBox.Text == stringHelper.getCorrectionPanelInitString())
            {
                Correction_RichTextBox.Text = "";
            }
        }

        private void Correction_RichTextBox_Leave(object sender, EventArgs e)
        {
            StringHelper stringHelper = new StringHelper();
            if (Correction_RichTextBox.Text == "")
            {
                Correction_RichTextBox.Text = stringHelper.getCorrectionPanelInitString();
            }
        }

        private void cutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Correction_RichTextBox.Cut();
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Correction_RichTextBox.Copy();
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Correction_RichTextBox.Paste();
        }

        private void checkToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Correction_Check_Button.PerformClick();
        }

        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Correction_RichTextBox.SelectAll();
        }

        private void Correction_RichTextBox_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            StringHelper stringHelper = new StringHelper();
            //reset popup and listbox to prevent overflow
            resetPopup();
            //get the word under the mouse
            if (correctionFinished == true)
            {
                if (Correction_RichTextBox.Text != "")
                {
                    Point p = new Point(e.X, e.Y);
                    string wordUnderMouse = getWordUnderMouse(p);
                    bool isCorrect = true;

                    if (wordUnderMouse.Trim() != "" && tree.Search(wordUnderMouse, 0).Count == 0)
                    {
                        //show a popup
                        isCorrect = false;
                        populateSuggestions(wordUnderMouse);
                    }
                    showPopup(p, isCorrect);
                }
            }
            else
            {
                MessageBox.Show(stringHelper.infoPleaseWait());
            }
        }

        private string getWordUnderMouse(Point e)
        {
            int pos = Correction_RichTextBox.GetCharIndexFromPosition(new Point(e.X, e.Y));
            string txt = Correction_RichTextBox.Text;
            int startPos;
            for (startPos = pos; startPos >= 0; startPos--)
            {
                char ch = txt[startPos];
                
                if (!(char.IsLetter(ch) || ch == '-' || ch == '\''))
                {
                    break;
                }
            }
            startPos++;
            int endPos;
            for (endPos = pos; endPos < txt.Length; endPos++)
            {
                char ch = txt[endPos];
                if (!(char.IsLetter(ch) || ch == '-' || ch == '\''))
                {
                    break;
                }
            }
            endPos--;
            if (startPos > endPos)
            {
                return "";
            }
            else
            {
                Correction_RichTextBox.Select(startPos, endPos - startPos + 1);
                selectedWordIndexLength = new Tuple<int, int>(startPos, endPos - startPos + 1);
                return txt.Substring(startPos, endPos - startPos + 1);
            }
        }

        private void populateSuggestions(string wordUnderMouse)
        {
            Dictionary<string, int> oneLength = new Dictionary<string, int>();
            Dictionary<string, int> twoLength = new Dictionary<string, int>();
            foreach (var item in tree.Search(wordUnderMouse, 1))
            {
                oneLength.Add(item, words[item]);
            }
            foreach (var item in tree.Search(wordUnderMouse, 2))
            {
                if (!oneLength.ContainsKey(item))
                {
                    twoLength.Add(item, words[item]);
                }
            }

            var oneList = oneLength.ToList();
            var twoList = twoLength.ToList();
            oneLength = null;
            twoLength = null;

            oneList.Sort((x, y) => (-1) * x.Value.CompareTo(y.Value));
            twoList.Sort((x, y) => (-1) * x.Value.CompareTo(y.Value));

            if (oneList.Count > 8)
            {
                foreach (var item in oneList.GetRange(0, 7))
                {
                    Suggestions_ListBox.Items.Add(item.Key);
                }
            }
            else
            {
                foreach (var item in oneList)
                {
                    Suggestions_ListBox.Items.Add(item.Key);
                }
            }

            if (twoList.Count > 8)
            {
                foreach (var item in twoList.GetRange(0, 7))
                {
                    Suggestions_ListBox.Items.Add(item.Key);
                }
            }
            else
            {
                foreach (var item in twoList)
                {
                    Suggestions_ListBox.Items.Add(item.Key);
                }
            }
        }

        private void showPopup(Point p, bool isCorrect)
        {
            if (isCorrect == true)
            {
                ToolStripControlHost searchWord = new ToolStripControlHost(Correction_Search_Popup_Button);
                searchWord.Margin = Padding.Empty;
                searchWord.Padding = Padding.Empty;
                searchWord.Dock = DockStyle.Top;
                popup.Items.Add(searchWord);
            }
            else
            {
                ToolStripControlHost suggestionsList = new ToolStripControlHost(Suggestions_ListBox);
                ToolStripControlHost learnWord = new ToolStripControlHost(Correction_Learn_Popup_Button);

                suggestionsList.Padding = Padding.Empty;
                suggestionsList.Margin = Padding.Empty;
                learnWord.Padding = Padding.Empty;
                learnWord.Margin = Padding.Empty;
                suggestionsList.Dock = DockStyle.Top;
                learnWord.Dock = DockStyle.Top;

                if (Suggestions_ListBox.Items.Count != 0)
                {
                    popup.Items.Add(suggestionsList);
                }
                else
                {
                    Suggestions_ListBox.Hide();
                }
                popup.Items.Add(learnWord);
            }
            popup.Show(Correction_RichTextBox, p.X + 20, p.Y + 20);
        }

        private void resetPopup()
        {
            popup.Items.Clear();
            if (Suggestions_ListBox.Items.Count != 0)
            {
                Suggestions_ListBox.Items.Clear();
            }
            popup.Hide();
        }

        private void Suggestions_ListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            Correction_RichTextBox.Select(selectedWordIndexLength.Item1, selectedWordIndexLength.Item2);
            Correction_RichTextBox.SelectedText = Suggestions_ListBox.SelectedItem.ToString();
            Correction_Check_Button.PerformClick();
            resetPopup();
        }

        private void Correction_Learn_Button_Click(object sender, EventArgs e)
        {
            StringHelper stringHelper = new StringHelper();
            if (Correction_RichTextBox.Text != stringHelper.getCorrectionPanelInitString())
            {
                Correction_RichTextBox.Select(selectedWordIndexLength.Item1, selectedWordIndexLength.Item2);
                string word = Correction_RichTextBox.SelectedText;
                if (tree.Search(word, 0).Count == 0)
                {
                    File.AppendAllText(stringHelper.getDataTrainingFileName(), Environment.NewLine + word);
                    words.Add(word, 1);
                    tree.Add(word);
                    Correction_Check_Button.PerformClick();
                }
            }
        }

        private void Correction_RichTextBox_VScroll(object sender, EventArgs e)
        {
            resetPopup();
        }

        private void Correction_Learn_Popup_Button_Click(object sender, EventArgs e)
        {
            Correction_Learn_Button.PerformClick();
            resetPopup();
        }

        private void Correction_Search_Popup_Button_Click(object sender, EventArgs e)
        {
            StringHelper stringHelper = new StringHelper();
            if (Correction_RichTextBox.Text != stringHelper.getCorrectionPanelInitString())
            {
                Correction_RichTextBox.Select(selectedWordIndexLength.Item1, selectedWordIndexLength.Item2);
                string word = Correction_RichTextBox.SelectedText;
                resetPopup();
                Search_Side_Button.PerformClick();
                Search_TextBox.Text = word;
            }
        }

        #endregion Region Correction Panel

        #region Learning Panel

        /// Redraw TabControl
        private void LearnTabControl_DrawItem(Object sender, System.Windows.Forms.DrawItemEventArgs e)
        {
            Graphics g = e.Graphics;
            Brush _textBrush, _fillBrush;
            _fillBrush = new SolidBrush(Color.FromArgb(24, 24, 25));
            Pen p = new Pen(Color.FromArgb(24, 24, 25));
            // Get the item from the collection.
            TabPage _tabPage = Learn_TabC.TabPages[e.Index];

            // Get the real bounds for the tab rectangle.
            Rectangle _tabBounds = Learn_TabC.GetTabRect(e.Index);
            e.DrawBackground();
            if (e.State == DrawItemState.Selected)
            {
                // Draw a different background color, and don't paint a focus rectangle.
                _textBrush = new SolidBrush(Color.Gold);
                g.FillRectangle(_fillBrush, e.Bounds);
            }
            else
            {
                _textBrush = new System.Drawing.SolidBrush(Color.FromArgb(24, 24, 25));
            }

            // Use our own font.
            Font _tabFont = new Font("Segoe UI", 14f, FontStyle.Bold, GraphicsUnit.Pixel);

            // Draw string. Center the text.
            StringFormat _stringFlags = new StringFormat();
            _stringFlags.Alignment = StringAlignment.Center;
            _stringFlags.LineAlignment = StringAlignment.Center;
            g.DrawString(_tabPage.Text, _tabFont, _textBrush, _tabBounds, new StringFormat(_stringFlags));
        }


        private List<string> getDataMyWords()
        {
            string query;
            SQLiteConnection sqliteConnection = new SQLiteConnection(ConnectionHelper.GetConnectionString());
            sqliteConnection.Open();
            DataTable datatable1 = new DataTable();
            
            query = "Select Word, MyWords.id from MyWords, Words where Words.id = MyWords.id";

            SQLiteCommand sqliteCommand = new SQLiteCommand(query, sqliteConnection);
            SQLiteDataAdapter dataAdapter = new SQLiteDataAdapter(sqliteCommand);
            dataAdapter.Fill(datatable1);
            myWordsList = new List<string>();

            foreach (DataRow row in datatable1.Rows)
            {
                myWordsList.Add(row["Word"].ToString());
            }
            sqliteConnection.Close();
            sqliteCommand.Dispose();
            dataAdapter.Dispose();

            return myWordsList;
        }
        
        
        private void MyWords_Learning()
        {
            Learning_LB.DataSource = getDataMyWords();
        }

        private void colorLearningRtb()
        {
            StringHelper stringHelper = new StringHelper();
            Regex r = new Regex(stringHelper.getRegexSearchResultWordType());
            foreach (Match match in r.Matches(Learning_RTB.Text))
            {
                Learning_RTB.Select(match.Index, match.Length);
                Learning_RTB.SelectionColor = Color.FromArgb(21, 195, 154);
                Learning_RTB.SelectionFont = new Font(Learning_RTB.SelectionFont, FontStyle.Bold);
            }
            r = new Regex(stringHelper.getRegexSearchResultWordDef());
            foreach (Match match in r.Matches(Learning_RTB.Text))
            {
                Learning_RTB.Select(match.Index, match.Length);
                Learning_RTB.SelectionColor = Color.LightGoldenrodYellow;
                Learning_RTB.SelectionFont = new Font(Learning_RTB.SelectionFont, FontStyle.Bold);

            }
            r = new Regex(stringHelper.getRegexSearchResultWordExample());
            foreach (Match match in r.Matches(Learning_RTB.Text))
            {
                Learning_RTB.Select(match.Index, match.Length);
                Learning_RTB.SelectionColor = Color.WhiteSmoke;
                Learning_RTB.SelectionFont = new Font(Learning_RTB.SelectionFont, FontStyle.Italic);
            }
        }

        private void Learning_LB_SelectedIndexChanged(object sender, EventArgs e)
        {
            string query;
            SQLiteConnection sQLiteConnection = new SQLiteConnection(ConnectionHelper.GetConnectionString());
            query = "Select Word,Ipa,Def from Words where Word = '" + Learning_LB.Text + "' ";
            SQLiteCommand sQLiteCommand = new SQLiteCommand(query, sQLiteConnection);
            SQLiteDataReader myReader;

            try
            {
                sQLiteConnection.Open();
                myReader = sQLiteCommand.ExecuteReader();

                while (myReader.Read())
                {
                    Learning_RTB.ResetText();
                    Learning_Word.ResetText();
                    string def = myReader.GetString(2);
                    string word = myReader.GetString(0);
                    string ipa = myReader.GetString(1);
                    Learning_Word.Text = word;
                    Learning_RTB.AppendText(ipa);
                    Learning_RTB.AppendText(def);
                    
                    colorLearningRtb();
                }       
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            sQLiteConnection.Close();

        }

        private string selectedItem;
        int selectedIndex;

        private void DONE_Click(object sender, EventArgs e)
        {
            
            selectedItem = Learning_LB.SelectedItem.ToString();
            selectedIndex = Learning_LB.SelectedIndex;

            Learned_LB.Items.Add(selectedItem);
            if (myWordsList != null)
            {
                myWordsList.RemoveAt(selectedIndex);
            }

            DataBinding();
        }

        private void DataBinding()
        {
            Learning_LB.DataSource = null;
            Learning_LB.DataSource = myWordsList;
        }

        private void DELETE_Click(object sender, EventArgs e)
        {
            selectedItem = Learned_LB.SelectedItem.ToString();
            selectedIndex = Learned_LB.SelectedIndex;
            myWordsList.Add(selectedItem);
            Learned_LB.Items.RemoveAt(Learned_LB.Items.IndexOf(Learned_LB.SelectedItem));
            DataBinding();
        }
        private void Hide_Delete_Button(object sender, EventArgs e)
        {
            DELETE.Hide();
        }

        private void Hide_Done_Button(object sender, EventArgs e)
        {
            DONE.Hide();
        }

        private void Show_Delete_Button(object sender, EventArgs e)
        {
            DELETE.Show();
        }
        private void Show_Done_Button(object sender, EventArgs e)
        {
            DONE.Show();
        }
        private void LearningTabC_SelectedIndexChanged(object sender, EventArgs e)

        {
            if (Learn_TabC.SelectedTab == Learning_Tab1)
            {
                DONE.Show();
                DELETE.Hide();
            }
            else if (Learn_TabC.SelectedTab == Learning_Tab2)
            {
                DELETE.Show();
                DONE.Hide();
            }
        }

        private void Learn_Button_Click(object sender, EventArgs e)
        {
            string query;
            SQLiteConnection sQLiteConnection = new SQLiteConnection(ConnectionHelper.GetConnectionString());

            query = "INSERT INTO MyWords (id) SELECT Words.id from  Words WHERE Words.Word = '" + Result_Word_Label.Text + "' ";

            SQLiteCommand sQLiteCommand = new SQLiteCommand(query, sQLiteConnection);
            SQLiteDataReader dbr;

            try
            {
                sQLiteConnection.Open();
                dbr = sQLiteCommand.ExecuteReader();
                MessageBox.Show("Đã thêm vào từ của bạn");
                // retake data 
                MyWords_Learning();
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            sQLiteConnection.Close();

        }

        #endregion Learning Panel


        private void btn_translate_Click(object sender, EventArgs e)
        {
            string strTranslatedText = null;
            try
            {
                TranslatorService.LanguageServiceClient client = new TranslatorService.LanguageServiceClient();
                client = new TranslatorService.LanguageServiceClient();
                strTranslatedText = client.Translate("6CE9C85A41571C050C379F60DA173D286384E0F2", txt_input.Text, "", "en");
                txt_output.Text = strTranslatedText;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnOtherLang_Click(object sender, EventArgs e)
        {
            OtherLanguages other = new OtherLanguages();
            other.ShowDialog();
        }
    }
}