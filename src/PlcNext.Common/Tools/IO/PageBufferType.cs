#region Copyright
///////////////////////////////////////////////////////////////////////////////
//
//  Copyright PHOENIX CONTACT GmbH & Co. KG
//
///////////////////////////////////////////////////////////////////////////////
#endregion

namespace PlcNext.Common.Tools.IO
{
    /// <summary>
    /// The types of <see cref="PageBuffer"/>.
    /// </summary>
    public enum PageBufferType
    {
        /// <summary>
        /// Pages are managed in memory.
        /// </summary>
        Memory,

        /// <summary>
        /// Pages are buffered inside of a file.
        /// </summary>
        File
    }
}
