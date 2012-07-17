using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using HtmlAgilityPack;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;


namespace JavaAPIDocApp
{
    class Program
    {
        public static int numAbtractClasses = 0;
        public static int numConcreteClasses = 0;
        public static int numInterfaceClasses = 0;
        public static int numOtherClasses = 0;

        public static Dictionary<string, int> abstractWords = new Dictionary<string, int>();
        public static Dictionary<string, int> concreteWords = new Dictionary<string, int>();


        /// <UsefulText>
        /// Take a file path as a parameter, read the file in a string and cut the definitaion part
        /// of class, at last return the result.
        /// </UsefulText>

        public static  string UsefulText(string fileName)
        {
            string  pageData = File.ReadAllText(fileName);

            int startIndex = pageData.IndexOf("<!-- ======== START OF CLASS DATA ======== -->");
            if (startIndex == -1)
                return null;
            startIndex = pageData.IndexOf("<HR>", startIndex+1);
            int endIndex = pageData.IndexOf("<HR>", startIndex + 4);

            string usefulData = pageData.Substring(startIndex, endIndex - startIndex);
            return usefulData;
        }

        /// <TextExtractor>
        /// Receive a string as a parameter, using HtmlAgilityPack to strip the tags and superfluous info
        /// at last return pure text. 
        /// </TextExtractor>

        public static string TextExtractor(string usefulData)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(usefulData);
            List<HtmlNode> nodeList = new List<HtmlNode>();
            int nodeListIndex = 0;

            foreach (HtmlNode node in doc.DocumentNode.ChildNodes)
                nodeList.Add(node);

            string pureData = "";
            while (nodeListIndex < nodeList.Count)
            {
                HtmlNode tempNode = nodeList[nodeListIndex++];
                if (tempNode.ChildNodes.Count == 0 && tempNode.InnerText != "")
                {
                    pureData += tempNode.InnerText;
                    pureData += "\n";
                }

                foreach (HtmlNode node in tempNode.ChildNodes)
                    nodeList.Add(node);
            }
            return pureData;
        }

        /// <CheckClassType>
        /// Type of each class was mentioned in the description part. 
        /// </CheckClassType>

        public static string CheckClassType(string usefulData)
        {
            int startIndex = usefulData.IndexOf("<PRE>") + 5;
            int endIndex = usefulData.IndexOf("<B>");
            string classText = usefulData.Substring(startIndex, endIndex);
            string[] words = classText.Split(' ');
            bool swClass = false;
            bool swAbstract = false;
            bool swInterface = false;
            foreach (string word in words)
            {
                if (word == "class")
                    swClass = true;
                else
                    if (word == "abstract")
                        swAbstract = true; 
                    else
                        if (word == "interface")
                            swInterface = true;
            }
            if (swInterface)
            {
                numInterfaceClasses++;
                return "interface";
            }
            if (swClass)
            {
                if (swAbstract)
                {
                    numAbtractClasses++;
                    return "abstract";
                }

                numConcreteClasses++;
                return "concrete";
            }           
            numOtherClasses++;
            return "other";
        }

        /// <TextSpliter>
        /// Receive the pure text and tokenize the text to words,
        /// store words in related dictionary structure based on class type. 
        /// </TextSpliter>

        public static void TextSpliter (string pureText , string classType)
        {
            Tokeniser tokenizer = new Tokeniser();
            string[] words = tokenizer.Partition(pureText);

            Dictionary<string,int> wordsDictionary = new Dictionary<string, int>();
            foreach (string word in words)
            {
                if(classType == "concrete")
                {
                    if (concreteWords.ContainsKey(word))
                    {
                        concreteWords[word]++;
                    }
                    else
                    {
                        concreteWords.Add(word, 1);
                    }   
                }
                else if (classType == "abstract")
                {
                    if (abstractWords.ContainsKey(word))
                    {
                        abstractWords[word]++;
                    }
                    else
                    {
                        abstractWords.Add(word, 1);
                    }
                }
                else if (classType == "interface")
                {
                    if (abstractWords.ContainsKey(word))
                    {
                        abstractWords[word]++;
                    }
                    else
                    {
                        abstractWords.Add(word, 1);
                    }
                }
            }
        }
        
        public static void Main(string[] args)
        {
            string originPath = new DirectoryInfo(Application.StartupPath).ToString();
            int lengh = originPath.IndexOf("JavaAPIDocMining");
            string mainPath = originPath.Substring(0,lengh);

            string fileDirectoty = mainPath + @"JavaAPIDocMining\JavaAPIDocuments\api\";
            string[] filePaths = Directory.GetFiles(fileDirectoty, "*.html", SearchOption.AllDirectories);
            StreamWriter OutFile = new StreamWriter(mainPath + @"JavaAPIDocMining\result.txt");
            int notRecognisedFiles = 0;
            int indexFile = 0;

            foreach (string filePath in filePaths)
            {

                string usefulData = UsefulText(filePath);
                
                if(usefulData == null)
                {
                    notRecognisedFiles++;
                    continue;
                }

                string pureText = TextExtractor(usefulData);

                string classType = CheckClassType(usefulData);

                if(classType == "abstract" ||classType == "interface" || classType == "concrete")
                    TextSpliter(pureText,classType);



                Console.Write(++indexFile);
                Console.WriteLine("- " + filePath.Substring(fileDirectoty.Length));
                
            }

            OutFile.WriteLine("*******************************************************************");
            OutFile.WriteLine("***  Proccesing of API documentation of Java 1.6 SE");
            OutFile.WriteLine("*******************************************************************\n");

            OutFile.WriteLine("Number of Files:" + filePaths.Count());
            OutFile.WriteLine("Abstract Classes:" + numAbtractClasses);
            OutFile.WriteLine("Interfaces:" + numInterfaceClasses);
            OutFile.WriteLine("Abstract Classes + Interfaces:" + (numAbtractClasses + numInterfaceClasses));
            OutFile.WriteLine("Concrete Classes:" + numConcreteClasses);
            OutFile.WriteLine("Other Classes:" + numOtherClasses);
            OutFile.WriteLine("Not Recognised Files:" + notRecognisedFiles);

            OutFile.WriteLine("\n*******************************************************************");
            OutFile.WriteLine("***  List of Abstract Classes and Interface Words");
            OutFile.WriteLine("*******************************************************************\n");


            var abstractItems = from pair in abstractWords
                        orderby pair.Value descending
                        select pair;
            int i = 1;
            foreach (KeyValuePair<string, int> pair in abstractItems)
            {
                OutFile.WriteLine("{0}- {1} {2}", i++, pair.Key, pair.Value);
            }
            
            OutFile.WriteLine("\n*******************************************************************");
            OutFile.WriteLine("***  List of Concrete Words");
            OutFile.WriteLine("*******************************************************************\n");

            var concreteItems = from pair in concreteWords
                                orderby pair.Value descending
                                select pair;
            i = 1;
            foreach (KeyValuePair<string, int> pair in concreteItems)
            {
                OutFile.WriteLine("{0}- {1} {2}", i++, pair.Key, pair.Value);
            }

            Console.WriteLine("\n END ");
            Console.WriteLine(" NaP :)");
            OutFile.Close();
            Console.Read();


        }
    }
}
