#region Copyright
///////////////////////////////////////////////////////////////////////////////
//
//  Copyright (c) Phoenix Contact GmbH & Co KG
//  This software is licensed under Apache-2.0
//
///////////////////////////////////////////////////////////////////////////////
#endregion

using PlcNext.Common.Tools;
using System.Globalization;

namespace PlcNext.Common.Project
{
    internal class DeployArgumentsException : FormattableException
    {
        public DeployArgumentsException(string arg1, string arg2) 
            : base(string.Format(CultureInfo.InvariantCulture, ExceptionTexts.DeployArgumentsWrongCombined, arg1, arg2))
        {

        }
    }
}
