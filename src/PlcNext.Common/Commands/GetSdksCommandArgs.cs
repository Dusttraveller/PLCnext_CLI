#region Copyright
///////////////////////////////////////////////////////////////////////////////
//
//  Copyright PHOENIX CONTACT GmbH & Co. KG
//
///////////////////////////////////////////////////////////////////////////////
#endregion

using System;
using System.Collections.Generic;
using System.Text;

namespace PlcNext.Common.Commands
{
    public class GetSdksCommandArgs : CommandArgs
    {
        public GetSdksCommandArgs(string path, bool all)
        {
            Path = path;
            All = all;
        }

        public string Path { get; }
        public bool All { get; }
    }
}
