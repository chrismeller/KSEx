using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Collections.Specialized;
using System.Globalization;

namespace KSEx
{
    class Program
    {

        const string CSV = "../../ProgrammingTestDataSet.csv";

        static void Main(string[] args)
        {

            // build a LINQ query to pull in all the lines of our CSV file
            var transactions = (from line in File.ReadAllLines(Program.CSV)
                                // there are multiple carriage returns at the end of each line -- damn you! filter those out
                                where !String.IsNullOrEmpty(line)
                                // split apart the CSV elements on this line, keeping empty ones so our indexes always line up
                                let pieces = line.Split(new[] { ',' }, StringSplitOptions.None)
                                // build the object we'll "return"... to the outer query
                                select new
                                {
                                    PageName = pieces[0],
                                    CookieID = pieces[1],
                                    VisitDateTime = DateTime.Parse(pieces[2])
                                })
                               // weed out results that are mysteriously missing a unique identifier
                               // it doesn't make sense to assume all those are a single visitor, or different visitors...
                               .Where(result => !String.IsNullOrEmpty(result.CookieID))
                               // make sure they're in ascending order by page view time
                               .OrderBy(result => result.VisitDateTime)
                               // group them by customer so we can track each user's transition
                               .GroupBy(result => result.CookieID)
                               // and here's the object we build that we'll iterate through
                               .Select(x => new
                               {
                                   CookieID = x.Key,
                                   Transactions = x
                               });

            // and this is how we'll store our arbitrary keys for page names
            var elapsed_times = new Dictionary<string, double>();
            var elapsed_views = new Dictionary<string, int>();

            foreach (var customer in transactions)
            {

                string last_page_name = null;
                DateTime? last_page_visit = null;

                foreach (var transaction in customer.Transactions)
                {

                    // make sure there is a value for all page names in both of our improvised arbitrary storage devices
                    if (!elapsed_times.ContainsKey(transaction.PageName))
                    {
                        elapsed_times.Add(transaction.PageName, 0.0);
                    }

                    if (!elapsed_views.ContainsKey(transaction.PageName))
                    {
                        elapsed_views.Add(transaction.PageName, 0);
                    }

                    // we only want to calculate the elapsed time if there is a previous page view for this customer
                    if (!String.IsNullOrEmpty(last_page_name) && last_page_visit.HasValue)
                    {

                        TimeSpan? difference = transaction.VisitDateTime - last_page_visit;

                        elapsed_times[last_page_name] += difference.Value.TotalSeconds;
                        elapsed_views[last_page_name] += 1;

                    }

                    // always save the last transaction's info though
                    last_page_name = transaction.PageName;
                    last_page_visit = transaction.VisitDateTime;
                }

            }

            // now we finally calculate our totals for all the types of pages we've seen
            foreach (var page_type in elapsed_times.Keys)
            {

                var average_time = elapsed_times[page_type] / elapsed_views[page_type];

                Console.WriteLine(page_type + ": " + average_time.ToString("N") + " seconds average over " + elapsed_views[page_type].ToString("N0") + " views");

            }

            Console.ReadKey();
        }
    }
}
