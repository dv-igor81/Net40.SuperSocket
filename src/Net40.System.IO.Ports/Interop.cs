using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Win32.SafeHandles;

internal static class Interop
{
	internal class Kernel32
	{
		internal static class DCBFlags
		{
			internal const int FBINARY = 0;

			internal const int FPARITY = 1;

			internal const int FOUTXCTSFLOW = 2;

			internal const int FOUTXDSRFLOW = 3;

			internal const int FDTRCONTROL = 4;

			internal const int FDSRSENSITIVITY = 6;

			internal const int FOUTX = 8;

			internal const int FINX = 9;

			internal const int FERRORCHAR = 10;

			internal const int FNULL = 11;

			internal const int FRTSCONTROL = 12;

			internal const int FABORTONOERROR = 14;

			internal const int FDUMMY2 = 15;
		}

		internal static class DCBDTRFlowControl
		{
			internal const int DTR_CONTROL_DISABLE = 0;

			internal const int DTR_CONTROL_ENABLE = 1;
		}

		internal static class DCBRTSFlowControl
		{
			internal const int RTS_CONTROL_DISABLE = 0;

			internal const int RTS_CONTROL_ENABLE = 1;

			internal const int RTS_CONTROL_HANDSHAKE = 2;

			internal const int RTS_CONTROL_TOGGLE = 3;
		}

		internal static class DCBStopBits
		{
			internal const byte ONESTOPBIT = 0;

			internal const byte ONE5STOPBITS = 1;

			internal const byte TWOSTOPBITS = 2;
		}

		internal struct DCB
		{
			internal const byte EOFCHAR = 26;

			internal const byte DEFAULTXONCHAR = 17;

			internal const byte DEFAULTXOFFCHAR = 19;

			public uint DCBlength;

			public uint BaudRate;

			public uint Flags;

			public ushort wReserved;

			public ushort XonLim;

			public ushort XoffLim;

			public byte ByteSize;

			public byte Parity;

			public byte StopBits;

			public byte XonChar;

			public byte XoffChar;

			public byte ErrorChar;

			public byte EofChar;

			public byte EvtChar;

			public ushort wReserved1;
		}

		internal struct COMMPROP
		{
			public ushort wPacketLength;

			public ushort wPacketVersion;

			public int dwServiceMask;

			public int dwReserved1;

			public int dwMaxTxQueue;

			public int dwMaxRxQueue;

			public int dwMaxBaud;

			public int dwProvSubType;

			public int dwProvCapabilities;

			public int dwSettableParams;

			public int dwSettableBaud;

			public ushort wSettableData;

			public ushort wSettableStopParity;

			public int dwCurrentTxQueue;

			public int dwCurrentRxQueue;

			public int dwProvSpec1;

			public int dwProvSpec2;

			public char wcProvChar;
		}

		internal struct COMMTIMEOUTS
		{
			public int ReadIntervalTimeout;

			public int ReadTotalTimeoutMultiplier;

			public int ReadTotalTimeoutConstant;

			public int WriteTotalTimeoutMultiplier;

			public int WriteTotalTimeoutConstant;
		}

		internal struct COMSTAT
		{
			public uint Flags;

			public uint cbInQue;

			public uint cbOutQue;
		}

		internal static class CommFunctions
		{
			internal const int SETRTS = 3;

			internal const int CLRRTS = 4;

			internal const int SETDTR = 5;

			internal const int CLRDTR = 6;
		}

		internal static class CommModemState
		{
			internal const int MS_CTS_ON = 16;

			internal const int MS_DSR_ON = 32;

			internal const int MS_RLSD_ON = 128;
		}

		internal static class CommEvents
		{
			internal const int EV_RXCHAR = 1;

			internal const int EV_ERR = 128;

			internal const int ALL_EVENTS = 507;
		}

		internal static class PurgeFlags
		{
			internal const uint PURGE_TXABORT = 1u;

			internal const uint PURGE_RXABORT = 2u;

			internal const uint PURGE_TXCLEAR = 4u;

			internal const uint PURGE_RXCLEAR = 8u;
		}

