using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace TestContent.Utility
{
    public class ModUtils
    {
        public static string GetNamespaceFileLocation(Type type, bool dropModName = false)
        {
            var nameSpace = type.Namespace.ToString();

            if (nameSpace.Equals(""))
            {
                return "";
            }

            nameSpace = nameSpace.Replace('.', '/');

            if (dropModName)
            {
                int firstInd = nameSpace.IndexOf('/');
                nameSpace = nameSpace.Substring(firstInd + 1);
            }
            return nameSpace;
        }

        public static string GetSoundFileLocation(string soundName)
        {
            return "TestContent/Assets/Sounds/" + soundName;
        }


    }
}
