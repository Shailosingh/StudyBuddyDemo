using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Reflection;
using Newtonsoft.Json;

namespace StudyBuddyDemo
{
    public class Pet
    {
        //Datafields
        [JsonProperty]
        private long Balance;
        [JsonProperty]
        private string Hat;
        [JsonProperty]
        private string Glasses;
        [JsonProperty]
        private string Top;
        [JsonProperty]
        private string Bed;
        [JsonProperty]
        private string Table;
        [JsonProperty]
        private string Nightstand;
        [JsonProperty]
        private string Window;

        //Constructor for pet
        public Pet()
        {
            //Check the user's roaming data and check if it has PetSave file
            string userPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string petSavePath = Path.Combine(userPath, @"Study Buddy Saves\PetSave.json");

            if (File.Exists(petSavePath))
            {
                //Deserialize file into object
                string petFileString = File.ReadAllText(petSavePath);
                Pet petObject = JsonConvert.DeserializeObject<Pet>(petFileString);

                //Setup values from object into this
                this.Balance = petObject.Balance;
                this.Glasses = petObject.Glasses;
                this.Top = petObject.Top;
                this.Hat = petObject.Hat;
                this.Bed = petObject.Bed;
                this.Table = petObject.Table;  
                this.Window = petObject.Window;
                this.Nightstand = petObject.Nightstand;
            }

            else
            {
                //Setup initial values
                this.Balance = 0;
                this.Glasses = "";
                this.Top = "";
                this.Hat = "";
                this.Bed = "";
                this.Table = "";
                this.Window = "";
                this.Nightstand = "";

                //Serialize this into the save file
                string petFileString = JsonConvert.SerializeObject(this, Formatting.Indented);
                File.WriteAllText(petSavePath, petFileString);
            }
        }

        [JsonConstructor]
        private Pet(long balance, string glasses, string top, string hat, string bed, string table, string nightstand, string window)
        {
            this.Balance = balance;
            this.Glasses = glasses;
            this.Top = top;
            this.Hat = hat;
            this.Bed = bed;
            this.Table = table;
            this.Nightstand = nightstand;
            this.Window = window;
        }

        //Getters
        /// <summary>
        /// Getter for balance
        /// </summary>
        /// <returns>Balance of wallet</returns>
        public long GetBalance()
        {
            return Balance;
        }

        //Methods
        /// <summary>
        /// Update the balance of the pet
        /// </summary>
        /// <param name="coins">Number of coins to be added/removed from pet's balance</param>
        public bool UpdateFunds(long coins)
        {
            //Initialize variables
            bool success = true;

            //Check if it is a deposit or a withdraw
            if (coins > 0)
            {
                //Calculate if the deposit will overflow the balance
                long remainingUntilMax = long.MaxValue - Balance;

                //If it will not overflow, deposit the money simply
                if (remainingUntilMax > coins)
                {
                    Balance += coins;
                }

                //If it will overflow, set the wallet to max
                else
                {
                    Balance = long.MaxValue;
                }
            }

            else if(coins < 0)
            {
                //Calculate if the withdraw is too much for the balance
                if (coins < Balance)
                {
                    success = false;
                }

                //Withdraw money if possible
                else
                {
                    Balance -= coins;
                }
            }

            //Save the pet's data
            SavePetFile();

            //Return if it was successful
            return success;
        }

        /// <summary>
        /// Saves current object into the PetSave file
        /// </summary>
        private void SavePetFile()
        {
            //Get the path for the save file
            string userPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string petSavePath = Path.Combine(userPath, @"Study Buddy Saves\PetSave.json");

            //Serialize this into the save file
            string petFileString = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(petSavePath, petFileString);
        }

        public override string ToString()
        {
            string outputString = $"Balance: {Balance} Coins\n" +
                                  $"Hat: {Hat}\n" +
                                  $"Glasses: {Glasses}\n" +
                                  $"Top: {Top}\n" +
                                  $"Bed: {Bed}\n" +
                                  $"Table: {Table}\n" +
                                  $"Nightstand: {Nightstand}\n" +
                                  $"Window: {Window}";

            return outputString;
        }
    }
}
