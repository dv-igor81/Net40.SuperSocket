using System.Resources;
using System.Runtime.CompilerServices;
using Net40.System.IO.Ports.Resources;

namespace System;

internal static class SR
{
	private static ResourceManager s_resourceManager;

	internal static ResourceManager ResourceManager => s_resourceManager ?? (s_resourceManager = new ResourceManager(typeof(Strings)));

	internal static string IO_EOF_ReadBeyondEOF => GetResourceString("IO_EOF_ReadBeyondEOF");

	internal static string BaseStream_Invalid_Not_Open => GetResourceString("BaseStream_Invalid_Not_Open");

	internal static string PortNameEmpty_String => GetResourceString("PortNameEmpty_String");

	internal static string Port_not_open => GetResourceString("Port_not_open");

	internal static string Port_already_open => GetResourceString("Port_already_open");

	internal static string Cant_be_set_when_open => GetResourceString("Cant_be_set_when_open");

	internal static string Max_Baud => GetResourceString("Max_Baud");

	internal static string In_Break_State => GetResourceString("In_Break_State");

	internal static string Write_timed_out => GetResourceString("Write_timed_out");

	internal static string CantSetRtsWithHandshaking => GetResourceString("CantSetRtsWithHandshaking");

	internal static string NotSupportedEncoding => GetResourceString("NotSupportedEncoding");

	internal static string Arg_InvalidSerialPort => GetResourceString("Arg_InvalidSerialPort");

	internal static string Arg_InvalidSerialPortExtended => GetResourceString("Arg_InvalidSerialPortExtended");

	internal static string Argument_InvalidOffLen => GetResourceString("Argument_InvalidOffLen");

	internal static string ArgumentOutOfRange_Bounds_Lower_Upper => GetResourceString("ArgumentOutOfRange_Bounds_Lower_Upper");

	internal static string ArgumentOutOfRange_Enum => GetResourceString("ArgumentOutOfRange_Enum");

	internal static string ArgumentOutOfRange_NeedNonNegNumRequired => GetResourceString("ArgumentOutOfRange_NeedNonNegNumRequired");

	internal static string ArgumentOutOfRange_NeedPosNum => GetResourceString("ArgumentOutOfRange_NeedPosNum");

	internal static string ArgumentOutOfRange_Timeout => GetResourceString("ArgumentOutOfRange_Timeout");

	internal static string ArgumentOutOfRange_WriteTimeout => GetResourceString("ArgumentOutOfRange_WriteTimeout");

	internal static string IndexOutOfRange_IORaceCondition => GetResourceString("IndexOutOfRange_IORaceCondition");

	internal static string IO_OperationAborted => GetResourceString("IO_OperationAborted");

	internal static string NotSupported_UnseekableStream => GetResourceString("NotSupported_UnseekableStream");

	internal static string ObjectDisposed_StreamClosed => GetResourceString("ObjectDisposed_StreamClosed");

	internal static string InvalidNullEmptyArgument => GetResourceString("InvalidNullEmptyArgument");

	internal static string Arg_WrongAsyncResult => GetResourceString("Arg_WrongAsyncResult");

	internal static string InvalidOperation_EndReadCalledMultiple => GetResourceString("InvalidOperation_EndReadCalledMultiple");

	internal static string InvalidOperation_EndWriteCalledMultiple => GetResourceString("InvalidOperation_EndWriteCalledMultiple");

	internal static string IO_PortNotFound => GetResourceString("IO_PortNotFound");

	internal static string IO_PortNotFoundFileName => GetResourceString("IO_PortNotFoundFileName");

	internal static string UnauthorizedAccess_IODenied_NoPortName => GetResourceString("UnauthorizedAccess_IODenied_NoPortName");

	internal static string IO_PathTooLong_PortName => GetResourceString("IO_PathTooLong_PortName");

	internal static string IO_SharingViolation_NoPortName => GetResourceString("IO_SharingViolation_NoPortName");

	internal static string IO_SharingViolation_Port => GetResourceString("IO_SharingViolation_Port");

	internal static string UnauthorizedAccess_IODenied_Port => GetResourceString("UnauthorizedAccess_IODenied_Port");

	internal static string PlatformNotSupported_IOPorts => GetResourceString("PlatformNotSupported_IOPorts");

