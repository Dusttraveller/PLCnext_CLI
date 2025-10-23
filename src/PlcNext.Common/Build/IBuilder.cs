#region Copyright
///////////////////////////////////////////////////////////////////////////////
//
//  Copyright PHOENIX CONTACT GmbH & Co. KG
//
///////////////////////////////////////////////////////////////////////////////
#endregion

using System;
using System.Collections.Generic;
using PlcNext.Common.Tools.Events;

namespace PlcNext.Common.Build
{
    internal interface IBuilder
    {
        void Build(BuildInformation buildInfo, ChangeObservable observable, IEnumerable<string> targets);
    }
}
