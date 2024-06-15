using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;

namespace System.IO.Ports.Net40;

internal sealed class SerialStream : Stream
{
	internal sealed class EventLoopRunner
	{
		private WeakReference streamWeakReference;

		internal ManualResetEvent waitCommEventWaitHandle = new ManualResetEvent(initialState: false);

		private SafeFileHandle handle;

		private ThreadPoolBoundHandle threadPoolBinding;

		private bool isAsync;

		internal bool endEventLoop;

		private int eventsOccurred;

		private WaitCallback callErrorEvents;

		private WaitCallback callReceiveEvents;

		private WaitCallback callPinEvents;

		private IOCompletionCallback freeNativeOverlappedCallback;

		internal bool ShutdownLoop => endEventLoop;

		internal unsafe EventLoopRunner(SerialStream stream)
		{
			handle = stream._handle;
			threadPoolBinding = stream._threadPoolBinding;
			streamWeakReference = new WeakReference(stream);
			callErrorEvents = CallErrorEvents;
			callReceiveEvents = CallReceiveEvents;
			callPinEvents = CallPinEvents;
			freeNativeOverlappedCallback = FreeNativeOverlappedCallback;
			isAsync = stream._isAsync;
		}

		internal unsafe void WaitForCommEvent()
		{
			int lpNumberOfBytesTransferred = 0;
			bool flag = false;
			NativeOverlapped* ptr = null;
			while (!ShutdownLoop)
			{
				SerialStreamAsyncResult serialStreamAsyncResult = null;
				if (isAsync)
				{
					serialStreamAsyncResult = new SerialStreamAsyncResult();
					serialStreamAsyncResult._userCallback = null;
					serialStreamAsyncResult._userStateObject = null;
					serialStreamAsyncResult._isWrite = false;
					serialStreamAsyncResult._numBytes = 2;
					serialStreamAsyncResult._waitHandle = waitCommEventWaitHandle;
					waitCommEventWaitHandle.Reset();
					ptr = threadPoolBinding.AllocateNativeOverlapped(freeNativeOverlappedCallback, serialStreamAsyncResult, null);
					ptr->EventHandle = waitCommEventWaitHandle.SafeWaitHandle.DangerousGetHandle();
				}
				fixed (int* lpEvtMask = &eventsOccurred)
				{
					if (!global::Interop.Kernel32.WaitCommEvent(handle, lpEvtMask, ptr))
					{
						switch (Marshal.GetLastWin32Error())
						{
						case 5:
						case 22:
						case 1617:
							flag = true;
							goto end_IL_015f;
						case 997:
						{
							bool flag2 = waitCommEventWaitHandle.WaitOne();
							int lastWin32Error;
							do
							{
								flag2 = global::Interop.Kernel32.GetOverlappedResult(handle, ptr, ref lpNumberOfBytesTransferred, bWait: false);
								lastWin32Error = Marshal.GetLastWin32Error();
							}
							while (lastWin32Error == 996 && !ShutdownLoop && !flag2);
							if (!flag2 && (lastWin32Error == 996 || lastWin32Error == 87) && ShutdownLoop)
							{
							}
							break;
						}
						default:
							_ = 87;
							break;
						}
					}
				}
				if (!ShutdownLoop)
				{
					CallEvents(eventsOccurred);
				}
				if (isAsync && Interlocked.Decrement(ref serialStreamAsyncResult._numBytes) == 0)
				{
					threadPoolBinding.FreeNativeOverlapped(ptr);
				}
				continue;
				end_IL_015f:
				break;
			}
			if (flag)
			{
				endEventLoop = true;
				threadPoolBinding.FreeNativeOverlapped(ptr);
			}
		}

		private unsafe void FreeNativeOverlappedCallback(uint errorCode, uint numBytes, NativeOverlapped* pOverlapped)
		{
			SerialStreamAsyncResult serialStreamAsyncResult = (SerialStreamAsyncResult)ThreadPoolBoundHandle.GetNativeOverlappedState(pOverlapped);
			if (Interlocked.Decrement(ref serialStreamAsyncResult._numBytes) == 0)
			{
				threadPoolBinding.FreeNativeOverlapped(pOverlapped);
			}
		}

		private void CallEvents(int nativeEvents)
		{
			if (((uint)nativeEvents & 0x81u) != 0)
			{
				int lpErrors = 0;
				if (!global::Interop.Kernel32.ClearCommError(handle, ref lpErrors, IntPtr.Zero))
				{
					endEventLoop = true;
					Thread.MemoryBarrier();
					return;
				}
				lpErrors &= 0x10F;
				if (lpErrors != 0)
				{
					ThreadPool.QueueUserWorkItem(callErrorEvents, lpErrors);
				}
			}
			if (((uint)nativeEvents & 0x178u) != 0)
			{
				ThreadPool.QueueUserWorkItem(callPinEvents, nativeEvents);
			}
			if (((uint)nativeEvents & 3u) != 0)
			{
				ThreadPool.QueueUserWorkItem(callReceiveEvents, nativeEvents);
			}
		}

		private void CallErrorEvents(object state)
		{
			int num = (int)state;
			SerialStream serialStream = (SerialStream)streamWeakReference.Target;
			if (serialStream == null)
			{
				return;
			}
			if (serialStream.ErrorReceived != null)
			{
				if (((uint)num & 0x100u) != 0)
				{
					serialStream.ErrorReceived(serialStream, new SerialErrorReceivedEventArgs(SerialError.TXFull));
				}
				if (((uint)num & (true ? 1u : 0u)) != 0)
				{
					serialStream.ErrorReceived(serialStream, new SerialErrorReceivedEventArgs(SerialError.RXOver));
				}
				if (((uint)num & 2u) != 0)
				{
					serialStream.ErrorReceived(serialStream, new SerialErrorReceivedEventArgs(SerialError.Overrun));
				}
				if (((uint)num & 4u) != 0)
				{
					serialStream.ErrorReceived(serialStream, new SerialErrorReceivedEventArgs(SerialError.RXParity));
				}
				if (((uint)num & 8u) != 0)
				{
					serialStream.ErrorReceived(serialStream, new SerialErrorReceivedEventArgs(SerialError.Frame));
				}
			}
			serialStream = null;
		}