		internal class GenericOperations
		{
			internal const int GENERIC_READ = int.MinValue;

			internal const int GENERIC_WRITE = 1073741824;
		}

		internal struct SECURITY_ATTRIBUTES
		{
			internal uint nLength;

			internal IntPtr lpSecurityDescriptor;

			internal BOOL bInheritHandle;
		}

		internal class FileTypes
		{
			internal const int FILE_TYPE_UNKNOWN = 0;

			internal const int FILE_TYPE_DISK = 1;

			internal const int FILE_TYPE_CHAR = 2;

			internal const int FILE_TYPE_PIPE = 3;
		}

		internal class IOReparseOptions
		{
			internal const uint IO_REPARSE_TAG_FILE_PLACEHOLDER = 2147483669u;

			internal const uint IO_REPARSE_TAG_MOUNT_POINT = 2684354563u;
		}

		internal class FileOperations
		{
			internal const int OPEN_EXISTING = 3;

			internal const int COPY_FILE_FAIL_IF_EXISTS = 1;

			internal const int FILE_FLAG_BACKUP_SEMANTICS = 33554432;

			internal const int FILE_FLAG_FIRST_PIPE_INSTANCE = 524288;

			internal const int FILE_FLAG_OVERLAPPED = 1073741824;

			internal const int FILE_LIST_DIRECTORY = 1;
		}

		internal const int MAXDWORD = -1;

		private const int FORMAT_MESSAGE_IGNORE_INSERTS = 512;

		private const int FORMAT_MESSAGE_FROM_HMODULE = 2048;

		private const int FORMAT_MESSAGE_FROM_SYSTEM = 4096;

		private const int FORMAT_MESSAGE_ARGUMENT_ARRAY = 8192;

		private const int FORMAT_MESSAGE_ALLOCATE_BUFFER = 256;

		private const int ERROR_INSUFFICIENT_BUFFER = 122;

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		internal static extern bool SetCommState(SafeFileHandle hFile, ref DCB lpDCB);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		internal static extern bool SetCommBreak(SafeFileHandle hFile);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		internal static extern bool ClearCommBreak(SafeFileHandle hFile);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		internal static extern bool EscapeCommFunction(SafeFileHandle hFile, int dwFunc);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		internal static extern bool SetCommTimeouts(SafeFileHandle hFile, ref COMMTIMEOUTS lpCommTimeouts);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		internal static extern bool GetCommModemStatus(SafeFileHandle hFile, ref int lpModemStat);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		internal static extern bool ClearCommError(SafeFileHandle hFile, ref int lpErrors, ref COMSTAT lpStat);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		internal static extern bool ClearCommError(SafeFileHandle hFile, ref int lpErrors, IntPtr lpStat);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		internal static extern bool GetCommProperties(SafeFileHandle hFile, ref COMMPROP lpCommProp);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		internal static extern bool SetCommMask(SafeFileHandle hFile, int dwEvtMask);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		internal static extern bool PurgeComm(SafeFileHandle hFile, uint dwFlags);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		internal static extern bool SetupComm(SafeFileHandle hFile, int dwInQueue, int dwOutQueue);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		internal static extern bool GetCommState(SafeFileHandle hFile, ref DCB lpDCB);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		internal static extern unsafe bool WaitCommEvent(SafeFileHandle hFile, int* lpEvtMask, NativeOverlapped* lpOverlapped);

		[DllImport("kernel32.dll", SetLastError = true)]
		internal static extern unsafe int ReadFile(SafeHandle handle, byte* bytes, int numBytesToRead, out int numBytesRead, IntPtr mustBeZero);

		[DllImport("kernel32.dll", SetLastError = true)]
		internal static extern unsafe int ReadFile(SafeHandle handle, byte* bytes, int numBytesToRead, IntPtr numBytesRead_mustBeZero, NativeOverlapped* overlapped);

		[DllImport("kernel32.dll", SetLastError = true)]
		internal static extern unsafe int WriteFile(SafeHandle handle, byte* bytes, int numBytesToWrite, out int numBytesWritten, IntPtr mustBeZero);

