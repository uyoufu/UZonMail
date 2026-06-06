using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UZonMail.Utils.Config
{
    public interface IConfig
    {
        public abstract T GetValue<T>(string path);
        public abstract string GetStringValue(string path);
    }
}