		private void CallReceiveEvents(object state)
		{
			int num = (int)state;
			SerialStream serialStream = (SerialStream)streamWeakReference.Target;
			if (serialStream == null)
			{
				return;
			}
			if (serialStream.DataReceived != null)
			{
				if (((uint)num & (true ? 1u : 0u)) != 0)
				{
					serialStream.DataReceived(serialStream, new SerialDataReceivedEventArgs(SerialData.Chars));
				}
				if (((uint)num & 2u) != 0)
				{
					serialStream.DataReceived(serialStream, new SerialDataReceivedEventArgs(SerialData.Eof));
				}
			}
			serialStream = null;
		}

		private void CallPinEvents(object state)
		{
			int num = (int)state;
			SerialStream serialStream = (SerialStream)streamWeakReference.Target;
			if (serialStream == null)
			{
				return;
			}
			if (serialStream.PinChanged != null)
			{
				if (((uint)num & 8u) != 0)
				{
					serialStream.PinChanged(serialStream, new SerialPinChangedEventArgs(SerialPinChange.CtsChanged));
				}
				if (((uint)num & 0x10u) != 0)
				{
					serialStream.PinChanged(serialStream, new SerialPinChangedEventArgs(SerialPinChange.DsrChanged));
				}
				if (((uint)num & 0x20u) != 0)
				{
					serialStream.PinChanged(serialStream, new SerialPinChangedEventArgs(SerialPinChange.CDChanged));
				}
				if (((uint)num & 0x100u) != 0)
				{
					serialStream.PinChanged(serialStream, new SerialPinChangedEventArgs(SerialPinChange.Ring));
				}
				if (((uint)num & 0x40u) != 0)
				{
					serialStream.PinChanged(serialStream, new SerialPinChangedEventArgs(SerialPinChange.Break));
				}
			}
			serialStream = null;
		}
	}

	internal sealed class SerialStreamAsyncResult : IAsyncResult
	{
		internal AsyncCallback _userCallback;

		internal object _userStateObject;

		internal bool _isWrite;

		internal bool _isComplete;

		internal bool _completedSynchronously;

		internal ManualResetEvent _waitHandle;

		internal int _EndXxxCalled;

		internal int _numBytes;

		internal int _errorCode;

		internal unsafe NativeOverlapped* _overlapped;

		public object AsyncState => _userStateObject;

		public bool IsCompleted => _isComplete;

		public WaitHandle AsyncWaitHandle => _waitHandle;

		public bool CompletedSynchronously => _completedSynchronously;
	}

	private const int MaxDataBits = 8;

	private const int MinDataBits = 5;

	private string _portName;

	private bool _inBreak;

	private Handshake _handshake;

	private const int ErrorEvents = 271;

	private const int ReceivedEvents = 3;

	private const int PinChangedEvents = 376;

	private const int infiniteTimeoutConst = -2;

	private SafeFileHandle _handle;

	private byte _parityReplace = 63;

	private bool _isAsync = true;

	private bool _rtsEnable;

	private global::Interop.Kernel32.DCB _dcb;

	private global::Interop.Kernel32.COMMTIMEOUTS _commTimeouts;

	private global::Interop.Kernel32.COMSTAT _comStat;

	private global::Interop.Kernel32.COMMPROP _commProp;

	private ThreadPoolBoundHandle _threadPoolBinding;

	private EventLoopRunner _eventRunner;

	private Task _waitForComEventTask;

	private byte[] _tempBuf;

	private unsafe static readonly IOCompletionCallback s_IOCallback = AsyncFSCallback;

	public override bool CanRead => _handle != null;

	public override bool CanSeek => false;

	public override bool CanTimeout => _handle != null;

	public override bool CanWrite => _handle != null;

	public override long Length
	{
		get
		{
			throw new NotSupportedException(System.SR.NotSupported_UnseekableStream);
		}
	}

	public override long Position
	{
		get
		{
			throw new NotSupportedException(System.SR.NotSupported_UnseekableStream);
		}
		set
		{
			throw new NotSupportedException(System.SR.NotSupported_UnseekableStream);
		}
	}

	internal int BaudRate
	{
		set
		{
			if (value <= 0 || (value > _commProp.dwMaxBaud && _commProp.dwMaxBaud > 0))
			{
				if (_commProp.dwMaxBaud == 0)
				{
					throw new ArgumentOutOfRangeException("BaudRate", System.SR.ArgumentOutOfRange_NeedPosNum);
				}
				throw new ArgumentOutOfRangeException("BaudRate", System.SR.Format(System.SR.ArgumentOutOfRange_Bounds_Lower_Upper, 0, _commProp.dwMaxBaud));
			}
			if (value != _dcb.BaudRate)
			{
				int baudRate = (int)_dcb.BaudRate;
				_dcb.BaudRate = (uint)value;
				if (!global::Interop.Kernel32.SetCommState(_handle, ref _dcb))
				{
					_dcb.BaudRate = (uint)baudRate;
					throw System.IO.Win32Marshal.GetExceptionForLastWin32Error();
				}
			}
		}
	}

	public bool BreakState
	{
		get
		{
			return _inBreak;
		}
		set
		{
			if (value)
			{
				if (!global::Interop.Kernel32.SetCommBreak(_handle))
				{
					throw System.IO.Win32Marshal.GetExceptionForLastWin32Error();
				}
				_inBreak = true;
			}
			else
			{
				if (!global::Interop.Kernel32.ClearCommBreak(_handle))
				{
					throw System.IO.Win32Marshal.GetExceptionForLastWin32Error();
				}
				_inBreak = false;
			}
		}
	}

	internal int DataBits
	{
		set
		{
			if (value != _dcb.ByteSize)
			{
				byte byteSize = _dcb.ByteSize;
				_dcb.ByteSize = (byte)value;
				if (!global::Interop.Kernel32.SetCommState(_handle, ref _dcb))
				{
					_dcb.ByteSize = byteSize;
					throw System.IO.Win32Marshal.GetExceptionForLastWin32Error();
				}
			}
		}
	}

