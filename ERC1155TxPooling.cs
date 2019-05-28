using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Web3;
using Newtonsoft.Json;
using Nethereum.RPC.Eth.DTOs;

namespace ERC1155TxPooling
{
    class Program
    {
        static void Main(string[] args)
        {
            MainAsync(args).Wait();
        }

        static async Task MainAsync(string[] args)
        {
            //var url = "https://kovan.infura.io/v3/...";

            var url = "https://mainnet.infura.io/v3/...";
            var web3 = new Web3(url);


            //kovan
            //string tokenID = "0x78000000000002f2000000000000000000000000000000000000000000000000";//rustbits

            //main
            string tokenID = "780000000000022e000000000000000000000000000000000000000000000000";//rustbits

            //kovan
            //string fromAddress = "0x...";

            //main
            string fromAddress = "0x...";


            /* if (toAddress == "")
            {
                toAddress = null;
            }*/

            //kovan
            //string toAddress = "0x...";

            //main
            string toAddress = "0x...";


            var dataArray = await EventsAsync(web3, tokenID, fromAddress, toAddress);

            //Using Newtonsoft JSON Convert, turns the Dynamic Array into JSON and Prints formatted JSON
           
            printJSON(dataArray);

            //Loop through array and print each in a structured format


        }

        private static void printData(dynamic dataArray)
        {
            Console.WriteLine(dataArray.Count());
            foreach (var item in dataArray)
            {
                Console.WriteLine("Tx: " + item.Tx);
                Console.WriteLine("Block: " + item.Block);
                Console.WriteLine("From: " + item.From);
                Console.WriteLine("To: " + item.To);
                Console.WriteLine("TokenID: " + item.TokenID);
                Console.WriteLine("Amount: " + item.Amount);
                Console.WriteLine();
            }
        }

        private static void printJSON(dynamic dataArray)
        {
            string json = JsonConvert.SerializeObject(dataArray, Formatting.Indented);
            Console.WriteLine(json);
        }

        public static async Task<dynamic> EventsAsync(Web3 web3, string tokenID, string from = null, string to = null)
        {
            var Data = new List<dynamic>();
            //kovan
            //var contractAddress = "0x8819a653b6c257b1e3356d902ecb1961d6471dd8";

            //mainnet
            var contractAddress = "0x8562c38485b1e8ccd82e44f89823da76c98eb0ab";

            var transferEventHandler = web3.Eth.GetEvent<TransferEventDTO>(contractAddress);

            //First three Params are the Event Topic to filter on. All Topics are optional
            //TokenID, From, To, StartBlock, EndBlock

            //If Startblock is null, filter starts with the begining of the contract
            var filterTransferEventsForContractFromAddress = transferEventHandler.CreateFilterInput(tokenID, from, to);

            //***Alternativly Start Block Number can be set using this***
            //BlockParameter startBlock = new BlockParameter(10697908);
            //var filterTransferEventsForContractFromAddress = transferEventHandler.CreateFilterInput(tokenID, from, to, startBlock);


            try
            {
                //Getting all changes from the Event Log based on filters
                var eventData = await transferEventHandler.GetAllChanges(filterTransferEventsForContractFromAddress);

                for (int i = 0; i < eventData.Count(); i++)
                {
                    dynamic obj = new ExpandoObject();
                    obj.Tx = eventData[i].Log.TransactionHash;
                    obj.Block = eventData[i].Log.BlockNumber.Value;
                    obj.To = eventData[i].Event.To;
                    obj.From = eventData[i].Event.From;
                    obj.TokenID = eventData[i].Log.Topics[1];
                    obj.Amount = eventData[i].Event.Value;
                    Data.Add(obj);
                }
                Console.WriteLine(eventData.Count());
            }
            catch (Exception e)
            {
                Console.WriteLine(e);

            }

            return Data;
        }

        //Define Transfer Event topic indexes and position
        [Event("Transfer")]
        public class TransferEventDTO : IEventDTO
        {
            //Topic[1] TokenID - Indexed 
            [Parameter("uint256", "_id", 1, true)]
            public BigInteger Id { get; set; }

            //Topic[2] From - Indexed
            [Parameter("address", "_from", 2, true)]
            public string From { get; set; }

            //Topic[3] To - Indexed
            [Parameter("address", "_to", 3, true)]
            public string To { get; set; }

            //Topic[4] - Data Not Indexed - This is the Amount Transfered.
            [Parameter("uint256", "_value", 4, false)]
            public BigInteger Value { get; set; }
        }

    }
}

