using hlatools.core.DataObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//

namespace hlatools.core
{
    public abstract class CommandVerb : AmgDataObject
    {
        public CommandVerb()
        {

        }

        public virtual string Version { get { return "0.0.0.0"; } }

        public abstract string Verb { get; }

        public abstract string Run(IDictionary<string,string> inputKvps);

        public abstract string GetUsage();

        public abstract string GetShortDescription();

        public abstract string GetParameterHelp();

    }
}