	internal bool DiscardNull
	{
		set
		{
			int dcbFlag = GetDcbFlag(11);
			if ((value && dcbFlag == 0) || (!value && dcbFlag == 1))
			{
				int setting = dcbFlag;
				SetDcbFlag(11, value ? 1 : 0);
				if (!global::Interop.Kernel32.SetCommState(_handle, ref _dcb))
				{
					SetDcbFlag(11, setting);
					throw System.IO.Win32Marshal.GetExceptionForLastWin32Error();
				}
			}
		}
	}

	internal bool DtrEnable
	{
		get
		{
			int dcbFlag = GetDcbFlag(4);
			return dcbFlag == 1;
		}
		set
		{
			int dcbFlag = GetDcbFlag(4);
			SetDcbFlag(4, value ? 1 : 0);
			if (!global::Interop.Kernel32.SetCommState(_handle, ref _dcb))
			{
				SetDcbFlag(4, dcbFlag);
				throw System.IO.Win32Marshal.GetExceptionForLastWin32Error();
			}
			if (!global::Interop.Kernel32.EscapeCommFunction(_handle, value ? 5 : 6))
			{
				throw System.IO.Win32Marshal.GetExceptionForLastWin32Error();
			}
		}
	}

	internal Handshake Handshake
	{
		set
		{
			if (value != _handshake)
			{
				Handshake handshake = _handshake;
				int dcbFlag = GetDcbFlag(9);
				int dcbFlag2 = GetDcbFlag(2);
				int dcbFlag3 = GetDcbFlag(12);
				_handshake = value;
				int setting = ((_handshake == Handshake.XOnXOff || _handshake == Handshake.RequestToSendXOnXOff) ? 1 : 0);
				SetDcbFlag(9, setting);
				SetDcbFlag(8, setting);
				SetDcbFlag(2, (_handshake == Handshake.RequestToSend || _handshake == Handshake.RequestToSendXOnXOff) ? 1 : 0);
				if (_handshake == Handshake.RequestToSend || _handshake == Handshake.RequestToSendXOnXOff)
				{
					SetDcbFlag(12, 2);
				}
				else if (_rtsEnable)
				{
					SetDcbFlag(12, 1);
				}
				else
				{
					SetDcbFlag(12, 0);
				}
				if (!global::Interop.Kernel32.SetCommState(_handle, ref _dcb))
				{
					_handshake = handshake;
					SetDcbFlag(9, dcbFlag);
					SetDcbFlag(8, dcbFlag);
					SetDcbFlag(2, dcbFlag2);
					SetDcbFlag(12, dcbFlag3);
					throw System.IO.Win32Marshal.GetExceptionForLastWin32Error();
				}
			}
		}
	}

	internal bool IsOpen
	{
		get
		{
			if (_handle != null)
			{
				return !_eventRunner.ShutdownLoop;
			}
			return false;
		}
	}

	internal Parity Parity
	{
		set
		{
			if ((byte)value != _dcb.Parity)
			{
				byte parity = _dcb.Parity;
				int dcbFlag = GetDcbFlag(1);
				byte errorChar = _dcb.ErrorChar;
				int dcbFlag2 = GetDcbFlag(10);
				_dcb.Parity = (byte)value;
				int num = ((_dcb.Parity != 0) ? 1 : 0);
				SetDcbFlag(1, num);
				if (num == 1)
				{
					SetDcbFlag(10, (_parityReplace != 0) ? 1 : 0);
					_dcb.ErrorChar = _parityReplace;
				}
				else
				{
					SetDcbFlag(10, 0);
					_dcb.ErrorChar = 0;
				}
				if (!global::Interop.Kernel32.SetCommState(_handle, ref _dcb))
				{
					_dcb.Parity = parity;
					SetDcbFlag(1, dcbFlag);
					_dcb.ErrorChar = errorChar;
					SetDcbFlag(10, dcbFlag2);
					throw System.IO.Win32Marshal.GetExceptionForLastWin32Error();
				}
			}
		}
	}

	internal byte ParityReplace
	{
		set
		{
			if (value != _parityReplace)
			{
				byte parityReplace = _parityReplace;
				byte errorChar = _dcb.ErrorChar;
				int dcbFlag = GetDcbFlag(10);
				_parityReplace = value;
				if (GetDcbFlag(1) == 1)
				{
					SetDcbFlag(10, (_parityReplace != 0) ? 1 : 0);
					_dcb.ErrorChar = _parityReplace;
				}
				else
				{
					SetDcbFlag(10, 0);
					_dcb.ErrorChar = 0;
				}
				if (!global::Interop.Kernel32.SetCommState(_handle, ref _dcb))
				{
					_parityReplace = parityReplace;
					SetDcbFlag(10, dcbFlag);
					_dcb.ErrorChar = errorChar;
					throw System.IO.Win32Marshal.GetExceptionForLastWin32Error();
				}
			}
		}
	}

	public override int ReadTimeout
	{
		get
		{
			int readTotalTimeoutConstant = _commTimeouts.ReadTotalTimeoutConstant;
			if (readTotalTimeoutConstant == -2)
			{
				return -1;
			}
			return readTotalTimeoutConstant;
		}
		set
		{
			if (value < 0 && value != -1)
			{
				throw new ArgumentOutOfRangeException("ReadTimeout", System.SR.ArgumentOutOfRange_Timeout);
			}
			if (_handle == null)
			{
				InternalResources.FileNotOpen();
			}
			int readTotalTimeoutConstant = _commTimeouts.ReadTotalTimeoutConstant;
			int readIntervalTimeout = _commTimeouts.ReadIntervalTimeout;
			int readTotalTimeoutMultiplier = _commTimeouts.ReadTotalTimeoutMultiplier;
			switch (value)
			{
			case 0:
				_commTimeouts.ReadTotalTimeoutConstant = 0;
				_commTimeouts.ReadTotalTimeoutMultiplier = 0;
				_commTimeouts.ReadIntervalTimeout = -1;
				break;
			case -1:
				_commTimeouts.ReadTotalTimeoutConstant = -2;
				_commTimeouts.ReadTotalTimeoutMultiplier = -1;
				_commTimeouts.ReadIntervalTimeout = -1;
				break;
			default:
				_commTimeouts.ReadTotalTimeoutConstant = value;
				_commTimeouts.ReadTotalTimeoutMultiplier = -1;
				_commTimeouts.ReadIntervalTimeout = -1;
				break;
			}
			if (!global::Interop.Kernel32.SetCommTimeouts(_handle, ref _commTimeouts))
			{
				_commTimeouts.ReadTotalTimeoutConstant = readTotalTimeoutConstant;
				_commTimeouts.ReadTotalTimeoutMultiplier = readTotalTimeoutMultiplier;
				_commTimeouts.ReadIntervalTimeout = readIntervalTimeout;
				throw System.IO.Win32Marshal.GetExceptionForLastWin32Error();
			}
		}
	}

