using System.ComponentModel;

namespace System.IO.Ports.Net40;

internal static class InternalResources
{
	internal static void EndOfFile()
	{
		throw new EndOfStreamException(System.SR.IO_EOF_ReadBeyondEOF);
	}

	internal static string GetMessage(int errorCode)
	{
		return new Win32Exception(errorCode).Message;
	}

	internal static Exception FileNotOpenException()
	{
		return new ObjectDisposedException(null, System.SR.Port_not_open);
	}

	internal static void FileNotOpen()
	{
		throw FileNotOpenException();
	}

	internal static void WrongAsyncResult()
	{
		throw new ArgumentException(System.SR.Arg_WrongAsyncResult);
	}

	internal static void EndReadCalledTwice()
	{
		throw new ArgumentException(System.SR.InvalidOperation_EndReadCalledMultiple);
	}

	internal static void EndWriteCalledTwice()
	{
		throw new ArgumentException(System.SR.InvalidOperation_EndWriteCalledMultiple);
	}
}
