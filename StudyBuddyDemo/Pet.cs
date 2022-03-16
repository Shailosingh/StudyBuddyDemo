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
        private ulong Balance;
        [JsonProperty]
        private string Hat;
        [JsonProperty]
        private string Glasses;
        [JsonProperty]
        private string Top;
        [JsonProperty]
        private string Furniture;

        //Constructor for pet
        public Pet()
        {
            //Check the Saves folder and check if a save for the pet is already there
            string currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string petSavePath = Path.Combine(currentDirectory, @"Saves\PetSave.json");

            if(File.Exists(petSavePath))
            {
                //Deserialize file into object
                string petFileString = File.ReadAllText(petSavePath);
                Pet petObject = JsonConvert.DeserializeObject<Pet>(petFileString);

                //Setup values from object into this
                this.Balance = petObject.Balance;
                this.Glasses = petObject.Glasses;
                this.Top = petObject.Top;
                this.Hat = petObject.Hat;
                this.Furniture = petObject.Furniture;
            }

            else
            {
                //Setup initial values
                this.Balance = 0;
                this.Glasses = "";
                this.Top = "";
                this.Hat = "";
                this.Furniture = "";

                //Serialize this into a save file
                string petFileString = JsonConvert.SerializeObject(this, Formatting.Indented);
                File.WriteAllText(petSavePath, petFileString);
            }
        }

        //Getters
        /// <summary>
        /// Getter for balance
        /// </summary>
        /// <returns>Balance of wallet</returns>
        public ulong GetBalance()
        {
            return Balance;
        }

        //Methods
        /// <summary>
        /// Adds however deposit number of coins to pet
        /// </summary>
        /// <param name="deposit">Number of coins to be added to pet</param>
        public void AddFunds(ulong deposit)
        {
            //Calculate if the deposit will overflow the balance
            ulong remainingUntilMax = ulong.MaxValue - Balance;

            //If it will not overflow, deposit the money simply
            if (remainingUntilMax > deposit)
            {
                Balance += deposit;
            }

            //If it will overflow, set the wallet to max
            else
            {
                Balance = ulong.MaxValue;
            }
        }

        /// <summary>
        /// Withdraws specified number of coins from pet
        /// </summary>
        /// <param name="withdraw">Number of coins to be removed</param>
        /// <returns>If the withdraw was successful</returns>
        public bool SubractFunds(ulong withdraw)
        {
            //Calculate if the withdraw is too much for the balance
            if (withdraw < Balance)
            {
                return false;
            }

            //Withdraw money if possible
            else
            {
                Balance -= withdraw;
                return true;
            }
        }

        public override string ToString()
        {
            string outputString = $"Balance: {Balance} Coins\n" +
                                  $"Hat: {Hat}\n" +
                                  $"Glasses: {Glasses}\n" +
                                  $"Top: {Top}\n" +
                                  $"Furniture: {Furniture}";

            return outputString;
        }
    }
}