	internal bool RtsEnable
	{
		get
		{
			int dcbFlag = GetDcbFlag(12);
			if (dcbFlag == 2)
			{
				throw new InvalidOperationException(System.SR.CantSetRtsWithHandshaking);
			}
			return dcbFlag == 1;
		}
		set
		{
			if (_handshake == Handshake.RequestToSend || _handshake == Handshake.RequestToSendXOnXOff)
			{
				throw new InvalidOperationException(System.SR.CantSetRtsWithHandshaking);
			}
			if (value != _rtsEnable)
			{
				int dcbFlag = GetDcbFlag(12);
				_rtsEnable = value;
				if (value)
				{
					SetDcbFlag(12, 1);
				}
				else
				{
					SetDcbFlag(12, 0);
				}
				if (!global::Interop.Kernel32.SetCommState(_handle, ref _dcb))
				{
					SetDcbFlag(12, dcbFlag);
					_rtsEnable = !_rtsEnable;
					throw System.IO.Win32Marshal.GetExceptionForLastWin32Error();
				}
				if (!global::Interop.Kernel32.EscapeCommFunction(_handle, value ? 3 : 4))
				{
					throw System.IO.Win32Marshal.GetExceptionForLastWin32Error();
				}
			}
		}
	}

	internal StopBits StopBits
	{
		set
		{
			byte b = 0;
			b = value switch
			{
				StopBits.One => 0, 
				StopBits.OnePointFive => 1, 
				_ => 2, 
			};
			if (b != _dcb.StopBits)
			{
				byte stopBits = _dcb.StopBits;
				_dcb.StopBits = b;
				if (!global::Interop.Kernel32.SetCommState(_handle, ref _dcb))
				{
					_dcb.StopBits = stopBits;
					throw System.IO.Win32Marshal.GetExceptionForLastWin32Error();
				}
			}
		}
	}

	public override int WriteTimeout
	{
		get
		{
			int writeTotalTimeoutConstant = _commTimeouts.WriteTotalTimeoutConstant;
			if (writeTotalTimeoutConstant != 0)
			{
				return writeTotalTimeoutConstant;
			}
			return -1;
		}
		set
		{
			if (value <= 0 && value != -1)
			{
				throw new ArgumentOutOfRangeException("WriteTimeout", System.SR.ArgumentOutOfRange_WriteTimeout);
			}
			if (_handle == null)
			{
				InternalResources.FileNotOpen();
			}
			int writeTotalTimeoutConstant = _commTimeouts.WriteTotalTimeoutConstant;
			_commTimeouts.WriteTotalTimeoutConstant = ((value != -1) ? value : 0);
			if (!global::Interop.Kernel32.SetCommTimeouts(_handle, ref _commTimeouts))
			{
				_commTimeouts.WriteTotalTimeoutConstant = writeTotalTimeoutConstant;
				throw System.IO.Win32Marshal.GetExceptionForLastWin32Error();
			}
		}
	}

	internal bool CDHolding
	{
		get
		{
			int lpModemStat = 0;
			if (!global::Interop.Kernel32.GetCommModemStatus(_handle, ref lpModemStat))
			{
				throw System.IO.Win32Marshal.GetExceptionForLastWin32Error();
			}
			return (0x80 & lpModemStat) != 0;
		}
	}

	internal bool CtsHolding
	{
		get
		{
			int lpModemStat = 0;
			if (!global::Interop.Kernel32.GetCommModemStatus(_handle, ref lpModemStat))
			{
				throw System.IO.Win32Marshal.GetExceptionForLastWin32Error();
			}
			return (0x10 & lpModemStat) != 0;
		}
	}

	internal bool DsrHolding
	{
		get
		{
			int lpModemStat = 0;
			if (!global::Interop.Kernel32.GetCommModemStatus(_handle, ref lpModemStat))
			{
				throw System.IO.Win32Marshal.GetExceptionForLastWin32Error();
			}
			return (0x20 & lpModemStat) != 0;
		}
	}

	internal int BytesToRead
	{
		get
		{
			int lpErrors = 0;
			if (!global::Interop.Kernel32.ClearCommError(_handle, ref lpErrors, ref _comStat))
			{
				throw System.IO.Win32Marshal.GetExceptionForLastWin32Error();
			}
			return (int)_comStat.cbInQue;
		}
	}

	internal int BytesToWrite
	{
		get
		{
			int lpErrors = 0;
			if (!global::Interop.Kernel32.ClearCommError(_handle, ref lpErrors, ref _comStat))
			{
				throw System.IO.Win32Marshal.GetExceptionForLastWin32Error();
			}
			return (int)_comStat.cbOutQue;
		}
	}

	internal event SerialPinChangedEventHandler PinChanged;

	internal event SerialErrorReceivedEventHandler ErrorReceived;

	internal event SerialDataReceivedEventHandler DataReceived;

	public override long Seek(long offset, SeekOrigin origin)
	{
		throw new NotSupportedException(System.SR.NotSupported_UnseekableStream);
	}

	public override void SetLength(long value)
	{
		throw new NotSupportedException(System.SR.NotSupported_UnseekableStream);
	}

	public override int ReadByte()
	{
		return ReadByte(ReadTimeout);
	}

	public override void Write(byte[] array, int offset, int count)
	{
		Write(array, offset, count, WriteTimeout);
	}

	~SerialStream()
	{
		Dispose(disposing: false);
	}

