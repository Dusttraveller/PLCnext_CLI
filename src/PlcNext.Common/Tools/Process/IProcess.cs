#region Copyright
///////////////////////////////////////////////////////////////////////////////
//
//  Copyright PHOENIX CONTACT GmbH & Co. KG
//
///////////////////////////////////////////////////////////////////////////////
#endregion

using System;
using System.Threading.Tasks;

namespace PlcNext.Common.Tools.Process
{
    internal interface IProcess : IDisposable
    {
        Task WaitForExitAsync();
        void WaitForExit();
        int ExitCode { get; }
    }
}