		[DllImport("kernel32.dll", SetLastError = true)]
		internal static extern unsafe int WriteFile(SafeHandle handle, byte* bytes, int numBytesToWrite, IntPtr numBytesWritten_mustBeZero, NativeOverlapped* lpOverlapped);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		internal static extern unsafe bool GetOverlappedResult(SafeFileHandle hFile, NativeOverlapped* lpOverlapped, ref int lpNumberOfBytesTransferred, bool bWait);

		[DllImport("kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool FlushFileBuffers(SafeHandle hHandle);

		[DllImport("kernel32.dll", SetLastError = true)]
		internal static extern int GetFileType(SafeHandle hFile);

		[DllImport("kernel32.dll", SetLastError = true)]
		internal static extern bool CloseHandle(IntPtr handle);

		[DllImport("kernel32.dll", BestFitMapping = true, CharSet = CharSet.Unicode, EntryPoint = "FormatMessageW", SetLastError = true)]
		private static extern unsafe int FormatMessage(int dwFlags, IntPtr lpSource, uint dwMessageId, int dwLanguageId, void* lpBuffer, int nSize, IntPtr arguments);

		internal static string GetMessage(int errorCode)
		{
			return GetMessage(errorCode, IntPtr.Zero);
		}

		internal static unsafe string GetMessage(int errorCode, IntPtr moduleHandle)
		{
			int num = 12800;
			if (moduleHandle != IntPtr.Zero)
			{
				num |= 0x800;
			}
			Span<char> span = stackalloc char[256];
			fixed (char* lpBuffer = span)
			{
				int num2 = FormatMessage(num, moduleHandle, (uint)errorCode, 0, lpBuffer, span.Length, IntPtr.Zero);
				if (num2 > 0)
				{
					return GetAndTrimString(span.Slice(0, num2));
				}
			}
			if (Marshal.GetLastWin32Error() == 122)
			{
				IntPtr intPtr = default(IntPtr);
				try
				{
					int num3 = FormatMessage(num | 0x100, moduleHandle, (uint)errorCode, 0, &intPtr, 0, IntPtr.Zero);
					if (num3 > 0)
					{
						return GetAndTrimString(new Span<char>((void*)intPtr, num3));
					}
				}
				finally
				{
					Marshal.FreeHGlobal(intPtr);
				}
			}
			return $"Unknown error (0x{errorCode:x})";
		}

		private static string GetAndTrimString(Span<char> buffer)
		{
			int num = buffer.Length;
			while (num > 0 && buffer[num - 1] <= ' ')
			{
				num--;
			}
			return buffer.Slice(0, num).ToString();
		}

		[DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Unicode, EntryPoint = "CreateFileW", ExactSpelling = true, SetLastError = true)]
		private static extern unsafe IntPtr CreateFilePrivate(string lpFileName, int dwDesiredAccess, FileShare dwShareMode, SECURITY_ATTRIBUTES* securityAttrs, FileMode dwCreationDisposition, int dwFlagsAndAttributes, IntPtr hTemplateFile);

		internal static unsafe SafeFileHandle CreateFile(string lpFileName, int dwDesiredAccess, FileShare dwShareMode, ref SECURITY_ATTRIBUTES securityAttrs, FileMode dwCreationDisposition, int dwFlagsAndAttributes, IntPtr hTemplateFile)
		{
			lpFileName = System.IO.PathInternal.EnsureExtendedPrefixIfNeeded(lpFileName);
			fixed (SECURITY_ATTRIBUTES* securityAttrs2 = &securityAttrs)
			{
				IntPtr intPtr = CreateFilePrivate(lpFileName, dwDesiredAccess, dwShareMode, securityAttrs2, dwCreationDisposition, dwFlagsAndAttributes, hTemplateFile);
				try
				{
					return new SafeFileHandle(intPtr, ownsHandle: true);
				}
				catch
				{
					CloseHandle(intPtr);
					throw;
				}
			}
		}

		internal static SafeFileHandle CreateFile(string lpFileName, int dwDesiredAccess, FileShare dwShareMode, FileMode dwCreationDisposition, int dwFlagsAndAttributes)
		{
			IntPtr intPtr = CreateFile_IntPtr(lpFileName, dwDesiredAccess, dwShareMode, dwCreationDisposition, dwFlagsAndAttributes);
			try
			{
				return new SafeFileHandle(intPtr, ownsHandle: true);
			}
			catch
			{
				CloseHandle(intPtr);
				throw;
			}
		}

		internal static unsafe IntPtr CreateFile_IntPtr(string lpFileName, int dwDesiredAccess, FileShare dwShareMode, FileMode dwCreationDisposition, int dwFlagsAndAttributes)
		{
			lpFileName = System.IO.PathInternal.EnsureExtendedPrefixIfNeeded(lpFileName);
			return CreateFilePrivate(lpFileName, dwDesiredAccess, dwShareMode, null, dwCreationDisposition, dwFlagsAndAttributes, IntPtr.Zero);
		}
	}