	private void CheckReadWriteArguments(byte[] array, int offset, int count)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		if (offset < 0)
		{
			throw new ArgumentOutOfRangeException("offset", System.SR.ArgumentOutOfRange_NeedNonNegNumRequired);
		}
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count", System.SR.ArgumentOutOfRange_NeedNonNegNumRequired);
		}
		if (array.Length - offset < count)
		{
			throw new ArgumentException(System.SR.Argument_InvalidOffLen);
		}
		if (_handle == null)
		{
			InternalResources.FileNotOpen();
		}
	}

	private void CheckWriteArguments(byte[] array, int offset, int count)
	{
		if (_inBreak)
		{
			throw new InvalidOperationException(System.SR.In_Break_State);
		}
		CheckReadWriteArguments(array, offset, count);
	}

	internal SerialStream(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits, int readTimeout, int writeTimeout, Handshake handshake, bool dtrEnable, bool rtsEnable, bool discardNull, byte parityReplace)
	{
		if (portName == null)
		{
			throw new ArgumentNullException("portName");
		}
		if (!portName.StartsWith("COM", StringComparison.OrdinalIgnoreCase) || !uint.TryParse(portName.Substring(3), out var result))
		{
			throw new ArgumentException(System.SR.Format(System.SR.Arg_InvalidSerialPort, portName), "portName");
		}
		SafeFileHandle safeFileHandle = OpenPort(result);
		if (safeFileHandle.IsInvalid)
		{
			throw System.IO.Win32Marshal.GetExceptionForLastWin32Error(portName);
		}
		try
		{
			int fileType = global::Interop.Kernel32.GetFileType(safeFileHandle);
			if (fileType != 2 && fileType != 0)
			{
				throw new ArgumentException(System.SR.Format(System.SR.Arg_InvalidSerialPort, portName), "portName");
			}
			_handle = safeFileHandle;
			_portName = portName;
			_handshake = handshake;
			_parityReplace = parityReplace;
			_tempBuf = new byte[1];
			_commProp = default(global::Interop.Kernel32.COMMPROP);
			int lpModemStat = 0;
			if (!global::Interop.Kernel32.GetCommProperties(_handle, ref _commProp) || !global::Interop.Kernel32.GetCommModemStatus(_handle, ref lpModemStat))
			{
				int lastWin32Error = Marshal.GetLastWin32Error();
				if (lastWin32Error == 87 || lastWin32Error == 6)
				{
					throw new ArgumentException(System.SR.Arg_InvalidSerialPortExtended, "portName");
				}
				throw System.IO.Win32Marshal.GetExceptionForWin32Error(lastWin32Error, string.Empty);
			}
			if (_commProp.dwMaxBaud != 0 && baudRate > _commProp.dwMaxBaud)
			{
				throw new ArgumentOutOfRangeException("baudRate", System.SR.Format(System.SR.Max_Baud, _commProp.dwMaxBaud));
			}
			_comStat = default(global::Interop.Kernel32.COMSTAT);
			_dcb = default(global::Interop.Kernel32.DCB);
			InitializeDCB(baudRate, parity, dataBits, stopBits, discardNull);
			DtrEnable = dtrEnable;
			_rtsEnable = GetDcbFlag(12) == 1;
			if (handshake != Handshake.RequestToSend && handshake != Handshake.RequestToSendXOnXOff)
			{
				RtsEnable = rtsEnable;
			}
			switch (readTimeout)
			{
			case 0:
				_commTimeouts.ReadTotalTimeoutConstant = 0;
				_commTimeouts.ReadTotalTimeoutMultiplier = 0;
				_commTimeouts.ReadIntervalTimeout = -1;
				break;
			case -1:
				_commTimeouts.ReadTotalTimeoutConstant = -2;
				_commTimeouts.ReadTotalTimeoutMultiplier = -1;
				_commTimeouts.ReadIntervalTimeout = -1;
				break;
			default:
				_commTimeouts.ReadTotalTimeoutConstant = readTimeout;
				_commTimeouts.ReadTotalTimeoutMultiplier = -1;
				_commTimeouts.ReadIntervalTimeout = -1;
				break;
			}
			_commTimeouts.WriteTotalTimeoutMultiplier = 0;
			_commTimeouts.WriteTotalTimeoutConstant = ((writeTimeout != -1) ? writeTimeout : 0);
			if (!global::Interop.Kernel32.SetCommTimeouts(_handle, ref _commTimeouts))
			{
				throw System.IO.Win32Marshal.GetExceptionForLastWin32Error();
			}
			if (_isAsync)
			{
				_threadPoolBinding = ThreadPoolBoundHandle.BindHandle(_handle);
			}
			global::Interop.Kernel32.SetCommMask(_handle, 507);
			_eventRunner = new EventLoopRunner(this);
			_waitForComEventTask = Task.Factory.StartNew(delegate(object s)
			{
				((EventLoopRunner)s).WaitForCommEvent();
			}, _eventRunner,
				CancellationToken.None,
				TaskCreationOptions.LongRunning 
				//| TaskCreationOptions.DenyChildAttach
				, TaskScheduler.Default);
		}
		catch
		{
			safeFileHandle.Close();
			_handle = null;
			_threadPoolBinding?.Dispose();
			throw;
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (_handle == null || _handle.IsInvalid)
		{
			return;
		}
		try
		{
			_eventRunner.endEventLoop = true;
			Thread.MemoryBarrier();
			bool flag = false;
			global::Interop.Kernel32.SetCommMask(_handle, 0);
			if (!global::Interop.Kernel32.EscapeCommFunction(_handle, 6))
			{
				int lastWin32Error = Marshal.GetLastWin32Error();
				if ((lastWin32Error == 5 || lastWin32Error == 22 || lastWin32Error == 1617) && !disposing)
				{
					flag = true;
				}
				else if (disposing)
				{
					throw System.IO.Win32Marshal.GetExceptionForLastWin32Error();
				}
			}
			if (!flag && !_handle.IsClosed)
			{
				Flush();
			}
			_eventRunner.waitCommEventWaitHandle.Set();
			if (!flag)
			{
				DiscardInBuffer();
				DiscardOutBuffer();
			}
			if (disposing && _eventRunner != null && _waitForComEventTask != null)
			{
				_waitForComEventTask.GetAwaiter().GetResult();
				_eventRunner.waitCommEventWaitHandle.Close();
			}
		}
		finally
		{
			if (disposing)
			{
				lock (this)
				{
					_handle.Close();
					_handle = null;
					_threadPoolBinding.Dispose();
				}
			}
			else
			{
				_handle.Close();
				_handle = null;
				_threadPoolBinding.Dispose();
			}
			base.Dispose(disposing);
		}
	}

	public override IAsyncResult BeginRead(byte[] array, int offset, int numBytes, AsyncCallback userCallback, object stateObject)
	{
		CheckReadWriteArguments(array, offset, numBytes);
		int readTimeout = ReadTimeout;
		ReadTimeout = -1;
		try
		{
			if (!_isAsync)
			{
				return base.BeginRead(array, offset, numBytes, userCallback, stateObject);
			}
			return BeginReadCore(array, offset, numBytes, userCallback, stateObject);
		}
		finally
		{
			ReadTimeout = readTimeout;
		}
	}

	public override IAsyncResult BeginWrite(byte[] array, int offset, int numBytes, AsyncCallback userCallback, object stateObject)
	{
		CheckWriteArguments(array, offset, numBytes);
		int writeTimeout = WriteTimeout;
		WriteTimeout = -1;
		try
		{
			if (!_isAsync)
			{
				return base.BeginWrite(array, offset, numBytes, userCallback, stateObject);
			}
			return BeginWriteCore(array, offset, numBytes, userCallback, stateObject);
		}
		finally
		{
			WriteTimeout = writeTimeout;
		}
	}

	internal void DiscardInBuffer()
	{
		if (!global::Interop.Kernel32.PurgeComm(_handle, 10u))
		{
			throw System.IO.Win32Marshal.GetExceptionForLastWin32Error();
		}
	}

	internal void DiscardOutBuffer()
	{
		if (!global::Interop.Kernel32.PurgeComm(_handle, 5u))
		{
			throw System.IO.Win32Marshal.GetExceptionForLastWin32Error();
		}
	}

	public unsafe override int EndRead(IAsyncResult asyncResult)
	{
		if (!_isAsync)
		{
			return base.EndRead(asyncResult);
		}
		if (asyncResult == null)
		{
			throw new ArgumentNullException("asyncResult");
		}
		SerialStreamAsyncResult serialStreamAsyncResult = asyncResult as SerialStreamAsyncResult;
		if (serialStreamAsyncResult == null || serialStreamAsyncResult._isWrite)
		{
			InternalResources.WrongAsyncResult();
		}
		if (1 == Interlocked.CompareExchange(ref serialStreamAsyncResult._EndXxxCalled, 1, 0))
		{
			InternalResources.EndReadCalledTwice();
		}
		bool flag = false;
		WaitHandle waitHandle = serialStreamAsyncResult._waitHandle;
		if (waitHandle != null)
		{
			try
			{
				waitHandle.WaitOne();
				if (serialStreamAsyncResult._numBytes == 0 && ReadTimeout == -1 && serialStreamAsyncResult._errorCode == 0)
				{
					flag = true;
				}
			}
			finally
			{
				waitHandle.Close();
			}
		}
		NativeOverlapped* overlapped = serialStreamAsyncResult._overlapped;
		if (overlapped != null)
		{
			_threadPoolBinding.FreeNativeOverlapped(overlapped);
		}
		if (serialStreamAsyncResult._errorCode != 0)
		{
			throw System.IO.Win32Marshal.GetExceptionForWin32Error(serialStreamAsyncResult._errorCode, _portName);
		}
		if (flag)
		{
			throw new IOException(System.SR.IO_OperationAborted);
		}
		return serialStreamAsyncResult._numBytes;
	}

	public unsafe override void EndWrite(IAsyncResult asyncResult)
	{
		if (!_isAsync)
		{
			base.EndWrite(asyncResult);
			return;
		}
		if (_inBreak)
		{
			throw new InvalidOperationException(System.SR.In_Break_State);
		}
		if (asyncResult == null)
		{
			throw new ArgumentNullException("asyncResult");
		}
		SerialStreamAsyncResult serialStreamAsyncResult = asyncResult as SerialStreamAsyncResult;
		if (serialStreamAsyncResult == null || !serialStreamAsyncResult._isWrite)
		{
			InternalResources.WrongAsyncResult();
		}
		if (1 == Interlocked.CompareExchange(ref serialStreamAsyncResult._EndXxxCalled, 1, 0))
		{
			InternalResources.EndWriteCalledTwice();
		}
		WaitHandle waitHandle = serialStreamAsyncResult._waitHandle;
		if (waitHandle != null)
		{
			try
			{
				waitHandle.WaitOne();
			}
			finally
			{
				waitHandle.Close();
			}
		}
		NativeOverlapped* overlapped = serialStreamAsyncResult._overlapped;
		if (overlapped != null)
		{
			_threadPoolBinding.FreeNativeOverlapped(overlapped);
		}
		if (serialStreamAsyncResult._errorCode == 0)
		{
			return;
		}
		throw System.IO.Win32Marshal.GetExceptionForWin32Error(serialStreamAsyncResult._errorCode, _portName);
	}

	public override void Flush()
	{
		if (_handle == null)
		{
			throw new ObjectDisposedException(System.SR.Port_not_open);
		}
		global::Interop.Kernel32.FlushFileBuffers(_handle);
	}

	public override int Read(byte[] array, int offset, int count)
	{
		return Read(array, offset, count, ReadTimeout);
	}

	internal unsafe int Read(byte[] array, int offset, int count, int timeout)
	{
		CheckReadWriteArguments(array, offset, count);
		if (count == 0)
		{
			return 0;
		}
		int num = 0;
		if (_isAsync)
		{
			IAsyncResult asyncResult = BeginReadCore(array, offset, count, null, null);
			num = EndRead(asyncResult);
		}
		else
		{
			num = ReadFileNative(array, offset, count, null, out var _);
			if (num == -1)
			{
				throw System.IO.Win32Marshal.GetExceptionForLastWin32Error();
			}
		}
		if (num == 0)
		{
			throw new TimeoutException();
		}
		return num;
	}

	internal unsafe int ReadByte(int timeout)
	{
		if (_handle == null)
		{
			InternalResources.FileNotOpen();
		}
		int num = 0;
		if (_isAsync)
		{
			IAsyncResult asyncResult = BeginReadCore(_tempBuf, 0, 1, null, null);
			num = EndRead(asyncResult);
		}
		else
		{
			num = ReadFileNative(_tempBuf, 0, 1, null, out var _);
			if (num == -1)
			{
				throw System.IO.Win32Marshal.GetExceptionForLastWin32Error();
			}
		}
		if (num == 0)
		{
			throw new TimeoutException();
		}
		return _tempBuf[0];
	}

	internal void SetBufferSizes(int readBufferSize, int writeBufferSize)
	{
		if (_handle == null)
		{
			InternalResources.FileNotOpen();
		}
		if (!global::Interop.Kernel32.SetupComm(_handle, readBufferSize, writeBufferSize))
		{
			throw System.IO.Win32Marshal.GetExceptionForLastWin32Error();
		}
	}

	internal unsafe void Write(byte[] array, int offset, int count, int timeout)
	{
		CheckWriteArguments(array, offset, count);
		if (count == 0)
		{
			return;
		}
		int num;
		if (_isAsync)
		{
			IAsyncResult asyncResult = BeginWriteCore(array, offset, count, null, null);
			EndWrite(asyncResult);
			SerialStreamAsyncResult serialStreamAsyncResult = asyncResult as SerialStreamAsyncResult;
			num = serialStreamAsyncResult._numBytes;
		}
		else
		{
			num = WriteFileNative(array, offset, count, null, out var hr);
			if (num == -1)
			{
				if (hr == 1121)
				{
					throw new TimeoutException(System.SR.Write_timed_out);
				}
				throw System.IO.Win32Marshal.GetExceptionForLastWin32Error();
			}
		}
		if (num != 0)
		{
			return;
		}
		throw new TimeoutException(System.SR.Write_timed_out);
	}

	public override void WriteByte(byte value)
	{
		WriteByte(value, WriteTimeout);
	}

	internal unsafe void WriteByte(byte value, int timeout)
	{
		if (_inBreak)
		{
			throw new InvalidOperationException(System.SR.In_Break_State);
		}
		if (_handle == null)
		{
			InternalResources.FileNotOpen();
		}
		_tempBuf[0] = value;
		int num;
		if (_isAsync)
		{
			IAsyncResult asyncResult = BeginWriteCore(_tempBuf, 0, 1, null, null);
			EndWrite(asyncResult);
			SerialStreamAsyncResult serialStreamAsyncResult = asyncResult as SerialStreamAsyncResult;
			num = serialStreamAsyncResult._numBytes;
		}
		else
		{
			num = WriteFileNative(_tempBuf, 0, 1, null, out var _);
			if (num == -1)
			{
				if (Marshal.GetLastWin32Error() == 1121)
				{
					throw new TimeoutException(System.SR.Write_timed_out);
				}
				throw System.IO.Win32Marshal.GetExceptionForLastWin32Error();
			}
		}
		if (num == 0)
		{
			throw new TimeoutException(System.SR.Write_timed_out);
		}
	}

	private unsafe void InitializeDCB(int baudRate, Parity parity, int dataBits, StopBits stopBits, bool discardNull)
	{
		if (!global::Interop.Kernel32.GetCommState(_handle, ref _dcb))
		{
			throw System.IO.Win32Marshal.GetExceptionForLastWin32Error();
		}
		_dcb.DCBlength = (uint)sizeof(global::Interop.Kernel32.DCB);
		_dcb.BaudRate = (uint)baudRate;
		_dcb.ByteSize = (byte)dataBits;
		switch (stopBits)
		{
		case StopBits.One:
			_dcb.StopBits = 0;
			break;
		case StopBits.OnePointFive:
			_dcb.StopBits = 1;
			break;
		case StopBits.Two:
			_dcb.StopBits = 2;
			break;
		}
		_dcb.Parity = (byte)parity;
		SetDcbFlag(1, (parity != 0) ? 1 : 0);
		SetDcbFlag(0, 1);
		SetDcbFlag(2, (_handshake == Handshake.RequestToSend || _handshake == Handshake.RequestToSendXOnXOff) ? 1 : 0);
		SetDcbFlag(3, 0);
		SetDcbFlag(4, 0);
		SetDcbFlag(6, 0);
		SetDcbFlag(9, (_handshake == Handshake.XOnXOff || _handshake == Handshake.RequestToSendXOnXOff) ? 1 : 0);
		SetDcbFlag(8, (_handshake == Handshake.XOnXOff || _handshake == Handshake.RequestToSendXOnXOff) ? 1 : 0);
		if (parity != 0)
		{
			SetDcbFlag(10, (_parityReplace != 0) ? 1 : 0);
			_dcb.ErrorChar = _parityReplace;
		}
		else
		{
			SetDcbFlag(10, 0);
			_dcb.ErrorChar = 0;
		}
		SetDcbFlag(11, discardNull ? 1 : 0);
		SetDcbFlag(14, 0);
		if (_handshake == Handshake.RequestToSend || _handshake == Handshake.RequestToSendXOnXOff)
		{
			SetDcbFlag(12, 2);
		}
		else if (GetDcbFlag(12) == 2)
		{
			SetDcbFlag(12, 0);
		}
		_dcb.XonChar = 17;
		_dcb.XoffChar = 19;
		_dcb.XonLim = (_dcb.XoffLim = (ushort)(_commProp.dwCurrentRxQueue / 4));
		_dcb.EofChar = 26;
		_dcb.EvtChar = 26;
		if (!global::Interop.Kernel32.SetCommState(_handle, ref _dcb))
		{
			throw System.IO.Win32Marshal.GetExceptionForLastWin32Error();
		}
	}

	internal int GetDcbFlag(int whichFlag)
	{
		uint num;
		switch (whichFlag)
		{
		case 4:
		case 12:
			num = 3u;
			break;
		case 15:
			num = 131071u;
			break;
		default:
			num = 1u;
			break;
		}
		uint num2 = _dcb.Flags & (num << whichFlag);
		return (int)(num2 >> whichFlag);
	}

	internal void SetDcbFlag(int whichFlag, int setting)
	{
		setting <<= whichFlag;
		uint num;
		switch (whichFlag)
		{
		case 4:
		case 12:
			num = 3u;
			break;
		case 15:
			num = 131071u;
			break;
		default:
			num = 1u;
			break;
		}
		_dcb.Flags &= ~(num << whichFlag);
		_dcb.Flags |= (uint)setting;
	}

	private unsafe SerialStreamAsyncResult BeginReadCore(byte[] array, int offset, int numBytes, AsyncCallback userCallback, object stateObject)
	{
		SerialStreamAsyncResult serialStreamAsyncResult = new SerialStreamAsyncResult();
		serialStreamAsyncResult._userCallback = userCallback;
		serialStreamAsyncResult._userStateObject = stateObject;
		serialStreamAsyncResult._isWrite = false;
		ManualResetEvent waitHandle = new ManualResetEvent(initialState: false);
		serialStreamAsyncResult._waitHandle = waitHandle;
		NativeOverlapped* overlapped = (serialStreamAsyncResult._overlapped = _threadPoolBinding.AllocateNativeOverlapped(s_IOCallback, serialStreamAsyncResult, array));
		int hr = 0;
		int num = ReadFileNative(array, offset, numBytes, overlapped, out hr);
		if (num == -1)
		{
			switch (hr)
			{
			case 38:
				InternalResources.EndOfFile();
				break;
			default:
				throw System.IO.Win32Marshal.GetExceptionForWin32Error(hr, string.Empty);
			case 997:
				break;
			}
		}
		return serialStreamAsyncResult;
	}

	private unsafe SerialStreamAsyncResult BeginWriteCore(byte[] array, int offset, int numBytes, AsyncCallback userCallback, object stateObject)
	{
		SerialStreamAsyncResult serialStreamAsyncResult = new SerialStreamAsyncResult();
		serialStreamAsyncResult._userCallback = userCallback;
		serialStreamAsyncResult._userStateObject = stateObject;
		serialStreamAsyncResult._isWrite = true;
		ManualResetEvent waitHandle = new ManualResetEvent(initialState: false);
		serialStreamAsyncResult._waitHandle = waitHandle;
		NativeOverlapped* overlapped = (serialStreamAsyncResult._overlapped = _threadPoolBinding.AllocateNativeOverlapped(s_IOCallback, serialStreamAsyncResult, array));
		int hr = 0;
		int num = WriteFileNative(array, offset, numBytes, overlapped, out hr);
		if (num == -1)
		{
			switch (hr)
			{
			case 38:
				InternalResources.EndOfFile();
				break;
			default:
				throw System.IO.Win32Marshal.GetExceptionForWin32Error(hr, string.Empty);
			case 997:
				break;
			}
		}
		return serialStreamAsyncResult;
	}

	private unsafe int ReadFileNative(byte[] bytes, int offset, int count, NativeOverlapped* overlapped, out int hr)
	{
		if (bytes.Length - offset < count)
		{
			throw new IndexOutOfRangeException(System.SR.IndexOutOfRange_IORaceCondition);
		}
		if (bytes.Length == 0)
		{
			hr = 0;
			return 0;
		}
		int num = 0;
		int numBytesRead = 0;
		fixed (byte* ptr = bytes)
		{
			num = ((!_isAsync) ? global::Interop.Kernel32.ReadFile(_handle, ptr + offset, count, out numBytesRead, IntPtr.Zero) : global::Interop.Kernel32.ReadFile(_handle, ptr + offset, count, IntPtr.Zero, overlapped));
		}
		if (num == 0)
		{
			hr = Marshal.GetLastWin32Error();
			if (hr == 6)
			{
				_handle.SetHandleAsInvalid();
			}
			return -1;
		}
		hr = 0;
		return numBytesRead;
	}

	private unsafe int WriteFileNative(byte[] bytes, int offset, int count, NativeOverlapped* overlapped, out int hr)
	{
		if (bytes.Length - offset < count)
		{
			throw new IndexOutOfRangeException(System.SR.IndexOutOfRange_IORaceCondition);
		}
		if (bytes.Length == 0)
		{
			hr = 0;
			return 0;
		}
		int numBytesWritten = 0;
		int num = 0;
		fixed (byte* ptr = bytes)
		{
			num = ((!_isAsync) ? global::Interop.Kernel32.WriteFile(_handle, ptr + offset, count, out numBytesWritten, IntPtr.Zero) : global::Interop.Kernel32.WriteFile(_handle, ptr + offset, count, IntPtr.Zero, overlapped));
		}
		if (num == 0)
		{
			hr = Marshal.GetLastWin32Error();
			if (hr == 6)
			{
				_handle.SetHandleAsInvalid();
			}
			return -1;
		}
		hr = 0;
		return numBytesWritten;
	}

	private unsafe static void AsyncFSCallback(uint errorCode, uint numBytes, NativeOverlapped* pOverlapped)
	{
		SerialStreamAsyncResult serialStreamAsyncResult = (SerialStreamAsyncResult)ThreadPoolBoundHandle.GetNativeOverlappedState(pOverlapped);
		serialStreamAsyncResult._numBytes = (int)numBytes;
		serialStreamAsyncResult._errorCode = (int)errorCode;
		serialStreamAsyncResult._completedSynchronously = false;
		serialStreamAsyncResult._isComplete = true;
		ManualResetEvent waitHandle = serialStreamAsyncResult._waitHandle;
		if (waitHandle != null && !waitHandle.Set())
		{
			throw System.IO.Win32Marshal.GetExceptionForLastWin32Error();
		}
		serialStreamAsyncResult._userCallback?.Invoke(serialStreamAsyncResult);
	}

	public SafeFileHandle OpenPort(uint portNumber)
	{
		return global::Interop.Kernel32.CreateFile("\\\\?\\COM" + portNumber.ToString(CultureInfo.InvariantCulture), -1073741824, FileShare.None, FileMode.Open, 1073741824);
	}
}