	internal static string PlatformNotSupported_SerialPort_GetPortNames => GetResourceString("PlatformNotSupported_SerialPort_GetPortNames");

	internal static string IO_PathTooLong_Path_PortName => GetResourceString("IO_PathTooLong_Path_PortName");

	internal static string IO_PathTooLong => GetResourceString("IO_PathTooLong");

	internal static string IO_PathNotFound_NoPathName => GetResourceString("IO_PathNotFound_NoPathName");

	internal static string IO_PathNotFound_Path => GetResourceString("IO_PathNotFound_Path");

	internal static string IO_FileNotFound => GetResourceString("IO_FileNotFound");

	internal static string IO_FileNotFound_FileName => GetResourceString("IO_FileNotFound_FileName");

	internal static string UnauthorizedAccess_IODenied_NoPathName => GetResourceString("UnauthorizedAccess_IODenied_NoPathName");

	internal static string UnauthorizedAccess_IODenied_Path => GetResourceString("UnauthorizedAccess_IODenied_Path");

	internal static string IO_PathTooLong_Path => GetResourceString("IO_PathTooLong_Path");

	internal static string IO_SharingViolation_File => GetResourceString("IO_SharingViolation_File");

	internal static string IO_SharingViolation_NoFileName => GetResourceString("IO_SharingViolation_NoFileName");

	internal static string ArgumentOutOfRange_FileLengthTooBig => GetResourceString("ArgumentOutOfRange_FileLengthTooBig");

	internal static string IO_FileExists_Name => GetResourceString("IO_FileExists_Name");

	internal static string IO_AlreadyExists_Name => GetResourceString("IO_AlreadyExists_Name");

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static bool UsingResourceKeys()
	{
		return false;
	}

	internal static string GetResourceString(string resourceKey, string defaultString = null)
	{
		if (UsingResourceKeys())
		{
			return defaultString ?? resourceKey;
		}
		string text = null;
		try
		{
			text = ResourceManager.GetString(resourceKey);
		}
		catch (MissingManifestResourceException)
		{
		}
		if (defaultString != null && resourceKey.Equals(text))
		{
			return defaultString;
		}
		return text;
	}

	internal static string Format(string resourceFormat, object p1)
	{
		if (UsingResourceKeys())
		{
			return string.Join(", ", resourceFormat, p1);
		}
		return string.Format(resourceFormat, p1);
	}

	internal static string Format(string resourceFormat, object p1, object p2)
	{
		if (UsingResourceKeys())
		{
			return string.Join(", ", resourceFormat, p1, p2);
		}
		return string.Format(resourceFormat, p1, p2);
	}

	internal static string Format(string resourceFormat, object p1, object p2, object p3)
	{
		if (UsingResourceKeys())
		{
			return string.Join(", ", resourceFormat, p1, p2, p3);
		}
		return string.Format(resourceFormat, p1, p2, p3);
	}

	internal static string Format(string resourceFormat, params object[] args)
	{
		if (args != null)
		{
			if (UsingResourceKeys())
			{
				return resourceFormat + ", " + string.Join(", ", args);
			}
			return string.Format(resourceFormat, args);
		}
		return resourceFormat;
	}

	internal static string Format(IFormatProvider provider, string resourceFormat, object p1)
	{
		if (UsingResourceKeys())
		{
			return string.Join(", ", resourceFormat, p1);
		}
		return string.Format(provider, resourceFormat, p1);
	}

	internal static string Format(IFormatProvider provider, string resourceFormat, object p1, object p2)
	{
		if (UsingResourceKeys())
		{
			return string.Join(", ", resourceFormat, p1, p2);
		}
		return string.Format(provider, resourceFormat, p1, p2);
	}

	internal static string Format(IFormatProvider provider, string resourceFormat, object p1, object p2, object p3)
	{
		if (UsingResourceKeys())
		{
			return string.Join(", ", resourceFormat, p1, p2, p3);
		}
		return string.Format(provider, resourceFormat, p1, p2, p3);
	}

	internal static string Format(IFormatProvider provider, string resourceFormat, params object[] args)
	{
		if (args != null)
		{
			if (UsingResourceKeys())
			{
				return resourceFormat + ", " + string.Join(", ", args);
			}
			return string.Format(provider, resourceFormat, args);
		}
		return resourceFormat;
	}
}
