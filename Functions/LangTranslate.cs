using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Collections;
using System.Net.Http.Headers;
using System.Web.Script.Serialization;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualBasic.Devices;
using System.Globalization;

namespace Functions_for_Dynamics_Operations
{
    public class AzureTranslate
    {
        public static string Url { get; set; }
        public static string Region { get; set; }
        public static string Secret { get; set; }
    }

    public class LangTranslate
    {
        public string TranslateAzure(string input, string languageFrom, string languageTo)
        {
            try
            {
                string url = string.Format($"{AzureTranslate.Url}/translate?api-version=3.0&from={languageFrom}&to={languageTo}");

                HttpClient httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", AzureTranslate.Secret);
                httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Region", AzureTranslate.Region);
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage responseMessage = httpClient.PostAsync(url, new StringContent("[{'Text':'" + input + "'}]", Encoding.UTF8, "application/json")).ConfigureAwait(false).GetAwaiter().GetResult();
                if (responseMessage.IsSuccessStatusCode)
                {
                    string response = responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                    JArray translated = (JArray)JsonConvert.DeserializeObject(response);
                    if (!(translated is null))
                    {
                        JArray translatedObj = (JArray)translated.First["translations"];
                        if (!(translatedObj is null))
                        {
                            string text = (string)translatedObj.First["text"];

                            if (text.Contains("\r"))
                                text = text.Replace("\r", "\\r");

                            if (text.Contains("\n"))
                                text = text.Replace("\n", "\\n");

                            return text;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                VStudioUtils.LogToOutput($"Failure to translate - {ex}{Environment.NewLine}");
            }

            return input;
        }

        public static string TranslateText(string input, string languageFrom, string languageTo)
        {
            try
            {
                HttpClient httpClient = new HttpClient();

                string url = string.Format("https://translate.googleapis.com/translate_a/single?client=gtx&sl={0}&tl={1}&dt=t&q={2}", languageFrom, languageTo, Uri.EscapeUriString(input));
                string result = GetResult(httpClient.GetStringAsync(url).ConfigureAwait(false).GetAwaiter().GetResult());

                return AmendPlaceHolders(result);
            }
            catch (Exception)
            {
                return input;
            }
        }

        private static string GetResult(string result)
        {
            // Get all json data
            var jsonData = new JavaScriptSerializer().Deserialize<List<dynamic>>(result);

            // Extract just the first array element (This is the only data we are interested in)
            var translationItems = jsonData[0];

            // Translation Data
            string translation = "";

            // Loop through the collection extracting the translated objects
            foreach (object item in translationItems)
            {
                // Convert the item array to IEnumerable
                IEnumerable translationLineObject = item as IEnumerable;

                // Convert the IEnumerable translationLineObject to a IEnumerator
                IEnumerator translationLineString = translationLineObject.GetEnumerator();

                // Get first object in IEnumerator
                translationLineString.MoveNext();

                // Save its value (translated text)
                translation += string.Format(" {0}", Convert.ToString(translationLineString.Current));
            }

            // Remove first blank character
            if (translation.Length > 1) { translation = translation.Substring(1); };

            // Return translation
            return translation;
        }

        private static string AmendPlaceHolders(string stringToAmend)
        {   // Google's API has the tendancy to refactor certain values
            stringToAmend = ReplaceValues(stringToAmend, "% ", " %");

            stringToAmend = ReplaceValues(stringToAmend, @"\ r \ n", @"\r\n");

            stringToAmend = ReplaceValues(stringToAmend, @"\ n", @"\n");

            if (stringToAmend.Contains(@"\r\n") && stringToAmend.Substring(stringToAmend.Length - 4) == @"\r\n")
            {
                stringToAmend = stringToAmend.Replace(@"\r\n", "");
            }

            if (stringToAmend.Contains(@"\n") && stringToAmend.Substring(stringToAmend.Length - 4) == @"\n")
            {
                stringToAmend = stringToAmend.Replace(@"\n", "");
            }

            return stringToAmend.TrimEnd();
        }

        private static string ReplaceValues(string stringToFix, string toReplace, string replaceWith)
        {
            if (stringToFix.Contains(toReplace))
            {
                stringToFix = stringToFix.Replace(toReplace, replaceWith);
            }

            return stringToFix;
        }
        public static string GetLanguageId(string fileid)
        {
            int index;

            if (fileid.Contains("-"))
            {
                index = fileid.IndexOf("-");
                return fileid.Substring(index - 2, 5).ToLower();
            }
            else
            {
                index = fileid.IndexOf(".label");
                return fileid.Substring(index - 2, 2).ToLower();
            }
        }

        public static string GetDynamicsLanguage(string prefix, string fileid)
        {
            int start = prefix.Length + 1;

            string languageIso = fileid.Substring(start, fileid.IndexOf(".label") - start);

            return languageIso;
        }
    }
}
