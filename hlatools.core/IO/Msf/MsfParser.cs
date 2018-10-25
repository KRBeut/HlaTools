using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;


namespace hlatools.core.IO
{
    public class MsfParser
    {

        public MsfParser()
        {

        }

        public static Dictionary<string, string> Parse(string imgtGenFilepath)
        {
            Dictionary<string, string> alignments = null;
            using (var genStrmRdr = File.OpenRead(imgtGenFilepath))
            using (var strmRdr = new StreamReader(genStrmRdr))
            {
                var prsr = new MsfParser();
                alignments = prsr.Parser(strmRdr);
            }
            return alignments;
        }

        public Dictionary<string, string> Parser(TextReader txtRdr)
        {
            ParserHeader(txtRdr);
            var alignments = ParseAlignments(txtRdr);
            return alignments;
        }

        protected static readonly char[] splitChars = new char[] { ' ' };

        protected Dictionary<string, string> ParseAlignments(TextReader txtRdr)
        {
            var alignments = new Dictionary<string, string>();
            string fileLine;
            while ((fileLine = txtRdr.ReadLine()) != null)
            {
                var lineToks = fileLine.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);
                if (lineToks.Length < 1)
                {
                    continue;
                }
                var allele = lineToks[0];
                var seq = string.Join("",lineToks.Skip(1));
                if (!alignments.ContainsKey(allele))
                {
                    alignments.Add(allele, seq);
                }
                else
                {
                    alignments[allele] += seq;
                }
            }
            return alignments;
        }

        protected void ParserHeader(TextReader txtRdr)
        {
            string headerLine;
            while ((headerLine = txtRdr.ReadLine()) != null && !headerLine.StartsWith("//"))
            {

            }
            txtRdr.ReadLine();//burn the line between the header and the alignments
        }

    }
}
