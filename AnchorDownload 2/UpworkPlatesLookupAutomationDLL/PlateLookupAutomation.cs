using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace UpworkPlatesLookupAutomationDLL
{
    public class PlateLookupAutomation
    {

        public static WebBrowser br;

        public string RegistrationMark { get; set; }
        public string CaseNumber { get; set; }
        public DateTime Date { get; set; }
        public Action<Result> Callback { get; set; }
        public string user { get; set; } = "sarah.sutton@jbrcapital.com";
        public string password { get; set; } = "Lamborghini773!";
        public bool IsLoggedIn { get; set; } = false;
        public bool IsLookingUp { get; set; } = false;

        public PlateLookupAutomation()
        {
            SetBrowserFeatureControl();
            Login();
        }

        public void Login()
        {
            var th = new Thread(() =>
            {
                br = new WebBrowser();
                br.DocumentCompleted += browser_DocumentCompleted;
                try
                {
                    br.Navigate("https://www.askmid.com/login.aspx");
                }
                catch (Exception ex)
                {
                    br.Navigate("https://www.askmid.com/login.aspx");
                }
                Application.Run();
            });
            th.SetApartmentState(ApartmentState.STA);
            th.Start();
        }

        bool onlyOnce = false;
        void browser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            var br = sender as WebBrowser;
            //making sure everything loaded fine
            switch (e.Url.ToString())
            {
                case "https://www.askmid.com/login.aspx":
                    {
                        if (onlyOnce) { return; }
                        onlyOnce = true;
                        HandleLoginEvents(br);
                        Thread.Sleep(4);
                        break;
                    }
                case "https://www.askmid.com/askmid.aspx":
                    {
                        HandleDataFilling(br);
                        break;
                    }
                case "https://www.askmid.com/accounthome.aspx":
                    {
                        IsLoggedIn = true;
                        break;
                    }
                case "https://www.askmid.com/askmidresults.aspx":
                    {
                        CheckResult(br);
                        break;
                    }
            }
        }

        private void CheckResult(WebBrowser br)
        {
            if (br.DocumentText.Contains("no insurance details have been found on the MID.") || br.Document.GetElementByTagAndAttribute("span", "id", "ctl00_mainContent_ctl01_ctl01_midresults") == null)
            {
                Callback(null);
            }
            else
            {
                var result = br.Document.GetElementByTagAndAttribute("span", "id", "ctl00_mainContent_ctl01_ctl01_midresults")
                                .InnerHtml.Split(new[] { "<br>" }, StringSplitOptions.RemoveEmptyEntries);
                Callback(new Result()
                {
                    PolicyNumber = result[0].Trim('"').Replace("Policy Number:", string.Empty).Trim(),
                    Insurer = result[1].Trim('"').Replace("Insurer:", string.Empty).Trim(),
                    ClaimsContact = result[2].Trim('"').Replace("Claims Contact:", string.Empty).Trim()
                });
            }
            IsLookingUp = false;
        }

        private void HandleLoginEvents(WebBrowser br)
        {
            if (!br.DocumentText.Contains("Your email address is not recognised"))
            {
                DoLogin(br);
                Console.WriteLine("Trying to login");
            }
            else
                IsLoggedIn = true;
        }

        private void HandleDataFilling(WebBrowser br)
        {
            br.Document.GetElementByTagAndAttribute("input", "name", "ctl00$mainContent$ctl01$ctl01$Wizard1$EnquiryRef").SetValue(CaseNumber);
            br.Document.GetElementByTagAndAttribute("input", "name", "ctl00$mainContent$ctl01$ctl01$Wizard1$tpVRM").SetValue(RegistrationMark);
            br.Document.GetElementByTagAndAttribute("input", "name", "ctl00$mainContent$ctl01$ctl01$Wizard1$IncidentDate").SetValue(Date.ToString("dd-MM-yyyy"));
            br.Document.GetElementByTagAndAttribute("input", "name", "ctl00$mainContent$ctl01$ctl01$Wizard1$CheckTandC").SetAttribute("checked", "true");
            br.Document.GetElementByTagAndAttribute("input", "name", "ctl00$mainContent$ctl01$ctl01$Wizard1$CheckTofU").SetAttribute("checked", "true");
            var FinishButton = br.Document.GetElementByTagAndAttribute("input", "name", "ctl00$mainContent$ctl01$ctl01$Wizard1$FinishNavigationTemplateContainerID$FinishButton");
            var firstvalidation = br.Document.GetElementByTagAndAttribute("span", "id", "ctl00_mainContent_ctl01_ctl01_Wizard1_tpVRMFieldValidator");
            var secondvalidation = br.Document.GetElementByTagAndAttribute("span", "id", "ctl00_mainContent_ctl01_ctl01_Wizard1_RegularExpressionValidator4");
            var thirdvalidation = br.Document.GetElementByTagAndAttribute("span", "id", "ctl00_mainContent_ctl01_ctl01_Wizard1_EnquiryRefFieldValidator");
                FinishButton.InvokeMember("Click");
            if (!firstvalidation.NotDisplayedInlineStyle() ||
            !secondvalidation.NotDisplayedInlineStyle() ||
            !thirdvalidation.NotDisplayedInlineStyle())
            {
                IsLookingUp = false;
                Callback(null);
            }
        }


        public void Lookup(Action<Result> Callback, string registrationMark, string caseNumber, DateTime date)
        {
            br.Navigate("https://www.askmid.com/askmid.aspx");
            this.Callback = Callback;
            IsLookingUp = true;
            RegistrationMark = registrationMark;
            CaseNumber = caseNumber;
            Date = date;
        }

        private void DoLogin(WebBrowser br)
        {
            br.Document.GetElementByTagAndAttribute("input", "name", "ctl00$mainContent$Login1$UserName").SetValue(user);
            br.Document.GetElementByTagAndAttribute("input", "name", "ctl00$mainContent$Login1$Password").SetValue(password);
            br.Document.GetElementByTagAndAttribute("input", "name", "ctl00$mainContent$Login1$LoginButton").InvokeMember("Click");
        }

        private static void SetBrowserFeatureControl()
        {
            // http://msdn.microsoft.com/en-us/library/ee330720(v=vs.85).aspx

            // FeatureControl settings are per-process
            var fileName = Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName);

            // make the control is not running inside Visual Studio Designer
            if (string.Compare(fileName, "devenv.exe", true) == 0
                || string.Compare(fileName, "XDesProc.exe", true) == 0)
                return;

            SetBrowserFeatureControlKey("FEATURE_BROWSER_EMULATION",
                                        fileName,
                                        GetBrowserEmulationMode()); // Webpages containing standards-based !DOCTYPE directives are displayed in IE10 Standards mode.
            SetBrowserFeatureControlKey("FEATURE_AJAX_CONNECTIONEVENTS", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_ENABLE_CLIPCHILDREN_OPTIMIZATION", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_MANAGE_SCRIPT_CIRCULAR_REFS", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_DOMSTORAGE ", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_GPU_RENDERING ", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_IVIEWOBJECTDRAW_DMLT9_WITH_GDI  ", fileName, 0);
            SetBrowserFeatureControlKey("FEATURE_DISABLE_LEGACY_COMPRESSION", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_LOCALMACHINE_LOCKDOWN", fileName, 0);
            SetBrowserFeatureControlKey("FEATURE_BLOCK_LMZ_OBJECT", fileName, 0);
            SetBrowserFeatureControlKey("FEATURE_BLOCK_LMZ_SCRIPT", fileName, 0);
            SetBrowserFeatureControlKey("FEATURE_DISABLE_NAVIGATION_SOUNDS", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_SCRIPTURL_MITIGATION", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_SPELLCHECKING", fileName, 0);
            SetBrowserFeatureControlKey("FEATURE_STATUS_BAR_THROTTLING", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_TABBED_BROWSING", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_VALIDATE_NAVIGATE_URL", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_WEBOC_DOCUMENT_ZOOM", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_WEBOC_POPUPMANAGEMENT", fileName, 0);
            SetBrowserFeatureControlKey("FEATURE_WEBOC_MOVESIZECHILD", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_ADDON_MANAGEMENT", fileName, 0);
            SetBrowserFeatureControlKey("FEATURE_WEBSOCKET", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_WINDOW_RESTRICTIONS ", fileName, 0);
            SetBrowserFeatureControlKey("FEATURE_XMLHTTP", fileName, 1);
        }

        private static void SetBrowserFeatureControlKey(string feature,
                                                    string appName,
                                                    uint value)
        {
            using (var key = Registry.CurrentUser.CreateSubKey(
                                                                string.Concat(@"Software\Microsoft\Internet Explorer\Main\FeatureControl\", feature),
                                                                RegistryKeyPermissionCheck.ReadWriteSubTree))
            {
                key.SetValue(appName, (uint)value, RegistryValueKind.DWord);
            }
        }

        private static uint GetBrowserEmulationMode()
        {
            var browserVersion = 7;
            using (var ieKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Internet Explorer",
                                                                RegistryKeyPermissionCheck.ReadSubTree,
                                                                RegistryRights.QueryValues))
            {
                var version = ieKey.GetValue("svcVersion");
                if (null == version)
                {
                    version = ieKey.GetValue("Version");
                    if (null == version)
                        throw new ApplicationException("Microsoft Internet Explorer is required!");
                }

                int.TryParse(version.ToString().Split('.')[0], out browserVersion);
            }

            uint
                mode = 11000; // Internet Explorer 11. Webpages containing standards-based !DOCTYPE directives are displayed in IE11 Standards mode. Default value for Internet Explorer 11.
            switch (browserVersion)
            {
                case 7:
                    mode = 7000; // Webpages containing standards-based !DOCTYPE directives are displayed in IE7 Standards mode. Default value for applications hosting the WebBrowser Control.
                    break;
                case 8:
                    mode = 8000; // Webpages containing standards-based !DOCTYPE directives are displayed in IE8 mode. Default value for Internet Explorer 8
                    break;
                case 9:
                    mode = 9000; // Internet Explorer 9. Webpages containing standards-based !DOCTYPE directives are displayed in IE9 mode. Default value for Internet Explorer 9.
                    break;
                case 10:
                    mode = 10000; // Internet Explorer 10. Webpages containing standards-based !DOCTYPE directives are displayed in IE10 mode. Default value for Internet Explorer 10.
                    break;
                default:
                    // use IE11 mode by default
                    break;
            }

            return mode;
        }
    }
}