	internal static class Libraries
	{
		internal const string Advapi32 = "advapi32.dll";

		internal const string BCrypt = "BCrypt.dll";

		internal const string CoreComm_L1_1_1 = "api-ms-win-core-comm-l1-1-1.dll";

		internal const string CoreComm_L1_1_2 = "api-ms-win-core-comm-l1-1-2.dll";

		internal const string Crypt32 = "crypt32.dll";

		internal const string CryptUI = "cryptui.dll";

		internal const string Error_L1 = "api-ms-win-core-winrt-error-l1-1-0.dll";

		internal const string HttpApi = "httpapi.dll";

		internal const string IpHlpApi = "iphlpapi.dll";

		internal const string Kernel32 = "kernel32.dll";

		internal const string Memory_L1_3 = "api-ms-win-core-memory-l1-1-3.dll";

		internal const string Mswsock = "mswsock.dll";

		internal const string NCrypt = "ncrypt.dll";

		internal const string NtDll = "ntdll.dll";

		internal const string Odbc32 = "odbc32.dll";

		internal const string Ole32 = "ole32.dll";

		internal const string OleAut32 = "oleaut32.dll";

		internal const string PerfCounter = "perfcounter.dll";

		internal const string RoBuffer = "api-ms-win-core-winrt-robuffer-l1-1-0.dll";

		internal const string Secur32 = "secur32.dll";

		internal const string Shell32 = "shell32.dll";

		internal const string SspiCli = "sspicli.dll";

		internal const string User32 = "user32.dll";

		internal const string Version = "version.dll";

		internal const string WebSocket = "websocket.dll";

		internal const string WinHttp = "winhttp.dll";

		internal const string WinMM = "winmm.dll";

		internal const string Ws2_32 = "ws2_32.dll";

		internal const string Wtsapi32 = "wtsapi32.dll";

		internal const string CompressionNative = "clrcompression.dll";

		internal const string CoreWinRT = "api-ms-win-core-winrt-l1-1-0.dll";
	}

	internal class Errors
	{
		internal const int ERROR_SUCCESS = 0;

		internal const int ERROR_INVALID_FUNCTION = 1;

		internal const int ERROR_FILE_NOT_FOUND = 2;

		internal const int ERROR_PATH_NOT_FOUND = 3;

		internal const int ERROR_ACCESS_DENIED = 5;

		internal const int ERROR_INVALID_HANDLE = 6;

		internal const int ERROR_NOT_ENOUGH_MEMORY = 8;

		internal const int ERROR_INVALID_DATA = 13;

		internal const int ERROR_INVALID_DRIVE = 15;

		internal const int ERROR_NO_MORE_FILES = 18;

		internal const int ERROR_NOT_READY = 21;

		internal const int ERROR_BAD_COMMAND = 22;

		internal const int ERROR_BAD_LENGTH = 24;

		internal const int ERROR_SHARING_VIOLATION = 32;

		internal const int ERROR_LOCK_VIOLATION = 33;

		internal const int ERROR_HANDLE_EOF = 38;

		internal const int ERROR_BAD_NETPATH = 53;

		internal const int ERROR_NETWORK_ACCESS_DENIED = 65;

		internal const int ERROR_BAD_NET_NAME = 67;

		internal const int ERROR_FILE_EXISTS = 80;

