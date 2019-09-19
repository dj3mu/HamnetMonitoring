using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using HamnetDbRest;

namespace RestService.DataFetchingService
{
    /// <summary>
    /// Class to parse the file containing networks to exclude.
    /// </summary>
    public class NetworkExcludeFile
    {
        private static readonly log4net.ILog log = Program.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Backing field for the llist of networks that have been parsed.
        /// </summary>
        private List<IPNetwork> parsedNetworksBacking = null;
        
        /// <summary>
        /// Counter for the current line (and at the end of parsing the number of lines in file).
        /// </summary>
        private int currentLine;

        /// <summary>
        /// Constructs the handler for a specific file.
        /// </summary>
        /// <param name="fileToParse">The file to parse.</param>
        public NetworkExcludeFile(string fileToParse)
        {
            this.FileToParse = fileToParse;
        }

        /// <summary>
        /// Gets the file to parse.
        /// </summary>
        public string FileToParse { get; }

        /// <summary>
        /// Gets the list of networks as parsed from the file.
        /// </summary>
        public IEnumerable<IPNetwork> ParsedNetworks
        {
            get
            {
                if (this.parsedNetworksBacking == null)
                {
                    this.ParseFile();
                }

                return this.parsedNetworksBacking;
            }
        }

        /// <summary>
        /// Forces the immediate parsing of the file.
        /// </summary>
        public void Parse()
        {
            if (this.parsedNetworksBacking == null)
            {
                this.ParseFile();
            }
        }

        /// <summary>
        /// Actually parsed the file.
        /// </summary>
        private void ParseFile()
        {
            this.parsedNetworksBacking = new List<IPNetwork>();

            if (!File.Exists(this.FileToParse))
            {
                throw new FileNotFoundException($"Cannot find monitoring exclude file '{this.FileToParse}'", this.FileToParse);
            }

            var lines = File.ReadAllLines(this.FileToParse);
             
            foreach (string line in lines)
            {
                this.currentLine++;
                this.ParseLine(line);
            }
        }

        /// <summary>
        /// Parse a single line.
        /// </summary>
        /// <param name="line">The line to parse.</param>
        private void ParseLine(string line)
        {
            string trimmedLine = line.Trim();
            if ((trimmedLine.Length == 0) || trimmedLine.StartsWith('#'))
            {
                // empty or comment line
                return;
            }

            var splitLine = trimmedLine.Split(new [] { ' ', '#' }, StringSplitOptions.RemoveEmptyEntries);
            if (splitLine.Length == 0)
            {
                // line somehow doesn't split at all (this normally shouldn't happen)
                return;
            }

            IPNetwork parsedNetwork = null;
            if (!IPNetwork.TryParse(splitLine[0], out parsedNetwork))
            {
                log.Warn($"'{this.FileToParse}' line #{this.currentLine}: Is not a comment or empty but cannot be parsed as valid IP network. It will be ignored. Line content '{line}'");
            }

            this.parsedNetworksBacking.Add(parsedNetwork);
        }
    }
}