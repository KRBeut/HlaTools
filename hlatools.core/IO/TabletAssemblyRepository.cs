using hlatools.core.DataObjects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using System.Xml;
using System.Xml.Serialization;

namespace hlatools.core.IO
{
    public class TabletAssemblyRepository
    {

        public TabletAssemblyRepository()
        {

        }

        public void SaveAssembly(TextWriter txtWrtr, TabletAssembly tbltAssmbly)
        {
            txtWrtr.WriteLine("<tablet>");

            if (!string.IsNullOrWhiteSpace(tbltAssmbly.reference))
            {
                txtWrtr.WriteLine("\t<reference>{0}</reference>", tbltAssmbly.reference);
            }

            if (!string.IsNullOrWhiteSpace(tbltAssmbly.asmbly))
            {
                txtWrtr.WriteLine("\t<assembly>{0}</assembly>", tbltAssmbly.asmbly);
            }

            if (tbltAssmbly.annotation != null)
            {
                foreach (var anno in tbltAssmbly.annotation)
                {
                    if (!string.IsNullOrWhiteSpace(anno))
                    {
                        txtWrtr.WriteLine("\t<annotation>{0}</annotation>", anno);
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(tbltAssmbly.contig))
            {
                txtWrtr.WriteLine("\t<contig>{0}</contig>", tbltAssmbly.contig);
                if (tbltAssmbly.position > 0)
                {
                    txtWrtr.WriteLine("\t<position>{0}</position>", tbltAssmbly.position);
                }
            }
            txtWrtr.WriteLine("</tablet>");
        }

        //public static void SerializeAssembly(string filepath, TabletAssembly tbltAssmbly)
        //{
        //    string xml;
        //    var xsSubmit = new XmlSerializer(typeof(TabletAssembly));
        //    using (var sww = new StringWriter())
        //    {
        //        using (var writer = XmlWriter.Create(sww))
        //        {
        //            xsSubmit.Serialize(writer, tbltAssmbly);
        //            xml = sww.ToString(); // Your XML
        //        }
        //    }
        //}

        public static void SaveAssembly(string filepath, TabletAssembly tbltAssmbly)
        {
            using (var strm = File.Open(filepath, FileMode.Create, FileAccess.Write, FileShare.Read))
            using (var strmWrtr = new StreamWriter(strm))
            {
                var assmblyRepo = new TabletAssemblyRepository();
                assmblyRepo.SaveAssembly(strmWrtr, tbltAssmbly);
            }
        }

        protected static readonly Regex regEx = new Regex("<(?<tag>.*)>(?<value>.*)<");

        public static TabletAssembly GetAssembly(TextReader txtrdr)
        {
            string fileLine;
            var tbltAssmbly = new TabletAssembly();
            while ((fileLine = txtrdr.ReadLine()) != null && !fileLine.StartsWith("</tablet"))
            {
                var mtch = regEx.Match(fileLine);
                if (mtch.Success)
                {
                    var tag = mtch.Groups["tag"].Value;
                    var val = mtch.Groups["value"].Value;
                    if (tag == "reference")
                    {
                        tbltAssmbly.reference = val;
                    }
                    else if (tag == "assembly")
                    {
                        tbltAssmbly.asmbly = val;
                    }
                    else if (tag == "annotation")
                    {
                        tbltAssmbly.annotation.Add(val);
                    }
                    else if (tag == "contig")
                    {
                        tbltAssmbly.contig = val;
                    }
                    else if (tag == "position")
                    {
                        int pos;
                        tbltAssmbly.position = int.TryParse(val, out pos) ? pos : 0;
                    }
                }
            }
            return tbltAssmbly;
        }

        public static TabletAssembly GetAssembly(string filepath)
        {
            TabletAssembly tblt = null;
            using (var strm = File.Open(filepath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var strmWrtr = new StreamReader(strm))
            {
                tblt = GetAssembly(strmWrtr);
            }
            return tblt;
        }

    }
}