		internal const int ERROR_INVALID_PARAMETER = 87;

		internal const int ERROR_BROKEN_PIPE = 109;

		internal const int ERROR_SEM_TIMEOUT = 121;

		internal const int ERROR_CALL_NOT_IMPLEMENTED = 120;

		internal const int ERROR_INSUFFICIENT_BUFFER = 122;

		internal const int ERROR_INVALID_NAME = 123;

		internal const int ERROR_NEGATIVE_SEEK = 131;

		internal const int ERROR_DIR_NOT_EMPTY = 145;

		internal const int ERROR_BAD_PATHNAME = 161;

		internal const int ERROR_LOCK_FAILED = 167;

		internal const int ERROR_BUSY = 170;

		internal const int ERROR_ALREADY_EXISTS = 183;

		internal const int ERROR_BAD_EXE_FORMAT = 193;

		internal const int ERROR_ENVVAR_NOT_FOUND = 203;

		internal const int ERROR_FILENAME_EXCED_RANGE = 206;

		internal const int ERROR_EXE_MACHINE_TYPE_MISMATCH = 216;

		internal const int ERROR_PIPE_BUSY = 231;

		internal const int ERROR_NO_DATA = 232;

		internal const int ERROR_PIPE_NOT_CONNECTED = 233;

		internal const int ERROR_MORE_DATA = 234;

		internal const int ERROR_NO_MORE_ITEMS = 259;

		internal const int ERROR_DIRECTORY = 267;

		internal const int ERROR_PARTIAL_COPY = 299;

		internal const int ERROR_ARITHMETIC_OVERFLOW = 534;

		internal const int ERROR_PIPE_CONNECTED = 535;

		internal const int ERROR_PIPE_LISTENING = 536;

		internal const int ERROR_OPERATION_ABORTED = 995;

		internal const int ERROR_IO_INCOMPLETE = 996;

		internal const int ERROR_IO_PENDING = 997;

		internal const int ERROR_NO_TOKEN = 1008;

		internal const int ERROR_SERVICE_DOES_NOT_EXIST = 1060;

		internal const int ERROR_DLL_INIT_FAILED = 1114;

		internal const int ERROR_COUNTER_TIMEOUT = 1121;

		internal const int ERROR_NO_ASSOCIATION = 1155;

		internal const int ERROR_DDE_FAIL = 1156;

		internal const int ERROR_DLL_NOT_FOUND = 1157;

		internal const int ERROR_NOT_FOUND = 1168;

		internal const int ERROR_NETWORK_UNREACHABLE = 1231;

		internal const int ERROR_NON_ACCOUNT_SID = 1257;

		internal const int ERROR_NOT_ALL_ASSIGNED = 1300;

		internal const int ERROR_UNKNOWN_REVISION = 1305;

		internal const int ERROR_INVALID_OWNER = 1307;

		internal const int ERROR_INVALID_PRIMARY_GROUP = 1308;

		internal const int ERROR_NO_SUCH_PRIVILEGE = 1313;

		internal const int ERROR_PRIVILEGE_NOT_HELD = 1314;

		internal const int ERROR_INVALID_ACL = 1336;

		internal const int ERROR_INVALID_SECURITY_DESCR = 1338;

		internal const int ERROR_INVALID_SID = 1337;

		internal const int ERROR_BAD_IMPERSONATION_LEVEL = 1346;

		internal const int ERROR_CANT_OPEN_ANONYMOUS = 1347;

		internal const int ERROR_NO_SECURITY_ON_OBJECT = 1350;

		internal const int ERROR_CANNOT_IMPERSONATE = 1368;

		internal const int ERROR_CLASS_ALREADY_EXISTS = 1410;

		internal const int ERROR_EVENTLOG_FILE_CHANGED = 1503;

		internal const int ERROR_TRUSTED_RELATIONSHIP_FAILURE = 1789;

		internal const int ERROR_RESOURCE_LANG_NOT_FOUND = 1815;

		internal const int EFail = -2147467259;

		internal const int E_FILENOTFOUND = -2147024894;
	}

	internal enum BOOL
	{
		FALSE,
		TRUE
	}
}
