#region Copyright
///////////////////////////////////////////////////////////////////////////////
//
//  Copyright PHOENIX CONTACT GmbH & Co. KG
//
///////////////////////////////////////////////////////////////////////////////
#endregion

using PlcNext.Common.Tools.FileSystem;
using System;
using System.Collections.Generic;
using System.Text;
using PlcNext.Common.Tools.Events;

namespace PlcNext.Common.Build
{
    internal interface IBuildExecuter
    {
        void ExecuteBuild(BuildInformation buildInfo, ChangeObservable changeObservable);
    }
}
