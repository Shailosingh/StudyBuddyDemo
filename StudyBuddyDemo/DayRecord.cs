using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudyBuddyDemo
{
    public class DayRecord
    {
        //Datafields
        [JsonProperty]
        public TimeSpan TimeStudiedToday { get; set; }
        [JsonProperty]
        public string Date { get; set; }
        [JsonProperty]
        public long TodaysBalance { get; set; }

        //Constructor
        public DayRecord()
        {
            //Check if today's date already exists as a record
            string userPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string studyBuddySavesPath = Path.Combine(userPath, @"Study Buddy Saves");
            string dateRecordSavePaths = Path.Combine(studyBuddySavesPath, @"Date Records");
            string todayDatePath = Path.Combine(dateRecordSavePaths, $@"{DateOnly.FromDateTime(DateTime.Now).ToString("MM-dd-yyyy")}.json");

            //If the record exists, deserialize the JSON into the object
            if(File.Exists(todayDatePath))
            {
                //Deserialize file into object
                string dayRecordString = File.ReadAllText(todayDatePath);
                DayRecord dayObject = JsonConvert.DeserializeObject<DayRecord>(dayRecordString);

                //Setup object values into this
                this.TodaysBalance = dayObject.TodaysBalance;
                this.Date = dayObject.Date;
                this.TimeStudiedToday = dayObject.TimeStudiedToday;
            }

            //If it doesn't exist, create a record for today and save in file
            else
            {
                //Setup initial values
                this.Date = DateOnly.FromDateTime(DateTime.Now).ToString("MM-dd-yyyy");
                this.TimeStudiedToday = new TimeSpan();
                this.TodaysBalance = 0;

                //Serialize to JSON and save
                string dayRecordString = JsonConvert.SerializeObject(this, Formatting.Indented);
                File.WriteAllText(todayDatePath, dayRecordString);
            }
        }

        [JsonConstructor]
        public DayRecord(TimeSpan timeStudiedToday, string date, long todaysBalance)
        {
            this.Date = date;
            this.TimeStudiedToday = timeStudiedToday;
            this.TodaysBalance = todaysBalance;
        }

        /// <summary>
        /// Adds time studied to the date record
        /// </summary>
        /// <param name="studyTime">Time studied</param>
        public void AddTimeStudied(TimeSpan studyTime)
        {
            this.TimeStudiedToday += studyTime;
            SaveDayFile();
        }

        /// <summary>
        /// Increments today's balance record (Accepts negatives)
        /// </summary>
        /// <param name="newFund">New coins earned or withdrawn</param>
        public void IncrementFunds(long newFund)
        {
            this.TodaysBalance += newFund;
            SaveDayFile();
        }

        public override string ToString()
        {
            string stringOutput = $"{DateOnly.ParseExact(Date, "MM-dd-yyyy").ToLongDateString()}:\n" +
                                  $"Time Studied: {TimeStudiedToday}\n" +
                                  $"Coins Earned: {TodaysBalance} Coins";

            return stringOutput;
        }

        /// <summary>
        /// Saves current object into its repective day record
        /// </summary>
        private void SaveDayFile()
        {
            //Get the path for the save file
            string userPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string studyBuddySavesPath = Path.Combine(userPath, @"Study Buddy Saves");
            string dateRecordSavePaths = Path.Combine(studyBuddySavesPath, @"Date Records");
            string todayDatePath = Path.Combine(dateRecordSavePaths, $@"{this.Date}.json");

            //Serialize this into the save file
            string dayRecordString = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(todayDatePath, dayRecordString);
        }
    }
}