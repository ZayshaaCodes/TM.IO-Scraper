using System.Diagnostics;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace TMioParse.Utilities
{
    public static class Utils
    {
        public static IWebElement ForLoadMoreButton(this WebDriverWait wait)
        {
            return wait.Until(wd => wd.FindElement(By.XPath("//button[.='Load more...']")));
        }
        
        public static IWebElement ForSearchBox(this WebDriverWait wait)
        {
            return wait.Until(wd => wd.FindElement(By.XPath("//input[@placeholder='Search']")));
        }

        public static void OpenWithDefaultProgram(string path)
        {
            using var fileOpener = new Process {StartInfo = {FileName = "explorer", Arguments = "\"" + path + "\""}};
            fileOpener.Start();
        }
    }
}