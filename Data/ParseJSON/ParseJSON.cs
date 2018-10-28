﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ParseJSON
{
    public class ParseJSON
    {

        public void ParseJSONs(ParseDYF dyfObj)
        {
            // DEFINE GLOBALS
            GraphDataParse csvParser = new GraphDataParse();

            Console.WriteLine("Enter Directory Path"); //Prompt user to enter directory to search for PDFS
            string dirPath = Console.ReadLine(); //Read user input
            string[] fileEntries = Directory.GetFiles(dirPath); //Get all the files of the input directory

            foreach (var filepath in fileEntries)
            {
                Console.WriteLine(filepath);

                using (StreamReader reader = File.OpenText(filepath))
                {
                    // Read JSON file, and get a JToken from it for iterating
                    try
                    {
                        JObject jObject = (JObject)JToken.ReadFrom(new JsonTextReader(reader));
                        //Define the Nodes into a JToken
                        JToken nodeObject = jObject["Nodes"];
                        JToken connectorObject = jObject["Connectors"];

                        //Define Dictionaries for the node and the In / Out
                        Dictionary<string, string> NodeDictionary = new Dictionary<string, string>();
                        Dictionary<string, string> IODictionary = new Dictionary<string, string>();

                        //int countfound;
                        //int countnotfound;

                        //Console.Write(nodeObject);

                        //Create dictionary for ID and node name
                        foreach (var node in nodeObject)
                        {
                            //Console.WriteLine(test);
                            //Console.WriteLine(node["FunctionSignature"]);
                            string stringnodeID = node["Id"].ToString();

                            string stringnodename = "NONE";

                            try
                            {
                                stringnodename = node["FunctionSignature"].ToString();
                                Console.WriteLine(stringnodename);
                            }
                            catch
                            {
                                Console.WriteLine("MISSING FUNCTION SIGNATURE");
                            }

                            if (stringnodename != "NONE")
                            {
                                NodeDictionary.Add(stringnodeID, stringnodename);


                                JToken outputObject = node["Outputs"];

                                foreach (var output in outputObject)
                                {
                                    string outputID = output["Id"].ToString();
                                    IODictionary.Add(outputID, stringnodeID);
                                }

                                JToken inputObject = node["Inputs"];

                                foreach (var inputs in inputObject)
                                {
                                    string inputID = inputs["Id"].ToString();
                                    IODictionary.Add(inputID, stringnodeID);
                                }

                                //Console.WriteLine(IODictionary);
                            }

                        }


                        foreach (var connector in connectorObject)
                        {
                            string inputID = connector["Start"].ToString();
                            string outputID = connector["End"].ToString();

                            try
                            {
                                string NodeAID = IODictionary[inputID];
                                string NodeBID = IODictionary[outputID];

                                string NodeASig = NodeDictionary[NodeAID];
                                string NodeBSig = NodeDictionary[NodeBID];


                                if (dyfObj.customPackageMappings.Keys.Contains(NodeASig))
                                {
                                    var result = dyfObj.customPackageMappings[NodeASig];
                                    NodeASig = result;
                                }

                                if (dyfObj.customPackageMappings.Keys.Contains(NodeBSig))
                                {
                                    var result = dyfObj.customPackageMappings[NodeBSig];
                                    NodeBSig = result;
                                }

                                Console.WriteLine(NodeAID);
                                Console.WriteLine(NodeBID);
                                Console.WriteLine(NodeASig);
                                Console.WriteLine(NodeBSig);

                                csvParser.AppendToCsv(NodeASig, NodeBSig, NodeAID, NodeBID);
                            }
                            catch (Exception)
                            {

                            }
                        }
                    }
                    catch
                    {
                        Console.WriteLine("NOT JSON");
                    }

                    csvParser.ExportCSV();
                    Console.WriteLine("EXPORT COMPLETE");
                    //Console.Read();
                }

            }
        }
    }
}