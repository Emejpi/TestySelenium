using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeleniumTest
{
    public enum Outcome
    {
        junior,
        młodzik,
        dorosły,
        senior,
        nie_zakwalifikowany,
        none
    }

    class TestCase //pojedynczy przypadek testowy
    {
        public static int errorID;

        public TestCase(int age, int birthDate, bool parentsAgreement, bool doctorCertificate, Outcome outcome)
        {
            this.age = age;
            this.birthDate = birthDate;
            this.parentsAgreement = parentsAgreement;
            this.doctorCertificate = doctorCertificate;
            this.outcome = outcome;
        }

        public void DebugOutcome(Outcome outcome, string outcomeText) //sprawdzenie czy odpowiedz systemu jest poprawna i wypisanie informacji o bledzie
        {
            if(this.outcome != outcome)
            {
                Console.WriteLine("Błąd#" + ++errorID + ": dla osoby w wieku " + age + " lat, "
                    + (parentsAgreement ? "ze zgodą rodzica, " : "bez zgody rodzica, ")
                    + (doctorCertificate ? "z zaświadczeniem lekarza " : "bez zgody lekarza ")
                    + "system zwrócił " + outcomeText
                    + ", a powinien zwrócić " + this.outcome.ToString());
            }
        }

        public int age;
        public int birthDate; //indeks do tablicy dates
        public bool parentsAgreement;
        public bool doctorCertificate; //zaświadczenie lekarza. Wiem, że brzmi jak certyfikat dektora, ale nei wieme jak jest zaświadczenie po angieslu xd
        public Outcome outcome;
    }

    class Tester //klasa odpowiadajaca za testowanie strony
    {
        int[] ageGates = { 10, 14, 18, 65 };

        DateTime[] dates;

        TestCase[] testCases = {
            new TestCase(10, 1, true, true, Outcome.młodzik),
            new TestCase(13, 2, true, true, Outcome.młodzik),
            new TestCase(14, 3, true, true, Outcome.junior),
            new TestCase(17, 4, true, true, Outcome.junior),
            new TestCase(18, 5, false, false, Outcome.dorosły),
            new TestCase(64, 6, false, false, Outcome.dorosły),
            new TestCase(65, 7, false, true, Outcome.senior),
            new TestCase(65, 7, false, false, Outcome.nie_zakwalifikowany),
            new TestCase(10, 1, false, false, Outcome.nie_zakwalifikowany),
            new TestCase(13, 2, false, false, Outcome.nie_zakwalifikowany),
            new TestCase(14, 3, false, false, Outcome.nie_zakwalifikowany),
            new TestCase(17, 4, false, false, Outcome.nie_zakwalifikowany),
            new TestCase(9, 0, false, false, Outcome.nie_zakwalifikowany)
        };

        public Tester()
        {
            SetBirthDates();
        }

        public void Test()
        {
            TestCase.errorID = 0;

            IWebDriver driver = new ChromeDriver();

            driver.Navigate().GoToUrl("https://lamp.ii.us.edu.pl/~mtdyd/zawody/");

            IWebElement nameInput = driver.FindElement(By.Id("inputEmail3"));
            nameInput.SendKeys("Michał");

            IWebElement lastNameInput = driver.FindElement(By.Id("inputPassword3"));
            lastNameInput.SendKeys("Marzec");

            IWebElement dateInput = driver.FindElement(By.Id("dataU"));
            IWebElement parentsAgreementInput = driver.FindElement(By.Id("rodzice"));
            IWebElement doctorCertificateInput = driver.FindElement(By.Id("lekarz"));
            IWebElement send = driver.FindElement(By.XPath("//*[@id='formma']/div[6]/div/button"));

            parentsAgreementInput.Click();

            Console.WriteLine();

            foreach (TestCase testCase in testCases)
            {
                int birthID = testCase.birthDate;
                bool parentsAgree = testCase.parentsAgreement;
                bool doctorCerti = testCase.doctorCertificate;

                dateInput.Clear();
                dateInput.SendKeys(dates[birthID].ToShortDateString());

                if ((parentsAgree && !parentsAgreementInput.Selected)
                    || (!parentsAgree && parentsAgreementInput.Selected))
                    parentsAgreementInput.Click();

                if ((doctorCerti && !doctorCertificateInput.Selected)
                    || (!doctorCerti && doctorCertificateInput.Selected))
                    doctorCertificateInput.Click();

                send.Click();

                driver.SwitchTo().Alert().Accept();
                string alertText = driver.SwitchTo().Alert().Text;

                switch(alertText)
                {
                    case "Brak kwalifikacji":
                        testCase.DebugOutcome(Outcome.nie_zakwalifikowany, alertText);
                        break;

                    case "Junior":
                        testCase.DebugOutcome(Outcome.junior, alertText);
                        break;

                    case "Mlodzik":
                        testCase.DebugOutcome(Outcome.młodzik, alertText);
                        break;

                    case "Dorosly":
                        testCase.DebugOutcome(Outcome.dorosły, alertText);
                        break;

                    case "Senior":
                        testCase.DebugOutcome(Outcome.junior, alertText);
                        break;

                    default:
                        testCase.DebugOutcome(Outcome.none, alertText);
                        break;
                }

                driver.SwitchTo().Alert().Accept();
            }

            Console.WriteLine();

            Console.WriteLine("Testy zakończone. Zostało znalezione " + TestCase.errorID + " błędów.");
        }

        void SetBirthDates() //ustawia dates na wartości brzegowe każdego wieku z ageGates, czyli data urodzin i dzień przed urodzinami
        {
            dates = new DateTime[ageGates.Length * 2];

            DateTime currentDate = DateTime.Now;

            for (int i = 0; i < ageGates.Length; i++)
            {
                int currentAge = ageGates[i];

                dates[i * 2] = currentDate.AddYears(-currentAge).AddDays(-1);
                dates[i * 2 + 1] = currentDate.AddYears(-currentAge);
            }

            for (int i = 0; i < dates.Length; i++)
            {
                Console.WriteLine(dates[i].ToShortDateString());
            }
        }
    }

    class Program
    {
     
        static void Main(string[] args)
        {
            Tester tester = new Tester();
            tester.Test();
        }

    }
}
